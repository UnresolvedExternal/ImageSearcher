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
	/// 
	/// </summary>
	[Serializable]
    internal sealed class SimpleStore: 
		Dictionary<string, BitmapSearcher>
    {
		
		private static DataContractJsonSerializer formatter =
			new DataContractJsonSerializer(typeof(SimpleStore));
		 
		/*
		private static BinaryFormatter formatter = new BinaryFormatter(null, 
			new System.Runtime.Serialization.StreamingContext(System.Runtime.Serialization.StreamingContextStates.File));
		*/
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
			catch (System.Runtime.Serialization.SerializationException e)
			{
				throw GetCorruptedFileException(e);
			}
			catch (System.IO.IOException e)
			{
				throw GetCorruptedFileException(e);
			}
			catch (InvalidDataException e)
			{
				throw GetCorruptedFileException(e);
			}
		}

		private static IOException GetCorruptedFileException(Exception inner)
		{
			if (inner.GetType() == typeof(System.IO.FileNotFoundException))
				return new System.IO.FileNotFoundException("File not found");
			return new System.IO.FileLoadException("The file is corrupted", inner);
		}

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
