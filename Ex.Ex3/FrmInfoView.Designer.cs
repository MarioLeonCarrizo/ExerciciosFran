namespace Ex.Ex3
{
    partial class FrmInfoView
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
            this.lbWidths = new System.Windows.Forms.ListBox();
            this.pnlDraw = new System.Windows.Forms.Panel();
            this.btCreate = new System.Windows.Forms.Button();
            this.cbTypes = new System.Windows.Forms.ComboBox();
            this.lbNameType = new System.Windows.Forms.Label();
            this.btWalls = new System.Windows.Forms.Button();
            this.btDoors = new System.Windows.Forms.Button();
            this.btWindows = new System.Windows.Forms.Button();
            this.btWallsArc = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbWidths
            // 
            this.lbWidths.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbWidths.FormattingEnabled = true;
            this.lbWidths.ItemHeight = 29;
            this.lbWidths.Location = new System.Drawing.Point(12, 48);
            this.lbWidths.Name = "lbWidths";
            this.lbWidths.Size = new System.Drawing.Size(135, 410);
            this.lbWidths.TabIndex = 0;
            this.lbWidths.SelectedIndexChanged += new System.EventHandler(this.lbWidth_SelectedIndexChanged);
            // 
            // pnlDraw
            // 
            this.pnlDraw.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlDraw.BackColor = System.Drawing.SystemColors.ControlLight;
            this.pnlDraw.Location = new System.Drawing.Point(153, 127);
            this.pnlDraw.Name = "pnlDraw";
            this.pnlDraw.Size = new System.Drawing.Size(846, 325);
            this.pnlDraw.TabIndex = 1;
            this.pnlDraw.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlDraw_Paint);
            // 
            // btCreate
            // 
            this.btCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btCreate.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btCreate.Location = new System.Drawing.Point(893, 48);
            this.btCreate.Name = "btCreate";
            this.btCreate.Size = new System.Drawing.Size(106, 73);
            this.btCreate.TabIndex = 2;
            this.btCreate.Text = "Crear";
            this.btCreate.UseVisualStyleBackColor = true;
            this.btCreate.Click += new System.EventHandler(this.btCreate_Click);
            // 
            // cbTypes
            // 
            this.cbTypes.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbTypes.FormattingEnabled = true;
            this.cbTypes.Location = new System.Drawing.Point(520, 83);
            this.cbTypes.Name = "cbTypes";
            this.cbTypes.Size = new System.Drawing.Size(366, 33);
            this.cbTypes.TabIndex = 4;
            this.cbTypes.SelectedIndexChanged += new System.EventHandler(this.cbTypes_SelectedIndexChanged);
            // 
            // lbNameType
            // 
            this.lbNameType.AutoSize = true;
            this.lbNameType.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbNameType.Location = new System.Drawing.Point(515, 48);
            this.lbNameType.Name = "lbNameType";
            this.lbNameType.Size = new System.Drawing.Size(121, 29);
            this.lbNameType.TabIndex = 5;
            this.lbNameType.Text = "Wall Type";
            // 
            // btWalls
            // 
            this.btWalls.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btWalls.Location = new System.Drawing.Point(9, 9);
            this.btWalls.Margin = new System.Windows.Forms.Padding(0);
            this.btWalls.Name = "btWalls";
            this.btWalls.Size = new System.Drawing.Size(62, 30);
            this.btWalls.TabIndex = 7;
            this.btWalls.Text = "Walls";
            this.btWalls.UseVisualStyleBackColor = true;
            this.btWalls.Click += new System.EventHandler(this.btWalls_Click);
            // 
            // btDoors
            // 
            this.btDoors.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btDoors.Location = new System.Drawing.Point(74, 9);
            this.btDoors.Name = "btDoors";
            this.btDoors.Size = new System.Drawing.Size(67, 30);
            this.btDoors.TabIndex = 8;
            this.btDoors.Text = "Doors";
            this.btDoors.UseVisualStyleBackColor = true;
            this.btDoors.Click += new System.EventHandler(this.btDoors_Click);
            // 
            // btWindows
            // 
            this.btWindows.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btWindows.Location = new System.Drawing.Point(144, 9);
            this.btWindows.Margin = new System.Windows.Forms.Padding(0);
            this.btWindows.Name = "btWindows";
            this.btWindows.Size = new System.Drawing.Size(88, 30);
            this.btWindows.TabIndex = 9;
            this.btWindows.Text = "Windows";
            this.btWindows.UseVisualStyleBackColor = true;
            this.btWindows.Click += new System.EventHandler(this.btWindows_Click);
            // 
            // btWallsArc
            // 
            this.btWallsArc.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btWallsArc.Location = new System.Drawing.Point(232, 9);
            this.btWallsArc.Margin = new System.Windows.Forms.Padding(0);
            this.btWallsArc.Name = "btWallsArc";
            this.btWallsArc.Size = new System.Drawing.Size(90, 30);
            this.btWallsArc.TabIndex = 10;
            this.btWallsArc.Text = "Walls Arc";
            this.btWallsArc.UseVisualStyleBackColor = true;
            this.btWallsArc.Click += new System.EventHandler(this.btWallsArc_Click);
            // 
            // FrmInfoView
            // 
            this.AcceptButton = this.btCreate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1006, 459);
            this.Controls.Add(this.btWallsArc);
            this.Controls.Add(this.btWindows);
            this.Controls.Add(this.btDoors);
            this.Controls.Add(this.btWalls);
            this.Controls.Add(this.lbWidths);
            this.Controls.Add(this.lbNameType);
            this.Controls.Add(this.cbTypes);
            this.Controls.Add(this.btCreate);
            this.Controls.Add(this.pnlDraw);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmInfoView";
            this.Text = "FrmInfoView";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lbWidths;
        private System.Windows.Forms.Panel pnlDraw;
        private System.Windows.Forms.Button btCreate;
        private System.Windows.Forms.ComboBox cbTypes;
        private System.Windows.Forms.Label lbNameType;
        private System.Windows.Forms.Button btWalls;
        private System.Windows.Forms.Button btDoors;
        private System.Windows.Forms.Button btWindows;
        private System.Windows.Forms.Button btWallsArc;
    }
}