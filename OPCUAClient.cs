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
        private readonly OpcUaSettings _settings;

        public bool IsConnected => _session?.Connected == true;

        public OPCUAClient()
        {
            _settings = ConfigurationManager.Configuration.OpcUaSettings;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                // Simple application configuration
                _configuration = new ApplicationConfiguration()
                {
                    ApplicationName = _settings.ApplicationName,
                    ApplicationType = ApplicationType.Client,
                    SecurityConfiguration = new SecurityConfiguration
                    {
                        ApplicationCertificate = new CertificateIdentifier(),
                        AutoAcceptUntrustedCertificates = true,
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
                    ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = _settings.SessionTimeout }
                };

                await _configuration.Validate(ApplicationType.Client);

                // Connect to endpoint
                var selectedEndpoint = CoreClientUtils.SelectEndpoint(_settings.EndpointUrl, useSecurity: false);
                var endpointConfiguration = EndpointConfiguration.Create(_configuration);
                var endpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);

                // Create session
                _session = await Session.Create(
                    _configuration,
                    endpoint,
                    false,
                    "FileReader Session",
                    (uint)_settings.SessionTimeout,
                    null,
                    null
                );

                if (_session?.Connected == true)
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Disconnect error: {ex.Message}");
            }
        }

        public bool WriteValue(string nodeId, object value)
        {
            if (!IsConnected) return false;

            try
            {
                var writeValue = new WriteValue()
                {
                    NodeId = new NodeId(nodeId),
                    AttributeId = Attributes.Value,
                    Value = new DataValue(new Variant(value))
                };

                var writeValues = new WriteValueCollection { writeValue };
                _session.Write(null, writeValues, out StatusCodeCollection results, out DiagnosticInfoCollection diagnostics);

                return results?.Count > 0 && StatusCode.IsGood(results[0]);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Write failed for {nodeId}: {ex.Message}");
                return false;
            }
        }

        public bool WriteDoubleArray(string nodeId, double[] values)
        {
            if (!IsConnected) return false;

            try
            {
                var writeValue = new WriteValue
                {
                    NodeId = new NodeId(nodeId),
                    AttributeId = Attributes.Value,
                    Value = new DataValue(new Variant(values))
                };

                var writeValues = new WriteValueCollection { writeValue };
                _session.Write(null, writeValues, out StatusCodeCollection results, out DiagnosticInfoCollection diagnostics);

                if (results?.Count > 0 && StatusCode.IsGood(results[0]))
                {
                    Console.WriteLine("✅ Double array written successfully!");
                    return true;
                }
                else
                {
                    Console.WriteLine($"❌ Array write failed: {results?[0]}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Array write failed for {nodeId}: {ex.Message}");
                return false;
            }
        }

        public object ReadValue(string nodeId)
        {
            if (!IsConnected) return null;

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
                    return results[0].Value;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Read failed for {nodeId}: {ex.Message}");
                return null;
            }
        }

        public void BrowseRootFolder()
        {
            if (!IsConnected) return;

            try
            {
                _session.Browse(
                    null, null, ObjectIds.ObjectsFolder, 0u, BrowseDirection.Forward,
                    ReferenceTypeIds.HierarchicalReferences, true,
                    (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method,
                    out byte[] cp, out ReferenceDescriptionCollection refs);

                Console.WriteLine("Available OPC UA nodes:");
                foreach (var reference in refs)
                {
                    Console.WriteLine($" - {reference.DisplayName} ({reference.NodeClass})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Browse failed: {ex.Message}");
            }
        }

        public bool WritePdfDataToOpcUa(PdfDataExtractor.ExtractedData pdfData)
        {
            if (!IsConnected) return false;

            try
            {
                var success = true;

                // Write basic PDF data
                var summaryData = ExtractSummaryData(pdfData);
                
                if (!string.IsNullOrEmpty(summaryData.StartedTime))
                    success &= WriteValue(_settings.NodeMappings.StartedTime, summaryData.StartedTime);
                
                if (!string.IsNullOrEmpty(summaryData.CompletedTime))
                    success &= WriteValue(_settings.NodeMappings.CompletedTime, summaryData.CompletedTime);

                // Extract and write sample mass and absolute density if available
                if (!string.IsNullOrEmpty(summaryData.SampleMass))
                {
                    var massValue = ExtractNumericValue(summaryData.SampleMass);
                    if (massValue.HasValue)
                        success &= WriteValue(_settings.NodeMappings.SampleMass, massValue.Value.ToString());
                }

                if (!string.IsNullOrEmpty(summaryData.AbsoluteDensity))
                {
                    var densityValue = ExtractNumericValue(summaryData.AbsoluteDensity);
                    if (densityValue.HasValue)
                        success &= WriteValue(_settings.NodeMappings.AbsoluteDensity, densityValue.Value.ToString());
                }

                // Write measurement cycle data as comma-separated strings
                var measurementTable = pdfData.Tables?.Find(t => t.TableName.Contains("Measurement"));
                if (measurementTable?.Rows?.Count > 0)
                {
                    Console.WriteLine($"Found {measurementTable.Rows.Count} measurement rows to write");
                    
                    // Write up to 10 measurement rows to cycle_row1 through cycle_row10
                    var maxRows = Math.Min(measurementTable.Rows.Count, 10);
                    
                    for (int i = 0; i < maxRows; i++)
                    {
                        if (measurementTable.Rows[i].Count >= 7)
                        {
                            // Create comma-separated string: "Cycle#,Blank,Sample,Volume,VolumeDeviation,Density,DensityDeviation"
                            var rowData = string.Join(",", measurementTable.Rows[i]);
                            
                            // Get the appropriate cycle row tag
                            string cycleRowNodeId = GetCycleRowNodeId(i + 1);
                            if (!string.IsNullOrEmpty(cycleRowNodeId))
                            {
                                success &= WriteValue(cycleRowNodeId, rowData);
                                Console.WriteLine($"✅ Written to cycle_row{i + 1}: {rowData}");
                            }
                        }
                    }

                    // Also write the first row as a double array to Data_import tag for compatibility
                    if (measurementTable.Rows[0].Count >= 7)
                    {
                        var values = new List<double>();
                        for (int i = 1; i < 7; i++) // Skip cycle number, take next 6 values
                        {
                            if (double.TryParse(measurementTable.Rows[0][i], out double val))
                                values.Add(val);
                        }

                        if (values.Count == 6)
                        {
                            success &= WriteDoubleArray(_settings.NodeMappings.DataImportArray, values.ToArray());
                            Console.WriteLine($"✅ Written double array to Data_import: [{string.Join(", ", values)}]");
                        }
                    }
                }

                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ PDF data write failed: {ex.Message}");
                return false;
            }
        }

        private PdfDataExtractor.SummaryData ExtractSummaryData(PdfDataExtractor.ExtractedData data)
        {
            var summary = new PdfDataExtractor.SummaryData();

            // Extract started time
            var startedMatch = System.Text.RegularExpressions.Regex.Match(data.FullText,
                @"Started[:\s]*([A-Za-z]{3}\s+\d{1,2},?\s+\d{4}\s+\d{1,2}:\d{2}\s*(?:AM|PM)?)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (startedMatch.Success)
                summary.StartedTime = startedMatch.Groups[1].Value.Trim();

            // Extract completed time
            var completedMatch = System.Text.RegularExpressions.Regex.Match(data.FullText,
                @"Completed[:\s]*([A-Za-z]{3}\s+\d{1,2},?\s+\d{4}\s+\d{1,2}:\d{2}\s*(?:AM|PM)?)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (completedMatch.Success)
                summary.CompletedTime = completedMatch.Groups[1].Value.Trim();

            // Extract sample mass
            var massMatch = System.Text.RegularExpressions.Regex.Match(data.FullText,
                @"Sample\s+mass[:\s]*(\d+(?:\.\d+)?)\s*g",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (massMatch.Success)
                summary.SampleMass = massMatch.Groups[1].Value.Trim() + " g";

            // Extract absolute density
            var densityMatch = System.Text.RegularExpressions.Regex.Match(data.FullText,
                @"Absolute\s+density[:\s]*(\d+(?:\.\d+)?)\s*g/cm³",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (densityMatch.Success)
                summary.AbsoluteDensity = densityMatch.Groups[1].Value.Trim() + " g/cm³";

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

        private string GetCycleRowNodeId(int rowNumber)
        {
            switch (rowNumber)
            {
                case 1: return _settings.NodeMappings.CycleRow1;
                case 2: return _settings.NodeMappings.CycleRow2;
                case 3: return _settings.NodeMappings.CycleRow3;
                case 4: return _settings.NodeMappings.CycleRow4;
                case 5: return _settings.NodeMappings.CycleRow5;
                case 6: return _settings.NodeMappings.CycleRow6;
                case 7: return _settings.NodeMappings.CycleRow7;
                case 8: return _settings.NodeMappings.CycleRow8;
                case 9: return _settings.NodeMappings.CycleRow9;
                case 10: return _settings.NodeMappings.CycleRow10;
                default: return null;
            }
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