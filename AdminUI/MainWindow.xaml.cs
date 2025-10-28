using AdminUI.Security;
using System.Windows;

namespace AdminUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            InitializeComponent();
            AskForPIN();
        }
        private async void ChangePin_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Views.ChangePinDialog { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                bool success = await PinStore.ChangePinAsync(dlg.OldPin, dlg.NewPin);
                if (success)
                {
                    MessageBox.Show("✅ PIN changed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("❌ Failed to change PIN (wrong old PIN?).", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AskForPIN()
        {
            // Ensure the PIN store exists
            PinStore.Initialize();

            bool isAuthenticated = false;

            while (!isAuthenticated)
            {
                // Create a new dialog instance each time
                var dlg = new Views.PinDialog();

                bool? result = dlg.ShowDialog();

                // User clicked Cancel or closed the dialog
                if (result != true)
                {
                    Dispatcher.BeginInvoke(new Action(() => Application.Current.Shutdown()));
                    return;
                }

                // Verify the entered PIN
                bool ok = PinStore.VerifyPin(dlg.EnteredPin);

                if (ok)
                {
                    isAuthenticated = true;
                }
                else
                {
                    // Tell the user and loop again
                    MessageBox.Show("❌ Incorrect PIN. Please try again.",
                        "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            // Only show main window after successful authentication
            //var main = new MainWindow();
            //MainWindow = main;
            //this.Show();
        }
    }
}