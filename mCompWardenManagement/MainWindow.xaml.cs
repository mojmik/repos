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
            if (FindName("MinutelyIntervalCombo") is ComboBox mic)
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
            if (FindName("MinutelyBlock") is StackPanel mb) MinutelyBlock = mb;
            if (FindName("MinutelyIntervalCombo") is ComboBox mic) MinutelyIntervalCombo = mic;

            if (V3ScheduleTypeCombo == null || WeeklyBlock == null || MonthlyBlock == null ||
                HourlyBlock == null || TimeBlock == null)
                return;

            var tag = GetSelectedTag(V3ScheduleTypeCombo);
            if (string.IsNullOrEmpty(tag))
            {
                if (V3ScheduleTypeCombo.Items.Count > 0) V3ScheduleTypeCombo.SelectedIndex = 0;
                tag = "daily";
            }

            bool isWeekly = tag.Equals("weekly", StringComparison.OrdinalIgnoreCase);
            bool isMonthly = tag.Equals("monthly", StringComparison.OrdinalIgnoreCase);
            bool isHourly = tag.Equals("hourly", StringComparison.OrdinalIgnoreCase);
            bool isMinutely = tag.Equals("minutely", StringComparison.OrdinalIgnoreCase);

            WeeklyBlock.Visibility = isWeekly ? Visibility.Visible : Visibility.Collapsed;
            MonthlyBlock.Visibility = isMonthly ? Visibility.Visible : Visibility.Collapsed;
            HourlyBlock.Visibility = isHourly ? Visibility.Visible : Visibility.Collapsed;
            if (MinutelyBlock != null) MinutelyBlock.Visibility = isMinutely ? Visibility.Visible : Visibility.Collapsed;

            TimeBlock.Visibility = (isHourly || isMinutely) ? Visibility.Collapsed : Visibility.Visible;

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
            var idx = _v3Tasks.IndexOf(_current);
            if (idx < 0) return;

            _v3Tasks.RemoveAt(idx);
            _current = null;
            RefreshTaskList();

            if (_v3Tasks.Count > 0)
            {
                var newIdx = Math.Max(0, idx - 1);
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
            PushUIIntoCurrent();
            var sel = TaskList.SelectedItem as V3TaskModel;
            SelectTask(sel);
        }

        private string BuildSuggestedV3Name()
        {
            string machine = string.IsNullOrWhiteSpace(MachineNameText.Text) ? "all" : MachineNameText.Text.Trim().ToLower();
            string dt = DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss");
            var rnd = new Random();
            string rndTxt = rnd.Next(10000, 99999) + "-" + rnd.Next(10000, 99999);
            string folder = @"\\rentex.intra\company\mkavan_upravy\scripts\mCompWarden2";
            return System.IO.Path.Combine(folder, $"{machine}-{dt}_{rndTxt}.mcw3.xml");
        }

        private void SelectTask(V3TaskModel t)
        {
            _current = t;
            TaskList.SelectedItem = t;
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
            if (V3ActionsGrid.SelectedItem is V3ActionModel row)
            {
                _current.Actions.Remove(row);
                BindActionsGrid(_current);
            }
        }

        private void MoveActionUp_Click(object sender, RoutedEventArgs e)
        {
            if (_current == null) return;
            if (V3ActionsGrid.SelectedItem is V3ActionModel row)
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
            if (V3ActionsGrid.SelectedItem is V3ActionModel row)
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
                // V3 save (multi-task)
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
                    var ok = dlg.ShowDialog();
                    if (ok == true)
                        FileNameText.Text = dlg.FileName;
                    else
                        return;
                }

                var path = EnsureV3Extension(FileNameText.Text);

                try
                {
                    var dir = System.IO.Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    var doc = BuildV3Document(DefaultTimezoneText.Text?.Trim(), _v3Tasks);
                    if (File.Exists(path)) File.Delete(path);
                    doc.Save(path);
                    MessageBox.Show("Soubor V3 uložen!");
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
                    var (tz, tasks) = ParseV3Document(file);
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

            string ext = (tag == "v3") ? "mcw3.xml" : "cwd";
            string machine = MachineNameText.Text.Trim();
            if (string.IsNullOrEmpty(machine)) machine = "all";

            var rnd = new Random();
            string rndTxt = rnd.Next(10000, 99999) + "-" + rnd.Next(10000, 99999);
            string dt = DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss");

            FileNameText.Text = @"\\server\path\" + machine + "-" + dt + "_" + rndTxt + "." + ext;
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
            var item = combo?.SelectedItem as ComboBoxItem;
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

            // --- NEW: targeting → UI ---
            V3MachineText.Text = t.MachineName ?? "";
            V3UserText.Text = t.UserName ?? "";
            if (t.RunAs == "user") V3RunAsCombo.SelectedIndex = 1;
            else if (t.RunAs == "system") V3RunAsCombo.SelectedIndex = 2;
            else V3RunAsCombo.SelectedIndex = 0;

            if (t.ScheduleType == "weekly") V3ScheduleTypeCombo.SelectedIndex = 1;
            else if (t.ScheduleType == "monthly") V3ScheduleTypeCombo.SelectedIndex = 2;
            else if (t.ScheduleType == "hourly") V3ScheduleTypeCombo.SelectedIndex = 3;
            else if (t.ScheduleType == "minutely") V3ScheduleTypeCombo.SelectedIndex = 4;
            else V3ScheduleTypeCombo.SelectedIndex = 0; // daily
            UpdateV3Blocks();

            string hh = "00", mm = "00";
            if (!string.IsNullOrEmpty(t.TimeHHmm))
            {
                var parts = t.TimeHHmm.Split(':');
                if (parts.Length == 2) { hh = parts[0]; mm = parts[1]; }
            }
            SelectComboByText(HourCombo, hh);
            SelectComboByText(MinuteCombo, mm);

            MonCheck.IsChecked = t.WeeklyDays.Contains("Mon");
            TueCheck.IsChecked = t.WeeklyDays.Contains("Tue");
            WedCheck.IsChecked = t.WeeklyDays.Contains("Wed");
            ThuCheck.IsChecked = t.WeeklyDays.Contains("Thu");
            FriCheck.IsChecked = t.WeeklyDays.Contains("Fri");
            SatCheck.IsChecked = t.WeeklyDays.Contains("Sat");
            SunCheck.IsChecked = t.WeeklyDays.Contains("Sun");

            MonthlyDayText.Text = t.MonthlyDay.HasValue ? t.MonthlyDay.Value.ToString() : "";

            SelectComboByText(HourlyMinuteCombo, (t.HourlyMinute ?? 0).ToString());

            if (MinutelyIntervalCombo != null)
                SelectComboByText(MinutelyIntervalCombo, (t.MinutelyInterval ?? 5).ToString());

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

            _current.V3Id = TaskIdText.Text?.Trim();
            _current.Enabled = TaskEnabledCheck.IsChecked == true;
            _current.Description = TaskDescriptionText.Text ?? "";
            _current.MachineName = V3MachineText.Text?.Trim() ?? "";
            _current.UserName = V3UserText.Text?.Trim() ?? "";
            _current.RunAs = GetSelectedTag(V3RunAsCombo); // "either"|"user"|"system"

            var tag = GetSelectedTag(V3ScheduleTypeCombo);
            _current.ScheduleType = string.IsNullOrEmpty(tag) ? "daily" : tag;

            string hh = (HourCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "00";
            string mm = (MinuteCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "00";
            _current.TimeHHmm = $"{hh}:{mm}";

            _current.WeeklyDays.Clear();
            if (MonCheck.IsChecked == true) _current.WeeklyDays.Add("Mon");
            if (TueCheck.IsChecked == true) _current.WeeklyDays.Add("Tue");
            if (WedCheck.IsChecked == true) _current.WeeklyDays.Add("Wed");
            if (ThuCheck.IsChecked == true) _current.WeeklyDays.Add("Thu");
            if (FriCheck.IsChecked == true) _current.WeeklyDays.Add("Fri");
            if (SatCheck.IsChecked == true) _current.WeeklyDays.Add("Sat");
            if (SunCheck.IsChecked == true) _current.WeeklyDays.Add("Sun");

            if (int.TryParse(MonthlyDayText.Text?.Trim(), out int md)) _current.MonthlyDay = md;
            else _current.MonthlyDay = null;

            if (int.TryParse((HourlyMinuteCombo.SelectedItem as ComboBoxItem)?.Content?.ToString(), out int hm))
                _current.HourlyMinute = hm;
            else
                _current.HourlyMinute = 0;

            if (_current.ScheduleType == "minutely")
            {
                if (int.TryParse((MinutelyIntervalCombo?.SelectedItem as ComboBoxItem)?.Content?.ToString(), out int iv))
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

                // NEW: targeting attributes (optional)
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
                    sched.SetAttributeValue("minute", (t.HourlyMinute ?? 0).ToString());
                }
                else if (type == "minutely")
                {
                    var n = t.MinutelyInterval ?? 5;
                    sched.SetAttributeValue("interval", n.ToString());
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
            if (root == null || !string.Equals(root.Name.LocalName, "Tasks", StringComparison.OrdinalIgnoreCase))
            {
                if (doc.Root != null && string.Equals(doc.Root.Name.LocalName, "Task", StringComparison.OrdinalIgnoreCase))
                {
                    return ("", new List<V3TaskModel> { ParseSingleTask(doc.Root) });
                }
                throw new InvalidOperationException("Root element <Tasks> not found.");
            }

            string tz = (string)root.Attribute("defaultTimezone") ?? "";
            var list = new List<V3TaskModel>();

            foreach (var t in root.Elements("Task"))
                list.Add(ParseSingleTask(t));

            return (tz, list);
        }

        private static V3TaskModel ParseSingleTask(XElement t)
        {
            var m = new V3TaskModel
            {
                V3Id = (string)t.Attribute("id") ?? "",
                Enabled = string.Equals((string)t.Attribute("enabled"), "true", StringComparison.OrdinalIgnoreCase),
                Description = (string)t.Element("Description") ?? ""
            };
            m.MachineName = (string)t.Attribute("machine") ?? "";
            m.UserName = (string)t.Attribute("user") ?? "";
            m.RunAs = ((string)t.Attribute("runAs") ?? "either").ToLowerInvariant();

            var s = t.Element("Schedule");
            string type = (string)s?.Attribute("type") ?? "daily";
            m.ScheduleType = type;

            if (type == "hourly")
            {
                if (int.TryParse((string)s?.Attribute("minute"), out int mn)) m.HourlyMinute = mn; else m.HourlyMinute = 0;
            }
            else if (type == "minutely")
            {
                if (int.TryParse((string)s?.Attribute("interval"), out int iv)) m.MinutelyInterval = iv; else m.MinutelyInterval = 5;
            }
            else
            {
                m.TimeHHmm = (string)s?.Attribute("time") ?? "00:00";
                if (type == "weekly")
                {
                    var daysAttr = (string)s?.Attribute("days") ?? "";
                    m.WeeklyDays = daysAttr.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                else if (type == "monthly")
                {
                    if (int.TryParse((string)s?.Attribute("day"), out int d)) m.MonthlyDay = d;
                }
            }

            var acts = t.Element("Actions");
            if (acts != null)
            {
                foreach (var a in acts.Elements("Action"))
                {
                    var atype = (string)a.Attribute("type") ?? "RunProgram";
                    var row = new V3ActionModel
                    {
                        Type = atype,
                        Args = (string)a.Attribute("args") ?? "",
                        Contents = (string)a.Attribute("contents") ?? "",
                        Append = string.Equals((string)a.Attribute("append"), "true", StringComparison.OrdinalIgnoreCase)
                    };

                    // Map file/path to unified Target
                    if (atype.Equals("RunProgram", StringComparison.OrdinalIgnoreCase))
                        row.Target = (string)a.Attribute("file") ?? (string)a.Attribute("target") ?? "";
                    else if (atype.Equals("WriteFile", StringComparison.OrdinalIgnoreCase))
                        row.Target = (string)a.Attribute("path") ?? (string)a.Attribute("target") ?? "";

                    m.Actions.Add(row);
                }
            }

            return m;
        }
    }

    // ====================================================================
    //  Simple models for the V3 editor
    // ====================================================================
    public class V3TaskModel
    {
        public string V3Id { get; set; } = "";
        public bool Enabled { get; set; } = true;
        public string Description { get; set; } = "";
        // NEW targeting
        public string MachineName { get; set; } = ""; // "" or "all" = any
        public string UserName { get; set; } = "";
        public string RunAs { get; set; } = "either"; // "either" | "user" | "system"

        // Schedule
        public string ScheduleType { get; set; } = "daily"; // daily / weekly / monthly / hourly / minutely
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
