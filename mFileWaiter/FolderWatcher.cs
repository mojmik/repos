using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mFileWaiter {
    class FolderWatcher {
        public FileSystemWatcher watcher = new FileSystemWatcher();
        DateTime lastRead = DateTime.MinValue;
        private string jobName;
        public void watchForFiles(string folder,string name) {
            watcher.Path = folder;
            jobName = name;
            /*
            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size; 
            */
            watcher.NotifyFilter = 
                                NotifyFilters.CreationTime
                               | NotifyFilters.DirectoryName
                               | NotifyFilters.FileName                               
                               | NotifyFilters.LastWrite                               
                               ;
            watcher.Filter = "*.*";
            watcher.Changed += OnChanged;
            watcher.Renamed += OnRenamed;
            watcher.Created += OnCreated;
            watcher.EnableRaisingEvents = true;
            WriteLog($"{name} job started " + DateTime.Now.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"));
        }
        private void WriteLog(string log) {
            //File.AppendAllText(Path.Combine(watcher.Path, $"watcherlog{jobName}.txt"), log);                        
            File.AppendAllText(Path.Combine(Program.ProgramPath,$"watcherlog{jobName}.txt"), log + Environment.NewLine);
        }
        private void OnChanged(object source, FileSystemEventArgs e) {
            DateTime lastWriteTime = File.GetLastWriteTime(e.FullPath);
            if (lastWriteTime != lastRead) {
                //Copies file to another directory.
                WriteLog(e.FullPath + " changed");
                if (e.FullPath.EndsWith("blocek.txt")) ReplaceFile(e.FullPath);
                lastRead = lastWriteTime;
            }            
        }
        public void ReplaceFile(string filePath) {
            System.IO.StreamReader file = new System.IO.StreamReader(filePath);

            string line = "";
            int counter = 0;
            string startStr = "Odberatel: ^c4";
            string endStr = "^k";
            string odberatel;
            int startPos;
            int endPos;
            string outLine = "";
            string log = System.DateTime.Now.ToString() + Environment.NewLine;
            log += filePath + Environment.NewLine;
            while ((line = file.ReadLine()) != null) {
                if (line.Contains("Odberatel: ^c4")) {
                    startPos = line.IndexOf(startStr) + startStr.Length;
                    endPos = line.IndexOf(endStr);
                    odberatel = line.Substring(startPos, endPos - startPos);
                    Console.WriteLine(odberatel);
                    //Regex.Replace(odberatel, @"\s+", "");
                    odberatel = Regex.Replace(odberatel, @"[^0-9a-zA-Z]", "");
                    Console.WriteLine(odberatel);
                    outLine += line.Substring(0, startPos) + odberatel + line.Substring(endPos, endStr.Length);
                }
                else outLine += line;
                outLine += Environment.NewLine;
                counter++;
            }
            file.Close();
            string newPath = filePath.Replace(@"\mtemp", "");
            File.WriteAllText(newPath, outLine);
            log += newPath + Environment.NewLine;
            log += outLine;
            WriteLog(log);
            File.Delete(filePath);            
        }
        private void OnRenamed(object source, RenamedEventArgs e) {
            // Read the file and display it line by line.  
            //File.Copy(e.FullPath, ConfigurationManager.AppSettings["ToPath"] + "\\" + Path.GetFileName(e.FullPath), true);
            //File.Delete(e.FullPath);
            WriteLog(e.FullPath + " renamed");
            if (e.FullPath.EndsWith("blocek.txt")) ReplaceFile(e.FullPath);
        }
        private void OnCreated(object source, FileSystemEventArgs e) {
            //Copies file to another directory.            
            //pockame si na onrenamed            
            WriteLog(e.FullPath + " created waiting for rename");
            return;
            if (e.FullPath.EndsWith("blocek.txt")) ReplaceFile(e.FullPath);
        }

        public void Dispose() {
            // avoiding resource leak
            watcher.Changed -= OnChanged;
            watcher.Renamed -= OnRenamed;
            watcher.Created -= OnCreated;
            this.watcher.Dispose();
        }
    }
}
