using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class JsonEncryptor
{
    // You can customize this key and IV (but must match between save/load)
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("My16ByteAESKey!!"); // ✅ 16 bytes
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("My16ByteInitVec!"); // ✅ 16 bytes


    public static string Encrypt(string plainText)
    {
        using Aes aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;

        using MemoryStream ms = new();
        using CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
        using (StreamWriter sw = new(cs))
        {
            sw.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public static string Decrypt(string encryptedBase64)
    {
        byte[] cipherBytes = Convert.FromBase64String(encryptedBase64);

        using Aes aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;

        using MemoryStream ms = new(cipherBytes);
        using CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using StreamReader sr = new(cs);
        return sr.ReadToEnd();
    }
}
