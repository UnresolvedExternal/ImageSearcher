using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using System.Runtime.Serialization.Json;

namespace WrappedSearcher
{
	/// <summary>
	/// При неудачной сериализации восстанавливает состояние потока
	/// </summary>
	internal static class SerializationHelper
	{
		private static DataContractJsonSerializer formatter =
			new DataContractJsonSerializer(typeof(WrappedSearcher));

		public static void Serialize(WrappedSearcher obj, Stream output)
		{
			var memory = new MemoryStream();
			try 
			{
				formatter.WriteObject(memory, obj);
				memory.WriteTo(output);
			} 
			catch
			{
				throw;
			}
		}

		public static WrappedSearcher Deserialize(Stream input)
		{
			var oldPosition = input.Position;
			WrappedSearcher result;
			try
			{
				result = (WrappedSearcher)formatter.ReadObject(input);
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
