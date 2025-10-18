using System;
using FileReader.Interfaces;
using FileReader.Services;

namespace FileReader.Services
{
    /// <summary>
    /// Simple service container for dependency injection
    /// Follows Dependency Inversion Principle
    /// </summary>
    public class ServiceContainer
    {
        private readonly AppConfiguration _configuration;

        public ServiceContainer(AppConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Creates and configures the data extraction service
        /// </summary>
        public IDataExtractionService CreateDataExtractionService()
        {
            return new DataExtractionService();
        }

        /// <summary>
        /// Creates and configures the node mapping service
        /// </summary>
        public INodeMappingService CreateNodeMappingService()
        {
            return new NodeMappingService(_configuration.OpcUaSettings.NodeMappings);
        }

        /// <summary>
        /// Creates and configures the OPC UA service
        /// </summary>
        public IOpcUaService CreateOpcUaService()
        {
            var nodeMappingService = CreateNodeMappingService();
            return new OpcUaService(_configuration.OpcUaSettings, nodeMappingService);
        }

        /// <summary>
        /// Creates and configures the CSV export service
        /// </summary>
        public ICsvExportService CreateCsvExportService()
        {
            return new CsvExportService();
        }

        /// <summary>
        /// Creates the application orchestrator with all dependencies
        /// </summary>
        public ApplicationOrchestrator CreateApplicationOrchestrator()
        {
            return new ApplicationOrchestrator(
                CreateDataExtractionService(),
                CreateOpcUaService(),
                CreateCsvExportService(),
                _configuration
            );
        }
    }
}

