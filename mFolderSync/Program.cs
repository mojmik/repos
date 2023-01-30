using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//syncs one folder to another and then keep this the first one synced
namespace mFolderSync {
    class Program {
        public static bool AppErr = false;
        public static string ProgramPath;
        private static List<FolderWatcher> watchers = new List<FolderWatcher>();
        static System.Threading.Mutex singleton = new Mutex(true, "mFolderSyncApp");
        private static string desktopPath, desktopLink,iconPath;
        private static string userName = Environment.UserName;
        private static List<string> sharedPath = new List<string>();
        private static List<string> localPath = new List<string>();
        private static List<string> locName = new List<string>();
        private static Dictionary<string, string> locationFolders = new Dictionary<string, string>();
        private static Dictionary<string, List<string>> members = new Dictionary<string, List<string>>();
        private static List<string> excludedComps = new List<string>();
        private static string localPathBase = @"c:\it\folderwatcher\";
        private static string localPathBaseFolder = @"\sharedfolder\";

        public static void WriteLogFile(string txt, string filePath = "folderwatcherlog.txt", bool includeDt = true) {
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(localPathBase + filePath, true)) {
                if (includeDt) txt = DateTime.Now + " " + txt;
                file.WriteLine(txt);
            }
        }
        public static void createShortcut(string linkName, string linkDst, string iconFile) {
            string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string linkFile = deskDir + "\\" + linkName + ".url";
            if (File.Exists(linkFile)) File.Delete(linkFile);
            using (StreamWriter writer = new StreamWriter(linkFile)) {
                string app = System.Reflection.Assembly.GetExecutingAssembly().Location;
                writer.WriteLine("[InternetShortcut]");
                writer.WriteLine("URL=file:///" + linkDst);
                writer.WriteLine("IconIndex=0");
                
                string icon = iconFile.Replace('\\', '/');
                writer.WriteLine("IconFile=" + icon);
                
            }
        }
        public static void initParams() {
            string[] lines;
            try {
                lines = System.IO.File.ReadAllLines(@"\\rentex.intra\company\data\Company\mkavan_upravy\scripts\folderwatcher\settings.txt");
            }
            catch (Exception e) {
                WriteLogFile("err: settings not found");
                AppErr = true;
                return;
            }
            
            char[] splitters = { '=' };
            foreach (string line in lines) {
                var mTxt = line.Split(splitters, 2);
                if (mTxt.Length > 1) {
                    if (mTxt[0].ToLower() == "pob".ToLower()) {
                        string[] locationSet = mTxt[1].Split(';');
                        locationFolders[locationSet[0]] = locationSet[1];
                    }
                    if (mTxt[0].ToLower() == "computers-exluded".ToLower()) {
                        excludedComps = mTxt[1].ToLower().Split(';').ToList();
                    }
                    if (mTxt[0].ToLower() == "localPathBase".ToLower()) {
                        localPathBase = mTxt[1];
                    }
                    if (mTxt[0].ToLower() == "localPathBaseFolder".ToLower()) {
                        localPathBaseFolder = mTxt[1];
                    }
                    if (mTxt[0].ToLower() == "desktopPath".ToLower()) {
                        desktopPath = mTxt[1];
                    }
                    if (mTxt[0].ToLower() == "desktopLink".ToLower()) {
                        desktopLink = mTxt[1];
                    }
                    if (mTxt[0].ToLower() == "iconPath".ToLower()) {
                        iconPath = mTxt[1];
                    }
                }
            }

            try {
                lines = System.IO.File.ReadAllLines(@"\\rentex.intra\company\data\Company\mkavan_upravy\scripts\folderwatcher\members.txt");
            }
            catch (Exception e) {
                WriteLogFile("err: members settings not found");
                AppErr = true;
                return;
            }

            foreach (string line in lines) {
                var mTxt = line.Split(splitters, 2);
                if (mTxt.Length > 1) {
                    if (mTxt[0].ToLower() == "user".ToLower()) {
                        string[] memberSet = mTxt[1].ToLower().Split(';');
                        string user = memberSet[0];
                        if (user.ToLower()==userName.ToLower()) {
                            string userLocation = memberSet[1];
                            List<string> locations = userLocation.Split('|').ToList();
                            members[user] = locations;
                            foreach (string loc in locations) {
                                try {
                                    sharedPath.Add(locationFolders[loc]);
                                }
                                catch {
                                    WriteLogFile("member location missing");
                                    AppErr = true;
                                }
                                localPath.Add(localPathBase + localPathBaseFolder + loc + "\\");
                                locName.Add(loc.ToUpper());
                            }
                        }
                    }

                }
            }

            if (excludedComps.Contains(Environment.MachineName.ToLower())) {
                AppErr = true;
                return;
            }

            if (sharedPath.Count<1) {
                WriteLogFile("not a member of any group");
                AppErr = true;
                return;
            }


        }

        static void Main(string[] args) {
            //var directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            //ProgramPath = new Uri(directory).LocalPath + "\\"; 
            ProgramPath = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            if (!singleton.WaitOne(TimeSpan.Zero, true)) {
                //there is already another instance running!
                return;
            }
            
            initParams();
            if (sharedPath == null || localPath == null) {
                WriteLogFile("err: settings not set");
                AppErr = true;
            }
            if (AppErr) {
                return;
            }
            WriteLogFile("about to initialsync");
            for (int n=0;n<sharedPath.Count;n++) {
                FolderWatcher folderWatcher = new FolderWatcher(); 
                watchers.Add(folderWatcher);
                string locPath = localPath[n];
                string shPath = sharedPath[n];
                createShortcut(desktopLink + " " + locName[n], locPath, iconPath);
                folderWatcher.initialFullSync(shPath, locPath);
                WriteLogFile($"initialsync done for {userName} at {locName[n]}");
                folderWatcher.startWatcher(locPath, shPath);

            }
            //aby nam to nezabijel GC


            //folderWatcher.ReplaceFile(@"G:\erp\KSC\in\mtemp\blocek.txt");
            for (; ; ) {
                Thread.Sleep(350);
                foreach (FolderWatcher fw in watchers) {
                    GC.KeepAlive(fw.watcherSrc);
                    GC.KeepAlive(fw.watcherDst);
                }
                
            }
        }

    }
}
