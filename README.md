# Swift Kiosk Browser
WPF application using WebView2 to show window in full screen (kiosk mode) without title bar.

**Usage**
```console
KioskBrowser.exe "http://www.google.com"
```

**Options:**

| Option | Description |
| --- | --- |
| <code>-t, --enable-titlebar</code>| Enable Title bar |
| <code>-r, --enable-content-refresh</code> |  (default: disabled) Enable automatic refresh of content |
| <code>--content-refresh-interval</code> | (min: 10, max: 3600, default: 60) Content refresh interval in seconds |

Close window by pressing ESC. (Disabled when title bar is enabled)


