using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SearchingTools
{
	/// <summary>
	/// Allows to search items on an image which match template.
	/// Supports decreasing strictness of comparison.
	/// </summary>
	[Serializable]
    public class ImageSearcher
    {
		private static readonly int smallTemplateWidth = 2;
		private static readonly int smallTemplateHeight = 2;

		// Part of template, witch is using as a first filter (in order to increase performance)
		private SimpleColor[][] smallPictureTemplate;

		private SimpleColor[][] template;
		private SimpleColor maxDifference;
		private SimpleColor smallPictureMaxDifference;
		
		/// Template's subregion
		private Rectangle smallPictureRegion;

		private SimpleColor reservedColor;

		// A simple comparator
		private static int Compare(SimpleColor leftDifference, SimpleColor rightDifference)
		{
			return (leftDifference.R + leftDifference.G + leftDifference.B)
				.CompareTo(rightDifference.R + rightDifference.G + rightDifference.B);
		}

		public int Width { get { return ImageComparer.Width(template); } }
		public int Height { get { return ImageComparer.Height(template); } }
		public Size Size { get { return new Size(Width, Height); } }

		public SimpleColor[][] GetTemplate()
		{
			return template;
		}

		/// <param name="reservedColor">Points in template which match reservedColor are considered transparent</param>
		/// <exception cref="System.ArgumentException"></exception>
		public ImageSearcher(SimpleColor[][] template, SimpleColor reservedColor)
		{
			CheckSize(template);
			CheckTransparentDistribution(template, reservedColor);

			this.template = template;
			this.reservedColor = reservedColor;
			InitializeSmallPictureTemplate();
		}

		private static void CheckSize(SimpleColor[][] template)
		{
			if (template.GetLength(0) < smallTemplateWidth || template[0].GetLength(0) < smallTemplateHeight)
				throw new ArgumentException(
					string.Format("To small size of template. Required at least: width = {0}, height = {1}",
						smallTemplateWidth, smallTemplateHeight));
		}

		private static void CheckTransparentDistribution(SimpleColor[][] template, SimpleColor transparent)
		{
			int width = ImageComparer.Width(template);
			int height = ImageComparer.Height(template);

			bool badTransparentDistribution =
				IsAllPointsHasOneColor(template, new Point(0, 0), new Point(1, 0), transparent) ||
				IsAllPointsHasOneColor(template, new Point(0, 0), new Point(0, 1), transparent) ||
				IsAllPointsHasOneColor(template, new Point(width - 1, height - 1), new Point(-1, 0), transparent) ||
				IsAllPointsHasOneColor(template, new Point(width - 1, height - 1), new Point(0, -1), transparent);

			if (badTransparentDistribution)
				throw BadTransparentDistributionException;
		}

		private static bool IsAllPointsHasOneColor(SimpleColor[][] image, Point start, Point delta, SimpleColor color)
		{
			while (start.X >= 0 && start.X < ImageComparer.Width(image) &&
				start.Y >= 0 && start.Y < ImageComparer.Height(image))
			{
				if (image[start.X][start.Y] != color)
					return false;
				start.X += delta.X;
				start.Y += delta.Y;
			}
			return true;
		}

		private static Exception BadTransparentDistributionException
		{
			get
			{
				return new ArgumentException("Threshold row or column couldn't be fully transparent");
			}
		}

		/// <summary>
		/// Initialize small picture subregion
		/// </summary>
		private void InitializeSmallPictureTemplate()
		{
			int maxNonTransparentPoints = 0;
			int width = ImageComparer.Width(template);
			int height = ImageComparer.Height(template);
			int maxX = width - smallTemplateWidth + 1;
			int maxY = height - smallTemplateHeight + 1;
			for (int x = 0; x < maxX; ++x)
				for (int y = 0; y < maxY; ++y)
				{
					int nonTransparent = 0;
					for (int dx = 0; dx < smallTemplateWidth; ++dx)
						for (int dy = 0; dy < smallTemplateHeight; ++dy)
							if (template[x + dx][y + dy] != reservedColor)
								++nonTransparent;
					if (nonTransparent > maxNonTransparentPoints)
					{
						maxNonTransparentPoints = nonTransparent;
						smallPictureRegion = new Rectangle(x, y, smallTemplateWidth, smallTemplateHeight);
					}
				}
			smallPictureTemplate = ImageConverter.CreateMatrix<SimpleColor>(smallTemplateWidth, smallTemplateHeight);
			for (int dx = 0; dx < smallTemplateWidth; ++dx)
				for (int dy = 0; dy < smallTemplateHeight; ++dy)
					smallPictureTemplate[dx][dy] = template[smallPictureRegion.Left + dx][smallPictureRegion.Top + dy];
		}

		/// <summary>
		/// Searches for potential positions of template
		/// </summary>
		private IEnumerable<Point> GetSmallPicturePositions(SimpleColor[][] source)
		{
			int maxX = source.GetLength(0) - template.GetLength(0);
			int maxY = source[0].GetLength(0) - template[0].GetLength(0);
			var verticals = new List<Point>[maxX];

			Parallel.For(0, maxX, x =>
				{
					verticals[x] = new List<Point>();
					for (int y = 0; y < maxY; ++y)
					{
						if (ImageComparer.Equals(
							source,
							new Point(x + smallPictureRegion.Left, y + smallPictureRegion.Top),
							smallPictureTemplate,
							reservedColor,
							smallPictureMaxDifference))
						{
							verticals[x].Add(new Point(x, y));
						}
					}
				});

			for (int x = 0; x < maxX; ++x)
				foreach (var p in verticals[x])
					yield return p;
		}

		/// <summary>
		/// Searches for subpictures that match template
		/// </summary>
		public IEnumerable<Point> GetPositions(SimpleColor[][] source)
		{
			var smallPicturePositions = GetSmallPicturePositions(source);

			foreach (var pos in smallPicturePositions)
			{
				if (ImageComparer.Equals(
					source,
					pos,
					template,
					reservedColor,
					maxDifference))
				{
					yield return pos;
				}
			}

		}

		/// <summary>
		/// Updates maximal admissible differences
		/// </summary>
		/// <param name="elements">Count of elements on the image that should match template</param>
		public void Learn(SimpleColor[][] image, int elements)
		{
			Learn(image, elements, new List<Rectangle>());
		}

		private void Learn(SimpleColor[][] image, int elementsLeft, List<Rectangle> invalidRegions)
		{
			int width = ImageComparer.Width(image);
			int height = ImageComparer.Height(image);

			int maxX = width - this.Width + 1;
			int maxY = height - this.Height + 1;

			var differences = ImageConverter.CreateMatrix<SimpleColor>(maxX, maxY);
			Parallel.For(0, maxX, x =>
				{
					for (int y = 0; y < maxY; ++y)
						differences[x][y] = ImageComparer.CalculateDifference(
							image,
							new Point(x, y),
							this.template,
							this.reservedColor);
				});

			var bestPositions = new List<Point>();
			
			while (elementsLeft > 0)
			{
				var position = FindMin(differences, bestPositions);
				bestPositions.Add(position);
				--elementsLeft;
			}

			var minDiffs = bestPositions.Select(pos => differences[pos.X][pos.Y]).ToArray();
			UpdateAdmissibleDifference(minDiffs);
			UpdateAdmissibleDifferenceForSmallTemplate(image, bestPositions.ToArray());
		}

		private Point FindMin(SimpleColor[][] differences, List<Point> taken)
		{
			int width = ImageComparer.Width(differences);
			int height = ImageComparer.Height(differences);

			SimpleColor min = (SimpleColor)Color.White;
			Point pos = new Point();
			object locker = new object();

			Parallel.For(0, width, x =>
				{
					Point localPos = new Point();
					SimpleColor localMin;
					localPos.X = x;
					lock (locker)
						localMin = min;

					for (int y = 0; y < height; ++y)
					{
						bool valid = true;
						foreach (var p in taken)
							if (IsConflicted(p, x, y))
							{
								valid = false;
								break;
							}

						if (valid && Compare(differences[x][y], localMin) < 0)
						{
							localMin = differences[x][y];
							localPos.Y = y;
						}
					}

					lock (locker)
						if (Compare(localMin, min) < 0)
						{
							min = localMin;
							pos = localPos;
						}
				});

			if (min == (SimpleColor)Color.White)
				throw new InvalidOperationException("Cant find all subpictures. State may be corrupted now.");
			return pos;
		}

		/// <summary>
		/// Templates regions are intersected
		/// </summary>
		private bool IsConflicted(Point p, int x, int y)
		{
			int dx = Math.Abs(p.X - x);
			int dy = Math.Abs(p.Y - y);
			if (dx < Width && dy < Height)
				return true;
			else
				return false;
		}

		private void UpdateAdmissibleDifference(SimpleColor[] differences)
		{
			foreach (var difference in differences)
			{
				maxDifference.R = Math.Max(maxDifference.R, difference.R);
				maxDifference.G = Math.Max(maxDifference.G, difference.G);
				maxDifference.B = Math.Max(maxDifference.B, difference.B);
			}
		}

		private void UpdateAdmissibleDifferenceForSmallTemplate(SimpleColor[][] sourceMatrix, Point[] positions)
		{
			foreach (var position in positions)
			{
				SimpleColor simpleColor = ImageComparer.CalculateDifference(
						sourceMatrix,
						new Point(position.X + smallPictureRegion.Left, position.Y + smallPictureRegion.Top),
						smallPictureTemplate,
						reservedColor);

				smallPictureMaxDifference.R = Math.Max(smallPictureMaxDifference.R, simpleColor.R);
				smallPictureMaxDifference.G = Math.Max(smallPictureMaxDifference.G, simpleColor.G);
				smallPictureMaxDifference.B = Math.Max(smallPictureMaxDifference.B, simpleColor.B);
			}
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

		public SimpleColor GetAdmissibleDifference()
		{
			return maxDifference;
		}

		public bool TemplatesEqual(ImageSearcher other)
		{
			return ImageComparer.Equals(GetTemplate(), other.GetTemplate());
		}

		/// <summary>
		/// Unite .Learn() results for this and another searchers which represent
		/// same templates
		/// </summary>
		public void UniteWith(ImageSearcher other)
		{
			if (!TemplatesEqual(other))
				throw new InvalidOperationException("Objects represent different templates");

			UniteDifferences(ref maxDifference, other.maxDifference);
			UniteDifferences(ref smallPictureMaxDifference, other.smallPictureMaxDifference);
		}

		private static void UniteDifferences(ref SimpleColor left, SimpleColor right)
		{
			left.R = Math.Max(left.R, right.R);
			left.G = Math.Max(left.G, right.G);
			left.B = Math.Max(left.B, right.B);
		}
	}
}