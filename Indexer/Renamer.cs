using System;
using System.IO;

namespace Indexer
{
    public class Renamer
    {
        private void RenameFile(FileInfo file)
        {
            if (file.FullName.EndsWith(".txt")) return; // Skip .txt files
            if (file.Name.StartsWith('.')) return; // Skip hidden files

            // Prepare new file name with .txt extension
            string newFileName = file.FullName + (file.FullName.EndsWith(".") ? "txt" : ".txt");

            try
            {
                File.Move(file.FullName, newFileName, true);
                Console.WriteLine($"Renamed: {file.FullName} to {newFileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error renaming file {file.FullName}: {ex.Message}");
            }
        }

        public void Crawl(DirectoryInfo dir)
        {
            if (!dir.Exists)
            {
                Console.WriteLine($"Directory does not exist: {dir.FullName}");
                return;
            }

            Console.WriteLine("Crawling " + dir.FullName);

            foreach (var file in dir.EnumerateFiles())
            {
                RenameFile(file);
            }

            foreach (var directory in dir.EnumerateDirectories())
            {
                Crawl(directory);
            }
        }
    }
}
