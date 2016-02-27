using System;
using NUnit.Framework;
using Fasdr.Backend;

namespace Fasdr.UnitTests
{
	[TestFixture]
	public class EntryTest
	{
		[Test]
		public void TestCanCreateFromString()
		{
			var now = DateTime.UtcNow;
			var s = string.Join ("" + Entry.Separator, @"c:\dir1\", "1", now, false);
			Entry e = Entry.FromString (s);
			Assert.AreEqual (@"c:\dir1\", e.FullPath);
		}
	}
}

