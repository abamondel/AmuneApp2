using System.Windows;
using System.Windows.Media.Animation;

namespace AmuneApp
{
    public partial class FloatingTextWindow : Window
    {
        public FloatingTextWindow(string sentence, string fontFamily)
        {
            InitializeComponent();

            floatingText.Text = sentence;
            floatingText.FontFamily = new System.Windows.Media.FontFamily(fontFamily);

            Loaded += FloatingTextWindow_Loaded;
        }

        private void FloatingTextWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Measure the text width
            floatingText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double textWidth = floatingText.DesiredSize.Width;
            double textHeight = floatingText.DesiredSize.Height;
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            // Position vertically at a random spot in the middle third of the screen
            double minY = screenHeight * 0.2;
            double maxY = screenHeight * 0.7 - textHeight;
            double yPos = minY + new Random().NextDouble() * (maxY - minY);
            System.Windows.Controls.Canvas.SetTop(floatingText, yPos);

            // Animate from right edge to beyond left edge (RTL flow)
            double startX = screenWidth;
            double endX = -textWidth;

            var animation = new DoubleAnimation
            {
                From = startX,
                To = endX,
                Duration = TimeSpan.FromSeconds(8),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            animation.Completed += (s, args) => Close();

            floatingText.BeginAnimation(System.Windows.Controls.Canvas.LeftProperty, animation);

            // Fade in then fade out
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(1));
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(1.5))
            {
                BeginTime = TimeSpan.FromSeconds(6.5)
            };

            floatingText.BeginAnimation(OpacityProperty, fadeIn);
            floatingText.BeginAnimation(OpacityProperty, fadeOut);
        }
    }
}
