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
	/// Эта версия класса не должна меняться в целях совместимости со старыми сохранениями
	/// </summary>
	[Serializable]
    internal sealed class SimpleStore: 
		Dictionary<string, BitmapSearcher>
    {
		
		private static DataContractJsonSerializer formatter =
			new DataContractJsonSerializer(typeof(SimpleStore));
		 
		public static SimpleStore Load(string filename)
		{
			SimpleStore store;

			try
			{
				using (var fs = new FileStream(filename, FileMode.Open))
				using (var zip = new GZipStream(fs, CompressionMode.Decompress, true))
					store = (SimpleStore)formatter.ReadObject(zip);
				return store;
			} 
			catch (Exception e)
			{
				throw GetCorruptedFileException(e);
			}
		}

		private static IOException GetCorruptedFileException(Exception inner)
		{
			if (inner.GetType() == typeof(System.IO.FileNotFoundException))
				return new System.IO.FileNotFoundException("File not found", inner);
			return new System.IO.FileLoadException("The file is corrupted", inner);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public void Save(string filename)
		{
			try
			{
				using (var fs = new FileStream(filename, FileMode.Create))
				using (var zip = new GZipStream(fs, CompressionMode.Compress))
					formatter.WriteObject(zip, this);
			}
			catch (System.Runtime.Serialization.SerializationException e)
			{
				throw new System.IO.IOException("Cant save to the file", e);
			}
		}
    }
}
