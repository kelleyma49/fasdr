using System;
using NUnit.Framework;
using Fasdr.Backend;
using System.IO.Abstractions.TestingHelpers;
using System.Collections.Generic;
using System.IO;

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

            Assert.AreEqual(0,db.PathToWeight.Count);
        }


        [Test]
        public void TestConfigLoadFile()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {
                {  Database.ConfigPath,
                    new MockFileData(
                        @"c:\dir1\" +  Database.Separator + "101.0" + Environment.NewLine +
                        @"c:\dir1\dir2" +  Database.Separator + "10.0" + Environment.NewLine) }
            });

            var db = new Database(fileSystem);
            db.Load();

            Assert.AreEqual(2, db.PathToWeight.Count);
            Assert.IsTrue(db.PathToWeight.ContainsKey(@"c:\dir1\"));
            Assert.AreEqual(101.0f, db.PathToWeight[@"c:\dir1\"]);
            Assert.IsTrue(db.PathToWeight.ContainsKey(@"c:\dir1\dir2"));
            Assert.AreEqual(10.0f, db.PathToWeight[@"c:\dir1\dir2"]);
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
            db.PathToWeight.Add(@"c:\dir1\", 12.0);
            db.PathToWeight.Add(@"c:\dir1\dir2", 34.0);
            db.Save();

            Assert.IsTrue(fileSystem.FileExists(Database.ConfigPath));
            Assert.AreEqual(@"c:\dir1\" + Database.Separator + "12" +Environment.NewLine +
                   @"c:\dir1\dir2" + Database.Separator + "34" + Environment.NewLine, 
                   fileSystem.File.ReadAllText(Database.ConfigPath));
        }

    }
}
