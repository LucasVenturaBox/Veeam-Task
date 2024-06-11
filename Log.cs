namespace VeeamTask
{
    public class Log{

        private string _logFilePath;

        private List<string> _logLines = new List<string>();


        public Log(string logPath)
        {
            _logFilePath = logPath;
        }

        public void AddLinesToLog(string line)
        {
            
            _logLines.Add(line);

            if (_logFilePath != null)
            {
                foreach (string missingLine in _logLines)
                {
                    string currentMinutes = DateTime.Now.TimeOfDay.Minutes.ToString();
                    string currentSeconds = DateTime.Now.TimeOfDay.Seconds.ToString();
                    string message = "["+currentMinutes +"mins:"+ currentSeconds +"secs] " + missingLine + "\n";
                   
                   File.AppendAllText(_logFilePath, message);
                }

                _logLines.Clear();
            }
        }
    }
}