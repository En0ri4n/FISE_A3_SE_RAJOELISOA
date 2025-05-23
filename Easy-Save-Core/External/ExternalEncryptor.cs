using System.Diagnostics;
using System.IO;
using CLEA.EasySaveCore.Utilities;
using Microsoft.Extensions.Logging;

namespace CLEA.EasySaveCore.External
{
    public static class ExternalEncryptor
    {
        public static bool IsEncryptorPresent()
        {
            return File.Exists("CLEA-Encryptor.exe");
        }
        
        public static void ProcessFile(string key, string fileInputPath, string fileOutputPath)
        {
            if (!IsEncryptorPresent())
            {
                Logger.Log(LogLevel.Error, "CLEA-Encryptor.exe not found. Please ensure it is in the same directory as the executable.");
                return;
            }
            
            Logger.Log(LogLevel.Information, $"Encrypting file using Encryptor from {fileInputPath} to {fileOutputPath}");
            Process process = new Process();
            process.StartInfo.FileName = "CLEA-Encryptor.exe";
            process.StartInfo.Arguments = $"{key} \"{fileInputPath}\" \"{fileOutputPath}\" -file";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
        }
    }
}