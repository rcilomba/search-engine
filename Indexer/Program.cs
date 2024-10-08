using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Diagnostics;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace Indexer
{
    class Program
    {
        private static readonly ActivitySource ActivitySource = new ActivitySource("Indexer");

        static void Main(string[] args)
        {
            // Set up OpenTelemetry
            using var tracerProvider = ConfigureOpenTelemetry();

            try
            {
                // Decompress the file with tracing
                using (var activity = ActivitySource.StartActivity("DecompressingFile"))
                {
                    DecompressGzipFile("enron/mikro.tar.gz", "mails.tar");
                }

                if (Directory.Exists("maildir"))
                    Directory.Delete("maildir", true);

                // Extract the mails
                ExtractTar("mails.tar", ".");

                // Start crawling and renaming
                new Renamer().Crawl(new DirectoryInfo("maildir"));

                // Run the application
                new App().Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                // You may want to log the exception or handle it accordingly
            }
            finally
            {
                // Dispose of tracer when done
                tracerProvider?.Dispose();
            }
        }

        static TracerProvider ConfigureOpenTelemetry()
        {
            return Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Indexer"))
                .AddSource("Indexer") // Add a source for this service
                .AddHttpClientInstrumentation() // Trace outgoing HTTP requests
                .AddZipkinExporter(options =>
                {
                    options.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
                })
                .Build();
        }

        static void DecompressGzipFile(string compressedFilePath, string decompressedFilePath)
        {
            using (FileStream compressedFileStream = File.OpenRead(compressedFilePath))
            using (FileStream decompressedFileStream = File.Create(decompressedFilePath))
            using (GZipStream gzipStream = new GZipStream(compressedFileStream, CompressionMode.Decompress))
            {
                gzipStream.CopyTo(decompressedFileStream);
            }
        }

        static void ExtractTar(string tarFilePath, string outputDirectory)
        {
            using (var archive = ArchiveFactory.Open(tarFilePath))
            {
                foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                {
                    entry.WriteToDirectory(outputDirectory, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }
        }
    }
}
