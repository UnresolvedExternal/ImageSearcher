using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using System.IO;
using System.Drawing;
using System.Text.RegularExpressions;

namespace GodsGameApi
{
	static class TestHelper
	{
		/// <summary>
		/// Проверяет совпадение типов и совпадение сериализованных представлений объектов
		/// </summary>
		public static void AssertEqual(object a, object b)
		{
			Assert.That(a, Is.TypeOf(b.GetType()));

			var serializer = new BinaryFormatter();
			var memory = new MemoryStream();
			
			serializer.Serialize(memory, a);
			var aBytes = memory.GetBuffer();
			
			memory.Position = 0;
			serializer.Serialize(memory, b);
			var bBytes = memory.GetBuffer();

			Assert.That(aBytes, Is.EqualTo(bBytes));
		}

		/// <summary>
		/// Создаёт игровое состояние в соответствии с параметрами
		/// </summary>
		public static ClassicGameState CreateState(string cells,
			int playerHp, int enemyHp,
			int playerBombs, int enemyBombs,
			int playerDynamits, int enemyDynamits)
		{
			var board = TestHelper.CreateSimpleBoard(cells);
			var dest = new ClassicGameState(board.Width, board.Height);
			dest.Board = board;

			dest.CurrentPlayer.Hp.Current = playerHp;
			dest.CurrentPlayer.Bombs = playerBombs;
			dest.CurrentPlayer.Dynamits = playerDynamits;

			dest.AnotherPlayer.Hp.Current = enemyHp;
			dest.AnotherPlayer.Bombs = enemyBombs;
			dest.AnotherPlayer.Dynamits = enemyDynamits;

			return dest;
		}

		public static ClassicMovement CreateMovementSwap(int x1, int y1, int x2, int y2)
		{
			return new ClassicMovement
			{
				First = new Point(x1, y1),
				Second = new Point(x2, y2),
				Kind = ClassicMovementKind.Swap
			};
		}

		public static ClassicMovement CreateMovementBomb(int x, int y)
		{
			return new ClassicMovement
			{
				First = new Point(x, y),
				Kind = ClassicMovementKind.Bomb
			};
		}

		public static ClassicMovement CreateMovementDynamit(int x, int y)
		{
			return new ClassicMovement
			{
				First = new Point(x, y),
				Kind = ClassicMovementKind.Dynamit
			};
		}

		public static ClassicMovement CreateMovementEmpty()
		{
			return new ClassicMovement
			{
				Kind = ClassicMovementKind.Empty
			};
		}

		public static Dictionary<Movement, ClassicGameState> CreateDictionary(
			IList<Movement> movements, IList<ClassicGameState> states)
		{
			var result = new Dictionary<Movement, ClassicGameState>();

			for (int k = 0; k < movements.Count; ++k)
				result.Add(movements[k], states[k]);

			return result;
		}

		/// <summary>
		/// Создает доску из многострочного текста.
		/// Строки должны содержать символы RGBYPHDEU в соответствии с перечисление ElementType
		/// </summary>
		public static SimpleBoard CreateSimpleBoard(string cells)
		{
			cells = cells.Trim();
			cells = Regex.Replace(cells, @"\r\n", "\n", RegexOptions.Singleline);

			int width = cells.IndexOf('\n');
			if (width == -1)
				width = 1;
			int height = cells.Count(ch => ch == '\n') + 1;
			
			var result = new SimpleBoard(width, height);
			
			for (int y = 0; y <= height + 1; ++y)
				for (int x = 0; x <= width + 1; ++x)
				{
					if (x == 0 || y == 0 || x == width + 1 || y == height + 1)
						result[x, y] = ElementType.Unknown;
					else
					{
						string name = Enum.GetNames(typeof(ElementType))
							.First(s => cells[(y - 1)*(width + 1) + x - 1] == s[0]);
						result[x, y] = (ElementType)Enum.Parse(typeof(ElementType), name);
					}
				}

			return result;
		}
	}
}
