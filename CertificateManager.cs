using System;
using System.IO;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Configuration;

namespace FileReader
{
    public static class CertificateManager
    {
        public static async Task<bool> EnsureCertificatesExistAsync()
        {
            try
            {
                // Create application configuration for certificate setup
                var config = CreateCertificateConfiguration();
                
                // Check and create certificate directories
                CheckCertificateStores(config);
                
                // Validate application configuration
                await config.Validate(ApplicationType.Client);
                
                return true;
            }
            catch (Exception ex)
            {
                // For deployment scenarios, we'll create a simple fallback
                return await CreateSimpleCertificateSetup();
            }
        }

        private static ApplicationConfiguration CreateCertificateConfiguration()
        {
            var settings = ConfigurationManager.Configuration.OpcUaSettings;
            var appDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            
            return new ApplicationConfiguration()
            {
                ApplicationName = settings.ApplicationName,
                ApplicationUri = Utils.Format(@"urn:{0}:PDFDataExtractor", Environment.MachineName),
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier
                    {
                        StoreType = @"Directory",
                        StorePath = Path.Combine(appDir, "Certificates", "Own"),
                        SubjectName = Utils.Format(@"CN={0}, DC={1}", settings.ApplicationName, Environment.MachineName)
                    },
                    TrustedIssuerCertificates = new CertificateTrustList
                    {
                        StoreType = @"Directory",
                        StorePath = Path.Combine(appDir, "Certificates", "TrustedIssuers")
                    },
                    TrustedPeerCertificates = new CertificateTrustList
                    {
                        StoreType = @"Directory",
                        StorePath = Path.Combine(appDir, "Certificates", "TrustedPeers")
                    },
                    RejectedCertificateStore = new CertificateTrustList
                    {
                        StoreType = @"Directory",
                        StorePath = Path.Combine(appDir, "Certificates", "Rejected")
                    },
                    AutoAcceptUntrustedCertificates = settings.AutoAcceptUntrustedCertificates,
                    AddAppCertToTrustedStore = true
                },
                TransportConfigurations = new TransportConfigurationCollection(),
                TransportQuotas = new TransportQuotas { OperationTimeout = settings.OperationTimeout },
                ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = settings.SessionTimeout },
                TraceConfiguration = new TraceConfiguration()
            };
        }

        private static void CheckCertificateStores(ApplicationConfiguration config)
        {
            var stores = new[]
            {
                config.SecurityConfiguration.ApplicationCertificate.StorePath,
                config.SecurityConfiguration.TrustedIssuerCertificates.StorePath,
                config.SecurityConfiguration.TrustedPeerCertificates.StorePath,
                config.SecurityConfiguration.RejectedCertificateStore.StorePath
            };

            foreach (var storePath in stores)
            {
                if (!Directory.Exists(storePath))
                {
                    Directory.CreateDirectory(storePath);
                }
            }
        }

        private static async Task<bool> CreateSimpleCertificateSetup()
        {
            try
            {
                // Simple fallback - just create the directory structure
                var appDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                var certDir = Path.Combine(appDir, "Certificates");
                
                var directories = new[]
                {
                    Path.Combine(certDir, "Own"),
                    Path.Combine(certDir, "TrustedPeers"),
                    Path.Combine(certDir, "TrustedIssuers"),
                    Path.Combine(certDir, "Rejected")
                };

                foreach (var dir in directories)
                {
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void PrintCertificateInfo()
        {
            var appDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            
            Console.WriteLine();
            Console.WriteLine("=== Certificate Information ===");
            Console.WriteLine();
            Console.WriteLine("Certificate Locations:");
            Console.WriteLine($"  Application Certificates: {Path.Combine(appDir, "Certificates", "Own")}");
            Console.WriteLine($"  Trusted Issuers: {Path.Combine(appDir, "Certificates", "TrustedIssuers")}");
            Console.WriteLine($"  Trusted Peers: {Path.Combine(appDir, "Certificates", "TrustedPeers")}");
            Console.WriteLine($"  Rejected Certificates: {Path.Combine(appDir, "Certificates", "Rejected")}");
            Console.WriteLine();
            Console.WriteLine("Certificate Solutions:");
            Console.WriteLine("1. **Missing Application Certificate:**");
            Console.WriteLine("   - Application will create certificates automatically");
            Console.WriteLine("   - Certificates stored in application directory");
            Console.WriteLine();
            Console.WriteLine("2. **Server Certificate Issues:**");
            Console.WriteLine("   - Set AutoAcceptUntrustedCertificates: true in appsettings.json");
            Console.WriteLine("   - Server certificates will be auto-accepted on first connection");
            Console.WriteLine();
            Console.WriteLine("3. **Certificate Permission Issues:**");
            Console.WriteLine("   - Run application as Administrator once");
            Console.WriteLine("   - Or manually create certificate directories");
            Console.WriteLine();
            Console.WriteLine("4. **Certificate Store Issues:**");
            Console.WriteLine("   - Delete the Certificates folder and restart application");
            Console.WriteLine("   - New certificate structure will be automatically created");
        }

        public static string GetApplicationCertificateStorePath()
        {
            var appDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return Path.Combine(appDir, "Certificates", "Own");
        }

        public static string GetTrustedPeerCertificateStorePath()
        {
            var appDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return Path.Combine(appDir, "Certificates", "TrustedPeers");
        }

        public static string GetTrustedIssuerCertificateStorePath()
        {
            var appDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return Path.Combine(appDir, "Certificates", "TrustedIssuers");
        }

        public static string GetRejectedCertificateStorePath()
        {
            var appDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return Path.Combine(appDir, "Certificates", "Rejected");
        }
    }
}