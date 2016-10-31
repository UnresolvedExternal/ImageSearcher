using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodsGameApi
{
	public class ClassicGameState: IEquatable<ClassicGameState>
	{
		public Player CurrentPlayer { get; set; }
		public Player AnotherPlayer { get; set; }

		private Player Player { get; set; }
		private Player Enemy { get; set; }

		public SimpleBoard Board { get; set; }

		public ClassicRules Rules { get; private set; }

		public void SwapPlayers()
		{
			var temp = CurrentPlayer;
			CurrentPlayer = AnotherPlayer;
			AnotherPlayer = temp;
		}

		public ClassicGameState(int width, int height)
		{
			CurrentPlayer = new Player();
			CurrentPlayer.Hp = new Hitpoints(100, 100);
			AnotherPlayer = CurrentPlayer.Clone();

			Player = CurrentPlayer;
			Enemy = AnotherPlayer;

			Rules = new ClassicRules(width, height);
		}

		public ClassicGameState(SimpleBoard board):
			this(board.Width, board.Height)
		{
			this.Board = board;
		}

		private ClassicGameState()
		{
		}

		public ClassicGameState Clone()
		{
			var result = new ClassicGameState
				{
					Player = this.Player.Clone(),
					Enemy = this.Enemy.Clone(),
					Board = this.Board.Clone(),
					Rules = new ClassicRules(this.Board.Width, this.Board.Height)
				};

			bool isPlayerIsCurrent = object.ReferenceEquals(this.CurrentPlayer, this.Player);
			result.CurrentPlayer = isPlayerIsCurrent ? result.Player : result.Enemy;
			result.AnotherPlayer = isPlayerIsCurrent ? result.Enemy : result.Player;

			return result;
		}

		public bool GameOver()
		{
			return Player.Hp.Current == 0 || Enemy.Hp.Current == 0;
		}

		#region Equality

		public override bool Equals(object obj)
		{
			return Equals(obj as ClassicGameState);
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public bool Equals(ClassicGameState other)
		{
			if (object.ReferenceEquals(other, null))
				return false;

			return CurrentPlayer.Equals(other.CurrentPlayer) &&
				AnotherPlayer.Equals(other.AnotherPlayer) &&
				Board.Equals(other.Board);
		}

		#endregion

		public override string ToString()
		{
			return string.Format("Player={0} Enemy={1} Board={2}", CurrentPlayer, AnotherPlayer, Board.ToString());
		}
	}
}
