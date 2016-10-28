using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SearchingTools;
using NUnit.Framework;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace BitmapSearcherTests
{
	[TestFixture]
	class StrictedTemplateTests
	{
		static string _dir = "Stricted\\";

		static IEnumerable<TestCaseData> TestCases
		{
			get
			{
				int num = 0;
				foreach (var test in Helper.EnumerateTests(_dir))
				{
					num++;
					yield return test.SetName("Stricted " + num);
				}
			}
		}

		[Test, TestCaseSource("TestCases")]
		public void Test(Bitmap image, Bitmap template, IEnumerable<Point> expected)
		{
			var searcher = new BitmapSearcher(template, (SimpleColor)Color.Black);
			var actual = searcher.GetPositions(image);
			Assert.That(actual, Is.EquivalentTo(expected));
		}
	}
}