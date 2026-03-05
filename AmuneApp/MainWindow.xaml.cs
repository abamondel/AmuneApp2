using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Newtonsoft.Json;
using AmuneApp.UserControls;
using Microsoft.Win32;

namespace AmuneApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<string> sentences = new ObservableCollection<string> {
        "אין שומע לי ומפסיד",
        "דע לפני מי אתה עומד",
        "בהיתר ולא באיסור"
        } ;

        private int currentSentenceIndex = 0;
        public string sentecePath = @"C:\ProgramData\AmuneApp\sentences.json";
        public MainWindow()
        {
            if (File.Exists(sentecePath))
            {
            sentences = LoadListFromFile(sentecePath);
            }
            else
            {
                System.IO.Directory.CreateDirectory(@"C:\ProgramData\AmuneApp\");
                File.Create(sentecePath);
            }


            InitializeComponent();
            AnimateSentence();
            Animations.AnimateTextColor(sentenceTextBlock);
            AnimateBorderColor();
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            this.Left = (screenWidth/2) - this.Width /2;
            this.Top = ((screenHeight/3 ) - this.Height) -  100;

            UpdateStartupCheckboxStatus();
            Deactivated += (s, e) => addStringPopup.IsOpen = false;
        }
       

        private ObservableCollection<string> LoadListFromFile(string sentecePath)
        {
            if (System.IO.File.Exists(sentecePath))
            {
                string json = System.IO.File.ReadAllText(sentecePath);
                List<string> items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(json);
                return new ObservableCollection<string>(items);
            }
            return new ObservableCollection<string>();
        }



        private async void AnimateSentence()
        {
            while (true)
            {
                if (sentences.Count == 0)
                {
                    await Task.Delay(500);
                    continue;
                }

                if (currentSentenceIndex >= sentences.Count)
                    currentSentenceIndex = 0;

                string sentence = sentences[currentSentenceIndex];
                await DisplaySentenceWithTypingAnimation(sentence);

                AppandBlinkingDot();
                await Task.Delay(3000); // Wait before transitioning to the next sentence
                spTextGrid.Children.Clear();
                spTextGrid.Children.Add(sentenceTextBlock);
                currentSentenceIndex = (currentSentenceIndex + 1) % sentences.Count;
            }
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

        private async Task AppandBlinkingDot()
        {
            // Append a blinking dot
            TextBlock dotTextBlock = new TextBlock();
            bool isDotted = false;
            if (isDotted == false)
            {
                dotTextBlock.Foreground = sentenceTextBlock.Foreground;
                dotTextBlock.FontSize = sentenceTextBlock.FontSize;
                dotTextBlock.FontFamily = sentenceTextBlock.FontFamily;
                dotTextBlock.FontWeight = sentenceTextBlock.FontWeight;
                spTextGrid.Children.Add(dotTextBlock);
                dotTextBlock.Text = ".";
                isDotted = true;
            }
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
        private void AnimateBorderColor()
        {
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, 0);
            brush.EndPoint = new Point(1, 1); // Diagonal gradient

            // Define the gradient stops with additional striking colors
            GradientStop colorStart = new GradientStop(Colors.AliceBlue, 0.0);
            GradientStop color1 = new GradientStop(Colors.Magenta, 0.2);
            GradientStop color2 = new GradientStop(Colors.Cyan, 0.5); // This is the striking color
            GradientStop color3 = new GradientStop(Colors.Gold, 0.8);
            GradientStop colorEnd = new GradientStop(Colors.AliceBlue, 1.0);

            brush.GradientStops.Add(colorStart);
            brush.GradientStops.Add(color1);
            brush.GradientStops.Add(color2);
           brush.GradientStops.Add(color3);
            brush.GradientStops.Add(colorEnd);

            rbWindowColor.Stroke = brush;

            // Animation for moving the striking colors across the border
            DoubleAnimation animation1 = new DoubleAnimation(-0.5, 1.5, new Duration(TimeSpan.FromSeconds(3)));
            animation1.RepeatBehavior = RepeatBehavior.Forever;
            DoubleAnimation animation2 = new DoubleAnimation(-0.5, 1.5, new Duration(TimeSpan.FromSeconds(3)));
            animation2.RepeatBehavior = RepeatBehavior.Forever;
            DoubleAnimation animation3 = new DoubleAnimation(1.5, - 0.5, new Duration(TimeSpan.FromSeconds(3)));
            animation3.RepeatBehavior = RepeatBehavior.Forever;

            // Start the animations with a slight delay between them to create a wave effect
            animation1.BeginTime = TimeSpan.FromSeconds(0.1);
            animation2.BeginTime = TimeSpan.FromSeconds(0.5);
            animation3.BeginTime = TimeSpan.FromSeconds(2.2);

            color1.BeginAnimation(GradientStop.OffsetProperty, animation1);
            color2.BeginAnimation(GradientStop.OffsetProperty, animation2);
           color3.BeginAnimation(GradientStop.OffsetProperty, animation3);
        }

        private void ContextMenu_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
           this.Close();
           // App.Current.Shutdown();
            
        }
      

        private void SaveListToFile(ObservableCollection<string> items, string sentecePath)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(items.ToList());
            System.IO.File.WriteAllText(sentecePath, json);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
           
            SaveListToFile(sentences, sentecePath);
        }

        private void TextBlock_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
            sentences.Clear();
            foreach (AddSentenceControle item in popupSp.Children.OfType<AddSentenceControle>())
            {
                if (!string.IsNullOrWhiteSpace(item.Text))
                    sentences.Add(item.Text.Trim());
            }
            tbAddSentence.Clear();
            addStringPopup.IsOpen = false;
        }

        private void tbAddSentence_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                AddNewSentenceFromTextBox();
        }

        private void btnAddNew_Click(object sender, RoutedEventArgs e)
        {
            AddNewSentenceFromTextBox();
        }

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

        private void AddToStartup()
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            key?.SetValue("AmuneApp", Environment.ProcessPath);
        }

        private void RemoveFromStartup()
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            key?.DeleteValue("AmuneApp", false);
        }

        private void UpdateStartupCheckboxStatus()
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
            if (StartupCheckBox != null)
                StartupCheckBox.IsChecked = key?.GetValue("AmuneApp") != null;
        }

        private CheckBox StartupCheckBox => miStartOnStartup.Header as CheckBox;

        private void miStartOnStartup_Click(object sender, RoutedEventArgs e)
        {
            StartupCheckBox.IsChecked = !(StartupCheckBox.IsChecked ?? false);
            if (StartupCheckBox.IsChecked == true)
                AddToStartup();
            else
                RemoveFromStartup();
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            addStringPopup.IsOpen = false;
        }
    }
}