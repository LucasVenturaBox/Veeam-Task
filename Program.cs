using System.Timers;

namespace VeeamTask
{
    public class Program
    {
        private static string _sourceFolderPath = default!;
        private static string _replicaFolderPath = default!;
        private static Log _logFile = default!;
        private static System.Timers.Timer _timer = default!;
        public static void Main(string[] args)
        {
            AskForArguments();
            CreateReplicaDirectory();
            PopulateReplica();

            _timer.Elapsed += UpdateReplicaFolder;
            _timer.Enabled = true;

            Console.WriteLine("Press enter to exit the program");
            Console.ReadLine();
            _logFile?.AddLinesToLog("Exited the program!");
            _timer.Stop();
            _timer.Dispose();

        }

        #region Initiallization Setup
        private static void AskForArguments()
        {
            //Source Path
            ManageProvidedSourcePath();

            //Replica Path
            ManageProvidedReplicaPath();

            //Log File
            ManageProvidedLogPath();

            //Sync Interval
            ManageProvideInterval();

            string setupValuesMessage = $"These are the values:\n Source: {_sourceFolderPath}\n Replica: {_replicaFolderPath}\n LogFile: {_logFile}\n Sync Interval: {_timer.Interval / 1000} seconds\n";

            FileUtils.WriteToLogAndConsole(setupValuesMessage, _logFile);

            Console.WriteLine("\n Press enter to continue");
            Console.ReadLine();
        }

        #region User Inputs
        private static void ManageProvidedSourcePath()
        {
            Console.WriteLine("Please input the path of the folder to be copied, the source.");
            string providedSourcePath = Console.ReadLine()?.ToString() ?? "Invalid string";

            if (Path.Exists(providedSourcePath))
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
        }
        
        private static void ManageProvidedReplicaPath()
        {
             Console.WriteLine("Please input the path of the intended destination folder, the replica.");
            string providedReplicaPath = Console.ReadLine()?.ToString() ?? "Invalid string";

            if (providedReplicaPath == "Invalid string" || providedReplicaPath == _sourceFolderPath)
            {
                _replicaFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Replica");
                Console.WriteLine($"The path is NOT valid, replica will be created on {_replicaFolderPath}.\n");
            }
            else
            {
                _replicaFolderPath = providedReplicaPath;
                Console.WriteLine($"The path {_replicaFolderPath} is valid.\n");
            }
        }

        private static void ManageProvidedLogPath()
        {
            Console.WriteLine("Please input the path of the Log File");
            string providedLogPath = Console.ReadLine()?.ToString() ?? "Invalid string";

            if (providedLogPath == "Invalid string" || providedLogPath == _sourceFolderPath || providedLogPath == _replicaFolderPath)
            {
                string logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Log.txt");
                _logFile = new Log(logFilePath);

                Console.WriteLine($"The path is NOT valid, Log File will be created on {logFilePath}.\n");
            }
            else
            {
                string logFilePath = providedLogPath + ".txt";
                _logFile = new Log(logFilePath);
                Console.WriteLine($"The path {logFilePath} is valid.\n");
            }
        }

        private static void ManageProvideInterval()
        {
            Console.WriteLine("Please input the folder synchronization interval, in seconds.");
            string providedInterval = Console.ReadLine()?.ToString() ?? "Invalid string";

            int intervalInSeconds;
            if (int.TryParse(providedInterval, out intervalInSeconds) && intervalInSeconds >= 1)
            {
                _timer = new System.Timers.Timer((double)intervalInSeconds * 1000);
                Console.WriteLine($"The interval{_timer.Interval / 1000} is valid.\n");
            }
            else
            {
                Console.WriteLine("The interval is too small, we will proceed with the default value, 3 seconds.\n");

                _timer = new System.Timers.Timer(3000);
            }
        }

        #endregion

        private static void CreateReplicaDirectory()
        {
            Directory.CreateDirectory(_replicaFolderPath);
            string replicaCreatedMessage = $"Replica folder has been created on {_replicaFolderPath}";
            FileUtils.WriteToLogAndConsole(replicaCreatedMessage, _logFile);
        }

        private static void PopulateReplica()
        {
            if(!string.IsNullOrEmpty(_replicaFolderPath) && !string.IsNullOrEmpty(_sourceFolderPath))
            {
                FileUtils.CopyEveryFileInDirectory(_sourceFolderPath, _replicaFolderPath,_logFile);
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
                
                List<string> allSourceFileNames = FileUtils.GetFileNames(Directory.GetFiles(sourcePath));
                List<string> allSourceDirectoryNames = FileUtils.GetFileNames(Directory.GetDirectories(sourcePath));
                
                //Folder to sync files and directiories paths
                List<string> allFolderToSyncFileNames = FileUtils.GetFileNames(Directory.GetFiles(folderToSyncPath));
                List<string> allFolderToSyncDirectoryNames = FileUtils.GetFileNames(Directory.GetDirectories(folderToSyncPath));

                //Files
                List<string> fileNamesToAdd = allSourceFileNames.Except(allFolderToSyncFileNames).ToList();
                List<string> fileNamesToRemove = allFolderToSyncFileNames.Except(allSourceFileNames).ToList();

                //Folders
                List<string> folderNamesToAdd = allSourceDirectoryNames.Except(allFolderToSyncDirectoryNames).ToList();
                List<string> folderNamesToRemove = allFolderToSyncDirectoryNames.Except(allSourceDirectoryNames).ToList();

                if(fileNamesToAdd.Any())
                {
                    FileUtils.CopyFilesFromSourceToDestination(sourcePath,folderToSyncPath, fileNamesToAdd, _logFile);
                }

                if(fileNamesToRemove.Any())
                {
                    FileUtils.DeleteFilesFromDestination(folderToSyncPath, fileNamesToRemove, _logFile);
                }

                if(folderNamesToAdd.Any())
                {
                    FileUtils.CopyFoldersFromSourceToDestination(sourcePath,folderToSyncPath, folderNamesToAdd, _logFile);
                }

                if(folderNamesToRemove.Any())
                {
                    FileUtils.DeleteFoldersFromDestination(folderToSyncPath,folderNamesToRemove, _logFile);
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
            List<string> allSourceFilesNames = FileUtils.GetFileNames(Directory.GetFiles(sourcePath));
            List<string> allSourceFoldersNames = FileUtils.GetFileNames(Directory.GetDirectories(sourcePath));

            foreach (string fileName in allSourceFilesNames)
            {
                string originalFilePath = Path.Combine(sourcePath, fileName);
                string destinationFilePath = Path.Combine(replicaPath, fileName);
                
                if (File.Exists(originalFilePath) && File.Exists(destinationFilePath))
                {
                    if(FileUtils.WasFileModified(originalFilePath, destinationFilePath))
                    {
                        File.Delete(destinationFilePath);
                        File.Copy(originalFilePath, destinationFilePath);

                        string fileUpdatedMessage = $"The file :{destinationFilePath} has just been updated, to match the source file {originalFilePath}";

                        FileUtils.WriteToLogAndConsole(fileUpdatedMessage, _logFile);
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

       


    }
}
