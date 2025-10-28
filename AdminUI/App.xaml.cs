using AdminUI.Security;
using System.Configuration;
using System.Data;
using System.Windows;
using Utility;

namespace AdminUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
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
                    Dispatcher.BeginInvoke(new Action(() => Shutdown()));
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
            var main = new MainWindow();
            MainWindow = main;
            main.Show();
        }


        //private void Application_Startup(object sender, StartupEventArgs e)
        //{
        //    // Initialize the PIN store (creates pin.json if missing)
        //    PinStore.Initialize();

        //    bool isAuthenticated = false;

        //    while (!isAuthenticated)
        //    {
        //        var dlg = new Views.PinDialog();

        //        // If user cancels (closes dialog or presses Cancel)
        //        if (dlg.ShowDialog() != true)
        //        {
        //            Shutdown();
        //            return;
        //        }

        //        // Verify PIN asynchronously
        //        bool ok = PinStore.VerifyPinAsync(dlg.EnteredPin).Result;

        //        if (ok)
        //        {
        //            isAuthenticated = true;
        //        }
        //        else
        //        {
        //            MessageBox.Show("❌ Incorrect PIN. Please try again.",
        //                "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        }
        //    }

        //    // Proceed to main window only after successful authentication
        //    var main = new MainWindow();
        //    main.Show();
        //}

    }

}
