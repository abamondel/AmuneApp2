using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AmuneApp.Dialogs
{
    public partial class FontPickerDialog : Window
    {
        private readonly List<string> allFonts;

        public string SelectedFontFamily { get; private set; }
        public double SelectedFontSize { get; private set; } = 64;

        public FontPickerDialog(string currentFont = "David", double currentSize = 64)
        {
            InitializeComponent();

            allFonts = Fonts.SystemFontFamilies
                .Select(f => f.Source)
                .OrderBy(f => f)
                .ToList();

            lbFonts.ItemsSource = allFonts;
            SelectedFontFamily = currentFont;
            SelectedFontSize = currentSize;
            slSize.Value = currentSize;

            // Select current font
            var index = allFonts.FindIndex(f =>
                f.Equals(currentFont, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                lbFonts.SelectedIndex = index;
                lbFonts.ScrollIntoView(lbFonts.SelectedItem);
            }

            UpdatePreview();
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var filter = tbSearch.Text?.Trim().ToLower() ?? "";
            lbFonts.ItemsSource = string.IsNullOrEmpty(filter)
                ? allFonts
                : allFonts.Where(f => f.ToLower().Contains(filter)).ToList();
        }

        private void lbFonts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbFonts.SelectedItem is string font)
            {
                SelectedFontFamily = font;
                UpdatePreview();
            }
        }

        private void slSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SelectedFontSize = Math.Round(slSize.Value);
            if (tbSizeValue != null)
                tbSizeValue.Text = SelectedFontSize.ToString();
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            if (tbPreview == null) return;
            tbPreview.FontFamily = new FontFamily(SelectedFontFamily);
            tbPreview.FontSize = SelectedFontSize;
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
