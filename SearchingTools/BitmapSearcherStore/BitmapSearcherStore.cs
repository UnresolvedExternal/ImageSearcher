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
	/// Поддерживает асинхроннные операции с элементами и потокобезопасен
	/// </summary>
	public sealed class BitmapSearcherStore
	{
		private object locker = new object();

		private int tasksRunning = 0;
		public int TasksRunning
		{
			get { lock (locker) return tasksRunning; }
			private set { lock (locker) tasksRunning = value; }
		}

		public int Count 
		{
			get { lock (locker) { return simpleStore.Count; } } 
		}

		private SimpleStoreActual simpleStore = new SimpleStoreActual();

		public IEnumerable<string> Keys
		{
			get { lock (locker) { return simpleStore.Keys; } }
		}

		/// <exception cref="Sysem.IOException"></exception>
		public static BitmapSearcherStore Load(string path)
		{
			var store = new BitmapSearcherStore();
			store.simpleStore = SimpleStoreActual.Load(path);
			return store;
		}

		/// <exception cref="Sysem.IOException"></exception>
		public void Save(string path)
		{
			lock (locker)
			{
				simpleStore.Save(path);
			}
		}

		/// <exception cref="System.Collections.Generic.KeyNotFoundException"></exception>
		/// <exception cref="System.ArgumentNullException"></exception>
		public void Remove(string id)
		{
			lock (locker)
			{
				if (!simpleStore.Remove(id))
					throw new KeyNotFoundException("id");
			}
		}

		/// <exception cref="System.Collections.Generic.KeyNotFoundException"></exception>
		/// <exception cref="System.ArgumentNullException"></exception>
		public Bitmap GetTemplate(string id)
		{
			lock (locker)
			{
				return simpleStore[id].GetLastVersion().GetTemplate();
			}
		}

		/// <exception cref="System.Collections.Generic.KeyNotFoundException"></exception>
		/// <exception cref="System.ArgumentNullException"></exception>
		public SimpleColor GetTemplateSettings(string id)
		{
			if (id == null)
				throw new ArgumentNullException("id");
			lock (locker)
			{
				return simpleStore[id].GetLastVersion().GetAdmissibleDifference();
			}
		}

		/// <exception cref="System.ArgumentException"></exception>
		/// <exception cref="System.ArgumentNullException"></exception>
		public void Add(string id, Bitmap template)
		{
			if (id == null)
				throw new ArgumentNullException("id");
			if (template == null)
				throw new ArgumentNullException("template");
			lock (locker)
			{
				if (simpleStore.ContainsKey(id))
					throw new ArgumentException("id");
				var searcher = new BitmapSearcher(template, SimpleColor.FromRgb(0, 0, 0));
				simpleStore[id] = new ConcurrentSearcher(searcher);
			}
		}

		private void ThrowIfIdDoesntExist(string id)
		{
			lock (locker)
			{
				if (!simpleStore.Keys.Contains(id))
					throw new KeyNotFoundException("Object with this id doesn't exist");
			}
		}

		/// <summary>
		/// Адаптирует настройки поисковика при помощи изображения, содержащего count элементов
		/// </summary>
		public async Task UpgradeAsync(string id, Bitmap image, int count)
		{
			ConcurrentSearcher cs;
			lock (locker)
			{
				ThrowIfIdDoesntExist(id);
				cs = simpleStore[id];
			}

			Interlocked.Increment(ref tasksRunning);
			await cs.LearnAsync(image, count);
			Interlocked.Decrement(ref tasksRunning);
		}

		public List<Point> GetPositions(string id, Bitmap image, bool noOverlapping = false)
		{
			List<Point> positions;
			Size templateSize;
			lock (locker)
			{
				ThrowIfIdDoesntExist(id);
				positions = simpleStore[id].GetLastVersion().GetPositions(image).ToList();
				templateSize = simpleStore[id].GetLastVersion().TemplateSize;
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
				for (int j = 0; j < i; ++j)
					needRemove[i] = AreOverlapped(positions[j], positions[i], templateSize);

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
