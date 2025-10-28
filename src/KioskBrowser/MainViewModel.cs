using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KioskBrowser.Common;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.IO;
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
        Url = options.Url ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "readme.html");;
        TitlebarEnabled = options.Url is not null && options.EnableTitlebar;
        TitlebarHeight = options.EnableTitlebar ? 48 : 0;
        RefreshContentEnabled = options.EnableAutomaticContentRefresh;
        RefreshContentIntervalInSeconds = Math.Max(Math.Min(options.ContentRefreshIntervalInSeconds, 3600), 10);
        
        SetIcons(Url);
        
        RegisterPages();
        
        _webView.Loaded += async (_, _) => await InitializeWebView();
        _webView.NavigationCompleted += _webView_NavigationCompleted;
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
        if(_webView.CoreWebView2 != null)
            return;
    
        var environment = await CoreWebView2Environment.CreateAsync(null, CacheFolderPath, new CoreWebView2EnvironmentOptions
        {
            AllowSingleSignOnUsingOSPrimaryAccount = true
        });
        await _webView.EnsureCoreWebView2Async(environment);
        
        if(_webView.CoreWebView2 == null)
            throw new Exception("Failed to initialize WebView control. Please restart application.");

        _webView.CoreWebView2.DocumentTitleChanged += (_, _) =>
        {
            var title = _webView.CoreWebView2.DocumentTitle;
            if(!string.IsNullOrEmpty(title))
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
        if(image == null) return;
            
        TitlebarIcon = image;
        TaskbarOverlayImage = image;
    }
    
    private void StartAutomaticContentRefresh()
    {
        _refreshContentTimer.Tick += (_, _) => _webView.Reload();
        _refreshContentTimer.Interval = TimeSpan.FromSeconds(RefreshContentIntervalInSeconds);
        _refreshContentTimer.Start();
    }
}