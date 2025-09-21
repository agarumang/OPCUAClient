using System;
using System.IO;
using System.Threading.Tasks;

namespace FileReader
{
    public static class CertificateManager
    {
        public static Task<bool> EnsureCertificatesExistAsync()
        {
            try
            {
                // Create basic certificate directory structure
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

                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public static void PrintCertificateInfo()
        {
            var appDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            
            Console.WriteLine();
            Console.WriteLine("=== Certificate Information ===");
            Console.WriteLine($"Certificate Directory: {Path.Combine(appDir, "Certificates")}");
            Console.WriteLine();
            Console.WriteLine("Certificate Solutions:");
            Console.WriteLine("1. Set AutoAcceptUntrustedCertificates: true in appsettings.json");
            Console.WriteLine("2. Run application as Administrator if permission issues occur");
            Console.WriteLine("3. Delete Certificates folder to reset if needed");
            Console.WriteLine();
        }
    }
}