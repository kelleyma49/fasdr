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
                {  Database.ConfigPath, new MockFileData("ThisShouldNotParse") }
            });

            var db = new Database(fileSystem);
            db.Load();
        }

        [Test]
        public void TestConfigLoadFileEmptyFile()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {
                {  Database.ConfigPath, new MockFileData("") }
            });
 
            var db = new Database(fileSystem);
            db.Load();

            Assert.AreEqual(0,db.Entries.Count);
        }


        [Test]
        public void TestConfigLoadFile()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {
                {  Database.ConfigPath,
                    new MockFileData(
                        string.Join("" + Database.Separator,@"c:\dir1\", "101.0", "FileSystem", "true") +
                        Environment.NewLine +
                        string.Join("" + Database.Separator,@"c:\dir1\file2", "10.0", "FileSystem", "false") +
                        Environment.NewLine) }
            });

            var db = new Database(fileSystem);
            db.Load();

            Assert.AreEqual(2, db.Entries.Count);
            Assert.IsTrue(db.Entries.ContainsKey(@"c:\dir1\"));
            Assert.AreEqual(101.0f, db.Entries[@"c:\dir1\"].Weight);
            Assert.AreEqual("FileSystem", db.Entries[@"c:\dir1\"].Provider);
            Assert.AreEqual(true, db.Entries[@"c:\dir1\"].IsLeaf);

            Assert.IsTrue(db.Entries.ContainsKey(@"c:\dir1\file2"));
            Assert.AreEqual(10.0f, db.Entries[@"c:\dir1\file2"].Weight);
            Assert.AreEqual("FileSystem", db.Entries[@"c:\dir1\file2"].Provider);
            Assert.AreEqual(false, db.Entries[@"c:\dir1\file2"].IsLeaf);
        }

        [Test]
        public void TestConfigSaveFileEmpty()
        {
            var fileSystem = new MockFileSystem();
          
            var db = new Database(fileSystem);
            db.Save();

            Assert.IsTrue(fileSystem.FileExists(Database.ConfigPath));
            Assert.AreEqual("",fileSystem.File.ReadAllText(Database.ConfigPath));
        }

        [Test]
        public void TestConfigSimpleDatabase()
        {
            var fileSystem = new MockFileSystem();

            var db = new Database(fileSystem);
            db.Entries.Add(@"c:\dir1\", new Entry(12.0,"FileSystem",false));
            db.Entries.Add(@"c:\dir1\file2", new Entry(34.0, "FileSystem", true));
            db.Save();

            Assert.IsTrue(fileSystem.FileExists(Database.ConfigPath));
            Assert.AreEqual(
                    string.Join("" + Database.Separator, @"c:\dir1\", "12", "FileSystem", "False") +
                    Environment.NewLine +
                    string.Join("" + Database.Separator, @"c:\dir1\file2", "34", "FileSystem", "True") +
                    Environment.NewLine, 
                   fileSystem.File.ReadAllText(Database.ConfigPath));
        }

    }
}
