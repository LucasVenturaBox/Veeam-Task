using System.Security.Cryptography;

namespace VeeamTask
{
    class FileUtils
    {
        /// <summary>Copies an entire directory to the provided path</summary>
        /// <param name="sourceDirectory">The original directory path</param>
        /// <param name="destinationPath">The replicated directory path</param>
        /// <param name="logFile">The log file to write the update messages</param>
        public static void CopyEveryFileInDirectory(string sourceDirectory, string destinationPath, Log logFile)
        {
            if (Path.Exists(sourceDirectory))
            {
                List<string> allSourceFilePaths = GetFileNames(Directory.GetFiles(sourceDirectory));
                List<string> allSourceFolderPaths = GetFileNames(Directory.GetDirectories(sourceDirectory));

                CopyFilesFromSourceToDestination(sourceDirectory, destinationPath, allSourceFilePaths, logFile);

                CopyFoldersFromSourceToDestination(sourceDirectory, destinationPath, allSourceFolderPaths, logFile);

            }
        }

        /// <summary>Copies every file provided from the source into the replicated folder</summary>
        /// <param name="sourcePath">The parent folder path</param>
        /// <param name="destinationPath">The replicated directory path</param>
        /// <param name="allSourceFileNames">The name of every file you intend to copy</param>
        /// <param name="logFile">The log file to write the update messages</param>
        public static void CopyFilesFromSourceToDestination(string sourcePath, string destinationPath, List<string> allSourceFileNames, Log logFile)
        {
            foreach (string filePath in allSourceFileNames)
            {
                string originalFilePath = Path.Combine(sourcePath, filePath);
                string destinationFilePath = Path.Combine(destinationPath, filePath);

                if (!File.Exists(destinationFilePath))
                {
                    File.Copy(originalFilePath, destinationFilePath);

                    string fileCreatedMessage = $"The file {originalFilePath} was copied to {destinationFilePath}";

                    WriteToLogAndConsole(fileCreatedMessage, logFile);

                }
            }
        }

        /// <summary>Deletes every file removed on the source, from the replicated folder</summary>
        /// <param name="destinationPath">The replicated directory path</param>
        /// <param name="allSourceFileNames">The name of every file you intend to delete</param>
        /// <param name="logFile">The log file to write the update messages</param>
        public static void DeleteFilesFromDestination(string destinationPath, List<string> allDestinationFileNames, Log logFile)
        {
            foreach (string filePath in allDestinationFileNames)
            {
                string destinationFilePath = Path.Combine(destinationPath, filePath);

                if (File.Exists(destinationFilePath))
                {
                    File.Delete(destinationFilePath);

                    string fileDeletedMessage = $"A file called {filePath} was deleted from {destinationFilePath}";

                    WriteToLogAndConsole(fileDeletedMessage, logFile);

                }
            }
        }

        /// <summary>Copies every folder provided from the source directory into the replicated directory</summary>
        /// <param name="sourcePath">The parent folder path</param>
        /// <param name="destinationPath">The replicated directory path</param>
        /// <param name="allSourceFolderNames">The name of every subfolder you intend to copy</param>
        /// <param name="logFile">The log file to write the update messages</param>
        public static void CopyFoldersFromSourceToDestination(string sourcePath, string destinationPath, List<string> allSourceFolderNames, Log logFile)
        {
            foreach (string directory in allSourceFolderNames)
            {
                string originalFolderPath = Path.Combine(sourcePath, directory);
                string destinationFolderPath = Path.Combine(destinationPath, directory);

                if (!Directory.Exists(destinationFolderPath))
                {
                    Directory.CreateDirectory(destinationFolderPath);

                    string createdFolderMessage = $"The folder {originalFolderPath} was copied to {destinationFolderPath}";

                    WriteToLogAndConsole(createdFolderMessage, logFile);

                    CopyEveryFileInDirectory(originalFolderPath, destinationFolderPath, logFile);
                }
            }
        }

        /// <summary>Deletes every folder removed on the source directory from the replicated directory</summary>
        /// <param name="destinationPath">The parent folder path</param>
        /// <param name="allDestinationFolderNames">The name of every subfolder you intend to delete</param>
        /// <param name="logFile">The log file to write the update messages</param>
        public static void DeleteFoldersFromDestination(string destinationPath, List<string> allDestinationFolderNames, Log logFile)
        {
            foreach (string directory in allDestinationFolderNames)
            {
                string destinationFilePath = Path.Combine(destinationPath, directory);
                if (Directory.Exists(destinationFilePath))
                {
                    Directory.Delete(destinationFilePath, true);

                    string deletedFolderMessage = $"A folder called {directory} was deleted from {destinationFilePath}";

                    WriteToLogAndConsole(deletedFolderMessage, logFile);
                }
            }
        }

        /// <summary>Use to get the file names when you only have the full file paths</summary>
        /// <param name="filePaths">An array of the full file paths</param>
        /// <returns>List of strings with all the file names</returns>
        public static List<string> GetFileNames(string[] filePaths)
        {
            List<string> fileNames = new List<string>();
            foreach (string file in filePaths)
            {
                fileNames.Add(Path.GetFileName(file));
            }

            return fileNames;
        }

        /// <summary>Through the use of MD5 cryptography, compares two file hashs to check if their contents differ</summary>
        /// <param name="sourceFilePath">The control file path</param>
        /// <param name="replicatedFilePath">The file path to compare</param>
        /// <returns>Returns true if their contents differ</returns>

        public static bool WasFileModified(string sourceFilePath, string replicatedFilePath)
        {
            MD5 md5Instance = MD5.Create();

            byte[] sourceBuffer = File.ReadAllBytes(sourceFilePath);
            string sourceHash = BitConverter.ToString(md5Instance.ComputeHash(sourceBuffer));

            byte[] replicatedBuffer = File.ReadAllBytes(replicatedFilePath);
            string replicatedHash = BitConverter.ToString(md5Instance.ComputeHash(replicatedBuffer));


            if (sourceHash != replicatedHash)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>Write a line on the log file and write the same message on the console</summary>
        /// <param name="message">The line message to be logged and written onto the console</param>
        /// <param name="logFile">The file where you intented to write the line</param>
        public static void WriteToLogAndConsole(string message, Log logFile)
        {
            logFile?.AddLinesToLog(message);
            Console.WriteLine(message);
        }

    }
}
