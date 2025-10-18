using System;
using System.IO;
using Newtonsoft.Json;

namespace FileReader
{
    public class AppConfiguration
    {
        public OpcUaSettings OpcUaSettings { get; set; } = new OpcUaSettings();
        public ApplicationSettings ApplicationSettings { get; set; } = new ApplicationSettings();
    }

    public class OpcUaSettings
    {
        public string EndpointUrl { get; set; } = "opc.tcp://localhost:49320";
        public string ApplicationName { get; set; } = "PDF Data Extractor OPC UA Client";
        public int SessionTimeout { get; set; } = 60000;
        public int OperationTimeout { get; set; } = 15000;
        public bool AutoAcceptUntrustedCertificates { get; set; } = true;
        public bool UseSecurity { get; set; } = false;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string PreferredAuthenticationType { get; set; } = "Anonymous";
        public NodeMappings NodeMappings { get; set; } = new NodeMappings();
    }

    public class NodeMappings
    {
        // Existing mappings
        public string StartedTime { get; set; } = "ns=2;s=pdf_extractor.Data_import.started";
        public string CompletedTime { get; set; } = "ns=2;s=pdf_extractor.Data_import.completed";
        public string SampleMass { get; set; } = "ns=2;s=pdf_extractor.Data_import.sample_mass";
        public string AbsoluteDensity { get; set; } = "ns=2;s=pdf_extractor.Data_import.absolute_density";
        public string CycleRow1 { get; set; } = "ns=2;s=pdf_extractor.Data_import.cycle_row1";
        public string CycleRow2 { get; set; } = "ns=2;s=pdf_extractor.Data_import.cycle_row2";
        public string CycleRow3 { get; set; } = "ns=2;s=pdf_extractor.Data_import.cycle_row3";
        public string CycleRow4 { get; set; } = "ns=2;s=pdf_extractor.Data_import.cycle_row4";
        public string CycleRow5 { get; set; } = "ns=2;s=pdf_extractor.Data_import.cycle_row5";
        public string CycleRow6 { get; set; } = "ns=2;s=pdf_extractor.Data_import.cycle_row6";
        public string CycleRow7 { get; set; } = "ns=2;s=pdf_extractor.Data_import.cycle_row7";
        public string CycleRow8 { get; set; } = "ns=2;s=pdf_extractor.Data_import.cycle_row8";
        public string CycleRow9 { get; set; } = "ns=2;s=pdf_extractor.Data_import.cycle_row9";
        public string CycleRow10 { get; set; } = "ns=2;s=pdf_extractor.Data_import.cycle_row10";
        public string DataImportArray { get; set; } = "ns=2;s=pdf_extractor.Data_import.Data_import";

        // Report Info mappings
        public string ReportGenerated { get; set; } = "ns=2;s=pdf_extractor.ReportInfo.generated";
        public string SourceFile { get; set; } = "ns=2;s=pdf_extractor.ReportInfo.source_file";
        public string ReportDate { get; set; } = "ns=2;s=pdf_extractor.ReportInfo.report_date";
        public string SerialNumber { get; set; } = "ns=2;s=pdf_extractor.ReportInfo.serial_number";
        public string ReportType { get; set; } = "ns=2;s=pdf_extractor.ReportInfo.report_type";

        // Instrument mappings
        public string InstrumentName { get; set; } = "ns=2;s=pdf_extractor.Instrument.instrument_name";
        public string InstrumentSerialNumber { get; set; } = "ns=2;s=pdf_extractor.Instrument.serial_number";
        public string InstrumentVersion { get; set; } = "ns=2;s=pdf_extractor.Instrument.version";

        // Sample mappings (additional to existing ones)
        public string SampleRecord { get; set; } = "ns=2;s=pdf_extractor.Sample.record";
        public string SampleOperator { get; set; } = "ns=2;s=pdf_extractor.Sample.operator";
        public string SampleSubmitter { get; set; } = "ns=2;s=pdf_extractor.Sample.submitter";
        public string ReportTime { get; set; } = "ns=2;s=pdf_extractor.Sample.report_time";

