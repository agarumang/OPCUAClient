using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

namespace FileReader
{
    public class OPCUAClient
    {
        private Session _session;
        private ApplicationConfiguration _configuration;
        private bool _isConnected;
        private readonly OpcUaSettings _settings;

        public bool IsConnected => _isConnected && _session?.Connected == true;

        public OPCUAClient(OpcUaSettings settings = null)
        {
            _settings = settings ?? ConfigurationManager.Configuration.OpcUaSettings;
            _isConnected = false;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                // Ensure certificates exist before creating configuration
                await CertificateManager.EnsureCertificatesExistAsync();

                // Create application configuration with local certificate stores
                var appDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                
                _configuration = new ApplicationConfiguration()
                {
                    ApplicationName = _settings.ApplicationName,
                    ApplicationUri = Utils.Format(@"urn:{0}:PDFDataExtractor", Environment.MachineName),
                    ApplicationType = ApplicationType.Client,
                    SecurityConfiguration = new SecurityConfiguration
                    {
                        ApplicationCertificate = new CertificateIdentifier 
                        { 
                            StoreType = @"Directory", 
                            StorePath = System.IO.Path.Combine(appDir, "Certificates", "Own"),
                            SubjectName = Utils.Format(@"CN={0}, DC={1}", _settings.ApplicationName, Environment.MachineName)
                        },
                        TrustedIssuerCertificates = new CertificateTrustList 
                        { 
                            StoreType = @"Directory", 
                            StorePath = System.IO.Path.Combine(appDir, "Certificates", "TrustedIssuers")
                        },
                        TrustedPeerCertificates = new CertificateTrustList 
                        { 
                            StoreType = @"Directory", 
                            StorePath = System.IO.Path.Combine(appDir, "Certificates", "TrustedPeers")
                        },
                        RejectedCertificateStore = new CertificateTrustList 
                        { 
                            StoreType = @"Directory", 
                            StorePath = System.IO.Path.Combine(appDir, "Certificates", "Rejected")
                        },
                        AutoAcceptUntrustedCertificates = _settings.AutoAcceptUntrustedCertificates,
                        AddAppCertToTrustedStore = true
                    },
                    TransportConfigurations = new TransportConfigurationCollection(),
                    TransportQuotas = new TransportQuotas { OperationTimeout = _settings.OperationTimeout },
                    ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = _settings.SessionTimeout },
                    TraceConfiguration = new TraceConfiguration()
                };

                await _configuration.Validate(ApplicationType.Client);
                if (_configuration.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                {
                    _configuration.CertificateValidator.CertificateValidation += (s, e) => { e.Accept = (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted); };
                }

                // Discover endpoints
                var endpointDescription = CoreClientUtils.SelectEndpoint(_settings.EndpointUrl, useSecurity: _settings.UseSecurity);
                var endpointConfiguration = EndpointConfiguration.Create(_configuration);
                var endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

                // Create session with user identity
                UserIdentity userIdentity;
                if (!string.IsNullOrEmpty(_settings.Username))
                {
                    userIdentity = new UserIdentity(_settings.Username, _settings.Password);
                }
                else
                {
                    userIdentity = new UserIdentity(new AnonymousIdentityToken());
                }

                _session = await Session.Create(_configuration, endpoint, false, "PDF Data Extractor Session", (uint)_settings.SessionTimeout, userIdentity, null);

                if (_session != null && _session.Connected)
                {
                    _isConnected = true;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to connect to OPC UA server: {ex.Message}", ex);
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (_session != null)
                {
                    await _session.CloseAsync();
                    _session.Dispose();
                    _session = null;
                }
                _isConnected = false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error during disconnect: {ex.Message}", ex);
            }
        }

        public bool WriteValue(string nodeId, object value)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("OPC UA client is not connected");
            }

