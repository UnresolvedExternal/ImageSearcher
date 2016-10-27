using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Json;
using System.Text;

namespace SearchingTools
{
	/// <summary>
	/// Сериализует объекты BitmapSearcher
	/// </summary>
	internal static class SerializationHelper
	{
		private static DataContractJsonSerializer formatter =
			new DataContractJsonSerializer(typeof(BitmapSearcher));

		/// <exception cref="System.Runtime.SerializationException"></exception>
		public static void Serialize(BitmapSearcher obj, Stream output)
		{
			try
			{
				using (var memory = new MemoryStream())
				{
					formatter.WriteObject(memory, obj);
					memory.WriteTo(output);
				}
			}
			catch (System.Exception e)
			{
				throw new System.Runtime.Serialization.SerializationException("Unknown", e);
			}
		}

		/// <summary>
		/// Десериализует объект. Восстанавливает поток в случае ошибки.
		/// </summary>
		/// <exception cref="System.Runtime.SerializationException"></exception>
		public static BitmapSearcher Deserialize(Stream input)
		{
			var oldPosition = input.Position;
			BitmapSearcher result;
			try
			{
				result = (BitmapSearcher)formatter.ReadObject(input);
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
