using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodsGameApi
{
	[Serializable]
	public class Hitpoints: IEquatable<Hitpoints>
	{
		public int Current { get; set; }
		public int Max { get; set; }

		public void AddHealth(int value)
		{
			Current = Math.Min(Max, Current + value);
			Current = Math.Max(Current, 0);
		}

		public Hitpoints(int current, int max)
		{
			this.Current = current;
			this.Max = max;
		}

		public Hitpoints Clone()
		{
			return new Hitpoints(Current, Max);
		}

		public bool Equals(Hitpoints other)
		{
			return Current == other.Current &&
				Max == other.Max;
		}

		public override string ToString()
		{
			return string.Format("[{0}/{1}]", Current, Max);
		}
	}
}
