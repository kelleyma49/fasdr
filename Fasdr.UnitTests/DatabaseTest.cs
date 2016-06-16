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
		static readonly string FileSystemConfigPath = System.IO.Path.Combine(Database.DefaultConfigDir,$"{Database.ConfigFilePrefix}.FileSystem.txt");

        [Test]
        public void TestCanConstruct()
        {
            var fileSystem = new MockFileSystem();
            var db = new Database(fileSystem);
			Assert.IsNotNull (db);
        }

        [Test]
        public void TestConfigLoadFileNotFound()
        {
            var fileSystem = new MockFileSystem();
            fileSystem.AddDirectory(Database.DefaultConfigDir);

            var db = new Database(fileSystem);
            db.Load();
        }

        [Test]
        public void TestConfigLoadFileCorruptedFile()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {
                {  FileSystemConfigPath, new MockFileData("ThisShouldNotParse") }
            });

            var db = new Database(fileSystem);
			Assert.Throws<Exception>(db.Load);
        }

        [Test]
        public void TestConfigNoProviders()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {});

            var db = new Database(fileSystem);
            db.Load();

            Assert.AreEqual(0, db.Providers.Count);
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
        public void TestAddEntryNoItemsOrProvider()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {
                {  FileSystemConfigPath, new MockFileData("") }
            });
            var db = new Database(fileSystem);
            Assert.IsTrue(db.AddEntry("FileSystem", @"c:\APath\", p => false));
        }

        [Test]
        public void TestRemoveEntryNoItemsOrProvider()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {
                {  FileSystemConfigPath, new MockFileData("") }
            });
            var db = new Database(fileSystem);
            Assert.IsFalse(db.RemoveEntry("FileSystem", @"c:\APath\"));
        }

        [Test]
        public void TestCanRemoveEntries()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {
                {   FileSystemConfigPath,
                    new MockFileData(
                        (new Entry(@"c:\dir1\",1,DateTime.Now,false)) +
                        Environment.NewLine +
                        (new Entry(@"c:\dir1\file2",10,DateTime.Now,true)) +
                        Environment.NewLine) }
            });

            var db = new Database(fileSystem);
            db.Load();

            Assert.IsTrue(db.RemoveEntry("FileSystem", @"c:\dir1\"));
            Assert.IsFalse(db.RemoveEntry("FileSystem", @"c:\dir1\"));
            Assert.IsTrue(db.RemoveEntry("FileSystem", @"c:\dir1\file2"));
        }

        [Test]
        public void TestConfigLoadFile()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {
				{   FileSystemConfigPath, 
					new MockFileData(
						(new Entry(@"c:\dir1\",1,DateTime.Now,false)) + 
                        Environment.NewLine +
						(new Entry(@"c:\dir1\file2",10,DateTime.Now,true)) +
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
            db.Save(100);

            Assert.IsTrue(!fileSystem.FileExists(FileSystemConfigPath));
        }

        [Test]
        public void TestSaveMaxEntries()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { });
            fileSystem.AddDirectory(Database.DefaultConfigDir);
            var db = new Database(fileSystem);
            var fsp = new Provider("FileSystem");
            db.Providers.Add("FileSystem", fsp);
            fsp.Add(new Entry(@"c:\dir1\", 12, DateTime.Now, false));
            fsp.Add(new Entry(@"c:\dir1\file2", 34, DateTime.Now, true));    
            db.Save(1);
            db.Load();

            Assert.AreEqual(1,db.Providers["FileSystem"].Entries.Count);
        }

        [Test]
        public void TestConfigSimpleDatabase()
        {
			var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {});
			fileSystem.AddDirectory (Database.DefaultConfigDir);

            var db = new Database(fileSystem);
			var fsp = new Provider("FileSystem");
			db.Providers.Add ("FileSystem", fsp);
			var e1 = new Entry (@"c:\dir1\", 12, DateTime.Now, false);
			var e2 = new Entry (@"c:\dir1\file2", 34, DateTime.Now, true);

			fsp.Add(e1);
			fsp.Add(e2);
            db.Save(100);

			var fsFileName = System.IO.Path.Combine (Database.DefaultConfigDir,$"{Database.ConfigFilePrefix}.FileSystem.txt");

			Assert.IsTrue(fileSystem.FileExists(FileSystemConfigPath));
            Assert.AreEqual(
				string.Join(Environment.NewLine,e1.ToString(),e2.ToString(),""),
			    fileSystem.File.ReadAllText(fsFileName));
        }

		[Test]
		public void TestConfigCanMerge()
		{
			var now = DateTime.Now;
			var mockE1 = new Entry (@"c:\dir2\", 1, now, false);
			var mockE2 = new Entry(@"c:\dir2\file3",10,now,true);
			var mockE3 = new Entry(@"c:\dir3\file1",101,now,true);
			var mockContent = string.Join (Environment.NewLine, 
				mockE1.ToString (), 
				mockE2.ToString (), 
				mockE3.ToString ());
			
			var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {
				{   FileSystemConfigPath, new MockFileData(mockContent) }
			});

			var db = new Database(fileSystem);
			var fsp = new Provider("FileSystem");
			db.Providers.Add ("FileSystem", fsp);
			now = now.AddSeconds (1);
			var e1 = new Entry (@"c:\dir1\", 12, now, false);
			var e2 = new Entry (@"c:\dir1\file2", 34, now, true);
			var e3 = new Entry(@"c:\dir2\file3",11,now,true);
			var e4 = new Entry(@"c:\dir3\file1",11,now,true);

			fsp.Add(e1);
			fsp.Add(e2);
			fsp.Add(e3);
			fsp.Add(e4);
			Assert.IsTrue(fsp.Remove(e4.FullPath));
			db.Save(100);

			var fsFileName = System.IO.Path.Combine (db.ConfigDir,$"{Database.ConfigFilePrefix}.FileSystem.txt");

			Assert.IsTrue(fileSystem.FileExists(FileSystemConfigPath));
			var configContent = fileSystem.File.ReadAllText (fsFileName);
			StringAssert.Contains (e1 + Environment.NewLine, configContent);
			StringAssert.Contains (e2 + Environment.NewLine, configContent);
			StringAssert.Contains (e3 + Environment.NewLine, configContent);
			StringAssert.Contains (mockE1 + Environment.NewLine, configContent);
			StringAssert.DoesNotContain (mockE2 + Environment.NewLine, configContent);
			StringAssert.DoesNotContain (mockE3 + Environment.NewLine, configContent);
            StringAssert.DoesNotContain(e4 + Environment.NewLine, configContent);
        }

        [Test]
        public void TestGetEntries()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { });
            fileSystem.AddDirectory(Database.DefaultConfigDir);
            var db = new Database(fileSystem);
            var fsp = new Provider("FileSystem");
            db.Providers.Add("FileSystem", fsp);
            var e1 = new Entry(@"c:\dir1\", 12, DateTime.Now, false);
            var e2 = new Entry(@"c:\dir1\file2", 34, DateTime.Now, true);
            fsp.Add(e1);
            fsp.Add(e2);

            Entry[] entries = null;
            Assert.IsTrue(db.GetEntries("FileSystem", out entries));
            Assert.AreEqual(e2, entries[0]);
            Assert.AreEqual(e1, entries[1]);
        }
    }
}
