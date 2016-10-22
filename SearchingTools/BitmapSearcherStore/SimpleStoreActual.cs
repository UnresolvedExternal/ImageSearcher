using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace SearchingTools
{
	/// <summary>
	/// Хранилище пар: ИД шаблона, поисковик
	/// </summary>
    internal sealed class SimpleStoreActual:
		Dictionary<string, ConcurrentSearcher>
    {
		public static explicit operator SimpleStoreActual(SimpleStore data)
		{
			var result = new SimpleStoreActual();
			foreach (var kv in data)
				result.Add(kv.Key, new ConcurrentSearcher(kv.Value));
			return result;
		}

		public static explicit operator SimpleStore(SimpleStoreActual data)
		{
			var result = new SimpleStore();
			foreach (var kv in data)
				result.Add(kv.Key, kv.Value.GetLastVersion());
			return result;
		}

		/// <exception cref="System.IOException"></exception>
		public void Save(string filename)
		{
			((SimpleStore)this).Save(filename);
		}

		/// <exception cref="System.IOException"></exception>
		public static SimpleStoreActual Load(string filename)
		{
			return (SimpleStoreActual)SimpleStore.Load(filename);
		}
    }
}
