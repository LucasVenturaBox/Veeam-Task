using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace VeeamTask
{
    public class Program
    {
        private static string replicaDirectoryPath = "something";
        public static void Main(string[] args)
        {
            CreateReplicaDirectory();
            InitializeReplica();
        }

        private static void CreateReplicaDirectory()
        {
            string newFolder = "Replica";
            replicaDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), newFolder);
            if(!Directory.Exists(replicaDirectoryPath))
            {
                Directory.CreateDirectory(replicaDirectoryPath);
                Console.WriteLine("Hey, I just created and empty Replica File");
            }
            else
            {
                Console.WriteLine("I didn't create the replica file, because it already existed!");
            }
        }

        private static void InitializeReplica()
        {
            if(!string.IsNullOrEmpty(replicaDirectoryPath))
            {
                string workingDirectoryPath = Directory.GetCurrentDirectory();
                string sourceDirectoryPath = Path.Combine(workingDirectoryPath, "Source");
                Console.WriteLine($"The Source Directory Path is {sourceDirectoryPath}");

                string[] allSourceFilePaths = Directory.GetFiles(sourceDirectoryPath);
                string[] allSourceDirectoryPaths = Directory.GetDirectories(sourceDirectoryPath);
                foreach (string filePath in allSourceFilePaths)
                {
                    string replicatedFilePath = Path.GetFileName(filePath);
                    File.Copy(filePath, Path.Combine(replicaDirectoryPath, replicatedFilePath));
                }

                // Doesn't work for directories, only single files
                // foreach (string directoryPath in allSourceDirectoryPaths)
                // {
                //     string replicatedDirectoryPath = Path.GetFileName(directoryPath);
                //     File.Copy(directoryPath, Path.Combine(replicaDirectoryPath, replicatedDirectoryPath));
                // }
                
                Console.WriteLine("Replica File is initialized");
            }

        }

    }
}
