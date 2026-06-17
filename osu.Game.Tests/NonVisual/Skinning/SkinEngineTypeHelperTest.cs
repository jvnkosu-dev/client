using System.Linq;
using NUnit.Framework;
using osu.Game.Extensions;
using osu.Game.Online.API.Requests;
using osu.Game.Skinning;

namespace osu.Game.Tests.NonVisual.Skinning
{
    [TestFixture]
    public class SkinEngineTypeHelperTest
    {
        [Test]
        public void TestFromInstantiationInfoMapsKnownEngines()
        {
            Assert.That(SkinEngineTypeHelper.FromInstantiationInfo(typeof(LegacySkin).GetInvariantInstantiationInfo()), Is.EqualTo(SkinEngineType.Legacy));
            Assert.That(SkinEngineTypeHelper.FromInstantiationInfo(typeof(TrianglesSkin).GetInvariantInstantiationInfo()), Is.EqualTo(SkinEngineType.Triangles));
            Assert.That(SkinEngineTypeHelper.FromInstantiationInfo(typeof(ArgonSkin).GetInvariantInstantiationInfo()), Is.EqualTo(SkinEngineType.Argon));
            Assert.That(SkinEngineTypeHelper.FromInstantiationInfo(typeof(ArgonProSkin).GetInvariantInstantiationInfo()), Is.EqualTo(SkinEngineType.ArgonPro));
        }

        [Test]
        public void TestParseAcceptsDisplayAndEnumNames()
        {
            Assert.That(SkinEngineTypeHelper.TryParse("Triangles", out var triangles), Is.True);
            Assert.That(triangles, Is.EqualTo(SkinEngineType.Triangles));

            Assert.That(SkinEngineTypeHelper.TryParse("argonpro", out var argonPro), Is.True);
            Assert.That(argonPro, Is.EqualTo(SkinEngineType.ArgonPro));
        }

        [Test]
        public void TestGetEngineTypeUsesApiField()
        {
            var skin = new APIOnlineSkin { EngineType = "Argon" };
            Assert.That(SkinEngineTypeHelper.GetEngineType(skin), Is.EqualTo(SkinEngineType.Argon));
        }

        [Test]
        public void TestFilterUsesApiField()
        {
            var skins = new[]
            {
                new APIOnlineSkin { OnlineID = 1, EngineType = "Legacy" },
                new APIOnlineSkin { OnlineID = 2, EngineType = "Argon" },
            };

            var filtered = SkinEngineTypeHelper.Filter(skins, SkinEngineType.Argon).ToArray();
            Assert.That(filtered, Has.Length.EqualTo(1));
            Assert.That(filtered[0].OnlineID, Is.EqualTo(2));
        }

        [Test]
        public void TestToStorageStringMatchesDisplayName()
        {
            Assert.That(SkinEngineTypeHelper.ToStorageString(SkinEngineType.ArgonPro), Is.EqualTo("ArgonPro"));
        }
    }
}
