using System.Reactive.Subjects;
using Autofac;
using Avalonia.Controls;
using CreationEditor.Avalonia.Models.Docking;
using CreationEditor.Avalonia.Services.Docking;
using CreationEditor.Avalonia.ViewModels.Record.Editor;
using Elscrux.Logging;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Serilog;
namespace CreationEditor.Avalonia.Services.Record.Editor;

public sealed class RecordEditorController : IRecordEditorController {
    private readonly ILogger _logger;
    private readonly ILifetimeScope _lifetimeScope;
    private readonly IDockingManagerService _dockingManagerService;

    private readonly Dictionary<FormKey, Control> _openRecordEditors = new();

    private readonly Subject<IMajorRecordGetter> _recordChanged = new();
    public IObservable<IMajorRecordGetter> RecordChanged => _recordChanged;

    public RecordEditorController(
        ILogger logger,
        ILifetimeScope lifetimeScope,
        IDockingManagerService dockingManagerService) {
        _logger = logger;
        _lifetimeScope = lifetimeScope;
        _dockingManagerService = dockingManagerService;

        _dockingManagerService.Closed.Subscribe(OnClosed);
    }
    
    public bool AnyEditorsOpen() => _openRecordEditors.Count > 0;

    public void OpenEditor<TMajorRecord, TMajorRecordGetter>(TMajorRecord record)
        where TMajorRecord : class, IMajorRecord, TMajorRecordGetter
        where TMajorRecordGetter : class, IMajorRecordGetter {
        
        if (_openRecordEditors.TryGetValue(record.FormKey, out var editor)) {
            // Select editor as active
            _dockingManagerService.Focus(editor);
        } else {
            // Open new editor
            if (_lifetimeScope.TryResolve<IRecordEditorVM<TMajorRecord, TMajorRecordGetter>>(out var recordEditorVM)) {
                var editorControl = recordEditorVM.CreateControl(record);

                _dockingManagerService.AddControl(
                    editorControl, 
                    new DockConfig {
                        DockInfo = new DockInfo {
                            Header = record.EditorID ?? record.FormKey.ToString(),
                        },
                        Dock = Dock.Right,
                        DockMode = DockMode.Document,
                        Size = new GridLength(2, GridUnitType.Star),
                    });
                _openRecordEditors.Add(record.FormKey, editorControl);
            } else {
                _logger.Here().Warning("Cannot open record editor of type {Type} because no such editor is available", typeof(TMajorRecord));
            }
        }
    }
    
    public void CloseEditor(IMajorRecord record) {
        if (_openRecordEditors.TryGetValue(record.FormKey, out var editor)) {
            _dockingManagerService.RemoveControl(editor);

            _recordChanged.OnNext(record);
            
            RemoveEditorCache(editor);
        }
    }

    private void OnClosed(IDockedItem dockedItem) {
        RemoveEditorCache(dockedItem.Control);
        if (dockedItem.Control.DataContext is IRecordEditorVM recordEditorVM) {
            _recordChanged.OnNext(recordEditorVM.Record);
        }
    }
    
    private void RemoveEditorCache(Control editor) {
        var editorsToRemove = _openRecordEditors
            .Where(x => ReferenceEquals(x.Value, editor))
            .Select(x => x.Key)
            .ToList();

        foreach (var key in editorsToRemove) {
            _openRecordEditors.Remove(key);
        }
    }
}
