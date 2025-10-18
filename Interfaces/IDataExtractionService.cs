using System.Threading.Tasks;
using FileReader.Models;

namespace FileReader.Interfaces
{
    /// <summary>
    /// Service responsible for extracting data from PDF files
    /// </summary>
    public interface IDataExtractionService
    {
        /// <summary>
        /// Extracts comprehensive data from a PDF file
        /// </summary>
        /// <param name="filePath">Path to the PDF file</param>
        /// <returns>Extracted report data</returns>
        Task<ExtractedReportData> ExtractDataAsync(string filePath);

        /// <summary>
        /// Validates if the file exists and is accessible
        /// </summary>
        /// <param name="filePath">Path to the PDF file</param>
        /// <returns>True if file is valid</returns>
        bool ValidateFile(string filePath);
    }
}

