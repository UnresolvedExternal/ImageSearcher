using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MouseManipulator
{
	public static class Mouse
	{
		/// <summary>
		/// You may lock it to be confident of no uses of this class in other threads
		/// </summary>
		public static object Locker = new object();

		[DllImport("user32.dll")]
		static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

		enum MouseEventFlags
		{
			LEFTDOWN = 0x00000002,
			LEFTUP = 0x00000004,
			MIDDLEDOWN = 0x00000020,
			MIDDLEUP = 0x00000040,
			MOVE = 0x00000001,
			ABSOLUTE = 0x00008000,
			RIGHTDOWN = 0x00000008,
			RIGHTUP = 0x00000010
		}

		public static void LeftClick()
		{
			mouse_event((int)(MouseEventFlags.LEFTDOWN), 0, 0, 0, 0);
			System.Threading.Thread.Sleep(100);
			mouse_event((int)(MouseEventFlags.LEFTUP), 0, 0, 0, 0);
		}

		public static void Move(int x, int y)
		{
			Cursor.Position = new System.Drawing.Point(x, y);
		}

		public static void Move(Point position)
		{
			Move(position.X, position.Y);
		}


		public static void PressLeft()
		{
			mouse_event((int)MouseEventFlags.LEFTDOWN, 0, 0, 0, 0);
		}

		public static void UnpressLeft()
		{
			mouse_event((int)MouseEventFlags.LEFTUP, 0, 0, 0, 0);
		}
	}
}