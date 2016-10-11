using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodsGameApi
{
	/// <summary>
	/// Прямоугольная игровая доска.
	/// Индексация [x, y], где Width >= x >= 1; Height >= y >= 1 - для реальных элементов
	/// граничные значения x, y - для служебных элементов
	/// </summary>
	[Serializable]
	public class SimpleBoard
	{
		public ElementType[,] Cells;

		private SimpleBoard()
		{
		}

		public SimpleBoard(int width, int height)
		{
			Cells = new ElementType[width + 2, height + 2];
			for (int x = 0; x < width + 2; ++x)
				for (int y = 0; y < height + 2; ++y)
					Cells[x, y] = ElementType.Empty;
		}

		public SimpleBoard Clone()
		{
			var newBoard = new SimpleBoard();
			newBoard.Cells = Clone(Cells);
			return newBoard;
		}

		internal static T[,] Clone<T>(T[,] board)
		{
			int width = board.GetLength(0);
			int height = board.GetLength(1);
			var newBoard = new T[width, height];
			for (int x = 0; x < width; ++x)
				for (int y = 0; y < height; ++y)
					newBoard[x, y] = board[x, y];
			return newBoard;
		}

		public int Width { get { return Cells.GetLength(0) - 2; } }
		public int Height { get { return Cells.GetLength(1) - 2; } }

		public static bool operator ==(SimpleBoard left, SimpleBoard right)
		{
			if (object.ReferenceEquals(left, right))
				return true;

			if (object.ReferenceEquals(left, null) || object.ReferenceEquals(right, null))
				return false;

			if (left.Width != right.Width || left.Height != right.Height)
				return false;

			for (int x = 0; x <= left.Width + 1; ++x)
				for (int y = left.Height + 1; y >= 0; --y)
					if (left.Cells[x, y] != right.Cells[x, y])
						return false;
			return true;
		}

		public override bool Equals(object obj)
		{
			var other = obj as SimpleBoard;
			return (!object.ReferenceEquals(other, null) && this == other);
		}

		public override int GetHashCode()
		{
			return this.Width;
		}

		public static bool operator !=(SimpleBoard left, SimpleBoard right)
		{
			return !(left == right);
		}

		public ElementType this[Point position]
		{
			get
			{
				return Cells[position.X, position.Y];
			}

			set
			{
				Cells[position.X, position.Y] = value;
			}
		}

		public ElementType this[int x, int y]
		{
			get
			{
				return this[new Point(x, y)];
			}

			set
			{
				this[new Point(x, y)] = value;
			}
		}
	}
}
