using System.Diagnostics;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
namespace CreationEditor.Render.View;

public class RenderViewHost : NativeControlHost {
    private readonly Process _process;
    private Window? _rootWindow;
    
    private static readonly MethodInfo DestroyNativeControl = typeof(NativeControlHost).GetMethod("DestroyNativeControl", BindingFlags.Instance | BindingFlags.NonPublic)!;

    public RenderViewHost(Process process) {
        _process = process;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
        if (e.Root is not Window { PlatformImpl: {} } window) return;

        _rootWindow = window;

        WinHelper.SetParent(_process.MainWindowHandle, _rootWindow.PlatformImpl.Handle.Handle);

        long style = WinHelper.GetWindowLongPtr(_process.MainWindowHandle, WinHelper.StyleIndex);
        style |= (long) WinHelper.WinStyle.Child;
        style &= (long) ~WinHelper.WinStyle.ResizeBar;
        style &= (long) ~WinHelper.WinStyle.Caption;

        WinHelper.SetWindowLongPtr(_process.MainWindowHandle, WinHelper.StyleIndex, (nint) style);

        DestroyNativeControl.Invoke(this, null);

        base.OnAttachedToVisualTree(e);
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent) {
        switch (System.Environment.OSVersion.Platform) {
            case PlatformID.Win32S:
            case PlatformID.Win32Windows:
            case PlatformID.Win32NT:
            case PlatformID.WinCE:
                return new PlatformHandle(_process.MainWindowHandle, "Viewport");
            case PlatformID.Unix:
                break;
            case PlatformID.Xbox:
                break;
            case PlatformID.MacOSX:
                break;
            case PlatformID.Other:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return base.CreateNativeControlCore(parent);
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control) {
        switch (System.Environment.OSVersion.Platform) {
            case PlatformID.Win32S:
            case PlatformID.Win32Windows:
            case PlatformID.Win32NT:
            case PlatformID.WinCE:
                break;
            case PlatformID.Unix:
                break;
            case PlatformID.Xbox:
                break;
            case PlatformID.MacOSX:
                break;
            case PlatformID.Other:
                break;
            default:
                base.DestroyNativeControlCore(control);
                break;
        }
    }
}
