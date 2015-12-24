using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.IO;
using System.Drawing;
using System.Text.RegularExpressions;

namespace SearchingTools
{
	[TestFixture]
	class StoreTest
	{
		static string QuestionFolder;
		static string LearnFolder;
		static string AnswerFolder;

		static StoreTest()
		{
			Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, @"..\..\Tests");
			QuestionFolder = Path.Combine(Environment.CurrentDirectory, @"Questions");
			LearnFolder = Path.Combine(Environment.CurrentDirectory, @"Learn");
			AnswerFolder = Path.Combine(Environment.CurrentDirectory, @"Answer");
		}

		private BitmapSearcher CreateAndLearnSearcher(Action<BitmapSearcher> afterTune = null)
		{
			var template = Image.FromFile(Path.Combine(LearnFolder, "template.bmp")) as Bitmap;
			var g = new BitmapSearcher(template);
			Assert.AreEqual(template.Size, g.TemplateSize);
			foreach (var filename in Directory.EnumerateFiles(LearnFolder).Where(f => !f.EndsWith("template.bmp")))
			{
				var bmp = Image.FromFile(filename) as Bitmap;
				var numberOfItems = int.Parse(Regex.Match(filename, @"\d+").Value);
				g.Tune(bmp, numberOfItems);
				if (afterTune != null)
					afterTune(g);
			}
			return g;
		}

		private void MarkItems(Bitmap toMark, IEnumerable<Point> positions, Size templateSize)
		{
			var graphics = Graphics.FromImage(toMark);
			foreach (var position in positions)
			{
				graphics.DrawRectangle(Pens.Red, new Rectangle(position, templateSize));
			}
		}

		private void MarkItems(BitmapSearcher g, Size templateSize)
		{
			foreach (var filename in Directory.EnumerateFiles(QuestionFolder))
			{
				var bmp = Image.FromFile(filename) as Bitmap;
				MarkItems(bmp, g.GetPositions(bmp), templateSize);
				bmp.Save(Path.Combine(AnswerFolder, Path.GetFileName(filename)));
			}
		}

		[Test]
		public void SaveImproveLoad()
		{
			var g = CreateAndLearnSearcher();
			var store = new BitmapSearcherStore();
			store.Add("panasonic", g);
			store.Add("unisonic", g);
			store.Save(Path.Combine(AnswerFolder, "store.dat"));
			g = null;
			store = null;
			store = BitmapSearcherStore.Load(Path.Combine(AnswerFolder, "store.dat"));
			g = store["unisonic"];
			MarkItems(g, g.TemplateSize);
		}
	}
}
