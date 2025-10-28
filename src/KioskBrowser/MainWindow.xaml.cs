using System.Windows.Interop;
using CommandLine;
using System.Windows;
using static KioskBrowser.Native.ShellHelper;


namespace KioskBrowser;

public partial class MainWindow
{
    private readonly MainViewModel _viewModel;
    private readonly NavigationService _navigationService;
    private bool _customURLProvided = false;
    private readonly string _customURL = "";
    
    public MainWindow(string customURL = "")
    {
        _customURLProvided = customURL != "";
        _customURL = customURL;
        _navigationService = new NavigationService();
        var storeService = new StoreService();

        _viewModel = new MainViewModel(Close, _navigationService, storeService);
        DataContext = _viewModel;

        InitializeComponent();

        WindowStyle = WindowStyle.None;
        this.ResizeMode = ResizeMode.NoResize;
        this.WindowState = WindowState.Maximized;
        this.Topmost = true;
        
        Loaded += async (_, _) =>
        {
            await _viewModel.CheckForUpdateAsync();
        };
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        _navigationService.SetNavigationFrame(MainFrame);
        ParseCommandLine(_customURLProvided, _customURL);
        

    }

    private void ParseCommandLine(bool skipCmdArgs = false, string url = "")
    {
        if (skipCmdArgs)
        {
            Options o = new();
            o.Url = url;
            _viewModel.Initialize(o);
        }
        else
        {
            var args = Environment.GetCommandLineArgs().Skip(1);
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o => _viewModel.Initialize(o));
        }
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);

        _navigationService.Navigate<BrowserPage>();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        var hwnd = new WindowInteropHelper(this).Handle;

        SetAppUserModelId(hwnd, "KioskBrowser" + Guid.NewGuid());
    }

    private void WebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
    {

    }
}