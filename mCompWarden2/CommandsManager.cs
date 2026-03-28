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

namespace mCompWarden2
{
    public class CommandsManager
    {
        public List<CommandSet> commandsList = new List<CommandSet>();
        private string xmlPath = Program.localPath + "cmdset.xml";

        bool isChanged;

        public CommandsManager()
        {

            MiscCommands.cmdMan = this;
        }
        public void SingleCommandSetFromFile(string file)
        {
            if (!File.Exists(file)) return;
            DateTime lastModified = System.IO.File.GetLastWriteTime(file);
            foreach (CommandSet cmdSet in commandsList)
            {
                if (cmdSet.SourceFilePath == file)
                {
                    if (cmdSet.FileLastModified == lastModified) return;
                    else
                    {
                        //update in command file- we recreate the commandset
                        try
                        {
                            cmdSet.MakeFromFile(file);
                        }
                        catch (Exception e)
                        {
                            Logger.WriteLog($"Exception file {file}: {e.Message} {e.InnerException}", Logger.TypeLog.both);
                        }
                        return;
                    }
                }
            }
            try
            {
                commandsList.Add(new CommandSet(file));
            }
            catch (Exception e)
            {
                Logger.WriteLog($"Exception file {file}: {e.Message} {e.InnerException}", Logger.TypeLog.both);
            }
            isChanged = true;
        }
        public void MultipleCommandSetsFromFile(string path, string fileMask)
        {

            DirectoryInfo d = new DirectoryInfo(path);
            try
            {
                FileInfo[] Files = d.GetFiles(fileMask);
                foreach (FileInfo file in Files)
                {
                    SingleCommandSetFromFile(file.FullName);
                }
            }
            catch (Exception e)
            {
                Logger.WriteLog($"Fileserver problably not reachable, exception: {e.Message} {e.InnerException}", Logger.TypeLog.both);
            }

        }

        public void LoadRemoteCommands()
        {


            var directory = new System.IO.DirectoryInfo(Program.mainPath);
            var files = directory.GetFiles();
            foreach (var file in files)
            {
                string fileExtension = file.Extension;
                if (fileExtension == ".cwd")
                {
                    if (file.Name.StartsWith("all") || file.Name.StartsWith(Environment.MachineName.ToLower()) || file.Name.StartsWith(Environment.UserName.ToLower()))
                    {
                        //Logger.WriteLog($"gonna load file: {file.FullName}", Logger.TypeLog.both);
                        SingleCommandSetFromFile(file.FullName);
                    }

                }
                if (fileExtension == ".txt")
                {
                    if (file.FullName == Program.mainPath + Environment.MachineName.ToLower() + "-" + System.Environment.UserName.ToLower() + ".txt")
                    {
                        SingleCommandSetFromFile(file.FullName);
                    }
                    if (file.FullName == Program.mainPath + System.Environment.UserName.ToLower() + ".txt")
                    {
                        SingleCommandSetFromFile(file.FullName);
                    }
                    if (file.FullName == Program.mainPath + System.Environment.MachineName.ToLower() + ".txt")
                    {
                        SingleCommandSetFromFile(file.FullName);
                    }
                    if (file.Name.StartsWith("all"))
                    {
                        SingleCommandSetFromFile(file.FullName);
                    }
                }
            }
            LoadV3ConfigsRemote();
        }
        public void LoadLocalCommands()
        {
            var directory = new System.IO.DirectoryInfo(Program.commandsLocalPath);
            var files = directory.GetFiles();
            foreach (var file in files)
            {
                string fileExtension = file.Extension;
                if (fileExtension == ".cwd") SingleCommandSetFromFile(file.FullName);
                if (fileExtension == ".txt")
                {
                    SingleCommandSetFromFile(file.FullName);
                }
            }
            LoadV3ConfigsLocal();
        }

        public void RunCommands(bool isOnline)
        {
            var filesToMaybeArchive = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (CommandSet cmd in commandsList.ToList())
            {
                try
                {
                    if (cmd.IsRemoved(isOnline))
                    {
                        // DO NOT mark for archive on disappearance
                        commandsList.Remove(cmd);
                        continue;
                    }

                    if (cmd.Run(isOnline))
                    {
                        Logger.WriteLog($"cmd {cmd.SourceFilePath} ran successfully at {cmd.LastRun} ", Logger.TypeLog.both);

                        var v3schedule = cmd as IV3Schedule;
                        if (v3schedule != null) v3schedule.AdvanceAfterRun();

                        if (!cmd.IsRepeating)
                        {
                            bool allowArchive = true;

                            // 1) Never archive shared files (filename starts with "all")
                            if (IsSharedFileName(cmd.SourceFileName)) allowArchive = false;

                            // 2) (Optional safety) V3 "once" shared (machine empty or "all"): don't archive
                            var v3cmd = cmd as V3CommandSet;
                            if (v3cmd != null &&
                                string.Equals(v3cmd.ScheduleTypeV3, "once", StringComparison.OrdinalIgnoreCase))
                            {
                                var mn = cmd.MachineName ?? "";
                                if (string.IsNullOrWhiteSpace(mn) || mn.Equals("all", StringComparison.OrdinalIgnoreCase))
                                    allowArchive = false;
                            }

                            if (allowArchive)
                                filesToMaybeArchive.Add(cmd.SourceFilePath ?? "");

                            commandsList.Remove(cmd);
                            isChanged = true;
                        }

                        isChanged = true;
                    }
                }
                catch (Exception e)
                {
                    Logger.WriteLog($"Exception run at file {cmd.SourceFilePath}: {e}", Logger.TypeLog.both);
                }
            }

            // Archive only when no CommandSets from that source remain
            foreach (var sourcePath in filesToMaybeArchive.ToList())
            {
                if (string.IsNullOrWhiteSpace(sourcePath))
                    continue;

                bool anyLeftFromThisFile = commandsList.Any(c =>
                    string.Equals(c.SourceFilePath, sourcePath, StringComparison.OrdinalIgnoreCase));

                if (!anyLeftFromThisFile)
                {
                    TryArchiveSourceFileOnce(sourcePath);
                    filesToMaybeArchive.Remove(sourcePath);
                }
            }
        }

