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
			int width = template.GetLength(0);
			int height = template[0].GetLength(0);
			
			for (int dx = 0; dx < width; ++dx)
				for (int dy = 0; dy < height; ++dy)
				{	
					var templateColor = template[dx][dy];
					
					if (reservedColor == templateColor)
					{
						++uncounted;
						continue;
					}

					var imageColor = image[imageStart.X + dx][imageStart.Y + dy];

					var dR = imageColor.R - templateColor.R;
					var dG = imageColor.G - templateColor.G;
					var dB = imageColor.B - templateColor.B;
					
					red += dR * dR;
					green += dG * dG;
					blue += dB * dB;
				}

			var totalPoints = width * height - uncounted;
			return GetResultSimpleColor(red, green, blue, totalPoints);
		}

		public static bool Equals(SimpleColor[][] left, SimpleColor[][] right)
		{
			if (object.ReferenceEquals(left, right))
				return true;
			
			int width = Width(left);
			int heigth = Height(left);

			if (width != Width(right) || heigth != Height(right))
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
			simpleColorComponent = Math.Ceiling(simpleColorComponent);
			simpleColorComponent = Math.Max(simpleColorComponent, 0);
			simpleColorComponent = Math.Min(simpleColorComponent, byte.MaxValue);
			return (byte)simpleColorComponent;
		}

		/// <summary>
		/// Сопоставляет с шаблоном часть изображения
		/// </summary>
		/// <param name="image">Изображение, часть которого будет сопоставляться с шаблоном</param>
		/// <param name="imageStart">Левая верхняя сопоставляемая точка изображения</param>
		/// <param name="template">Сопоставляемый шаблон</param>
		/// <param name="reservedColor">Не учитываемый цвет в шаблоне</param>
		/// <param name="admissibleDifference">Содержит максимальные допустимые отклонения</param>
		/// <returns>Результат сравнения</returns>
		public static bool Equals(SimpleColor[][] image, Point imageStart, 
			SimpleColor[][] template, SimpleColor reservedColor, SimpleColor admissibleDifference)
		{
			SimpleColor diff = CalculateDifference(image, imageStart, template, reservedColor);
			return (diff.R <= admissibleDifference.R && diff.G <= admissibleDifference.G && diff.B <= admissibleDifference.B);
		}

		public static int Width(SimpleColor[][] image)
		{
			return image.Length;
		}

		public static int Height(SimpleColor[][] image)
		{
			return image[0].Length;
		}
	}
}