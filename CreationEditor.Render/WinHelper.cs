using System.Runtime.InteropServices;
namespace CreationEditor.Render;

public static class WinHelper {
    //Sets a window to be a child window of another window
    [DllImport("user32.dll")]
    public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll")]
    public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

    //Sets window attributes
    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, WinStyle dwNewLong);
    
    [DllImport("user32.dll")]
    public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")]
    public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    public const int StyleIndex = -16;

    [Flags]
    public enum WinStyle {
        Visible = 0x10000000,
        Child = 0x40000000, //child window
        Border = 0x00800000, //window with border
        Frame = 0x00400000, //window with double border but no title
        ResizeBar = 0x00040000, //resize window bar
        Caption = Border | Frame //window with a title bar
    }
}
