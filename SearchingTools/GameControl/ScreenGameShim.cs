using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GodsGameApi;
using MouseManipulator;
using SearchingTools;

namespace GameControl
{
	/// <summary>
	/// Инкапсулирует связь между клиентом игры и моделью игры
	/// </summary>
    public class ScreenGameShim
    {
		public const int BoardWidth = 9;
		public const int BoardHeight = 7;

		BitmapSearcherStore templatesStorage;

		Dictionary<Point, string> gameEntities;
		
		ClassicGameState gameState;

		Point leftTopBoardCell;
		int cellWidth;
		int cellHeight;

		public ScreenGameShim(string storeFilename)
		{
			templatesStorage = BitmapSearcherStore.Load(storeFilename);
		}

		Stopwatch start;

		/// <summary>
		/// Распознаёт открытый клиент игры по скриншоту.
		/// Анализирует состояние игры и делает ход при помощи мыши.
		/// </summary>
		/// <returns>Было ли состояние игры распознано как корректное</returns>
		public bool MakeMovement()
		{
			start = Stopwatch.StartNew();

			var screen = GetScreenshot();
			Logger.Log(screen);

			gameState = GetGameState(screen);
			if (gameState == null)
			{
				List<Point> closes = templatesStorage.GetPositions("Close", screen, true);
				if (closes.Count > 0)
				{
					Mouse.Move(closes[0]);
					Thread.Sleep(200);
					Mouse.LeftClick();
					Thread.Sleep(200);
					Mouse.Move(0, 0);
					return true;
				}
				else
				{
					closes = templatesStorage.GetPositions("RandomPlayerGame", screen, true);
					if (closes.Count > 0)
					{
						Mouse.Move(closes[0]);
						Thread.Sleep(200);
						Mouse.LeftClick();
						Thread.Sleep(200);
						Mouse.Move(0, 0);
						Thread.Sleep(5000);
						return true;
					}
				}
				return false;
			}

			var movement = gameState.Ai.GetGoodMovement(3);
			
			MakeMovement(movement);
			Logger.Log(movement);
			return true;
		}

		private Bitmap GetScreenshot()
		{
			var screen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
			var g = Graphics.FromImage(screen);
			g.CopyFromScreen(0, 0, 0, 0, screen.Size);
			return screen;
		}

		private IEnumerable<string> EnumerateInBoardEntities()
		{
			yield return "Red";
			yield return "Blue";
			yield return "Green";
			yield return "Yellow";
			yield return "Purple";
			yield return "Heart";
			yield return "Diamond";
		}

		private IEnumerable<string> EnumerateNotInBoardEntities()
		{
			yield return "Bomb";
			yield return "Dynamit";
			yield return "TurtleShell";
		}

		private ClassicGameState GetGameState(Bitmap screen)
		{
			gameEntities = GetGameEntities(screen);

			var inBoardEntities = gameEntities
				.Where(pair => EnumerateInBoardEntities()
				.Contains(pair.Value))
				.ToDictionary(pair => pair.Key, pair => pair.Value);

			if (inBoardEntities.Count != 63)
			{
				return null;
			}

			leftTopBoardCell = GetLeftTopBoardCell(inBoardEntities);
			cellWidth = GetMaxDistance(inBoardEntities.Keys, p => p.X) / (BoardWidth - 1);
			cellHeight = GetMaxDistance(inBoardEntities.Keys, p => p.Y) / (BoardHeight - 1);

			var simpleBoard = GetSimpleBoard(inBoardEntities);
			var gameState = new ClassicGameState();
			gameState.Board = simpleBoard;

			// Hp...
			var hps = templatesStorage.GetPositions("HpHeart", screen, true);
			if (hps.Count == 2)
			{
				var pos1 = hps[0];
				int width = templatesStorage.GetTemplate("HpHeart").Width;

				double hpLeft = 0;
				for (int i = 1; i <= 227; ++i)
				{
					var toCheck = pos1;
					toCheck.X += width + i;
					if (i > 180)
						toCheck.Y += 7;
					var pixel = screen.GetPixel(toCheck.X, toCheck.Y);
					if (Math.Max(pixel.R, pixel.G) > 180)
						hpLeft++;
				}

				gameState.CurrentPlayer.Hp.Current = (int)Math.Round(hpLeft / 227 * gameState.CurrentPlayer.Hp.Max);
				if (gameState.CurrentPlayer.Hp.Current == 0)
					gameState.CurrentPlayer.Hp.Current = 1;

				pos1 = hps[1];

				hpLeft = 0;
				for (int i = 1; i <= 227; ++i)
				{
					var toCheck = pos1;
					toCheck.X -= i;
					if (i > 180)
						toCheck.Y += 7;
					var pixel = screen.GetPixel(toCheck.X, toCheck.Y);
					if (Math.Max(pixel.R, pixel.G) > 180)
						hpLeft++;
				}

				gameState.AnotherPlayer.Hp.Current = (int)Math.Round(hpLeft / 227 * gameState.AnotherPlayer.Hp.Max);

				if (gameState.AnotherPlayer.Hp.Current == 0)
					gameState.AnotherPlayer.Hp.Current = 1;

				Console.WriteLine("Hps: {0} {1}", gameState.CurrentPlayer.Hp.Current, gameState.AnotherPlayer.Hp.Current);
			}

			/*
			gameState.CurrentPlayer.Hp.Current = 50;
			gameState.AnotherPlayer.Hp.Current = 50;

			 */
			foreach (var entity in EnumerateNotInBoardEntities())
			{
				int countLeft = GetNotInBoardEntitiesCount(entity, true);
				int countRight = GetNotInBoardEntitiesCount(entity, false);
				var name = entity + "s";

				if (entity == "TurtleShell")
				{
					if (countRight > 0)
						gameState.CurrentPlayer.Bombs -= 10;
				}
				else
				{ 
				
					gameState.CurrentPlayer.GetType()
						.GetProperty(entity + "s").SetValue(gameState.CurrentPlayer, countLeft);


					gameState.AnotherPlayer.GetType()
						.GetProperty(entity + "s").SetValue(gameState.AnotherPlayer, countRight);
				}
			}

			return gameState;
		}

