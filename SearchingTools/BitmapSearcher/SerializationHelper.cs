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

		public static void Serialize(BitmapSearcher obj, Stream output)
		{
			using (var memory = new MemoryStream())
			{
				formatter.WriteObject(memory, obj);
				memory.WriteTo(output);
			}
		}

		/// <summary>
		/// Десериализует объект. Восстанавливает поток в случае ошибки.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
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
