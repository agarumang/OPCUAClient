using System.Collections.Generic;
using System.Threading.Tasks;
using FileReader.Models;

namespace FileReader.Interfaces
{
    /// <summary>
    /// Service responsible for CSV export operations
    /// </summary>
    public interface ICsvExportService
    {
        /// <summary>
        /// Exports extracted report data to CSV file
        /// </summary>
        /// <param name="reportData">The extracted report data</param>
        /// <param name="outputPath">Path where CSV file should be saved</param>
        /// <returns>True if export successful</returns>
        Task<bool> ExportToCsvAsync(ExtractedReportData reportData, string outputPath);

        /// <summary>
        /// Converts extracted report data to CSV data items
        /// </summary>
        /// <param name="reportData">The extracted report data</param>
        /// <returns>Collection of CSV data items</returns>
        IEnumerable<CsvDataItem> ConvertToCSvItems(ExtractedReportData reportData);
    }
}

