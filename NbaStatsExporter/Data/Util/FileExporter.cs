using System.IO;
using Newtonsoft.Json;

namespace NbaStatsExporter.Data.ExporterUtil
{
    public class FileExporter
    {
        public static void WriteToJsonFile<T>(string fileName, T data, string outputDirectory = "output")
        {
            var actualFileName = $"{fileName}.json";
            try
            {
                Console.WriteLine($"Saving data to file: " + Path.Combine(outputDirectory, actualFileName));

                string filePath = Path.Combine(outputDirectory, actualFileName);

                if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);

                File.WriteAllText(filePath, JsonConvert.SerializeObject(data, Formatting.Indented));

                Console.WriteLine($"Data successfully written to {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public static void WriteToFile(string fileName, string data, string outputDirectory = "output")
        {
            var actualFileName = $"{fileName}.json";
            try
            {
                Console.WriteLine($"Saving data to file: " + Path.Combine(outputDirectory, actualFileName));

                string filePath = Path.Combine(outputDirectory, actualFileName);

                if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);

                File.WriteAllText(filePath, data);

                Console.WriteLine($"Data successfully written to {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public static T Read<T>(string directoryPath, string fileName)
        {
            try
            {
                string fullPath = Path.GetFullPath(Path.Combine(directoryPath, fileName + ".json"));
                if (File.Exists(fullPath))
                {
                    string jsonContent = File.ReadAllText(fullPath);
                    T data = JsonConvert.DeserializeObject<T>(jsonContent);

                    return data;
                }
                else throw new Exception("File: " + fullPath + " does not exist");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading or deserializing the JSON file: " + ex.Message);
                return default(T);
            }
        }

        public static string ReadString(string directoryPath, string fileName)
        {
            try
            {
                string fullPath = Path.GetFullPath(Path.Combine(directoryPath, fileName + ".json"));
                if (File.Exists(fullPath))
                {
                    return File.ReadAllText(fullPath);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading or deserializing the JSON file: " + ex.Message);
                return null;
            }
        }

        public static void CopyFolder(string sourceFolderPath, string destFolderPath, int currentDepth = 0, int maxDepth = 5)
        {
            if (!Directory.Exists(sourceFolderPath))
            {
                Console.WriteLine($"Source folder does not exist: {sourceFolderPath}");
                return;
            }

            if (currentDepth > maxDepth)
            {
                Console.WriteLine($"Maximum depth reached at: {sourceFolderPath}");
                return;
            }

            Directory.CreateDirectory(destFolderPath);

            foreach (var file in Directory.GetFiles(sourceFolderPath))
            {
                string destFile = Path.Combine(destFolderPath, Path.GetFileName(file));
                File.Copy(file, destFile, true); // Overwrite = true
            }

            foreach (var subdir in Directory.GetDirectories(sourceFolderPath))
            {
                string destSubdir = Path.Combine(destFolderPath, Path.GetFileName(subdir));
                CopyFolder(subdir, destSubdir, currentDepth + 1, maxDepth); // Increase depth
            }

            if (currentDepth == 0)
            {
                Console.WriteLine($"Folder copied from {sourceFolderPath} to {destFolderPath} up to depth {maxDepth}");
            }
        }
    }
}
