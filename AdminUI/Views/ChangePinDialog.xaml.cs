using System.Windows;

namespace AdminUI.Views
{
    public partial class ChangePinDialog : Window
    {
        public string OldPin => OldPinBox.Password;
        public string NewPin => NewPinBox.Password;

        public ChangePinDialog()
        {
            InitializeComponent();
            OldPinBox.Focus();
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(OldPin) || string.IsNullOrWhiteSpace(NewPin))
            {
                MessageBox.Show("Both fields are required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
