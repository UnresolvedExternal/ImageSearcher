using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SearchingTools;

namespace StoreEditor
{
	public partial class StoreEditor : Form
	{
		BitmapSearcherStore _store;
		private int _tasksRunning;
		private bool changed;

		static string NEED_VALID_ID_STR = "Choose Id!";
		static string STORE_EXTENSION = ".store";
		static string IMAGE_EXTENSION = ".bmp";

		public StoreEditor()
		{
			InitializeComponent();
			this.MinimumSize = this.Size;

			_store = new BitmapSearcherStore();
			_tasksRunning = 0;
			changed = false;
		}

		private void createButton_Click(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		private void loadButton_Click(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		private void saveButton_Click(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		private void addTemplateButton_Click(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		private void upgradeTemplateButton_Click(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		private void viewTemplateButton_Click(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		private void UpdateTemplateNamesListBox()
		{
			templateNamesListBox.Items.Clear();
			foreach (var id in _store.Keys.OrderBy(x => x))
			{
				templateNamesListBox.Items.Add(id);
			}
			templateNamesListBox.SelectedIndex = templateNamesListBox.Items.Count == 0 ? -1 : 0;
		}

		private void removeTemplateButton_Click(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		private void StoreEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}
