﻿using System;
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

		// A simple comparator for any reason
		private static int Compare(SimpleColor leftDifference, SimpleColor rightDifference)
		{
			return (leftDifference.R + leftDifference.G + leftDifference.B)
				.CompareTo(rightDifference.R + rightDifference.G + rightDifference.B);
		}

		public SimpleColor[][] GetTemplate()
		{
			return template;
		}

		/// <param name="template"></param>
		/// <param name="reservedColor">Points in template which match reservedColor are considered transparent</param>
		public ImageSearcher(SimpleColor[][] template, SimpleColor reservedColor)
		{
			if (template.GetLength(0) <= smallTemplateWidth || template[0].GetLength(0) <= smallTemplateHeight)
				throw new ArgumentException(
					string.Format("To small size of template. Required at least: width = {0}, height = {1}", 
						smallTemplateWidth + 1, smallTemplateHeight + 1));
						
			this.template = template;
			this.reservedColor = reservedColor;
			InitializeSmallPictureTemplate();
		}

		/// <summary>
		/// Initialize small picture subregion
		/// </summary>
		private void InitializeSmallPictureTemplate()
		{
			int left = (template.GetLength(0) - smallTemplateWidth) / 2;
			int top = (template[0].GetLength(0) - smallTemplateHeight) / 2;
			smallPictureRegion = new Rectangle(left, top, smallTemplateWidth, smallTemplateHeight);
			
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
		/// <param name="source"></param>
		/// <returns></returns>
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
		/// <param name="image"></param>
		/// <param name="elements">Count of elements on the image that should match template</param>
		public void Learn(SimpleColor[][] image, int elements)
		{
			SimpleColor[] minDifferences = Enumerable
				.Repeat(SimpleColor.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue), elements)
				.ToArray();
				
			Point[] positions = new Point[elements];
			
			int maxX = image.GetLength(0) - template.GetLength(0) + 1;
			int maxY = image[0].GetLength(0) - template[0].GetLength(0) + 1;
			
			for (int x = 0; x < maxX; ++x)
				for (int y = 0; y < maxY; ++y)
				{
					SimpleColor diff = ImageComparer.CalculateDifference(
						image,
						new Point(x, y),
						template,
						reservedColor);

					SmartInsert(minDifferences, positions, diff, new Point(x, y));
				}
				
			UpdateAdmissibleDifference(minDifferences);
			UpdateAdmissibleDifferenceForSmallTemplate(image, positions);
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
		/// <param name="other"></param>
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