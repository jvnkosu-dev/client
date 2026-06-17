using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using osu.Game.Skinning;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Writers;
using SharpCompress.Writers.Zip;

namespace osu.Game.Tests.NonVisual.Skinning
{
    [TestFixture]
    public class SkinIniVersionHelperTest
    {
        [Test]
        public void TestGetSkinVersionReturnsEmptyWhenMissing()
        {
            Assert.That(SkinIniVersionHelper.GetSkinVersion(new SkinConfiguration()), Is.Empty);
            Assert.That(SkinIniVersionHelper.GetSkinVersion(new SkinConfiguration(), useDefaultIfMissing: true), Is.EqualTo(SkinIniVersionHelper.DEFAULT_VERSION));
        }

        [Test]
        public void TestSanitizeUploadNameRemovesBracketedSegments()
        {
            Assert.That(SkinIniVersionHelper.SanitizeUploadName("sas [6.7]"), Is.EqualTo("sas"));
            Assert.That(SkinIniVersionHelper.SanitizeUploadName("Cool [remix] Skin [1.0]"), Is.EqualTo("Cool Skin"));
            Assert.That(SkinIniVersionHelper.SanitizeUploadName("Plain Name"), Is.EqualTo("Plain Name"));
        }

        [Test]
        public void TestGetDisplayNameMatchesSanitizedUploadName()
        {
            Assert.That(SkinIniVersionHelper.GetDisplayName("My Skin [2.0]"), Is.EqualTo("My Skin"));
        }

        [Test]
        public void TestUpdateSkinIniMetadataAppendsAuthoritativeBlock()
        {
            const string original = """
                [General]
                Author: Unknown
                Version: 2.7
                """;

            string updated = SkinIniVersionHelper.UpdateSkinIniMetadata(original, "My Skin", "Test Author", "2.0-beta", "Argon", "osu,taiko");

            Assert.That(updated, Does.Contain("Author: Test Author"));
            Assert.That(updated, Does.Contain("SkinVersion: 2.0-beta"));
            Assert.That(updated, Does.Contain("SkinType: Argon"));
            Assert.That(updated, Does.Contain("ModifiedModes: osu,taiko"));
            Assert.That(updated, Does.Contain("Name: My Skin"));
            Assert.That(updated, Does.Contain("automatically added during skin upload"));
        }

        [Test]
        public void TestEnsureSkinMetadataInOskUpdatesSharpCompressArchive()
        {
            string path = Path.Combine(Path.GetTempPath(), $"skin-patch-test-{Guid.NewGuid()}.osk");

            try
            {
                createSharpCompressOsk(path, """
                    // The following content was automatically added by osu!
                    [General]
                    Name: old name
                    Author: Unknown
                    Version: latest
                    """);

                SkinIniVersionHelper.EnsureSkinMetadataInOsk(path, "Uploaded Name", "Uploaded Author", "1.5-rc", "Legacy", "mania");

                string iniContent = readSkinIniFromOsk(path);

                Assert.That(iniContent, Does.Contain("Author: Uploaded Author"));
                Assert.That(iniContent, Does.Contain("SkinVersion: 1.5-rc"));
                Assert.That(iniContent, Does.Contain("SkinType: Legacy"));
                Assert.That(iniContent, Does.Contain("ModifiedModes: mania"));
                Assert.That(iniContent, Does.Contain("Name: Uploaded Name"));
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        [Test]
        public void TestTryGetOnlineSkinIdReturnsFalseWhenMissing()
        {
            Assert.That(SkinIniVersionHelper.TryGetOnlineSkinId(new SkinConfiguration(), out _), Is.False);
        }

        [Test]
        public void TestUpdateSkinIniMetadataWritesOnlineSkinId()
        {
            const string original = """
                [General]
                Name: Test Skin
                """;

            string updated = SkinIniVersionHelper.UpdateSkinIniMetadata(original, name: null, author: null, version: null, onlineSkinId: 18);

            Assert.That(updated, Does.Contain("OnlineSkinID: 18"));
        }

        [Test]
        public void TestEnsureOnlineSkinIdInOskUpdatesSharpCompressArchive()
        {
            string path = Path.Combine(Path.GetTempPath(), $"skin-online-id-test-{Guid.NewGuid()}.osk");

            try
            {
                createSharpCompressOsk(path, """
                    [General]
                    Name: Test Skin
                    """);

                SkinIniVersionHelper.EnsureOnlineSkinIdInOsk(path, 42);

                string iniContent = readSkinIniFromOsk(path);
                Assert.That(iniContent, Does.Contain("OnlineSkinID: 42"));
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        [Test]
        public void TestParseModifiedModesSplitsCommaSeparatedValues()
        {
            Assert.That(SkinIniVersionHelper.ParseModifiedModes("osu, taiko,mania"), Is.EqualTo(new[] { "osu", "taiko", "mania" }));
        }

        private static void createSharpCompressOsk(string path, string iniContent)
        {
            var encoding = new ArchiveEncoding { Default = Encoding.UTF8, Password = Encoding.UTF8 };

            using (var output = File.Create(path))
            using (var writer = new ZipWriter(output, new ZipWriterOptions(CompressionType.Deflate) { ArchiveEncoding = encoding }))
            using (var iniStream = new MemoryStream(Encoding.UTF8.GetBytes(iniContent)))
                writer.Write("skin.ini", iniStream);
        }

        private static string readSkinIniFromOsk(string path)
        {
            using var archive = ZipArchive.OpenArchive(path);

            foreach (var entry in archive.Entries)
            {
                if (!entry.Key.Equals("skin.ini"))
                    continue;

                using var stream = entry.OpenEntryStream();
                using var reader = new StreamReader(stream, Encoding.UTF8, true);
                return reader.ReadToEnd();
            }

            throw new AssertionException("skin.ini entry was not found in archive");
        }
    }
}
