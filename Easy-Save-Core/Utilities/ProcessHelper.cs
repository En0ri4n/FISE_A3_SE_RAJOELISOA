using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace CLEA.EasySaveCore.Utilities
{
    public class ProcessHelper
    {
        private static readonly ProcessHelper Instance = new ProcessHelper();
        public static ProcessHelper Get() => Instance;
        
        public static bool IsExactProcessRunning(string processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                return processes.Length > 0;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warning, $"Error checking if process {processName} is running");
                return false;
            }
        }
        
        public static bool IsProcessRunning(string processName)
        {
            try
            {
                Regex regex = new Regex(processName, RegexOptions.IgnoreCase);
                Process[] processes = Process.GetProcesses();
                if (processes.Any(process => regex.IsMatch(process.ProcessName)))
                    return true;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warning, $"Error checking if process {processName} is running :\n{ex.Message}");
            }

            return false;
        }

        public static bool IsAnyProcessRunning(string[] processNames)
        {
            try
            {
                if (processNames.Any(IsProcessRunning))
                    return true;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warning, $"Error checking if any process is running :\n{ex.Message}");
            }

            return false;
        }
    }
}