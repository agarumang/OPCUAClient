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
        private readonly string _endpointUrl;

        public bool IsConnected => _isConnected && _session?.Connected == true;

        public OPCUAClient(string endpointUrl = "opc.tcp://localhost:49320")
        {
            _endpointUrl = endpointUrl;
            _isConnected = false;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                // Create application configuration
                _configuration = new ApplicationConfiguration()
                {
                    ApplicationName = "PDF Data Extractor OPC UA Client",
                    ApplicationUri = Utils.Format(@"urn:{0}:PDFDataExtractor", System.Net.Dns.GetHostName()),
                    ApplicationType = ApplicationType.Client,
                    SecurityConfiguration = new SecurityConfiguration
                    {
                        ApplicationCertificate = new CertificateIdentifier { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\MachineDefault", SubjectName = Utils.Format(@"CN={0}, DC={1}", "PDF Data Extractor", System.Net.Dns.GetHostName()) },
                        TrustedIssuerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Certificate Authorities" },
                        TrustedPeerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Applications" },
                        RejectedCertificateStore = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\RejectedCertificates" },
                        AutoAcceptUntrustedCertificates = true,
                        AddAppCertToTrustedStore = true
                    },
                    TransportConfigurations = new TransportConfigurationCollection(),
                    TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                    ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
                    TraceConfiguration = new TraceConfiguration()
                };

                await _configuration.Validate(ApplicationType.Client);
                if (_configuration.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                {
                    _configuration.CertificateValidator.CertificateValidation += (s, e) => { e.Accept = (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted); };
                }

                // Discover endpoints
                var endpointDescription = CoreClientUtils.SelectEndpoint(_endpointUrl, useSecurity: false);
                var endpointConfiguration = EndpointConfiguration.Create(_configuration);
                var endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

                // Create session
                _session = await Session.Create(_configuration, endpoint, false, "PDF Data Extractor Session", 60000, new UserIdentity(new AnonymousIdentityToken()), null);

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

                // Map PDF data to OPC UA nodes (adjust node IDs according to your Kepware server configuration)
                if (!string.IsNullOrEmpty(summaryData.StartedTime) && summaryData.StartedTime != "Not found")
                {
                    nodeValues["ns=2;s=PDF.StartedTime"] = summaryData.StartedTime;
                }

                if (!string.IsNullOrEmpty(summaryData.CompletedTime) && summaryData.CompletedTime != "Not found")
                {
                    nodeValues["ns=2;s=PDF.CompletedTime"] = summaryData.CompletedTime;
                }

                if (!string.IsNullOrEmpty(summaryData.SampleMass) && summaryData.SampleMass != "Not found")
                {
                    // Extract numeric value from "X.XXXX g" format
                    var massValue = ExtractNumericValue(summaryData.SampleMass);
                    if (massValue.HasValue)
                    {
                        nodeValues["ns=2;s=PDF.SampleMass"] = massValue.Value;
                    }
                }

                if (!string.IsNullOrEmpty(summaryData.AbsoluteDensity) && summaryData.AbsoluteDensity != "Not found")
                {
                    // Extract numeric value from "X.XXXX g/cm³" format
                    var densityValue = ExtractNumericValue(summaryData.AbsoluteDensity);
                    if (densityValue.HasValue)
                    {
                        nodeValues["ns=2;s=PDF.AbsoluteDensity"] = densityValue.Value;
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
                        nodeValues["ns=2;s=PDF.MeasurementCount"] = measurementCount;

                        // Write first few measurement cycle values (adjust as needed)
                        for (int i = 0; i < Math.Min(measurementTable.Rows.Count, 10); i++)
                        {
                            if (measurementTable.Rows[i].Count >= 7)
                            {
                                // Cycle number
                                if (int.TryParse(measurementTable.Rows[i][0], out int cycleNum))
                                {
                                    nodeValues[$"ns=2;s=PDF.Cycle{i + 1}.Number"] = cycleNum;
                                }

                                // Volume
                                if (double.TryParse(measurementTable.Rows[i][3], out double volume))
                                {
                                    nodeValues[$"ns=2;s=PDF.Cycle{i + 1}.Volume"] = volume;
                                }

                                // Density
                                if (double.TryParse(measurementTable.Rows[i][5], out double density))
                                {
                                    nodeValues[$"ns=2;s=PDF.Cycle{i + 1}.Density"] = density;
                                }
                            }
                        }
                    }
                }

                // Write timestamp of data extraction
                nodeValues["ns=2;s=PDF.LastExtracted"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

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