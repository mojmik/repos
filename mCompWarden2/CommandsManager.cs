using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Permissions;
using System.ComponentModel.Design;
using System.Security.Cryptography;
using System.Xml.Serialization;
using System.Linq.Expressions;

namespace mCompWarden2 {
    public class CommandsManager {
        public List<CommandSet> commandsList = new List<CommandSet>();
        private string xmlPath = Program.localPath + "cmdset.xml";
        public Logger logger;
        bool isChanged;

        public CommandsManager(Logger logger) {
            this.logger = logger;
            MiscCommands.cmdMan = this;
        }
        public void SingleCommandSetFromFile(string file) {
            if (!File.Exists(file)) return;
            DateTime lastModified = System.IO.File.GetLastWriteTime(file);
            foreach (CommandSet cmdSet in commandsList) {
                if (cmdSet.SourceFilePath == file) {
                    if (cmdSet.FileLastModified == lastModified) return;
                    else {
                        //update in command file- we recreate the commandset
                        try {
                            cmdSet.MakeFromFile(file);
                        }                        
                        catch (Exception e) {
                            logger.WriteLog($"Exception file {file}: {e.Message} {e.InnerException}",Logger.TypeLog.both);
                        }
                        return;
                    }
                }                
            }
            try {
                commandsList.Add(new CommandSet(file));
            }
            catch (Exception e) {
                logger.WriteLog($"Exception file {file}: {e.Message} {e.InnerException}", Logger.TypeLog.both);
            }
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
            SingleCommandSetFromFile(Program.mainPath + System.Environment.MachineName.ToLower() + "-" + System.Environment.UserName.ToLower() + ".txt");
            SingleCommandSetFromFile(Program.mainPath + System.Environment.UserName.ToLower() + ".txt");
            SingleCommandSetFromFile(Program.mainPath + System.Environment.MachineName.ToLower() + ".txt");
        }
        public void LoadLocalCommands() {
            //run immediately                        
            MultipleCommandSetsFromFile(Program.commandsLocalPath, "*.txt");
        }

        public void RunCommands(bool isOnline) {
            foreach (CommandSet cmd in commandsList.ToList()) {
                try {
                    if (cmd.IsRemoved(isOnline)) {
                        commandsList.Remove(cmd);
                    }
                    else if (cmd.Run(isOnline)) {
                        logger.WriteLog($"cmd {cmd.SourceFilePath} ran successfully at {cmd.LastRun} ", Logger.TypeLog.both);
                        if (!cmd.IsRepeating) {
                            cmd.ArchiveSource();
                            commandsList.Remove(cmd);
                        }
                        isChanged = true;
                    }
                }
                catch (Exception e) {
                    logger.WriteLog($"Exception run at file {cmd.SourceFilePath}: {e.Message} {e.InnerException}", Logger.TypeLog.both);
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
        public void SaveIntoXML(bool checkChanged=true, string path="") {
            if (checkChanged && !isChanged) return;
            path = (path != "") ? path : xmlPath;
            if (File.Exists(path)) File.Delete(path);
            XmlSerializer ser = new XmlSerializer(commandsList.GetType());
            using (StreamWriter sw = new StreamWriter(path)) {
                ser.Serialize(sw, commandsList);
            }
            isChanged = false;
        }
        public void ClearCommands() {
            commandsList.Clear();
        }
    }
}
