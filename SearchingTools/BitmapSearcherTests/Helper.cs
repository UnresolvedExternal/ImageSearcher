using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Drawing;

namespace BitmapSearcherTests
{
	static class Helper
	{
		static Lazy<string> _root = new Lazy<string>(() =>
			{
				var _location = System.Reflection.Assembly.GetExecutingAssembly().Location;
				_location = _location.Replace(@"\bin\Debug", "");
				_location = _location.Replace(@"\bin\Release", "");
				_location = Path.GetDirectoryName(_location);
				_location = Path.Combine(_location, "Tests");
				return _location;
			});
		public static string RootDirectory { get { return _root.Value; } }

		public static string GetFilename(string basic, int testNumber, string extension)
		{
			return Path.Combine(RootDirectory, basic + testNumber + extension);
		}

		public static IEnumerable<Point> ReadPoints(string filename)
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

		public static IEnumerable<TestCaseData> EnumerateTests(string dir)
		{
			for (int num = 1; true; ++num)
			{
				string templateFilename = Helper.GetFilename(dir + "template", num, ".bmp");
				string imageFilename = Helper.GetFilename(dir + "image", num, ".bmp");
				string answerFilename = Helper.GetFilename(dir + "answer", num, ".txt");

				if (!File.Exists(templateFilename))
					yield break;

				var template = (Bitmap)Image.FromFile(templateFilename);
				var image = (Bitmap)Image.FromFile(imageFilename);
				var points = Helper.ReadPoints(answerFilename);

				yield return new TestCaseData(image, template, points);
			}
		}
	}
}
