using Elscrux.Logging;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Records;
using Serilog;
namespace CreationEditor.Environment;

public interface IRecordController {
    public TMajorRecord CreateRecord<TMajorRecord, TMajorRecordGetter>()
        where TMajorRecord : class, IMajorRecord, TMajorRecordGetter
        where TMajorRecordGetter : class, IMajorRecordGetter;
    
    public TMajorRecord DuplicateRecord<TMajorRecord, TMajorRecordGetter>(TMajorRecordGetter record)
        where TMajorRecord : class, IMajorRecord, TMajorRecordGetter
        where TMajorRecordGetter : class, IMajorRecordGetter;
    
    public void DeleteRecord<TMajorRecord, TMajorRecordGetter>(TMajorRecordGetter record)
        where TMajorRecord : class, IMajorRecord, TMajorRecordGetter, IMajorRecordQueryable
        where TMajorRecordGetter : class, IMajorRecordGetter;

    public TMajorRecord GetOrAddOverride<TMajorRecord, TMajorRecordGetter>(TMajorRecordGetter record)
        where TMajorRecord : class, IMajorRecord, TMajorRecordGetter, IMajorRecordQueryable
        where TMajorRecordGetter : class, IMajorRecordGetter;
}

public sealed class RecordController<TMod, TModGetter> : IRecordController
    where TMod : class, IContextMod<TMod, TModGetter>, TModGetter
    where TModGetter : class, IContextGetterMod<TMod, TModGetter> {
    private readonly ILogger _logger;
    private readonly IEditorEnvironment<TMod, TModGetter> _editorEnvironment;
    
    public RecordController(
        ILogger logger,
        IEditorEnvironment<TMod, TModGetter> editorEnvironment) {
        _logger = logger;
        _editorEnvironment = editorEnvironment;
    }
    
    public TMajorRecord CreateRecord<TMajorRecord, TMajorRecordGetter>() 
        where TMajorRecord : class, IMajorRecord, TMajorRecordGetter
        where TMajorRecordGetter : class, IMajorRecordGetter {
        var group = _editorEnvironment.ActiveMod.GetTopLevelGroup<TMajorRecord>();
        var record = group.AddNew(_editorEnvironment.ActiveMod.GetNextFormKey());
        
        _logger.Here().Verbose("Creating new record {Record} of type {Type} in {Mod}",
            record, typeof(TMajorRecord), _editorEnvironment.ActiveMod.ModKey);

        return (record as TMajorRecord)!;
    }
    
    public TMajorRecord DuplicateRecord<TMajorRecord, TMajorRecordGetter>(TMajorRecordGetter record)
        where TMajorRecord : class, IMajorRecord, TMajorRecordGetter
        where TMajorRecordGetter : class, IMajorRecordGetter {
        var resolveContext = _editorEnvironment.LinkCache.ResolveContext<TMajorRecord, TMajorRecordGetter>(record.FormKey);
        var duplicate = resolveContext.DuplicateIntoAsNewRecord(_editorEnvironment.ActiveMod);
        
        _logger.Here().Verbose("Creating new record {Duplicate} by duplicating {Record} in {Mod}",
            duplicate, record, _editorEnvironment.ActiveMod.ModKey);

        return duplicate;
    }
    
    public void DeleteRecord<TMajorRecord, TMajorRecordGetter>(TMajorRecordGetter record)
        where TMajorRecord : class, IMajorRecord, TMajorRecordGetter, IMajorRecordQueryable
        where TMajorRecordGetter : class, IMajorRecordGetter {
        var newOverride = GetOrAddOverride<TMajorRecord, TMajorRecordGetter>(record);
        newOverride.IsDeleted = true;
        
        _logger.Here().Verbose("Deleting record {Record} from {Mod}",
            record, _editorEnvironment.ActiveMod);
    }
    
    public TMajorRecord GetOrAddOverride<TMajorRecord, TMajorRecordGetter>(TMajorRecordGetter record)
        where TMajorRecord : class, IMajorRecord, TMajorRecordGetter, IMajorRecordQueryable
        where TMajorRecordGetter : class, IMajorRecordGetter {
        if (!_editorEnvironment.LinkCache.TryResolveContext<TMajorRecord, TMajorRecordGetter>(record.FormKey, out var context)) {
            context = _editorEnvironment.ActiveModLinkCache.ResolveContext<TMajorRecord, TMajorRecordGetter>(record.FormKey);
        }

        var newOverride = context.GetOrAddAsOverride(_editorEnvironment.ActiveMod);
        if (context.ModKey != _editorEnvironment.ActiveMod.ModKey) {
            _logger.Here().Verbose("Creating overwrite of record {Record} in {Mod}",
                record, _editorEnvironment.ActiveMod.ModKey);
        }

        return newOverride;
    }
}
