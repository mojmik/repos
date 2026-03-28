using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace mCompWardenManagement
{
    public partial class MainWindow : Window
    {
        // Base folder for saving files (you already had this)
        static string folder = @"\\rentex.intra\company\mkavan_upravy\scripts\mCompWarden2";

        // NEW: computers export (OU → host list) source file
        private const string ComputersExportPath =
            @"\\rentex.intra\company\hertz_czsk\IT Service CZ & SK\IT Procedures & Bulletins\IT only\evidence pocitacu\Computers-Export.txt";

        // NEW: OU index built from Computers-Export.txt → key = "OU-OU-...", value = host list
        private readonly Dictionary<string, List<string>> _ouIndex = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        private bool _suppressSelectionChanged;

        // --- V2 support you already had ---
        CommandsManager comMan = new CommandsManager();

        // --- V3 in-memory model for the multi-task editor ---
        private readonly List<V3TaskModel> _v3Tasks = new List<V3TaskModel>();
        private V3TaskModel _current; // currently selected task in the UI

        public MainWindow()
        {
            InitializeComponent();
            SetControls();

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Safe defaults
            if (FileTypeCombo.SelectedItem == null) FileTypeCombo.SelectedIndex = 0;
            if (HourCombo.SelectedItem == null && HourCombo.Items.Count > 0) HourCombo.SelectedIndex = 8;   // 08:00
            if (MinuteCombo.SelectedItem == null && MinuteCombo.Items.Count > 0) MinuteCombo.SelectedIndex = 0;
            if (HourlyMinuteCombo.SelectedItem == null && HourlyMinuteCombo.Items.Count > 0) HourlyMinuteCombo.SelectedIndex = 0;

            // Minutely default to "5" if present
            ComboBox mic = FindName("MinutelyIntervalCombo") as ComboBox;
            if (mic != null)
            {
                if (mic.SelectedItem == null && mic.Items.Count > 0)
                {
                    SelectComboByText(mic, "5");
                    if (mic.SelectedIndex < 0) mic.SelectedIndex = 0;
                }
            }

            UpdatePanelsFromFileType();
            UpdateV3Blocks();
            RefreshTaskList();

            // NEW: load OU list into the ComboBox (OuCombo)
            LoadOuList();
        }

        // ============================================================
        //  Top-level V2 / V3 panel switching
        // ============================================================
        private void FileTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;
            UpdatePanelsFromFileType();
        }

        private void UpdatePanelsFromFileType()
        {
            // Late-bind in case fields were not generated yet
            if (V2Panel == null) V2Panel = this.FindName("V2Panel") as StackPanel;
            if (V2Scroller == null) V2Scroller = this.FindName("V2Scroller") as ScrollViewer;
            if (V3Grid == null) V3Grid = this.FindName("V3Grid") as Grid;
            if (V3RootBar == null) V3RootBar = this.FindName("V3RootBar") as StackPanel;

            if (FileTypeCombo == null || V2Panel == null || V2Scroller == null || V3Grid == null || V3RootBar == null)
                return;

            var tag = GetSelectedTag(FileTypeCombo);
            bool v2 = string.Equals(tag, "v2", StringComparison.OrdinalIgnoreCase);

            V2Panel.Visibility = v2 ? Visibility.Visible : Visibility.Collapsed;
            V2Scroller.Visibility = v2 ? Visibility.Visible : Visibility.Collapsed;

            bool v3 = !v2;
            V3Grid.Visibility = v3 ? Visibility.Visible : Visibility.Collapsed;
            V3RootBar.Visibility = v3 ? Visibility.Visible : Visibility.Collapsed;
        }

        // ============================================================
        //  V3 Schedule controls toggling
        // ============================================================
        private void V3ScheduleTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            UpdateV3Blocks();
        }

        private void UpdateV3Blocks()
        {
            if (!IsLoaded) return;

            // Late-bind controls in case the fields aren't ready yet
            if (V3ScheduleTypeCombo == null) V3ScheduleTypeCombo = FindName("V3ScheduleTypeCombo") as ComboBox;
            if (WeeklyBlock == null) WeeklyBlock = FindName("WeeklyBlock") as StackPanel;
            if (MonthlyBlock == null) MonthlyBlock = FindName("MonthlyBlock") as StackPanel;
            if (HourlyBlock == null) HourlyBlock = FindName("HourlyBlock") as StackPanel;
            if (TimeBlock == null) TimeBlock = FindName("TimeBlock") as StackPanel;
            if (HourCombo == null) HourCombo = FindName("HourCombo") as ComboBox;
            if (MinuteCombo == null) MinuteCombo = FindName("MinuteCombo") as ComboBox;
            if (HourlyMinuteCombo == null) HourlyMinuteCombo = FindName("HourlyMinuteCombo") as ComboBox;
            StackPanel mb = FindName("MinutelyBlock") as StackPanel; if (mb != null) MinutelyBlock = mb;
            ComboBox mic = FindName("MinutelyIntervalCombo") as ComboBox; if (mic != null) MinutelyIntervalCombo = mic;

            if (V3ScheduleTypeCombo == null || WeeklyBlock == null || MonthlyBlock == null ||
                HourlyBlock == null || TimeBlock == null)
                return;

            var tag = GetSelectedTag(V3ScheduleTypeCombo);
            if (string.IsNullOrEmpty(tag))
            {
                if (V3ScheduleTypeCombo.Items.Count > 0) V3ScheduleTypeCombo.SelectedIndex = 0;
                tag = "daily";
            }
            bool isOnce = tag.Equals("once", StringComparison.OrdinalIgnoreCase);
            bool isWeekly = tag.Equals("weekly", StringComparison.OrdinalIgnoreCase);
            bool isMonthly = tag.Equals("monthly", StringComparison.OrdinalIgnoreCase);
            bool isHourly = tag.Equals("hourly", StringComparison.OrdinalIgnoreCase);
            bool isMinutely = tag.Equals("minutely", StringComparison.OrdinalIgnoreCase);

            WeeklyBlock.Visibility = isWeekly ? Visibility.Visible : Visibility.Collapsed;
            MonthlyBlock.Visibility = isMonthly ? Visibility.Visible : Visibility.Collapsed;
            HourlyBlock.Visibility = isHourly ? Visibility.Visible : Visibility.Collapsed;
            if (MinutelyBlock != null) MinutelyBlock.Visibility = isMinutely ? Visibility.Visible : Visibility.Collapsed;

            TimeBlock.Visibility = (isHourly || isMinutely || isOnce) ? Visibility.Collapsed : Visibility.Visible;

            if (isHourly)
            {
                if (HourlyMinuteCombo != null && HourlyMinuteCombo.SelectedIndex < 0 && HourlyMinuteCombo.Items.Count > 0)
                    HourlyMinuteCombo.SelectedIndex = 0;
            }
            else if (isMinutely)
            {
                if (MinutelyIntervalCombo != null)
                {
                    if (MinutelyIntervalCombo.SelectedIndex < 0 && MinutelyIntervalCombo.Items.Count > 0)
                    {
                        SelectComboByText(MinutelyIntervalCombo, "5");
                        if (MinutelyIntervalCombo.SelectedIndex < 0) MinutelyIntervalCombo.SelectedIndex = 0;
                    }
                }
            }
            else
            {
                if (HourCombo != null && HourCombo.SelectedIndex < 0 && HourCombo.Items.Count > 0) HourCombo.SelectedIndex = 8;
                if (MinuteCombo != null && MinuteCombo.SelectedIndex < 0 && MinuteCombo.Items.Count > 0) MinuteCombo.SelectedIndex = 0;
            }
        }

        private static void SelectByTag(ComboBox combo, string tag)
        {
            if (combo == null) return;
            for (int i = 0; i < combo.Items.Count; i++)
            {
                ComboBoxItem cbi = combo.Items[i] as ComboBoxItem;
                if (cbi != null)
                {
                    string t = (cbi.Tag as string) ?? "";
                    if (string.Equals(t, tag, StringComparison.OrdinalIgnoreCase))
                    {
                        combo.SelectedIndex = i;
                        return;
                    }
                }
            }
            if (combo.Items.Count > 0) combo.SelectedIndex = 0;
        }

        private static void SelectByText(ComboBox combo, string text)
        {
            if (combo == null) return;
            for (int i = 0; i < combo.Items.Count; i++)
            {
                ComboBoxItem cbi = combo.Items[i] as ComboBoxItem;
                if (cbi != null)
                {
                    string s = cbi.Content != null ? cbi.Content.ToString() : "";
                    if (string.Equals(s, text, StringComparison.OrdinalIgnoreCase))
                    {
                        combo.SelectedIndex = i;
                        return;
                    }
                }
            }
            if (combo.Items.Count > 0) combo.SelectedIndex = 0;
        }

        // ============================================================
        //  V3 Task list operations
        // ============================================================
        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            PushUIIntoCurrent();

            var t = new V3TaskModel
            {
                V3Id = SuggestNewId(),
                Enabled = true,
                Description = "",
                ScheduleType = "daily",
                TimeHHmm = "08:00",
                WeeklyDays = new List<string>(),
                MonthlyDay = null,
                HourlyMinute = 0,
                MinutelyInterval = 5
            };
            t.Actions.Add(new V3ActionModel { Type = "RunProgram" });

            _v3Tasks.Add(t);
            RefreshTaskList();
            SelectTask(t);
        }

        private void RemoveTask_Click(object sender, RoutedEventArgs e)
        {
            if (_current == null) return;
            int idx = _v3Tasks.IndexOf(_current);
            if (idx < 0) return;

            _v3Tasks.RemoveAt(idx);
            _current = null;
            RefreshTaskList();

            if (_v3Tasks.Count > 0)
            {
                int newIdx = Math.Max(0, idx - 1);
                SelectTask(_v3Tasks[newIdx]);
            }
            else
            {
                ClearTaskUI();
            }
        }

        private void DuplicateTask_Click(object sender, RoutedEventArgs e)
        {
            if (_current == null) return;
            PushUIIntoCurrent();

            var clone = _current.Clone();
            clone.V3Id = SuggestNewId(_current.V3Id + "_copy");
            _v3Tasks.Add(clone);
            RefreshTaskList();
            SelectTask(clone);
        }

        private void TaskList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressSelectionChanged) return;

            // Persist edits from the previously selected task
            if (_current != null) PushUIIntoCurrent();

            var sel = TaskList.SelectedItem as V3TaskModel;
            _current = sel;
            PopulateTaskUI(sel);
        }
        private static string SanitizeFilePart(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "all";
            foreach (var ch in System.IO.Path.GetInvalidFileNameChars())
                s = s.Replace(ch, '_');
            return s.Trim();
        }

        private string GetTargetMachineForV2()
        {
            var m = (MachineNameText != null ? MachineNameText.Text : null) ?? "";
            if (string.IsNullOrWhiteSpace(m)) m = "all";        // <= default to ALL
            return SanitizeFilePart(m.ToLowerInvariant());
        }

        private string GetTargetMachineForV3()
        {
            var m = (V3MachineText != null ? V3MachineText.Text : null) ?? "";
            if (string.IsNullOrWhiteSpace(m)) m = "all";        // <= default to ALL
            return SanitizeFilePart(m.ToLowerInvariant());
        }

        private string BuildSuggestedV3Name()
        {
            string machine = GetTargetMachineForV3();
            string dt = DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss");
            var rnd = new Random();
            string rndTxt = rnd.Next(10000, 99999) + "-" + rnd.Next(10000, 99999);
            return System.IO.Path.Combine(folder, $"{machine}-{dt}_{rndTxt}.mcw3.xml");
        }

        private void SelectTask(V3TaskModel t)
        {
            // Avoid PushUIIntoCurrent running while we change selection
            _suppressSelectionChanged = true;
            try
            {
                TaskList.SelectedItem = t;
            }
            finally
            {
                _suppressSelectionChanged = false;
            }

            _current = t;
            PopulateTaskUI(t);
        }

        private void RefreshTaskList()
        {
            TaskList.ItemsSource = null;
            TaskList.ItemsSource = _v3Tasks;
        }

        private string SuggestNewId(string baseId = "task")
        {
            string id = baseId;
            int n = 1;
            var ids = new HashSet<string>(_v3Tasks.Select(x => x.V3Id), StringComparer.OrdinalIgnoreCase);
            while (ids.Contains(id))
            {
                n++;
                id = baseId + n;
            }
            return id;
        }

        // ============================================================
        //  V3 Actions grid operations
        // ============================================================
        private void AddAction_Click(object sender, RoutedEventArgs e)
        {
            if (_current == null) return;
            PushUIIntoCurrent();
            _current.Actions.Add(new V3ActionModel { Type = "RunProgram" });
            BindActionsGrid(_current);
        }

        private void RemoveAction_Click(object sender, RoutedEventArgs e)
        {
            if (_current == null) return;
            V3ActionModel row = V3ActionsGrid.SelectedItem as V3ActionModel;
            if (row != null)
            {
                _current.Actions.Remove(row);
                BindActionsGrid(_current);
            }
        }

        private void MoveActionUp_Click(object sender, RoutedEventArgs e)
        {
            if (_current == null) return;
            V3ActionModel row = V3ActionsGrid.SelectedItem as V3ActionModel;
            if (row != null)
            {
                int i = _current.Actions.IndexOf(row);
                if (i > 0)
                {
                    _current.Actions.RemoveAt(i);
                    _current.Actions.Insert(i - 1, row);
                    BindActionsGrid(_current);
                    V3ActionsGrid.SelectedItem = row;
                }
            }
        }

        private void MoveActionDown_Click(object sender, RoutedEventArgs e)
        {
            if (_current == null) return;
            V3ActionModel row = V3ActionsGrid.SelectedItem as V3ActionModel;
            if (row != null)
            {
                int i = _current.Actions.IndexOf(row);
                if (i >= 0 && i < _current.Actions.Count - 1)
                {
                    _current.Actions.RemoveAt(i);
                    _current.Actions.Insert(i + 1, row);
                    BindActionsGrid(_current);
                    V3ActionsGrid.SelectedItem = row;
                }
            }
        }

        // ============================================================
        //  SAVE (V2 & V3)
        // ============================================================
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var tag = GetSelectedTag(FileTypeCombo);

            if (tag == "v2")
            {
                bool saveOk = comMan.AddCommandFile()
                    .SetRepeating(RepeatingText.Text, (RepeatingTypeCombo.SelectedItem as string) ?? RepeatingTypeCombo.Text)
                    .SetRemote((RemoteCheckBox.IsChecked == true))
                    .SetNetwork((NeedsNetworkCheckBox.IsChecked == true))
                    .SetUser((NeedsUserCheckBox.IsChecked == true))
                    .SetSystem((NeedsSystemCheckBox.IsChecked == true))
                    .SetExcluded(ExcludedComputersText.Text)
                    .SetUserName(UserNameText.Text)
                    .SetMachineName(MachineNameText.Text)
                    .SetRunAt(RunAtText.Text)
                    .SetExcludedRegex(ExcludedComputersRegexText.Text)
                    .SetCommandLines(commandsBox.Text.Split('\n').ToList())
                    .Save(EnsureV2Extension(FileNameText.Text));

                if (saveOk) MessageBox.Show("Soubor V2 uložen!");
            }
            else
            {
                // V3 save (multi-task) — with OU expansion (always recursive)
                PushUIIntoCurrent();

                if (string.IsNullOrWhiteSpace(FileNameText.Text))
                {
                    var suggested = BuildSuggestedV3Name();
                    var dlg = new Microsoft.Win32.SaveFileDialog
                    {
                        Title = "Uložit V3 konfiguraci",
                        Filter = "V3 XML (*.mcw3.xml)|*.mcw3.xml",
                        FileName = System.IO.Path.GetFileName(suggested),
                        InitialDirectory = System.IO.Path.GetDirectoryName(suggested)
                    };
                    bool? ok = dlg.ShowDialog();
                    if (ok == true)
                        FileNameText.Text = dlg.FileName;
                    else
                        return;
                }

                string basePath = EnsureV3Extension(FileNameText.Text);

                try
                {
                    string dir = System.IO.Path.GetDirectoryName(basePath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    // Prefer OU expansion if an OU is selected
                    ComboBox ouCombo = OuCombo as ComboBox; // must exist in XAML
                    string selectedOu = (ouCombo != null && ouCombo.SelectedItem != null) ? ouCombo.SelectedItem as string : null;
                    bool useOuExpansion = !string.IsNullOrWhiteSpace(selectedOu);

                    if (!useOuExpansion)
                    {
                        // Normal single file save
                        var doc = BuildV3Document(DefaultTimezoneText.Text != null ? DefaultTimezoneText.Text.Trim() : "", _v3Tasks);
                        if (File.Exists(basePath)) File.Delete(basePath);
                        doc.Save(basePath);
                        MessageBox.Show("Soubor V3 uložen!");
                    }
                    else
                    {
                        // Expand OU → recursive host list; one file per host with machine=<host>
                        List<string> hosts = GetHostsInOuRecursive(selectedOu);
                        if (hosts.Count == 0)
                        {
                            MessageBox.Show("Žádné počítače v této OU.");
                            return;
                        }

                        int okCount = 0, failCount = 0;
                        foreach (string host in hosts)
                        {
                            // clone tasks per host, stamp machine
                            List<V3TaskModel> cloned = new List<V3TaskModel>();
                            foreach (var t in _v3Tasks)
                            {
                                var c = t.Clone();
                                c.MachineName = host;
                                cloned.Add(c);
                            }

                            var doc = BuildV3Document(DefaultTimezoneText.Text != null ? DefaultTimezoneText.Text.Trim() : "", cloned);
                            string path = BuildPerHostFileName(basePath, host);

                            try
                            {
                                if (File.Exists(path)) File.Delete(path);
                                doc.Save(path);
                                okCount++;
                            }
                            catch
                            {
                                failCount++;
                            }
                        }

                        MessageBox.Show("Hotovo: uloženo " + okCount + " souborů, selhalo " + failCount + ".");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Chyba při ukládání V3: " + ex.Message);
                }
            }
        }

        // ============================================================
        //  LOAD (V2 & V3)
        // ============================================================
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            string file = "";
            Microsoft.Win32.OpenFileDialog openFileDialog1 = new Microsoft.Win32.OpenFileDialog
            {
                InitialDirectory = @"\\rentex.intra\company\mkavan_upravy\scripts\mCompWarden2",
                Title = "Vyber soubor",
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "V2 files (*.cwd)|*.cwd|V3 XML (*.mcw3.xml)|*.mcw3.xml|XML files (*.xml)|*.xml|All files (*.*)|*.*",
                RestoreDirectory = true,
                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == true)
                file = openFileDialog1.FileName;

            if (string.IsNullOrEmpty(file)) return;

            if (file.EndsWith(".cwd", StringComparison.OrdinalIgnoreCase))
            {
                var cmdSet = new mCompWarden2.CommandSet(file);
                SetControls(cmdSet, file);
                FileTypeCombo.SelectedIndex = 0;
                MessageBox.Show("Soubor V2 načten!");
            }
            else
            {
                try
                {
                    var tuple = ParseV3Document(file);
                    string tz = tuple.DefaultTimezone;
                    List<V3TaskModel> tasks = tuple.Tasks;

                    DefaultTimezoneText.Text = tz ?? "";
                    _v3Tasks.Clear();
                    _v3Tasks.AddRange(tasks);
                    RefreshTaskList();
                    FileNameText.Text = file;
                    FileTypeCombo.SelectedIndex = 1;
                    if (_v3Tasks.Count > 0)
                        SelectTask(_v3Tasks[0]);
                    else
                        ClearTaskUI();

                    MessageBox.Show("Soubor V3 načten!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Chyba při čtení V3: " + ex.Message);
                }
            }
        }

        // ============================================================
        //  Clear
        // ============================================================
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            // V2 reset
            SetControls();
            // V3 reset
            _v3Tasks.Clear();
            RefreshTaskList();
            ClearTaskUI();
            DefaultTimezoneText.Text = "";

            if (HourCombo.Items.Count > 0) HourCombo.SelectedIndex = 8;
            if (MinuteCombo.Items.Count > 0) MinuteCombo.SelectedIndex = 0;
            if (HourlyMinuteCombo.Items.Count > 0) HourlyMinuteCombo.SelectedIndex = 0;

            if (MinutelyIntervalCombo != null)
            {
                SelectComboByText(MinutelyIntervalCombo, "5");
                if (MinutelyIntervalCombo.SelectedIndex < 0 && MinutelyIntervalCombo.Items.Count > 0)
                    MinutelyIntervalCombo.SelectedIndex = 0;
            }

            V3ScheduleTypeCombo.SelectedIndex = 0;
            UpdateV3Blocks();
        }

        // ============================================================
        //  V2 bits you already had (unchanged)
        // ============================================================
        private void NeedsUserCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            NeedsSystemCheckBox.IsChecked = false;
        }

        private void NeedsSystemCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            NeedsUserCheckBox.IsChecked = false;
        }

        private void genFileNameButton_Click(object sender, RoutedEventArgs e)
        {
            var tag = GetSelectedTag(FileTypeCombo);
            string machine = string.Equals(tag, "v3", StringComparison.OrdinalIgnoreCase)
                ? GetTargetMachineForV3()
                : GetTargetMachineForV2();

            string dt = DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss");
            var rnd = new Random();
            string rndTxt = rnd.Next(10000, 99999) + "-" + rnd.Next(10000, 99999);

            string ext = string.Equals(tag, "v3", StringComparison.OrdinalIgnoreCase) ? "mcw3.xml" : "cwd";
            FileNameText.Text = System.IO.Path.Combine(folder, $"{machine}-{dt}_{rndTxt}.{ext}");
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
            ExcludedComputersText.Text = cmdSet.ExcludedComputers != null ? string.Join(",", cmdSet.ExcludedComputers) : "";
            ExcludedComputersRegexText.Text = cmdSet.ExcludedComputersRegex ?? "";
            commandsBox.Text = cmdSet.CommandLines != null ? string.Join("\n", cmdSet.CommandLines.ToArray()) : "";
            FileNameText.Text = file ?? "";
            if (!string.IsNullOrEmpty(cmdSet.MachineName))
            {
                AllMachines.IsChecked = false;
                MachineNameText.Text = cmdSet.MachineName;
            }
            if (!string.IsNullOrEmpty(cmdSet.UserName))
            {
                UserNameText.Text = cmdSet.UserName;
                NeedsSystemCheckBox.IsChecked = false;
                NeedsUserCheckBox.IsChecked = true;
            }
            if (cmdSet.NeedsSystem)
            {
                UserNameText.Text = "";
                NeedsUserCheckBox.IsChecked = false;
                NeedsSystemCheckBox.IsChecked = true;
            }
        }

        // ============================================================
        //  Helpers
        // ============================================================
        private string EnsureV2Extension(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            if (!path.EndsWith(".cwd", StringComparison.OrdinalIgnoreCase))
                path += path.EndsWith(".") ? "cwd" : ".cwd";
            return path;
        }

        private string EnsureV3Extension(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            if (!path.EndsWith(".mcw3.xml", StringComparison.OrdinalIgnoreCase))
            {
                if (path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    path = path.Substring(0, path.Length - 4);
                path += ".mcw3.xml";
            }
            return path;
        }

        private string GetSelectedTag(ComboBox combo)
        {
            var item = combo != null ? combo.SelectedItem as ComboBoxItem : null;
            return item != null ? (item.Tag as string ?? "") : "";
        }

        private void SelectComboByText(ComboBox combo, string text)
        {
            if (combo == null) return;
            for (int i = 0; i < combo.Items.Count; i++)
            {
                var it = combo.Items[i] as ComboBoxItem;
                if (it != null && string.Equals(it.Content as string, text, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
            if (combo.Items.Count > 0) combo.SelectedIndex = 0;
        }

        // ---------------- V3 UI <-> Model ----------------
        private void PopulateTaskUI(V3TaskModel t)
        {
            if (t == null)
            {
                ClearTaskUI();
                return;
            }

            TaskIdText.Text = t.V3Id ?? "";
            TaskEnabledCheck.IsChecked = t.Enabled;
            TaskDescriptionText.Text = t.Description ?? "";

            // Targeting
            V3MachineText.Text = t.MachineName ?? "";
            V3UserText.Text = t.UserName ?? "";
            SelectByTag(V3RunAsCombo, string.IsNullOrWhiteSpace(t.RunAs) ? "either" : t.RunAs);

            // Schedule type -> select by Tag to be robust
            SelectByTag(V3ScheduleTypeCombo, string.IsNullOrWhiteSpace(t.ScheduleType) ? "daily" : t.ScheduleType);
            UpdateV3Blocks();

            // Time (for daily/weekly/monthly)
            string hh = "00", mm = "00";
            if (!string.IsNullOrEmpty(t.TimeHHmm))
            {
                var parts = t.TimeHHmm.Split(':');
                if (parts.Length == 2)
                {
                    hh = parts[0].PadLeft(2, '0');
                    mm = parts[1].PadLeft(2, '0');
                }
            }
            SelectByText(HourCombo, hh);
            SelectByText(MinuteCombo, mm);

            // Weekly
            MonCheck.IsChecked = t.WeeklyDays.Contains("Mon");
            TueCheck.IsChecked = t.WeeklyDays.Contains("Tue");
            WedCheck.IsChecked = t.WeeklyDays.Contains("Wed");
            ThuCheck.IsChecked = t.WeeklyDays.Contains("Thu");
            FriCheck.IsChecked = t.WeeklyDays.Contains("Fri");
            SatCheck.IsChecked = t.WeeklyDays.Contains("Sat");
            SunCheck.IsChecked = t.WeeklyDays.Contains("Sun");

            // Monthly
            MonthlyDayText.Text = t.MonthlyDay.HasValue ? t.MonthlyDay.Value.ToString() : "";

            // Hourly/minutely
            SelectByText(HourlyMinuteCombo, (t.HourlyMinute.HasValue ? t.HourlyMinute.Value : 0).ToString());
            if (MinutelyIntervalCombo != null)
                SelectByText(MinutelyIntervalCombo, (t.MinutelyInterval.HasValue ? t.MinutelyInterval.Value : 5).ToString());

            // Actions
            BindActionsGrid(t);
        }

        private void BindActionsGrid(V3TaskModel t)
        {
            V3ActionsGrid.ItemsSource = null;
            if (t != null)
                V3ActionsGrid.ItemsSource = t.Actions;
        }

        private void PushUIIntoCurrent()
        {
            if (_current == null) return;

            _current.V3Id = TaskIdText.Text != null ? TaskIdText.Text.Trim() : null;
            _current.Enabled = TaskEnabledCheck.IsChecked == true;
            _current.Description = TaskDescriptionText.Text ?? "";
            _current.MachineName = V3MachineText.Text != null ? V3MachineText.Text.Trim() : "";
            _current.UserName = V3UserText.Text != null ? V3UserText.Text.Trim() : "";
            _current.RunAs = GetSelectedTag(V3RunAsCombo); // "either"|"user"|"system"

            var tag = GetSelectedTag(V3ScheduleTypeCombo);
            _current.ScheduleType = string.IsNullOrEmpty(tag) ? "daily" : tag;

            string hh = (HourCombo.SelectedItem as ComboBoxItem) != null ? ((ComboBoxItem)HourCombo.SelectedItem).Content.ToString() : "00";
            string mm = (MinuteCombo.SelectedItem as ComboBoxItem) != null ? ((ComboBoxItem)MinuteCombo.SelectedItem).Content.ToString() : "00";
            _current.TimeHHmm = (hh ?? "00") + ":" + (mm ?? "00");

            _current.WeeklyDays.Clear();
            if (MonCheck.IsChecked == true) _current.WeeklyDays.Add("Mon");
            if (TueCheck.IsChecked == true) _current.WeeklyDays.Add("Tue");
            if (WedCheck.IsChecked == true) _current.WeeklyDays.Add("Wed");
            if (ThuCheck.IsChecked == true) _current.WeeklyDays.Add("Thu");
            if (FriCheck.IsChecked == true) _current.WeeklyDays.Add("Fri");
            if (SatCheck.IsChecked == true) _current.WeeklyDays.Add("Sat");
            if (SunCheck.IsChecked == true) _current.WeeklyDays.Add("Sun");

            int md;
            if (int.TryParse(MonthlyDayText.Text != null ? MonthlyDayText.Text.Trim() : "", out md)) _current.MonthlyDay = md;
            else _current.MonthlyDay = null;

            int hm;
            if (int.TryParse(((HourlyMinuteCombo.SelectedItem as ComboBoxItem) != null ? ((ComboBoxItem)HourlyMinuteCombo.SelectedItem).Content.ToString() : ""), out hm))
                _current.HourlyMinute = hm;
            else
                _current.HourlyMinute = 0;

            if (_current.ScheduleType == "minutely")
            {
                int iv;
                if (int.TryParse(((MinutelyIntervalCombo != null && MinutelyIntervalCombo.SelectedItem is ComboBoxItem) ? ((ComboBoxItem)MinutelyIntervalCombo.SelectedItem).Content.ToString() : ""), out iv))
                    _current.MinutelyInterval = iv;
                else
                    _current.MinutelyInterval = 5;
            }
            else
            {
                _current.MinutelyInterval = null;
            }
        }

        private void ClearTaskUI()
        {
            TaskIdText.Text = "";
            TaskEnabledCheck.IsChecked = true;
            TaskDescriptionText.Text = "";
            V3ScheduleTypeCombo.SelectedIndex = 0;
            UpdateV3Blocks();

            SelectComboByText(HourCombo, "08");
            SelectComboByText(MinuteCombo, "00");
            MonCheck.IsChecked = TueCheck.IsChecked = WedCheck.IsChecked = ThuCheck.IsChecked =
                FriCheck.IsChecked = SatCheck.IsChecked = SunCheck.IsChecked = false;
            MonthlyDayText.Text = "";
            SelectComboByText(HourlyMinuteCombo, "0");

            if (MinutelyIntervalCombo != null)
            {
                SelectComboByText(MinutelyIntervalCombo, "5");
                if (MinutelyIntervalCombo.SelectedIndex < 0 && MinutelyIntervalCombo.Items.Count > 0)
                    MinutelyIntervalCombo.SelectedIndex = 0;
            }

            V3ActionsGrid.ItemsSource = null;
        }

        // ---------------- V3 XML: build & parse ----------------
        private static XDocument BuildV3Document(string defaultTimezone, IEnumerable<V3TaskModel> tasks)
        {
            var root = new XElement("Tasks");
            if (!string.IsNullOrWhiteSpace(defaultTimezone))
                root.SetAttributeValue("defaultTimezone", defaultTimezone);

            foreach (var t in tasks)
            {
                var taskEl = new XElement("Task");
                if (!string.IsNullOrWhiteSpace(t.V3Id)) taskEl.SetAttributeValue("id", t.V3Id);
                taskEl.SetAttributeValue("enabled", t.Enabled ? "true" : "false");

                // targeting attributes (optional)
                if (!string.IsNullOrWhiteSpace(t.MachineName)) taskEl.SetAttributeValue("machine", t.MachineName);
                if (!string.IsNullOrWhiteSpace(t.UserName)) taskEl.SetAttributeValue("user", t.UserName);
                if (!string.IsNullOrWhiteSpace(t.RunAs) && t.RunAs != "either") taskEl.SetAttributeValue("runAs", t.RunAs);

                if (!string.IsNullOrWhiteSpace(t.Description))
                    taskEl.Add(new XElement("Description", t.Description));

                var sched = new XElement("Schedule");
                var type = string.IsNullOrWhiteSpace(t.ScheduleType) ? "daily" : t.ScheduleType;
                sched.SetAttributeValue("type", type);

                if (type == "hourly")
                {
                    sched.SetAttributeValue("minute", (t.HourlyMinute.HasValue ? t.HourlyMinute.Value : 0).ToString());
                }
                else if (type == "minutely")
                {
                    int n = t.MinutelyInterval.HasValue ? t.MinutelyInterval.Value : 5;
                    sched.SetAttributeValue("interval", n.ToString());
                }
                else if (type == "once")
                {
                    // no extras
                    sched.SetAttributeValue("type", "once");
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(t.TimeHHmm))
                        sched.SetAttributeValue("time", t.TimeHHmm);

                    if (type == "weekly" && t.WeeklyDays.Any())
                        sched.SetAttributeValue("days", string.Join(",", t.WeeklyDays));
                    else if (type == "monthly" && t.MonthlyDay.HasValue)
                        sched.SetAttributeValue("day", t.MonthlyDay.Value);
                }

                taskEl.Add(sched);

                var acts = new XElement("Actions");
                foreach (var a in t.Actions)
                {
                    var ae = new XElement("Action");
                    var atype = a.Type ?? "RunProgram";
                    ae.SetAttributeValue("type", atype);

                    if (!string.IsNullOrWhiteSpace(a.Args)) ae.SetAttributeValue("args", a.Args);
                    if (!string.IsNullOrWhiteSpace(a.Contents)) ae.SetAttributeValue("contents", a.Contents);
                    if (a.Append) ae.SetAttributeValue("append", "true");

                    // unified Target → correct attribute
                    if (atype.Equals("RunProgram", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrWhiteSpace(a.Target)) ae.SetAttributeValue("file", a.Target);
                    }
                    else if (atype.Equals("WriteFile", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrWhiteSpace(a.Target)) ae.SetAttributeValue("path", a.Target);
                    }

                    acts.Add(ae);
                }
                taskEl.Add(acts);

                root.Add(taskEl);
            }

            return new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
        }

        private static (string DefaultTimezone, List<V3TaskModel> Tasks) ParseV3Document(string path)
        {
            var doc = XDocument.Load(path);
            var root = doc.Root;
            if (root == null)
                throw new InvalidOperationException("Root element not found.");

            // defaultTimezone (if any)
            string tz = (string)root.Attribute("defaultTimezone") ?? "";

            // Accept either <Tasks>…<Task/>… or a single <Task/>
            IEnumerable<XElement> taskNodes;
            if (string.Equals(root.Name.LocalName, "Tasks", StringComparison.OrdinalIgnoreCase))
                taskNodes = root.Elements().Where(e => e.Name.LocalName == "Task");
            else if (string.Equals(root.Name.LocalName, "Task", StringComparison.OrdinalIgnoreCase))
                taskNodes = new[] { root };
            else
                throw new InvalidOperationException("Root element must be <Tasks> or <Task>.");

            var list = new List<V3TaskModel>();
            foreach (var t in taskNodes)
                list.Add(ParseSingleTask(t));

            return (tz, list);
        }

        private static V3TaskModel ParseSingleTask(XElement t)
        {
            // Attributes (namespace-agnostic)
            var m = new V3TaskModel
            {
                V3Id = (string)t.Attribute("id") ?? "",
                Enabled = string.Equals((string)t.Attribute("enabled"), "true", StringComparison.OrdinalIgnoreCase),
                MachineName = (string)t.Attribute("machine") ?? "",
                UserName = (string)t.Attribute("user") ?? "",
                RunAs = ((string)t.Attribute("runAs") ?? "either").ToLowerInvariant()
            };

            // <Description>
            var descEl = t.Elements().FirstOrDefault(e => e.Name.LocalName == "Description");
            m.Description = descEl != null ? (string)descEl : "";

            // <Schedule ... />
            var s = t.Elements().FirstOrDefault(e => e.Name.LocalName == "Schedule");
            var type = ((string)s.Attribute("type") ?? "daily").ToLowerInvariant();
            m.ScheduleType = type;

            if (type == "hourly")
            {
                m.TimeHHmm = ""; // no wall-clock time for hourly
                int mn;
                m.HourlyMinute = int.TryParse((string)s.Attribute("minute"), out mn) ? mn : 0;
            }
            else if (type == "minutely")
            {
                m.TimeHHmm = ""; // no wall-clock time for minutely
                int iv;
                if (int.TryParse((string)s.Attribute("interval"), out iv) && iv > 0) m.MinutelyInterval = iv; else m.MinutelyInterval = 5;
            }
            else if (type == "once")
            {
                // one-shot
            }
            else
            {
                m.TimeHHmm = (string)s.Attribute("time") ?? "00:00";
                if (type == "weekly")
                {
                    var days = (string)s.Attribute("days") ?? "";
                    m.WeeklyDays = days.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                else if (type == "monthly")
                {
                    int d;
                    if (int.TryParse((string)s.Attribute("day"), out d)) m.MonthlyDay = d;
                }
            }

            // <Actions> / <Action />
            var actsParent = t.Elements().FirstOrDefault(e => e.Name.LocalName == "Actions");
            IEnumerable<XElement> actionEls = actsParent != null
                ? actsParent.Elements().Where(e => e.Name.LocalName == "Action")
                : t.Elements().Where(e => e.Name.LocalName == "Action"); // allow top-level actions too

            foreach (var a in actionEls)
            {
                var atype = ((string)a.Attribute("type") ?? "RunProgram");
                var row = new V3ActionModel
                {
                    Type = atype,
                    Args = (string)a.Attribute("args") ?? "",
                    Contents = (string)a.Attribute("contents") ?? "",
                    Append = string.Equals((string)a.Attribute("append"), "true", StringComparison.OrdinalIgnoreCase)
                };

                // map file/path to unified Target
                if (string.Equals(atype, "WriteFile", StringComparison.OrdinalIgnoreCase))
                    row.Target = (string)a.Attribute("path") ?? (string)a.Attribute("target") ?? "";
                else
                    row.Target = (string)a.Attribute("file") ?? (string)a.Attribute("target") ?? "";

                m.Actions.Add(row);
            }

            return m;
        }

        // ============================================================
        //  OU support (load, pick, expand recursively)
        // ============================================================
        private void LoadOuList()
        {
            // Build index once; then fill ComboBox with OU keys
            EnsureOuIndexLoaded();

            ComboBox ouCombo = OuCombo as ComboBox;
            TextBlock ouInfo = OuInfo as TextBlock;

            if (ouCombo == null) return;

            if (_ouIndex.Count == 0)
            {
                ouCombo.ItemsSource = null;
                if (ouInfo != null) ouInfo.Text = "(OU list not available)";
                return;
            }

            var keys = _ouIndex.Keys
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)
                .ToList();

            ouCombo.ItemsSource = keys;
            if (ouInfo != null) ouInfo.Text = "(" + keys.Count + " OUs)";
            ouCombo.SelectionChanged -= OuCombo_SelectionChanged;
            ouCombo.SelectionChanged += OuCombo_SelectionChanged;
        }

        private void OuCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBox ouCombo = OuCombo as ComboBox;
                TextBlock ouInfo = OuInfo as TextBlock;

                string ou = (ouCombo != null && ouCombo.SelectedItem != null) ? (string)ouCombo.SelectedItem : null;
                if (string.IsNullOrWhiteSpace(ou))
                {
                    if (ouInfo != null) ouInfo.Text = "";
                    return;
                }

                // Always recursive per your request
                List<string> hosts = GetHostsInOuRecursive(ou);
                if (ouInfo != null) ouInfo.Text = "(" + hosts.Count + " PC)";
            }
            catch
            {
                TextBlock ouInfo = OuInfo as TextBlock;
                if (ouInfo != null) ouInfo.Text = "";
            }
        }

        private void EnsureOuIndexLoaded()
        {
            if (_ouIndex.Count > 0) return;

            try
            {
                if (!File.Exists(ComputersExportPath)) return;

                foreach (var raw in File.ReadAllLines(ComputersExportPath))
                {
                    string line = (raw ?? "").Trim();
                    if (line.Length == 0 || line.StartsWith("#")) continue;

                    // Format: OU-OU-...-HOSTNAME (dash-separated)
                    string[] parts = line.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < parts.Length; i++) parts[i] = parts[i].Trim();
                    if (parts.Length < 2) continue;

                    string host = parts[parts.Length - 1];
                    string ouPath = string.Join("-", parts.Take(parts.Length - 1).ToArray());

                    List<string> list;
                    if (!_ouIndex.TryGetValue(ouPath, out list))
                    {
                        list = new List<string>();
                        _ouIndex[ouPath] = list;
                    }

                    if (!string.IsNullOrWhiteSpace(host))
                        list.Add(host);
                }
            }
            catch
            {
                // don't crash the UI; ComboBox will just stay empty
            }
        }

        private List<string> GetHostsInOuRecursive(string ouPath)
        {
            EnsureOuIndexLoaded();
            var result = new List<string>();
            if (_ouIndex.Count == 0 || string.IsNullOrWhiteSpace(ouPath)) return result;

            string prefix = ouPath.EndsWith("-") ? ouPath : (ouPath + "-");
            foreach (var kv in _ouIndex)
            {
                if (kv.Key.Equals(ouPath, StringComparison.OrdinalIgnoreCase) ||
                    kv.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    result.AddRange(kv.Value);
                }
            }

            result = result
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return result;
        }

        private static string BuildPerHostFileName(string basePath, string hostname, string requiredExt = ".mcw3.xml")
        {
            if (string.IsNullOrWhiteSpace(basePath)) throw new ArgumentNullException("basePath");
            if (string.IsNullOrWhiteSpace(hostname)) throw new ArgumentNullException("hostname");

            // Ensure extension
            var path = basePath;
            if (!path.EndsWith(requiredExt, StringComparison.OrdinalIgnoreCase))
            {
                if (path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    path = path.Substring(0, path.Length - 4);
                path += requiredExt;
            }

            var dir = System.IO.Path.GetDirectoryName(path) ?? "";
            var name = System.IO.Path.GetFileNameWithoutExtension(path) ?? "task";
            var ext = System.IO.Path.GetExtension(path);

            // Normalize
            var hn = hostname.Trim();

            // 1) If a wildcard is present, replace it
            if (name.IndexOf('*') >= 0)
            {
                name = name.Replace("*", hn);
            }
            // 2) If it starts with "all-", replace that prefix with hostname
            else if (name.StartsWith("all-", StringComparison.OrdinalIgnoreCase))
            {
                name = hn + "-" + name.Substring(4);
            }
            // 3) If it already starts with "hostname-", leave it
            else if (!name.StartsWith(hn + "-", StringComparison.OrdinalIgnoreCase))
            {
                // 4) Otherwise, force "hostname-" prefix
                name = hn + "-" + name;
            }

            return System.IO.Path.Combine(dir, name + ext);
        }

        // ====================================================================
        //  Simple models for the V3 editor
        // ====================================================================
        public class V3TaskModel
        {
            public string V3Id { get; set; } = "";
            public bool Enabled { get; set; } = true;
            public string Description { get; set; } = "";
            // targeting
            public string MachineName { get; set; } = ""; // "" or "all" = any
            public string UserName { get; set; } = "";
            public string RunAs { get; set; } = "either"; // "either" | "user" | "system"

            // Schedule
            public string ScheduleType { get; set; } = "daily"; // daily / weekly / monthly / hourly / minutely / once
            public string TimeHHmm { get; set; } = "08:00";     // for daily/weekly/monthly
            public List<string> WeeklyDays { get; set; } = new List<string>();
            public int? MonthlyDay { get; set; }
            public int? HourlyMinute { get; set; } = 0;         // for hourly
            public int? MinutelyInterval { get; set; } = 5;     // for minutely

            // Actions
            public List<V3ActionModel> Actions { get; } = new List<V3ActionModel>();

            public V3TaskModel Clone()
            {
                var c = new V3TaskModel
                {
                    V3Id = V3Id,
                    Enabled = Enabled,
                    Description = Description,
                    ScheduleType = ScheduleType,
                    TimeHHmm = TimeHHmm,
                    WeeklyDays = new List<string>(WeeklyDays),
                    MonthlyDay = MonthlyDay,
                    HourlyMinute = HourlyMinute,
                    MinutelyInterval = MinutelyInterval
                };
                foreach (var a in Actions)
                    c.Actions.Add(a.Clone());
                return c;
            }
        }

        public class V3ActionModel
        {
            public string Type { get; set; } = "RunProgram"; // RunProgram | WriteFile
            public string Target { get; set; } = "";         // exe path OR file path
            public string Args { get; set; } = "";           // for RunProgram
            public string Contents { get; set; } = "";       // for WriteFile
            public bool Append { get; set; } = false;        // for WriteFile

            public V3ActionModel Clone()
            {
                return new V3ActionModel
                {
                    Type = this.Type,
                    Target = this.Target,
                    Args = this.Args,
                    Contents = this.Contents,
                    Append = this.Append
                };
            }
        }
    }
}
