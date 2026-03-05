using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AmuneApp.Dialogs
{
    public partial class ColorPickerDialog : Window
    {
        private bool suppressUpdate = false;
        private bool isDraggingSpectrum = false;
        private WriteableBitmap spectrumBitmap;

        public Color SelectedColor { get; private set; }

        private static readonly string[] SwatchColors = new[]
        {
            "#FFFFFF", "#C0C0C0", "#808080", "#404040", "#000000",
            "#FF0000", "#FF6600", "#FFCC00", "#33CC00", "#0099FF",
            "#6633FF", "#CC00CC", "#FF3366", "#FF9999", "#FFCC99",
        };

        public ColorPickerDialog(Color initialColor)
        {
            InitializeComponent();

            SelectedColor = initialColor;
            BuildSwatches();

            Loaded += (s, e) =>
            {
                SetupSpectrum();

                suppressUpdate = true;
                slR.Value = initialColor.R;
                slG.Value = initialColor.G;
                slB.Value = initialColor.B;
                suppressUpdate = false;

                UpdateFromSliders();
            };
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !spectrumCanvas.IsMouseOver)
                DragMove();
        }

        #region Spectrum

        private void SetupSpectrum()
        {
            double w = spectrumCanvas.ActualWidth;
            double h = spectrumCanvas.ActualHeight;
            if (w <= 0 || h <= 0) return;

            spectrumRect.Width = w;
            spectrumRect.Height = h;
            spectrumWhite.Width = w;
            spectrumWhite.Height = h;
            spectrumBlack.Width = w;
            spectrumBlack.Height = h;

            // Render to bitmap for pixel-accurate color picking
            RenderSpectrumBitmap((int)w, (int)h);
        }

        private void RenderSpectrumBitmap(int w, int h)
        {
            spectrumBitmap = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, null);
            byte[] pixels = new byte[w * h * 4];

            for (int y = 0; y < h; y++)
            {
                double brightness = 1.0 - (double)y / h;
                for (int x = 0; x < w; x++)
                {
                    double hue = (double)x / w * 360.0;
                    double saturation = 1.0;

                    // Top half: white to full color (saturation increases)
                    // Bottom half: full color to black (brightness decreases)
                    double s, v;
                    if (y < h / 2)
                    {
                        s = (double)y / (h / 2);
                        v = 1.0;
                    }
                    else
                    {
                        s = 1.0;
                        v = 1.0 - (double)(y - h / 2) / (h / 2);
                    }

                    var (r, g, b) = HsvToRgb(hue, s, v);
                    int idx = (y * w + x) * 4;
                    pixels[idx + 0] = b;
                    pixels[idx + 1] = g;
                    pixels[idx + 2] = r;
                    pixels[idx + 3] = 255;
                }
            }

            spectrumBitmap.WritePixels(new Int32Rect(0, 0, w, h), pixels, w * 4, 0);
        }

        private static (byte r, byte g, byte b) HsvToRgb(double h, double s, double v)
        {
            double c = v * s;
            double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            double m = v - c;
            double r, g, b;

            if (h < 60) { r = c; g = x; b = 0; }
            else if (h < 120) { r = x; g = c; b = 0; }
            else if (h < 180) { r = 0; g = c; b = x; }
            else if (h < 240) { r = 0; g = x; b = c; }
            else if (h < 300) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }

            return ((byte)((r + m) * 255), (byte)((g + m) * 255), (byte)((b + m) * 255));
        }

        private void Spectrum_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDraggingSpectrum = true;
            spectrumCanvas.CaptureMouse();
            PickColorFromSpectrum(e.GetPosition(spectrumCanvas));
        }

        private void Spectrum_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingSpectrum)
                PickColorFromSpectrum(e.GetPosition(spectrumCanvas));
        }

        private void Spectrum_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDraggingSpectrum = false;
            spectrumCanvas.ReleaseMouseCapture();
        }

        private void PickColorFromSpectrum(Point pos)
        {
            if (spectrumBitmap == null) return;

            int x = (int)Math.Clamp(pos.X, 0, spectrumBitmap.PixelWidth - 1);
            int y = (int)Math.Clamp(pos.Y, 0, spectrumBitmap.PixelHeight - 1);

            byte[] pixel = new byte[4];
            spectrumBitmap.CopyPixels(new Int32Rect(x, y, 1, 1), pixel, 4, 0);

            // Move crosshair
            Canvas.SetLeft(spectrumCursor, x - 8);
            Canvas.SetTop(spectrumCursor, y - 8);

            suppressUpdate = true;
            slR.Value = pixel[2];
            slG.Value = pixel[1];
            slB.Value = pixel[0];
            suppressUpdate = false;
            UpdateFromSliders();
        }

        #endregion

        #region Swatches

        private void BuildSwatches()
        {
            foreach (var hex in SwatchColors)
            {
                var color = (Color)ColorConverter.ConvertFromString(hex);
                var rect = new Rectangle
                {
                    Width = 24, Height = 24,
                    Margin = new Thickness(2),
                    Fill = new SolidColorBrush(color),
                    RadiusX = 4, RadiusY = 4,
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

        #endregion

        #region Sliders & Hex

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

        #endregion

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
