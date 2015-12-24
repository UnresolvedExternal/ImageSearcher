using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;
using System.IO.Compression;

namespace SearchingTools
{
	/// <summary>
	/// Хранилище пар: ИД шаблона, поисковик
	/// </summary>
    public sealed class BitmapSearcherStore: 
		Dictionary<string, BitmapSearcher>
    {
		private static DataContractJsonSerializer formatter =
			new DataContractJsonSerializer(typeof(BitmapSearcherStore));

		public static BitmapSearcherStore Load(string filename)
		{
			BitmapSearcherStore store;
			using (var fs = new FileStream(filename, FileMode.Open))
			using (var zip = new GZipStream(fs, CompressionMode.Decompress, true))
				store = (BitmapSearcherStore)formatter.ReadObject(zip);
			return store;
		}

		public void Save(string filename)
		{
			using (var fs = new FileStream(filename, FileMode.Create))
			using (var zip = new GZipStream(fs, CompressionLevel.Optimal, true))
				formatter.WriteObject(zip, this);
		}
    }
}
