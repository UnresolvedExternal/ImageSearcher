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
	internal partial class TextRequestForm : Form
	{
		public TextRequestForm(TextRequestData data)
		{
			InitializeComponent();
			this.ControlBox = false;
			this.MinimumSize = this.Size;
			this.MaximumSize = new Size(int.MaxValue, this.Height);
			
			this.Text = data.Title;

			data.ResultText = data.InitialText ?? "";
			this.textBox1.Text = data.ResultText;
			this.textBox1.SelectAll();

			var btn = new Button();
			this.AcceptButton = btn;
			btn.Click += (s, e) =>
				{
					if (data.TextValidator != null && !data.TextValidator(textBox1.Text))
					{
						MessageBox.Show("Incorrect value");
					}
					else
					{
						data.ResultText = textBox1.Text;
						this.DialogResult = System.Windows.Forms.DialogResult.OK;
					}
				};
		}
	}

	class TextRequestData
	{
		public string InitialText;
		public string ResultText;
		public string Title;
		public Func<string, bool> TextValidator;
	}
}
