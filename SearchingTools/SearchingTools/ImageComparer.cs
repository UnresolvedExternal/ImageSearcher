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
					/*
					var dR = Math.Abs(imagePointColor.R - templatePointColor.R);
					var dG = Math.Abs(imagePointColor.G - templatePointColor.G);
					var dB = Math.Abs(imagePointColor.B - templatePointColor.B);
					red += Math.Pow(dR, power);
					green += Math.Pow(dG, power);
					blue += Math.Pow(dB, power);
					 * */
				}
			int totalPoints = template.GetLength(0) * template[0].GetLength(0) - uncounted;
			return GetResultSimpleColor(red, green, blue, totalPoints);
		}

		private static SimpleColor GetResultSimpleColor(double red, double green, double blue, double totalPoints)
		{
			red = Math.Sqrt(red);
			green = Math.Sqrt(green);
			blue = Math.Sqrt(blue);
			totalPoints = Math.Sqrt(totalPoints);
			red /= totalPoints;
			green /= totalPoints;
			blue /= totalPoints;
			return SimpleColor.FromRgb(NormalizeColorComponent(red), NormalizeColorComponent(green), NormalizeColorComponent(blue));
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
		public static bool AreEqual(SimpleColor[][] image, Point imageStartPoint, 
			SimpleColor[][] template, SimpleColor reservedColor, SimpleColor admissibleDifference)
		{
			SimpleColor diff = CalculateDifference(image, imageStartPoint, template, reservedColor);
			return (diff.R <= admissibleDifference.R && diff.G <= admissibleDifference.G && diff.B <= admissibleDifference.B);
		}
	}
}