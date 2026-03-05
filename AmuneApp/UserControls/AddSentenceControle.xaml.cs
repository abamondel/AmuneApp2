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

namespace AmuneApp.UserControls
{
    public partial class AddSentenceControle : UserControl
    {
        public StackPanel ParentStackPanel { get; set; }

        public string Text
        {
            get => tbAddSentence.Text;
            set => tbAddSentence.Text = value;
        }

        public AddSentenceControle()
        {
            InitializeComponent();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ParentStackPanel?.Children.Remove(this);
        }
    }
}
