using System;
using System.IO;
using System.Text;

namespace ConvertFileEncoding {
    class Program {
        public static void ConvertFileEncoding(String sourcePath, String destPath, bool deleteSource=false) {
            const int WindowsCodepage1252 = 1252;
            //var codepage1252Encoding = Encoding.GetEncoding(WindowsCodepage1252, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
            //var codepage1252Encoding = CodePagesEncodingProvider.Instance.GetEncoding(1252);            

            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            //iso = CodePagesEncodingProvider.Instance.GetEncoding("ISO-8859-2");
            iso = CodePagesEncodingProvider.Instance.GetEncoding(852); //nav pracuje s cp852
            iso = CodePagesEncodingProvider.Instance.GetEncoding(1250);
            Encoding utf8 = Encoding.UTF8;


            // If the destination's parent doesn't exist, create it.
            String parent = Path.GetDirectoryName(Path.GetFullPath(destPath));
            if (!Directory.Exists(parent)) {
                Directory.CreateDirectory(parent);
            }

            string tempName = null;
            tempName = Path.GetTempFileName();
            string allText = File.ReadAllText(sourcePath, Encoding.UTF8);

            File.Delete(destPath);
            File.WriteAllText(destPath, allText, iso);
            if (deleteSource) File.Delete(sourcePath);


        }

        public static bool isFolder(string path) {
            FileAttributes attr = File.GetAttributes(path);

            //detect whether its a directory or file
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory) return true;
            else return false;
        }

        public static string AddSuffix(string filename, string suffix) {
            string fDir = Path.GetDirectoryName(filename);
            string fName = Path.GetFileNameWithoutExtension(filename);
            string fExt = Path.GetExtension(filename);
            return Path.Combine(fDir, String.Concat(fName, suffix, fExt));
        }

        static void Main(string[] args) {

            //string srcPath = @"\\rentex.intra\company\data\Company\mkavan_upravy\navision-utils\hertz247\ready-test\dbg\5693536-21-12-2021_1_463_20210311101557_HOD_WF_RA_HERTZ-CR-20210311.csv";
            //string dstPath = @"\\rentex.intra\company\data\Company\mkavan_upravy\navision-utils\hertz247\ready-test\dbg\5693536-21-12-2021_1_463_20210311101557_HOD_WF_RA_HERTZ-CR-20210311-iso.csv";

            string srcPath = @"\\rentex.intra\company\data\Company\mkavan_upravy\navision-utils\hertz247\downloaded\utf8\";
            string dstPath = @"\\rentex.intra\company\data\Company\mkavan_upravy\navision-utils\hertz247\downloaded\";

            if (args.Length >= 2) {
                srcPath = args[0];
                dstPath = args[1];
            }

            //convert encoding from utf8 to cp852 (fNAV) 
            if (isFolder(srcPath)) {
                //convert everything in folder
                string[] fileEntries = Directory.GetFiles(srcPath);
                foreach (string filePath in fileEntries) {
                    string fnOnly= Path.GetFileName(filePath);
                    string dstFile = AddSuffix(dstPath + fnOnly, "-iso");
                    ConvertFileEncoding(filePath, dstFile);
                    File.Move(filePath, srcPath + @"bck\" + fnOnly,true);
                }
            } else {
                //convert single file
                ConvertFileEncoding(srcPath, dstPath);
                Console.WriteLine($"Econfing utf8 {srcPath} to cp852 {dstPath} ok");
            }

            


            


        }
    }
}
