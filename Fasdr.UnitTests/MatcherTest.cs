using System;
using NUnit.Framework;
using Fasdr.Backend;
using System.IO.Abstractions.TestingHelpers;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Fasdr.UnitTests
{
    [TestFixture]
    public class MatcherTest
    {
        static readonly string FileSystemConfigPath = System.IO.Path.Combine(Database.DefaultConfigDir, $"{Database.ConfigFilePrefix}.FileSystem.txt");

        private IDatabase SetupEmptyDatabase()
        {
            var mfs = new MockFileSystem(new Dictionary<string, MockFileData> { });
            return new Database(mfs);
        }

        private IDatabase SetupMatchSimple(string contents)
        {
            var mfs = new MockFileSystem(new Dictionary<string, MockFileData> {
                {
                    FileSystemConfigPath,new MockFileData(contents)
                }
            });
            var db = new Database(mfs);
            db.Load();
            return db;
        }

        private IDatabase SetupMatchSimple()
        {
            return SetupMatchSimple(TestData.GetMatchFilesystem());
        }


        [Test]
        public void TestEmptyDatabase()
        {
            var db = SetupEmptyDatabase();

            var matches = Matcher.Matches(db, "FileSystem", false, false, false, "testStr");
            CollectionAssert.AreEqual(new List<string> { }, matches);
        }


        [Test, TestCaseSource(typeof(MyFactoryClass),"TestSingleElementMatchCases")]
        public void TestSingleElementMatch(string configFileContents, string[] patterns, string[] expected, bool matchAllIfEmpty)
        {
            var db = SetupMatchSimple(configFileContents);

            var actual = Matcher.Matches(db, "FileSystem", false, false, false, patterns);
            CollectionAssert.AreEqual(expected, actual);
        }


        public class MyFactoryClass
        {
            public static IEnumerable TestSingleElementMatchCases
            {
                get
                {
                    yield return new TestCaseData(String.Join(Environment.NewLine,
                        new Entry(@"c:\tools\", 101, DateTime.Now, false),
                        new Entry(@"c:\tree\", 102, DateTime.Now, false)),
                        new string[] { "t" },
                        new string[] { @"c:\tree\", @"c:\tools\" },
                        false)
                        .SetName("TestSingleElementMatchSingleChar");


                    yield return new TestCaseData(TestData.GetMatchFilesystem(),
                        new string[] { "testStr" },
                        new string[] { @"c:\dir1\dir2\testStr", @"c:\dir1\testStr", @"c:\testStr"  },
                        false)
                        .SetName("TestSingleElementMatchTestStr");

                    yield return new TestCaseData(TestData.GetMatchFilesystem(),
                        new string[] { "dir1", "testStr" },
                        new string[] { @"c:\dir1\dir2\testStr", @"c:\dir1\testStr", @"c:\testStr" },
                        false)
                        .SetName("TestSingleElementMatchDir1TestStr");

                    yield return new TestCaseData(String.Join(Environment.NewLine,
                        new Entry(@"c:\ThisIsATest\", 101, DateTime.Now, false),
                        new Entry(@"c:\tiat\", 101, DateTime.Now, false)),
                        new string[] { "TIAT" },
                        new string[] { @"c:\tiat\",@"c:\ThisIsATest\"  },
                        false)
                        .SetName("TestSingleElementMatchCaseInsensitive");

                    yield return new TestCaseData(String.Join(Environment.NewLine,
                        new Entry(@"c:\ThisIsATest\", 101, DateTime.Now, false),
                        new Entry(@"c:\tat\", 101, DateTime.Now, false)),
                        new string[] { "TIAT" },
                        new string[] { @"c:\ThisIsATest\", @"c:\tat\" },
                        false)
                        .SetName("TestSingleElementMatchCamelCase");

                    yield return new TestCaseData(String.Join(Environment.NewLine,
                        new Entry(@"c:\this is a test\", 101, DateTime.Now, false),
                        new Entry(@"c:\tiat\", 101, DateTime.Now, false)),
                        new string[] { "tiat" },
                        new string[] { @"c:\tiat\", @"c:\this is a test\"  },
                        false)
                        .SetName("TestSingleElementMatchSeparators");

                    yield return new TestCaseData(String.Join(Environment.NewLine,
                        new Entry(@"c:\this is a test\", 101, DateTime.Now, false),
                        new Entry(@"c:\tiat\", 150, DateTime.Now, false)),
                        new string[] { "tiat" },
                        new string[] { @"c:\tiat\", @"c:\this is a test\",  },
                        false)
                        .SetName("TestSingleElementMatchSeparatorsFrequencyWins");

                    yield return new TestCaseData(String.Join(Environment.NewLine,
                        new Entry(@"c:\test this is\", 150, DateTime.Now, false),
                        new Entry(@"c:\test\", 101, DateTime.Now, false),
                        new Entry(@"c:\te\", 150, DateTime.Now, false)),
                        new string[] { "test$" },
                        new string[] { @"c:\test\" },
                        false)
                        .SetName("TestElementAtEndOfLine");
                }
            }

            public static IEnumerable TestSingleElementNoMatchCases
            {
                get
                {
                    yield return new TestCaseData(TestData.GetMatchFilesystem(),new string[] { "NotThere" })
                        .SetName("TestElementNoMatchSingle");

                    yield return new TestCaseData(TestData.GetMatchFilesystem(), new string[] { "NotThere1" , "NotThere2" })
                        .SetName("TestElementNoMatchDouble");
                }
            }

            public static IEnumerable TestFilterTypes
            {
                get
                {
                    yield return new TestCaseData(String.Join(Environment.NewLine,
                        new Entry(@"c:\TestDir\AName", 101, DateTime.Now, false),
                        new Entry(@"c:\TestFile\AName", 100, DateTime.Now, true)),
                        new string[] { "AName" },
                        new string[] { @"c:\TestDir\AName", @"c:\TestFile\AName" },
                        false, false)
                        .SetName("TestNoFilters");
                    yield return new TestCaseData(String.Join(Environment.NewLine,
                        new Entry(@"c:\TestDir\AName", 101, DateTime.Now, false),
                        new Entry(@"c:\TestFile\AName", 100, DateTime.Now, true)),
                        new string[] { "AName" },
                        new string[] { @"c:\TestDir\AName" },
                        false, true)
                        .SetName("TestFilterLeaves");
                    yield return new TestCaseData(String.Join(Environment.NewLine,
                        new Entry(@"c:\TestDir\AName", 101, DateTime.Now, false),
                        new Entry(@"c:\TestFile\AName", 100, DateTime.Now, true)),
                        new string[] { "AName" },
                        new string[] { @"c:\TestFile\AName" },
                        true, false)
                        .SetName("TestFilterContainers");
                    yield return new TestCaseData(String.Join(Environment.NewLine,
                        new Entry(@"c:\TestDir\AName", 101, DateTime.Now, false),
                        new Entry(@"c:\TestFile\AName", 100, DateTime.Now, true)),
                        new string[] { "AName" },
                        new string[] {},
                        true, true)
                        .SetName("TestFilterContainersAndLeaves");
                }
            }
        }

        [Test, TestCaseSource(typeof(MyFactoryClass), "TestFilterTypes")]
        public void TestFilterByTypes(string configFileContents, string[] patterns, string[] expected,bool filterContainers,bool filterLeaves)
        {
            var db = SetupMatchSimple(configFileContents);

            var actual = Matcher.Matches(db, "FileSystem", filterContainers, filterLeaves, false, patterns);
            CollectionAssert.AreEqual(expected, actual);
        }

        [Test, TestCaseSource(typeof(MyFactoryClass), "TestSingleElementNoMatchCases")]
        public void TestElementNoMatch(string configFileContents, string[] patterns)
        {
            var db = SetupMatchSimple(configFileContents);

            var actual = Matcher.Matches(db, "FileSystem", false, false, false, patterns);
            CollectionAssert.AreEqual(new List<string> {}, actual);
        }

        [Test]
		public void TestSingleElementMatchUpdated()
		{
			var db = SetupMatchSimple ();

			var matches = Matcher.Matches(db, "FileSystem", false, false, false, "testStr");
			CollectionAssert.AreEqual(new List<string>{ @"c:\dir1\dir2\testStr", @"c:\dir1\testStr", @"c:\testStr"},matches);
			Assert.IsTrue(db.Providers["FileSystem"].UpdateEntry(@"c:\dir1\testStr"));
			matches = Matcher.Matches(db, "FileSystem", false, false, false, "testStr");
			CollectionAssert.AreEqual(new List<string>{ @"c:\dir1\testStr", @"c:\dir1\dir2\testStr", @"c:\testStr"},matches);
		}
    }
}
