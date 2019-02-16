using System;
using System.Collections.Concurrent;

namespace VigenereCipher
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Input the ciphertext:");
            string cipherText = Console.ReadLine();
            Console.WriteLine("Input the keyLength");
            int keyLength = int.Parse(Console.ReadLine());
            Console.WriteLine("Input the firstWordLength");
            int firstWordLength = int.Parse(Console.ReadLine());
            DateTime before = DateTime.Now;
            Decoder decoder = new Decoder(cipherText, keyLength, firstWordLength);
            ConcurrentBag<string> results = decoder.Decode();
            TimeSpan span = DateTime.Now - before;
            Console.WriteLine("Results ("+ span.TotalSeconds+"s): ");
            foreach (string result in results)
                Console.WriteLine(result);
            Console.ReadKey();
        }
    }
}
