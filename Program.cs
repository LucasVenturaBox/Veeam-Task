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
            }
            else
            {
                Console.WriteLine("Replica folder already exists");
            }
        }

        private static void InitializeReplica()
        {
            if(!string.IsNullOrEmpty(replicaDirectoryPath))
            {
                string workingDirectoryPath = Directory.GetCurrentDirectory();
                string sourceDirectoryPath = Path.Combine(workingDirectoryPath, "Source");

                CopyEveryFileInDirectory(sourceDirectoryPath, replicaDirectoryPath);
                
            }

        }

        private static void CopyEveryFileInDirectory(string sourceDirectory, string destinationPath)
        {
            string[] allSourceFilePaths = Directory.GetFiles(sourceDirectory);
            string[] allSourceDirectoryPaths = Directory.GetDirectories(sourceDirectory);
            foreach (string filePath in allSourceFilePaths)
            {
                string replicatedFilePath = Path.GetFileName(filePath);
                string newFile = Path.Combine(destinationPath, replicatedFilePath);
                
                if (!File.Exists(newFile))
                {
                    File.Copy(filePath, newFile);
                }
            }

            if (allSourceDirectoryPaths.Length > 0)
            {
                foreach (string directory in allSourceDirectoryPaths)
                {
                    string replicatedDirectoryPath = Path.GetFileName(directory);
                    string newFolder = Path.Combine(destinationPath, replicatedDirectoryPath);
                    if (!File.Exists(newFolder))
                    {
                        Directory.CreateDirectory(newFolder);
                        CopyEveryFileInDirectory(directory, newFolder);
                    }
                }
            }
        }

    }
}
