using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkTestConsol
{
    class Program
    {
        static void Main(string[] args)
        {
            GetNancyUriViaHttpClient();
        }

        private static void GetVersionString()
        {
            var versionFilepath = Path.Combine("..", "..", "version.txt");
            // Alle Zeilen aus der version.txt lesen
            var lines = File.ReadAllLines(versionFilepath);
            // Die Zeile für den master raussuchen, um nicht eventuell den falschen Pfad zu nehmen
            var master = lines.FirstOrDefault(i => i.StartsWith("master"));
            // Feststellen an welcher Stelle der Pfad der .exe anfängt
            var startIndex = master.IndexOf('=');
            // Parameter herausfiltern, falls welche vorhanden sind
            var endIndex = master.IndexOf(" -");

            string executable;
            // Den Pfad der .exe extrahieren
            if (endIndex == -1) executable = master.Substring(startIndex + 1).Trim();
            else executable = master.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();
            // Den Ordnernamen der Version herausnehmen
            var versionDirectory = Path.GetDirectoryName(executable).Split(Path.DirectorySeparatorChar).LastOrDefault();
            // In den folgenden drei Zeilen wird der Ordnername auseinandergenommen und anschließend passend für die Registry wieder zusammen gesetzt
            var firstSplitIndex = versionDirectory.IndexOf('-');

            var secondSplitIndex = versionDirectory.IndexOf('.');
            // den Versionstext wieder zusammensetzen und zurückgeben
            var versionString = $"{versionDirectory.Substring(secondSplitIndex + 1)}-{versionDirectory.Substring(firstSplitIndex + 1, secondSplitIndex - firstSplitIndex - 1)}";

            Console.WriteLine("VersionString:");
            Console.WriteLine(versionString);
            Console.ReadLine();
        }

        private static void GetNancyUriViaHttpClient()
        {
            Console.WriteLine("RegistryUri:");
            var registryUri = Console.ReadLine();

            Console.WriteLine("Version:");
            var version = Console.ReadLine();

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = httpClient.GetAsync(registryUri.TrimEnd('/') + $"/Nancy/{version}");

                    response.Wait();

                    var result = response.Result;

                    var resultContent = result.Content.ReadAsStringAsync().Result;

                    if (result.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new Exception("Fehler beim Synchronisieren des anderen Mandanten:\nDas abrufen der Serveradresse ist fehlgeschlage:\n\n" + resultContent);
                    }

                    var nancyUri = resultContent;

                    Console.WriteLine("NancyUri:");
                    Console.WriteLine(nancyUri);
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }
    }
}