using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Xml.Linq;

namespace mCompWardenTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==================================================");
            Console.WriteLine(" mCompWarden2 Un-logged User Execution Test Tool  ");
            Console.WriteLine("==================================================");

            string xmlPath = args.Length > 0 ? args[0] : "";

            if (string.IsNullOrWhiteSpace(xmlPath) || !File.Exists(xmlPath))
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string testSubfolder = Path.Combine(baseDir, "test");
                if (Directory.Exists(testSubfolder))
                {
                    string[] xmlFiles = Directory.GetFiles(testSubfolder, "*.xml");
                    if (xmlFiles.Length > 0)
                    {
                        xmlPath = xmlFiles[0];
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(xmlPath) || !File.Exists(xmlPath))
            {
                Console.Write("Enter XML Config Path: ");
                xmlPath = Console.ReadLine()?.Trim('"');
            }

            if (!File.Exists(xmlPath))
            {
                Console.WriteLine($"ERROR: XML file not found at '{xmlPath}'");
                return;
            }

            Console.WriteLine($"Loading XML: {xmlPath}");
            try
            {
                XDocument doc = XDocument.Load(xmlPath);
                var task = doc.Root?.Element("Task");
                if (task == null)
                {
                    Console.WriteLine("ERROR: No <Task> element found in XML.");
                    return;
                }

                string id = (string)task.Attribute("id") ?? "";
                string user = (string)task.Attribute("user") ?? "";
                string domain = (string)task.Attribute("domain") ?? "";
                string passwordEnc = (string)task.Attribute("passwordEnc") ?? (string)task.Attribute("password") ?? "";
                string workDir = (string)task.Attribute("workDir") ?? "";

                var action = task.Element("Actions")?.Element("Action");
                string file = (string)action?.Attribute("file") ?? "";
                string actionArgs = (string)action?.Attribute("args") ?? "";

                Console.WriteLine($"Task ID    : {id}");
                Console.WriteLine($"User       : {(string.IsNullOrWhiteSpace(domain) ? "" : domain + "\\")}{user}");
                Console.WriteLine($"WorkDir    : {workDir}");
                Console.WriteLine($"File       : {file}");
                Console.WriteLine($"Args       : {actionArgs}");

                string plainPassword = Decrypt(passwordEnc);
                Console.WriteLine($"Password   : {(string.IsNullOrWhiteSpace(plainPassword) ? "[FAILED TO DECRYPT]" : "[DECRYPTED OK]")}");

                if (string.IsNullOrWhiteSpace(plainPassword))
                {
                    Console.WriteLine("ERROR: Cannot proceed without decrypted password.");
                    return;
                }

                Console.WriteLine("\n--- Launching Test Process ---");
                TestLaunch(domain, user, plainPassword, file, actionArgs, workDir);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION: {ex}");
            }

            Console.WriteLine("\nPress Enter to exit...");
            Console.ReadLine();
        }

        #region Win32 API Definitions

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool LogonUser(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out IntPtr phToken);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CreateProcessAsUser(
            IntPtr hToken,
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CreateProcessWithTokenW(
            IntPtr hToken,
            uint dwLogonFlags,
            string lpApplicationName,
            string lpCommandLine,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool ImpersonateLoggedOnUser(IntPtr hToken);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool RevertToSelf();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        private static extern int WNetAddConnection2(ref NETRESOURCE lpNetResource, string lpPassword, string lpUsername, uint dwFlags);

        private const int LOGON32_LOGON_INTERACTIVE = 2;
        private const int LOGON32_LOGON_BATCH = 4;
        private const int LOGON32_LOGON_SERVICE = 5;
        private const int LOGON32_LOGON_NETWORK_CLEARTEXT = 8;
        private const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private const uint LOGON_WITH_PROFILE = 0x00000001;
        private const uint CREATE_NO_WINDOW = 0x08000000;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NETRESOURCE
        {
            public uint dwScope;
            public uint dwType;
            public uint dwDisplayType;
            public uint dwUsage;
            public string lpLocalName;
            public string lpRemoteName;
            public string lpComment;
            public string lpProvider;
        }

        #endregion

        private static void TestLaunch(string domain, string username, string password, string file, string args, string workDir)
        {
            string targetDomain = string.IsNullOrWhiteSpace(domain) ? "." : domain;
            IntPtr hToken = IntPtr.Zero;

            // LOGON32_LOGON_BATCH (4) first so process executes locally under target user identity (srv)
            int[] logonTypes = new int[]
            {
                LOGON32_LOGON_BATCH,
                LOGON32_LOGON_SERVICE,
                LOGON32_LOGON_INTERACTIVE,
                LOGON32_LOGON_NETWORK_CLEARTEXT,
                LOGON32_LOGON_NEW_CREDENTIALS
            };

            int lastError = 0;
            bool loggedOn = false;
            int usedLogonType = 0;

            foreach (int lt in logonTypes)
            {
                loggedOn = LogonUser(username, targetDomain, password, lt, LOGON32_PROVIDER_DEFAULT, out hToken);
                if (loggedOn)
                {
                    usedLogonType = lt;
                    Console.WriteLine($"[LOGON OK] LogonUser succeeded using LogonType={lt}");
                    break;
                }
                lastError = Marshal.GetLastWin32Error();
                Console.WriteLine($"[LOGON FAIL] LogonType={lt} failed (Win32 Error {lastError})");
            }

            if (!loggedOn)
            {
                Console.WriteLine($"[ERROR] All LogonUser attempts failed. Final Win32 Error: {lastError}");
                return;
            }

            try
            {
                // Authenticate UNC share inside impersonated user token context
                if (ImpersonateLoggedOnUser(hToken))
                {
                    Console.WriteLine("[IMPERSONATE OK] Impersonated user token for WNetAddConnection2");
                    if (!string.IsNullOrWhiteSpace(workDir) && workDir.StartsWith(@"\\"))
                    {
                        string[] parts = workDir.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2)
                        {
                            string uncShare = @"\\" + parts[0] + @"\" + parts[1];
                            string fullUser = targetDomain + @"\" + username;
                            var nr = new NETRESOURCE { dwType = 1, lpRemoteName = uncShare };
                            int wres = WNetAddConnection2(ref nr, password, fullUser, 0);
                            Console.WriteLine($"[WNetAddConnection2] {uncShare} returned code {wres}");
                        }
                    }
                    RevertToSelf();
                }

                var si = new STARTUPINFO();
                si.cb = Marshal.SizeOf(si);

                bool isBatchScript = file.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase);
                bool isCmdExe = file.Equals("cmd.exe", StringComparison.OrdinalIgnoreCase) || file.EndsWith(@"\cmd.exe", StringComparison.OrdinalIgnoreCase);

                string cmdLine;
                if (!string.IsNullOrWhiteSpace(workDir) && workDir.StartsWith(@"\\"))
                {
                    string targetCall;
                    if (isCmdExe)
                    {
                        targetCall = args;
                    }
                    else
                    {
                        string relFile = file;
                        if (file.StartsWith(workDir, StringComparison.OrdinalIgnoreCase))
                        {
                            relFile = file.Substring(workDir.Length).TrimStart('\\');
                        }
                        if (!relFile.Contains("\\") && !relFile.Contains(":") && !relFile.StartsWith("."))
                        {
                            relFile = @".\" + relFile;
                        }
                        targetCall = (relFile.Contains(" ") ? $"\"{relFile}\"" : relFile) + (string.IsNullOrWhiteSpace(args) ? "" : " " + args);
                    }

                    cmdLine = $"cmd.exe /c \"pushd \"{workDir}\" && {targetCall}\"";
                }
                else if (isBatchScript)
                {
                    string batchCall = (file.Contains(" ") ? $"\"{file}\"" : file) + (string.IsNullOrWhiteSpace(args) ? "" : " " + args);
                    cmdLine = $"cmd.exe /c \"{batchCall}\"";
                }
                else
                {
                    cmdLine = (file.Contains(" ") ? $"\"{file}\"" : file) + (string.IsNullOrWhiteSpace(args) ? "" : " " + args);
                }

                Console.WriteLine($"[EXECUTE] CommandLine: {cmdLine}");

                PROCESS_INFORMATION pi;
                bool created = CreateProcessAsUser(hToken, null, cmdLine, IntPtr.Zero, IntPtr.Zero, false, 0, IntPtr.Zero, null, ref si, out pi);
                if (created)
                {
                    Console.WriteLine("[CREATE OK] CreateProcessAsUser succeeded!");
                }
                else
                {
                    int err0 = Marshal.GetLastWin32Error();
                    Console.WriteLine($"[FAIL] CreateProcessAsUser failed (Error {err0}). Trying CreateProcessWithTokenW...");
                    created = CreateProcessWithTokenW(hToken, LOGON_WITH_PROFILE, null, cmdLine, 0, IntPtr.Zero, null, ref si, out pi);
                    if (!created)
                    {
                        int err = Marshal.GetLastWin32Error();
                        Console.WriteLine($"[FAIL] CreateProcessWithTokenW with LOGON_WITH_PROFILE failed (Error {err}). Trying without profile...");
                        created = CreateProcessWithTokenW(hToken, 0, null, cmdLine, 0, IntPtr.Zero, null, ref si, out pi);
                    }
                }

                if (!created)
                {
                    int err2 = Marshal.GetLastWin32Error();
                    Console.WriteLine($"[FAIL] All process creation attempts failed (Error {err2}).");
                    return;
                }

                Console.WriteLine($"[SUCCESS] Process created successfully! PID: {pi.dwProcessId}");
                CloseHandle(pi.hProcess);
                CloseHandle(pi.hThread);
            }
            finally
            {
                if (hToken != IntPtr.Zero) CloseHandle(hToken);
            }
        }

        #region Decryption Helpers

        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("mCompWardenV3SecretEntropy");
        private static readonly byte[] AesKey = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes("mCompWardenSharedSecretKey2026!"));
        private static readonly byte[] AesIV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF, 0xfe, 0xdc, 0xba, 0x09, 0x87, 0x65, 0x43, 0x21 };

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return "";

            if (cipherText.StartsWith("aes:", StringComparison.OrdinalIgnoreCase))
            {
                string base64 = cipherText.Substring(4);
                return DecryptAes(base64);
            }

            string aesDec = DecryptAes(cipherText);
            if (!string.IsNullOrEmpty(aesDec)) return aesDec;

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                byte[] plainBytes = ProtectedData.Unprotect(cipherBytes, Entropy, DataProtectionScope.LocalMachine);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch { }

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                byte[] plainBytes = ProtectedData.Unprotect(cipherBytes, null, DataProtectionScope.LocalMachine);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch { }

            return "";
        }

        private static string DecryptAes(string cipherBase64)
        {
            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherBase64);
                using (var aes = Aes.Create())
                {
                    aes.Key = AesKey;
                    aes.IV = AesIV;
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.FlushFinalBlock();
                        }
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
            catch
            {
                return "";
            }
        }

        #endregion
    }
}
