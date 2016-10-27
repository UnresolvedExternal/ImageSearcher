using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
		int _TasksRunning
		{
			get
			{
				return _tasksRunning;
			}
			set
			{
				_tasksRunning = value;
				tasksCountLabel.Text = _tasksRunning.ToString();
			}
		}
		bool _changed;

		static string STORE_EXTENSION = ".store";
		static string IMAGE_EXTENSION = ".bmp";

		public StoreEditor()
		{
			InitializeComponent();
			this.MinimumSize = this.Size;

			_store = new BitmapSearcherStore();
			_TasksRunning = 0;
			_changed = false;
		}

		private void UpdateTemplateNamesListBox()
		{
			var selectedItem = templateNamesListBox.SelectedItem;
			Populate(templateNamesListBox);
			InitializeSelectedIndex(templateNamesListBox, selectedItem);
		}

		private static void InitializeSelectedIndex(ListBox list, object previousSelected)
		{
			int index = previousSelected == null ? -1 : list.Items.IndexOf(previousSelected);
			if (index == -1)
				list.SelectedIndex = list.Items.Count == 0 ? -1 : 0;
			else
				list.SelectedIndex = index;
		}

		private void Populate(ListBox list)
		{
			list.Items.Clear();
			foreach (var id in _store.Keys.OrderBy(x => x))
				list.Items.Add(id);
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
			if (result == DialogResult.OK)
				return ProcessSave(dialog);
			else
				return SaveResult.Cancelled;
		}

		private void ShowUnknownErrorExhortation(Exception e)
		{
			MessageBox.Show(e.Message, "Unknown error");
			MessageBox.Show("Unknown error was occured. It is better to reopen " +
				"the application without saving. Otherwise you riscs to corrupt the data.", "Error");
		}

		private SaveResult ProcessSave(SaveFileDialog dialog)
		{
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
			catch (IOException e)
			{
				MessageBox.Show("Input/output error: " + e.Message, "Error");
				return SaveResult.Cancelled;
			}
			catch (Exception e)
			{
				ShowUnknownErrorExhortation(e);
				return SaveResult.Cancelled;
			}
		}

		private void LoadStore()
		{
			var dialog = new OpenFileDialog();
			var result = StartDialog(dialog, STORE_EXTENSION, "Store files");
			if (result == DialogResult.OK)
				ProcessLoad(dialog.FileName);
		}

		private void ProcessLoad(string filename)
		{
			bool isLoaded = false;
			try
			{
				_store = BitmapSearcherStore.Load(filename);
				isLoaded = true;
				_changed = false;
				UpdateTemplateNamesListBox();
			}
			catch (IOException e)
			{
				MessageBox.Show("Input/output error: " + e.Message, "Error");
			}
			catch (Exception e)
			{
				ShowUnknownErrorExhortation(e);
			}
			finally
			{
				if (!isLoaded)
				{
					_store = new BitmapSearcherStore();
					_changed = false;
				}
			}
		}

		/// <summary>
		/// Сохраняет хранилище при необходимости.
		/// </summary>
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
			else
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
				LoadStore();
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
				ProcessAddTemplate(dialog.FileName);
		}

		private void ProcessAddTemplate(string filename)
		{
			Bitmap image = LoadBitmap(filename);

			try
			{
				var data = GetTemplateNameRequestData(filename);
				new TextRequestForm(data).ShowDialog();
				data.ResultText = data.ResultText.Trim();
				_changed = true;
				_store.Add(data.ResultText, image);
				UpdateTemplateNamesListBox();
			}
			catch (ArgumentException)
			{
				MessageBox.Show("Store already contains an alement with same id", "Error");
			}
			catch (Exception e)
			{
				ShowUnknownErrorExhortation(e);
			}
		}

		private Bitmap LoadBitmap(string filename)
		{
			Bitmap image = null;
			try
			{
				image = (Bitmap)Image.FromFile(filename);
			}
			catch (IOException e)
			{
				MessageBox.Show("Input/output error: " + e.Message, "Error");
			}
			catch (Exception e)
			{
				ShowUnknownErrorExhortation(e);
			}
			return image;
		}

		private TextRequestData GetTemplateNameRequestData(string filename)
		{
			var data = new TextRequestData
			{
				InitialText = Path.GetFileNameWithoutExtension(filename),
				Title = "Enter template's name",
				TextValidator = str => !_store.Keys.Contains(str.Trim())
			};
			return data;
		}

		private async void upgradeTemplateButton_Click(object sender, EventArgs e)
		{
			if (templateNamesListBox.SelectedItem == null)
				MessageBox.Show("Select a template to upgrade", "Error");
			else
			{
				var dialog = new OpenFileDialog();
				dialog.Title = "Upgrade template";
				var result = StartDialog(dialog, IMAGE_EXTENSION, "Bitmap files");
				if (result == DialogResult.OK)
					try
					{
						await ProcessUpgrade(dialog.FileName);
					}
					catch (ArgumentNullException)
					{
						MessageBox.Show("Wrong file name", "Error");
					}
					catch (Exception ex)
					{
						ShowUnknownErrorExhortation(ex);
					}
			}
		}

		private async Task ProcessUpgrade(string filename)
		{
			var image = LoadBitmap(filename);
			var data = GetNumberRequestData();
			new TextRequestForm(data).ShowDialog();
			_changed = true;
			++_TasksRunning;
			try
			{
				await _store.UpgradeAsync((string)templateNamesListBox.SelectedItem, image, int.Parse(data.ResultText));
			}
			finally
			{
				--_TasksRunning;
			}
		}

		private static TextRequestData GetNumberRequestData()
		{
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
			return data;
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
			var id = templateNamesListBox.SelectedItem;
			if (id == null)
				MessageBox.Show("Select a template to remove", "Error");
			else
			{
				_changed = true;
				try
				{
					_store.Remove((string)id);
				}
				catch (ArgumentException exc)
				{
					ShowUnknownErrorExhortation(exc);
					return;
				}
				UpdateTemplateNamesListBox();
			}
		}

		private void StoreEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (AskAndSave() == SaveResult.Cancelled)
				e.Cancel = true;
		}
	}
}