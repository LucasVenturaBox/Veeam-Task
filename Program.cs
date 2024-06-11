using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Timers;

namespace VeeamTask
{
    public class Program
    {
        private static string _sourceFolderPath = default!;
        private static string _replicaFolderPath = default!;
        private static string _logFilePath = default!;
        private static System.Timers.Timer _timer = default!;
        private static List<string> _logLines = new List<string>();
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
            //Source Path
            Console.WriteLine("Please input the path of the folder to be copied, the source.");
            string providedSourcePath = Console.ReadLine()?.ToString() ?? "Invalid string";
            if(Path.Exists(providedSourcePath))
            {
                _sourceFolderPath = providedSourcePath;
                Console.WriteLine($"The path {_sourceFolderPath} is valid.\n");
            }
            else
            {
                string workingDirectoryPath = Directory.GetCurrentDirectory();
                string sourceDirectoryPath = Path.Combine(workingDirectoryPath, "Source");
                _sourceFolderPath = sourceDirectoryPath;
                Console.WriteLine($"The path is NOT valid, we will proceed with the sample folder on {_sourceFolderPath}.\n");
            }

            //Replica Path
            Console.WriteLine("Please input the path of the intended destination folder, the replica.");
            string providedReplicaPath = Console.ReadLine()?.ToString() ?? "Invalid string";
            if(providedReplicaPath == "Invalid string" || providedReplicaPath == _sourceFolderPath)
            {
                _replicaFolderPath =Path.Combine(Directory.GetCurrentDirectory(), "Replica");
                Console.WriteLine($"The path is NOT valid, replica will be created on {_replicaFolderPath}.\n");
            }
            else
            {
                _replicaFolderPath = providedReplicaPath;
                Console.WriteLine($"The path {_replicaFolderPath} is valid.\n");
            }

            //Log File
            Console.WriteLine("Please input the path of the Log File");
            string providedLogPath = Console.ReadLine()?.ToString() ?? "Invalid string";
            if(providedLogPath == "Invalid string" || providedLogPath == _sourceFolderPath || providedLogPath == _replicaFolderPath)
            {
                _logFilePath =Path.Combine(Directory.GetCurrentDirectory(), "Log.txt");
                Console.WriteLine($"The path is NOT valid, Log File will be created on {_logFilePath}.\n");
            }
            else
            {
                _logFilePath = providedLogPath + ".txt";
                Console.WriteLine($"The path {_logFilePath} is valid.\n");
            }

            //Sync Interval
            Console.WriteLine("Please input the folder synchronization interval, in seconds.");
            string providedInterval = Console.ReadLine()?.ToString() ?? "Invalid string";
            int intervalInSeconds;
            if(int.TryParse(providedInterval,out intervalInSeconds) && intervalInSeconds >= 1)
            {
                _timer = new System.Timers.Timer((double)intervalInSeconds*1000);
                Console.WriteLine($"The interval{_timer.Interval/1000} is valid.\n");
            }
            else
            {
                Console.WriteLine("The interval is too small, we will proceed with the default value, 3 seconds.\n");
                
                _timer = new System.Timers.Timer(3000);
            }

            string setupValuesMessage = $"These are the values:\n Source: {_sourceFolderPath}\n Replica: {_replicaFolderPath}\n LogFile: {_logFilePath}\n Sync Interval: {_timer.Interval/1000} seconds\n";
           
            AddLinesToLog(_logFilePath, setupValuesMessage);
            Console.WriteLine(setupValuesMessage + "\n Press enter to continue");
            

            Console.ReadLine();
        }

