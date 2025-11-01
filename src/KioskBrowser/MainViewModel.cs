using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KioskBrowser.Common;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static System.Net.WebRequestMethods;

namespace KioskBrowser;

public partial class MainViewModel(Action close, NavigationService navigationService, StoreService storeService) : ObservableObject
{
    private readonly DispatcherTimer _refreshContentTimer = new();
    private readonly WebView2 _webView = new();

    private string Url { get; set; } = default!;
    private static string CacheFolderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KioskBrowser");
    private bool RefreshContentEnabled { get; set; }
    private double RefreshContentIntervalInSeconds { get; set; }

    [ObservableProperty]
    private string _title = "Kiosk Browser";

    [ObservableProperty]
    private int _titlebarHeight;

    [ObservableProperty]
    private BitmapImage _titlebarIcon = new(new Uri("pack://application:,,,/Images/app.png"));

    [ObservableProperty]
    private BitmapImage? _taskbarOverlayImage;

    [ObservableProperty]
    private bool _isUpdateAvailable = false;

    public async Task CheckForUpdateAsync()
    {
        IsUpdateAvailable = await storeService.IsUpdateAvailableAsync();
    }

    public bool TitlebarEnabled { get; private set; } = true;

    [RelayCommand]
    private void Close()
    {
        if (!TitlebarEnabled)
            close();
    }

    [RelayCommand]
    private void ShowAboutPage()
    {
        navigationService.Navigate<AboutPage>();
    }

