using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SearchingTools
{
	/// <summary>
	/// Хранилище объектов BitmapSearcher, доступных по строковым id
	/// Поддерживает асинхроннные операции с элементами
	/// </summary>
	public sealed class BitmapSearcherStore
	{
		private object locker = new object();

		private int tasksRunning = 0;
		public int TasksRunning
		{
			get { return tasksRunning; }
			private set { tasksRunning = value; }
		}

		public int Count 
		{
			get { lock (locker) { return simpleStore.Count; } } 
		}
		

		private SimpleStore simpleStore = new SimpleStore();

		public IEnumerable<string> Keys
		{
			get { lock (locker) { return simpleStore.Keys; } }
		}

		/// <summary>
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		/// <exception cref="Sysem.IOException"></exception>
		public static BitmapSearcherStore Load(string path)
		{
			var store = new BitmapSearcherStore();
			store.simpleStore = SimpleStore.Load(path);
			return store;
		}

		public void Save(string path)
		{
			lock (locker)
			{
				simpleStore.Save(path);
			}
		}

		public bool Remove(string id)
		{
			lock (locker)
			{
				if (tasksRunning > 0)
					return false;
				return simpleStore.Remove(id);
			}
		}

		public Bitmap GetTemplate(string id)
		{
			lock (locker)
			{
				return simpleStore[id].GetTemplate();
			}
		}

		public SimpleColor GetTemplateSettings(string id)
		{
			lock (locker)
			{
				return simpleStore[id].GetAdmissibleDifference();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <param name="template"></param>
		/// <exception cref="System.InvalidOperationException"></exception>
		public void Add(string id, Bitmap template)
		{
			lock (locker)
			{
				if (simpleStore.ContainsKey(id))
					throw new InvalidOperationException("Object with the same id alredy exists");
				var searcher = new BitmapSearcher(template, SimpleColor.FromRgb(0, 0, 0));
				simpleStore[id] = searcher;
			}
		}

		private void ThrowIfIdDoesntExist(string id)
		{
			lock (locker)
			{
				if (!simpleStore.Keys.Contains(id))
					throw new InvalidOperationException("Object with this id doesn't exist");
			}
		}

		/// <summary>
		/// Адаптирует настройки поисковика при помощи изображения, содержащего count элементов
		/// </summary>
		/// <param name="id"></param>
		/// <param name="image"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public Task UpgradeAsync(string id, Bitmap image, int count)
		{
			ThrowIfIdDoesntExist(id);

			Action upgrade = () =>
			{
				BitmapSearcher newSearcher;
				lock (locker)
				{
					ThrowIfIdDoesntExist(id);
					tasksRunning++;
					newSearcher = simpleStore[id].DeepClone();
				}

				// Затратная операция не запирает хранилище
				newSearcher.Tune(image, count);

				lock (locker)
				{
					simpleStore[id].UniteWith(newSearcher);
					tasksRunning--;
				}
			};

			return Task.Run(upgrade);
		}

		public List<Point> GetPositions(string id, Bitmap image, bool noOverlapping = false)
		{
			List<Point> positions;
			Size templateSize;
			lock (locker)
			{
				ThrowIfIdDoesntExist(id);
				positions = simpleStore[id].GetPositions(image).ToList();
				templateSize = simpleStore[id].TemplateSize;
			}

			if (noOverlapping)
				positions = DistinctOverlapped(positions, templateSize);
			return positions;
		}

		private static List<Point> DistinctOverlapped(List<Point> positions, Size templateSize)
		{
			positions = positions.OrderBy(p => p.X).ToList();
			var needRemove = new List<bool>(Enumerable.Repeat(false, positions.Count));

			for (int i = 1; i < positions.Count; ++i)
				needRemove[i] = AreOverlapped(positions[i-1], positions[i], templateSize);

			var result = new List<Point>();
			for (int i = 0; i < positions.Count; ++i)
				if (!needRemove[i])
					result.Add(positions[i]);

			return result;
		}

		private static bool AreOverlapped(Point left, Point right, Size size)
		{
			int dx = Math.Abs(left.X - right.X);
			int dy = Math.Abs(left.Y - right.Y);
			return (dx < size.Width && dy < size.Height);
		}
	}
}
