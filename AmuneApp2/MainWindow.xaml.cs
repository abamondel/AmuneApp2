using AmuneApp.UserControls;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using AmuneApp2.Properties;
using System.Linq;
using IWshRuntimeLibrary; // Ensure this reference is added to your project

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

        public MainWindow()
        {
            InitializeComponent();
            LoadOrInitializeSentences();
            //AnimateSentence();
           Animations.AnimateTextColor(sentenceTextBlock);
          // AnimateBorderColor();
            CenterWindowOnScreen();
            HandleFirstRun();
            UpdateStartupCheckboxStatus();
            MoveWindowAcrossScreen();
        }

        private void MoveWindowAcrossScreen()
        {
            // Ensure the window is initially positioned on the left side of the screen
            this.Left = -this.Width;

            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var animation = new DoubleAnimation
            {
                From = screenWidth,
                To = -this.Width,
                Duration = TimeSpan.FromSeconds(5) // Adjust the duration as needed
            };

            animation.Completed += (s, e) => this.Close(); // Close the window after the animation completes

            this.BeginAnimation(Window.LeftProperty, animation);
        }

        private void LoadOrInitializeSentences()
        {
            if (System.IO.File.Exists(sentencePath))
            {
                sentences = LoadListFromFile(sentencePath) ?? new ObservableCollection<string>();
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(sentencePath));
                SaveListToFile();
            }
        }

        private ObservableCollection<string> LoadListFromFile(string path)
        {
            string json = System.IO.File.ReadAllText(path);
            return JsonConvert.DeserializeObject<ObservableCollection<string>>(json);
        }

        private async void AnimateSentence()
        {
            while (true)
            {
                string sentence = sentences[currentSentenceIndex];
                await DisplaySentenceWithTypingAnimation(sentence);
                AppendBlinkingDot();
                await Task.Delay(3000); // Wait before transitioning to the next sentence
                ResetSentenceDisplay();
            }
        }

        private void CenterWindowOnScreen()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            Left = (screenWidth - Width) / 2;
            Top = (screenHeight - Height) / 3 - 100;
        }

        private void HandleFirstRun()
        {
            if (Settings.Default.IsFirstRun)
            {
                AddToStartup();
                Settings.Default.IsFirstRun = false;
                Settings.Default.Save();
            }
        }

        private void SaveListToFile()
        {
            string json = JsonConvert.SerializeObject(sentences);
            System.IO.File.WriteAllText(sentencePath, json);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            sentences.Clear();
            foreach (AddSentenceControle item in popupSp.Children.OfType<AddSentenceControle>())
            {
                if (!string.IsNullOrWhiteSpace(item.Text))
                    sentences.Add(item.Text.Trim());
            }

            tbAddSentence.Clear();
            addStringPopup.IsOpen = false;
            SaveListToFile();
        }

        private void ResetSentenceDisplay()
        {
            spTextGrid.Children.Clear();
            spTextGrid.Children.Add(sentenceTextBlock);
            currentSentenceIndex = (currentSentenceIndex + 1) % sentences.Count;
        }

        private async Task DisplaySentenceWithTypingAnimation(string sentence)
        {
            sentenceTextBlock.Text = "";
            foreach (char c in sentence)
            {
                sentenceTextBlock.Text += c;
                await Task.Delay(100); // Typing speed
            }
        }

        private async Task AppendBlinkingDot()
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

        private void AddToStartup()
        {
            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = Path.Combine(startupFolder, "AmuneApp.lnk");

            if (!System.IO.File.Exists(shortcutPath))
            {
                string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);

                shortcut.Description = "Shortcut for AmuneApp";
                shortcut.TargetPath = appPath;
                shortcut.Save();
            }
        }

        private void RemoveFromStartup()
        {
            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = Path.Combine(startupFolder, "AmuneApp.lnk");

            if (System.IO.File.Exists(shortcutPath))
            {
                System.IO.File.Delete(shortcutPath);
            }
        }

        private void UpdateStartupCheckboxStatus()
        {
            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = Path.Combine(startupFolder, "AmuneApp.lnk");
            cbStartOnStarup.IsChecked = System.IO.File.Exists(shortcutPath);
        }

        // Continuing with the unchanged methods and event handlers

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
            addStringPopup.IsOpen = false;
        }

        private void AnimateBorderColor()
        {
            LinearGradientBrush brush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1) // Diagonal gradient
            };

            // Define the gradient stops with additional striking colors
            brush.GradientStops.Add(new GradientStop(Colors.AliceBlue, 0.0));
            brush.GradientStops.Add(new GradientStop(Colors.Magenta, 0.2));
            brush.GradientStops.Add(new GradientStop(Colors.Cyan, 0.5)); // This is the striking color
            brush.GradientStops.Add(new GradientStop(Colors.Gold, 0.8));
            brush.GradientStops.Add(new GradientStop(Colors.AliceBlue, 1.0));

            //rbWindowColor.Stroke = brush;

            // Animation for moving the striking colors across the border
            DoubleAnimation animation1 = new DoubleAnimation(-0.5, 1.5, new Duration(TimeSpan.FromSeconds(3)))
            {
                RepeatBehavior = RepeatBehavior.Forever,
                BeginTime = TimeSpan.FromSeconds(0.1) // Start the animations with a slight delay between them to create a wave effect
            };
            DoubleAnimation animation2 = new DoubleAnimation(-0.5, 1.5, new Duration(TimeSpan.FromSeconds(3)))
            {
                RepeatBehavior = RepeatBehavior.Forever,
                BeginTime = TimeSpan.FromSeconds(0.5)
            };
            DoubleAnimation animation3 = new DoubleAnimation(1.5, -0.5, new Duration(TimeSpan.FromSeconds(3)))
            {
                RepeatBehavior = RepeatBehavior.Forever,
                BeginTime = TimeSpan.FromSeconds(2.2)
            };

            brush.GradientStops[1].BeginAnimation(GradientStop.OffsetProperty, animation1);
            brush.GradientStops[2].BeginAnimation(GradientStop.OffsetProperty, animation2);
            brush.GradientStops[3].BeginAnimation(GradientStop.OffsetProperty, animation3);
        }

        private void ContextMenu_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Close();
            // App.Current.Shutdown();
        }

        private void MenuItemSentenceMouseUp(object sender, MouseButtonEventArgs e)
        {
            int i = 0;
            popupSp.Children.Clear();
            popupSp.Children.Add(stNewSentence);

            // Show the popup for adding a string
            foreach (var item in sentences)
            {
                addStringPopup.IsOpen = true;
                AddSentenceControle textBox = new AddSentenceControle
                {
                    Name = $"tb{i}",
                    parentStackPanel = popupSp,
                    Text = item,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                popupSp.Children.Insert(popupSp.Children.Count - 1, textBox);
                i++;
            }
        }

        bool act = true;
        private void tbAddSentence_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (act)
            {
                act = false;
                AddSentenceControle textBox = new AddSentenceControle
                {
                    Text = tbAddSentence.Text,
                    parentStackPanel = popupSp,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                popupSp.Children.Insert(popupSp.Children.Count - 1, textBox);
                textBox.Focus();
                tbAddSentence.Text = "";
                act = true;
            }
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !addStringPopup.IsMouseOver)
            {
                addStringPopup.IsOpen = false;
                DragMove();
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            AddToStartup();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            RemoveFromStartup();
        }

        private void MenuItem_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!cbStartOnStarup.IsMouseOver)
                cbStartOnStarup.IsChecked = !cbStartOnStarup.IsChecked;
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            addStringPopup.IsOpen = false;
        }
    }
}
