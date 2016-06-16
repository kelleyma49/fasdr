using System;
using Fasdr.Backend;

namespace Fasdr.UnitTests
{
	public static class TestData
	{
		internal static readonly Entry Dir1 = new Entry(@"c:\dir1\",101,DateTime.Now,false);
		internal static readonly Entry Dir1File2 = new Entry(@"c:\dir1\file2",100,DateTime.Now,true);
		internal static readonly Entry Dir1Dir2TestStr = new Entry(@"c:\dir1\dir2\testStr",101,DateTime.Now,true);
		internal static readonly Entry Dir1TestStr = new Entry(@"c:\dir1\testStr",100,DateTime.Now.AddDays(-1),true);
		internal static readonly Entry TestStr = new Entry(@"c:\testStr",10,DateTime.Now.AddDays(-1),true);

		internal static string GetTwoDirFilesystem()
		{
			return string.Join (Environment.NewLine, Dir1.ToString (), Dir1File2.ToString ());
		}

		internal static string GetMatchFilesystem()
		{
			return string.Join (Environment.NewLine, Dir1Dir2TestStr.ToString (), Dir1TestStr.ToString (),
				TestStr.ToString ());
		}
	}
}

