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
        [TestMethod]
        public void TestConfigLoadFileNotFound()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>{ });
            var db = new Database(fileSystem);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TestConfigLoadFileCorruptedFile()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {
                {  @"c:\fasdrConfig.txt", new MockFileData("ThisShouldNotParse") }
            });
 
            var db = new Database(fileSystem);
            db.Load();
        }


    }
}
