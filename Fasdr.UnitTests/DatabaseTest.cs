using System;
using NUnit.Framework;
using Fasdr.Backend;
using System.IO.Abstractions.TestingHelpers;
using System.Collections.Generic;

namespace Fasdr.UnitTests
{
    [TestFixture]
    public class DatabaseTest
    {
		static readonly string FileSystemConfigPath = System.IO.Path.Combine(Database.ConfigDir,$"{Database.ConfigFilePrefix}.FileSystem.txt");

        [Test]
        public void TestCanConstruct()
        {
            var fileSystem = new MockFileSystem();
            var db = new Database(fileSystem);
        }

        [Test]
        public void TestConfigLoadFileNotFound()
        {
            var fileSystem = new MockFileSystem();
            fileSystem.AddDirectory(Database.ConfigDir);

            var db = new Database(fileSystem);
            db.Load();
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void TestConfigLoadFileCorruptedFile()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {
                {  FileSystemConfigPath, new MockFileData("ThisShouldNotParse") }
            });

            var db = new Database(fileSystem);
            db.Load();
        }

        [Test]
        public void TestConfigNoProviders()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {});

            var db = new Database(fileSystem);
            db.Load();

            Assert.AreEqual(0, db.ProviderEntries.Count);
        }

        [Test]
        public void TestConfigLoadFileEmptyFile()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {
                {  FileSystemConfigPath, new MockFileData("") }
            });
 
            var db = new Database(fileSystem);
            db.Load();

            Assert.AreEqual(1,db.ProviderEntries.Count);
			Assert.AreEqual(0,db.ProviderEntries["FileSystem"].Count);
        }


        [Test]
        public void TestConfigLoadFile()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {
				{   FileSystemConfigPath, 
					new MockFileData(
                        string.Join("" + Database.Separator,@"c:\dir1\", "101.0", "FileSystem", "true") +
                        Environment.NewLine +
                        string.Join("" + Database.Separator,@"c:\dir1\file2", "10.0", "FileSystem", "false") +
                        Environment.NewLine) }
            });

            var db = new Database(fileSystem);
            db.Load();

			Assert.AreEqual(1, db.ProviderEntries.Count);
			var fsp = db.ProviderEntries ["FileSystem"];
			Assert.AreEqual (2, fsp.Count);
            Assert.IsTrue(fsp.ContainsKey(@"c:\dir1\"));
            Assert.AreEqual(101.0f, fsp[@"c:\dir1\"].Weight);
            Assert.AreEqual("FileSystem", fsp[@"c:\dir1\"].Provider);
            Assert.AreEqual(true, fsp[@"c:\dir1\"].IsLeaf);

            Assert.IsTrue(fsp.ContainsKey(@"c:\dir1\file2"));
            Assert.AreEqual(10.0f, fsp[@"c:\dir1\file2"].Weight);
            Assert.AreEqual("FileSystem", fsp[@"c:\dir1\file2"].Provider);
            Assert.AreEqual(false, fsp[@"c:\dir1\file2"].IsLeaf);
        }

        [Test]
        public void TestConfigSaveNoFiles()
        {
            var fileSystem = new MockFileSystem();
          
            var db = new Database(fileSystem);
            db.Save();

            Assert.IsTrue(!fileSystem.FileExists(FileSystemConfigPath));
        }

        [Test]
        public void TestConfigSimpleDatabase()
        {
            var fileSystem = new MockFileSystem();

            var db = new Database(fileSystem);
			var fsp = new Dictionary<string,Entry>();
			db.ProviderEntries.Add ("FileSystem", fsp);
            fsp.Add(@"c:\dir1\", new Entry(12.0,"FileSystem",false));
            fsp.Add(@"c:\dir1\file2", new Entry(34.0, "FileSystem", true));
            db.Save();

			var fsFileName = System.IO.Path.Combine (Database.ConfigDir,$"{Database.ConfigFilePrefix}.FileSystem.txt");

			Assert.IsTrue(fileSystem.FileExists(FileSystemConfigPath));
            Assert.AreEqual(
                    string.Join("" + Database.Separator, @"c:\dir1\", "12", "FileSystem", "False") +
                    Environment.NewLine +
                    string.Join("" + Database.Separator, @"c:\dir1\file2", "34", "FileSystem", "True") +
                    Environment.NewLine, 
                   fileSystem.File.ReadAllText(fsFileName));
        }

    }
}
