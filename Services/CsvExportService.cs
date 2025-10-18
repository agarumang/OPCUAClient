using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileReader.Interfaces;
using FileReader.Models;

namespace FileReader.Services
{
    /// <summary>
    /// Service responsible for CSV export operations
    /// Follows Single Responsibility Principle - only handles CSV export logic
    /// </summary>
    public class CsvExportService : ICsvExportService
    {
        /// <summary>
        /// Exports extracted report data to CSV file
        /// </summary>
        public async Task<bool> ExportToCsvAsync(ExtractedReportData reportData, string outputPath)
        {
            if (reportData == null)
            {
                Console.WriteLine("⚠️ Cannot export CSV - report data is null");
                return false;
            }

            if (string.IsNullOrEmpty(outputPath))
            {
                Console.WriteLine("⚠️ Cannot export CSV - output path is null or empty");
                return false;
            }

            try
            {
                Console.WriteLine($"📄 Exporting data to CSV: {outputPath}");
                
                // Ensure output directory exists
                var directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var csvItems = ConvertToCSvItems(reportData);
                await WriteCsvFileAsync(outputPath, csvItems, reportData.MeasurementCycles);
                
                Console.WriteLine($"✅ CSV export completed: {outputPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CSV export failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Converts extracted report data to CSV data items
        /// </summary>
        public IEnumerable<CsvDataItem> ConvertToCSvItems(ExtractedReportData reportData)
        {
            if (reportData == null)
                return Enumerable.Empty<CsvDataItem>();

            var items = new List<CsvDataItem>();

            // Add Report Info
            items.AddRange(ConvertReportInfoToCsv(reportData.ReportInfo));
            
            // Add Instrument Info
            items.AddRange(ConvertInstrumentInfoToCsv(reportData.Instrument));
            
            // Add Sample Info
            items.AddRange(ConvertSampleInfoToCsv(reportData.Sample));
            
            // Add Parameters
            items.AddRange(ConvertParametersToCsv(reportData.Parameters));
            
            // Add Results
            items.AddRange(ConvertResultsToCsv(reportData.Results));
            
            // Add Measurement Cycles
            items.AddRange(ConvertMeasurementCyclesToCsv(reportData.MeasurementCycles));

            return items;
        }

        #region Private Methods

        private async Task WriteCsvFileAsync(string outputPath, IEnumerable<CsvDataItem> csvItems, List<MeasurementCycle> measurementCycles)
        {
            using (var writer = new StreamWriter(outputPath, false, System.Text.Encoding.UTF8))
            {
                var itemsList = csvItems.ToList();
                
                // Write basic data first (non-measurement cycles)
                await writer.WriteLineAsync("Category,Field,Value");
                await writer.WriteLineAsync(); // Empty line
                
                // Write all non-measurement cycle data
                foreach (var item in itemsList.Where(i => i.Category != "Measurement Cycles"))
                {
                    var escapedValue = EscapeCsvValue(item.Value);
                    await writer.WriteLineAsync($"\"{item.Category}\",\"{item.Field}\",\"{escapedValue}\"");
                }
                
                // Write measurement cycles in table format
                if (measurementCycles != null && measurementCycles.Any())
                {
                    await writer.WriteLineAsync(); // Empty line before measurement cycles
                    await WriteMeasurementCyclesTableAsync(writer, measurementCycles);
                }
            }
        }

        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value)) 
                return string.Empty;
            
            // Replace quotes with double quotes and handle commas/newlines
            return value.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", "");
        }

        private IEnumerable<CsvDataItem> ConvertReportInfoToCsv(ReportInfo reportInfo)
        {
            yield return new CsvDataItem("Report Info", "Generated", reportInfo.Generated.ToString("yyyy-MM-dd HH:mm:ss"));
            yield return new CsvDataItem("Report Info", "Source File", reportInfo.SourceFile);
            yield return new CsvDataItem("Report Info", "Report Date", reportInfo.ReportDate);
            yield return new CsvDataItem("Report Info", "Serial Number", reportInfo.SerialNumber);
            yield return new CsvDataItem("Report Info", "Report Type", reportInfo.ReportType);
        }

