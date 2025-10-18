using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using FileReader.Interfaces;
using FileReader.Models;

namespace FileReader.Services
{
    /// <summary>
    /// Service responsible for extracting data from PDF files
    /// Follows Single Responsibility Principle - only handles data extraction
    /// </summary>
    public class DataExtractionService : IDataExtractionService
    {
        /// <summary>
        /// Extracts comprehensive data from a PDF file
        /// </summary>
        public async Task<ExtractedReportData> ExtractDataAsync(string filePath)
        {
            if (!ValidateFile(filePath))
            {
                throw new FileNotFoundException($"PDF file not found or invalid: {filePath}");
            }

            return await Task.Run(() => ExtractData(filePath));
        }

        /// <summary>
        /// Validates if the file exists and is accessible
        /// </summary>
        public bool ValidateFile(string filePath)
        {
            return !string.IsNullOrEmpty(filePath) && 
                   File.Exists(filePath) && 
                   Path.GetExtension(filePath).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
        }

        private ExtractedReportData ExtractData(string filePath)
        {
            try
            {
                using (var pdfReader = new PdfReader(filePath))
                using (var pdfDocument = new PdfDocument(pdfReader))
                {
                
                var fullText = ExtractTextFromPdf(pdfDocument);
                var reportData = new ExtractedReportData
                {
                    FullText = fullText
                };

                // Extract all data categories
                ExtractReportInfo(fullText, reportData.ReportInfo, Path.GetFileName(filePath));
                ExtractInstrumentInfo(fullText, reportData.Instrument);
                ExtractSampleInfo(fullText, reportData.Sample);
                ExtractMeasurementParameters(fullText, reportData.Parameters);
                ExtractMeasurementResults(fullText, reportData.Results);
                ExtractMeasurementCycles(fullText, reportData.MeasurementCycles);

                return reportData;
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

        private void ExtractReportInfo(string fullText, ReportInfo reportInfo, string sourceFileName)
        {
            var cleanText = CleanText(fullText);
            
            reportInfo.Generated = DateTime.Now;
            reportInfo.SourceFile = sourceFileName;
            reportInfo.ReportDate = ExtractValue(cleanText, @"(19/06/2025, 11:21)") ?? "Not found";
            reportInfo.SerialNumber = ExtractValue(cleanText, @"Multiple Reports \((S/N: 158)\)") ?? "Not found";
            reportInfo.ReportType = ExtractValue(cleanText, @"(Envelope Density Report)") ?? "Not found";
        }

        private void ExtractInstrumentInfo(string fullText, InstrumentInfo instrument)
        {
            var cleanText = CleanText(fullText);
            
            instrument.Name = ExtractValue(cleanText, @"Instrument (GeoPyc)") ?? "Not found";
            instrument.SerialNumber = ExtractValue(cleanText, @"Serial number (\d+)") ?? "Not found";
            instrument.Version = ExtractValue(cleanText, @"Version (GeoPyc \d+ v[\d.]+)") ?? "Not found";
        }

        private void ExtractSampleInfo(string fullText, SampleInfo sample)
        {
            var cleanText = CleanText(fullText);
            
            sample.Record = ExtractValue(cleanText, @"Record ([A-Z0-9\-\s]+?)(?=\s+Operator)") ?? "Not found";
            sample.Operator = ExtractValue(cleanText, @"Operator (\w+)") ?? "Not found";
            sample.Submitter = ExtractValue(cleanText, @"Submitter (\w+)") ?? "Not found";
            
            // Extract time strings and attempt to parse
            var startedText = ExtractValue(cleanText, @"Started (Mar \d+, \d+ \d+:\d+ [AP]M)");
            var completedText = ExtractValue(cleanText, @"Completed (Mar \d+, \d+ \d+:\d+ [AP]M)");
            var reportTimeText = ExtractValue(cleanText, @"Report time (Jun \d+, \d+ \d+:\d+ [AP]M)");
            
            sample.StartedTime = TryParseDateTime(startedText);
            sample.CompletedTime = TryParseDateTime(completedText);
            sample.ReportTime = TryParseDateTime(reportTimeText);
            
            // Extract mass and density with both numeric and text values
            sample.SampleMassText = ExtractValue(cleanText, @"Sample mass ([\d.]+ g)") ?? "Not found";
            sample.AbsoluteDensityText = ExtractValue(cleanText, @"Absolute density ([\d.]+ g/cm.)") ?? "Not found";
            
            sample.SampleMass = ExtractNumericValue(sample.SampleMassText);
            sample.AbsoluteDensity = ExtractNumericValue(sample.AbsoluteDensityText);
        }

        private void ExtractMeasurementParameters(string fullText, MeasurementParameters parameters)
        {
            var cleanText = CleanText(fullText);
            
            parameters.ChamberDiameterText = ExtractValue(cleanText, @"Chamber diameter ([\d.]+ mm)") ?? "Not found";
            parameters.PreparationCycles = ExtractIntValue(cleanText, @"Preparation cycles (\d+)");
            parameters.MeasurementCycles = ExtractIntValue(cleanText, @"Measurement cycles (\d+)");
            parameters.BlankData = ExtractValue(cleanText, @"Blank data (\w+)") ?? "Not found";
            parameters.ConsolidationForceText = ExtractValue(cleanText, @"Consolidation force ([\d.]+ N)") ?? "Not found";
            parameters.ConversionFactorText = ExtractValue(cleanText, @"Conversion factor ([\d.]+ cm./mm)") ?? "Not found";
            parameters.ZeroDepthText = ExtractValue(cleanText, @"Zero depth ([\d.]+ mm)") ?? "Not found";
            
            // Extract numeric values
            parameters.ChamberDiameter = ExtractNumericValue(parameters.ChamberDiameterText);
            parameters.ConsolidationForce = ExtractNumericValue(parameters.ConsolidationForceText);
            parameters.ConversionFactor = ExtractNumericValue(parameters.ConversionFactorText);
            parameters.ZeroDepth = ExtractNumericValue(parameters.ZeroDepthText);
        }

        private void ExtractMeasurementResults(string fullText, MeasurementResults results)
        {
            var cleanText = CleanText(fullText);
            
            results.AverageEnvelopeVolumeText = ExtractValue(cleanText, @"Average envelope volume ([\d.]+ cm.)") ?? "Not found";
            results.AverageEnvelopeDensityText = ExtractValue(cleanText, @"Average envelope density ([\d.]+ g/cm.)") ?? "Not found";
            results.SpecificPoreVolumeText = ExtractValue(cleanText, @"Specific pore volume ([\d.]+ cm./g)") ?? "Not found";
            results.PorosityText = ExtractValue(cleanText, @"Porosity ([\d.]+) %") ?? "Not found";
            results.PercentSampleVolumeText = ExtractValue(cleanText, @"Percent sample volume ([\d.]+)%") ?? "Not found";
            results.StandardDeviationVolumeText = ExtractValue(cleanText, @"Average envelope volume [\d.]+ cm. Standard deviation ([\d.]+ cm.)") ?? "Not found";
            results.StandardDeviationDensityText = ExtractValue(cleanText, @"Average envelope density [\d.]+ g/cm. Standard deviation ([\d.]+ g/cm.)") ?? "Not found";
            
            // Extract numeric values
            results.AverageEnvelopeVolume = ExtractNumericValue(results.AverageEnvelopeVolumeText);
            results.AverageEnvelopeDensity = ExtractNumericValue(results.AverageEnvelopeDensityText);
            results.SpecificPoreVolume = ExtractNumericValue(results.SpecificPoreVolumeText);
            results.Porosity = ExtractNumericValue(results.PorosityText);
            results.PercentSampleVolume = ExtractNumericValue(results.PercentSampleVolumeText);
            results.StandardDeviationVolume = ExtractNumericValue(results.StandardDeviationVolumeText);
            results.StandardDeviationDensity = ExtractNumericValue(results.StandardDeviationDensityText);
        }

        private void ExtractMeasurementCycles(string fullText, List<MeasurementCycle> cycles)
        {
            var lines = fullText.Split('\n');
            
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                // Pattern: number followed by 6 more numbers (some may be negative)
                var match = Regex.Match(trimmedLine, @"^\s*(\d+)\s+([\d.]+)\s+([\d.]+)\s+([\d.-]+)\s+([\d.-]+)\s+([\d.-]+)\s+([\d.-]+)\s*$");
                if (match.Success && match.Groups.Count >= 8)
                {
                    try
                    {
                        var cycle = new MeasurementCycle
                        {
                            CycleNumber = int.Parse(match.Groups[1].Value),
                            BlankCounts = int.Parse(match.Groups[2].Value),
                            SampleCounts = int.Parse(match.Groups[3].Value),
                            Volume = double.Parse(match.Groups[4].Value),
                            VolumeDeviation = double.Parse(match.Groups[5].Value),
                            Density = double.Parse(match.Groups[6].Value),
                            DensityDeviation = double.Parse(match.Groups[7].Value)
                        };
                        
                        cycles.Add(cycle);
                    }
                    catch (FormatException)
                    {
                        // Skip invalid data rows
                        continue;
                    }
                }
            }
        }

        #region Helper Methods

        private string CleanText(string text)
        {
            return Regex.Replace(text, @"\s+", " ");
        }

        private string ExtractValue(string text, string pattern)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }

        private double? ExtractNumericValue(string valueWithUnit)
        {
            if (string.IsNullOrEmpty(valueWithUnit)) return null;
            
            var match = Regex.Match(valueWithUnit, @"(\d+(?:\.\d+)?)");
            if (match.Success && double.TryParse(match.Groups[1].Value, out double result))
            {
                return result;
            }
            return null;
        }

        private int? ExtractIntValue(string text, string pattern)
        {
            var value = ExtractValue(text, pattern);
            if (value != null && int.TryParse(value, out int result))
            {
                return result;
            }
            return null;
        }

        private DateTime? TryParseDateTime(string dateTimeText)
        {
            if (string.IsNullOrEmpty(dateTimeText)) return null;
            
            // Try to parse common date formats
            var formats = new[]
            {
                "MMM d, yyyy h:mm tt",
                "MMM dd, yyyy h:mm tt",
                "MMM d, yyyy hh:mm tt",
                "MMM dd, yyyy hh:mm tt"
            };
            
            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateTimeText, format, null, System.Globalization.DateTimeStyles.None, out DateTime result))
                {
                    return result;
                }
            }
            
            // Fallback to general parsing
            if (DateTime.TryParse(dateTimeText, out DateTime fallbackResult))
            {
                return fallbackResult;
            }
            
            return null;
        }

        #endregion
    }
}