            try
            {
                var writeValue = new WriteValue()
                {
                    NodeId = new NodeId(nodeId),
                    AttributeId = Attributes.Value,
                    Value = new DataValue(new Variant(value))
                };

                var writeValueCollection = new WriteValueCollection { writeValue };
                StatusCodeCollection results;
                DiagnosticInfoCollection diagnosticInfos;
                
                _session.Write(null, writeValueCollection, out results, out diagnosticInfos);

                return results != null && results.Count > 0 && StatusCode.IsGood(results[0]);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to write value to node {nodeId}: {ex.Message}", ex);
            }
        }

        public bool WriteMultipleValues(Dictionary<string, object> nodeValues)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("OPC UA client is not connected");
            }

            try
            {
                var writeValueCollection = new WriteValueCollection();

                foreach (var kvp in nodeValues)
                {
                    var writeValue = new WriteValue()
                    {
                        NodeId = new NodeId(kvp.Key),
                        AttributeId = Attributes.Value,
                        Value = new DataValue(new Variant(kvp.Value))
                    };
                    writeValueCollection.Add(writeValue);
                }

                StatusCodeCollection results;
                DiagnosticInfoCollection diagnosticInfos;
                
                _session.Write(null, writeValueCollection, out results, out diagnosticInfos);

                // Check if all writes were successful
                if (results != null && results.Count == nodeValues.Count)
                {
                    foreach (var result in results)
                    {
                        if (!StatusCode.IsGood(result))
                        {
                            return false;
                        }
                    }
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to write multiple values: {ex.Message}", ex);
            }
        }

        public object ReadValue(string nodeId)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("OPC UA client is not connected");
            }

            try
            {
                var readValue = new ReadValueId()
                {
                    NodeId = new NodeId(nodeId),
                    AttributeId = Attributes.Value
                };

                var readValueCollection = new ReadValueIdCollection { readValue };
                DataValueCollection results;
                DiagnosticInfoCollection diagnosticInfos;
                
                _session.Read(null, 0, TimestampsToReturn.Neither, readValueCollection, out results, out diagnosticInfos);

                if (results != null && results.Count > 0 && StatusCode.IsGood(results[0].StatusCode))
                {
                    return results[0].Value;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to read value from node {nodeId}: {ex.Message}", ex);
            }
        }

        public bool WritePdfDataToOpcUa(PdfDataExtractor.ExtractedData pdfData)
        {
            if (!IsConnected)
            {
                return false;
            }

            try
            {
                var nodeValues = new Dictionary<string, object>();

                // Extract summary data for writing to OPC UA
                var summaryData = ExtractSummaryDataForOpcUa(pdfData);

                // Map PDF data to OPC UA nodes using configuration
                if (!string.IsNullOrEmpty(summaryData.StartedTime) && summaryData.StartedTime != "Not found")
                {
                    nodeValues[_settings.NodeMappings.StartedTime] = summaryData.StartedTime;
                }

                if (!string.IsNullOrEmpty(summaryData.CompletedTime) && summaryData.CompletedTime != "Not found")
                {
                    nodeValues[_settings.NodeMappings.CompletedTime] = summaryData.CompletedTime;
                }

                if (!string.IsNullOrEmpty(summaryData.SampleMass) && summaryData.SampleMass != "Not found")
                {
                    // Extract numeric value from "X.XXXX g" format
                    var massValue = ExtractNumericValue(summaryData.SampleMass);
                    if (massValue.HasValue)
                    {
                        nodeValues[_settings.NodeMappings.SampleMass] = massValue.Value;
                    }
                }

                if (!string.IsNullOrEmpty(summaryData.AbsoluteDensity) && summaryData.AbsoluteDensity != "Not found")
                {
                    // Extract numeric value from "X.XXXX g/cm³" format
                    var densityValue = ExtractNumericValue(summaryData.AbsoluteDensity);
                    if (densityValue.HasValue)
                    {
                        nodeValues[_settings.NodeMappings.AbsoluteDensity] = densityValue.Value;
                    }
                }

                // Write measurement cycles count
                var measurementCount = 0;
                if (pdfData.Tables != null && pdfData.Tables.Count > 0)
                {
                    var measurementTable = pdfData.Tables.Find(t => t.TableName.Contains("Measurement"));
                    if (measurementTable != null)
                    {
                        measurementCount = measurementTable.Rows.Count;
                        nodeValues[_settings.NodeMappings.MeasurementCount] = measurementCount;

                        // Write measurement cycle values using configuration
                        var maxCycles = Math.Min(measurementTable.Rows.Count, ConfigurationManager.Configuration.ApplicationSettings.MaxMeasurementCycles);
                        for (int i = 0; i < maxCycles; i++)
                        {
                            if (measurementTable.Rows[i].Count >= 7)
                            {
                                var cyclePrefix = _settings.NodeMappings.CycleNodePrefix;
                                
                                // Cycle number
                                if (int.TryParse(measurementTable.Rows[i][0], out int cycleNum))
                                {
                                    nodeValues[$"{cyclePrefix}{i + 1}.Number"] = cycleNum;
                                }

                                // Volume
                                if (double.TryParse(measurementTable.Rows[i][3], out double volume))
                                {
                                    nodeValues[$"{cyclePrefix}{i + 1}.Volume"] = volume;
                                }

                                // Density
                                if (double.TryParse(measurementTable.Rows[i][5], out double density))
                                {
                                    nodeValues[$"{cyclePrefix}{i + 1}.Density"] = density;
                                }
                            }
                        }
                    }
                }

                // Write timestamp of data extraction
                nodeValues[_settings.NodeMappings.LastExtracted] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // Write all values to OPC UA server
                return WriteMultipleValues(nodeValues);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to write PDF data to OPC UA: {ex.Message}", ex);
            }
        }

        private PdfDataExtractor.SummaryData ExtractSummaryDataForOpcUa(PdfDataExtractor.ExtractedData data)
        {
            var summary = new PdfDataExtractor.SummaryData();

            var startedMatch = System.Text.RegularExpressions.Regex.Match(data.FullText,
                @"Started[:\s]*([A-Za-z]{3}\s+\d{1,2},?\s+\d{4}\s+\d{1,2}:\d{2}\s*(?:AM|PM)?)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (startedMatch.Success)
            {
                summary.StartedTime = startedMatch.Groups[1].Value.Trim();
            }

            var completedMatch = System.Text.RegularExpressions.Regex.Match(data.FullText,
                @"Completed[:\s]*([A-Za-z]{3}\s+\d{1,2},?\s+\d{4}\s+\d{1,2}:\d{2}\s*(?:AM|PM)?)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (completedMatch.Success)
            {
                summary.CompletedTime = completedMatch.Groups[1].Value.Trim();
            }

            var massMatch = System.Text.RegularExpressions.Regex.Match(data.FullText,
                @"Sample\s+mass[:\s]*(\d+(?:\.\d+)?)\s*g",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (massMatch.Success)
            {
                summary.SampleMass = massMatch.Groups[1].Value.Trim() + " g";
            }

            var densityMatch = System.Text.RegularExpressions.Regex.Match(data.FullText,
                @"Absolute\s+density[:\s]*(\d+(?:\.\d+)?)\s*g/cm³",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (densityMatch.Success)
            {
                summary.AbsoluteDensity = densityMatch.Groups[1].Value.Trim() + " g/cm³";
            }

            return summary;
        }

        private double? ExtractNumericValue(string valueWithUnit)
        {
            var match = System.Text.RegularExpressions.Regex.Match(valueWithUnit, @"(\d+(?:\.\d+)?)");
            if (match.Success && double.TryParse(match.Groups[1].Value, out double result))
            {
                return result;
            }
            return null;
        }

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
    }
}