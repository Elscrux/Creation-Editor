using System.Numerics;
using System.Runtime.InteropServices;
namespace CreationEditor.Render;

public static class Interop {
    private const string DllName = "TGInterOp.dll";

    public struct ReferenceTransform {
        public Vector3 Translation;
        public Vector3 Scale;
        public Vector3 Rotations;
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
    public static extern bool initTGEditor(int count, string[] strings);

    public static void Start() {
        Task.Run(() => {
            initTGEditor(0, Array.Empty<string>());
        });
        
        addLoadCallback((callbackCount, callbackLoads) => {
            Console.WriteLine($"CALLBACK: {callbackCount}");
            if (callbackCount == 0) return;
            
            foreach (var load in callbackLoads) {
                Console.WriteLine($"CALLBACK: {load}");
            }
        });

        loadReferences(1, new ReferenceLoad[] {
            new() {
                FormKey = "123456:Skyrim.esm",
                Path = "test.nif",
                Transform = new ReferenceTransform {
                    Translation = new Vector3(1, 2, 3),
                    Scale = new Vector3(2, 3, 3),
                    Rotations = new Vector3()
                }
            }
        });
    }
}
