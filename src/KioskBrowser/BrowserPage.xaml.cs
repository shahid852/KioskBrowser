using System.Windows.Controls;
using Microsoft.Web.WebView2.Wpf;

namespace KioskBrowser;

public partial class BrowserPage : Page
{
    public BrowserPage(WebView2 webView)
    {
        InitializeComponent();
        
        DataContext = this;
        WebView = webView;
        //WebView.NavigationCompleted += WebView_NavigationCompleted;
    }

    public WebView2 WebView { get; set; }
    //private void WebView_NavigationCompleted(object sender, 
    //    Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
    //{
    //    // Handle navigation completed event if needed
    //    WebView.Visibility = System.Windows.Visibility.Hidden;
    //}
}