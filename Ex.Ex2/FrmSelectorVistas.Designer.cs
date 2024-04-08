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
            this.pbPlanoView = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbPlanoView)).BeginInit();
            this.SuspendLayout();
            // 
            // pbPlanoView
            // 
            this.pbPlanoView.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.pbPlanoView.Location = new System.Drawing.Point(371, 12);
            this.pbPlanoView.Name = "pbPlanoView";
            this.pbPlanoView.Size = new System.Drawing.Size(566, 426);
            this.pbPlanoView.TabIndex = 1;
            this.pbPlanoView.TabStop = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(371, 444);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(109, 31);
            this.button1.TabIndex = 2;
            this.button1.Text = "View Image";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // FrmSelectorVistas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(949, 487);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.pbPlanoView);
            this.Name = "FrmSelectorVistas";
            this.Text = "FrmSelectorVistas";
            this.Load += new System.EventHandler(this.FrmSelectorVistas_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbPlanoView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.PictureBox pbPlanoView;
        private System.Windows.Forms.Button button1;
    }
}