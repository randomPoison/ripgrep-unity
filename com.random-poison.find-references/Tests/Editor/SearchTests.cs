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

    [Test]
    public void FindByGuid()
    {
        var references = FindReferences.Editor.FindReferences.ForGuid("c2a5ad7e3804412489dd24aa7be0e6c5");
        Assert.AreEqual(new string[] { "Assets/Scenes/SampleScene.unity" }, references);
    }
}
