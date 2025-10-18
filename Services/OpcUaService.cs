using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using FileReader.Interfaces;
using FileReader.Models;

namespace FileReader.Services
{
    /// <summary>
    /// Service responsible for OPC UA operations
    /// Follows Single Responsibility Principle - only handles OPC UA communication
    /// </summary>
    public class OpcUaService : IOpcUaService, IDisposable
    {
        private Session _session;
        private ApplicationConfiguration _configuration;
        private readonly OpcUaSettings _settings;
        private readonly INodeMappingService _nodeMappingService;

        public bool IsConnected => _session?.Connected == true;

        public OpcUaService(OpcUaSettings settings, INodeMappingService nodeMappingService)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _nodeMappingService = nodeMappingService ?? throw new ArgumentNullException(nameof(nodeMappingService));
        }

        /// <summary>
        /// Connects to the OPC UA server
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                await CreateApplicationConfiguration();
                await EstablishSession();
                
                if (IsConnected)
                {
                    Console.WriteLine("✅ Connected to OPC UA Server!");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Connection failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Disconnects from the OPC UA server
        /// </summary>
        public async Task DisconnectAsync()
        {
            try
            {
                if (_session != null)
                {
                    await _session.CloseAsync();
                    _session.Dispose();
                    _session = null;
                    Console.WriteLine("🔌 Disconnected from OPC UA Server");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Disconnect error: {ex.Message}");
            }
        }

        /// <summary>
        /// Writes extracted report data to OPC UA server
        /// </summary>
        public async Task<bool> WriteReportDataAsync(ExtractedReportData reportData)
        {
            if (!IsConnected)
            {
                Console.WriteLine("⚠️ Cannot write data - not connected to OPC UA server");
                return false;
            }

            if (reportData == null)
            {
                Console.WriteLine("⚠️ Cannot write data - report data is null");
                return false;
            }

            try
            {
                Console.WriteLine("🔄 Writing comprehensive report data to OPC UA...");
                
                var writeItems = _nodeMappingService.MapReportDataToOpcUaItems(reportData);
                var success = await WriteBatchAsync(writeItems);
                
                Console.WriteLine($"🔄 Report data write completed. Success: {success}");
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Report data write failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Writes a single value to an OPC UA node
        /// </summary>
        public async Task<bool> WriteValueAsync(string nodeId, object value)
        {
            if (!IsConnected)
            {
                Console.WriteLine($"⚠️ WriteValue called but not connected - NodeId: {nodeId}");
                return false;
            }

            if (string.IsNullOrEmpty(nodeId))
            {
                Console.WriteLine("⚠️ Cannot write value - nodeId is null or empty");
                return false;
            }

            try
            {
                Console.WriteLine($"🔧 Writing value - NodeId: {nodeId}, Value: '{value}', Type: {value?.GetType().Name}");
                
                var writeValue = CreateWriteValue(nodeId, value);
                var writeValues = new WriteValueCollection { writeValue };
                
                _session.Write(null, writeValues, out StatusCodeCollection results, out DiagnosticInfoCollection diagnostics);

                var success = results?.Count > 0 && StatusCode.IsGood(results[0]);
                
                if (success)
                {
                    Console.WriteLine($"✅ Write successful - NodeId: {nodeId}");
                }
                else
                {
                    Console.WriteLine($"❌ Write failed - NodeId: {nodeId}, Status: {results?[0]}");
                    LogDiagnostics(diagnostics);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Write exception for {nodeId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Writes multiple values to OPC UA nodes
        /// </summary>
        public async Task<bool> WriteBatchAsync(IEnumerable<OpcUaWriteItem> writeItems)
        {
            if (!IsConnected)
            {
                Console.WriteLine("⚠️ Cannot write batch - not connected to OPC UA server");
                return false;
            }

            var items = writeItems?.ToList();
            if (items == null || !items.Any())
            {
                Console.WriteLine("⚠️ No items to write");
                return true; // Consider empty batch as success
            }

            try
            {
                Console.WriteLine($"🔄 Writing batch of {items.Count} items to OPC UA...");
                
                var writeValues = new WriteValueCollection();
                foreach (var item in items)
                {
                    writeValues.Add(CreateWriteValue(item.NodeId, item.Value));
                }

                _session.Write(null, writeValues, out StatusCodeCollection results, out DiagnosticInfoCollection diagnostics);

                var successCount = 0;
                for (int i = 0; i < results.Count && i < items.Count; i++)
                {
                    var success = StatusCode.IsGood(results[i]);
                    if (success)
                    {
                        successCount++;
                        Console.WriteLine($"✅ {items[i].Description}: Success");
                    }
                    else
                    {
                        Console.WriteLine($"❌ {items[i].Description}: Failed - {results[i]}");
                    }
                }

                var allSuccess = successCount == items.Count;
                Console.WriteLine($"📊 Batch write completed: {successCount}/{items.Count} successful");
                
                if (!allSuccess)
                {
                    LogDiagnostics(diagnostics);
                }

                return allSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Batch write failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Reads a value from an OPC UA node
        /// </summary>
        public async Task<object> ReadValueAsync(string nodeId)
        {
            if (!IsConnected)
            {
                Console.WriteLine($"⚠️ Cannot read value - not connected. NodeId: {nodeId}");
                return null;
            }

            if (string.IsNullOrEmpty(nodeId))
            {
                Console.WriteLine("⚠️ Cannot read value - nodeId is null or empty");
                return null;
            }

            try
            {
                var readValue = new ReadValueId()
                {
                    NodeId = new NodeId(nodeId),
                    AttributeId = Attributes.Value
                };

                var readValues = new ReadValueIdCollection { readValue };
                _session.Read(null, 0, TimestampsToReturn.Neither, readValues, out DataValueCollection results, out DiagnosticInfoCollection diagnostics);

                if (results?.Count > 0 && StatusCode.IsGood(results[0].StatusCode))
                {
                    Console.WriteLine($"📖 Read successful - NodeId: {nodeId}, Value: {results[0].Value}");
                    return results[0].Value;
                }
                else
                {
                    Console.WriteLine($"❌ Read failed - NodeId: {nodeId}, Status: {results?[0]?.StatusCode}");
                    LogDiagnostics(diagnostics);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Read failed for {nodeId}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Browses the root folder of the OPC UA server
        /// </summary>
        public void BrowseRootFolder()
        {
            if (!IsConnected)
            {
                Console.WriteLine("⚠️ Cannot browse - not connected to OPC UA server");
                return;
            }

            try
            {
                _session.Browse(
                    null, null, ObjectIds.ObjectsFolder, 0u, BrowseDirection.Forward,
                    ReferenceTypeIds.HierarchicalReferences, true,
                    (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method,
                    out byte[] cp, out ReferenceDescriptionCollection refs);

                Console.WriteLine("📁 Available OPC UA nodes:");
                foreach (var reference in refs)
                {
                    Console.WriteLine($"   - {reference.DisplayName} ({reference.NodeClass})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Browse failed: {ex.Message}");
            }
        }

        #region Private Methods

        private async Task CreateApplicationConfiguration()
        {
            _configuration = new ApplicationConfiguration()
            {
                ApplicationName = _settings.ApplicationName,
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier(),
                    AutoAcceptUntrustedCertificates = _settings.AutoAcceptUntrustedCertificates,
                    AddAppCertToTrustedStore = true,
                    TrustedIssuerCertificates = new CertificateTrustList
                    {
                        StoreType = "Directory",
                        StorePath = "OPC Foundation/CertificateStores/UA Certificate Authorities"
                    },
                    TrustedPeerCertificates = new CertificateTrustList
                    {
                        StoreType = "Directory",
                        StorePath = "OPC Foundation/CertificateStores/UA Applications"
                    },
                    RejectedCertificateStore = new CertificateTrustList
                    {
                        StoreType = "Directory",
                        StorePath = "OPC Foundation/CertificateStores/RejectedCertificates"
                    }
                },
                ClientConfiguration = new ClientConfiguration 
                { 
                    DefaultSessionTimeout = _settings.SessionTimeout 
                }
            };

            await _configuration.Validate(ApplicationType.Client);
        }

        private async Task EstablishSession()
        {
            var selectedEndpoint = CoreClientUtils.SelectEndpoint(_settings.EndpointUrl, useSecurity: _settings.UseSecurity);
            var endpointConfiguration = EndpointConfiguration.Create(_configuration);
            var endpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);

            _session = await Session.Create(
                _configuration,
                endpoint,
                false,
                $"{_settings.ApplicationName} Session",
                (uint)_settings.SessionTimeout,
                null,
                null
            );
        }

        private WriteValue CreateWriteValue(string nodeId, object value)
        {
            // Handle different value types appropriately
            Variant variant;
            
            if (value is double[] doubleArray)
            {
                variant = new Variant(doubleArray);
            }
            else
            {
                // Convert to string for most cases
                var stringValue = value?.ToString() ?? string.Empty;
                variant = new Variant(stringValue);
            }

            return new WriteValue()
            {
                NodeId = new NodeId(nodeId),
                AttributeId = Attributes.Value,
                Value = new DataValue(variant)
            };
        }

        private void LogDiagnostics(DiagnosticInfoCollection diagnostics)
        {
            if (diagnostics?.Count > 0)
            {
                foreach (var diagnostic in diagnostics)
                {
                    if (diagnostic != null)
                    {
                        Console.WriteLine($"    Diagnostic: {diagnostic}");
                    }
                }
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            try
            {
                DisconnectAsync().GetAwaiter().GetResult();
            }
            catch
            {
                // Silent cleanup
            }
        }

        #endregion
    }
}

