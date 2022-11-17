using System.Runtime.InteropServices;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using MutagenLibrary.Core.Plugins;
using Noggog;
namespace CreationEditor.Render;

public static class Interop {
    private const string DllName = "TGInterOp.dll";

    public struct ReferenceTransform {
        public P3Float Translation;
        public P3Float Scale;
        public P3Float Rotations;
    }

    public struct ReferenceLoad {
        public string FormKey;
        public string Path;
        public ReferenceTransform Transform;
    }

    public enum UpdateType {
        Transform,
        Path,
    }

    public struct ReferenceUpdate {
        public string FormKey;
        public UpdateType Update;
        public string Path;
    }
    
    public struct InitConfig {
        public uint Version;
        public string AssetDirectory;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LoadCallback(uint count, ReferenceLoad[] load);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void UpdateCallback (uint count, ReferenceUpdate[] load);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void HideCallback (uint count, string[] keys, bool hide);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DeleteCallback (uint count, string[] keys, bool hide);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern bool addLoadCallback(LoadCallback callback);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern bool addUpdateCallback(UpdateCallback callback);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern bool addHideCallback(HideCallback callback);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern bool addDeleteCallback(DeleteCallback  callback);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void loadReferences(uint count, ReferenceLoad[] load);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern bool updateReferences(uint count, ReferenceUpdate[] keys);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern bool hideReferences(uint count, string[] formKeys, bool hide);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern bool deleteReferences(uint count, string[] formKeys);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern bool initTGEditor(InitConfig count);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern bool isFinished();
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void waitFinishedInit();

    public static async void Start(ISimpleEnvironmentContext simpleEnvironmentContext) {
        Task.Run(() => {
            initTGEditor(new InitConfig {
                Version = 1,
                AssetDirectory = @"E:\TES\Skyrim\vanilla-files\"//simpleEnvironmentContext.DataDirectoryProvider.Path
            });
        });
        
        addLoadCallback((callbackCount, callbackLoads) => {
            Console.WriteLine($"CALLBACK: {callbackCount}");
            if (callbackCount == 0) return;
            
            foreach (var load in callbackLoads) {
                Console.WriteLine($"CALLBACK: {load}");
            }
        });

        waitFinishedInit();

        var gameEnvironment = GameEnvironment.Typical.Skyrim(SkyrimRelease.SkyrimSE);
        
        var linkCache = gameEnvironment.LoadOrder[0].Mod.ToImmutableLinkCache();
        var whiterunCell = linkCache.Resolve<ICellGetter>(FormKey.Factory("01A27a:Skyrim.esm"));
        
        var gridPoint = whiterunCell.Grid.Point;
        
        var origin = new P3Float(gridPoint.X * 4096, gridPoint.Y * 4096, 0);
        var refs = new List<ReferenceLoad>();
        foreach (var placed in whiterunCell.Temporary.Concat(whiterunCell.Persistent)) {
            if (placed is IPlacedObjectGetter placedObject) {
                var placement = placedObject.Placement;
                if (placement == null) return;
        
                var relativePosition = placement.Position - origin;
        
                var baseObject = placedObject.Base.Resolve(gameEnvironment.LinkCache);
        
                var model = baseObject switch {
                    IActivatorGetter activator => activator.Model?.File.DataRelativePath,
                    IAlchemicalApparatusGetter alchemicalApparatus => alchemicalApparatus.Model?.File.DataRelativePath,
                    IAmmunitionGetter ammunition => ammunition.Model?.File.DataRelativePath,
                    IArmorGetter armor => armor.WorldModel.Male.Model?.File.DataRelativePath,
                    IBookGetter book => book.Model?.File.DataRelativePath,
                    IContainerGetter container => container.Model?.File.DataRelativePath,
                    IDoorGetter door => door.Model?.File.DataRelativePath,
                    IFloraGetter flora => flora.Model?.File.DataRelativePath,
                    IFurnitureGetter furniture => furniture.Model?.File.DataRelativePath,
                    IIdleMarkerGetter idleMarker => idleMarker.Model?.File.DataRelativePath,
                    IIngestibleGetter ingestible => ingestible.Model?.File.DataRelativePath,
                    IIngredientGetter ingredient => ingredient.Model?.File.DataRelativePath,
                    IKeyInternal keyInternal => keyInternal.Model?.File.DataRelativePath,
                    IKeyGetter key => key.Model?.File.DataRelativePath,
                    ILightGetter light => light.Model?.File.DataRelativePath,
                    IMiscItemGetter miscItem => miscItem.Model?.File.DataRelativePath,
                    IMoveableStaticGetter moveableStatic => moveableStatic.Model?.File.DataRelativePath,
                    IScrollGetter scroll => scroll.Model?.File.DataRelativePath,
                    ISoulGemGetter soulGem => soulGem.Model?.File.DataRelativePath,
                    ISoundMarkerGetter soundMarker => null,
                    ISpellGetter spell => null,
                    IStaticGetter @static => @static.Model?.File.DataRelativePath,
                    ITalkingActivatorGetter talkingActivator => talkingActivator.Model?.File.DataRelativePath,
                    ITextureSetGetter textureSet => null,
                    ITreeGetter tree => tree.Model?.File.DataRelativePath,
                    IWeaponGetter weapon => weapon.Model?.File.DataRelativePath,
                    _ => throw new ArgumentOutOfRangeException(nameof(baseObject))
                };
        
                if (model == null) continue;
        
        
                var scale = placedObject.Scale ?? 1;
                
                refs.Add(new ReferenceLoad {
                    FormKey = placedObject.FormKey.ToString(),
                    Path = model.ToLower(),
                    Transform = new ReferenceTransform {
                        Translation = relativePosition,
                        Scale = new P3Float(scale, scale, scale),
                        Rotations = placement.Rotation
                    }
                });
            }
        }
        
        foreach (var referenceLoad in refs) {
            Console.WriteLine(referenceLoad.ToString());
        }
        
        await Task.Delay(5000);
        
        loadReferences(Convert.ToUInt32(refs.Count), refs.ToArray());
    }
}
