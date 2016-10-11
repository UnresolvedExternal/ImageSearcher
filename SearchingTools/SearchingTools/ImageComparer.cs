using System;
using System.Drawing;

namespace SearchingTools
{
	internal static class ImageComparer
	{
		//private static double power = 2;

		/// <summary>
		/// Подсчитывает среднеквадратичное отклонение по каждому компоненту RGB.
		/// </summary>
		/// <param name="image">Матрица первого изображения</param>
		/// <param name="imageStart">Левая верхняя сопоставляемая точка в матрице image</param>
		/// <param name="template">Матрица сопоставляемого шаблона</param>
		/// <param name="reservedColor">Точки этого цвета в шаблоне не учитываются</param>
		public static SimpleColor CalculateDifference(SimpleColor[][] image, Point imageStart, 
			SimpleColor[][] template, SimpleColor reservedColor)
		{
			int red = 0, green = 0, blue = 0;
			int uncounted = 0;
			for (int dx = 0; dx < template.GetLength(0); ++dx)
				for (int dy = 0; dy < template[0].GetLength(0); ++dy)
				{
					var imagePointColor = image[imageStart.X + dx][imageStart.Y + dy];
					var templatePointColor = template[dx][dy];
					if (reservedColor == templatePointColor)
					{
						++uncounted;
						continue;
					}
					var dR = imagePointColor.R - templatePointColor.R;
					var dG = imagePointColor.G - templatePointColor.G;
					var dB = imagePointColor.B - templatePointColor.B;
					red += dR * dR;
					green += dG * dG;
					blue += dB * dB;
				}
			int totalPoints = template.GetLength(0) * template[0].GetLength(0) - uncounted;
			return GetResultSimpleColor(red, green, blue, totalPoints);
		}

		public static bool Equals(SimpleColor[][] left, SimpleColor[][] right)
		{
			if (object.ReferenceEquals(left, right))
				return true;
			int width = left.Length;
			int heigth = left[0].Length;

			if (width != right.Length || heigth != right[0].Length)
				return false;

			for (int x = 0; x < width; ++x)
				for (int y = 0; y < heigth; ++y)
					if (left[x][y] != right[x][y])
						return false;

			return true;
		}

		private static SimpleColor GetResultSimpleColor(
			double redSquared, 
			double greenSquared, 
			double blueSquared, 
			double totalPoints)
		{
			redSquared = Math.Sqrt(redSquared);
			greenSquared = Math.Sqrt(greenSquared);
			blueSquared = Math.Sqrt(blueSquared);
			totalPoints = Math.Sqrt(totalPoints);

			redSquared /= totalPoints;
			greenSquared /= totalPoints;
			blueSquared /= totalPoints;

			return SimpleColor.FromRgb(
				NormalizeColorComponent(redSquared), 
				NormalizeColorComponent(greenSquared), 
				NormalizeColorComponent(blueSquared)
			);
		}

		private static byte NormalizeColorComponent(double simpleColorComponent)
		{
			//simpleColorComponent = Math.Pow(simpleColorComponent, 1.0 / 3);
			simpleColorComponent = Math.Round(simpleColorComponent);
			simpleColorComponent = Math.Max(simpleColorComponent, 0);
			simpleColorComponent = Math.Min(simpleColorComponent, byte.MaxValue);
			return (byte)simpleColorComponent;
		}

		/// <summary>
		/// Сопоставляет с шаблоном часть изображения
		/// </summary>
		/// <param name="image">Изображение, часть которого будет сопоставляться с шаблоном</param>
		/// <param name="imageStartPoint">Левая верхняя сопоставляемая точка изображения</param>
		/// <param name="template">Сопоставляемый шаблон</param>
		/// <param name="reservedColor">Не учитываемый цвет в шаблоне</param>
		/// <param name="admissibleDifference">Содержит максимальные допустимые отклонения</param>
		/// <returns>Результат сравнения</returns>
		public static bool Equals(SimpleColor[][] image, Point imageStartPoint, 
			SimpleColor[][] template, SimpleColor reservedColor, SimpleColor admissibleDifference)
		{
			SimpleColor diff = CalculateDifference(image, imageStartPoint, template, reservedColor);
			return (diff.R <= admissibleDifference.R && diff.G <= admissibleDifference.G && diff.B <= admissibleDifference.B);
		}
	}
}