		private int GetNotInBoardEntitiesCount(string entityName, bool ownedByLeftPlayer)
		{
			return gameEntities
				.Where(p => p.Value == entityName)
				.Where(p => OwnedByLeftPlayer(p.Key) == ownedByLeftPlayer)
				.Count();
		}

		private SimpleBoard GetSimpleBoard(IEnumerable<KeyValuePair<Point, string>> inBoardEntities)
		{
			var board = new SimpleBoard(BoardWidth, BoardHeight);

			foreach (var pair in inBoardEntities)
			{
				var inBoardCoords = FromScreenToBoard(pair.Key);
				ElementType value = (ElementType)Enum.Parse(typeof(ElementType), pair.Value);
				board[inBoardCoords] = value;
			}

			return board;
		}

		private Point FromScreenToBoard(Point p)
		{
			var res = new Point();
			res.X = (int)Math.Round((p.X - leftTopBoardCell.X) / (double)cellWidth) + 1;
			res.Y = (int)Math.Round((p.Y - leftTopBoardCell.Y) / (double)cellHeight) + 1;
			return res;
		}

		private delegate int CoordinateSelector(Point p);

		private int GetMaxDistance(IEnumerable<Point> points, CoordinateSelector selector)
		{
			var coordinates = points.Select(p => selector(p)).ToList();
			return coordinates.Max() - coordinates.Min();
		}

		private Point GetLeftTopBoardCell(IEnumerable<KeyValuePair<Point, string>> inBoardEntities)
		{
			var cell = new Point();
			
			cell.X = inBoardEntities.Min(pair => pair.Key.X);
			cell.Y = inBoardEntities.Min(pair => pair.Key.Y);

			return cell;
		}

		private Dictionary<Point, string> GetGameEntities(Bitmap image)
		{
			var result = new Dictionary<Point, string>();

			foreach (var e in EnumerateInBoardEntities().Concat(EnumerateNotInBoardEntities()))
			{
				var positions = templatesStorage.GetPositions(e, image, true);
				foreach (var p in positions)
					result.Add(p, e);
			}

			return result;
		}

		private void MakeMovement(Movement movement)
		{
			var m = (ClassicMovement)movement;

			switch (m.Kind)
			{
				case ClassicMovementKind.Swap:
					TwoClicks(FromBoardToScreen(m.First),
						FromBoardToScreen(m.Second));
					break;

				case ClassicMovementKind.Bomb:
					MakeExplositionMovement("Bomb", m.First);
					break;

				case ClassicMovementKind.Dynamit:
					MakeExplositionMovement("Dynamit", m.First);
					break;

				case ClassicMovementKind.Empty:
					Thread.Sleep(TimeSpan.FromSeconds(14) - start.Elapsed);
					break;

				default:
					break;
			}

			Mouse.Move(0, 0);
		}

		private Point FromBoardToScreen(Point inBoardPoint)
		{
			var x = leftTopBoardCell.X + cellWidth * (inBoardPoint.X - 1);
			var y = leftTopBoardCell.Y + cellWidth * (inBoardPoint.Y - 1);
			return new Point(x, y);
		}

		private void MakeExplositionMovement(string explositionSource, Point center)
		{
			var first = gameEntities.First(pair => 
				pair.Value == explositionSource && OwnedByLeftPlayer(pair.Key)).Key;

			var second = FromBoardToScreen(center);

			TwoClicks(first, second);
		}

		private void TwoClicks(Point first, Point second)
		{
			Mouse.Move(first);
			Thread.Sleep(200);
			
			Mouse.LeftClick();
			Thread.Sleep(200);
			
			Mouse.Move(second);
			Thread.Sleep(200);

			Mouse.LeftClick();
		}

		private bool OwnedByLeftPlayer(Point position)
		{
			return position.X < leftTopBoardCell.X;
		}
    }
}
 