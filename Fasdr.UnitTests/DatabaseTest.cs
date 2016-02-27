﻿using System;
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
        public void TestConfigLoadFile()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {
				{   FileSystemConfigPath, 
					new MockFileData(
						(new Entry(@"c:\dir1\",1,DateTime.Now,false)).ToString() + 
                        Environment.NewLine +
						(new Entry(@"c:\dir1\file2",10,DateTime.Now,true)).ToString() +
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

            Assert.IsTrue(!fileSystem.FileExists(FileSystemConfigPath));
        }

        [Test]
        public void TestConfigSimpleDatabase()
        {
			var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {});
			fileSystem.AddDirectory (Database.ConfigDir);

            var db = new Database(fileSystem);
			var fsp = new Provider("FileSystem");
			db.Providers.Add ("FileSystem", fsp);
			var e1 = new Entry (@"c:\dir1\", 12, DateTime.Now, false);
			var e2 = new Entry (@"c:\dir1\file2", 34, DateTime.Now, true);

			fsp.Add(e1);
			fsp.Add(e2);
            db.Save();

			var fsFileName = System.IO.Path.Combine (Database.ConfigDir,$"{Database.ConfigFilePrefix}.FileSystem.txt");

			Assert.IsTrue(fileSystem.FileExists(FileSystemConfigPath));
            Assert.AreEqual(
				string.Join(Environment.NewLine,e1.ToString(),e2.ToString()),
			    fileSystem.File.ReadAllText(fsFileName));
        }

    }
}
