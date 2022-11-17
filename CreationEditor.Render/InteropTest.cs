using System.Numerics;
using Xunit;
namespace CreationEditor.Render; 

//Must be run in debug mode to include dll
public class InteropTest {
    private readonly Interop.ReferenceLoad _testReference = new() {
        FormKey = "123456:Skyrim.esm",
        Path = "test.nif",
        Transform = new Interop.ReferenceTransform {
            Translation = new Vector3(1, 2, 3),
            Scale = new Vector3(2, 3, 3),
            Rotations = new Vector3()
        }
    };

    [Fact]
    public async void TestInit() {
        var task = Record.ExceptionAsync(() =>  Task.Run(() => {
            Interop.initTGEditor(new Interop.InitConfig {
                Version = 1,
                AssetDirectory = "test"
            });
        }));

        Interop.waitFinishedInit();

        Assert.Null(task.Exception);
    }

    [Fact]
    public void TestLoadRefs() {
        var referenceLoads = new[] { _testReference };
        var count = Convert.ToUInt32(referenceLoads.Length);

        Assert.True(Interop.addLoadCallback((callbackCount, callbackLoads) => {
            Assert.Equal(count, callbackCount);
            for (var i = 0; i < callbackLoads.Length; i++) {
                Assert.Equal(callbackLoads[i].FormKey, referenceLoads[i].FormKey);
            }
        }));
        
        Interop.loadReferences(count, referenceLoads);
    }
}
