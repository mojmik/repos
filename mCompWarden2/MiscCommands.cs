using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace mCompWarden2 {
    static class MiscCommands {
        public static CommandsManager cmdMan { get; set; }
        
        private static void FullScreenshot(string filePath, ImageFormat format) {
            Rectangle bounds = Screen.GetBounds(Point.Empty);
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height)) {
                using (Graphics g = Graphics.FromImage(bitmap)) {
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
                }
                bitmap.Save(filePath, format);
            }
        }
        public static void SaveScreenshot(string filePath) {
            //black screen
            FullScreenshot(filePath, ImageFormat.Jpeg);

            //taky black screen
            //ScreenCapture.SaveScreenShot(filePath);
        }
        public static void ShowMessage(string message, string messageTitle) {
            new System.Threading.Thread(new System.Threading.ThreadStart(delegate
            {
                MessageBox.Show
                (
                  message,
                  messageTitle,
                  MessageBoxButtons.OK,
                  MessageBoxIcon.Warning
                );
            })).Start();
        }
        public static void ListCommands() {
            cmdMan.SaveIntoXML(false, Program.outPath + "commands.xml");
        }
        public static void ClearCommands() {
            cmdMan.ClearCommands();
        }
        public static void PostMessage(string widget, string message) {
            cmdMan.logger.WriteRemoteInfo(widget, message);
        }
    }
}
