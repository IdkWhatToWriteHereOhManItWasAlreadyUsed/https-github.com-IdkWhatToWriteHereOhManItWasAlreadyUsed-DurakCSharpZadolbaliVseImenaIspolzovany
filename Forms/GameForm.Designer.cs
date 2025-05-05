namespace Durak_.Forms
{
    partial class GameForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GameForm));
            pbGameField = new PictureBox();
            btnMoveTransfer = new Button();
            btnGrab = new Button();
            btnDecCradsPage = new Button();
            btnIncCardsPage = new Button();
            ((System.ComponentModel.ISupportInitialize)pbGameField).BeginInit();
            SuspendLayout();
            // 
            // pbGameField
            // 
            pbGameField.Location = new Point(28, 12);
            pbGameField.Name = "pbGameField";
            pbGameField.Size = new Size(800, 600);
            pbGameField.TabIndex = 0;
            pbGameField.TabStop = false;
            // 
            // btnMoveTransfer
            // 
            btnMoveTransfer.BackColor = Color.GreenYellow;
            btnMoveTransfer.ForeColor = SystemColors.ActiveCaptionText;
            btnMoveTransfer.Location = new Point(845, 303);
            btnMoveTransfer.Name = "btnMoveTransfer";
            btnMoveTransfer.Size = new Size(157, 31);
            btnMoveTransfer.TabIndex = 1;
            btnMoveTransfer.Text = "Передать ход";
            btnMoveTransfer.UseVisualStyleBackColor = false;
            // 
            // btnGrab
            // 
            btnGrab.BackColor = Color.Red;
            btnGrab.ForeColor = SystemColors.ButtonHighlight;
            btnGrab.Location = new Point(845, 250);
            btnGrab.Name = "btnGrab";
            btnGrab.Size = new Size(157, 34);
            btnGrab.TabIndex = 2;
            btnGrab.Text = "Грести";
            btnGrab.UseVisualStyleBackColor = false;
            // 
            // btnDecCradsPage
            // 
            btnDecCradsPage.Image = (Image)resources.GetObject("btnDecCradsPage.Image");
            btnDecCradsPage.Location = new Point(17, 634);
            btnDecCradsPage.Name = "btnDecCradsPage";
            btnDecCradsPage.Size = new Size(403, 85);
            btnDecCradsPage.TabIndex = 3;
            btnDecCradsPage.Tag = "228";
            btnDecCradsPage.UseVisualStyleBackColor = true;
            // 
            // btnIncCardsPage
            // 
            btnIncCardsPage.Image = (Image)resources.GetObject("btnIncCardsPage.Image");
            btnIncCardsPage.Location = new Point(426, 634);
            btnIncCardsPage.Name = "btnIncCardsPage";
            btnIncCardsPage.Size = new Size(416, 85);
            btnIncCardsPage.TabIndex = 4;
            btnIncCardsPage.Tag = "1";
            btnIncCardsPage.UseVisualStyleBackColor = true;
            // 
            // GameForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1014, 731);
            Controls.Add(btnIncCardsPage);
            Controls.Add(btnDecCradsPage);
            Controls.Add(btnGrab);
            Controls.Add(btnMoveTransfer);
            Controls.Add(pbGameField);
            Name = "GameForm";
            Text = "GameForm";
            Load += GameForm_Load;
            Shown += GameForm_Shown;
            ((System.ComponentModel.ISupportInitialize)pbGameField).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pbGameField;
        private Button btnMoveTransfer;
        private Button btnGrab;
        private Button btnDecCradsPage;
        private Button btnIncCardsPage;
    }
}