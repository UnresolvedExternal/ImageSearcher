using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Json;
using System.Text;

namespace SearchingTools
{
	/// <summary>
	/// При неудачной сериализации восстанавливает состояние потока
	/// </summary>
	internal static class SerializationHelper
	{
		private static DataContractJsonSerializer formatter =
			new DataContractJsonSerializer(typeof(BitmapSearcher));

		public static void Serialize(BitmapSearcher obj, Stream output)
		{
			using (var memory = new MemoryStream())
			{
				formatter.WriteObject(memory, obj);
				var bytes = memory.GetBuffer();
				var size = memory.Length;
				using (var zip = new GZipStream(output, CompressionLevel.Fastest, true))
				{
					zip.Write(bytes, 0, (int)size);
				}
			}
		}

		public static BitmapSearcher Deserialize(Stream input)
		{
			var oldPosition = input.Position;
			BitmapSearcher result;
			try
			{
				using (var zip = new GZipStream(input, CompressionMode.Decompress, true))
					result = (BitmapSearcher)formatter.ReadObject(new BufferedStream(zip));
			}
			catch
			{
				input.Position = oldPosition;
				throw;
			}
			return result;
		}
	}
}
