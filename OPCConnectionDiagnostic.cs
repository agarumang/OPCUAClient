using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

namespace FileReader
{
    public static class OPCConnectionDiagnostic
    {
        public static async Task<bool> DiagnoseConnectionAsync(string endpointUrl = null)
        {
            Console.WriteLine("=== OPC UA Connection Diagnostic ===");
            Console.WriteLine();

            try
            {
                // Load configuration
                var config = ConfigurationManager.Configuration.OpcUaSettings;
                var testEndpoint = endpointUrl ?? config.EndpointUrl;
                
                Console.WriteLine($"Testing connection to: {testEndpoint}");
                Console.WriteLine($"Application Name: {config.ApplicationName}");
                Console.WriteLine($"Session Timeout: {config.SessionTimeout}ms");
                Console.WriteLine($"Operation Timeout: {config.OperationTimeout}ms");
                Console.WriteLine($"Use Security: {config.UseSecurity}");
                Console.WriteLine($"Auto Accept Certificates: {config.AutoAcceptUntrustedCertificates}");
                Console.WriteLine();

                // Test 1: Network connectivity
                Console.WriteLine("Step 1: Testing network connectivity...");
                var networkResult = await TestNetworkConnectivity(testEndpoint);
                Console.WriteLine($"Network Test: {(networkResult ? "✅ PASS" : "❌ FAIL")}");
                Console.WriteLine();

                if (!networkResult)
                {
                    Console.WriteLine("❌ Network connectivity failed. Check:");
                    Console.WriteLine("   - Server is running");
                    Console.WriteLine("   - Correct IP address/hostname");
                    Console.WriteLine("   - Firewall allows connection");
                    Console.WriteLine("   - Port is not blocked");
                    return false;
                }

                // Test 2: Endpoint discovery
                Console.WriteLine("Step 2: Testing endpoint discovery...");
                var discoveryResult = await TestEndpointDiscovery(testEndpoint);
                Console.WriteLine($"Endpoint Discovery: {(discoveryResult ? "✅ PASS" : "❌ FAIL")}");
                Console.WriteLine();

                if (!discoveryResult)
                {
                    Console.WriteLine("❌ Endpoint discovery failed. Check:");
                    Console.WriteLine("   - OPC UA server is running");
                    Console.WriteLine("   - Endpoint URL is correct");
                    Console.WriteLine("   - Server allows anonymous discovery");
                    return false;
                }

                // Test 3: Application configuration
                Console.WriteLine("Step 3: Testing application configuration...");
                var configResult = await TestApplicationConfiguration(config);
                Console.WriteLine($"Application Config: {(configResult ? "✅ PASS" : "❌ FAIL")}");
                Console.WriteLine();

                // Test 4: Session creation
                Console.WriteLine("Step 4: Testing session creation...");
                var sessionResult = await TestSessionCreation(testEndpoint, config);
                Console.WriteLine($"Session Creation: {(sessionResult ? "✅ PASS" : "❌ FAIL")}");
                Console.WriteLine();

                if (sessionResult)
                {
                    Console.WriteLine("✅ All tests passed! Connection should work.");
                    return true;
                }
                else
                {
                    Console.WriteLine("❌ Session creation failed. Check:");
                    Console.WriteLine("   - Authentication credentials");
                    Console.WriteLine("   - Security policy compatibility");
                    Console.WriteLine("   - Certificate issues");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Diagnostic failed with exception: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        private static async Task<bool> TestNetworkConnectivity(string endpointUrl)
        {
            try
            {
                // Extract hostname and port from OPC UA endpoint
                var uri = new Uri(endpointUrl);
                var hostname = uri.Host;
                var port = uri.Port;

                Console.WriteLine($"   Testing ping to {hostname}...");
                
                // Test ping
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(hostname, 5000);
                    Console.WriteLine($"   Ping result: {reply.Status}");
                    
                    if (reply.Status != IPStatus.Success)
                    {
                        Console.WriteLine($"   ❌ Ping failed: {reply.Status}");
                        return false;
                    }
                }

                Console.WriteLine($"   Testing TCP connection to {hostname}:{port}...");
                
                // Test TCP connection
                using (var client = new System.Net.Sockets.TcpClient())
                {
                    var connectTask = client.ConnectAsync(hostname, port);
                    var completedTask = await Task.WhenAny(connectTask, Task.Delay(5000));
                    
                    if (completedTask == connectTask && client.Connected)
                    {
                        Console.WriteLine("   ✅ TCP connection successful");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("   ❌ TCP connection failed or timed out");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Network test exception: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> TestEndpointDiscovery(string endpointUrl)
        {
            try
            {
                Console.WriteLine("   Discovering endpoints...");
                
                var configuration = new ApplicationConfiguration()
                {
                    ApplicationName = "OPC UA Diagnostic Tool",
                    ApplicationType = ApplicationType.Client,
                    SecurityConfiguration = new SecurityConfiguration
                    {
                        AutoAcceptUntrustedCertificates = true
                    },
                    TransportQuotas = new TransportQuotas { OperationTimeout = 15000 }
                };

                await configuration.Validate(ApplicationType.Client);

                var discoveryClient = DiscoveryClient.Create(new Uri(endpointUrl));
                var endpoints = discoveryClient.GetEndpoints(null);
                
                if (endpoints != null && endpoints.Count > 0)
                {
                    Console.WriteLine($"   ✅ Found {endpoints.Count} endpoint(s):");
                    foreach (var endpoint in endpoints)
                    {
                        Console.WriteLine($"      - {endpoint.EndpointUrl} ({endpoint.SecurityPolicyUri})");
                    }
                    return true;
                }
                else
                {
                    Console.WriteLine("   ❌ No endpoints found");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Endpoint discovery exception: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> TestApplicationConfiguration(OpcUaSettings settings)
        {
            try
            {
                Console.WriteLine("   Validating application configuration...");
                
                var configuration = new ApplicationConfiguration()
                {
                    ApplicationName = settings.ApplicationName,
                    ApplicationType = ApplicationType.Client,
                    SecurityConfiguration = new SecurityConfiguration
                    {
                        AutoAcceptUntrustedCertificates = settings.AutoAcceptUntrustedCertificates,
                        ApplicationCertificate = new CertificateIdentifier 
                        { 
                            StoreType = @"Directory", 
                            StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\MachineDefault"
                        }
                    },
                    TransportQuotas = new TransportQuotas { OperationTimeout = settings.OperationTimeout },
                    ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = settings.SessionTimeout }
                };

                await configuration.Validate(ApplicationType.Client);
                Console.WriteLine("   ✅ Application configuration valid");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Configuration validation exception: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> TestSessionCreation(string endpointUrl, OpcUaSettings settings)
        {
            Session session = null;
            try
            {
                Console.WriteLine("   Creating OPC UA session...");
                
                var configuration = new ApplicationConfiguration()
                {
                    ApplicationName = settings.ApplicationName,
                    ApplicationType = ApplicationType.Client,
                    SecurityConfiguration = new SecurityConfiguration
                    {
                        AutoAcceptUntrustedCertificates = settings.AutoAcceptUntrustedCertificates,
                        ApplicationCertificate = new CertificateIdentifier 
                        { 
                            StoreType = @"Directory", 
                            StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\MachineDefault"
                        }
                    },
                    TransportQuotas = new TransportQuotas { OperationTimeout = settings.OperationTimeout },
                    ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = settings.SessionTimeout }
                };

                await configuration.Validate(ApplicationType.Client);
                
                if (configuration.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                {
                    configuration.CertificateValidator.CertificateValidation += (s, e) => 
                    { 
                        e.Accept = (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted); 
                    };
                }

                var endpointDescription = CoreClientUtils.SelectEndpoint(endpointUrl, useSecurity: settings.UseSecurity);
                var endpointConfiguration = EndpointConfiguration.Create(configuration);
                var endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

                UserIdentity userIdentity;
                if (!string.IsNullOrEmpty(settings.Username))
                {
                    userIdentity = new UserIdentity(settings.Username, settings.Password);
                    Console.WriteLine($"   Using username authentication: {settings.Username}");
                }
                else
                {
                    userIdentity = new UserIdentity(new AnonymousIdentityToken());
                    Console.WriteLine("   Using anonymous authentication");
                }

                session = await Session.Create(
                    configuration, 
                    endpoint, 
                    false, 
                    "Diagnostic Session", 
                    (uint)settings.SessionTimeout, 
                    userIdentity, 
                    null);

                if (session != null && session.Connected)
                {
                    Console.WriteLine("   ✅ Session created successfully");
                    Console.WriteLine($"   Session ID: {session.SessionId}");
                    Console.WriteLine($"   Server URI: {session.ConfiguredEndpoint.EndpointUrl}");
                    return true;
                }
                else
                {
                    Console.WriteLine("   ❌ Session creation failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Session creation exception: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
            finally
            {
                if (session != null)
                {
                    try
                    {
                        await session.CloseAsync();
                        session.Dispose();
                        Console.WriteLine("   Session closed successfully");
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
        }

        public static void PrintCommonSolutions()
        {
            Console.WriteLine();
            Console.WriteLine("=== Common Solutions ===");
            Console.WriteLine();
            Console.WriteLine("1. **Kepware Server Not Running**");
            Console.WriteLine("   - Start Kepware Server");
            Console.WriteLine("   - Check Windows Services for 'KEPServerEX'");
            Console.WriteLine();
            Console.WriteLine("2. **Wrong Endpoint URL**");
            Console.WriteLine("   - Default Kepware: opc.tcp://localhost:49320");
            Console.WriteLine("   - Check Kepware OPC UA configuration");
            Console.WriteLine("   - Verify port number in appsettings.json");
            Console.WriteLine();
            Console.WriteLine("3. **Firewall Issues**");
            Console.WriteLine("   - Allow port 49320 in Windows Firewall");
            Console.WriteLine("   - Add FileReader.exe to firewall exceptions");
            Console.WriteLine();
            Console.WriteLine("4. **Certificate Issues**");
            Console.WriteLine("   - Set AutoAcceptUntrustedCertificates: true");
            Console.WriteLine("   - Check OPC UA certificate stores");
            Console.WriteLine();
            Console.WriteLine("5. **Security Policy Mismatch**");
            Console.WriteLine("   - Set UseSecurity: false for testing");
            Console.WriteLine("   - Check Kepware security settings");
            Console.WriteLine();
            Console.WriteLine("6. **Authentication Problems**");
            Console.WriteLine("   - Use anonymous authentication (empty username/password)");
            Console.WriteLine("   - Check Kepware user management settings");
        }
    }
}
