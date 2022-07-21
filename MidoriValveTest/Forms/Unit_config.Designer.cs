
namespace MidoriValveTest
{
    partial class unit_form
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(unit_form));
            this.label9 = new System.Windows.Forms.Label();
            this.unit_scale = new System.Windows.Forms.ComboBox();
            this.Save = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.Cancel = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.White;
            this.label9.Location = new System.Drawing.Point(105, 78);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(99, 18);
            this.label9.TabIndex = 12;
            this.label9.Text = "Pressure unit:";
            // 
            // unit_scale
            // 
            this.unit_scale.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.unit_scale.FormattingEnabled = true;
            this.unit_scale.Items.AddRange(new object[] {
            "mbar",
            "Torr",
            "PSI",
            "ATM"});
            this.unit_scale.Location = new System.Drawing.Point(210, 75);
            this.unit_scale.Name = "unit_scale";
            this.unit_scale.Size = new System.Drawing.Size(223, 21);
            this.unit_scale.TabIndex = 13;
            this.unit_scale.SelectedIndexChanged += new System.EventHandler(this.unit_scale_SelectedIndexChanged);
            // 
            // Save
            // 
            this.Save.BackColor = System.Drawing.Color.Transparent;
            this.Save.BackgroundImage = global::MidoriValveTest.Properties.Resources.btnDisa2;
            this.Save.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Save.FlatAppearance.BorderSize = 0;
            this.Save.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Save.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Save.ForeColor = System.Drawing.Color.SteelBlue;
            this.Save.Location = new System.Drawing.Point(143, 124);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(109, 40);
            this.Save.TabIndex = 18;
            this.Save.Text = "Save";
            this.Save.UseVisualStyleBackColor = false;
            this.Save.Click += new System.EventHandler(this.Save_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(73, 192);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(389, 18);
            this.label1.TabIndex = 15;
            this.label1.Text = "Any scale alteration, will restart the pressure-position chart";
            // 
            // Cancel
            // 
            this.Cancel.BackColor = System.Drawing.Color.Transparent;
            this.Cancel.BackgroundImage = global::MidoriValveTest.Properties.Resources.btnDisa2;
            this.Cancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Cancel.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.Cancel.FlatAppearance.BorderSize = 0;
            this.Cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Cancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Cancel.ForeColor = System.Drawing.Color.SteelBlue;
            this.Cancel.Location = new System.Drawing.Point(277, 124);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(104, 40);
            this.Cancel.TabIndex = 16;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = false;
            this.Cancel.Click += new System.EventHandler(this.button1_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::MidoriValveTest.Properties.Resources.MIDORI_OFICIAL3;
            this.pictureBox1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureBox1.Location = new System.Drawing.Point(399, 2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(117, 49);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 19;
            this.pictureBox1.TabStop = false;
            // 
            // unit_form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.SteelBlue;
            this.ClientSize = new System.Drawing.Size(517, 219);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Save);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.unit_scale);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "unit_form";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Unit configuration. ";
            this.Load += new System.EventHandler(this.unit_form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox unit_scale;
        private System.Windows.Forms.Button Save;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}