        // Helper: shared files start with "all"
        private static bool IsSharedFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return false;
            var name = fileName.Trim().ToLowerInvariant();
            return name.StartsWith("all-") || name.StartsWith("all.");
        }

        private static bool ShouldLoadV3File(string path)
        {
            var file = System.IO.Path.GetFileName(path) ?? "";
            var name = file.ToLowerInvariant();         // includes extension; StartsWith works fine
            var host = System.Environment.MachineName.ToLowerInvariant();
            var user = System.Environment.UserName.ToLowerInvariant();

            // Accept: all*.mcw3.xml, <hostname>*.mcw3.xml, <username>*.mcw3.xml
            // (Keeps it consistent with your V2 filter and your GUI’s suggested names)
            return name.StartsWith("all")
                || name.StartsWith(host)
                || name.StartsWith(user);
        }

        private static bool IsRemotePath(string path)
        {
            // UNC or \\?\UNC\…
            return !string.IsNullOrEmpty(path) &&
                   (path.StartsWith(@"\\", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase));
        }

        private static void TryArchiveSourceFileOnce(string sourcePath)
        {
            try
            {
                if (!File.Exists(sourcePath)) return;

                string uncArc = Program.archiveUNC;
                string localArc = Program.archiveLocal;

                // If the source is REMOTE (UNC), we do not fall back to local on failure
                bool sourceIsRemote = IsRemotePath(sourcePath);

                string arcFolder = null;
                bool uncOk = false;

                try
                {
                    Directory.CreateDirectory(uncArc);
                    var probe = Path.Combine(uncArc, ".probe");
                    File.WriteAllText(probe, "ok");
                    File.Delete(probe);
                    arcFolder = uncArc;
                    uncOk = true;
                }
                catch
                {
                    // UNC arc not available
                    if (!sourceIsRemote)
                    {
                        // Local source: allow local fallback
                        try { Directory.CreateDirectory(localArc); } catch { /* ignore */ }
                        arcFolder = localArc;
                    }
                    else
                    {
                        // Remote source: skip archiving if we cannot write to UNC arc
                        return;
                    }
                }

                var name = Path.GetFileNameWithoutExtension(sourcePath);
                var ext = Path.GetExtension(sourcePath);
                var ts = DateTime.Now.ToString("yyyyMMdd-HHmmss");

                var dest = Path.Combine(arcFolder, $"{name}_{Environment.MachineName}_{Environment.UserName}_{ts}{ext}");

                try
                {
                    File.Move(sourcePath, dest);
                }
                catch
                {
                    File.Copy(sourcePath, dest, overwrite: true);
                    try { File.Delete(sourcePath); } catch { /* ignore */ }
                }
            }
            catch
            {
                // Never block pipeline on archiving failure
            }
        }

        public void LoadFromXML()
        {
            XmlSerializer ser = new XmlSerializer(commandsList.GetType());
            if (File.Exists(xmlPath))
            {
                using (StreamReader sr = new StreamReader(xmlPath))
                {
                    commandsList = (List<CommandSet>)ser.Deserialize(sr);
                }
            }
        }
        public void SaveIntoXML(bool checkChanged = true, string path = "")
        {
            if (checkChanged && !isChanged) return;
            path = (path != "") ? path : xmlPath;
            if (File.Exists(path)) File.Delete(path);
            XmlSerializer ser = new XmlSerializer(commandsList.GetType());
            using (StreamWriter sw = new StreamWriter(path))
            {
                ser.Serialize(sw, commandsList);
            }
            isChanged = false;
        }
        public void ClearCommands()
        {
            commandsList.Clear();
        }

        public void LoadV3ConfigsLocal()
        {
            try
            {
                var folder = Program.commandsLocalPath;
                foreach (var path in Directory.GetFiles(folder, "*.mcw3.xml"))
                {
                    if (!ShouldLoadV3File(path)) continue;

                    // Remove previously loaded V3 tasks from this file (fresh reload)
                    commandsList.RemoveAll(c => string.Equals(c.SourceFilePath, path, StringComparison.OrdinalIgnoreCase));

                    foreach (var v3 in V3ConfigLoader.LoadFile(path))
                        commandsList.Add(v3);

                    isChanged = true;
                }
            }
            catch (Exception e)
            {
                Logger.WriteLog($"V3 local load failed: {e}", Logger.TypeLog.both);
            }
        }


        public void LoadV3ConfigsRemote()
        {
            try
            {
                var folder = Program.mainPath;
                foreach (var path in Directory.GetFiles(folder, "*.mcw3.xml"))
                {
                    if (!ShouldLoadV3File(path)) continue;

                    // Remove previously loaded V3 tasks from this file (fresh reload)
                    commandsList.RemoveAll(c => string.Equals(c.SourceFilePath, path, StringComparison.OrdinalIgnoreCase));

                    foreach (var v3 in V3ConfigLoader.LoadFile(path))
                        commandsList.Add(v3);

                    isChanged = true;
                }
            }
            catch (Exception e)
            {
                Logger.WriteLog($"V3 remote load failed: {e}", Logger.TypeLog.both);
            }
        }
    }
}
