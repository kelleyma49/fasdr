using System;
using NUnit.Framework;
using Fasdr.Backend;
using System.IO.Abstractions.TestingHelpers;
using System.Collections.Generic;
using System.Linq;

namespace Fasdr.UnitTests
{
    [TestFixture]
    public class MatcherTest
    {
        IDatabase Db { get; set;  }

        [TestFixtureSetUp]
        public void Init()
        {
            var mfs = new MockFileSystem(new Dictionary<string, MockFileData> {
                {  Database.ConfigPath,
                    new MockFileData(
                        string.Join("" + Database.Separator,@"c:\dir1\testStr", "101.0", "FileSystem", "true") +
                        Environment.NewLine +
                        string.Join("" + Database.Separator,@"c:\testStr", "10.0", "FileSystem", "false") +
                        Environment.NewLine)
                }
            });
            var db = new Database(mfs);
            db.Load();
            Db = db;
        }

        [Test]
        public void TestMatch()
        {
            var matches = Matcher.Matches(Db, "testStr", "FileSystem");
            Assert.AreEqual(2, matches.Count());
            CollectionAssert.AreEqual(new List<string>{@"c:\dir1\testStr", @"c:\testStr"},matches);
        }
    }
}
