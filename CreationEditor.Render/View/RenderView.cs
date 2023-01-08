using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.Presenters;
using MutagenLibrary.Core.Plugins;
using Noggog;
namespace CreationEditor.Render.View;

public class RenderView : ContentPresenter {
    public RenderView(ISimpleEnvironmentContext environmentContext) {
        Interop.Start(environmentContext);
        
        var process = Process.GetProcessesByName("CreationEditor.Skyrim.Avalonia")
            .NotNull()
            .FirstOrDefault(p => p.MainWindowTitle == "BSE");

        if (process != null) Content = new RenderViewHost(process);
    }
}
