namespace Toolkit
{
    partial class StartDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StartDialog));
            this.ConnectedDevicesListBox = new System.Windows.Forms.ListBox();
            this.refreshButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.driverButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.loadingSpinner = new System.Windows.Forms.PictureBox();
            this.NoDevicesLabel = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.loadingSpinner)).BeginInit();
            this.SuspendLayout();
            // 
            // ConnectedDevicesListBox
            // 
            this.ConnectedDevicesListBox.BackColor = System.Drawing.SystemColors.Control;
            this.ConnectedDevicesListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ConnectedDevicesListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.ConnectedDevicesListBox.Enabled = false;
            this.ConnectedDevicesListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ConnectedDevicesListBox.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.ConnectedDevicesListBox.ItemHeight = 20;
            this.ConnectedDevicesListBox.Location = new System.Drawing.Point(24, 45);
            this.ConnectedDevicesListBox.Name = "ConnectedDevicesListBox";
            this.ConnectedDevicesListBox.Size = new System.Drawing.Size(449, 100);
            this.ConnectedDevicesListBox.TabIndex = 3;
            this.ConnectedDevicesListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ConnectedDevicesListBox_DrawItem);
            this.ConnectedDevicesListBox.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ConnectedDevicesListBox_MouseDoubleClick);
            // 
            // refreshButton
            // 
            this.refreshButton.Enabled = false;
            this.refreshButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.refreshButton.Location = new System.Drawing.Point(123, 177);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(235, 43);
            this.refreshButton.TabIndex = 2;
            this.refreshButton.Text = "Refresh connected devices";
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.driverButton);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(12, 255);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(473, 197);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Help:";
            // 
            // driverButton
            // 
            this.driverButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.driverButton.Location = new System.Drawing.Point(111, 21);
            this.driverButton.Name = "driverButton";
            this.driverButton.Size = new System.Drawing.Size(261, 34);
            this.driverButton.TabIndex = 5;
            this.driverButton.Text = "Download and install ADB drivers";
            this.driverButton.UseVisualStyleBackColor = true;
            this.driverButton.Click += new System.EventHandler(this.driverButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(9, 72);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(428, 112);
            this.label1.TabIndex = 0;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.loadingSpinner);
            this.groupBox2.Controls.Add(this.NoDevicesLabel);
            this.groupBox2.Controls.Add(this.ConnectedDevicesListBox);
            this.groupBox2.Controls.Add(this.refreshButton);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(479, 237);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Double click on the device you want to flash:";
            // 
            // loadingSpinner
            // 
            this.loadingSpinner.Image = ((System.Drawing.Image)(resources.GetObject("loadingSpinner.Image")));
            this.loadingSpinner.Location = new System.Drawing.Point(133, 45);
            this.loadingSpinner.Name = "loadingSpinner";
            this.loadingSpinner.Size = new System.Drawing.Size(226, 22);
            this.loadingSpinner.TabIndex = 4;
            this.loadingSpinner.TabStop = false;
            // 
            // NoDevicesLabel
            // 
            this.NoDevicesLabel.AutoSize = true;
            this.NoDevicesLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.NoDevicesLabel.Location = new System.Drawing.Point(173, 46);
            this.NoDevicesLabel.Name = "NoDevicesLabel";
            this.NoDevicesLabel.Size = new System.Drawing.Size(149, 20);
            this.NoDevicesLabel.TabIndex = 0;
            this.NoDevicesLabel.Text = "No device detected!";
            // 
            // StartDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(503, 464);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "StartDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Transformer Toolkit ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.StartDialog_FormClosing);
            this.Load += new System.EventHandler(this.StartDialog_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.loadingSpinner)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox ConnectedDevicesListBox;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label NoDevicesLabel;
        private System.Windows.Forms.Button driverButton;
        private System.Windows.Forms.PictureBox loadingSpinner;
    }
}