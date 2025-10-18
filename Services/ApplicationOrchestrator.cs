using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using FileReader.Interfaces;
using FileReader.Models;

namespace FileReader.Services
{
    /// <summary>
    /// Application orchestrator that coordinates all services
    /// Follows Single Responsibility Principle - only handles application flow orchestration
    /// </summary>
    public class ApplicationOrchestrator
    {
        private readonly IDataExtractionService _dataExtractionService;
        private readonly IOpcUaService _opcUaService;
        private readonly ICsvExportService _csvExportService;
        private readonly AppConfiguration _configuration;

        public ApplicationOrchestrator(
            IDataExtractionService dataExtractionService,
            IOpcUaService opcUaService,
            ICsvExportService csvExportService,
            AppConfiguration configuration)
        {
            _dataExtractionService = dataExtractionService ?? throw new ArgumentNullException(nameof(dataExtractionService));
            _opcUaService = opcUaService ?? throw new ArgumentNullException(nameof(opcUaService));
            _csvExportService = csvExportService ?? throw new ArgumentNullException(nameof(csvExportService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Executes the main application workflow
        /// </summary>
        public async Task<bool> ExecuteMainWorkflowAsync()
        {
            try
            {
                Console.WriteLine("🚀 Starting PDF Data Extraction and OPC UA Export Workflow");
                Console.WriteLine("=========================================================");

                // Step 1: Get PDF file path
                var filePath = GetPdfFilePath();
                if (string.IsNullOrEmpty(filePath))
                {
                    Console.WriteLine("❌ No PDF file selected. Exiting.");
                    return false;
                }

                Console.WriteLine($"📄 Selected PDF: {Path.GetFileName(filePath)}");

                // Step 2: Extract data from PDF
                Console.WriteLine("\n🔍 Extracting data from PDF...");
                var extractedData = await _dataExtractionService.ExtractDataAsync(filePath);
                
                if (extractedData == null)
                {
                    Console.WriteLine("❌ Failed to extract data from PDF");
                    return false;
                }

                Console.WriteLine($"✅ Data extraction completed");
                LogExtractionSummary(extractedData);

                // Step 3: Export to CSV
                Console.WriteLine("\n📊 Exporting data to CSV...");
                var csvPath = GetCsvOutputPath();
                var csvSuccess = await _csvExportService.ExportToCsvAsync(extractedData, csvPath);
                
                if (csvSuccess)
                {
                    Console.WriteLine($"✅ CSV export completed: {csvPath}");
                }
                else
                {
                    Console.WriteLine("⚠️ CSV export failed, but continuing with OPC UA...");
                }

                // Step 4: Write to OPC UA
                Console.WriteLine("\n🔗 Connecting to OPC UA server...");
                var opcUaSuccess = await WriteToOpcUaAsync(extractedData);

                // Step 5: Summary
                Console.WriteLine("\n📋 Workflow Summary:");
                Console.WriteLine($"   📄 PDF Processing: ✅ Success");
                Console.WriteLine($"   📊 CSV Export: {(csvSuccess ? "✅ Success" : "❌ Failed")}");
                Console.WriteLine($"   🔗 OPC UA Export: {(opcUaSuccess ? "✅ Success" : "❌ Failed")}");

                var overallSuccess = csvSuccess || opcUaSuccess; // Success if at least one export worked
                Console.WriteLine($"\n🎯 Overall Result: {(overallSuccess ? "✅ SUCCESS" : "❌ FAILED")}");

                return overallSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Workflow failed with exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Executes the diagnostic workflow
        /// </summary>
        public async Task<bool> ExecuteDiagnosticWorkflowAsync()
        {
            try
            {
                Console.WriteLine("🔧 OPC UA Connection Diagnostic Tool");
                Console.WriteLine("====================================");
                Console.WriteLine();

                var success = await TestOpcUaConnectionAsync();
                
                if (!success)
                {
                    PrintCommonSolutions();
                }

                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Diagnostic failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Executes the setup workflow
        /// </summary>
        public async Task<bool> ExecuteSetupWorkflowAsync()
        {
            try
            {
                Console.WriteLine("⚙️ OPC UA First-Time Setup Tool");
                Console.WriteLine("===============================");
                Console.WriteLine();

                Console.WriteLine("🔐 Setting up certificates...");
                var certResult = await CertificateManager.EnsureCertificatesExistAsync();
                
                if (certResult)
                {
                    Console.WriteLine();
                    Console.WriteLine("🔗 Testing connection...");
                    var connectionResult = await TestOpcUaConnectionAsync();
                    
                    if (connectionResult)
                    {
                        Console.WriteLine("✅ Setup completed successfully!");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("⚠️ Setup completed but connection test failed.");
                        Console.WriteLine("This may be normal if the OPC UA server is not running.");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("❌ Certificate setup failed!");
                    CertificateManager.PrintCertificateInfo();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Setup failed: {ex.Message}");
                return false;
            }
        }

        #region Private Methods

        private string GetPdfFilePath()
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
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error selecting file: {ex.Message}");
                return string.Empty;
            }
        }

        private string GetCsvOutputPath()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var outputFolderPath = Path.Combine(currentDirectory, _configuration.ApplicationSettings.OutputFolderName);
            
            if (!Directory.Exists(outputFolderPath))
            {
                Directory.CreateDirectory(outputFolderPath);
            }

            return Path.Combine(outputFolderPath, _configuration.ApplicationSettings.CsvFileName);
        }

        private void LogExtractionSummary(ExtractedReportData data)
        {
            Console.WriteLine($"   📋 Report Type: {data.ReportInfo.ReportType}");
            Console.WriteLine($"   🔬 Instrument: {data.Instrument.Name} (S/N: {data.Instrument.SerialNumber})");
            Console.WriteLine($"   📊 Sample: {data.Sample.Record}");
            Console.WriteLine($"   🔄 Measurement Cycles: {data.MeasurementCycles.Count}");
        }

        private async Task<bool> WriteToOpcUaAsync(ExtractedReportData extractedData)
        {
            try
            {
                var connected = await _opcUaService.ConnectAsync();
                if (!connected)
                {
                    Console.WriteLine("❌ Failed to connect to OPC UA server");
                    return false;
                }

                // Browse server (optional, for diagnostics)
                _opcUaService.BrowseRootFolder();

                // Write the data
                var success = await _opcUaService.WriteReportDataAsync(extractedData);
                
                if (success)
                {
                    Console.WriteLine("✅ All data written to OPC UA server successfully!");
                }
                else
                {
                    Console.WriteLine("⚠️ Some OPC UA writes may have failed. Check logs above for details.");
                }

                await _opcUaService.DisconnectAsync();
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ OPC UA operation failed: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> TestOpcUaConnectionAsync()
        {
            try
            {
                Console.WriteLine($"🔗 Attempting to connect to: {_configuration.OpcUaSettings.EndpointUrl}");
                
                var connected = await _opcUaService.ConnectAsync();
                if (connected)
                {
                    Console.WriteLine("✅ Connection successful!");
                    _opcUaService.BrowseRootFolder();
                    await _opcUaService.DisconnectAsync();
                    return true;
                }
                else
                {
                    Console.WriteLine("❌ Connection failed!");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Connection test failed: {ex.Message}");
                return false;
            }
        }

        private void PrintCommonSolutions()
        {
            Console.WriteLine();
            Console.WriteLine("🔧 Common Solutions:");
            Console.WriteLine("===================");
            Console.WriteLine("1. Verify OPC UA server is running");
            Console.WriteLine("2. Check endpoint URL in appsettings.json");
            Console.WriteLine("3. Verify firewall settings");
            Console.WriteLine("4. Check OPC UA server security settings");
            Console.WriteLine("5. Run setup mode: FileReader.exe --setup");
        }

        #endregion
    }
}
