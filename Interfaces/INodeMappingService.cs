using System.Collections.Generic;
using FileReader.Models;

namespace FileReader.Interfaces
{
    /// <summary>
    /// Service responsible for mapping extracted data to OPC UA node IDs
    /// </summary>
    public interface INodeMappingService
    {
        /// <summary>
        /// Maps extracted report data to OPC UA write items
        /// </summary>
        /// <param name="reportData">The extracted report data</param>
        /// <returns>Collection of OPC UA write items</returns>
        IEnumerable<OpcUaWriteItem> MapReportDataToOpcUaItems(ExtractedReportData reportData);

        /// <summary>
        /// Gets the node ID for a specific data category and field
        /// </summary>
        /// <param name="category">Data category (e.g., "Sample", "Results")</param>
        /// <param name="field">Field name</param>
        /// <returns>OPC UA node ID or null if not found</returns>
        string GetNodeId(string category, string field);

        /// <summary>
        /// Gets the node ID for a specific measurement cycle row
        /// </summary>
        /// <param name="cycleNumber">Cycle number (1-10)</param>
        /// <returns>OPC UA node ID or null if not found</returns>
        string GetCycleRowNodeId(int cycleNumber);

        /// <summary>
        /// Validates that all required node mappings are configured
        /// </summary>
        /// <returns>True if all mappings are valid</returns>
        bool ValidateMappings();
    }
}

