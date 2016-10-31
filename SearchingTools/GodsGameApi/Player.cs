using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodsGameApi
{
	[Serializable]
	public class Player: IEquatable<Player>
	{
		public Hitpoints Hp { get; set; }
		public int Bombs { get; set; }
		public int Dynamits { get; set; }
		public int Diamonds { get; set; }

		public Player Clone()
		{
			return new Player
				{
					Hp = this.Hp.Clone(),
					Bombs = this.Bombs,
					Dynamits = this.Dynamits,
					Diamonds = this.Diamonds
				};
		}

		public bool Equals(Player other)
		{
			return Hp.Equals(other.Hp) &&
				Bombs == other.Bombs &&
				Dynamits == other.Dynamits &&
				Diamonds == other.Diamonds;
		}

		public override string ToString()
		{
			return string.Format("Hp={0} Bombs={1} Dynamits={2} Diamonds={3}", Hp.ToString(), Bombs, Dynamits, Diamonds);
		}
	}
}
