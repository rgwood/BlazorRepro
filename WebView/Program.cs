namespace WebView;
using Microsoft.Web.WebView2.Core;
using Microsoft.AspNetCore.Builder;
using System.Drawing;
using System.Reactive.Linq;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

class Program
{
    internal const uint WM_SYNCHRONIZATIONCONTEXT_WORK_AVAILABLE = Constants.WM_USER + 1;
    private static CoreWebView2Controller _controller;
    private static UiThreadSynchronizationContext _uiThreadSyncCtx;
    private static WebApplication _webApp;

    [STAThread]
    static int Main(string[] args)
    {
        HWND hwnd;

        unsafe
        {
            HINSTANCE hInstance = PInvoke.GetModuleHandle((char*)null);
            ushort classId;

            fixed (char* classNamePtr = "WebView")
            {
                WNDCLASSW wc = new WNDCLASSW();
                wc.lpfnWndProc = WndProc;
                wc.lpszClassName = classNamePtr;
                wc.hInstance = hInstance;
                wc.style = WNDCLASS_STYLES.CS_VREDRAW | WNDCLASS_STYLES.CS_HREDRAW;
                classId = PInvoke.RegisterClass(wc);
            }

            fixed (char* windowNamePtr = nameof(WebView))
            {
                hwnd = PInvoke.CreateWindowEx(
                    0,
                    (char*)classId,
                    windowNamePtr,
                    WINDOW_STYLE.WS_OVERLAPPEDWINDOW,
                    Constants.CW_USEDEFAULT, Constants.CW_USEDEFAULT, 800, 900,
                    new HWND(),
                    new HMENU(),
                    hInstance,
                    null);
            }
        }

        PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_NORMAL);

        _uiThreadSyncCtx = new UiThreadSynchronizationContext(hwnd);
        SynchronizationContext.SetSynchronizationContext(_uiThreadSyncCtx);

        InitializeWebView2AndBlazor(hwnd);

        MSG msg;
        while (PInvoke.GetMessage(out msg, new HWND(), 0, 0))
        {
            PInvoke.TranslateMessage(msg);
            PInvoke.DispatchMessage(msg);
        }

        return (int)msg.wParam.Value;
    }

    private static LRESULT WndProc(HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam)
    {
        switch (msg)
        {
            case Constants.WM_SIZE:
                OnSize(hwnd, wParam, GetLowWord(lParam.Value), GetHighWord(lParam.Value));
                break;
            case WM_SYNCHRONIZATIONCONTEXT_WORK_AVAILABLE:
                _uiThreadSyncCtx.RunAvailableWorkOnCurrentThread();
                break;
            case Constants.WM_CLOSE:
                if(_webApp is not null)
                {
                    _uiThreadSyncCtx.Post(async _ => await _webApp.StopAsync(new CancellationTokenSource(millisecondsDelay: 1000).Token), null);
                    _uiThreadSyncCtx.Post(async _ => await _webApp.DisposeAsync(), null);
                }
                PInvoke.PostQuitMessage(0);
                break;
        }

        return PInvoke.DefWindowProc(hwnd, msg, wParam, lParam);
    }

    private static async void InitializeWebView2AndBlazor(HWND hwnd)
    {
        CoreWebView2Environment environment = await CoreWebView2Environment.CreateAsync(null, null, null);
        _controller = await environment.CreateCoreWebView2ControllerAsync(hwnd);
        PInvoke.GetClientRect(hwnd, out var hwndRect);
        _controller.Bounds = new Rectangle(0, 0, hwndRect.right, hwndRect.bottom);
        _controller.IsVisible = true;

        // Navigate to a local folder, just to confirm that WebView2 is working
        _controller.CoreWebView2.SetVirtualHostNameToFolderMapping("webview.example", "staticfiles", CoreWebView2HostResourceAccessKind.Allow);
        _controller.CoreWebView2.Navigate("https://webview.example/index.html");

        // Spin up ASP.NET and navigate to it
        _webApp = await BlazorServer.Hosting.StartOnThreadpool();
        _controller.CoreWebView2.Navigate("http://localhost:5003/");
    }

    #region Helper Methods
    private static void OnSize(HWND hwnd, WPARAM wParam, int width, int height)
    {
        if (_controller != null)
            _controller.Bounds = new Rectangle(0, 0, width, height);
    }

    private static int GetLowWord(nint value)
    {
        uint xy = (uint)value;
        int x = unchecked((short)xy);
        return x;
    }

    private static int GetHighWord(nint value)
    {
        uint xy = (uint)value;
        int y = unchecked((short)(xy >> 16));
        return y;
    }
    #endregion
}
