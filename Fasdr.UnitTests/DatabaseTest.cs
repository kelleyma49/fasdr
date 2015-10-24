using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fasdr.Backend;
using System.IO.Abstractions.TestingHelpers;
using System.Collections.Generic;
using System.IO;

namespace Fasdr.UnitTests
{
    [TestClass]
    public class DatabaseTest
    {
        static string UserPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        [TestMethod]
        public void TestCanConstruct()
        {
            var fileSystem = new MockFileSystem();
            var db = new Database(fileSystem);
        }

        [TestMethod]
        public void TestConfigLoadFileNotFound()
        {
            var fileSystem = new MockFileSystem();
            fileSystem.AddDirectory(UserPath);

            var db = new Database(fileSystem);
            db.Load();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TestConfigLoadFileCorruptedFile()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {
                {  System.IO.Path.Combine(UserPath,"fasdrConfig.txt"), new MockFileData("ThisShouldNotParse") }
            });

            var db = new Database(fileSystem);
            db.Load();
        }

        [TestMethod]
        public void TestConfigLoadFileEmptyFile()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {
                {  System.IO.Path.Combine(UserPath,"fasdrConfig.txt"), new MockFileData("") }
            });
 
            var db = new Database(fileSystem);
            db.Load();

            Assert.AreEqual(0,db.PathToWeight.Count);
        }


        [TestMethod]
        public void TestConfigLoadFile()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {
                {  System.IO.Path.Combine(UserPath,"fasdrConfig.txt"),
                    new MockFileData(
                        @"c:\dir1\|101.0" + Environment.NewLine +
                        @"c:\dir1\dir2|10.0" + Environment.NewLine) }
            });

            var db = new Database(fileSystem);
            db.Load();

            Assert.AreEqual(2, db.PathToWeight.Count);
            Assert.IsTrue(db.PathToWeight.ContainsKey(@"c:\dir1\"));
            Assert.AreEqual(101.0f, db.PathToWeight[@"c:\dir1\"]);
            Assert.IsTrue(db.PathToWeight.ContainsKey(@"c:\dir1\dir2"));
            Assert.AreEqual(10.0f, db.PathToWeight[@"c:\dir1\dir2"]);
        }

    }
}
