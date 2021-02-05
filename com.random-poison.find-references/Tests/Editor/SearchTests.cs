using System.Collections;
using NUnit.Framework;
using Ripgrep.Editor;
using UnityEngine.TestTools;

public class SearchTests
{
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        yield return Installer.Install();
    }

    [UnityTest]
    [Timeout(5000)]
    public IEnumerator FindByGuid()
    {
        var search = FindReferences.Editor.FindReferences.ForGuid("c2a5ad7e3804412489dd24aa7be0e6c5");
        search.Run();
        while (!search.IsDone)
        {
            yield return null;
        }
        Assert.AreEqual(new string[] { "Assets/SampleScene.unity" }, search.Result);
    }
}
