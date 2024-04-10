namespace Ex.Ex2
{
    partial class FrmSelectorVistas
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
            this.btViewSheet = new System.Windows.Forms.Button();
            this.btSelectAll = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btViewSheet
            // 
            this.btViewSheet.Location = new System.Drawing.Point(716, 12);
            this.btViewSheet.Name = "btViewSheet";
            this.btViewSheet.Size = new System.Drawing.Size(109, 31);
            this.btViewSheet.TabIndex = 2;
            this.btViewSheet.Text = "View Image";
            this.btViewSheet.UseVisualStyleBackColor = true;
            this.btViewSheet.Click += new System.EventHandler(this.btViewSheet_Click);
            // 
            // btSelectAll
            // 
            this.btSelectAll.Location = new System.Drawing.Point(731, 49);
            this.btSelectAll.Name = "btSelectAll";
            this.btSelectAll.Size = new System.Drawing.Size(75, 23);
            this.btSelectAll.TabIndex = 3;
            this.btSelectAll.Text = "Select All";
            this.btSelectAll.UseVisualStyleBackColor = true;
            this.btSelectAll.Click += new System.EventHandler(this.btSelectAll_Click);
            // 
            // FrmSelectorVistas
            // 
            this.AcceptButton = this.btViewSheet;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(832, 563);
            this.Controls.Add(this.btSelectAll);
            this.Controls.Add(this.btViewSheet);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmSelectorVistas";
            this.Text = "FrmSelectorVistas";
            this.Load += new System.EventHandler(this.FrmSelectorVistas_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btViewSheet;
        private System.Windows.Forms.Button btSelectAll;
    }
}