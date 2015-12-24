// COMMENT

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SearchingTools
{
	[Serializable]
    public class ImageSearcher
    {
		private static readonly int smallPictureWidth = 2;
		private static readonly int smallPictureHeight = 2;

		private SimpleColor[][] template;
		public int TemplateWidth { get { return template.Length; } }
		public int TemplateHeight { get { return template[0].Length; } }

		// Part of template, witch is using as a first filter (in order to increase performance)
		private SimpleColor[][] smallPictureTemplate;

		private SimpleColor maxDifference;
		private SimpleColor smallPictureMaxDifference;
		
		/// Template's subregion
		private Rectangle smallPictureRegion;

		private SimpleColor reservedColor;

		// A simple comparator for any reason
		private static int Compare(SimpleColor first, SimpleColor second)
		{
			return (first.R + first.G + first.B).CompareTo(second.R + second.G + second.B);
		}

		public ImageSearcher(SimpleColor[][] template, SimpleColor reservedColor)
		{
			if (template.GetLength(0) <= smallPictureWidth || template[0].GetLength(0) <= smallPictureHeight)
				throw new ArgumentException(
					string.Format("To small size of template. Required at least: width = {0}, height = {1}", 
						smallPictureWidth + 1, smallPictureHeight + 1));
						
			this.template = template;
			this.reservedColor = reservedColor;
			
			InitializeSmallPictureTemlpate();
		}

		// Middle subregion of template
		private void InitializeSmallPictureTemlpate()
		{
			int left = (template.GetLength(0) - smallPictureWidth) / 2;
			int top = (template[0].GetLength(0) - smallPictureHeight) / 2;
			
			smallPictureRegion = new Rectangle(left, top, smallPictureWidth, smallPictureHeight);
			
			smallPictureTemplate = ImageConverter.CreateMatrix<SimpleColor>(smallPictureRegion.Width, 
				smallPictureRegion.Height);
				
			for (int x = 0; x < smallPictureRegion.Width; ++x)
				for (int y = 0; y < smallPictureRegion.Height; ++y)
					smallPictureTemplate[x][y] = template[smallPictureRegion.Left + x][smallPictureRegion.Top + y];
		}

		/// <summary>
		/// Searches for potential positions of template
		/// </summary>
		private IEnumerable<Point> GetSmallPicturePositions(SimpleColor[][] source)
		{
			int maxX = source.GetLength(0) - template.GetLength(0);
			int maxY = source[0].GetLength(0) - template[0].GetLength(0);
			
			for (int x = 0; x < maxX; ++x)
				for (int y = 0; y < maxY; ++y)
				{
					if (ImageComparer.AreEqual(
						source,
						new Point(x + smallPictureRegion.Left, y + smallPictureRegion.Top),
						smallPictureTemplate,
						reservedColor,
						smallPictureMaxDifference))
					{
						yield return (new Point(x, y));
					}
				}
		}

		public IEnumerable<Point> GetPositions(SimpleColor[][] source)
		{
			var smallPicturePositions = GetSmallPicturePositions(source);
			
			foreach (var pos in smallPicturePositions)
				if (ImageComparer.AreEqual(
					source,
					pos,
					template,
					reservedColor,
					maxDifference))
				{
					yield return pos;
				}
		}

		/// <summary>
		/// Finds .bmp files in given directory. (Each .bmp filename must consists of a number - how much positions
		/// are satisfied to template. According to image and this info, this method sets maximal admissible
		/// differences of RGB components.
		/// </summary>
		public void Learn(string directory)
		{
			this.maxDifference = SimpleColor.FromRgb(0, 0, 0);
			this.smallPictureMaxDifference = SimpleColor.FromRgb(0, 0, 0);
			
			foreach (var filename in Directory.GetFiles(directory).Where(name => name.ToUpper().EndsWith(".BMP")))
			{
				Bitmap source = Image.FromFile(filename) as Bitmap;
				var name = Path.GetFileName(filename);
				var number = Regex.Match(name, @"\d+");
				int value = int.Parse(number.Value);
				Learn(ImageConverter.ToMatrix(source), value);
			}
		}

		/// <summary>
		/// Updates maximal admissible differences.
		/// </summary>
		public void Learn(SimpleColor[][] sourceMatrix, int elementsCount)
		{
			SimpleColor[] minValues = 
				Enumerable.Repeat(SimpleColor.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue), elementsCount).ToArray();
				
			Point[] positions = new Point[elementsCount];
			
			int maxX = sourceMatrix.GetLength(0) - template.GetLength(0);
			int maxY = sourceMatrix[0].GetLength(0) - template[0].GetLength(0);
			
			for (int x = 0; x < maxX; ++x)
				for (int y = 0; y < maxY; ++y)
				{
					SimpleColor diff = ImageComparer.CalculateDifference(
						sourceMatrix,
						new Point(x, y),
						template,
						SimpleColor.FromRgb(0, 0, 0));
					SmartInsert(minValues, positions, diff, new Point(x, y));
				}
				
			UpdateMaxDifference(minValues);
			ModifySmallPictureMaxDifference(sourceMatrix, positions);
		}

		private void UpdateMaxDifference(SimpleColor[] minValues)
		{
			byte[] newDiff = new[] { this.maxDifference.R, this.maxDifference.G, this.maxDifference.B };
			
			foreach (var color in minValues)
			{
				newDiff[0] = Math.Max(newDiff[0], color.R);
				newDiff[1] = Math.Max(newDiff[1], color.G);
				newDiff[2] = Math.Max(newDiff[2], color.B);
			}
			
			this.maxDifference = SimpleColor.FromRgb(newDiff[0], newDiff[1], newDiff[2]);
		}

		private void ModifySmallPictureMaxDifference(SimpleColor[][] sourceMatrix, Point[] positions)
		{
			byte[] newDiff = new[] { this.smallPictureMaxDifference.R, this.smallPictureMaxDifference.G, this.smallPictureMaxDifference.B };
			
			foreach (var position in positions)
			{
				SimpleColor simpleColor = ImageComparer.CalculateDifference(
						sourceMatrix,
						new Point(position.X + smallPictureRegion.Left, position.Y + smallPictureRegion.Top),
						smallPictureTemplate,
						reservedColor);
				newDiff[0] = Math.Max(newDiff[0], simpleColor.R);
				newDiff[1] = Math.Max(newDiff[1], simpleColor.G);
				newDiff[2] = Math.Max(newDiff[2], simpleColor.B);
			}
			
			this.smallPictureMaxDifference = SimpleColor.FromRgb(newDiff[0], newDiff[1], newDiff[2]);
		}

		private static void SmartInsert(SimpleColor[] minValues, Point[] positions, SimpleColor value, Point pos)
		{
			int indexOfMax = 0;
			
			for (int i = 1; i < minValues.Length; ++i)
				if (Compare(minValues[i], minValues[indexOfMax]) > 0)
					indexOfMax = i;
					
			if (Compare(value, minValues[indexOfMax]) < 0)
			{
				minValues[indexOfMax] = value;
				positions[indexOfMax] = pos;
			}
		}
    }
}