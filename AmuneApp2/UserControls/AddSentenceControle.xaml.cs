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
    /// <summary>
    /// Interaction logic for AddSentenceControle.xaml
    /// </summary>
    public partial class AddSentenceControle : UserControl
    {
        public string Text 
        {
            get { return tbAddSentence.Text; }
            set { tbAddSentence.Text = value; }
        }
        public StackPanel parentStackPanel;
        public AddSentenceControle()
        {

            InitializeComponent();
           
        }

        private void btnDelete_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Check if the UserControl's parent is a Panel
            if (parentStackPanel != null)
            {
                // Remove this UserControl from the parent Panel
                parentStackPanel.Children.Remove(this);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            tbAddSentence.Focus();
            tbAddSentence.Select(tbAddSentence.Text.Length , 0);
        }
    }
}
