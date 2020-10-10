using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Permissions;
using System.ComponentModel.Design;
using System.Security.Cryptography;
using System.Xml.Serialization;


namespace mCompWarden2 {
    class CommandsManager {
        public List<CommandSet> commandsList = new List<CommandSet>();
        private string xmlPath = Program.localPath + "cmdset.xml";
        Logger logger;
        bool isChanged;

        public CommandsManager(Logger logger) {
            this.logger = logger;
        }
        public void SingleCommandSetFromFile(string file) {
            if (!File.Exists(file)) return;
            foreach (CommandSet cmdSet in commandsList) {
                if (cmdSet.SourceFilePath == file) return;
            }
            commandsList.Add(new CommandSet(file));
            isChanged = true;
        }
        public void MultipleCommandSetsFromFile(string path, string fileMask) {

            DirectoryInfo d = new DirectoryInfo(path);
            FileInfo[] Files = d.GetFiles(fileMask);
            foreach (FileInfo file in Files) {
                SingleCommandSetFromFile(file.FullName);
            }
        }

        public void LoadRemoteCommands() {
            //run daily            
            MultipleCommandSetsFromFile(Program.mainPath, "all*.txt");
            SingleCommandSetFromFile(Program.mainPath + System.Environment.MachineName + "-" + System.Environment.UserName + ".txt");
            SingleCommandSetFromFile(Program.mainPath + System.Environment.MachineName + ".txt");
        }
        public void LoadLocalCommands() {
            //run immediately                        
            MultipleCommandSetsFromFile(Program.commandsLocalPath, "*.txt");
        }

        public void RunCommands(bool isOnline) {
            foreach (CommandSet cmd in commandsList.ToList()) {
                if (cmd.Run(isOnline)) {
                    logger.WriteLog($"cmd {cmd.SourceFilePath} ran successfully at {cmd.LastRun} ",Logger.TypeLog.both);
                    if (!cmd.IsRepeating) {
                        cmd.ArchiveSource();
                        commandsList.Remove(cmd);
                    }

                    isChanged = true;
                }
            }
        }

        public void LoadFromXML() {
            XmlSerializer ser = new XmlSerializer(commandsList.GetType());
            if (File.Exists(xmlPath)) {
                using (StreamReader sr = new StreamReader(xmlPath)) {
                    commandsList = (List<CommandSet>)ser.Deserialize(sr);
                }
            }
        }
        public void SaveIntoXML() {

            if (!isChanged) return;            
            XmlSerializer ser = new XmlSerializer(commandsList.GetType());
            using (StreamWriter sw = new StreamWriter(xmlPath)) {
                ser.Serialize(sw, commandsList);
            }
            isChanged = false;
        }
    }
}
