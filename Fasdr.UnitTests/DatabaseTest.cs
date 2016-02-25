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
        public void TestConfigLoadFileEmptyFile()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {
                {  FileSystemConfigPath, new MockFileData("") }
            });
 
            var db = new Database(fileSystem);
            db.Load();

            Assert.AreEqual(1,db.Providers.Count);
			Assert.AreEqual(0,db.Providers["FileSystem"].Entries.Count);
        }


        [Test]
        public void TestConfigLoadFile()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {
				{   FileSystemConfigPath, 
					new MockFileData(
                        string.Join("" + Provider.Separator,@"c:\dir1\", "101.0", "true") +
                        Environment.NewLine +
                        string.Join("" + Provider.Separator,@"c:\dir1\file2", "10.0", "false") +
                        Environment.NewLine) }
            });

            var db = new Database(fileSystem);
            db.Load();

			Assert.AreEqual(1, db.Providers.Count);
			var fsp = db.Providers["FileSystem"];
			Assert.AreEqual (2, fsp.Entries.Count);
        }

        [Test]
        public void TestConfigSaveNoFiles()
        {
            var fileSystem = new MockFileSystem();
          
            var db = new Database(fileSystem);
            db.Save();

            Assert.IsTrue(!fileSystem.FileExists(Database.ConfigPath));
        }

        [Test]
        public void TestConfigSimpleDatabase()
        {
			var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {});
			fileSystem.AddDirectory (Database.ConfigDir);

            var db = new Database(fileSystem);
			var fsp = new Provider("FileSystem");
			db.Providers.Add ("FileSystem", fsp);
            fsp.Add(@"c:\dir1\", false, 12.0);
            fsp.Add(@"c:\dir1\file2", true, 34.0);
            db.Save();

			var fsFileName = System.IO.Path.Combine (Database.ConfigDir,$"{Database.ConfigFilePrefix}.FileSystem.txt");

			Assert.IsTrue(fileSystem.FileExists(FileSystemConfigPath));
            Assert.AreEqual(
				string.Join("" + Provider.Separator, @"c:\dir1\", "12", "False") +
                    Environment.NewLine +
                    string.Join("" + Provider.Separator, @"c:\dir1\file2", "34", "True") +
                    Environment.NewLine, 
                   fileSystem.File.ReadAllText(fsFileName));
        }

    }
}
