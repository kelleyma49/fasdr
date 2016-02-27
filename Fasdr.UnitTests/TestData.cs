using System;
using System.Text;
using Fasdr.Backend;

namespace Fasdr.UnitTests
{
	public static class TestData
	{
		static readonly Entry Dir1 = new Entry(@"c:\dir1\",101,DateTime.Now,false);
		static readonly Entry Dir1File2 = new Entry(@"c:\dir1\file2",10,DateTime.Now,true);
		static readonly Entry Dir1Dir2TestStr = new Entry(@"c:\dir1\dir2\testStr",110,DateTime.Now,true);
		static readonly Entry Dir1TestStr = new Entry(@"c:\dir1\testStr",101,DateTime.Now,true);
		static readonly Entry TestStr = new Entry(@"c:\testStr",10,DateTime.Now,true);

		public static string GetTwoDirFilesystem()
		{
			return String.Join (Environment.NewLine, Dir1.ToString (), Dir1File2.ToString ());
		}

		public static string GetMatchFilesystem()
		{
			return string.Join (Environment.NewLine, Dir1Dir2TestStr.ToString (), Dir1TestStr.ToString (),
				TestStr.ToString ());
		}
	}
}