        // Parameters mappings
        public string ChamberDiameter { get; set; } = "ns=2;s=pdf_extractor.Parameters.chamber_diameter";
        public string PreparationCycles { get; set; } = "ns=2;s=pdf_extractor.Parameters.preparation_cycles";
        public string MeasurementCycles { get; set; } = "ns=2;s=pdf_extractor.Parameters.measurement_cycles";
        public string BlankData { get; set; } = "ns=2;s=pdf_extractor.Parameters.blank_data";
        public string ConsolidationForce { get; set; } = "ns=2;s=pdf_extractor.Parameters.consolidation_force";
        public string ConversionFactor { get; set; } = "ns=2;s=pdf_extractor.Parameters.conversion_factor";
        public string ZeroDepth { get; set; } = "ns=2;s=pdf_extractor.Parameters.zero_depth";

        // Results mappings
        public string AverageEnvelopeVolume { get; set; } = "ns=2;s=pdf_extractor.Results.average_envelope_volume";
        public string AverageEnvelopeDensity { get; set; } = "ns=2;s=pdf_extractor.Results.average_envelope_density";
        public string SpecificPoreVolume { get; set; } = "ns=2;s=pdf_extractor.Results.specific_pore_volume";
        public string Porosity { get; set; } = "ns=2;s=pdf_extractor.Results.porosity";
        public string PercentSampleVolume { get; set; } = "ns=2;s=pdf_extractor.Results.percent_sample_volume";
        public string StandardDeviationVolume { get; set; } = "ns=2;s=pdf_extractor.Results.standard_deviation_volume";
        public string StandardDeviationDensity { get; set; } = "ns=2;s=pdf_extractor.Results.standard_deviation_density";
    }

    public class ApplicationSettings
    {
        public string OutputFolderName { get; set; } = "output";
        public string CsvFileName { get; set; } = "ExtractedData.csv";
        public int MaxMeasurementCycles { get; set; } = 10;
    }

    public static class ConfigurationManager
    {
        private static AppConfiguration _configuration;
        private const string ConfigFileName = "appsettings.json";

        public static AppConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    LoadConfiguration();
                }
                return _configuration;
            }
        }

        public static void LoadConfiguration()
        {
            try
            {
                var configPath = GetConfigPath();
                
                if (File.Exists(configPath))
                {
                    var jsonContent = File.ReadAllText(configPath);
                    _configuration = JsonConvert.DeserializeObject<AppConfiguration>(jsonContent);
                }
                else
                {
                    // Create default configuration if file doesn't exist
                    _configuration = new AppConfiguration();
                    SaveConfiguration();
                }
            }
            catch (Exception)
            {
                // If config loading fails, use default configuration
                _configuration = new AppConfiguration();
            }
        }

        public static void SaveConfiguration()
        {
            try
            {
                var configPath = GetConfigPath();
                var jsonContent = JsonConvert.SerializeObject(_configuration, Formatting.Indented);
                File.WriteAllText(configPath, jsonContent);
            }
            catch (Exception)
            {
                // Silent error handling for config save
            }
        }

        private static string GetConfigPath()
        {
            // Try to get config from executable directory first
            var exeDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var configPath = Path.Combine(exeDirectory, ConfigFileName);
            
            if (File.Exists(configPath))
            {
                return configPath;
            }

            // Fallback to current directory
            return Path.Combine(Directory.GetCurrentDirectory(), ConfigFileName);
        }

        public static void ReloadConfiguration()
        {
            _configuration = null;
            LoadConfiguration();
        }

        // Helper method to update OPC UA endpoint at runtime
        public static void UpdateOpcUaEndpoint(string newEndpoint)
        {
            Configuration.OpcUaSettings.EndpointUrl = newEndpoint;
            SaveConfiguration();
        }

        // Helper method to update node mappings
        public static void UpdateNodeMapping(string property, string nodeId)
        {
            var mappings = Configuration.OpcUaSettings.NodeMappings;
            
            switch (property.ToLower())
            {
                case "startedtime":
                    mappings.StartedTime = nodeId;
                    break;
                case "completedtime":
                    mappings.CompletedTime = nodeId;
                    break;
                case "samplemass":
                    mappings.SampleMass = nodeId;
                    break;
                case "absolutedensity":
                    mappings.AbsoluteDensity = nodeId;
                    break;
                // Note: MeasurementCount, LastExtracted, and CycleNodePrefix properties removed
                // Individual cycle rows are now used instead
            }
            
            SaveConfiguration();
        }
    }
}
