﻿using System;
using System.Linq;
using CreationEditor.Services.Environment;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Noggog.WPF;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
namespace CreationEditor.GUI.Models.Record.RecordBrowser;

public interface IRecordBrowserSettings {
    [Reactive] public bool OnlyActive { get; set; }
    [Reactive] public ILinkCache LinkCache { get; set; }
    [Reactive] public BrowserScope Scope { get; set; }
    [Reactive] public string SearchTerm { get; set; }

    public bool Filter(IMajorRecordIdentifier record);
}

public class RecordBrowserSettings : ViewModel, IRecordBrowserSettings {
    private const char SplitChar = '*';
    
    private readonly IEditorEnvironment _editorEnvironment;

    [Reactive] public bool OnlyActive { get; set; } = false;
    [Reactive] public ILinkCache LinkCache { get; set; } = null!;
    [Reactive] public BrowserScope Scope { get; set; } = BrowserScope.Environment;
    [Reactive] public string SearchTerm { get; set; } = string.Empty;

    public RecordBrowserSettings(
        IEditorEnvironment editorEnvironment) {
        _editorEnvironment = editorEnvironment;
        
        this.WhenAnyValue(x => x.OnlyActive)
            .Subscribe(_ => Scope = OnlyActive ? BrowserScope.ActiveMod : BrowserScope.Environment);

        this.WhenAnyValue(x => x.Scope)
            .Subscribe(_ => UpdateScope());

        editorEnvironment.EditorInitialized += (_, _) =>  UpdateScope();
    }
    
    private void UpdateScope() {
        LinkCache = Scope switch {
            BrowserScope.Environment => _editorEnvironment.LinkCache,
            BrowserScope.ActiveMod => _editorEnvironment.ActiveMod.ToImmutableLinkCache(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public bool Filter(IMajorRecordIdentifier record) {
        return SearchTerm.IsNullOrWhitespace() || record.EditorID != null && SearchTerm.Split(SplitChar).All(term => record.EditorID.Contains(term, StringComparison.OrdinalIgnoreCase));
    }
}
