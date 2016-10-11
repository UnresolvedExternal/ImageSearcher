using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StoreEditor
{
	public partial class StoreEditor : Form
	{
		private SearchingTools.BitmapSearcherStore store = new SearchingTools.BitmapSearcherStore();
		private static string needValidId = "Choose Id!";
		private static string storeExtension = ".store";
		private static string imageExtension = ".bmp";
		
		/// <summary>
		/// Количество задач в очереди
		/// </summary>
		private int tasksCount = 0;

		private bool saved = true;

		public StoreEditor()
		{
			InitializeComponent();
			this.MinimumSize = this.Size;
		}

		private void createButton_Click(object sender, EventArgs e)
		{
			var result = AskSave();
			if (result != DialogResult.Cancel)
			{
				store = new SearchingTools.BitmapSearcherStore();
			}
			UpdateTemplateNamesListBox();
		}

		private DialogResult StartLoadStoreDialog()
		{
			var dialog = new OpenFileDialog();
			var result = StartDialog(dialog, storeExtension);

			if (result == DialogResult.OK)
			{
				try
				{
					store = SearchingTools.BitmapSearcherStore.Load(dialog.FileName);
					saved = true;
				}
				catch (System.IO.IOException e)
				{
					store = new SearchingTools.BitmapSearcherStore();
					MessageBox.Show(e.Message);
				}
			}

			return result;
		}

		private DialogResult StartSaveStoreDialog()
		{
			var dialog = new SaveFileDialog();
			var result = StartDialog(dialog, storeExtension);
			
			if (result == DialogResult.OK)
			{
				try
				{
					store.Save(dialog.FileName);
					saved = true;
				}
				catch (System.IO.IOException e)
				{
					MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK);
					return DialogResult.Cancel;
				}
			}

			return result;
		}

		private static DialogResult StartDialog(FileDialog dialog, string extension)
		{
			dialog.AddExtension = true;
			dialog.DefaultExt = extension;
			dialog.Filter = "Store files (*" + extension + ")|*" + extension + "";
			var result = dialog.ShowDialog();
			return result;
		}

		/// <summary>
		/// Инициирует сохранение при необходимости.
		/// </summary>
		/// <returns></returns>
		private DialogResult AskSave()
		{
			if (saved)
				return DialogResult.No;

			if (tasksCount > 0)
			{
				MessageBox.Show("Wait for all upgrades to complete", "Please wait", MessageBoxButtons.OK);
				return DialogResult.Cancel;
			}

			DialogResult result = MessageBox.Show("Do you want to save this storage?", "Save", MessageBoxButtons.YesNoCancel);
			if (result == DialogResult.Yes)
			{
				result = StartSaveStoreDialog();
			}
			return result;
		}

		private void loadButton_Click(object sender, EventArgs e)
		{
			var result = AskSave();
			if (result != DialogResult.Cancel)
			{
				StartLoadStoreDialog();
			}
			UpdateTemplateNamesListBox();
		}

		private void saveButton_Click(object sender, EventArgs e)
		{
			var oldSaved = saved;

			saved = false;
			var result = AskSave();

			saved = oldSaved || result == System.Windows.Forms.DialogResult.OK;
		}

		private void addTemplateButton_Click(object sender, EventArgs e)
		{
			var dialog = new OpenFileDialog();
			var result = StartDialog(dialog, imageExtension);
			
			if (result == DialogResult.OK)
			{
				Bitmap bmp = null;
				try
				{
					bmp = TryLoadBitmap(dialog);

					var data = GetTextRequestDataForTemplateName(dialog.FileName);

					var newForm = new TextRequestForm(data);
					newForm.ShowDialog();
					TryAddElem(data.ResultText, bmp);
				}
				catch (System.IO.IOException exc)
				{
					MessageBox.Show(exc.Message);
				}
				finally
				{
					if (bmp != null)
						bmp.Dispose();
				}

				UpdateTemplateNamesListBox();
			}
		}

		private static TextRequestData GetTextRequestDataForTemplateName(string filename)
		{
			var data = new TextRequestData
			{
				InitialText = Path.GetFileNameWithoutExtension(filename),
				Title = "Enter template's name",
				TextValidator = (s) => !string.IsNullOrEmpty(s)
			};
			return data;
		}

		private void TryAddElem(string id, Bitmap bmp)
		{
			try
			{
				store.Add(id, bmp);
				saved = false;
			}
			catch (InvalidOperationException e)
			{
				MessageBox.Show(e.Message);
			}
		}

		private void upgradeTemplateButton_Click(object sender, EventArgs e)
		{
			var id = this.templateNamesListBox.Text;
			if (!store.Keys.Contains(id))
				MessageBox.Show(needValidId);
			else
			{
				var dialog = new OpenFileDialog();
				var result = StartDialog(dialog, imageExtension);

				if (result == DialogResult.OK)
				{
					Bitmap image = TryLoadBitmap(dialog);

					var data = GetRequestDataForNumber();

					new TextRequestForm(data).ShowDialog();
					int count = int.Parse(data.ResultText);

					UpgradeAsync(id, image, count);
				}
			}
		}

		private async void UpgradeAsync(string id, Bitmap image, int count)
		{
			saved = false;
			IncrementTasks(1);
			
			try
			{
				var task = store.UpgradeAsync(id, image, count);
				await task;
			} 
			catch (InvalidOperationException)
			{
				MessageBox.Show("Key doesn't exist", "Error", MessageBoxButtons.OK);
			}
			catch (AggregateException e)
			{
				e.Handle(exc => exc.GetType() == typeof(InvalidOperationException));
				MessageBox.Show("Key doesn't exist", "Error", MessageBoxButtons.OK);
			}

			IncrementTasks(-1);
		}

		private static TextRequestData GetRequestDataForNumber()
		{
			var data = new TextRequestData
			{
				InitialText = "0",
				Title = "Enter number of elemnts the image consists of",
				TextValidator = (s) =>
				{
					int value;
					bool correct = int.TryParse(s, out value);
					if (!correct)
						return false;
					if (value <= 0)
						return false;
					return true;
				}
			};
			return data;
		}

		private static Bitmap TryLoadBitmap(OpenFileDialog dialog)
		{
			Bitmap image = null;
			try
			{
				image = (Bitmap)Image.FromFile(dialog.FileName);
			}
			catch (InvalidCastException)
			{
				MessageBox.Show("Format of this file isn't supported");
			}
			catch (System.IO.IOException)
			{
				MessageBox.Show("Cant read the file");
			}
			return image;
		}

		private void IncrementTasks(int value)
		{
			tasksCount += value;
			tasksCountLabel.Text = tasksCount.ToString();
		}

		private void viewTemplateButton_Click(object sender, EventArgs e)
		{
			var id = this.templateNamesListBox.Text;
			if (!store.Keys.Contains(id))
				MessageBox.Show(needValidId);
			else
			{
				var image = store.GetTemplate(id);
				var form = new ImageViewerForm(image, store.GetTemplateSettings(id));
				form.ShowDialog();
			}
		}

		private void UpdateTemplateNamesListBox()
		{
			templateNamesListBox.Items.Clear();
			foreach (var id in store.Keys.OrderBy(x => x))
			{
				templateNamesListBox.Items.Add(id);
			}
			templateNamesListBox.SelectedIndex = -1;
		}

		private void removeTemplateButton_Click(object sender, EventArgs e)
		{
			var id = this.templateNamesListBox.Text;
			if (!store.Remove(id))
				MessageBox.Show(needValidId);
			else
			{
				saved = false;
				UpdateTemplateNamesListBox();
			}
		}

		private void StoreEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			e.Cancel = (AskSave() == DialogResult.Cancel);
		}
	}
}
