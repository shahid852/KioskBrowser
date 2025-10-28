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
