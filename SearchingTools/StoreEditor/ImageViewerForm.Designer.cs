namespace StoreEditor
{
	partial class ImageViewerForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.pictureBox = new System.Windows.Forms.PictureBox();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.blueLabel = new System.Windows.Forms.Label();
			this.greenLabel = new System.Windows.Forms.Label();
			this.redLabel = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// pictureBox
			// 
			this.pictureBox.Location = new System.Drawing.Point(12, 12);
			this.pictureBox.Name = "pictureBox";
			this.pictureBox.Size = new System.Drawing.Size(260, 241);
			this.pictureBox.TabIndex = 0;
			this.pictureBox.TabStop = false;
			this.pictureBox.WaitOnLoad = true;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.label1.ForeColor = System.Drawing.Color.Red;
			this.label1.Location = new System.Drawing.Point(6, 29);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(34, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Red:";
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.blueLabel);
			this.groupBox1.Controls.Add(this.greenLabel);
			this.groupBox1.Controls.Add(this.redLabel);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.groupBox1.Location = new System.Drawing.Point(278, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(197, 241);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Admissible differences";
			// 
			// blueLabel
			// 
			this.blueLabel.AutoSize = true;
			this.blueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.blueLabel.ForeColor = System.Drawing.Color.Blue;
			this.blueLabel.Location = new System.Drawing.Point(58, 74);
			this.blueLabel.Name = "blueLabel";
			this.blueLabel.Size = new System.Drawing.Size(14, 13);
			this.blueLabel.TabIndex = 6;
			this.blueLabel.Text = "0";
			// 
			// greenLabel
			// 
			this.greenLabel.AutoSize = true;
			this.greenLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.greenLabel.ForeColor = System.Drawing.Color.Green;
			this.greenLabel.Location = new System.Drawing.Point(58, 51);
			this.greenLabel.Name = "greenLabel";
			this.greenLabel.Size = new System.Drawing.Size(14, 13);
			this.greenLabel.TabIndex = 5;
			this.greenLabel.Text = "0";
			// 
			// redLabel
			// 
			this.redLabel.AutoSize = true;
			this.redLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.redLabel.ForeColor = System.Drawing.Color.Red;
			this.redLabel.Location = new System.Drawing.Point(58, 29);
			this.redLabel.Name = "redLabel";
			this.redLabel.Size = new System.Drawing.Size(14, 13);
			this.redLabel.TabIndex = 4;
			this.redLabel.Text = "0";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.label3.ForeColor = System.Drawing.Color.Green;
			this.label3.Location = new System.Drawing.Point(6, 51);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(45, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Green:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.label2.ForeColor = System.Drawing.Color.Blue;
			this.label2.Location = new System.Drawing.Point(6, 74);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(36, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Blue:";
			// 
			// ImageViewerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(487, 265);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.pictureBox);
			this.Name = "ImageViewerForm";
			this.Text = "Template\'s properties";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label blueLabel;
		private System.Windows.Forms.Label greenLabel;
		private System.Windows.Forms.Label redLabel;
	}
}