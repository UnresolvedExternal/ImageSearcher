using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GodsGameApi
{
	/// <summary>
	/// Инкапсулирует правила классической дуэли
	/// </summary>
	public class ClassicRules: IRules
	{
		#region Damage and heal values accessors

		static int _arraySize = typeof(ElementType).GetEnumValues().Length;

		double[] damage = new double[_arraySize];
		double[] heal = new double[_arraySize];

		/// <returns>Индекс для массивов damage или heal</returns>
		private int GetIndex(ElementType e)
		{
			return (int)e;
		}

		private double GetDamage(ElementType e)
		{
			return damage[GetIndex(e)];
		}

		private double GetHeal(ElementType e)
		{
			return heal[GetIndex(e)];
		}

		#endregion

		double _secondaryBouncesMultiplier = 2 / 3.0;

		// В целях оптимизации храним наборы точек каждого ряда
		Point[][] _rowsPoints;
		Point[][] _columnsPoints;

		int _width;
		int _height;

		public ClassicRules(int width, int height)
		{
			_width = width;
			_height = height;
			InitializeRowsAndColumnsPoints(width, height);

			damage[GetIndex(ElementType.Red)] = 5;
			damage[GetIndex(ElementType.Green)] = 3;
			damage[GetIndex(ElementType.Blue)] = 4;
			damage[GetIndex(ElementType.Purple)] = 1;
			damage[GetIndex(ElementType.Yellow)] = 2;
			heal[GetIndex(ElementType.Heart)] = 4;
		}

		private void InitializeRowsAndColumnsPoints(int width, int height)
		{
			_rowsPoints = new Point[height + 1][];
			for (int y = 1; y <= height; ++y)
			{
				_rowsPoints[y] = new Point[width];
				for (int x = 0; x < width; ++x)
				{
					_rowsPoints[y][x] = new Point(x + 1, y);
				}
			}

			_columnsPoints = new Point[width + 1][];
			for (int x = 1; x <= width; ++x)
			{
				_columnsPoints[x] = new Point[height];
				for (int y = 0; y < height; ++y)
				{
					_columnsPoints[x][y] = new Point(x, y + 1);
				}
			}
		}

		private static bool IsOrdinary(ElementType e)
		{
			switch (e)
			{
				case ElementType.Red:
					return true;
				case ElementType.Green:
					return true;
				case ElementType.Blue:
					return true;
				case ElementType.Yellow:
					return true;
				case ElementType.Purple:
					return true;
				case ElementType.Heart:
					return true;
				default:
					return false;
			}
		}

		private static bool CanBeExplodedByBombOrDynamit(ElementType e)
		{
			return IsExplosiveInLine(e) || e == ElementType.Unknown;
		}

		private static bool IsManuallyMovable(ElementType e)
		{
			return IsOrdinary(e) || e == ElementType.Unknown;
		}

		/// <summary>
		/// Определяет, будет ли элемент разрушен, если будет в линии с такими же элементами
		/// </summary>
		private static bool IsExplosiveInLine(ElementType e)
		{
			return IsOrdinary(e);
		}

		/// <summary>
		/// Проверяет корректен ли ход с обменом элементов
		/// </summary>
		private bool IsSwapCorrect(Point first, Point second, SimpleBoard board)
		{
			if (!IsManuallyMovable(board.Cells[first.X, first.Y]) ||
				!IsManuallyMovable(board.Cells[second.X, second.Y]))
			{
				return false;
			}

			Swap(first, second, board);

			bool result;
			try
			{
				result =
					IsExplosiveRow(first.Y, board) ||
					(second.Y != first.Y) && IsExplosiveRow(second.Y, board) ||
					IsExplosiveColumn(first.X, board) ||
					(first.X != second.X) && IsExplosiveColumn(second.X, board);
			}
			finally
			{
				Swap(first, second, board);
			}
			return result;
		}

		/// <summary>
		/// Проверяет, есть ли в строке ряд, который должен быть разрушен
		/// </summary>
		private bool IsExplosiveRow(int y, SimpleBoard board)
		{
			return IsExplosiveLine(new Point(1, y), new Point(1, 0), board);
		}

		/// <summary>
		/// Проверяет, есть ли в столбце ряд, который должен быть разрушен
		/// </summary>
		private bool IsExplosiveColumn(int x, SimpleBoard board)
		{
			return IsExplosiveLine(new Point(x, 1), new Point(0, 1), board);
		}

		/// <summary>
		/// Проверяет, нужно ли производить взрывы на доске в соответствии с правилами игры
		/// </summary>
		private bool IsDetonationRequired(SimpleBoard board)
		{
			for (int x = 1; x <= board.Width; ++x)
				if (IsExplosiveColumn(x, board))
					return true;

			for (int y = 1; y <= board.Height; ++y)
				if (IsExplosiveRow(y, board))
					return true;

			return false;
		}

		/// <summary>
		/// Возвращает все точки строки
		/// </summary>
		private Point[] GetRowPoints(int y)
		{
			return _rowsPoints[y];
		}

		/// <summary>
		/// Возвращает все точки столбца
		/// </summary>
		private Point[] GetColumnPoints(int x)
		{
			return _columnsPoints[x];
		}

		/// <summary>
		/// Проверяет, есть ли в линии ряд, который должен быть разрушен
		/// </summary>
		/// <param name="start">Первая точка линни</param>
		/// <param name="shift">Смещение, определяющее направление линии: (0, 1) или (1, 0)</param>
		private bool IsExplosiveLine(Point start, Point shift, SimpleBoard board)
		{
			int sequence = 1;
			var points = (shift.X == 1) ? GetRowPoints(start.Y) : GetColumnPoints(start.X);

			var prevPoint = points[0];
			for (int i = 1; i < points.Length; ++i)
			{
				var currPoint = points[i];
				var currCell = board[currPoint];

				if (currCell == board[prevPoint] && IsExplosiveInLine(currCell))
				{
					++sequence;
					if (sequence >= 3)
						return true;
				}
				else
				{
					sequence = 1;
				}
				prevPoint = currPoint;
			}
			return false;
		}

		/// <summary>
		/// Меняет элементы в соответствующих позициях местами
		/// </summary>
		private static void Swap(Point first, Point second, SimpleBoard board)
		{
			var temp = board[first];
			board[first] = board[second];
			board[second] = temp;
		}

		/// <summary>
		/// Получает все возможные состояния игры, получаемые из
		/// исходного состояния за один ход
		/// </summary>
		/// <param name="source">Исходное состояние</param>
		/// <returns>Словарь возможных состояний</returns>
		public Dictionary<Movement, ClassicGameState> GetAllMovements(ClassicGameState source)
		{
			Validate(source);
			var result = new Dictionary<Movement, ClassicGameState>();
			if (source.GameOver())
				return result;

			AddSwaps(result, source);
			if (result.Count == 0)
				return result; // некорректное состояние игры

			if (source.CurrentPlayer.Bombs > 0)
				AddExplosiveItemUsages(result, source, isBomb: true);

			if (source.CurrentPlayer.Dynamits > 0)
				AddExplosiveItemUsages(result, source, isBomb: false);

			AddEmptyMovement(result, source);

			return result;
		}

		private void Validate(ClassicGameState source)
		{
			if (source.Board.Width != _width || source.Board.Height != _height)
				throw new ArgumentException("Incorrect source size");
		}

		private void AddEmptyMovement(Dictionary<Movement, ClassicGameState> result, ClassicGameState source)
		{
			var movement = new ClassicMovement
				{
					Kind = ClassicMovementKind.Empty
				};

			var state = source.Clone();
			state.SwapPlayers();

			result.Add(movement, state);
		}

		private static Point[] GetBombShape(Point center)
		{
			var shape = new Point[5];
			shape[0] = center;
			shape[1] = new Point(center.X, center.Y - 1);
			shape[2] = new Point(center.X, center.Y + 1);
			shape[3] = new Point(center.X - 1, center.Y);
			shape[4] = new Point(center.X + 1, center.Y);
			return shape;
		}

		private static Point[] GetDynamitShape(Point center)
		{
			var shape = new Point[9];
			int count = 0;
			for (int dx = -1; dx <= 1; ++dx)
				for (int dy = -1; dy <= 1; ++dy)
					shape[count++] = new Point(center.X + dx, center.Y + dy);
			return shape;
		}

		/// <summary>
		/// Добавляет в словарь ходов, все возможные ходы с использованием взрывчатки
		/// </summary>
		private void AddExplosiveItemUsages(Dictionary<Movement, ClassicGameState> dest, ClassicGameState state,
			bool isBomb)
		{
			var current = isBomb ? ClassicMovementKind.Bomb : ClassicMovementKind.Dynamit;
			var explosiveItemGetShape = isBomb ? (Func<Point, Point[]>)GetBombShape : GetDynamitShape;

			for (int x = 1; x <= state.Board.Width; ++x)
				for (int y = 1; y <= state.Board.Height; ++y)
				{
					Point center = new Point(x, y);
					var shape = explosiveItemGetShape(center);
					ClassicMovement movement = new ClassicMovement
						{
							First = center,
							Kind =  current
						};
					dest.Add(movement, GetStateAfterExplosition(shape, state, current == ClassicMovementKind.Bomb));
				}
		}

		private ClassicGameState GetStateAfterExplosition(Point[] explositionShape, ClassicGameState state, bool isBomb)
		{
			double heal;
			double damage;
			int addBombs = isBomb ? -1 : 0;
			int addDynamits = isBomb ? 0 : -1;
			int addDiamonds;
			
			var result = state.Clone();

			ExplodeOnce(explositionShape, result.Board, out heal, out damage, out addDiamonds);
			
			// TODO: вынести информацию о доп. уроне в класс Player
			damage += isBomb ? +2 : +3;
			
			addBombs = Math.Min(addBombs, 1);
			addDynamits = Math.Min(addDynamits, 1);

			ChangeGameState(result, heal, damage, addBombs, addDynamits, addDiamonds);
			ExplodeSecondariesOnly(result);
			
			return result;
		}

		/// <summary>
		/// Добавляет в словарь ходов, все возможные ходы с обменом элементов
		/// </summary>
		/// <param name="dest"></param>
		/// <param name="state"></param>
		private void AddSwaps(Dictionary<Movement, ClassicGameState> dest, ClassicGameState state)
		{
			foreach (var movement in GetUncheckedSwaps(state.Board))
			{
				var classicMovement = (ClassicMovement)movement;
				Point first = classicMovement.First;
				Point second = classicMovement.Second;

				if (IsSwapCorrect(first, second, state.Board))
				{
					var newMovement = new ClassicMovement { First = first, Second = second };
					dest.Add(movement, GetStateAfterSwap(first, second, state));
				}
			}
		}

		/// <summary>
		/// Возвращает все возможные обмены соседних по стороне элементов доски.
		/// Правила игры не учитываются.
		/// </summary>
		private static List<Movement> GetUncheckedSwaps(SimpleBoard board)
		{
			var result = new List<Movement>();

			int width = board.Width;
			int height = board.Height;

			for (int x = 1; x <= width; ++x)
				for (int y = 1; y <= height; ++y)
				{
					var second = new Point(x, y);
					var first = new Point(x - 1, y);
					
					if (first.X >= 1)
					{
						ClassicMovement movement = new ClassicMovement
							{
								Kind = ClassicMovementKind.Swap,
								First = first,
								Second = second
							};
						result.Add(movement);
					}

					first = new Point(x, y - 1);

					if (first.Y >= 1)
					{
						ClassicMovement movement = new ClassicMovement
						{
							Kind = ClassicMovementKind.Swap,
							First = first,
							Second = second
						};
						result.Add(movement);
					}
				}

			return result;
		}

		private static bool InRange(int value, int a, int b)
		{
			return (value >= a && value <= b) || (value >= b && value <= a);
		}

		/// <summary>
		/// Уничтожает элементы в заданных позициях и производит смещение верхних элементов вниз
		/// </summary>
		/// <param name="positions">Позиции уничтожаемых элементов</param>
		/// <param name="board">Доска с элементами</param>
		/// <param name="rawHeal">Получаемый отхил</param>
		/// <param name="rawDamage">Нанесённый урон</param>
		/// <returns>Количество уничтоженных Diamond</returns>
		private void ExplodeOnce(Point[] positions, SimpleBoard board,
			out double rawHeal, out double rawDamage, out int diamondsDestroyed)
		{
			rawHeal = 0;
			rawDamage = 0;

			Array.Sort(positions, (a, b) => a.Y.CompareTo(b.Y));
			foreach (var pos in positions)
				if (InRange(pos.X, 1, board.Width) && InRange(pos.Y, 1, board.Height))
				{
					var curr = board[pos];
					if (curr != ElementType.Diamond)
					{
						rawHeal += GetHeal(curr);
						rawDamage += GetDamage(curr);

						for (int y = pos.Y; y >= 1; --y)
							board[pos.X, y] = board[pos.X, y - 1];
					}
				}

			diamondsDestroyed = DestroyDiamondsAtBottom(board);
		}

		/// <summary>
		/// Уничтожает Diamonds на нижнем уровне доски. Производит смещение элементов при уничтожении.
		/// </summary>
		/// <returns>Количество уничтоженных алмазов</returns>
		private static int DestroyDiamondsAtBottom(SimpleBoard board)
		{
			int diamonds = 0;
			for (int x = 1; x <= board.Width; ++x)
				if (board[x, board.Height] == ElementType.Diamond)
				{
					++diamonds;
					DestroySinglePoint(new Point(x, board.Height), board);
				}
			return diamonds;
		}

		/// <summary>
		/// Уничтожение элемента и смещение элементов над ним вниз.
		/// </summary>
		private static void DestroySinglePoint(Point point, SimpleBoard board)
		{
			for (int y = point.Y; y >= 1; --y)
				board[point.X, y] = board[point.X, y - 1];
		}

		/// <summary>
		/// Получает новое состояние игры при помощи обмена элементов и последующими взрывами
		/// </summary>
		private ClassicGameState GetStateAfterSwap(Point first, Point second, ClassicGameState source)
		{
			Swap(first, second, source.Board);
			var result = Explode(source);
			Swap(first, second, source.Board);
			
			return result;
		}

		/// <summary>
		/// Получает новое состояние игры, путём взрывов элементов в соответствии с правилами.
		/// Производит все серии взрывов. Смена хода также произодится.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		private ClassicGameState Explode(ClassicGameState source)
		{
			double heal;
			double damage;
			int bombs;
			int dynamits;
			int diamonds;

			var result = source.Clone();

			// Первая серия взрывов
			ExplodeOnce(result.Board, out heal, out damage, out bombs, out dynamits, out diamonds);
			ChangeGameState(result, heal, damage, bombs, dynamits, diamonds);

			ExplodeSecondariesOnly(result);

			return result;
		}

		/// <summary>
		/// Изменяет состояние игры, производя только вторичные взрывы в соответствии с правилами игры
		/// </summary>
		/// <param name="result"></param>
		private void ExplodeSecondariesOnly(ClassicGameState result)
		{
			double heal = 0;
			double damage = 0;
			int bombs = 0;
			int dynamits = 0;
			int diamonds = 0;

			while (IsDetonationRequired(result.Board))
			{
				double newHeal;
				double newDamage;
				int newBombs;
				int newDynamits;

				ExplodeOnce(result.Board, out newHeal, out newDamage, out newBombs, out newDynamits, out diamonds);

				heal += newHeal * _secondaryBouncesMultiplier;
				damage += newDamage * _secondaryBouncesMultiplier;
				bombs += newBombs;
				dynamits += newDynamits;
			}

			ChangeGameState(result, heal, damage, bombs, dynamits, diamonds);
			result.SwapPlayers();
		}

		private static void ChangeGameState(ClassicGameState state, double heal, double damage, int bombs, int dynamits, int diamonds)
		{
			state.CurrentPlayer.Hp.AddHealth((int)Math.Round(heal));
			state.AnotherPlayer.Hp.AddHealth(-(int)Math.Round(damage));
			state.CurrentPlayer.Bombs += bombs;
			state.CurrentPlayer.Dynamits += dynamits;
		}

		/// <summary>
		/// Взрывает все ряды из взрываемых элементов на доске. Производит только одну серию взрывов.
		/// </summary>
		/// <param name="board">Доска на которой произойдёт взрыв (по месту)</param>
		/// <param name="rawHeal">Полученное в результате здоровье</param>
		/// <param name="rawDamage">Нанесённый урон</param>
		/// <param name="bombsAcquired">Приобретено бомб</param>
		/// <param name="dynamitsAcquired">Приобретено динамитов</param>
		private void ExplodeOnce(SimpleBoard board, out double rawHeal, out double rawDamage, 
			out int bombsAcquired, out int dynamitsAcquired, out int diamondsDestroyed)
		{
			var explosingPositions = GetAllPointsToExplode(board, out bombsAcquired, out dynamitsAcquired);
			ExplodeOnce(explosingPositions.ToArray(), board, out rawHeal, out rawDamage, out diamondsDestroyed);
		}

		/// <summary>
		/// Находит все точки на линии, заданной начальной точкой и смещением, которые
		/// подлежат уничтожению
		/// </summary>
		/// <param name="bombsAcquired">Сюда добавляются приобретённые бомбы</param>
		/// <param name="dynamitsAcquired">Сюда добавляются приобретённые динамиты</param>
		private List<Point> GetLinePointsToExplode(Point start, Point shift, SimpleBoard board,
			ref int bombsAcquired, ref int dynamitsAcquired)
		{
			var result = new List<Point>();
			var positions = (shift.X == 1) ? GetRowPoints(start.Y) : GetColumnPoints(start.X);
			int curr = 1;

			for (int i = 1; i < positions.Length; ++i)
			{
				var currPoint = positions[i];
				var prevPoint = positions[i - 1];

				if (IsExplosiveInLine(board.Cells[currPoint.X, currPoint.Y]) && 
					board[currPoint] == board[prevPoint])
				{
					curr += 1;
					if (curr == 3)
						result.AddRange(new[] { currPoint, prevPoint, positions[i - 2] });
					if (curr >= 4)
						result.Add(currPoint);
				}
				else
				{
					dynamitsAcquired += (curr >= 5) ? 1 : 0;
					bombsAcquired += (curr == 4 || curr >= 6) ? 1 : 0;
					curr = 1;
				}
			}

			return result;
		}

		/// <summary>
		/// Находит позиции всех элементов, которые должны быть уничтожены по правилам игры.
		/// Заодно вычисляет количество приобретенной взрывчатки.
		/// </summary>
		private List<Point> GetAllPointsToExplode(SimpleBoard board, 
			out int bombsAcquired, out int dynamitsAcquired)
		{
			bombsAcquired = 0;
			dynamitsAcquired = 0;
			var result = new List<Point>();

			for (int x = 1; x <= board.Width; ++x)
			{
				var start = new Point(x, 1);
				var shift = new Point(0, 1);
				result.AddRange(GetLinePointsToExplode(start, shift, board, ref bombsAcquired, ref dynamitsAcquired));
			}

			for (int y = 1; y <= board.Height; ++y)
			{
				var start = new Point(1, y);
				var shift = new Point(1, 0);
				result.AddRange(GetLinePointsToExplode(start, shift, board, ref bombsAcquired, ref dynamitsAcquired));
			}

			result = result.Distinct().ToList();
			return result;
		}
	}
}
