using System;
using System.Threading.Tasks;
using FileReader.Services;

namespace FileReader
{
    /// <summary>
    /// Main program entry point
    /// Follows Clean Architecture principles with proper dependency injection
    /// </summary>
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                // Load configuration
                ConfigurationManager.LoadConfiguration();
                var configuration = ConfigurationManager.Configuration;
                
                // Create service container
                var serviceContainer = new ServiceContainer(configuration);
                var orchestrator = serviceContainer.CreateApplicationOrchestrator();

                // Route to appropriate workflow based on command line arguments
                if (args.Length > 0)
                {
                    switch (args[0].ToLower())
                    {
                        case "--diagnostic":
                            RunDiagnosticMode(orchestrator).GetAwaiter().GetResult();
                            return;
                        case "--setup":
                            RunSetupMode(orchestrator).GetAwaiter().GetResult();
                            return;
                        default:
                            Console.WriteLine($"Unknown argument: {args[0]}");
                            Console.WriteLine("Valid arguments: --diagnostic, --setup");
                            return;
                    }
                }

                // Run main workflow
                RunMainWorkflow(orchestrator).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Application failed: {ex.Message}");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }

        static async Task RunMainWorkflow(ApplicationOrchestrator orchestrator)
        {
            try
            {
                // First-time setup check
                await EnsureFirstTimeSetup();

                // Execute main workflow
                var success = await orchestrator.ExecuteMainWorkflowAsync();
                
                if (!success)
                {
                    Console.WriteLine("\n⚠️ Workflow completed with errors. Check the logs above for details.");
                }

                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Main workflow failed: {ex.Message}");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }

        static async Task EnsureFirstTimeSetup()
        {
            try
            {
                await CertificateManager.EnsureCertificatesExistAsync();
            }
            catch
            {
                // Silent setup - certificates are optional for basic operation
            }
        }

        static async Task RunDiagnosticMode(ApplicationOrchestrator orchestrator)
        {
            try
            {
                var success = await orchestrator.ExecuteDiagnosticWorkflowAsync();
                
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Diagnostic mode failed: {ex.Message}");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }

        static async Task RunSetupMode(ApplicationOrchestrator orchestrator)
        {
            try
            {
                var success = await orchestrator.ExecuteSetupWorkflowAsync();
                
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Setup mode failed: {ex.Message}");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }

        // All CSV and OPC UA functionality has been moved to dedicated services
        // This keeps Program.cs focused only on application entry point and workflow coordination
    }
}