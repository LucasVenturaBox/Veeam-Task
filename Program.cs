using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Timers;

namespace VeeamTask
{
    public class Program
    {
        private static string _sourceFolderPath = "";
        private static string _replicaDirectoryPath = "";
        private static System.Timers.Timer _timer = new System.Timers.Timer(5000); //5 seconds = 5000 miliseconds
        public static void Main(string[] args)
        {
            AskForArguments();
            CreateReplicaDirectory();
            PopulateReplica();
        }

        #region Initial setup
        private static void AskForArguments()
        {
            Console.WriteLine("Please input the path of the folder to be copied, the source.");
            string providedSourcePath = Console.ReadLine();
            if(Path.Exists(providedSourcePath))
            {
                Console.WriteLine("The path is valid.\n");
                _sourceFolderPath = providedSourcePath;
            }
            else
            {
                Console.WriteLine("The path is not valid, we will proceed with the sample folder.\n");
                string workingDirectoryPath = Directory.GetCurrentDirectory();
                string sourceDirectoryPath = Path.Combine(workingDirectoryPath, "Source");
                _sourceFolderPath = sourceDirectoryPath;
            }

            Console.WriteLine("Please input the path of the intended destination folder, the replica.");
            string providedReplicaPath = Console.ReadLine();
            if(!Path.Exists(providedReplicaPath))
            {
                Console.WriteLine("The path is valid.\n");
                _replicaDirectoryPath = providedReplicaPath;
            }
            else
            {
                Console.WriteLine("The path is not valid, we will proceed with the default destination, the desktop.\n");
               string newFolder = "Replica";
                _replicaDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), newFolder);
            }

            Console.WriteLine("Please input the folder synchronization interval, in miliseconds.");
            int providedInterval = Convert.ToInt32(Console.ReadLine());
            if(providedInterval >= 1)
            {
                Console.WriteLine("The interval is valid.\n");
                _timer = new System.Timers.Timer(providedInterval*1000);
            }
            else
            {
                Console.WriteLine("The interval is too small, we will proceed with the default value, 3 seconds.\n");
                
                _timer = new System.Timers.Timer(3000);
            }

            Console.WriteLine($"These are the values:\n Source: {_sourceFolderPath}\n Replica: {_replicaDirectoryPath}\n Sync Interval: {_timer.Interval/1000} seconds\n\nPress enter to continue");
            Console.ReadLine();
        }

        private static void CreateReplicaDirectory()
        {
            if(!Directory.Exists(_replicaDirectoryPath))
            {
                Directory.CreateDirectory(_replicaDirectoryPath);
            }
            else
            {
                Console.WriteLine("Replica folder already exists");
            }
        }

        private static void PopulateReplica()
        {
            if(!string.IsNullOrEmpty(_replicaDirectoryPath) && !string.IsNullOrEmpty(_sourceFolderPath))
            {
                CopyEveryFileInDirectory(_sourceFolderPath, _replicaDirectoryPath);
            }

        }
        #endregion

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
                    Console.WriteLine($"A new file called {replicatedFilePath} was created on {newFile}");

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
                        Console.WriteLine($"A new folder called {replicatedDirectoryPath} was created on {newFolder}");

                        CopyEveryFileInDirectory(directory, newFolder);
                    }
                }
            }
        }

        

    }
}
