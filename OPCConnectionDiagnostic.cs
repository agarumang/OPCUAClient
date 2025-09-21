using System;
using System.Threading.Tasks;

namespace FileReader
{
    public static class OPCConnectionDiagnostic
    {
        public static async Task<bool> DiagnoseConnectionAsync()
        {
            Console.WriteLine("=== OPC UA Connection Test ===");
            Console.WriteLine();

            try
            {
                var config = ConfigurationManager.Configuration.OpcUaSettings;
                Console.WriteLine($"Testing connection to: {config.EndpointUrl}");
                Console.WriteLine();

                // Simple connection test using the actual OPC UA client
                var opcClient = new OPCUAClient();
                var connected = await opcClient.ConnectAsync();

                if (connected)
                {
                    Console.WriteLine("✅ Connection successful!");
                    
                    // Test basic operations
                    Console.WriteLine("Testing browse operation...");
                    opcClient.BrowseRootFolder();
                    
                    await opcClient.DisconnectAsync();
                    Console.WriteLine("✅ All tests passed!");
                    return true;
                }
                else
                {
                    Console.WriteLine("❌ Connection failed!");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Test failed: {ex.Message}");
                return false;
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
            Console.WriteLine("   - Default Kepware: opc.tcp://127.0.0.1:49320");
            Console.WriteLine("   - Check Kepware OPC UA configuration");
            Console.WriteLine();
            Console.WriteLine("3. **Firewall Issues**");
            Console.WriteLine("   - Allow port 49320 in Windows Firewall");
            Console.WriteLine("   - Add FileReader.exe to firewall exceptions");
            Console.WriteLine();
            Console.WriteLine("4. **Certificate Issues**");
            Console.WriteLine("   - Set AutoAcceptUntrustedCertificates: true in appsettings.json");
            Console.WriteLine();
        }
    }
}