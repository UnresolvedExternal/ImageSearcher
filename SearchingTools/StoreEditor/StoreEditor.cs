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
		enum SaveResult
		{
			Succeeded, Rejected, Cancelled
		}

		BitmapSearcherStore _store;
		int _tasksRunning;
		bool _changed;

		static string STORE_EXTENSION = ".store";
		static string IMAGE_EXTENSION = ".bmp";

		public StoreEditor()
		{
			InitializeComponent();
			this.MinimumSize = this.Size;

			_store = new BitmapSearcherStore();
			_tasksRunning = 0;
			_changed = false;
		}

		private void UpdateTemplateNamesListBox()
		{
			var list = templateNamesListBox;
			var selectedItem = list.SelectedItem;
			list.Items.Clear();
			foreach (var id in _store.Keys.OrderBy(x => x))
				list.Items.Add(id);
			if (selectedItem == null)
				list.SelectedIndex = list.Items.Count == 0 ? -1 : 0;
			else
				list.SelectedIndex = list.Items.IndexOf(selectedItem);
			tasksCountLabel.Text = _tasksRunning.ToString();
		}

		private static DialogResult StartDialog(FileDialog dialog, string extension, string description)
		{
			dialog.AddExtension = true;
			dialog.DefaultExt = extension;
			dialog.Filter = string.Format("{0}(*{1})|*{1}", description, extension);
			var result = dialog.ShowDialog();
			return result;
		}

		private SaveResult SaveStore()
		{
			var dialog = new SaveFileDialog();
			var result = StartDialog(dialog, STORE_EXTENSION, "Store files");
			switch (result)
			{
				case DialogResult.OK:
					try
					{
						if (_store.TasksRunning > 0)
						{
							MessageBox.Show("Wait for all operations to complete", "Error");
							return SaveResult.Cancelled;
						}
						_store.Save(dialog.FileName);
						_changed = false;
						return SaveResult.Succeeded;
					}
					catch (Exception e)
					{
						MessageBox.Show(e.Message, "Unknown error");
						return SaveResult.Cancelled;
					}
				default:
					return SaveResult.Cancelled;
			}
		}

		private void LoadStore()
		{
			var dialog = new OpenFileDialog();
			var result = StartDialog(dialog, STORE_EXTENSION, "Store files");
			switch (result)
			{
				case DialogResult.OK:
					try
					{
						_store = BitmapSearcherStore.Load(dialog.FileName);
						_changed = false;
						UpdateTemplateNamesListBox();
						return;
					}
					catch (Exception e)
					{
						MessageBox.Show(e.Message, "Unknown error");
						return;
					}
			}
		}

		/// <summary>
		/// Сохраняет хранилище при необходимости.
		/// </summary>
		/// <returns></returns>
		private SaveResult AskAndSave()
		{
			if (_changed)
			{
				var result = MessageBox.Show("Do you want to save the changes?", "Save", MessageBoxButtons.YesNoCancel);
				switch (result)
				{
					case DialogResult.No:
						return SaveResult.Rejected;
					case DialogResult.Yes:
						return SaveStore();
					default:
						return SaveResult.Cancelled;
				}
			}
			return SaveResult.Rejected;
		}

		private void createButton_Click(object sender, EventArgs e)
		{
			switch (AskAndSave())
			{
				case SaveResult.Cancelled:
					return;
				default:
					_store = new BitmapSearcherStore();
					_changed = false;
					return;
			}
		}

		private void loadButton_Click(object sender, EventArgs e)
		{
			var result = AskAndSave();
			if (result != SaveResult.Cancelled)
			{
				LoadStore();
			}
		}

		private void saveButton_Click(object sender, EventArgs e)
		{
			SaveStore();
		}

		private void addTemplateButton_Click(object sender, EventArgs e)
		{
			var dialog = new OpenFileDialog();
			dialog.Title = "Add template";
			var result = StartDialog(dialog, IMAGE_EXTENSION, "Bitmap files");
			if (result == DialogResult.OK)
			{
				try
				{
					var image = (Bitmap)Image.FromFile(dialog.FileName);
					var data = new TextRequestData
					{
						InitialText = Path.GetFileNameWithoutExtension(dialog.FileName),
						Title = "Enter template's name",
						TextValidator = str => !_store.Keys.Contains(str.Trim())
					};
					new TextRequestForm(data).ShowDialog();
					data.ResultText = data.ResultText.Trim();
					_store.Add(data.ResultText, image);
					_changed = true;
					UpdateTemplateNamesListBox();
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message, "Unknown error");
				}
			}
		}

		private async void upgradeTemplateButton_Click(object sender, EventArgs e)
		{
			var dialog = new OpenFileDialog();
			dialog.Title = "Upgrade template";
			var result = StartDialog(dialog, IMAGE_EXTENSION, "Bitmap files");
			if (result == DialogResult.OK)
			{
				try
				{
					var image = (Bitmap)Image.FromFile(dialog.FileName);
					var data = new TextRequestData
					{
						InitialText = "1",
						Title = "Enter number of elements the image consists of",
						TextValidator = str =>
							{
								int num;
								if (!int.TryParse(str, out num))
									return false;
								return num > 0;
							}
					};
					new TextRequestForm(data).ShowDialog();
					_changed = true;
					++_tasksRunning;
					await _store.UpgradeAsync((string)templateNamesListBox.SelectedItem, image, int.Parse(data.ResultText));
					--_tasksRunning;
					UpdateTemplateNamesListBox();
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message, "Unknown error");
				}
			}
		}

		private void viewTemplateButton_Click(object sender, EventArgs e)
		{
			string id = (string)templateNamesListBox.SelectedItem;
			if (id == null)
				MessageBox.Show("Select a template to view", "Error");
			else
				new ImageViewerForm(_store.GetTemplate(id), _store.GetTemplateSettings(id)).ShowDialog();
		}

		private void removeTemplateButton_Click(object sender, EventArgs e)
		{
			string id = (string)templateNamesListBox.SelectedItem;
			if (id == null)
				MessageBox.Show("Select a template to remove", "Error");
			else
				_store.Remove(id);
		}

		private void StoreEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (AskAndSave() == SaveResult.Cancelled)
				e.Cancel = true;
		}
	}
}