using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace GameControl
{
	[TestFixture]
	class ManualTest
	{
		[Test]
		public void Test()
		{
			var shim = new ScreenGameShim(@"C:\Users\Администратор\Desktop\My\Storages\GodsGame\1280x1024\store.store");
			System.Threading.Thread.Sleep(5000);
			shim.MakeMovement();
			Assert.That(true);
		}
	}
}
