﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SearchingTools
{
	[TestFixture]
	class SimpleTest
	{
		static string QuestionFolder;
		static string LearnFolder;
		static string AnswerFolder;

		static SimpleTest()
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
		public void GoldTest()
		{
			var g = CreateAndLearnSearcher();
			MarkItems(g, g.TemplateSize);
		}

		[Test]
		public void AndvancedGoldTest()
		{
			var g = CreateAndLearnSearcher();
			using (var fs = new FileStream(Path.Combine(AnswerFolder, "saves.txt"), FileMode.Create))
			{
				g.Save(fs);
				fs.WriteByte(15);
			}
			using (var fs = new FileStream(Path.Combine(AnswerFolder, "saves.txt"), FileMode.Open))
			{
				var g2 = BitmapSearcher.Load(fs);
				MarkItems(g2, g2.TemplateSize);
			}
		}

		[Test]
		public void SaveTest()
		{
			using (var fs = new FileStream(Path.Combine(AnswerFolder, "saves.txt"), FileMode.Create))
			{
				var g = CreateAndLearnSearcher((ws) =>
					{
						ws.Save(fs);
						var t = new System.IO.StreamWriter(fs);
						t.Write(Environment.NewLine);
						t.Flush();
					});
			}
		}

		[Test]
		public async void Crazy()
		{
			var bmp = Image.FromFile(Path.Combine(AnswerFolder, "A13.bmp")) as Bitmap;
			bmp.UnlockBits(
			bmp.LockBits(new Rectangle(new Point(0, 0), bmp.Size), 
				System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat)
			);
			bmp.SetPixel(bmp.Width - 1, bmp.Height - 1, Color.Red);
			bmp.RotateFlip(RotateFlipType.Rotate90FlipY);
			System.Threading.Thread.Sleep(1000);
			bmp.Save(Path.Combine(AnswerFolder, "A13(2).bmp"));
		}
	}
}
