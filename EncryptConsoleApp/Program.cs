﻿using System;
using System.Diagnostics;
using System.IO;

namespace EncryptConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var timer = new Stopwatch();
            timer.Start();

            var sample = @"C:\Users\axels\Downloads\sample.gif";
            var encrypted = @"C:\Users\axels\Downloads\ecrypted.gif";
            var decrypted = @"C:\Users\axels\Downloads\decrypted.gif";
            var fileBytes = File.ReadAllBytes(sample);

            var resultEncrypt = EncryptionUtil.EncryptBytes(fileBytes, "password", "salt");
            File.WriteAllBytes(encrypted, resultEncrypt);

            var resultDecrypt = EncryptionUtil.DecryptBytes(resultEncrypt, "password", "salt");
            File.WriteAllBytes(decrypted, resultDecrypt);

            var elapsedTime = timer.ElapsedMilliseconds;
            timer.Stop();
            Console.WriteLine($"ElapsedTime : {elapsedTime} ms");
            Console.ReadLine();
        }
    }
}