        private IEnumerable<CsvDataItem> ConvertInstrumentInfoToCsv(InstrumentInfo instrument)
        {
            yield return new CsvDataItem("Instrument", "Instrument", instrument.Name);
            yield return new CsvDataItem("Instrument", "Serial number", instrument.SerialNumber);
            yield return new CsvDataItem("Instrument", "Version", instrument.Version);
        }

        private IEnumerable<CsvDataItem> ConvertSampleInfoToCsv(SampleInfo sample)
        {
            yield return new CsvDataItem("Sample", "Record", sample.Record);
            yield return new CsvDataItem("Sample", "Operator", sample.Operator);
            yield return new CsvDataItem("Sample", "Submitter", sample.Submitter);
            yield return new CsvDataItem("Sample", "Started", sample.StartedTime?.ToString("MMM d, yyyy h:mm tt") ?? "Not found");
            yield return new CsvDataItem("Sample", "Completed", sample.CompletedTime?.ToString("MMM d, yyyy h:mm tt") ?? "Not found");
            yield return new CsvDataItem("Sample", "Report time", sample.ReportTime?.ToString("MMM d, yyyy h:mm tt") ?? "Not found");
            yield return new CsvDataItem("Sample", "Sample mass", sample.SampleMassText);
            yield return new CsvDataItem("Sample", "Absolute density", sample.AbsoluteDensityText);
        }

        private IEnumerable<CsvDataItem> ConvertParametersToCsv(MeasurementParameters parameters)
        {
            yield return new CsvDataItem("Parameters", "Chamber diameter", parameters.ChamberDiameterText);
            yield return new CsvDataItem("Parameters", "Preparation cycles", parameters.PreparationCycles?.ToString() ?? "Not found");
            yield return new CsvDataItem("Parameters", "Measurement cycles", parameters.MeasurementCycles?.ToString() ?? "Not found");
            yield return new CsvDataItem("Parameters", "Blank data", parameters.BlankData);
            yield return new CsvDataItem("Parameters", "Consolidation force", parameters.ConsolidationForceText);
            yield return new CsvDataItem("Parameters", "Conversion factor", parameters.ConversionFactorText);
            yield return new CsvDataItem("Parameters", "Zero depth", parameters.ZeroDepthText);
        }

        private IEnumerable<CsvDataItem> ConvertResultsToCsv(MeasurementResults results)
        {
            yield return new CsvDataItem("Results", "Average envelope volume", results.AverageEnvelopeVolumeText);
            yield return new CsvDataItem("Results", "Average envelope density", results.AverageEnvelopeDensityText);
            yield return new CsvDataItem("Results", "Specific pore volume", results.SpecificPoreVolumeText);
            yield return new CsvDataItem("Results", "Porosity", results.PorosityText);
            yield return new CsvDataItem("Results", "Percent sample volume", results.PercentSampleVolumeText);
            yield return new CsvDataItem("Results", "Standard deviation (Volume)", results.StandardDeviationVolumeText);
            yield return new CsvDataItem("Results", "Standard deviation (Density)", results.StandardDeviationDensityText);
        }

        private IEnumerable<CsvDataItem> ConvertMeasurementCyclesToCsv(List<MeasurementCycle> cycles)
        {
            // Return a placeholder item to indicate we have measurement cycles
            // The actual table writing is handled separately in WriteMeasurementCyclesTableAsync
            if (cycles.Any())
            {
                yield return new CsvDataItem("Measurement Cycles", "Table", "See table below");
            }
        }

        private async Task WriteMeasurementCyclesTableAsync(StreamWriter writer, List<MeasurementCycle> measurementCycles)
        {
            // Write table header
            await writer.WriteLineAsync("MEASUREMENT CYCLES TABLE");
            await writer.WriteLineAsync("Cycle #,Blank (counts),Sample (counts),Volume (cm³),Volume Deviation (cm³),Density (g/cm³),Density Deviation (g/cm³)");
            
            // Write each cycle as a row with values in separate columns
            foreach (var cycle in measurementCycles)
            {
                var row = $"{cycle.CycleNumber},{cycle.BlankCounts},{cycle.SampleCounts},{cycle.Volume:F4},{cycle.VolumeDeviation:F4},{cycle.Density:F4},{cycle.DensityDeviation:F4}";
                await writer.WriteLineAsync(row);
            }
        }

        #endregion
    }
}
