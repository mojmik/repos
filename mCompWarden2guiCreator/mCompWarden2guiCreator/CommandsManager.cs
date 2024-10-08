using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Permissions;
using System.ComponentModel.Design;
using System.Security.Cryptography;
using System.Xml.Serialization;


namespace mCompWarden2guiCreator {
    class CommandsManager {
        public List<CommandFile> commandFiles = new List<CommandFile>();
        public List<string> repeatingTypes=new List<string>();
        public static string FileExtension = "cwd";

        public CommandsManager() {
            repeatingTypes.Add("");
            repeatingTypes.Add("s");
            repeatingTypes.Add("m");
            repeatingTypes.Add("h");
            repeatingTypes.Add("d");
            repeatingTypes.Add("!");
        }

        public CommandFile AddCommandFile() {
            CommandFile cf = new CommandFile();
            commandFiles.Add(cf);
            return cf;
        }

      

        
    }
}
