using System;
using System.Drawing;

namespace SearchingTools
{
	internal static class ImageComparer
	{
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
					var dR = (imagePointColor.R - templatePointColor.R);
					var dG = (imagePointColor.G - templatePointColor.G);
					var dB = (imagePointColor.B - templatePointColor.B);
					red += dR * dR;
					green += dG * dG;
					blue += dB * dB;
				}
			int totalPoints = template.GetLength(0) * template[0].GetLength(0) - uncounted;
			return GetResultSimpleColor(red, green, blue, totalPoints);
		}

		private static SimpleColor GetResultSimpleColor(double red, double green, double blue, int totalPoints)
		{
			red /= totalPoints;
			green /= totalPoints;
			blue /= totalPoints;
			return SimpleColor.FromRgb(NormalizeSimpleColorComponent(red), NormalizeSimpleColorComponent(green), NormalizeSimpleColorComponent(blue));
		}

		private static byte NormalizeSimpleColorComponent(double SimpleColorComponent)
		{
			SimpleColorComponent = Math.Sqrt(SimpleColorComponent);
			SimpleColorComponent = Math.Round(SimpleColorComponent);
			SimpleColorComponent = Math.Max(SimpleColorComponent, 0);
			SimpleColorComponent = Math.Min(SimpleColorComponent, byte.MaxValue);
			return (byte)SimpleColorComponent;
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