        private static void CreateReplicaDirectory()
        {
            Directory.CreateDirectory(_replicaFolderPath);
            string replicaCreatedMessage = $"Replica folder has been created on {_replicaFolderPath}";
            AddLinesToLog(_logFilePath, replicaCreatedMessage);
            Console.WriteLine(replicaCreatedMessage);
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
                    
                    string fileCreatedMessage = $"The file {originalFilePath} was copied to {destinationFilePath}";

                    AddLinesToLog(_logFilePath, fileCreatedMessage);
                    Console.WriteLine(fileCreatedMessage);

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

                    string fileDeletedMessage =$"A file called {filePath} was deleted from {destinationFilePath}";

                    AddLinesToLog(_logFilePath, fileDeletedMessage);
                    Console.WriteLine(fileDeletedMessage);

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
                string destinationFolderPath = Path.Combine(destinationPath, directory);
                
                if (!Directory.Exists(destinationFolderPath))
                {
                    Directory.CreateDirectory(destinationFolderPath);

                    string createdFolderMessage = $"The folder {originalFolderPath} was copied to {destinationFolderPath}";

                    AddLinesToLog(_logFilePath, createdFolderMessage);
                    Console.WriteLine(createdFolderMessage);

                    CopyEveryFileInDirectory(originalFolderPath, destinationFolderPath);
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

                    string deletedFolderMessage = $"A folder called {directory} was deleted from {destinationFilePath}";

                    AddLinesToLog(_logFilePath, deletedFolderMessage);
                    Console.WriteLine(deletedFolderMessage);
                }
            }
        }

        #endregion

        private static void UpdateReplicaFolder(Object? source, ElapsedEventArgs e)
        {
            SyncFolders(_sourceFolderPath, _replicaFolderPath);
            CycleThroughEverySourceFile(_sourceFolderPath,_replicaFolderPath);
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

       
        private static void CycleThroughEverySourceFile(string sourcePath, string replicaPath)
        {
            List<string> allSourceFilesNames = GetFileNames(Directory.GetFiles(sourcePath));
            List<string> allSourceFoldersNames = GetFileNames(Directory.GetDirectories(sourcePath));

            foreach (string fileName in allSourceFilesNames)
            {
                string originalFilePath = Path.Combine(sourcePath, fileName);
                string destinationFilePath = Path.Combine(replicaPath, fileName);
                
                if (File.Exists(originalFilePath) && File.Exists(destinationFilePath))
                {
                    if(WasFileModified(originalFilePath, destinationFilePath))
                    {
                        File.Delete(destinationFilePath);
                        File.Copy(originalFilePath, destinationFilePath);

                        string fileUpdatedMessage = $"The file :{destinationFilePath} has just been updated, to match the source file {originalFilePath}";

                        AddLinesToLog(_logFilePath, fileUpdatedMessage);
                        Console.WriteLine(fileUpdatedMessage);
                    }
                }
            } 

            foreach (string folderName in allSourceFoldersNames)
            {
                string originalFolderPath = Path.Combine(sourcePath, folderName);
                string destinationFilePath = Path.Combine(replicaPath, folderName);
                
                if (Directory.Exists(originalFolderPath) && Directory.Exists(destinationFilePath))
                {
                    CycleThroughEverySourceFile(originalFolderPath, destinationFilePath);
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

        private static bool WasFileModified(string sourceFilePath, string replicatedFilePath)
        {
            MD5 md5Instance = MD5.Create();

            byte[] sourceBuffer = File.ReadAllBytes(sourceFilePath);
            string sourceHash = BitConverter.ToString(md5Instance.ComputeHash(sourceBuffer));

            byte[] replicatedBuffer = File.ReadAllBytes(replicatedFilePath);
            string replicatedHash = BitConverter.ToString(md5Instance.ComputeHash(replicatedBuffer));

            
            if(sourceHash != replicatedHash)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        private static void AddLinesToLog(string logFile, string line)
        {
            _logLines.Add(line);

            if (logFile != null)
            {
                foreach (string missingLine in _logLines)
                {
                    string currentMinutes = DateTime.Now.TimeOfDay.Minutes.ToString();
                    string currentSeconds = DateTime.Now.TimeOfDay.Seconds.ToString();
                    string message = "["+currentMinutes +","+ currentSeconds +"] " + missingLine + "\n";
                   
                   File.AppendAllText(logFile, message);
                }

                _logLines.Clear();
            }
        }

    }
}
