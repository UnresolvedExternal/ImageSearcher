using System.Drawing;
using System.Drawing.Imaging;

namespace SearchingTools
{
	/// <summary>
	/// Encapsulates to and from matrix coversations for Bitmap images.
	/// </summary>
	public static class ImageConverter
	{
		/// <summary>
		/// Creates a matrix T[width][height] 
		/// </summary>
		public static T[][] CreateMatrix<T>(int width, int height)
		{
			T[][] matrix = new T[width][];
			for (int x = 0; x < width; ++x)
				matrix[x] = new T[height];
			return matrix;
		}

		public static SimpleColor[][] ToMatrix(Bitmap image)
		{
			int width = image.Width;
			int height = image.Height;
			BitmapData imageData =
				image.LockBits(new Rectangle(Point.Empty, image.Size), System.Drawing.Imaging.ImageLockMode.ReadOnly, image.PixelFormat);
			var imageStride = imageData.Stride;
			var imageScan0 = imageData.Scan0;
			var matrix = CreateMatrix<SimpleColor>(width, height);
			var pixelSize = imageStride / width;
			unsafe
			{
				for (int y = 0; y < height; ++y)
				{
					var imageRow = (byte*)imageScan0 + (y * imageStride);
					for (int x = 0; x < width; ++x)
					{
						matrix[x][y] = SimpleColor.FromRgb(
							imageRow[x * pixelSize + 2],
							imageRow[x * pixelSize + 1],
							imageRow[x * pixelSize + 0]);
					}
				}
			}
			image.UnlockBits(imageData);
			return matrix;
		}

		public static Bitmap ToBitmap(SimpleColor[][] matrix)
		{
			int width = matrix.Length;
			int height = matrix[0].Length;
			Bitmap result = new Bitmap(width, height);
			BitmapData imageData =
				result.LockBits(new Rectangle(Point.Empty, result.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, result.PixelFormat);
			var imageStride = imageData.Stride;
			var imageScan0 = imageData.Scan0;
			var pixelSize = imageStride / width;
			unsafe
			{
				for (int y = 0; y < height; ++y)
				{
					var imageRow = (byte*)imageScan0 + (y * imageStride);
					for (int x = 0; x < width; ++x)
					{
						imageRow[x * pixelSize + 2] = matrix[x][y].R;
						imageRow[x * pixelSize + 1] = matrix[x][y].G;
						imageRow[x * pixelSize + 0] = matrix[x][y].B;
					}
				}
			}
			result.UnlockBits(imageData);
			return result;
		}

	}
}