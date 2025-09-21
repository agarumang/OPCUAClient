using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using OfficeOpenXml;

namespace FileReader
{
    public class PdfDataExtractor
    {
        public class ExtractedData
        {
            public string FullText { get; set; } = string.Empty;
            public List<TableData> Tables { get; set; } = new List<TableData>();
        }

        public class TableData
        {
            public string TableName { get; set; } = string.Empty;
            public List<string> Headers { get; set; } = new List<string>();
            public List<List<string>> Rows { get; set; } = new List<List<string>>();
        }

        public class SummaryData
        {
            public string StartedTime { get; set; } = "Not found";
            public string CompletedTime { get; set; } = "Not found";
            public string SampleMass { get; set; } = "Not found";
            public string AbsoluteDensity { get; set; } = "Not found";
        }

        public ExtractedData ExtractDataFromPdf(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"PDF file not found: {filePath}");
            }

            var extractedData = new ExtractedData();

            try
            {
                using (var pdfReader = new PdfReader(filePath))
                using (var pdfDocument = new PdfDocument(pdfReader))
                {
                    var text = ExtractTextFromPdf(pdfDocument);
                    extractedData.FullText = text;
                    extractedData.Tables = ExtractTables(text);
                    return extractedData;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error processing PDF file: {ex.Message}", ex);
            }
        }

        private string ExtractTextFromPdf(PdfDocument pdfDocument)
        {
            var text = new System.Text.StringBuilder();
            
            for (int pageNum = 1; pageNum <= pdfDocument.GetNumberOfPages(); pageNum++)
            {
                var page = pdfDocument.GetPage(pageNum);
                var strategy = new SimpleTextExtractionStrategy();
                var pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
                text.AppendLine(pageText);
            }

            return text.ToString();
        }

        private List<TableData> ExtractTables(string text)
        {
            var tables = new List<TableData>();
            
            // Look for measurement cycle data - the headers might be split across lines
            if (text.Contains("Cycle") && text.Contains("Blank") && text.Contains("Sample") && text.Contains("Volume") && text.Contains("Density"))
            {
                var table = new TableData
                {
                    TableName = "Measurement Cycles",
                    Headers = new List<string> { "Cycle #", "Blank (counts)", "Sample (counts)", "Volume (cm³)", "Deviation (cm³)", "Density (g/cm³)", "Deviation (g/cm³)" }
                };

                var lines = text.Split('\n');
                bool foundHeaders = false;
                
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    
                    // Look for the start of measurement data - lines that start with a number followed by measurement values
                    // Pattern: number followed by 6 more numbers (some may be negative)
                    var match = Regex.Match(line, @"^\s*(\d+)\s+([\d.]+)\s+([\d.]+)\s+([\d.-]+)\s+([\d.-]+)\s+([\d.-]+)\s+([\d.-]+)\s*$");
                    if (match.Success)
                    {
                        foundHeaders = true; // We found data, so we know we're in the right section
                        var row = new List<string>();
                        for (int j = 1; j <= 7; j++)
                        {
                            row.Add(match.Groups[j].Value.Trim());
                        }
                        table.Rows.Add(row);
                    }
                    else if (foundHeaders && table.Rows.Count > 0)
                    {
                        // Check if we've reached the end of the table (empty line or non-matching content)
                        if (string.IsNullOrWhiteSpace(line) || line.Contains("Cycle #") || line.Contains("Density (g/cm3)"))
                        {
                            // We might be at the end, but let's continue a bit more to catch all rows
                            continue;
                        }
                        // If we encounter something that clearly isn't a data row and we have data, we're done
                        if (!Regex.IsMatch(line, @"^\s*\d+\s+[\d.-]+") && table.Rows.Count >= 5)
                        {
                            break;
                        }
                    }
                }
                
                if (table.Rows.Count > 0)
                {
                    tables.Add(table);
                }
            }
            
