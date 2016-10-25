using System;

namespace SearchingTools
{
	// Just a RGB point (without Alpha and etc.)
	[System.Serializable]
	public struct SimpleColor: IEquatable<SimpleColor>
	{
		public byte R;
		public byte G;
		public byte B;

		public static SimpleColor FromRgb(byte red, byte green, byte blue)
		{
			return new SimpleColor
			{
				R = red,
				G = green,
				B = blue
			};
		}

		public static implicit operator System.Drawing.Color(SimpleColor color)
		{
			return System.Drawing.Color.FromArgb(color.R, color.G, color.B);
		}

		public static explicit operator SimpleColor(System.Drawing.Color color)
		{
			return SimpleColor.FromRgb(color.R, color.B, color.G);
		}

		#region Equality and equivalence

		public static bool operator ==(SimpleColor left, SimpleColor right)
		{
			return left.R == right.R && left.G == right.G && left.B == right.B;
		}

		public static bool operator !=(SimpleColor left, SimpleColor right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			try
			{
				return Equals((SimpleColor)obj);
			} 
			catch (InvalidCastException)
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			int digit = byte.MaxValue + 1;
			return R * digit * digit + G * digit + B;
		}

		public bool Equals(SimpleColor other)
		{
			return this == other;
		}

		#endregion

		public override string ToString()
		{
			return string.Format("({0}, {1}, {2})", R, G, B);
		}
	}
}
