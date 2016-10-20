﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchingTools
{
	/// <summary>
	/// Потокобезопасная оболочка для BitmapSearcher
	/// </summary>
	internal class ConcurrentSearcher
	{
		BitmapSearcher _lastVersion;
		object _locker;
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
		/// <returns></returns>
		public BitmapSearcher GetLastVersion()
		{
			lock (_locker)
				return _lastVersion.DeepClone();
		}

		/// <summary>
		/// Вызывает асинхронно метод Learn подконтрольного объекта
		/// </summary>
		/// <param name="image"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public async void LearnAsync(Bitmap image, int count)
		{
			Task task;
			lock (_locker)
				task = _task.ContinueWith(t => Learn(image, count));
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
		/// Блокирует вызывающий поток до завершения всех операций (запланированных к моменту вызова) 
		/// с подконтрольным объектом
		/// </summary>
		public void Join()
		{
			Task task;
			lock (_locker)
				task = _task;
			task.Wait();
		}
	}
}
