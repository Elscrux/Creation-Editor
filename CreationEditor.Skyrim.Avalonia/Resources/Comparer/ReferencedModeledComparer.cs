﻿using System;
using CreationEditor.Avalonia.Comparer;
using CreationEditor.Avalonia.Models.Record;
using Mutagen.Bethesda.Skyrim;
namespace CreationEditor.Skyrim.Avalonia.Resources.Comparer;

public class ReferencedModeledComparer : ReferencedComparer {
    public static readonly ReferencedModeledComparer Instance = new();
    
    public override int Compare(IReferencedRecord? x, IReferencedRecord? y) {
        if (x?.Record is IModeledGetter m1 && y?.Record is IModeledGetter m2) {
            return ModeledComparer.Instance.Compare(m1, m2);
        }

        throw new ArgumentException($"Can't compare {x} and {y}, one of them is not IModeledGetter");
    }
}