using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using MutagenLibrary.Core.Plugins;
using Noggog;
namespace CreationEditor.Render.Views;

public static class WinHelper {
    //Sets a window to be a child window of another window
    [DllImport("user32.dll")]
    public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll")]
    public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

    //Sets window attributes
    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, WinStyle dwNewLong);

    public const int StyleIndex = -16;

    [Flags]
    public enum WinStyle {
        Visible = 0x10000000,
        Child = 0x40000000, //child window
        Border = 0x00800000, //window with border
        Frame = 0x00400000, //window with double border but no title
        Caption = Border | Frame //window with a title bar
    }
}

public partial class RenderControl {
    private readonly ISimpleEnvironmentContext _environmentContext;
    private readonly Process? _process;

    public RenderControl(
        ISimpleEnvironmentContext environmentContext) {
        _environmentContext = environmentContext;
        InitializeComponent();
        
        Interop.Start(_environmentContext);

        _process = 
            Process.GetProcessesByName("CreationEditor.WPF.Skyrim")
            .NotNull()
            .FirstOrDefault(p => p.MainWindowTitle == "BSE");

        SizeChanged += OnSizeChanged;
    }

    private Visual? GetRootVisual() {
        var parent = VisualTreeHelper.GetParent(this);
        Visual? visualParent = null; 
        while (parent != null) {
            parent = VisualTreeHelper.GetParent(parent);
            if (parent is Visual visual) {
                visualParent = visual;
            }
        }

        return visualParent;
    }
    
    private void OnSizeChanged(object sender, SizeChangedEventArgs e) {
        if (_process == null) return;
        
        var visualParent = GetRootVisual();
        if (visualParent is Window parentWindow) {
            var parentHandle = new WindowInteropHelper(parentWindow).Handle;
            
            WinHelper.SetParent(_process.MainWindowHandle, parentHandle);
            WinHelper.SetWindowLong(_process.MainWindowHandle, WinHelper.StyleIndex, WinHelper.WinStyle.Visible);
            
            var topLeft = TransformToAncestor(visualParent).Transform(new Point(0, 0));
            var bottomRight = TransformToAncestor(visualParent).Transform(new Point(ActualWidth, ActualHeight));
            WinHelper.MoveWindow(_process.MainWindowHandle, (int) topLeft.X, (int) topLeft.Y, (int) bottomRight.X - (int) topLeft.X, (int) bottomRight.Y - (int) topLeft.Y, true);
        }
    }
}
