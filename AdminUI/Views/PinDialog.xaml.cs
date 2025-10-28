using System.Windows;
using System.Windows.Controls;
using Utility;

namespace AdminUI.Views
{
    public partial class PinDialog : Window
    {
        public string EnteredPin => txtPIN.Password;

        public PinDialog()
        {
            InitializeComponent();
            txtPIN.Focus();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
