using System;
using System.Buffers.Text;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CLEA.EasySaveCore.Utilities;
using EasySaveCore.Jobs.Backup.Configurations;
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

        public static string EncodeKeyInBase64(string key)
        {
            byte[] data = Encoding.UTF8.GetBytes(key);
            return Convert.ToBase64String(data);
        }

        public static string DecodeKeyFromBase64(string base64Encoded)
        {
            byte[] base64Bytes = Convert.FromBase64String(base64Encoded);
            return Encoding.UTF8.GetString(base64Bytes);
        }

        public static string ProcessEncryptionKey(string encryptedKey)
        {
            byte[] data = Encoding.UTF8.GetBytes(encryptedKey);
            byte[] encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encrypted);
        }

        public static string GetEncryptionKey()
        {
            byte[] data = Convert.FromBase64String(BackupJobConfiguration.Get().EncryptionKey);
            byte[] decrypted = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
    }
}