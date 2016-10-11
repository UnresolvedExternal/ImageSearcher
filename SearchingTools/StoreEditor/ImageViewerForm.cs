using SearchingTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StoreEditor
{
	public partial class ImageViewerForm : Form
	{
		public ImageViewerForm(Bitmap image, SimpleColor differences)
		{
			InitializeComponent();

			this.MinimumSize = this.Size;
			this.MaximizeBox = false;
			this.MaximumSize = this.Size;

			this.pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
			this.pictureBox.Image = new Bitmap(image);
			this.pictureBox.BorderStyle = BorderStyle.FixedSingle;

			this.redLabel.Text = differences.R.ToString();
			this.greenLabel.Text = differences.G.ToString();
			this.blueLabel.Text = differences.B.ToString();
		}
	}
}
