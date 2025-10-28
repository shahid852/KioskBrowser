using System.Windows;
using Utility;

namespace AdminUI.Views
{
    public partial class PinDialog : Window
    {
        public string EnteredPin => PinBox.Password;

        public PinDialog()
        {
            InitializeComponent();
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
