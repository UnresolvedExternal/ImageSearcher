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

		static IEnumerable<Point> ReadPoints(string filename)
		{
			string content = File.ReadAllText(filename);
			foreach (Match match in Regex.Matches(content, @"\((?<X>\d+),\s*(?<Y>\d+)\)"))
			{
				Point next = new Point();
				next.X = int.Parse(match.Groups["X"].Value);
				next.Y = int.Parse(match.Groups["Y"].Value);
				yield return next;
			}
		}

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