using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;


namespace MotionDetectorNet.Helpers;

internal static class WindowTools
{
    public static void HideWindowButtons(Window window)
    {
        if (window.IsLoaded)
        {
            RemoveSysMenu(window);
        }
        else
        {
            window.Loaded += (s, e) => RemoveSysMenu(window);
        }
    }
    public static void HideWindowMinMaxButtons(Window window)
    {
        if (window.IsLoaded)
        {
            RemoveMinMaxButtons(window);
        }
        else
        {
            window.Loaded += (s, e) => RemoveMinMaxButtons(window);
        }
    }

    public static void SetCentralPosition(Window window)
    {
        if (!Application.Current.MainWindow.IsLoaded || Application.Current.Dispatcher.Thread != Thread.CurrentThread)
        {
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.ShowInTaskbar = true;
        }
        else
        {
            window.Owner = Application.Current.MainWindow;
        }
    }

    public static T ShowSafe<T>(Func<T> createAndShow)
    {
        if (Application.Current.Dispatcher.Thread != Thread.CurrentThread)
        {
            return Application.Current.Dispatcher.Invoke(createAndShow);
        }
        else
        {
            return createAndShow();
        }
    }

    // Internal

    const int GWL_STYLE = -16;
    const int WS_SYSMENU = 0x80000;
    const int WS_MAXIMIZEBOX = 0x00010000;
    const int WS_MINIMIZEBOX = 0x00020000;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(nint hWnd, int nIndex);
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);

    private static void RemoveSysMenu(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        _ = SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
    }

    private static void RemoveMinMaxButtons(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        _ = SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~(WS_MAXIMIZEBOX | WS_MINIMIZEBOX));
    }
}