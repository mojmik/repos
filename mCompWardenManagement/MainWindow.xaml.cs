using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace mCompWardenManagement
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CommandsManager comMan = new CommandsManager();
        public MainWindow()
        {
            InitializeComponent();
            SetControls();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bool saveOk = comMan.AddCommandFile()
                .SetRepeating(RepeatingText.Text, RepeatingTypeCombo.Text)
                .SetRemote((RemoteCheckBox.IsChecked == true ? true : false))
                .SetNetwork((NeedsNetworkCheckBox.IsChecked == true ? true : false))
                .SetUser((NeedsUserCheckBox.IsChecked == true ? true : false))
                .SetSystem((NeedsSystemCheckBox.IsChecked == true ? true : false))
                .SetExcluded(ExcludedComputersText.Text)
                .SetUserName(UserNameText.Text)
                .SetMachineName(MachineNameText.Text)
                .SetRunAt(RunAtText.Text)
                .SetExcludedRegex(ExcludedComputersRegexText.Text)
                .SetCommandLines(commandsBox.Text.Split('\n').ToList())
                .Save(FileNameText.Text);

            if (saveOk) MessageBox.Show("ok, written!");

            //System.Collections.Specialized.NameValueCollection data = new System.Collections.Specialized.NameValueCollection();
            //data[""]
        }
        private void SetControls(mCompWarden2.CommandSet cmdSet = null, string file = null)
        {
            if (cmdSet == null) cmdSet = new mCompWarden2.CommandSet();
            RepeatingTypeCombo.ItemsSource = comMan.repeatingTypes;
            RepeatingTypeCombo.SelectedItem = cmdSet.RepeatingType;
            RepeatingText.Text = "" + cmdSet.RepeatingInterval;
            RemoteCheckBox.IsChecked = cmdSet.IsRemote;
            NeedsNetworkCheckBox.IsChecked = cmdSet.NeedsNetwork;
            NeedsUserCheckBox.IsChecked = cmdSet.NeedsUser;
            NeedsSystemCheckBox.IsChecked = cmdSet.NeedsSystem;
            if (cmdSet.RunAt != DateTime.MinValue) RunAtText.Text = cmdSet.RunAt.ToString();
            if (cmdSet.ExcludedComputers != null) ExcludedComputersText.Text = string.Join("'", cmdSet.ExcludedComputers);
            else ExcludedComputersText.Text = "";
            ExcludedComputersRegexText.Text = cmdSet.ExcludedComputersRegex;
            if (cmdSet.CommandLines != null)
            {
                commandsBox.Text = string.Join("\n", cmdSet.CommandLines.ToArray());
            }
            else commandsBox.Text = "";
            FileNameText.Text = file;
            if (cmdSet.MachineName != "")
            {
                AllMachines.IsChecked = false;
                MachineNameText.Text = cmdSet.MachineName;
            }
            if (cmdSet.UserName != "")
            {
                UserNameText.Text = cmdSet.UserName;
                NeedsSystemCheckBox.IsChecked = false;
                NeedsUserCheckBox.IsChecked = true;
            }
            if (cmdSet.NeedsSystem)
            {
                UserNameText.Text = "";
                NeedsUserCheckBox.IsChecked = true;
                NeedsUserCheckBox.IsChecked = false;
            }
        }


        private void genFileNameButton_Click(object sender, RoutedEventArgs e)
        {
            Random rnd = new Random();
            string rndTxt = rnd.Next(10000, 99999) + "-" + rnd.Next(10000, 99999);
            string dt = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss").Replace('/', '-').Replace(':', '-').Replace(' ', '_').Replace('.', '-');
            if (MachineNameText.Text != "")
            {
                FileNameText.Text = @"\\rentex.intra\company\data\Company\mkavan_upravy\scripts\mCompWarden2\" + MachineNameText.Text + "-" + dt + "_" + rndTxt + "." + CommandsManager.FileExtension;
            }
            else
            {
                FileNameText.Text = @"\\rentex.intra\company\data\Company\mkavan_upravy\scripts\mCompWarden2\all-" + dt + "_" + rndTxt + "." + CommandsManager.FileExtension;
            }


        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            string file = "";
            string mainPath = @"\\rentex.intra\company\data\Company\mkavan_upravy\scripts\mCompWarden2\";
            Microsoft.Win32.OpenFileDialog openFileDialog1 = new Microsoft.Win32.OpenFileDialog
            {
                InitialDirectory = mainPath,
                Title = "Browse Text Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = CommandsManager.FileExtension,
                Filter = $"{CommandsManager.FileExtension} files (*.{CommandsManager.FileExtension})|*.{CommandsManager.FileExtension}",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == true)
            {
                file = openFileDialog1.FileName;
            }
            //mCompWarden2.Logger logger = new mCompWarden2.Logger();
            //mCompWarden2.CommandsManager cmdMan = new mCompWarden2.CommandsManager();
            //mCompWarden2.CommandsManager.SingleCommandSetFromFile(file);
            if (file != "")
            {
                mCompWarden2.CommandSet cmdSet = new mCompWarden2.CommandSet(file);
                SetControls(cmdSet, file);
                MessageBox.Show("ok, loaded!");
            }

        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            SetControls();
        }

        private void NeedsUserCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            NeedsSystemCheckBox.IsChecked = false;
        }

        private void NeedsSystemCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            NeedsUserCheckBox.IsChecked = false;
        }
    }
}
