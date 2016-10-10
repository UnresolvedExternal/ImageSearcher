namespace StoreEditor
{
	partial class StoreEditor
	{
		/// <summary>
		/// Требуется переменная конструктора.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Освободить все используемые ресурсы.
		/// </summary>
		/// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Код, автоматически созданный конструктором форм Windows

		/// <summary>
		/// Обязательный метод для поддержки конструктора - не изменяйте
		/// содержимое данного метода при помощи редактора кода.
		/// </summary>
		private void InitializeComponent()
		{
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.saveButton = new System.Windows.Forms.Button();
			this.loadButton = new System.Windows.Forms.Button();
			this.createButton = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.templateNamesListBox = new System.Windows.Forms.ListBox();
			this.viewTemplateButton = new System.Windows.Forms.Button();
			this.upgradeTemplateButton = new System.Windows.Forms.Button();
			this.addTemplateButton = new System.Windows.Forms.Button();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
			this.tasksCountLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.removeTemplateButton = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.statusStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox1.Controls.Add(this.saveButton);
			this.groupBox1.Controls.Add(this.loadButton);
			this.groupBox1.Controls.Add(this.createButton);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(147, 201);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Choosing store";
			// 
			// saveButton
			// 
			this.saveButton.Location = new System.Drawing.Point(6, 107);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(135, 38);
			this.saveButton.TabIndex = 3;
			this.saveButton.Text = "Save";
			this.saveButton.UseVisualStyleBackColor = true;
			this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
			// 
			// loadButton
			// 
			this.loadButton.Location = new System.Drawing.Point(6, 63);
			this.loadButton.Name = "loadButton";
			this.loadButton.Size = new System.Drawing.Size(135, 38);
			this.loadButton.TabIndex = 2;
			this.loadButton.Text = "Load existing";
			this.loadButton.UseVisualStyleBackColor = true;
			this.loadButton.Click += new System.EventHandler(this.loadButton_Click);
			// 
			// createButton
			// 
			this.createButton.Location = new System.Drawing.Point(6, 19);
			this.createButton.Name = "createButton";
			this.createButton.Size = new System.Drawing.Size(135, 38);
			this.createButton.TabIndex = 1;
			this.createButton.Text = "Create new";
			this.createButton.UseVisualStyleBackColor = true;
			this.createButton.Click += new System.EventHandler(this.createButton_Click);
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.removeTemplateButton);
			this.groupBox2.Controls.Add(this.templateNamesListBox);
			this.groupBox2.Controls.Add(this.viewTemplateButton);
			this.groupBox2.Controls.Add(this.upgradeTemplateButton);
			this.groupBox2.Controls.Add(this.addTemplateButton);
			this.groupBox2.Location = new System.Drawing.Point(165, 12);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(458, 201);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Editor";
			// 
			// templateNamesListBox
			// 
			this.templateNamesListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.templateNamesListBox.FormattingEnabled = true;
			this.templateNamesListBox.Location = new System.Drawing.Point(6, 50);
			this.templateNamesListBox.Name = "templateNamesListBox";
			this.templateNamesListBox.Size = new System.Drawing.Size(447, 147);
			this.templateNamesListBox.TabIndex = 7;
			// 
			// viewTemplateButton
			// 
			this.viewTemplateButton.Location = new System.Drawing.Point(232, 16);
			this.viewTemplateButton.Name = "viewTemplateButton";
			this.viewTemplateButton.Size = new System.Drawing.Size(107, 25);
			this.viewTemplateButton.TabIndex = 6;
			this.viewTemplateButton.Text = "View Template";
			this.viewTemplateButton.UseVisualStyleBackColor = true;
			this.viewTemplateButton.Click += new System.EventHandler(this.viewTemplateButton_Click);
			// 
			// upgradeTemplateButton
			// 
			this.upgradeTemplateButton.Location = new System.Drawing.Point(119, 16);
			this.upgradeTemplateButton.Name = "upgradeTemplateButton";
			this.upgradeTemplateButton.Size = new System.Drawing.Size(107, 25);
			this.upgradeTemplateButton.TabIndex = 5;
			this.upgradeTemplateButton.Text = "Upgrade template";
			this.upgradeTemplateButton.UseVisualStyleBackColor = true;
			this.upgradeTemplateButton.Click += new System.EventHandler(this.upgradeTemplateButton_Click);
			// 
			// addTemplateButton
			// 
			this.addTemplateButton.Location = new System.Drawing.Point(6, 16);
			this.addTemplateButton.Name = "addTemplateButton";
			this.addTemplateButton.Size = new System.Drawing.Size(107, 25);
			this.addTemplateButton.TabIndex = 4;
			this.addTemplateButton.Text = "Add template";
			this.addTemplateButton.UseVisualStyleBackColor = true;
			this.addTemplateButton.Click += new System.EventHandler(this.addTemplateButton_Click);
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.tasksCountLabel});
			this.statusStrip1.Location = new System.Drawing.Point(0, 221);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(630, 22);
			this.statusStrip1.TabIndex = 2;
			this.statusStrip1.Text = "statusStrip";
			// 
			// toolStripStatusLabel1
			// 
			this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
			this.toolStripStatusLabel1.Size = new System.Drawing.Size(116, 17);
			this.toolStripStatusLabel1.Text = "Upgrades in process:";
			// 
			// tasksCountLabel
			// 
			this.tasksCountLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.tasksCountLabel.Name = "tasksCountLabel";
			this.tasksCountLabel.Size = new System.Drawing.Size(14, 17);
			this.tasksCountLabel.Text = "0";
			// 
			// removeTemplateButton
			// 
			this.removeTemplateButton.Location = new System.Drawing.Point(345, 16);
			this.removeTemplateButton.Name = "removeTemplateButton";
			this.removeTemplateButton.Size = new System.Drawing.Size(107, 25);
			this.removeTemplateButton.TabIndex = 8;
			this.removeTemplateButton.Text = "Remove template";
			this.removeTemplateButton.UseVisualStyleBackColor = true;
			this.removeTemplateButton.Click += new System.EventHandler(this.removeTemplateButton_Click);
			// 
			// StoreEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(630, 243);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Name = "StoreEditor";
			this.Text = "Bitmap templates editor";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.StoreEditor_FormClosing);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button loadButton;
		private System.Windows.Forms.Button createButton;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button saveButton;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.Button addTemplateButton;
		private System.Windows.Forms.Button upgradeTemplateButton;
		private System.Windows.Forms.Button viewTemplateButton;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
		private System.Windows.Forms.ToolStripStatusLabel tasksCountLabel;
		private System.Windows.Forms.ListBox templateNamesListBox;
		private System.Windows.Forms.Button removeTemplateButton;
	}
}

