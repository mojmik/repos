using System;
using System.IO;

namespace TestDummy
{
    class Program
    {
        static void Main(string[] args)
        {
            string cwd = Directory.GetCurrentDirectory();
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string user = Environment.UserDomainName + "\\" + Environment.UserName;
            string message = $"Test executed successfully at {timestamp} by {user} in working directory: {cwd}{Environment.NewLine}";

            string outputFile = Path.Combine(cwd, "test.txt");

            Console.WriteLine("=================================");
            Console.WriteLine(" TestDummy Executable Running    ");
            Console.WriteLine("=================================");
            Console.WriteLine($"User      : {user}");
            Console.WriteLine($"CWD       : {cwd}");
            Console.WriteLine($"Target File: {outputFile}");

            try
            {
                File.AppendAllText(outputFile, message);
                Console.WriteLine("SUCCESS: Written to test.txt");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR writing to test.txt: {ex.Message}");
            }
        }
    }
}
