using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodsGameApi
{
	public class ClassicMovement: Movement, IEquatable<ClassicMovement>
	{
		/// <summary>
		/// Если используется динамит или бомба, эта точка - центр взрыва
		/// </summary>
		public Point First { get; set; }

		public Point Second { get; set; }

		public ClassicMovementKind Kind { get; set; }

		#region Equality

		public override bool Equals(object obj)
		{
			var other = obj as ClassicMovement;
			return Equals(other);
		}

		public override int GetHashCode()
		{
			switch (Kind)
			{
				case ClassicMovementKind.Swap:
					return First.GetHashCode() * Second.GetHashCode();
				case ClassicMovementKind.Bomb:
					return First.GetHashCode() + 1;
				case ClassicMovementKind.Dynamit:
					return First.GetHashCode() + 1;
				case ClassicMovementKind.Empty:
					return 0;
				default:
					return 0;
			}
		}

		public bool Equals(ClassicMovement other)
		{
			if (object.ReferenceEquals(other, null))
				return false;

			if (Kind != other.Kind)
				return false;

			switch (Kind)
			{
				case ClassicMovementKind.Swap:
					return First.Equals(other.First) && Second.Equals(other.Second) ||
						First.Equals(other.Second) && Second.Equals(other.First);
				case ClassicMovementKind.Bomb:
					return First.Equals(other.First);
				case ClassicMovementKind.Dynamit:
					return First.Equals(other.First);
				case ClassicMovementKind.Empty:
					return true;
				default:
					return false;
			}
		}

		public bool Equals(Movement other)
		{
			return Equals(other);
		}

		#endregion

		public override string ToString()
		{
			return string.Format("Kind={0} First={1} Second={2}", Kind, First, Second);
		}
	}
}
