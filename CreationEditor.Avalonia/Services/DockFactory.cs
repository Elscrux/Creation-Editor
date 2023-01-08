using Autofac;
using Avalonia.Controls;
using CreationEditor.Avalonia.Models;
using CreationEditor.Avalonia.Models.Docking;
using CreationEditor.Avalonia.Services.Docking;
using CreationEditor.Avalonia.ViewModels.Logging;
using CreationEditor.Avalonia.ViewModels.Record.Browser;
using CreationEditor.Avalonia.Views;
using CreationEditor.Avalonia.Views.Logging;
using CreationEditor.Avalonia.Views.Record;
using CreationEditor.Render.View;
using MutagenLibrary.Core.Plugins;
namespace CreationEditor.Avalonia.Services;

public sealed class DockFactory : IDockFactory {
    private readonly IMainWindow _mainWindow;
    private readonly IComponentContext _componentContext;
    private readonly IDockingManagerService _dockingManagerService;
    private readonly ISimpleEnvironmentContext _simpleEnvironmentContext;

    public DockFactory(
        IMainWindow mainWindow,
        IComponentContext componentContext,
        IDockingManagerService dockingManagerService,
        ISimpleEnvironmentContext simpleEnvironmentContext) {
        _mainWindow = mainWindow;
        _componentContext = componentContext;
        _dockingManagerService = dockingManagerService;
        _simpleEnvironmentContext = simpleEnvironmentContext;
    }
    
    public void Open(DockElement dockElement, DockMode? dockMode = null, Dock? dock = null) {
        Control control;
        DockConfig dockConfig;
        
        switch (dockElement) {
            case DockElement.Log:
                control = new LogView(_componentContext.Resolve<ILogVM>());
                dockConfig = new DockConfig {
                    DockInfo = new DockInfo {
                        Header = "Log",
                    },
                    Dock = Dock.Bottom,
                    DockMode = DockMode.Side,
                };
                break;
            case DockElement.RecordBrowser:
                control = new RecordBrowser(_componentContext.Resolve<IRecordBrowserVM>());
                dockConfig = new DockConfig {
                    DockInfo = new DockInfo {
                        Header = "Record Browser",
                    },
                    Dock = Dock.Left,
                    DockMode = DockMode.Layout,
                    Size = new GridLength(3, GridUnitType.Star),
                };
                break;
            case DockElement.Viewport:
                control = new RenderView(_simpleEnvironmentContext);
                dockConfig = new DockConfig {
                    DockInfo = new DockInfo {
                        Header = "Viewport",
                    },
                    Dock = Dock.Right,
                    DockMode = DockMode.Layout,
                    Size = new GridLength(3, GridUnitType.Star),
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dockElement), dockElement, null);
        }

        if (dockMode != null) dockConfig = dockConfig with { DockMode = dockMode.Value };
        if (dock != null) dockConfig = dockConfig with { Dock = dock.Value };

        _dockingManagerService.AddControl(control, dockConfig);
    }
}
