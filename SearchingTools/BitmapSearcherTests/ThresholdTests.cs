using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SearchingTools;
using NUnit.Framework;

namespace BitmapSearcherTests
{
	[TestFixture]
    public class ThresholdTests
    {
		Color transparent = Color.Black;

		IEnumerable<Bitmap> InvalidSizeTemplates
		{
			get
			{
				yield return new Bitmap(1, 1);
				yield return new Bitmap(1, 2);
				yield return new Bitmap(2, 1);
				yield return new Bitmap(1, 4);
				yield return new Bitmap(4, 1);
			}
		}

		IEnumerable<Bitmap> ValidSizeTemplates
		{
			get
			{
				yield return new Bitmap(2, 2);
				yield return new Bitmap(2, 3);
				yield return new Bitmap(3, 2);
				yield return new Bitmap(3, 3);
			}
		}

		IEnumerable<Bitmap> InvalidTransparentDistributionTemplates
		{
			get
			{
				var res = Create(2, 2, Color.Red);
				res.SetPixel(0, 0, transparent);
				res.SetPixel(0, 1, transparent);
				yield return res;

				res = Create(2, 2, Color.Red);
				res.SetPixel(1, 0, transparent);
				res.SetPixel(1, 1, transparent);
				yield return res;

				res = Create(3, 3, Color.Red);
				for (int x = 0; x < 3; ++x)
					res.SetPixel(x, 0, transparent);
				yield return res;

				res = Create(3, 3, Color.Red);
				for (int x = 0; x < 3; ++x)
					res.SetPixel(x, 2, transparent);
				yield return res;
			}
		}

		IEnumerable<Bitmap> ValidTransparentDistributionTemplates
		{
			get
			{
				var res = Create(2, 2, Color.Red);
				res.SetPixel(0, 0, transparent);
				res.SetPixel(1, 1, transparent);
				yield return res;
			}
		}

		[Test]
		public void ThresoldSizeTest()
		{
			foreach (var template in InvalidSizeTemplates)
				Assert.That(() => new BitmapSearcher(template, (SimpleColor)transparent), Throws.ArgumentException);
			foreach (var template in ValidSizeTemplates)
				Assert.That(() => new BitmapSearcher(template, (SimpleColor)transparent), Throws.Nothing);
		}

		[Test]
		public void TransparentDistributionTest()
		{
			foreach (var template in InvalidTransparentDistributionTemplates)
				Assert.That(() => new BitmapSearcher(template, (SimpleColor)transparent), Throws.ArgumentException);
			foreach (var template in ValidTransparentDistributionTemplates)
				Assert.That(() => new BitmapSearcher(template, (SimpleColor)transparent), Throws.Nothing);
		}

		Bitmap Create(int width, int height, Color fillColor)
		{
			var image = new Bitmap(width, height);
			var g = Graphics.FromImage(image);
			g.FillRectangle(new SolidBrush(fillColor), new Rectangle(0, 0, image.Width, image.Height));
			return image;
		}
	}

}
