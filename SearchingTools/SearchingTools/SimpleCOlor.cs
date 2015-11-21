
namespace SearchingTools
{
	// Just a RGB point (without Alpha and etc.)
	public struct SimpleColor
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

		public static explicit operator System.Drawing.Color(SimpleColor color)
		{
			return System.Drawing.Color.FromArgb(color.R, color.G, color.B);
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
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion
	}
}
