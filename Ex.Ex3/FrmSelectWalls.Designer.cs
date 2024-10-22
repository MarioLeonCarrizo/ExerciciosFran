namespace Ex.Ex3
{
    partial class FrmSelectWalls
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
            this.cbDoor = new System.Windows.Forms.ComboBox();
            this.cbWall = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btCrear = new System.Windows.Forms.Button();
            this.cbTextNote = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbWindows = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // cbDoor
            // 
            this.cbDoor.FormattingEnabled = true;
            this.cbDoor.Location = new System.Drawing.Point(12, 79);
            this.cbDoor.Name = "cbDoor";
            this.cbDoor.Size = new System.Drawing.Size(209, 24);
            this.cbDoor.TabIndex = 0;
            // 
            // cbWall
            // 
            this.cbWall.FormattingEnabled = true;
            this.cbWall.Location = new System.Drawing.Point(12, 33);
            this.cbWall.Name = "cbWall";
            this.cbWall.Size = new System.Drawing.Size(209, 24);
            this.cbWall.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Wall Type";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Door Type";
            // 
            // btCrear
            // 
            this.btCrear.Location = new System.Drawing.Point(146, 165);
            this.btCrear.Name = "btCrear";
            this.btCrear.Size = new System.Drawing.Size(75, 23);
            this.btCrear.TabIndex = 3;
            this.btCrear.Text = "Crear";
            this.btCrear.UseVisualStyleBackColor = true;
            this.btCrear.Click += new System.EventHandler(this.btCrear_Click);
            // 
            // cbTextNote
            // 
            this.cbTextNote.AutoSize = true;
            this.cbTextNote.Location = new System.Drawing.Point(15, 165);
            this.cbTextNote.Name = "cbTextNote";
            this.cbTextNote.Size = new System.Drawing.Size(130, 20);
            this.cbTextNote.TabIndex = 4;
            this.cbTextNote.Text = "Create Text Note";
            this.cbTextNote.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 107);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(97, 16);
            this.label3.TabIndex = 6;
            this.label3.Text = "Windows Type";
            // 
            // cbWindows
            // 
            this.cbWindows.FormattingEnabled = true;
            this.cbWindows.Location = new System.Drawing.Point(12, 126);
            this.cbWindows.Name = "cbWindows";
            this.cbWindows.Size = new System.Drawing.Size(209, 24);
            this.cbWindows.TabIndex = 5;
            // 
            // FrmSelectWalls
            // 
            this.AcceptButton = this.btCrear;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(239, 197);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbWindows);
            this.Controls.Add(this.cbTextNote);
            this.Controls.Add(this.btCrear);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbWall);
            this.Controls.Add(this.cbDoor);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmSelectWalls";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.FrmSelectWalls_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbDoor;
        private System.Windows.Forms.ComboBox cbWall;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btCrear;
        private System.Windows.Forms.CheckBox cbTextNote;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbWindows;
    }
}