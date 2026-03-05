using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Newtonsoft.Json;
using AmuneApp.UserControls;
using Microsoft.Win32;
using Forms = System.Windows.Forms;

namespace AmuneApp
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<string> sentences = new ObservableCollection<string>
        {
            "אין שומע לי ומפסיד",
            "דע לפני מי אתה עומד",
            "בהיתר ולא באיסור"
        };

        private int currentSentenceIndex = 0;
        private readonly string sentencePath = @"C:\ProgramData\AmuneApp\sentences.json";
        private AppSettings settings;
        private Forms.NotifyIcon trayIcon;
        private readonly Random random = new();
        private bool isReallyClosing = false;

        public MainWindow()
        {
            LoadOrInitializeSentences();
            settings = AppSettings.Load();

            InitializeComponent();
            SetupTrayIcon();
            ApplySettings();
            AnimateSentence();
            Animations.AnimateTextColor(sentenceTextBlock);
            AnimateBorderColor();
            RestoreWindowPosition();
            UpdateStartupCheckboxStatus();
            CheckForUpdatesAsync();

            Deactivated += (s, e) => addStringPopup.IsOpen = false;
        }

        #region Initialization

        private void LoadOrInitializeSentences()
        {
            if (File.Exists(sentencePath))
            {
                var loaded = LoadListFromFile(sentencePath);
                if (loaded.Count > 0) sentences = loaded;
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(sentencePath));
                SaveListToFile(sentences, sentencePath);
            }
        }

        private ObservableCollection<string> LoadListFromFile(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                var items = JsonConvert.DeserializeObject<List<string>>(json);
                return items != null ? new ObservableCollection<string>(items) : new ObservableCollection<string>();
            }
            catch { return new ObservableCollection<string>(); }
        }

        private void SaveListToFile(ObservableCollection<string> items, string path)
        {
            string json = JsonConvert.SerializeObject(items.ToList());
            File.WriteAllText(path, json);
        }

        private void ApplySettings()
        {
            Opacity = settings.WindowOpacity;
            UpdateTimerChecked();
            UpdateOpacityChecked();

            sentenceTextBlock.FontFamily = new FontFamily(settings.FontFamily);

            (miShuffle.Header as CheckBox).IsChecked = settings.ShuffleMode;
            (miDarkMode.Header as CheckBox).IsChecked = settings.DarkMode;
            ApplyTheme();

            try
            {
                windowBgBrush.Color = (Color)ColorConverter.ConvertFromString(settings.BackgroundColor);
            }
            catch { }
        }

        private void SetupTrayIcon()
        {
            trayIcon = new Forms.NotifyIcon
            {
                Icon = new System.Drawing.Icon(
                    Application.GetResourceStream(new Uri("pack://application:,,,/Resources/app.ico")).Stream),
                Text = "AmuneApp",
                Visible = true
            };

            var trayMenu = new Forms.ContextMenuStrip();
            trayMenu.Items.Add("Show / Hide", null, (s, e) => ToggleWindowVisibility());
            trayMenu.Items.Add(new Forms.ToolStripSeparator());
            trayMenu.Items.Add("Edit Pusikim", null, (s, e) => { Show(); Activate(); EditPusikim_Click(null, null); });
            trayMenu.Items.Add(new Forms.ToolStripSeparator());
            trayMenu.Items.Add("Quit", null, (s, e) => QuitApp());

            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.DoubleClick += (s, e) => ToggleWindowVisibility();
        }

        private void ToggleWindowVisibility()
        {
            if (IsVisible) Hide();
            else { Show(); Activate(); }
        }

        private void RestoreWindowPosition()
        {
            if (settings.WindowLeft >= 0 && settings.WindowTop >= 0)
            {
                Left = settings.WindowLeft;
                Top = settings.WindowTop;
            }
            else
            {
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;
                Left = (screenWidth - Width) / 2;
                Top = (screenHeight / 3) - Height - 100;
            }
        }

        #endregion

        #region Sentence Animation

        private async void AnimateSentence()
        {
            while (true)
            {
                if (sentences.Count == 0)
                {
                    UpdateCounter(0, 0);
                    sentenceTextBlock.Text = "";
                    await Task.Delay(500);
                    continue;
                }

                if (currentSentenceIndex >= sentences.Count)
                    currentSentenceIndex = 0;

                UpdateCounter(currentSentenceIndex + 1, sentences.Count);
                string sentence = sentences[currentSentenceIndex];
                await DisplaySentenceWithTypingAnimation(sentence);

                AppandBlinkingDot();
                await Task.Delay(settings.DisplayTimerSeconds * 1000);

                spTextGrid.Children.Clear();
                spTextGrid.Children.Add(sentenceTextBlock);

                if (sentences.Count == 0) continue;

                if (settings.ShuffleMode && sentences.Count > 1)
                {
                    int next;
                    do { next = random.Next(sentences.Count); }
                    while (next == currentSentenceIndex);
                    currentSentenceIndex = next;
                }
                else
                {
                    currentSentenceIndex = (currentSentenceIndex + 1) % sentences.Count;
                }
            }
        }

        private async Task DisplaySentenceWithTypingAnimation(string sentence)
        {
            sentenceTextBlock.Text = "";
            foreach (char c in sentence)
            {
                sentenceTextBlock.Text += c;
                await Task.Delay(100);
            }
        }

        private async Task AppandBlinkingDot()
        {
            TextBlock dotTextBlock = new TextBlock
            {
                Foreground = sentenceTextBlock.Foreground,
                FontSize = sentenceTextBlock.FontSize,
                FontFamily = sentenceTextBlock.FontFamily,
                FontWeight = sentenceTextBlock.FontWeight,
                Text = "."
            };
            spTextGrid.Children.Add(dotTextBlock);

            Storyboard storyboard = new Storyboard();
            DoubleAnimation blinkAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromSeconds(0.22)),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTarget(blinkAnimation, dotTextBlock);
            Storyboard.SetTargetProperty(blinkAnimation, new PropertyPath("Opacity"));
            storyboard.Children.Add(blinkAnimation);
            storyboard.Begin();
        }

        private void UpdateCounter(int current, int total)
        {
            tbCounter.Text = $"{current} / {total}";
        }

        #endregion

        #region Border Animation

        private void AnimateBorderColor()
        {
            LinearGradientBrush brush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };

            GradientStop colorStart = new GradientStop(Colors.AliceBlue, 0.0);
            GradientStop color1 = new GradientStop(Colors.Magenta, 0.2);
            GradientStop color2 = new GradientStop(Colors.Cyan, 0.5);
            GradientStop color3 = new GradientStop(Colors.Gold, 0.8);
            GradientStop colorEnd = new GradientStop(Colors.AliceBlue, 1.0);

            brush.GradientStops.Add(colorStart);
            brush.GradientStops.Add(color1);
            brush.GradientStops.Add(color2);
            brush.GradientStops.Add(color3);
            brush.GradientStops.Add(colorEnd);

            rbWindowColor.Stroke = brush;

            DoubleAnimation animation1 = new DoubleAnimation(-0.5, 1.5, new Duration(TimeSpan.FromSeconds(3)))
            { RepeatBehavior = RepeatBehavior.Forever, BeginTime = TimeSpan.FromSeconds(0.1) };
            DoubleAnimation animation2 = new DoubleAnimation(-0.5, 1.5, new Duration(TimeSpan.FromSeconds(3)))
            { RepeatBehavior = RepeatBehavior.Forever, BeginTime = TimeSpan.FromSeconds(0.5) };
            DoubleAnimation animation3 = new DoubleAnimation(1.5, -0.5, new Duration(TimeSpan.FromSeconds(3)))
            { RepeatBehavior = RepeatBehavior.Forever, BeginTime = TimeSpan.FromSeconds(2.2) };

            color1.BeginAnimation(GradientStop.OffsetProperty, animation1);
            color2.BeginAnimation(GradientStop.OffsetProperty, animation2);
            color3.BeginAnimation(GradientStop.OffsetProperty, animation3);
        }

        #endregion

        #region Window Events

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (addStringPopup.IsOpen && !addStringPopup.IsMouseOver)
                    addStringPopup.IsOpen = false;
                else
                    DragMove();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!isReallyClosing)
            {
                e.Cancel = true;
                Hide();
                return;
            }

            settings.WindowLeft = Left;
            settings.WindowTop = Top;
            settings.Save();
            SaveListToFile(sentences, sentencePath);
            trayIcon?.Dispose();
        }

        #endregion

        #region Click to Copy

        private void SentenceTextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && !string.IsNullOrEmpty(sentenceTextBlock.Text))
            {
                Clipboard.SetText(sentenceTextBlock.Text);
                sentenceTextBlock.ToolTip = "✓ Copied!";
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1.5)
                };
                timer.Tick += (s, args) =>
                {
                    sentenceTextBlock.ToolTip = "Double-click to copy";
                    timer.Stop();
                };
                timer.Start();
                e.Handled = true;
            }
        }

        #endregion

        #region Sentence Management

        private void EditPusikim_Click(object sender, RoutedEventArgs e)
        {
            popupSp.Children.Clear();
            foreach (var sentence in sentences)
            {
                popupSp.Children.Add(new AddSentenceControle
                {
                    ParentStackPanel = popupSp,
                    Text = sentence,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                });
            }
            tbAddSentence.Clear();
            addStringPopup.IsOpen = true;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewSentenceFromTextBox();

            sentences.Clear();
            foreach (AddSentenceControle item in popupSp.Children.OfType<AddSentenceControle>())
            {
                if (!string.IsNullOrWhiteSpace(item.Text))
                    sentences.Add(item.Text.Trim());
            }
            tbAddSentence.Clear();
            addStringPopup.IsOpen = false;
            SaveListToFile(sentences, sentencePath);
        }

        private void tbAddSentence_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) AddNewSentenceFromTextBox();
        }

        private void btnAddNew_Click(object sender, RoutedEventArgs e) => AddNewSentenceFromTextBox();

        private void AddNewSentenceFromTextBox()
        {
            if (string.IsNullOrWhiteSpace(tbAddSentence.Text)) return;
            popupSp.Children.Add(new AddSentenceControle
            {
                ParentStackPanel = popupSp,
                Text = tbAddSentence.Text.Trim(),
                HorizontalAlignment = HorizontalAlignment.Stretch
            });
            tbAddSentence.Clear();
            tbAddSentence.Focus();
        }

        private void btCancel_Click(object sender, RoutedEventArgs e) => addStringPopup.IsOpen = false;

        #endregion

        #region Timer & Opacity Settings

        private void TimerOption_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && int.TryParse(item.Tag.ToString(), out int seconds))
            {
                settings.DisplayTimerSeconds = seconds;
                settings.Save();
                UpdateTimerChecked();
            }
        }

        private void OpacityOption_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && double.TryParse(item.Tag.ToString(),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out double opacity))
            {
                settings.WindowOpacity = opacity;
                Opacity = opacity;
                settings.Save();
                UpdateOpacityChecked();
            }
        }

        private void UpdateTimerChecked()
        {
            foreach (var child in miTimer.Items.OfType<MenuItem>())
            {
                if (int.TryParse(child.Tag?.ToString(), out int val))
                    child.IsChecked = val == settings.DisplayTimerSeconds;
            }
        }

        private void UpdateOpacityChecked()
        {
            foreach (var child in miOpacity.Items.OfType<MenuItem>())
            {
                if (double.TryParse(child.Tag?.ToString(),
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double val))
                    child.IsChecked = Math.Abs(val - settings.WindowOpacity) < 0.01;
            }
        }

        #endregion

        #region Toggle Settings

        private CheckBox StartupCheckBox => miStartOnStartup.Header as CheckBox;
        private CheckBox ShuffleCheckBox => miShuffle.Header as CheckBox;
        private CheckBox DarkModeCheckBox => miDarkMode.Header as CheckBox;

        private void miStartOnStartup_Click(object sender, RoutedEventArgs e)
        {
            StartupCheckBox.IsChecked = !(StartupCheckBox.IsChecked ?? false);
            if (StartupCheckBox.IsChecked == true) AddToStartup();
            else RemoveFromStartup();
        }

        private void miShuffle_Click(object sender, RoutedEventArgs e)
        {
            ShuffleCheckBox.IsChecked = !(ShuffleCheckBox.IsChecked ?? false);
            settings.ShuffleMode = ShuffleCheckBox.IsChecked == true;
            settings.Save();
        }

        private void miDarkMode_Click(object sender, RoutedEventArgs e)
        {
            DarkModeCheckBox.IsChecked = !(DarkModeCheckBox.IsChecked ?? false);
            settings.DarkMode = DarkModeCheckBox.IsChecked == true;
            ApplyTheme();
            settings.Save();
        }

        private void ApplyTheme()
        {
            var merged = Application.Current.Resources.MergedDictionaries;
            for (int i = merged.Count - 1; i >= 0; i--)
            {
                if (merged[i] is Wpf.Ui.Markup.ThemesDictionary)
                    merged.RemoveAt(i);
            }
            merged.Insert(0, new Wpf.Ui.Markup.ThemesDictionary
            {
                Theme = settings.DarkMode
                    ? Wpf.Ui.Appearance.ApplicationTheme.Dark
                    : Wpf.Ui.Appearance.ApplicationTheme.Light
            });
        }

        #endregion

        #region Startup

        private void AddToStartup()
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            key?.SetValue("AmuneApp", Environment.ProcessPath);
        }

        private void RemoveFromStartup()
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            key?.DeleteValue("AmuneApp", false);
        }

        private void UpdateStartupCheckboxStatus()
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
            if (StartupCheckBox != null)
                StartupCheckBox.IsChecked = key?.GetValue("AmuneApp") != null;
        }

        #endregion

        #region Font & Background

        private void ChangeFont_Click(object sender, RoutedEventArgs e)
        {
            var fontDialog = new Forms.FontDialog();
            try { fontDialog.Font = new System.Drawing.Font(settings.FontFamily, 12); } catch { }
            if (fontDialog.ShowDialog() == Forms.DialogResult.OK)
            {
                settings.FontFamily = fontDialog.Font.FontFamily.Name;
                sentenceTextBlock.FontFamily = new FontFamily(settings.FontFamily);
                settings.Save();
            }
        }

        private void ChangeBackground_Click(object sender, RoutedEventArgs e)
        {
            var colorDialog = new Forms.ColorDialog { FullOpen = true };
            if (colorDialog.ShowDialog() == Forms.DialogResult.OK)
            {
                var c = colorDialog.Color;
                var color = Color.FromArgb(204, c.R, c.G, c.B);
                windowBgBrush.Color = color;
                settings.BackgroundColor = color.ToString();
                settings.Save();
            }
        }

        #endregion

        #region Import / Export

        private void ImportSentences_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Import Sentences",
                Filter = "JSON files (*.json)|*.json",
                DefaultExt = ".json"
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var loaded = LoadListFromFile(dialog.FileName);
                    if (loaded.Count > 0)
                    {
                        sentences = loaded;
                        SaveListToFile(sentences, sentencePath);
                        currentSentenceIndex = 0;
                        MessageBox.Show($"Imported {sentences.Count} sentences.", "AmuneApp",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Import failed: {ex.Message}", "AmuneApp",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportSentences_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Export Sentences",
                Filter = "JSON files (*.json)|*.json",
                DefaultExt = ".json",
                FileName = "sentences.json"
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    SaveListToFile(sentences, dialog.FileName);
                    MessageBox.Show($"Exported {sentences.Count} sentences.", "AmuneApp",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export failed: {ex.Message}", "AmuneApp",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Quit

        private void QuitApp_Click(object sender, RoutedEventArgs e) => QuitApp();

        private void QuitApp()
        {
            isReallyClosing = true;
            Close();
        }

        #endregion

        #region Auto-Update

        private async void CheckForUpdatesAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("AmuneApp/1.0");
                var response = await client.GetStringAsync(
                    "https://api.github.com/repos/abamondel/AmuneApp2/releases/latest");
                var release = JsonConvert.DeserializeObject<dynamic>(response);
                string latestVersion = ((string)release.tag_name).TrimStart('v');
                var currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

                if (Version.TryParse(latestVersion, out var latest) && latest > currentVersion)
                {
                    var result = MessageBox.Show(
                        $"A new version ({latestVersion}) is available. Open download page?",
                        "AmuneApp Update", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = (string)release.html_url,
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch { }
        }

        #endregion
    }
}