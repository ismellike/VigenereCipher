using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VigenereCipher
{
    class Decoder
    {
        //CONSTS
        const string path = "dict.txt";
        readonly char[] aArray = Alphabet.ToArray();
        const string Alphabet = "abcdefghijklmnopqrstuvwxyz";

        //VARIABLES
        private List<string> words = new List<string>();
        private ConcurrentBag<string> results = new ConcurrentBag<string>();
        private byte[] data = null;
        public int KeyLength { get; private set; }
        public int FirstWordLength { get; private set; }
        long Total
        {
            get
            {
                long n = 1;
                for (int i = 0; i < KeyLength; i++)
                    n *= 26;
                return n;
            }
        }

        public Decoder(string cipherText, int keyLength, int firstWordLength)
        {
            //SETUP
            KeyLength = keyLength;
            FirstWordLength = firstWordLength;

            string normalized = cipherText.Replace(' ', '\0').ToLower();
            data = new byte[normalized.Length];
            for (int i = 0; i < normalized.Length; i++)
            {
                data[i] = (byte)Alphabet.IndexOf(normalized[i]);
            }

            //LOAD WORDS
            using (StreamReader sr = new StreamReader(path))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    int length = line.Length;
                    if (length > firstWordLength)
                        break;
                    else if (firstWordLength == length)
                        words.Add(line);
                }
            }
        }

        public ConcurrentBag<string> Decode()
        {
            //GENERATE ALL COMBOS & TRANSLATE
            List<Thread> threads = new List<Thread>();
            long total = Total;
            byte[] combo = new byte[KeyLength];
            int size = total < Math.Pow(26, 5) ? (int)total : (int)Math.Pow(26, 5);
            byte[][] combinations = new byte[size][];
            for (int i = 0; i < size; i++)
                combinations[i] = new byte[KeyLength];

            int count = 0;
            int index = 0;

            while (count < total)
            {
                for (int i = 0; i < 26; i++)
                {
                    combo[0] = (byte)i;
                    index = count % size;
                    combinations[index] = (byte[])combo.Clone();
                    if (index == size - 1)
                       Check(combinations);
                    count++;
                }
                for (int i = 1; i < KeyLength; i++)
                {
                    if (combo[i - 1] == 25)
                    {
                        if (combo[i] == 25)
                            continue;
                        combo[i - 1] = 0;
                        combo[i]++;
                        break;
                    }
                }
            }
            //RETURN RESULTS
            return results;
        }

        private void Check(byte[][] combinations)
        {
            //FIND TRANSLATION IN SET
            Parallel.ForEach(combinations, (combination) =>
            {
                int n = 0;
                byte[] local = (byte[])data.Clone();
                for (int i = 0; i < FirstWordLength; i++)
                {
                    local[i] = VigenereTranslate(local[i], combination[n]);
                    n = (n + 1) % KeyLength;
                }

                if (words.Contains(Translation(local, FirstWordLength).ToUpper()))
                {
                    for (int i = FirstWordLength; i < data.Length; i++)
                    {
                        local[i] = VigenereTranslate(local[i], combination[n]);
                        n = (n + 1) % KeyLength;
                    }
                    results.Add(Translation(local, data.Length));
                }
            });

        }

        public byte VigenereTranslate(byte c, byte k)
        {
            //REAL MODULO
            int x = c - k;
			int shift = x % 26 + 26;
			byte val =  shift % 26;
            return val;
        }

        public string Translation(byte[] array, int length)
        {
            string builder = "";
            for (int i = 0; i < length; i++)
                builder += aArray[array[i]];
            return builder;
        }
    }
}
