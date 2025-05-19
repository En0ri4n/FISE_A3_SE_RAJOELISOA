using System.Numerics;
using System.Text;

namespace Prosit_5;

/// <summary>
/// Encryptor is a singleton class helping in encryption and decryption
/// </summary>
/// TODO: Multiple encryption ?
public class Encryptor
{
    // 8 bytes key for XOR Encryption
    private byte[] _encryptionKey = "12345678"u8.ToArray();
    private static readonly Encryptor Instance = new Encryptor();
    
    public static Encryptor Get()
    {
        return Instance;
    }
    
    public void SetEncryptionKey(byte[] key)
    {
        if (key.Length != 8)
            throw new ArgumentException("Key must be 8 bytes long");
        
        _encryptionKey = key;
    }

    public byte[] GetEncryptionKey()
    {
        return _encryptionKey;
    }

    public byte[] ProcessString(string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);

        for (int i = 0; i < inputBytes.Length; i++)
            inputBytes[i] ^= _encryptionKey[i % _encryptionKey.Length];

        return inputBytes;
    }
    
    /// <summary>
    /// Encrypt the given file into the target file<br></br>
    /// If the source file doesn't exists, it will throw a <see cref="FileNotFoundException"/><br></br>
    /// If the target file exists, it will overwrite it.<br></br>
    /// </summary>
    /// <param name="sourceFile">The source file to encrypt</param>
    /// <param name="targetFile">The path of the encrypted source file content</param>
    public void ProcessFile(string sourceFile, string targetFile)
    {
        if (!File.Exists(sourceFile))
            throw new FileNotFoundException($"The file {sourceFile} doesn't exists.");
        
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
                buffer[i] ^= _encryptionKey[i % _encryptionKey.Length]; // XOR operation to encrypt
            
            bufferedWriter.Write(buffer, 0, bytesRead);
        }
        bufferedWriter.Flush();
        bufferedWriter.Close();
    }
}

public class SimpleRSA
{
    public BigInteger PublicKeyExponent { get; private set; }
    public BigInteger PrivateKeyExponent { get; private set; }
    public BigInteger Modulus { get; private set; }

    public SimpleRSA()
    {
        BigInteger p = 10000019;
        BigInteger q = 10000079;
        Modulus = p * q;
        BigInteger phi = (p - 1) * (q - 1);
        PublicKeyExponent = 65537;
        PrivateKeyExponent = ModInverse(PublicKeyExponent, phi);
    }

    /// <summary>
    /// Encrypt the given string using RSA algorithm<br></br>
    /// </summary>
    /// <param name="plainText">The string to encrypt</param>
    /// <returns></returns>
    public string EncryptString(string plainText)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(plainText);
        StringBuilder sb = new StringBuilder();

        foreach (byte b in bytes)
        {
            BigInteger m = new BigInteger(b);
            BigInteger c = BigInteger.ModPow(m, PublicKeyExponent, Modulus);
            byte[] encryptedBytes = c.ToByteArray();
            string base64 = Convert.ToBase64String(encryptedBytes).PadRight(8, '=');
            sb.Append(base64);
        }

        return sb.ToString();
    }
    
    /// <summary>
    /// Decrypt the given string using RSA algorithm<br></br>
    /// </summary>
    /// <param name="encrypted">The encrypted string to decrypt</param>
    /// <returns></returns>
    public string DecryptString(string encrypted)
    {
        List<byte> decryptedBytes = new List<byte>();
        for (int i = 0; i < encrypted.Length; i += 8)
        {
            string base64Block = encrypted.Substring(i, 8).TrimEnd('=');
            byte[] blockBytes = Convert.FromBase64String(base64Block + new string('=', (4 - base64Block.Length % 4) % 4));
            BigInteger c = new BigInteger(blockBytes);
            BigInteger m = BigInteger.ModPow(c, PrivateKeyExponent, Modulus);
            decryptedBytes.Add((byte)m);
        }

        return Encoding.UTF8.GetString(decryptedBytes.ToArray());
    }

    // Modular inverse using Extended Euclidean Algorithm
    private BigInteger ModInverse(BigInteger a, BigInteger m)
    {
        BigInteger m0 = m, t, q;
        BigInteger x0 = 0, x1 = 1;

        while (a > 1)
        {
            q = a / m;
            t = m;
            m = a % m;
            a = t;
            t = x0;

            x0 = x1 - q * x0;
            x1 = t;
        }

        if (x1 < 0) x1 += m0;
        return x1;
    }
}