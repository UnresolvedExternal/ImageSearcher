using SearchingTools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using Converter = SearchingTools.ImageConverter;

namespace SearchingTools
{
	/// <summary>
	/// Позволяет искать соответствия между изображением Bitmap и
	/// некоторым постоянным шаблоном.
	/// </summary>
	[DataContract]
	public sealed class BitmapSearcher
    {
		[DataMember]
		private ImageSearcher searcher;

		public BitmapSearcher(Bitmap template)
		{
			searcher = new ImageSearcher(Converter.ToMatrix(template), SimpleColor.FromRgb(0, 0, 0));
		}

		/// <summary>
		/// При необходимости настраивает допустимые отклонения от шаблона. (в сторону увеличения допустимых отклонений)
		/// </summary>
		/// <param name="image">Некоторое изображение</param>
		/// <param name="templateCount">Количество ожидаемых совпадений с шаблоном в image</param>
		public void Tune(Bitmap image, int templateCount)
		{
			if (templateCount <= 0 || object.ReferenceEquals(image, null))
				throw new ArgumentException("image was null or templateCount <= 0");
			searcher.Learn(Converter.ToMatrix(image), templateCount);
		}

		/// <summary>
		/// Находит позиции всех совпадений с шаблоном.
		/// </summary>
		/// <param name="image">Изображение, на котором будет произведен поиск</param>
		/// <returns>Список совпадений - координаты самого верхнего-левого пиксела в каждом совпадении</returns>
		public IEnumerable<Point> GetPositions(Bitmap image)
		{
			if (object.ReferenceEquals(image, null))
				throw new ArgumentException("image is null");
			return searcher.GetPositions(Converter.ToMatrix(image));
		}

		/// <summary>
		/// Загружает из потока объект SearchingTools, сохранённый при помощи метода Save.
		/// </summary>
		public static BitmapSearcher Load(Stream input)
		{
			return SerializationHelper.Deserialize(input);
		}

		public void Save(Stream output)
		{
			SerializationHelper.Serialize(this, output);
		}

		public int TemplateWidth { get { return searcher.TemplateWidth; } }
		public int TemplateHeight { get { return searcher.TemplateHeight; } }
		public Size TemplateSize { get { return new Size(TemplateWidth, TemplateHeight); } }
    }
}
