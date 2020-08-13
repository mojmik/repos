using System;

namespace SpusteniVBS {
    class Program {
        static void Main(string[] args) {            
            string scriptFile = "mscript.wsf";
            System.Diagnostics.Process.Start("WScript.exe", " " + scriptFile);
            Console.WriteLine("script ran");
        }
    }
}
