using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SearchingTools
{
	/// <summary>
	/// Потокобезопасная оболочка для BitmapSearcher
	/// </summary>
	[DataContract]
	internal class ConcurrentSearcher
	{
		[DataMember]
		BitmapSearcher _lastVersion;
		[DataMember]
		object _locker;
		[DataMember]
		Task _task;

		public ConcurrentSearcher(BitmapSearcher searcher)
		{
			_locker = new object();
			_lastVersion = searcher;
			InitializeEmptyTask();
		}

		private void InitializeEmptyTask()
		{
			var tcs = new TaskCompletionSource<object>();
			tcs.SetResult(null);
			_task = tcs.Task;
		}

		/// <summary>
		/// Возращает копию последнего зафиксированного состояния экземпляра BitmapSearcher
		/// </summary>
		public BitmapSearcher GetLastVersion()
		{
			lock (_locker)
				return _lastVersion.Clone();
		}

		/// <summary>
		/// Вызывает асинхронно метод Learn подконтрольного объекта
		/// </summary>
		public async Task LearnAsync(Bitmap image, int count)
		{
			Task task;
			lock (_locker)
				task = _task = _task.ContinueWith(t => Learn(image, count));
			await task;
		}

		private void Learn(Bitmap image, int count)
		{
			BitmapSearcher copie = GetLastVersion();
			copie.Learn(image, count);
			lock (_locker)
				_lastVersion.UniteWith(copie);
		}

		/// <summary>
		/// Блокирует вызывающий поток до завершения всех операций
		/// </summary>
		public void Join()
		{
			bool isCompleted = false;
			while (!isCompleted)
			{
				Task task;
				lock (_locker)
					task = _task;
				task.Wait();
				isCompleted = task.IsCompleted;
			}
		}
	}
}
