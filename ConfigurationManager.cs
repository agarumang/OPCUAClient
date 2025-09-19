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
        public string StartedTime { get; set; } = "ns=2;s=PDF.StartedTime";
        public string CompletedTime { get; set; } = "ns=2;s=PDF.CompletedTime";
        public string SampleMass { get; set; } = "ns=2;s=PDF.SampleMass";
        public string AbsoluteDensity { get; set; } = "ns=2;s=PDF.AbsoluteDensity";
        public string MeasurementCount { get; set; } = "ns=2;s=PDF.MeasurementCount";
        public string LastExtracted { get; set; } = "ns=2;s=PDF.LastExtracted";
        public string CycleNodePrefix { get; set; } = "ns=2;s=PDF.Cycle";
    }

    public class ApplicationSettings
    {
        public string ExcelFolderName { get; set; } = "excel";
        public string ExcelFileName { get; set; } = "ExtractedExcel.xlsx";
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
                case "measurementcount":
                    mappings.MeasurementCount = nodeId;
                    break;
                case "lastextracted":
                    mappings.LastExtracted = nodeId;
                    break;
                case "cyclenodeprefix":
                    mappings.CycleNodePrefix = nodeId;
                    break;
            }
            
            SaveConfiguration();
        }
    }
}
