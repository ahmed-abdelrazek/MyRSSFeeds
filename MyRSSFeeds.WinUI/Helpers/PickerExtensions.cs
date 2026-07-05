namespace MyRSSFeeds.WinUI.Helpers
{
    // In WinUI 3 desktop apps, Windows.Storage.Pickers must be parented to a window handle
    // before use or they throw / fail silently.
    public static class PickerExtensions
    {
        public static T InitializeForMainWindow<T>(this T picker)
        {
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow));
            return picker;
        }
    }
}
