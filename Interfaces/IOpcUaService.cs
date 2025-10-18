using System.Collections.Generic;
using System.Threading.Tasks;
using FileReader.Models;

namespace FileReader.Interfaces
{
    /// <summary>
    /// Service responsible for OPC UA operations
    /// </summary>
    public interface IOpcUaService
    {
        /// <summary>
        /// Gets the connection status
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Connects to the OPC UA server
        /// </summary>
        /// <returns>True if connection successful</returns>
        Task<bool> ConnectAsync();

        /// <summary>
        /// Disconnects from the OPC UA server
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// Writes extracted report data to OPC UA server
        /// </summary>
        /// <param name="reportData">The extracted report data</param>
        /// <returns>True if all writes successful</returns>
        Task<bool> WriteReportDataAsync(ExtractedReportData reportData);

        /// <summary>
        /// Writes a single value to an OPC UA node
        /// </summary>
        /// <param name="nodeId">The OPC UA node ID</param>
        /// <param name="value">The value to write</param>
        /// <returns>True if write successful</returns>
        Task<bool> WriteValueAsync(string nodeId, object value);

        /// <summary>
        /// Writes multiple values to OPC UA nodes
        /// </summary>
        /// <param name="writeItems">Collection of items to write</param>
        /// <returns>True if all writes successful</returns>
        Task<bool> WriteBatchAsync(IEnumerable<OpcUaWriteItem> writeItems);

        /// <summary>
        /// Reads a value from an OPC UA node
        /// </summary>
        /// <param name="nodeId">The OPC UA node ID</param>
        /// <returns>The value read from the node</returns>
        Task<object> ReadValueAsync(string nodeId);

        /// <summary>
        /// Browses the root folder of the OPC UA server
        /// </summary>
        void BrowseRootFolder();
    }
}

