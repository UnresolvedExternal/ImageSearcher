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
		const int arraySize = 256;

		double[] damage = new double[arraySize];
		double[] heal = new double[arraySize];

		double secondaryBouncesMultiplier = 2 / 3.0;

		// В целях оптимизации храним наборы точек каждого ряда
		Point[][] rowsPoints;
		Point[][] columnsPoints;
		SpinLock locker = new SpinLock();

		private void InitializeRowsAndColumnsPoints(int width, int height)
		{
			rowsPoints = new Point[height + 1][];
			for (int y = 1; y <= height; ++y)
			{
				rowsPoints[y] = new Point[width];
				for (int x = 0; x < width; ++x)
				{
					rowsPoints[y][x] = new Point(x + 1, y);
				}
			}

			columnsPoints = new Point[width + 1][];
			for (int x = 1; x <= width; ++x)
			{
				columnsPoints[x] = new Point[height];
				for (int y = 0; y < height; ++y)
				{
					columnsPoints[x][y] = new Point(x, y + 1);
				}
			}
		}

		public ClassicRules()
		{
			damage[GetIndex(ElementType.Red)] = 5;
			damage[GetIndex(ElementType.Green)] = 3;
			damage[GetIndex(ElementType.Blue)] = 4;
			damage[GetIndex(ElementType.Purple)] = 1;
			damage[GetIndex(ElementType.Yellow)] = 2;
			heal[GetIndex(ElementType.Heart)] = 4;
		}
		
		/// <summary>
		/// </summary>
		/// <param name="e"></param>
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
		/// <param name="e"></param>
		/// <returns></returns>
		private static bool IsExplosiveInLine(ElementType e)
		{
			return IsOrdinary(e);
		}

		/// <summary>
		/// Проверяет корректен ли ход с обменом элементов
		/// </summary>
		/// <param name="first"></param>
		/// <param name="second"></param>
		/// <param name="board"></param>
		/// <returns></returns>
		private bool IsCorrectMovement(Point first, Point second, SimpleBoard board)
		{
			Contract.Requires(AreNear(first, second));

			if (!IsManuallyMovable(board.Cells[first.X, first.Y]) ||
				!IsManuallyMovable(board.Cells[second.X, second.Y]))
			{
				return false;
			}

			Swap(first, second, board);
			bool result =
				IsExplosiveRow(first.Y, board) ||
				IsExplosiveRow(second.Y, board) ||
				IsExplosiveColumn(first.X, board) ||
				IsExplosiveColumn(second.X, board);
			Swap(first, second, board);

			return result;
		}

		[Pure]
		private static bool InRange(int value, int a, int b)
		{
			return (value >= a && value <= b) || (value >= b && value <= a);
		} 

		/// <summary>
		/// Проверяет, есть ли в строке ряд, который должен быть разрушен
		/// </summary>
		/// <param name="y"></param>
		/// <param name="board"></param>
		/// <returns></returns>
		private bool IsExplosiveRow(int y, SimpleBoard board)
		{
			return IsExplosiveLine(new Point(1, y), new Point(1, 0), board);
		}

		/// <summary>
		/// Проверяет, нужно ли производить взрывы на доске в соответствии с правилами игры
		/// </summary>
		/// <param name="board"></param>
		/// <returns></returns>
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
		/// Проверяет, есть ли в столбце ряд, который должен быть разрушен
		/// </summary>
		/// <param name="x"></param>
		/// <param name="board"></param>
		/// <returns></returns>
		private bool IsExplosiveColumn(int x, SimpleBoard board)
		{
			return IsExplosiveLine(new Point(x, 1), new Point(0, 1), board);
		}

		/// <summary>
		/// Возвращает все точки строки
		/// </summary>
		/// <param name="start"></param>
		/// <param name="shift"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Point[] GetRowPoints(int y)
		{
			return rowsPoints[y];
		}

		/// <summary>
		/// Возвращает все точки столбца
		/// </summary>
		/// <param name="start"></param>
		/// <param name="shift"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Point[] GetColumnPoints(int x)
		{
			return columnsPoints[x];
		}

		/// <summary>
		/// Проверяет, есть ли в линии ряд, который должен быть разрушен
		/// </summary>
		/// <param name="start">Первая точка линни</param>
		/// <param name="shift">Смещение, определяющее направление линии</param>
		/// <param name="board"></param>
		/// <returns></returns>
		private bool IsExplosiveLine(Point start, Point shift, SimpleBoard board)
		{
			int sequence = 1;
			var points = (shift.X == 1) ? GetRowPoints(start.Y) : GetColumnPoints(start.X);

			var prevPoint = points[0];
			for (int i = 1; i < points.Length; ++i)
			{
				var currPoint = points[i];
				var currCell = board.Cells[currPoint.X, currPoint.Y];

				if (currCell == board.Cells[prevPoint.X, prevPoint.Y] &&
					IsExplosiveInLine(currCell))
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
		/// <param name="first"></param>
		/// <param name="second"></param>
		/// <param name="board"></param>
		private static void Swap(Point first, Point second, SimpleBoard board)
		{
			ElementType temp = board.Cells[first.X, first.Y];
			board.Cells[first.X, first.Y] = board.Cells[second.X, second.Y];
			board.Cells[second.X, second.Y] = temp;
		}

		/// <summary>
		/// Определяет, смежны ли клетки по стороне
		/// </summary>
		/// <param name="first"></param>
		/// <param name="second"></param>
		/// <returns></returns>
		[Pure]
		private static bool AreNear(Point first, Point second)
		{
			int dx = second.X - first.X;
			int dy = second.Y - first.Y;
			return (Math.Abs(dx) == 1 && dy == 0) || (Math.Abs(dy) == 1 && dx == 0);
		}

		/// <summary>
		/// Получает все возможные состояния игры, получаемые из
		/// исходного состояния за один ход
		/// </summary>
		/// <param name="source">Исходное состояние</param>
		/// <returns>Список возможных состояний</returns>
		public Dictionary<Movement, ClassicGameState> GetAllMovements(ClassicGameState source)
		{
			var result = new Dictionary<Movement, ClassicGameState>();

			bool lockTaken = false;
			locker.Enter(ref lockTaken);
			if (lockTaken == false)
				throw new SynchronizationLockException("");

			InitializeRowsAndColumnsPoints(source.Board.Width, source.Board.Height);

			locker.Exit(true);

			if (source.GameOver())
			{
				return result;
			}

			AddSwaps(result, source);
			if (result.Count == 0)
				return result; // некорректное состояние игры

			if (source.CurrentPlayer.Bombs > 0)
				AddExplosiveItemUsages(result, source, GetBombShape);

			if (source.CurrentPlayer.Dynamits > 0)
				AddExplosiveItemUsages(result, source, GetDynamitShape);

			AddEmptyMovement(result, source);
			return result;
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

		private static IEnumerable<Point> GetDynamitShape(Point center)
		{
			for (int dx = -1; dx <= 1; ++dx)
				for (int dy = -1; dy <= 1; ++dy)
					yield return new Point(center.X + dx, center.Y + dy);
		}

		/// <summary>
		/// Добавляет в словарь ходов, все возможные ходы с использованием взрывчатки
		/// </summary>
		/// <param name="dest"></param>
		/// <param name="state"></param>
		private void AddExplosiveItemUsages(Dictionary<Movement, ClassicGameState> dest, ClassicGameState state,
			Func<Point, IEnumerable<Point>> explosiveItemGetShape)
		{
			ClassicMovementKind current = (explosiveItemGetShape == GetBombShape) ?
				ClassicMovementKind.Bomb : ClassicMovementKind.Dynamit;

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

		private ClassicGameState GetStateAfterExplosition(IEnumerable<Point> explositionShape, ClassicGameState state, bool isBomb)
		{
			double heal;
			double damage;
			int addBombs = isBomb ? -1 : 0;
			int addDynamits = isBomb ? 0 : -1;
			
			var result = state.Clone();

			ExplodeOnce(explositionShape, result.Board, out heal, out damage);
			
			// TODO: вынести информацию о доп. уроне в класс Player
			damage += isBomb ? +2 : +3;
			
			addBombs = Math.Min(addBombs, 1);
			addDynamits = Math.Min(addDynamits, 1);

			ChangeGameState(result, heal, damage, addBombs, addDynamits);
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
			foreach (var movement in GetAllUncheckedSwaps(state.Board))
			{
				var classicMovement = (ClassicMovement)movement;
				Point first = classicMovement.First;
				Point second = classicMovement.Second;

				if (IsCorrectMovement(first, second, state.Board))
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
		/// <param name="board"></param>
		/// <returns></returns>
		private static IEnumerable<Movement> GetAllUncheckedSwaps(SimpleBoard board)
		{
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
						yield return movement;
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
						yield return movement;
					}
				}
		}

		/// <summary>
		/// Уничтожает элементы в заданных позициях и производит смещение верхних элементов вниз
		/// </summary>
		/// <param name="positions">Позиции уничтожаемых элементов</param>
		/// <param name="board">Доска с элементами</param>
		/// <param name="rawHeal">Получаемый отхил</param>
		/// <param name="rawDamage">Нанесённый урон</param>
		/// <returns>Количество уничтоженных Diamond</returns>
		private int ExplodeOnce(IEnumerable<Point> positions, SimpleBoard board, 
			out double rawHeal, out double rawDamage)
		{
			rawHeal = 0;
			rawDamage = 0;

			foreach (var pos in positions.OrderBy(p => p.Y).Where(p => InRange(p.X, 1, board.Width) && InRange(p.Y, 1, board.Height)))
			{
				var curr = board.Cells[pos.X, pos.Y];
				if (curr != ElementType.Diamond)
				{
					rawHeal += GetHeal(curr);
					rawDamage += GetDamage(curr);

					for (int y = pos.Y; y >= 1; --y)
						board.Cells[pos.X, y] = board.Cells[pos.X, y - 1];
				}
			}

			return DestroyDiamondsAtBottom(board);
		}

		/// <summary>
		/// Уничтожает Diamonds на нижнем уровне доски. Производит смещение элементов при уничтожении.
		/// </summary>
		/// <param name="board"></param>
		/// <returns>Количество уничтоженных алмазов</returns>
		private static int DestroyDiamondsAtBottom(SimpleBoard board)
		{
			int diamonds = 0;
			for (int x = 1; x <= board.Width; ++x)
				if (board.Cells[x, board.Height] == ElementType.Diamond)
				{
					++diamonds;
					DestroySinglePoint(new Point(x, board.Height), board);
				}
			return diamonds;
		}

		/// <summary>
		/// Уничтожение элемента и смещение элементов над ним вниз.
		/// </summary>
		/// <param name="point"></param>
		/// <param name="board"></param>
		private static void DestroySinglePoint(Point point, SimpleBoard board)
		{
			for (int y = point.Y; y >= 1; --y)
				board.Cells[point.X, y] = board.Cells[point.X, y - 1];
		}

		/// <summary>
		/// Получает новое состояние игры при помощи обмена элементов и последующими взрывами
		/// </summary>
		/// <param name="first"></param>
		/// <param name="second"></param>
		/// <param name="source"></param>
		/// <returns></returns>
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

			var result = source.Clone();

			// Первая серия взрывов
			ExplodeOnce(result.Board, out heal, out damage, out bombs, out dynamits);
			ChangeGameState(result, heal, damage, bombs, dynamits);

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

			while (IsDetonationRequired(result.Board))
			{
				double newHeal;
				double newDamage;
				int newBombs;
				int newDynamits;

				ExplodeOnce(result.Board, out newHeal, out newDamage, out newBombs, out newDynamits);

				heal += newHeal * secondaryBouncesMultiplier;
				damage += newDamage * secondaryBouncesMultiplier;
				bombs += newBombs;
				dynamits += newDynamits;
			}

			ChangeGameState(result, heal, damage, bombs, dynamits);
			result.SwapPlayers();
		}

		private static void ChangeGameState(ClassicGameState state, double heal, double damage, int bombs, int dynamits)
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
			out int bombsAcquired, out int dynamitsAcquired)
		{
			var explosingPositions = GetAllPointsToExplode(board, out bombsAcquired, out dynamitsAcquired);
			//CalculateRawHealAndDamage(board, explosingPositions, out rawHeal, out rawDamage);
			ExplodeOnce(explosingPositions, board, out rawHeal, out rawDamage);
		}

		/// <summary>
		/// Находит все точки на линии, заданной начальной точкой и смещением, которые
		/// подлежат уничтожению
		/// </summary>
		/// <param name="start"></param>
		/// <param name="shift"></param>
		/// <param name="board"></param>
		/// <param name="bombsAcquired">Сюда добавляются приобретённые бомбы</param>
		/// <param name="dynamitsAcquired">Сюда добавляются приобретённые динамиты</param>
		/// <returns></returns>
		private IList<Point> GetLinePointsToExplode(Point start, Point shift, SimpleBoard board,
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
					board.Cells[currPoint.X, currPoint.Y] == board.Cells[prevPoint.X, prevPoint.Y])
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
					bombsAcquired += (curr == 4) ? 1 : 0;
					curr = 1;
				}
			}

			return result;
		}

		/// <summary>
		/// Находит позиции всех элементов, которые должны быть уничтожены по правилам игры.
		/// Заоодно вычисляет количество приобретенной взрывчатки.
		/// </summary>
		/// <param name="board"></param>
		/// <returns></returns>
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
				result = result.Concat(GetLinePointsToExplode(start, shift, board, ref bombsAcquired, ref dynamitsAcquired)).ToList();
			}

			for (int y = 1; y <= board.Height; ++y)
			{
				var start = new Point(1, y);
				var shift = new Point(1, 0);
				result = result.Concat(GetLinePointsToExplode(start, shift, board, ref bombsAcquired, ref dynamitsAcquired)).ToList();
			}

			result = result.Distinct().ToList();
			return result;
		}

		/// <summary>
		/// Подсчитывает значения суммарного лечения и повреждения взрываемых элементов
		/// </summary>
		/// <param name="board"></param>
		/// <param name="explosingPositions"></param>
		/// <param name="rawHeal"></param>
		/// <param name="rawDamage"></param>
		private void CalculateRawHealAndDamage(SimpleBoard board, IEnumerable<Point> explosingPositions,
			out double rawHeal, out double rawDamage)
		{
			rawHeal = 0;
			rawDamage = 0;

			foreach (var p in explosingPositions)
			{
				rawHeal += GetHeal(board.Cells[p.X, p.Y]);
				rawDamage += GetDamage(board.Cells[p.X, p.Y]);
			}
		}
	}
}
