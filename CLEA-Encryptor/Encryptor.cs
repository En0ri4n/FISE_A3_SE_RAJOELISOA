using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CLEA.Encryptor
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Usage: Encryptor <process|check> <key> <input> <output> [-file]");
                return;
            }

            bool isFile = args.Length >= 5 && args[4] == "-file";
            
            string type = args[0];
            string key = args[1];
            string input = args[2];
            string output = args[3];
            
            if (key.Length < 8)
            {
                Console.WriteLine("Key must be at least 8 characters long.");
                return;
            }

            if (type.Equals("process", StringComparison.OrdinalIgnoreCase))
            {
                if (isFile)
                {
                    Encryptor.ProcessFile(key, input, output);
                }
                else
                {
                    byte[] encryptedBytes = Encryptor.ProcessString(key, input);
                    Console.WriteLine($"Result:\n{Encoding.UTF8.GetString(encryptedBytes)}");
                }
            }
            else if (type.Equals("check", StringComparison.OrdinalIgnoreCase))
            {
                if (isFile)
                {
                    // Create a temporary file to store the processed file and check if the hashes match of the target
                    string tempFile = Path.GetTempFileName();
                    Encryptor.ProcessFile(key, input, tempFile);
                    Console.WriteLine(Encryptor.CompareFileHashes(tempFile, output) ? bool.TrueString : bool.FalseString);
                }
                else
                {
                    // Get the encrypted string and compare it with the output
                    byte[] encryptedBytes = Encryptor.ProcessString(key, input);
                    string encryptedString = Encoding.UTF8.GetString(encryptedBytes);
                    Console.WriteLine(encryptedString.Equals(output, StringComparison.Ordinal) ? bool.TrueString : bool.FalseString);
                }
            }
        }
    }
    
    /// <summary>
    /// Encryptor is a singleton class helping in encryption and decryption
    /// </summary>
    internal static class Encryptor
    {
        public static byte[] ProcessString(string key, string input)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "Key cannot be null or empty.");
            
            byte[] encryptionKey = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            for (int i = 0; i < inputBytes.Length; i++)
                inputBytes[i] ^= encryptionKey[i % encryptionKey.Length];

            return inputBytes;
        }

        /// <summary>
        /// Encrypt the given file into the target file<br></br>
        /// If the source file doesn't exists, it will throw a <see cref="FileNotFoundException"/><br></br>
        /// If the target file exists, it will overwrite it.<br></br>
        /// </summary>
        /// <param name="key">The key to encrypt/decrypt the file</param>
        /// <param name="sourceFile">The source file to encrypt</param>
        /// <param name="targetFile">The path of the encrypted source file content</param>
        public static void ProcessFile(string key, string sourceFile, string targetFile)
        {
            if (!File.Exists(sourceFile))
                throw new FileNotFoundException($"The file {sourceFile} doesn't exists.");

            byte[] encryptionKey = Encoding.UTF8.GetBytes(key);
            using FileStream sourceReaderStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
            using FileStream targetWriterStream = new FileStream(targetFile, FileMode.Create, FileAccess.Write);
            using BufferedStream bufferedReader = new BufferedStream(sourceReaderStream);
            using BufferedStream bufferedWriter = new BufferedStream(targetWriterStream);

            // Read and write in chunks of 4KB
            byte[] buffer = new byte[4096];
            int bytesRead;
            while ((bytesRead = bufferedReader.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < bytesRead; i++)
                    buffer[i] ^= encryptionKey[i % encryptionKey.Length]; // XOR operation to encrypt

                bufferedWriter.Write(buffer, 0, bytesRead);
            }

            bufferedWriter.Flush();
            bufferedWriter.Close();
        }

        public static bool CompareFileHashes(string file1, string file2)
        {
            using SHA256 hashAlgorithm = SHA256.Create();
            using FileStream stream1 = File.OpenRead(file1);
            using FileStream stream2 = File.OpenRead(file2);
            byte[] hash1 = hashAlgorithm.ComputeHash(stream1);
            byte[] hash2 = hashAlgorithm.ComputeHash(stream2);
            return StructuralComparisons.StructuralEqualityComparer.Equals(hash1, hash2);
        }
    }
}