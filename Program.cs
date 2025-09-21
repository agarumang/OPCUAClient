using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace FileReader
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                // Check for diagnostic mode
                if (args.Length > 0 && args[0] == "--diagnostic")
                {
                    RunDiagnosticMode().GetAwaiter().GetResult();
                    return;
                }

                // Check for setup mode
                if (args.Length > 0 && args[0] == "--setup")
                {
                    RunSetupMode().GetAwaiter().GetResult();
                    return;
                }

                MainAsync(args).GetAwaiter().GetResult();
            }
            catch
            {
                // Silent error handling - application exits
            }
        }

        static async Task MainAsync(string[] args)
        {
            try
            {
                // First-time setup check
                await EnsureFirstTimeSetup();

                var filePath = GetPdfFilePath();
                if (string.IsNullOrEmpty(filePath)) return;

                var extractor = new PdfDataExtractor();
                var data = extractor.ExtractDataFromPdf(filePath);
                
                // Create Excel file
                CreateExcelFile(data);
                
                // Write data to OPC UA server
                await WriteToOpcUaAsync(data);
            }
            catch
            {
                // Silent error handling - application exits
            }
        }

        static async Task EnsureFirstTimeSetup()
        {
            try
            {
                await CertificateManager.EnsureCertificatesExistAsync();
            }
            catch
            {
                // Silent setup - certificates are optional for basic operation
            }
        }

        static async Task RunDiagnosticMode()
        {
            Console.WriteLine("OPC UA Connection Diagnostic Tool");
            Console.WriteLine("=================================");
            Console.WriteLine();

            try
            {
                var success = await OPCConnectionDiagnostic.DiagnoseConnectionAsync();
                
                if (!success)
                {
                    OPCConnectionDiagnostic.PrintCommonSolutions();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Diagnostic failed: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static async Task RunSetupMode()
        {
            Console.WriteLine("OPC UA First-Time Setup Tool");
            Console.WriteLine("============================");
            Console.WriteLine();

            try
            {
                Console.WriteLine("Setting up certificates...");
                var certResult = await CertificateManager.EnsureCertificatesExistAsync();
                
                if (certResult)
                {
                    Console.WriteLine();
                    Console.WriteLine("Testing connection...");
                    var connectionResult = await OPCConnectionDiagnostic.DiagnoseConnectionAsync();
                    
                    if (connectionResult)
                    {
                        Console.WriteLine("✅ Setup completed successfully!");
                    }
                    else
                    {
                        Console.WriteLine("⚠️ Setup completed but connection test failed.");
                        Console.WriteLine("This may be normal if the OPC UA server is not running.");
                    }
                }
                else
                {
                    Console.WriteLine("❌ Certificate setup failed!");
                    CertificateManager.PrintCertificateInfo();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Setup failed: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void CreateExcelFile(PdfDataExtractor.ExtractedData data)
        {
            try
            {
                var config = ConfigurationManager.Configuration.ApplicationSettings;
                var currentDirectory = Directory.GetCurrentDirectory();
                var excelFolderPath = Path.Combine(currentDirectory, config.ExcelFolderName);
                
                if (!Directory.Exists(excelFolderPath))
                {
                    Directory.CreateDirectory(excelFolderPath);
                }

                var excelPath = Path.Combine(excelFolderPath, config.ExcelFileName);
                var measurementTable = CreateMeasurementTable(data);
                var tablesToExport = new List<PdfDataExtractor.TableData> { measurementTable };
                
                if (data.Tables.Any())
                {
                    tablesToExport.AddRange(data.Tables);
                }

                var extractor = new PdfDataExtractor();
                extractor.ExportTablesToExcel(tablesToExport, excelPath, data);
            }
            catch
            {
                // Silent error handling
            }
        }

        static PdfDataExtractor.TableData CreateMeasurementTable(PdfDataExtractor.ExtractedData data)
        {
            var extractedRows = ExtractMeasurementCyclesFromText(data.FullText);
            
            return new PdfDataExtractor.TableData
            {
                TableName = "Measurement Cycles",
                Headers = new List<string> { "Cycle #", "Blank (counts)", "Sample (counts)", "Volume (cm³)", "Deviation (cm³)", "Density (g/cm³)", "Deviation (g/cm³)" },
                Rows = extractedRows
            };
        }

        static List<List<string>> ExtractMeasurementCyclesFromText(string text)
        {
            var rows = new List<List<string>>();
            
            try
            {
                var lines = text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                bool inTable = false;
                bool foundHeader = false;
                
                for (int i = 0; i < lines.Length; i++)
                {
                    var trimmedLine = lines[i].Trim();
                    
                    if (trimmedLine.Contains("Cycle") && trimmedLine.Contains("#"))
                    {
                        for (int j = i + 1; j < Math.Min(i + 10, lines.Length); j++)
                        {
                            var nextLine = lines[j].Trim();
                            if (nextLine.Contains("Deviation") && nextLine.Contains("g/cm³"))
                            {
                                foundHeader = true;
                                inTable = true;
                                i = j;
                                break;
                            }
                        }
                        
                        if (foundHeader) continue;
                    }
                    
                    if (inTable)
                    {
                        var dataRowPattern = @"^(\d+)\s+(\d+)\s+(\d+)\s+([\d.-]+)\s+([\d.-]+)\s+([\d.-]+)\s+([\d.-]+)";
                        var match = System.Text.RegularExpressions.Regex.Match(trimmedLine, dataRowPattern);
                        
                        if (match.Success)
                        {
                            var row = new List<string>();
                            for (int g = 1; g <= 7; g++)
                            {
                                if (g < match.Groups.Count)
                                {
                                    row.Add(match.Groups[g].Value.Trim());
                                }
                            }
                            
                            if (row.Count == 7)
                            {
                                rows.Add(row);
                            }
                        }
                        else if (rows.Count > 0 && (string.IsNullOrWhiteSpace(trimmedLine) || 
                                 trimmedLine.Contains("Cycle #") || 
                                 trimmedLine.Contains("Density (g/cm³)")))
                        {
                            break;
                        }
                    }
                }
                
                if (rows.Count == 0)
                {
                    foreach (var line in lines)
                    {
                        var trimmedLine = line.Trim();
                        var simplePattern = @"^(\d+)\s+(\d+)\s+(\d+)\s+([\d.-]+)\s+([\d.-]+)\s+([\d.-]+)\s+([\d.-]+)$";
                        var match = System.Text.RegularExpressions.Regex.Match(trimmedLine, simplePattern);
                        
                        if (match.Success)
                        {
                            var row = new List<string>();
                            for (int g = 1; g <= 7; g++)
                            {
                                row.Add(match.Groups[g].Value.Trim());
                            }
                            rows.Add(row);
                        }
                    }
                }
            }
            catch
            {
                // Silent error handling
            }
            
            return rows;
        }

        static string GetPdfFilePath()
        {
            try
            {
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Title = "Select PDF File";
                    openFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
                    openFileDialog.CheckFileExists = true;
                    openFileDialog.CheckPathExists = true;
                    openFileDialog.Multiselect = false;

                    return openFileDialog.ShowDialog() == DialogResult.OK ? openFileDialog.FileName : string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        static async Task WriteToOpcUaAsync(PdfDataExtractor.ExtractedData data)
        {
            try
            {
                var opcClient = new OPCUAClient();
                
                if (await opcClient.ConnectAsync())
                {
                    opcClient.BrowseRootFolder();
                    var success = opcClient.WritePdfDataToOpcUa(data);
                    
                    if (success)
                        Console.WriteLine("✅ PDF data written to OPC UA server!");
                    else
                        Console.WriteLine("⚠️ Some OPC UA writes may have failed.");
                    
                    await opcClient.DisconnectAsync();
                }
            }
            catch
            {
                // Silent error handling - OPC UA operations are optional
            }
        }
    }
}