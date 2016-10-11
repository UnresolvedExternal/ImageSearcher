using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameControl
{
	static class Logger
	{
		static string logDir = 
			@"C:\Users\Администратор\Desktop\My\Storages\GodsGame\1280x1024\Logs\";

		static int logCount = 0;

		static Logger()
		{
			var dateFormat = @"dd.hh.mm.ss";
			logDir = Path.Combine(logDir, DateTime.Now.ToString(dateFormat));
			Directory.CreateDirectory(logDir + "\\");
		}

		[method: System.Diagnostics.Conditional("LOG_ON")]
		internal static void Log(Bitmap image)
		{
			++logCount;
			image.Save(logDir + "\\screen" + logCount.ToString() + ".bmp");
		}

		[method: System.Diagnostics.Conditional("LOG_ON")]
		internal static void Log(GodsGameApi.Movement movement)
		{
			var file = File.CreateText(Path.Combine(logDir, "move" + logCount + ".txt"));
			file.WriteLine(movement.ToString());
			file.Close();
		}
	}
}
