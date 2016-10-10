using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GameControl
{
	class Program
	{
		static void Main(string[] args)
		{
			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
			Console.WindowHeight = 8;
			Console.WindowTop = 100;
			Console.ReadLine();
			var shim = new ScreenGameShim(@"C:\Users\Администратор\Desktop\My\Storages\GodsGame\1280x1024\store.store");
			for (int i = 1; true; ++i)
			{
				Console.WriteLine("Iteration: {0}", i);
				var sw = Stopwatch.StartNew();
				bool fl = shim.MakeMovement();
				Console.WriteLine("{0} {1}", fl, sw.Elapsed);
				Console.WriteLine("------------------------------------");
				if (fl)
					System.Threading.Thread.Sleep(2000);
			}
		}
	}
}
