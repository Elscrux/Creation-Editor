using Mutagen.Bethesda.Plugins.Records;
namespace CreationEditor.Avalonia.Services.Record.Editor;

public interface IRecordEditorController {
    public void OpenEditor<TMajorRecord, TMajorRecordGetter>(TMajorRecord record)
        where TMajorRecord : class, IMajorRecord, TMajorRecordGetter
        where TMajorRecordGetter : class, IMajorRecordGetter;

    public void CloseEditor(IMajorRecord record);

    public bool AnyEditorsOpen();

    public IObservable<IMajorRecordGetter> RecordChanged { get; }
}
