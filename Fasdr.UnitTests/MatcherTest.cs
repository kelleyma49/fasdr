using System;
using NUnit.Framework;
using Fasdr.Backend;
using System.IO.Abstractions.TestingHelpers;
using System.Collections.Generic;
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

            var matches = Matcher.Matches(db, "FileSystem", false, false, false, null, "testStr");
            CollectionAssert.AreEqual(new List<string> { }, matches);
        }


        [Test, TestCaseSource(typeof(MyFactoryClass),"TestSingleElementMatchCases")]
        public void TestSingleElementMatch(string configFileContents, string[] patterns, string[] expected)
        {
            var db = SetupMatchSimple(configFileContents);

            var actual = Matcher.Matches(db, "FileSystem", false, false, false, null, patterns);
            CollectionAssert.AreEqual(expected, actual);
        }

        [Test, TestCaseSource(typeof(MyFactoryClass), "TestSingleElementWithBasePathCases")]
        public void TestSingleElementMatchWithBasePath(string configFileContents, string[] patterns, string[] expected,string basePath)
        {
            var db = SetupMatchSimple(configFileContents);

            var actual = Matcher.Matches(db, "FileSystem", false, false, false, basePath, patterns);
            CollectionAssert.AreEqual(expected, actual);
        }


        public static class MyFactoryClass
        {
            public static IEnumerable TestSingleElementMatchCases
            {
                get
                {
                    yield return new TestCaseData(string.Join(Environment.NewLine,
                        new Entry(@"c:\tools\", 101, DateTime.Now, false),
                        new Entry(@"c:\tree\", 102, DateTime.Now, false)),
                        new string[] { "t" },
                        new string[] { @"c:\tree", @"c:\tools" })
                        .SetName("TestSingleElementMatchSingleChar");


                    yield return new TestCaseData(TestData.GetMatchFilesystem(),
                        new string[] { "testStr" },
                        new string[] { @"c:\dir1\dir2\testStr", @"c:\dir1\testStr", @"c:\testStr" })
                        .SetName("TestSingleElementMatchTestStr");

                    yield return new TestCaseData(TestData.GetMatchFilesystem(),
                        new string[] { "dir1", "testStr" },
                        new string[] { @"c:\dir1\dir2\testStr", @"c:\dir1\testStr", @"c:\testStr" })
                        .SetName("TestSingleElementMatchDir1TestStr");

                    yield return new TestCaseData(string.Join(Environment.NewLine,
                        new Entry(@"c:\ThisIsATest\", 101, DateTime.Now, false),
                        new Entry(@"c:\tiat\", 101, DateTime.Now, false)),
                        new string[] { "TIAT" },
                        new string[] { @"c:\tiat", @"c:\ThisIsATest" })
                        .SetName("TestSingleElementMatchCaseInsensitive");

                    yield return new TestCaseData(string.Join(Environment.NewLine,
                        new Entry(@"c:\ThisIsATest\", 101, DateTime.Now, false),
                        new Entry(@"c:\tat\", 101, DateTime.Now, false)),
                        new string[] { "TIAT" },
                        new string[] { @"c:\ThisIsATest", @"c:\tat" })
                        .SetName("TestSingleElementMatchCamelCase");

                    yield return new TestCaseData(string.Join(Environment.NewLine,
                        new Entry(@"c:\this is a test\", 101, DateTime.Now, false),
                        new Entry(@"c:\tiat\", 101, DateTime.Now, false)),
                        new string[] { "tiat" },
                        new string[] { @"c:\tiat", @"c:\this is a test" })
                        .SetName("TestSingleElementMatchSeparators");

                    yield return new TestCaseData(string.Join(Environment.NewLine,
                        new Entry(@"c:\this is a test\", 101, DateTime.Now, false),
                        new Entry(@"c:\tiat\", 150, DateTime.Now, false)),
                        new string[] { "tiat" },
                        new string[] { @"c:\tiat", @"c:\this is a test" })
                        .SetName("TestSingleElementMatchSeparatorsFrequencyWins");

                    yield return new TestCaseData(string.Join(Environment.NewLine,
                        new Entry(@"c:\test this is\", 150, DateTime.Now, false),
                        new Entry(@"c:\test\", 101, DateTime.Now, false),
                        new Entry(@"c:\te\", 150, DateTime.Now, false)),
                        new string[] { "test$" },
                        new string[] { @"c:\test" })
                        .SetName("TestSuffixMatches");

                    yield return new TestCaseData(string.Join(Environment.NewLine,
                        new Entry(@"c:\test this is\", 150, DateTime.Now, false),
                        new Entry(@"c:\test\", 101, DateTime.Now, false),
                        new Entry(@"c:\te\", 150, DateTime.Now, false)),
                        new string[] { "" },
                        new string[] { })
                        .SetName("TestOnlySuffixToken");

                    yield return new TestCaseData(string.Join(Environment.NewLine,
                       new Entry(@"c:\test this is\", 150, DateTime.Now, false),
                       new Entry(@"c:\this is a test\", 101, DateTime.Now, false),
                       new Entry(@"c:\te\", 150, DateTime.Now, false)),
                       new string[] { "^test" },
                       new string[] { @"c:\test this is" })
                       .SetName("TestPrefixMatches");

                    yield return new TestCaseData(string.Join(Environment.NewLine,
                        new Entry(@"c:\test this is\", 150, DateTime.Now, false),
                        new Entry(@"c:\test\", 101, DateTime.Now, false),
                        new Entry(@"c:\te\", 150, DateTime.Now, false)),
                        new string[] { "^" },
                        new string[] { })
                        .SetName("TestOnlyPrefixToken");

                    yield return new TestCaseData(string.Join(Environment.NewLine,
                      new Entry(@"c:\test this is\", 150, DateTime.Now, false),
                      new Entry(@"c:\test\", 101, DateTime.Now, false),
                      new Entry(@"c:\te\", 150, DateTime.Now, false)),
                      new string[] { "^$" },
                      new string[] { })
                      .SetName("TestPrefixAndSuffixButNoStringToMatch");

                    yield return new TestCaseData(string.Join(Environment.NewLine,
                      new Entry(@"c:\test this is", 150, DateTime.Now, false),
                      new Entry(@"c:\test", 101, DateTime.Now, false),
                      new Entry(@"c:\te", 101, DateTime.Now, false),
                      new Entry(@"c:\this is test", 150, DateTime.Now, false)),
                      new string[] { "^test$" },
                      new string[] { @"c:\test" })
                      .SetName("TestPrefixAndSuffixMatchesExact");

                    yield return new TestCaseData(string.Join(Environment.NewLine,
                      new Entry(@"c:\test this is", 150, DateTime.Now, false),
                      new Entry(@"c:\testandtest", 101, DateTime.Now, false),
                      new Entry(@"c:\te", 101, DateTime.Now, false),
                      new Entry(@"c:\this is test", 150, DateTime.Now, false)),
                      new string[] { "^test$" },
                      new string[] { @"c:\testandtest" })
                      .SetName("TestPrefixAndSuffixAtBothEnds");

                    yield return new TestCaseData(string.Join(Environment.NewLine,
                     new Entry(@"c:\test this is\", 150, DateTime.Now, false),
                     new Entry(@"c:\test\", 101, DateTime.Now, false),
                     new Entry(@"c:\te\", 150, DateTime.Now, false)),
                     new string[] { "=" },
                     new string[] { })
                     .SetName("TestExactButNoStringToMatch");

                    yield return new TestCaseData(string.Join(Environment.NewLine,
                      new Entry(@"c:\test this is", 150, DateTime.Now, false),
                      new Entry(@"c:\test", 101, DateTime.Now, false),
                      new Entry(@"c:\te", 101, DateTime.Now, false),
                      new Entry(@"c:\this is test", 150, DateTime.Now, false)),
                      new string[] { "=test" },
                      new string[] { @"c:\test" })
                      .SetName("TestExactMatch");

                    yield return new TestCaseData(string.Join(Environment.NewLine,
                      new Entry(@"c:\test this is", 150, DateTime.Now, false),
                      new Entry(@"c:\testtest", 101, DateTime.Now, false),
                      new Entry(@"c:\te", 101, DateTime.Now, false),
                      new Entry(@"c:\this is test", 150, DateTime.Now, false)),
                      new string[] { "=test" },
                      new string[] {})
                      .SetName("TestExactMatchShouldFail");
                }
            }

            public static IEnumerable TestSingleElementWithBasePathCases
            {
                get
                {
                    yield return new TestCaseData(string.Join(Environment.NewLine,
                        new Entry(@"c:\tools\d1", 101, DateTime.Now, false),
                        new Entry(@"c:\tree\d1", 102, DateTime.Now, false)),
                        new string[] { "**d1" },
                        new string[] { @"c:\tools\d1" },
                        @"c:\tools")
                        .SetName("TestSingleElementMatchWithBasePathCasesDepth1");

                    yield return new TestCaseData(string.Join(Environment.NewLine,
                        new Entry(@"c:\tools\d1", 101, DateTime.Now, false),
                        new Entry(@"c:\tools\d1d2", 101, DateTime.Now, false),
                        new Entry(@"c:\tree\d1", 102, DateTime.Now, false)),
                        new string[] { "**d2$" },
                        new string[] { @"c:\tools\d1d2" },
                        @"c:\tools")
                        .SetName("TestSingleElementMatchWithBasePathCasesMatchSuffix");

                    yield return new TestCaseData(string.Join(Environment.NewLine,
                        new Entry(@"c:\tools\d1", 101, DateTime.Now, false),
                        new Entry(@"c:\tools\D1D2", 101, DateTime.Now, false),
                        new Entry(@"c:\tree\d1", 102, DateTime.Now, false)),
                        new string[] { "**d2$" },
                        new string[] { @"c:\tools\D1D2" },
                        @"c:\tools")
                        .SetName("TestSingleElementMatchWithBasePathCasesMatchSuffixDifferenceCase");

                    yield return new TestCaseData(string.Join(Environment.NewLine,
                        new Entry(@"c:\tools\d1d2", 101, DateTime.Now, false),
                        new Entry(@"c:\tools\d2d1", 101, DateTime.Now, false),
                        new Entry(@"c:\tree\d1", 102, DateTime.Now, false)),
                        new string[] { "**^d2" },
                        new string[] { @"c:\tools\d2d1" },
                        @"c:\tools")
                        .SetName("TestSingleElementMatchWithBasePathCasesMatchPrefix");

                    yield return new TestCaseData(string.Join(Environment.NewLine,
                        new Entry(@"c:\tools\D1D2", 101, DateTime.Now, false),
                        new Entry(@"c:\tools\D2D1", 101, DateTime.Now, false),
                        new Entry(@"c:\tree\d1", 102, DateTime.Now, false)),
                        new string[] { "**^d2" },
                        new string[] { @"c:\tools\D2D1" },
                        @"c:\tools")
                        .SetName("TestSingleElementMatchWithBasePathCasesMatchPrefixDifferentCase");

                    yield return new TestCaseData(string.Join(Environment.NewLine,
                        new Entry(@"c:\tools\d1d2", 101, DateTime.Now, false),
                        new Entry(@"c:\tools\d2d1", 101, DateTime.Now, false),
                        new Entry(@"c:\tools\d2", 101, DateTime.Now, false),
                        new Entry(@"c:\tree\d1", 102, DateTime.Now, false),
                        new Entry(@"c:\tree\d2", 102, DateTime.Now, false)),
                        new string[] { "**^d2$" },
                        new string[] { @"c:\tools\d2" },
                        @"c:\tools")
                        .SetName("TestSingleElementMatchWithBasePathCasesMatchPrefixAndSuffix");

                    yield return new TestCaseData(string.Join(Environment.NewLine,
                        new Entry(@"c:\tools\D1D2", 101, DateTime.Now, false),
                        new Entry(@"c:\tools\D2D1", 101, DateTime.Now, false),
                        new Entry(@"c:\tools\D2", 101, DateTime.Now, false),
                        new Entry(@"c:\tree\D1", 102, DateTime.Now, false),
                        new Entry(@"c:\tree\D2", 102, DateTime.Now, false)),
                        new string[] { "**^d2$" },
                        new string[] { @"c:\tools\D2" },
                        @"c:\tools")
                        .SetName("TestSingleElementMatchWithBasePathCasesMatchPrefixAndSuffixDifferentCase");

                    yield return new TestCaseData(string.Join(Environment.NewLine,
                        new Entry(@"c:\tools\", 101, DateTime.Now, false),
                        new Entry(@"c:\tree\", 102, DateTime.Now, false)),
                        new string[] { "**t" },
                        new string[] { @"c:\tree", @"c:\tools" },
                        @"c:\")
                        .SetName("TestSingleElementMatchWithBasePathCasesSingleChar");

                    yield return new TestCaseData(string.Join(Environment.NewLine,
                        new Entry(@"c:\TOOLS\", 101, DateTime.Now, false),
                        new Entry(@"c:\TREE\", 102, DateTime.Now, false)),
                        new string[] { "**t" },
                        new string[] { @"c:\TREE", @"c:\TOOLS" },
                        @"c:\")
                        .SetName("TestSingleElementMatchWithBasePathCasesSingleCharDifferentCase");

                    yield return new TestCaseData(string.Join(Environment.NewLine,
                        new Entry(@"c:\dir1\dir2\final3", 101, DateTime.Now, false),
                        new Entry(@"c:\otherdir1\otherdir2\final3", 102, DateTime.Now, false)),
                        new string[] { "**final3" },
                        new string[] { @"c:\dir1\dir2\final3" },
                        @"c:\dir1")
                        .SetName("TestSingleElementMatchWithBasePathCasesDeeperTreeHierarchy");

                    yield return new TestCaseData(string.Join(Environment.NewLine,
                        new Entry(@"c:\dir1\dir2\final3", 101, DateTime.Now, false),
                        new Entry(@"c:\otherdir1\otherdir2\final3", 102, DateTime.Now, false)),
                        new string[] { "**final3" },
                        new string[] { },
                        @"c:\dir1\dir2\final3")
                        .SetName("TestSingleElementMatchWithBasePathCasesShouldFail");
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
                    yield return new TestCaseData(string.Join(Environment.NewLine,
                        new Entry(@"c:\TestDir\AName", 101, DateTime.Now, false),
                        new Entry(@"c:\TestFile\AName", 100, DateTime.Now, true)),
                        new string[] { "AName" },
                        new string[] { @"c:\TestDir\AName", @"c:\TestFile\AName" },
                        false, false)
                        .SetName("TestNoFilters");
                    yield return new TestCaseData(string.Join(Environment.NewLine,
                        new Entry(@"c:\TestDir\AName", 101, DateTime.Now, false),
                        new Entry(@"c:\TestFile\AName", 100, DateTime.Now, true)),
                        new string[] { "AName" },
                        new string[] { @"c:\TestDir\AName" },
                        false, true)
                        .SetName("TestFilterLeaves");
                    yield return new TestCaseData(string.Join(Environment.NewLine,
                        new Entry(@"c:\TestDir\AName", 101, DateTime.Now, false),
                        new Entry(@"c:\TestFile\AName", 100, DateTime.Now, true)),
                        new string[] { "AName" },
                        new string[] { @"c:\TestFile\AName" },
                        true, false)
                        .SetName("TestFilterContainers");
                    yield return new TestCaseData(string.Join(Environment.NewLine,
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

            var actual = Matcher.Matches(db, "FileSystem", filterContainers, filterLeaves, false, null, patterns);
            CollectionAssert.AreEqual(expected, actual);
        }

        [Test, TestCaseSource(typeof(MyFactoryClass), "TestSingleElementNoMatchCases")]
        public void TestElementNoMatch(string configFileContents, string[] patterns)
        {
            var db = SetupMatchSimple(configFileContents);

            var actual = Matcher.Matches(db, "FileSystem", false, false, false, null, patterns);
            CollectionAssert.AreEqual(new List<string> {}, actual);
        }

        [Test]
		public void TestSingleElementMatchUpdated()
		{
			var db = SetupMatchSimple ();

			var matches = Matcher.Matches(db, "FileSystem", false, false, false, null, "testStr");
			CollectionAssert.AreEqual(new List<string>{ @"c:\dir1\dir2\testStr", @"c:\dir1\testStr", @"c:\testStr"},matches);
			Assert.IsTrue(db.Providers["FileSystem"].UpdateEntry(@"c:\dir1\testStr"));
			matches = Matcher.Matches(db, "FileSystem", false, false, false, null, "testStr");
			CollectionAssert.AreEqual(new List<string>{ @"c:\dir1\testStr", @"c:\dir1\dir2\testStr", @"c:\testStr"},matches);
		}
    }
}
