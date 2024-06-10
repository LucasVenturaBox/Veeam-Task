using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Timers;

namespace VeeamTask
{
    public class Program
    {
        private static string _sourceFolderPath = "";
        private static string _replicaFolderPath = "";
        private static System.Timers.Timer _timer;
        public static void Main(string[] args)
        {
            AskForArguments();
            CreateReplicaDirectory();
            PopulateReplica();

            _timer.Elapsed += UpdateReplicaFolder;
            _timer.Enabled = true;

            Console.WriteLine("Press enter to exit the program");
            Console.ReadLine();
            _timer.Stop();
            _timer.Dispose();

        }

        #region Initial Setup
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
                _replicaFolderPath = providedReplicaPath;
            }
            else
            {
                Console.WriteLine("The path is not valid, we will proceed with the default destination, the desktop.\n");
               string newFolder = "Replica";
                _replicaFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), newFolder);
            }

            Console.WriteLine("Please input the folder synchronization interval, in seconds.");
            int? providedInterval = Convert.ToInt32(Console.ReadLine());
            if(providedInterval >= 1)
            {
                Console.WriteLine("The interval is valid.\n");
                _timer = new System.Timers.Timer((double)providedInterval*1000);
            }
            else
            {
                Console.WriteLine("The interval is too small, we will proceed with the default value, 3 seconds.\n");
                
                _timer = new System.Timers.Timer(3000);
            }

            Console.WriteLine($"These are the values:\n Source: {_sourceFolderPath}\n Replica: {_replicaFolderPath}\n Sync Interval: {_timer.Interval/1000} seconds\n\nPress enter to continue");
            Console.ReadLine();
        }

        private static void CreateReplicaDirectory()
        {
            if(!Directory.Exists(_replicaFolderPath))
            {
                Directory.CreateDirectory(_replicaFolderPath);
            }
            else
            {
                Console.WriteLine("Replica folder already exists");
            }
        }

        private static void PopulateReplica()
        {
            if(!string.IsNullOrEmpty(_replicaFolderPath) && !string.IsNullOrEmpty(_sourceFolderPath))
            {
                CopyEveryFileInDirectory(_sourceFolderPath, _replicaFolderPath);
            }

        }
        #endregion

        #region File Utils
        private static void CopyEveryFileInDirectory(string sourceDirectory, string destinationPath)
        {
            if (Path.Exists(sourceDirectory))
            {
                List<string> allSourceFilePaths = GetFileNames(Directory.GetFiles(sourceDirectory));
                List<string> allSourceFolderPaths = GetFileNames(Directory.GetDirectories(sourceDirectory));

                CopyFilesFromSourceToDestination(sourceDirectory, destinationPath, allSourceFilePaths);

                CopyFoldersFromSourceToDestination(sourceDirectory, destinationPath, allSourceFolderPaths);

            }
        }

        private static void CopyFilesFromSourceToDestination(string sourcePath, string destinationPath, List<string> allSourceFileNames)
        {
            foreach (string filePath in allSourceFileNames)
            {
                string originalFilePath = Path.Combine(sourcePath, filePath);
                string destinationFilePath = Path.Combine(destinationPath, filePath);

                if (!File.Exists(destinationFilePath))
                {
                    File.Copy(originalFilePath, destinationFilePath);
                    Console.WriteLine($"A new file called {filePath} was copied to {destinationFilePath}");

                }
            }
        }

        private static void DeleteFilesFromDestination(string destinationPath, List<string> allDestinationFileNames)
        {
            foreach (string filePath in allDestinationFileNames)
            {
                string destinationFilePath = Path.Combine(destinationPath, filePath);

                if (File.Exists(destinationFilePath))
                {
                    File.Delete(destinationFilePath);
                    Console.WriteLine($"A new file called {filePath} was deleted from {destinationFilePath}");

                }
            }
        }

        #endregion


        #region Folder Utils
        private static void CopyFoldersFromSourceToDestination(string sourcePath, string destinationPath, List<string> allSourceFolderNames)
        {
            foreach (string directory in allSourceFolderNames)
            {
                string originalFolderPath = Path.Combine(sourcePath, directory);
                string destinationFilePath = Path.Combine(destinationPath, directory);
                
                if (!Directory.Exists(destinationFilePath))
                {
                    Directory.CreateDirectory(destinationFilePath);
                    Console.WriteLine($"A new folder called {directory} was copied to {destinationFilePath}");

                    CopyEveryFileInDirectory(originalFolderPath, destinationFilePath);
                }
            }
        }

        private static void DeleteFoldersFromDestination( string destinationPath, List<string> allDestinationFolderNames)
        {
            foreach (string directory in allDestinationFolderNames)
            {
                string destinationFilePath = Path.Combine(destinationPath, directory);
                if (Directory.Exists(destinationFilePath))
                {
                    Directory.Delete(destinationFilePath, true);
                    Console.WriteLine($"A new folder called {directory} was deleted from {destinationFilePath}");
                }
            }
        }

        #endregion

        private static void UpdateReplicaFolder(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine($"It updates every {_timer.Interval / 1000} seconds");
            //if the replica folder is different from the source, add the missing folders and/or remove an additional folder
            SyncFolders(_sourceFolderPath, _replicaFolderPath);

        }

        private static void SyncFolders(string sourcePath, string folderToSyncPath)
        {
            if (Path.Exists(sourcePath))
            {
                //Source files and directiories paths
                
                List<string> allSourceFileNames = GetFileNames(Directory.GetFiles(sourcePath));
                List<string> allSourceDirectoryNames = GetFileNames(Directory.GetDirectories(sourcePath));
                
                //Folder to sync files and directiories paths
                List<string> allFolderToSyncFileNames = GetFileNames(Directory.GetFiles(folderToSyncPath));
                List<string> allFolderToSyncDirectoryNames = GetFileNames(Directory.GetDirectories(folderToSyncPath));

                //Files
                List<string> fileNamesToAdd = allSourceFileNames.Except(allFolderToSyncFileNames).ToList();
                List<string> fileNamesToRemove = allFolderToSyncFileNames.Except(allSourceFileNames).ToList();

                //Folders
                List<string> folderNamesToAdd = allSourceDirectoryNames.Except(allFolderToSyncDirectoryNames).ToList();
                List<string> folderNamesToRemove = allFolderToSyncDirectoryNames.Except(allSourceDirectoryNames).ToList();

                if(fileNamesToAdd.Any())
                {
                    CopyFilesFromSourceToDestination(sourcePath,folderToSyncPath, fileNamesToAdd);
                }

                if(fileNamesToRemove.Any())
                {
                    DeleteFilesFromDestination(folderToSyncPath, fileNamesToRemove);
                }

                if(folderNamesToAdd.Any())
                {
                    CopyFoldersFromSourceToDestination(sourcePath,folderToSyncPath, folderNamesToAdd);
                }

                if(folderNamesToRemove.Any())
                {
                    DeleteFoldersFromDestination(folderToSyncPath,folderNamesToRemove);
                }

                if(allSourceDirectoryNames.Any())
                {
                    foreach (var sourceDirectoryName in allSourceDirectoryNames)
                    {
                        string sourceDirectoryPath = Path.Combine(sourcePath, sourceDirectoryName);
                        string folderToSyncInDirectoryPath = Path.Combine(folderToSyncPath, sourceDirectoryName);
                        SyncFolders(sourceDirectoryPath,folderToSyncInDirectoryPath);
                    }
                }
                
            }
        }

        private static List<string> GetFileNames(string[] filePaths)
        {
            List<string> fileNames = new List<string>();
            foreach(string file in filePaths)
            {
                fileNames.Add(Path.GetFileName(file));
            }

            return fileNames;
        }

    }
}
