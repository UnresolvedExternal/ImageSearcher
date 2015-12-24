using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StoreEditor
{
	public partial class StoreEditor : Form
	{

		async Task DisplayResultAsync()
		{
			int num = 5;

			int result = await FactorialAsync(num);
			Thread.Sleep(3000);
			this.BeginInvoke(new Action(
				() => this.Text += string.Format("Факториал числа {0} равен {1}", num, result))
			);
		}

		async Task DisplayResultAsync2()
		{
			int num = 9;

			int result = await FactorialAsync(num);
			Thread.Sleep(3000);
			this.BeginInvoke(new Action(
				() => this.Text += string.Format("Факториал числа {0} равен {1}", num, result))
			);
		}

		static async Task<int> FactorialAsync(int x)
		{
			int result = 1;

			return await Task.Run(() =>
			{
				for (int i = 1; i <= x; i++)
				{
					result *= i;
				}
				return result;
			});
		}

		async void G()
		{
			await FactorialAsync(10);
		}

		public StoreEditor()
		{
			InitializeComponent();
			this.Click += (o, e) => ThreadPool.QueueUserWorkItem(async x =>
			{
				DisplayResultAsync2();
				DisplayResultAsync();
			});

			button1.Click += (o, e) => ThreadPool.QueueUserWorkItem(f =>
				{
					GC.Collect();
					GC.WaitForPendingFinalizers();
					GC.Collect();
				});
		}
	}
}
