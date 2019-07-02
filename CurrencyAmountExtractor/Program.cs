using System;
using System.IO;
using System.Reflection;
using System.Text;
using CurrencyAmountExtractor.CurrAmnt;

namespace CurrencyAmountExtractor
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var pathToTestFile = Path.Combine(currentDirectory, @"Resources/Doc_To_Scan.txt");

            using (StreamReader reader = new StreamReader(pathToTestFile))
            {
                string line;
                StringBuilder sb = new StringBuilder();
                while ((line = reader.ReadLine()) != null)
                {
                    sb.AppendLine(line);
                }
                long start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                CurrAmntExtractor currencyAmountExtractor = new CurrAmntExtractor(sb.ToString(), 2);
                // Main method
                Console.WriteLine(currencyAmountExtractor.DetectJSONCurrencyAmount());
                long end = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                Console.WriteLine((end - start) / 1000);
            }
        }
    }
}
