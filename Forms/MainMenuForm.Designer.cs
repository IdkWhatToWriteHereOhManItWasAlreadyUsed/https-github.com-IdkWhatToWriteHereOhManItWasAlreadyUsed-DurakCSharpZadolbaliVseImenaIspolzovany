namespace Durak_
{
    partial class MainMenuForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            NameTextBox = new TextBox();
            PlayersGrid = new DataGridView();
            Players = new DataGridViewTextBoxColumn();
            Connect = new DataGridViewTextBoxColumn();
            button1 = new Button();
            label2 = new Label();
            button2 = new Button();
            ((System.ComponentModel.ISupportInitialize)PlayersGrid).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 56);
            label1.Name = "label1";
            label1.Size = new Size(216, 15);
            label1.TabIndex = 7;
            label1.Text = "Имя, отображаемое у других игроков";
            // 
            // NameTextBox
            // 
            NameTextBox.Location = new Point(12, 30);
            NameTextBox.Name = "NameTextBox";
            NameTextBox.Size = new Size(216, 23);
            NameTextBox.TabIndex = 6;
            // 
            // PlayersGrid
            // 
            PlayersGrid.AllowUserToAddRows = false;
            PlayersGrid.AllowUserToDeleteRows = false;
            PlayersGrid.AllowUserToResizeColumns = false;
            PlayersGrid.AllowUserToResizeRows = false;
            PlayersGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            PlayersGrid.ColumnHeadersVisible = false;
            PlayersGrid.Columns.AddRange(new DataGridViewColumn[] { Players, Connect });
            PlayersGrid.Location = new Point(12, 108);
            PlayersGrid.Name = "PlayersGrid";
            PlayersGrid.ReadOnly = true;
            PlayersGrid.RowHeadersVisible = false;
            PlayersGrid.Size = new Size(392, 300);
            PlayersGrid.TabIndex = 10;
            PlayersGrid.TabStop = false;
            PlayersGrid.CellContentClick += PlayersGrid_CellContentClick;
            // 
            // Players
            // 
            Players.HeaderText = "Column1";
            Players.Name = "Players";
            Players.ReadOnly = true;
            // 
            // Connect
            // 
            Connect.HeaderText = "Column1";
            Connect.Name = "Connect";
            Connect.ReadOnly = true;
            // 
            // button1
            // 
            button1.Location = new Point(262, 30);
            button1.Name = "button1";
            button1.Size = new Size(142, 23);
            button1.TabIndex = 11;
            button1.Text = "Начать поиск игроков";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click_1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 90);
            label2.Name = "label2";
            label2.Size = new Size(110, 15);
            label2.TabIndex = 12;
            label2.Text = "Доступные игроки";
            // 
            // button2
            // 
            button2.Location = new Point(12, 418);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 13;
            button2.Text = "button2";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // MainMenuForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(428, 453);
            Controls.Add(button2);
            Controls.Add(label2);
            Controls.Add(button1);
            Controls.Add(PlayersGrid);
            Controls.Add(label1);
            Controls.Add(NameTextBox);
            Name = "MainMenuForm";
            Text = "Главное меню";
            Load += MainMenuForm_Load;
            ((System.ComponentModel.ISupportInitialize)PlayersGrid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label1;
        private TextBox NameTextBox;
        private DataGridView PlayersGrid;
        private Button button1;
        private Label label2;
        private DataGridViewTextBoxColumn Players;
        private DataGridViewTextBoxColumn Connect;
        private Button button2;
    }
}
