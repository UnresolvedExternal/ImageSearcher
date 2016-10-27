using SearchingTools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
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

		/// <exception cref="System.ArgumentException">Invalid template</exception>
		public BitmapSearcher(Bitmap template, SimpleColor transparentColor)
		{
			searcher = new ImageSearcher(Converter.ToMatrix(template), transparentColor);
		}

		public Bitmap GetTemplate()
		{
			return Converter.ToBitmap(searcher.GetTemplate());
		}

		public BitmapSearcher Clone()
		{
			using (var memory = new MemoryStream())
			{
				SerializationHelper.Serialize(this, memory);
				memory.Position = 0;
				return (BitmapSearcher)SerializationHelper.Deserialize(memory);
			}
		}

		/// <summary>
		/// При необходимости увеличивает допустимые отклонения от шаблона. (в сторону увеличения допустимых отклонений)
		/// </summary>
		/// <param name="image">Некоторое изображение</param>
		/// <param name="templateCount">Количество ожидаемых совпадений с шаблоном в image</param>
		/// <exception cref="System.ArgumentOutOfRangeException"></exception>
		/// <exception cref="System.ArgumentNullException"></exception>
		public void Learn(Bitmap image, int templateCount)
		{
			if (templateCount <= 0)
				throw new ArgumentOutOfRangeException("templateCount");
			if (object.ReferenceEquals(image, null))
				throw new ArgumentNullException("image");
			searcher.Learn(Converter.ToMatrix(image), templateCount);
		}

		/// <summary>
		/// Находит позиции всех совпадений с шаблоном.
		/// </summary>
		/// <param name="image">Изображение, на котором будет произведен поиск</param>
		/// <returns>Список совпадений - координаты самого верхнего-левого пиксела в каждом совпадении</returns>
		public IEnumerable<Point> GetPositions(Bitmap image)
		{
			return searcher.GetPositions(Converter.ToMatrix(image));
		}

		/// <summary>
		/// Загружает из потока объект BitmapSearcher, сохранённый при помощи метода Save.
		/// </summary>
		/// <exception cref="System.Runtime.SerializationException"></exception>
		public static BitmapSearcher Load(Stream input)
		{
			return SerializationHelper.Deserialize(input);
		}

		/// <exception cref="System.Runtime.SerializationException"></exception>
		public void Save(Stream output)
		{
			SerializationHelper.Serialize(this, output);
		}

		public int TemplateWidth { get { return searcher.GetTemplate().Length; } }
		public int TemplateHeight { get { return searcher.GetTemplate()[0].Length; } }
		public Size TemplateSize { get { return new Size(TemplateWidth, TemplateHeight); } }

		public SimpleColor GetAdmissibleDifference()
		{
			return searcher.GetAdmissibleDifference();
		}

		/// <summary>
		/// Объединяет результаты обучения объектов. Результат сохраняет в текущий объект.
		/// </summary>
		/// <param name="other"></param>
		public void UniteWith(BitmapSearcher other)
		{
			this.searcher.UniteWith(other.searcher);
		}
	}
}