    public void Initialize(Options options)
    {
        try
        {
            //_ = InitWebViewAsync();
            Url = options.Url ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "readme.html");
            TitlebarEnabled = options.Url is not null && options.EnableTitlebar;
            TitlebarHeight = options.EnableTitlebar ? 48 : 0;
            RefreshContentEnabled = options.EnableAutomaticContentRefresh;
            RefreshContentIntervalInSeconds = Math.Max(Math.Min(options.ContentRefreshIntervalInSeconds, 3600), 10);

            SetIcons(Url);

            RegisterPages();

            _webView.Loaded += async (_, _) => await InitializeWebView();
            _webView.NavigationCompleted += _webView_NavigationCompleted;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
   
    private async void _webView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        try
        {
            // Wait a bit to ensure dynamic content has loaded
            await Task.Delay(3000);

            string js = @"
            const fsButton = document.querySelector('.fullscreen-button, [aria-label=""Full screen""]');
            if (fsButton) {
                fsButton.click();
            } else {
                console.log('Fullscreen button not found');
            }
        ";

            await _webView.CoreWebView2.ExecuteScriptAsync(js);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] Failed to enter fullscreen: " + ex.Message);
        }
    }

    private async Task InitializeWebView()
    {
        if (_webView.CoreWebView2 != null)
            return;

        var environment = await CoreWebView2Environment.CreateAsync(null, CacheFolderPath, new CoreWebView2EnvironmentOptions
        {
            AllowSingleSignOnUsingOSPrimaryAccount = true
        });
        await _webView.EnsureCoreWebView2Async(environment);

        if (_webView.CoreWebView2 == null)
            throw new Exception("Failed to initialize WebView control. Please restart application.");

        _ = InitWebViewAsyncNEW();
        //_ = InitWebViewAutoDetectAsync();

        _webView.CoreWebView2.DocumentTitleChanged += (_, _) =>
        {
            var title = _webView.CoreWebView2.DocumentTitle;
            if (!string.IsNullOrEmpty(title))
                Title = title;
        };

        _webView.CoreWebView2.FaviconChanged += async (_, _) =>
        {
            var faviconUri = _webView.CoreWebView2.FaviconUri;
            if (faviconUri == null) return;

            var image = await FaviconIcon.DownloadAsync(faviconUri);
            if (image == null) return;

            TitlebarIcon = image;
            TaskbarOverlayImage = image;
        };


        _webView.Source = new UriBuilder(Url).Uri;

        if (RefreshContentEnabled)
            StartAutomaticContentRefresh();
    }
    private async Task InitWebViewAsyncOLD()
    {
      

        // 1) Set the User-Agent to a Firefox mobile string
        string firefoxMobileUA = "Mozilla/5.0 (Android 13; Mobile; rv:122.0) Gecko/122.0 Firefox/122.0";
        _webView.CoreWebView2.Settings.UserAgent = firefoxMobileUA;

        // 2) Optionally set the ZoomFactor (page scale)
        // e.g., 1.0 = 100%, 0.8 = 80% (smaller), 1.25 = 125% (bigger)
        //_webView.CoreWebView2.ZoomFactor = 1.0;

        // 3) Inject viewport meta tag early (so pages without it still render mobile)
        string injectViewportMeta = @"
        (function() {
          if (!document.querySelector('meta[name=viewport]')) {
            var m = document.createElement('meta');
            m.name = 'viewport';
            m.content = 'width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=0';
            document.head.appendChild(m);
          }
        })();";
        await _webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(injectViewportMeta);

        // 4) Use DevTools Protocol to set device metrics and enable touch emulation
        // Choose metrics that match a mobile device (e.g., Pixel 6 / iPhone-like values).
        // width/height are in CSS pixels.
        var deviceMetrics = new
        {
            width = 1080,                // viewport width in CSS px (example: iPhone 14 ~390)
            height = 1920,               // viewport height in CSS px
            deviceScaleFactor = 3.0,    // DPR (devicePixelRatio)
            mobile = true,              // true to enable mobile rendering (touch UA/layout)
                                        // optional: screenWidth, screenHeight, positionX, positionY
        };

        // set device metrics
        string deviceMetricsJson = System.Text.Json.JsonSerializer.Serialize(deviceMetrics);
        await _webView.CoreWebView2.CallDevToolsProtocolMethodAsync("Emulation.setDeviceMetricsOverride", deviceMetricsJson);

        // enable touch emulation
        var touchParams = new { enabled = true, maxTouchPoints = 1 };
        string touchJson = System.Text.Json.JsonSerializer.Serialize(touchParams);
        await _webView.CoreWebView2.CallDevToolsProtocolMethodAsync("Emulation.setTouchEmulationEnabled", touchJson);

        // (Optional) Tell DevTools to report the same UA explicitly (some sites check via DevTools)
        var uaParams = new { userAgent = firefoxMobileUA };
        string uaJson = System.Text.Json.JsonSerializer.Serialize(uaParams);
        await _webView.CoreWebView2.CallDevToolsProtocolMethodAsync("Emulation.setUserAgentOverride", uaJson);

        // 5) Resize your WebView control in the XAML or code-behind to match the CSS viewport if you want
        // Example: you could set Width = 390 and Height = 844 on the control or place it inside a container sized accordingly.
        // If you keep the window larger, pages still render responsive mobile layout, but you'll see the "mobile-width" content centered.

        // Now navigate
        //_webView.Source = new Uri("https://www.example.com/");

        // If you changed the UA after a page was already loaded, consider Reload:
        // _webView.CoreWebView2.Reload();
    }

    private async Task InitWebViewAsyncNEW()
    {
        // Wait for WebView2 to be ready
        await _webView.EnsureCoreWebView2Async();

        // 🔹 Use a mobile User-Agent (unchanged)
        string firefoxMobileUA = "Mozilla/5.0 (Android 13; Mobile; rv:122.0) Gecko/122.0 Firefox/122.0";
        _webView.CoreWebView2.Settings.UserAgent = firefoxMobileUA;

        // 🔹 Inject mobile viewport script (unchanged)
        string injectViewportMeta = @"
    (function() {
      if (!document.querySelector('meta[name=viewport]')) {
        var m = document.createElement('meta');
        m.name = 'viewport';
        m.content = 'width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=0';
        document.head.appendChild(m);
      }
    })();";
        await _webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(injectViewportMeta);

        // ✅ Get real screen resolution (instead of fixed values)
        var screenWidth = (int)SystemParameters.PrimaryScreenWidth;
        var screenHeight = (int)SystemParameters.PrimaryScreenHeight;

        var deviceMetrics = new
        {
            width = screenWidth,
            height = screenHeight,
            deviceScaleFactor = 3.0,
            mobile = true
        };
        string deviceMetricsJson = System.Text.Json.JsonSerializer.Serialize(deviceMetrics);
        await _webView.CoreWebView2.CallDevToolsProtocolMethodAsync("Emulation.setDeviceMetricsOverride", deviceMetricsJson);

        // 🔹 Enable touch simulation (unchanged)
        var touchParams = new { enabled = true, maxTouchPoints = 10 };
        string touchJson = System.Text.Json.JsonSerializer.Serialize(touchParams);
        await _webView.CoreWebView2.CallDevToolsProtocolMethodAsync("Emulation.setTouchEmulationEnabled", touchJson);
        
        // 🔹 Navigation left untouched
        //_webView.Source = new Uri("https://www.example.com/");
    }


    private void RegisterPages()
    {
        var browserPage = new BrowserPage(_webView);
        var aboutPage = new AboutPage(navigationService, storeService);

        navigationService.AddPage(browserPage);
        navigationService.AddPage(aboutPage);
    }

    private void SetIcons(string url)
    {
        if (!FileUtils.IsFilePath(url)) return;

        var image = FileUtils.GetFileIcon(url);
        if (image == null) return;

        TitlebarIcon = image;
        TaskbarOverlayImage = image;
    }

    private void StartAutomaticContentRefresh()
    {
        _refreshContentTimer.Tick += (_, _) => _webView.Reload();
        _refreshContentTimer.Interval = TimeSpan.FromSeconds(RefreshContentIntervalInSeconds);
        _refreshContentTimer.Start();
    }

    //private async Task InitWebViewAutoDetectAsync()
    //{
    //    // Ensure WebView2 is initialized first
    //    await _webView.EnsureCoreWebView2Async();

    //    // Set mobile UA
    //    string firefoxMobileUA = "Mozilla/5.0 (Android 13; Mobile; rv:122.0) Gecko/122.0 Firefox/122.0";
    //    _webView.CoreWebView2.Settings.UserAgent = firefoxMobileUA;

    //    // Inject viewport meta tag early
    //    string injectViewportMeta = @"
    //    (function() {
    //      if (!document.querySelector('meta[name=viewport]')) {
    //        var m = document.createElement('meta');
    //        m.name = 'viewport';
    //        m.content = 'width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=0';
    //        document.head.appendChild(m);
    //      }
    //    })();";
    //    await _webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(injectViewportMeta);

    //    // Get monitor pixel bounds and DPI for the window's monitor
    //    var monitorInfo = GetMonitorBoundsAndDpi(this);
    //    int pxWidth = monitorInfo.WidthPx;
    //    int pxHeight = monitorInfo.HeightPx;
    //    double dpiX = monitorInfo.DpiX;
    //    double dpiY = monitorInfo.DpiY;

    //    // Compute approximate DPI (use X for simplicity) and scale factor (DPI / 96)
    //    double dpi = (dpiX + dpiY) / 2.0;
    //    double scaleFactor = dpi / 96.0; // this is the Windows scale (1.0, 1.25, 1.5, 2.0, etc.)

    //    // Compute CSS pixels width/height that correspond to browser CSS pixels
    //    double cssWidth = pxWidth / scaleFactor;
    //    double cssHeight = pxHeight / scaleFactor;

    //    // Compute physical diagonal in inches
    //    double diagInches = Math.Sqrt(pxWidth * pxWidth + pxHeight * pxHeight) / dpi;

    //    // Decide whether to emulate a phone viewport (small width) or use the actual screen size
    //    // For big kiosks we usually emulate the phone viewport so sites respond with mobile layout
    //    bool emulatePhone = diagInches >= LargeScreenInchesThreshold;

    //    // Prepare device metrics
    //    int emWidth;
    //    int emHeight;
    //    double deviceDPR;

    //    if (emulatePhone)
    //    {
    //        // Phone preset (you can pick iPhone-ish or Android-ish)
    //        emWidth = 390;                         // CSS px width we want the site to "think" it has
    //        emHeight = 844;                        // CSS px height
    //        // Choose DPR based on actual scale; clamp to common values
    //        deviceDPR = Math.Clamp(scaleFactor, 1.0, 3.0);
    //    }
    //    else
    //    {
    //        // Use the actual CSS width/height computed from monitor pixels & Windows scale
    //        emWidth = (int)Math.Round(cssWidth);
    //        emHeight = (int)Math.Round(cssHeight);
    //        deviceDPR = Math.Clamp(scaleFactor, 1.0, 3.0);
    //    }

    //    // Compose JSON for DevTools call
    //    var deviceMetrics = new
    //    {
    //        width = emWidth,
    //        height = emHeight,
    //        deviceScaleFactor = deviceDPR,
    //        mobile = true
    //    };
    //    string deviceMetricsJson = System.Text.Json.JsonSerializer.Serialize(deviceMetrics);
    //    await _webView.CoreWebView2.CallDevToolsProtocolMethodAsync("Emulation.setDeviceMetricsOverride", deviceMetricsJson);

    //    // Enable touch emulation (so touch handlers run)
    //    var touchParams = new { enabled = true, maxTouchPoints = 1 };
    //    string touchJson = System.Text.Json.JsonSerializer.Serialize(touchParams);
    //    await _webView.CoreWebView2.CallDevToolsProtocolMethodAsync("Emulation.setTouchEmulationEnabled", touchJson);

    //    // Optionally override user agent on the DevTools side
    //    var uaParams = new { userAgent = firefoxMobileUA };
    //    string uaJson = System.Text.Json.JsonSerializer.Serialize(uaParams);
    //    await _webView.CoreWebView2.CallDevToolsProtocolMethodAsync("Emulation.setUserAgentOverride", uaJson);

    //    // Optional: set WebView zoom to better match physical visual scale on big screens
    //    // Example: scale down content a bit if you emulate a 390px viewport on a huge monitor
    //    if (emulatePhone)
    //    {
    //        // Compute suggested zoom so the emulated viewport appears reasonably sized on the big screen
    //        // This is heuristic: you can tune to taste.
    //        double visualZoom = Math.Min(1.0, (cssWidth / emWidth)); // if CSS width >> emWidth, scale down
    //        _webView.CoreWebView2.ZoomFactor = visualZoom;
    //    }

    //    // navigate after settings applied
    //    _webView.Source = new Uri("https://www.example.com/");
    //}

    // Helper: retrieve monitor bounds (px) and DPI for the monitor containing the WPF window
    //private (int WidthPx, int HeightPx, double DpiX, double DpiY) GetMonitorBoundsAndDpi(Window w)
    //{
    //    var helper = new WindowInteropHelper(w);
    //    IntPtr hwnd = helper.Handle;

    //    // If window has no handle yet, use primary screen
    //    if (hwnd == IntPtr.Zero)
    //    {
    //        var primary = System.Windows.Forms.Screen.PrimaryScreen;
    //        using (GraphicsH g = new GraphicsH())
    //        {
    //            // fallback DPI query
    //            double dpiX = g.DpiX;
    //            double dpiY = g.DpiY;
    //            return (primary.Bounds.Width, primary.Bounds.Height, dpiX, dpiY);
    //        }
    //    }

    //    // Get Screen for the HWND
    //    var screen = System.Windows.Forms.Screen.FromHandle(hwnd);
    //    int wPx = screen.Bounds.Width;
    //    int hPx = screen.Bounds.Height;

    //    // Get DPI for the monitor
    //    uint dpiX_u = 96, dpiY_u = 96;
    //    IntPtr hmonitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
    //    if (GetDpiForMonitor(hmonitor, Monitor_DPI_Type.MDT_Default, out dpiX_u, out dpiY_u) != 0)
    //    {
    //        // fallback to using Graphics
    //        using (GraphicsH g = new GraphicsH())
    //        {
    //            return (wPx, hPx, g.DpiX, g.DpiY);
    //        }
    //    }

    //    return (wPx, hPx, dpiX_u, dpiY_u);
    //}

    // small helper class to get Graphics DPI via System.Drawing (disposable)
    private sealed class GraphicsH : IDisposable
    {
        System.Drawing.Graphics g;
        public GraphicsH() { g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero); }
        public double DpiX => g.DpiX;
        public double DpiY => g.DpiY;
        public void Dispose() { g.Dispose(); }
    }

    // --- PInvoke for monitor and DPI
    private const int MONITOR_DEFAULTTONEAREST = 2;

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    private enum Monitor_DPI_Type : int
    {
        MDT_Effective_DPI = 0,
        MDT_Angular_DPI = 1,
        MDT_Raw_DPI = 2,
        MDT_Default = MDT_Effective_DPI
    }

    [DllImport("Shcore.dll")]
    private static extern int GetDpiForMonitor(IntPtr hmonitor, Monitor_DPI_Type dpiType, out uint dpiX, out uint dpiY);
}