            return tables;
        }

        public void ExportTablesToExcel(List<TableData> tables, string outputPath, ExtractedData data = null)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                
                var directory = Path.GetDirectoryName(outputPath);
                
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }
                
                using (var package = new ExcelPackage())
                {
                    foreach (var table in tables)
                    {
                        var worksheetName = CleanWorksheetName(table.TableName);
                        var worksheet = package.Workbook.Worksheets.Add(worksheetName);
                        
                        int currentRow = 1;
                        
                        // Summary header
                        worksheet.Cells[currentRow, 1].Value = "REPORT SUMMARY";
                        worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                        worksheet.Cells[currentRow, 1].Style.Font.Size = 14;
                        currentRow += 2;
                        
                        var summaryData = data != null ? ExtractSummaryData(data) : new SummaryData();
                        
                        worksheet.Cells[currentRow, 1].Value = "Started:";
                        worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                        worksheet.Cells[currentRow, 2].Value = summaryData.StartedTime;
                        currentRow++;
                        
                        worksheet.Cells[currentRow, 1].Value = "Completed:";
                        worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                        worksheet.Cells[currentRow, 2].Value = summaryData.CompletedTime;
                        currentRow++;
                        
                        worksheet.Cells[currentRow, 1].Value = "Sample mass:";
                        worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                        worksheet.Cells[currentRow, 2].Value = summaryData.SampleMass;
                        currentRow++;
                        
                        worksheet.Cells[currentRow, 1].Value = "Absolute density:";
                        worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                        worksheet.Cells[currentRow, 2].Value = summaryData.AbsoluteDensity;
                        currentRow += 3;
                        
                        worksheet.Cells[currentRow, 1].Value = "MEASUREMENT CYCLES DATA";
                        worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                        worksheet.Cells[currentRow, 1].Style.Font.Size = 12;
                        currentRow += 2;
                        
                        for (int col = 0; col < table.Headers.Count; col++)
                        {
                            worksheet.Cells[currentRow, col + 1].Value = table.Headers[col];
                            worksheet.Cells[currentRow, col + 1].Style.Font.Bold = true;
                            worksheet.Cells[currentRow, col + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            worksheet.Cells[currentRow, col + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                        }
                        currentRow++;
                        
                        for (int row = 0; row < table.Rows.Count; row++)
                        {
                            for (int col = 0; col < table.Rows[row].Count && col < table.Headers.Count; col++)
                            {
                                var cellValue = table.Rows[row][col];
                                
                                if (double.TryParse(cellValue, out double numValue))
                                {
                                    worksheet.Cells[currentRow, col + 1].Value = numValue;
                                    
                                    if (col == 0)
                                    {
                                        worksheet.Cells[currentRow, col + 1].Style.Numberformat.Format = "0";
                                    }
                                    else if (col == 1 || col == 2)
                                    {
                                        worksheet.Cells[currentRow, col + 1].Style.Numberformat.Format = "#,##0";
                                    }
                                    else
                                    {
                                        worksheet.Cells[currentRow, col + 1].Style.Numberformat.Format = "0.0000";
                                    }
                                }
                                else
                                {
                                    worksheet.Cells[currentRow, col + 1].Value = cellValue;
                                }
                            }
                            currentRow++;
                        }
                        
                        var tableStartRow = currentRow - table.Rows.Count - 1;
                        var tableEndRow = currentRow - 1;
                        var tableRange = worksheet.Cells[tableStartRow, 1, tableEndRow, table.Headers.Count];
                        tableRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        tableRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        
                        worksheet.Cells.AutoFitColumns();
                    }
                    
                    var fileInfo = new FileInfo(outputPath);
                    package.SaveAs(fileInfo);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create Excel file: {ex.Message}", ex);
            }
        }

        private SummaryData ExtractSummaryData(ExtractedData data)
        {
            var summary = new SummaryData();
            
            var startedMatch = System.Text.RegularExpressions.Regex.Match(data.FullText, 
                @"Started[:\s]*([A-Za-z]{3}\s+\d{1,2},?\s+\d{4}\s+\d{1,2}:\d{2}\s*(?:AM|PM)?)", 
                RegexOptions.IgnoreCase);
            if (startedMatch.Success)
            {
                summary.StartedTime = startedMatch.Groups[1].Value.Trim();
            }
            
            var completedMatch = System.Text.RegularExpressions.Regex.Match(data.FullText, 
                @"Completed[:\s]*([A-Za-z]{3}\s+\d{1,2},?\s+\d{4}\s+\d{1,2}:\d{2}\s*(?:AM|PM)?)", 
                RegexOptions.IgnoreCase);
            if (completedMatch.Success)
            {
                summary.CompletedTime = completedMatch.Groups[1].Value.Trim();
            }
            
            var massMatch = System.Text.RegularExpressions.Regex.Match(data.FullText, 
                @"Sample\s+mass[:\s]*(\d+(?:\.\d+)?)\s*g", 
                RegexOptions.IgnoreCase);
            if (massMatch.Success)
            {
                summary.SampleMass = massMatch.Groups[1].Value.Trim() + " g";
            }
            
            var densityMatch = System.Text.RegularExpressions.Regex.Match(data.FullText, 
                @"Absolute\s+density[:\s]*(\d+(?:\.\d+)?)\s*g/cm³", 
                RegexOptions.IgnoreCase);
            if (densityMatch.Success)
            {
                summary.AbsoluteDensity = densityMatch.Groups[1].Value.Trim() + " g/cm³";
            }
            
            return summary;
        }

        private string CleanWorksheetName(string name)
        {
            var cleaned = name.Replace("\\", "").Replace("/", "").Replace("?", "")
                             .Replace("*", "").Replace("[", "").Replace("]", "");
            
            if (cleaned.Length > 31)
            {
                cleaned = cleaned.Substring(0, 31);
            }
            
            return string.IsNullOrEmpty(cleaned) ? "Sheet1" : cleaned;
        }
    }
}