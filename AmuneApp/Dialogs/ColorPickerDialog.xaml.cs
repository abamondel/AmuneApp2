using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AmuneApp.Dialogs
{
    public partial class ColorPickerDialog : Window
    {
        private bool suppressUpdate = false;

        public Color SelectedColor { get; private set; }

        private static readonly string[] SwatchColors = new[]
        {
            "#FFFFFF", "#C0C0C0", "#808080", "#404040", "#000000",
            "#FF0000", "#FF4444", "#FF6666", "#FF8888", "#FFAAAA",
            "#FF8800", "#FFAA44", "#FFCC88", "#FFDD00", "#FFEE66",
            "#00CC00", "#44DD44", "#88EE88", "#00AA44", "#44CC88",
            "#0088FF", "#44AAFF", "#88CCFF", "#0044CC", "#4466DD",
            "#8800FF", "#AA44FF", "#CC88FF", "#FF00FF", "#FF66FF",
            "#884422", "#AA6644", "#CC8866", "#664422", "#886644",
            "#003344", "#006688", "#0099CC", "#00CCCC", "#66EEEE",
        };

        public ColorPickerDialog(Color initialColor)
        {
            InitializeComponent();

            SelectedColor = initialColor;
            BuildSwatches();

            suppressUpdate = true;
            slR.Value = initialColor.R;
            slG.Value = initialColor.G;
            slB.Value = initialColor.B;
            suppressUpdate = false;

            UpdateFromSliders();
        }

        private void BuildSwatches()
        {
            foreach (var hex in SwatchColors)
            {
                var color = (Color)ColorConverter.ConvertFromString(hex);
                var rect = new Rectangle
                {
                    Width = 32,
                    Height = 32,
                    Margin = new Thickness(2),
                    Fill = new SolidColorBrush(color),
                    RadiusX = 4,
                    RadiusY = 4,
                    Cursor = Cursors.Hand,
                    Stroke = new SolidColorBrush(Colors.Gray),
                    StrokeThickness = 0.5
                };
                rect.MouseLeftButtonUp += (s, e) =>
                {
                    suppressUpdate = true;
                    slR.Value = color.R;
                    slG.Value = color.G;
                    slB.Value = color.B;
                    suppressUpdate = false;
                    UpdateFromSliders();
                };
                colorSwatches.Children.Add(rect);
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!suppressUpdate)
                UpdateFromSliders();
        }

        private void UpdateFromSliders()
        {
            byte r = (byte)slR.Value;
            byte g = (byte)slG.Value;
            byte b = (byte)slB.Value;

            SelectedColor = Color.FromRgb(r, g, b);

            if (tbR != null) tbR.Text = r.ToString();
            if (tbG != null) tbG.Text = g.ToString();
            if (tbB != null) tbB.Text = b.ToString();
            if (tbHex != null) tbHex.Text = $"#{r:X2}{g:X2}{b:X2}";
            if (previewBorder != null) previewBorder.Background = new SolidColorBrush(SelectedColor);
        }

        private void tbHex_LostFocus(object sender, RoutedEventArgs e) => ApplyHex();
        private void tbHex_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) ApplyHex();
        }

        private void ApplyHex()
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(tbHex.Text);
                suppressUpdate = true;
                slR.Value = color.R;
                slG.Value = color.G;
                slB.Value = color.B;
                suppressUpdate = false;
                UpdateFromSliders();
            }
            catch { }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
