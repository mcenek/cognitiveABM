namespace GUI
{
    partial class Form1
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
            label2 = new Label();
            Label3 = new Label();
            TerrainCheckList = new CheckedListBox();
            RewardCheckList = new CheckedListBox();
            BehaviorsCheckList = new CheckedListBox();
            StartButton = new Button();
            TestOutPuts = new TextBox();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(133, 75);
            label1.Name = "label1";
            label1.Size = new Size(75, 30);
            label1.TabIndex = 0;
            label1.Text = "Terrain";
            label1.Click += label1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label2.Location = new Point(365, 75);
            label2.Name = "label2";
            label2.Size = new Size(81, 30);
            label2.TabIndex = 1;
            label2.Text = "Reward";
            label2.Click += label2_Click;
            // 
            // Label3
            // 
            Label3.AutoSize = true;
            Label3.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Label3.Location = new Point(567, 75);
            Label3.Name = "Label3";
            Label3.Size = new Size(102, 30);
            Label3.TabIndex = 2;
            Label3.Text = "Behaviors\r\n";
            Label3.Click += label3_Click;
            // 
            // TerrainCheckList
            // 
            TerrainCheckList.BackColor = SystemColors.Control;
            TerrainCheckList.FormattingEnabled = true;
            TerrainCheckList.Items.AddRange(new object[] { "Normal Peaks", "Inverted on Creation", "Inverted After Creation", "Only create Peaks around Perimeter", "Hill with Blocker", "Canyon", "Hill with Perimeter Opening", "Terrain going top Left to Bottom Right" });
            TerrainCheckList.Location = new Point(64, 108);
            TerrainCheckList.Name = "TerrainCheckList";
            TerrainCheckList.Size = new Size(232, 148);
            TerrainCheckList.TabIndex = 3;
            TerrainCheckList.MouseClick += TerrainCheckList_MouseClick;
            // 
            // RewardCheckList
            // 
            RewardCheckList.BackColor = SystemColors.Control;
            RewardCheckList.FormattingEnabled = true;
            RewardCheckList.Items.AddRange(new object[] { "Centered", "Spread Out" });
            RewardCheckList.Location = new Point(338, 108);
            RewardCheckList.Name = "RewardCheckList";
            RewardCheckList.Size = new Size(157, 148);
            RewardCheckList.TabIndex = 4;
            RewardCheckList.MouseClick += RewardCheckList_MouseClick;
            // 
            // BehaviorsCheckList
            // 
            BehaviorsCheckList.BackColor = SystemColors.Control;
            BehaviorsCheckList.FormattingEnabled = true;
            BehaviorsCheckList.Items.AddRange(new object[] { "Climb", "Descend", "Collect Food" });
            BehaviorsCheckList.Location = new Point(549, 108);
            BehaviorsCheckList.Name = "BehaviorsCheckList";
            BehaviorsCheckList.Size = new Size(175, 148);
            BehaviorsCheckList.TabIndex = 5;
            BehaviorsCheckList.MouseClick += BehaviorsCheckList_MouseClick;
            // 
            // StartButton
            // 
            StartButton.Location = new Point(356, 291);
            StartButton.Name = "StartButton";
            StartButton.Size = new Size(100, 53);
            StartButton.TabIndex = 6;
            StartButton.Text = "Start";
            StartButton.UseVisualStyleBackColor = true;
            StartButton.MouseClick += StartButton_MouseClick;
            // 
            // TestOutPuts
            // 
            TestOutPuts.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            TestOutPuts.Location = new Point(153, 366);
            TestOutPuts.Name = "TestOutPuts";
            TestOutPuts.Size = new Size(516, 29);
            TestOutPuts.TabIndex = 7;
            TestOutPuts.Text = "\r\n";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(TestOutPuts);
            Controls.Add(StartButton);
            Controls.Add(BehaviorsCheckList);
            Controls.Add(RewardCheckList);
            Controls.Add(TerrainCheckList);
            Controls.Add(Label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "Form1";
            Text = "CognitiveABM";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label Label3;
        private CheckedListBox TerrainCheckList;
        private CheckedListBox RewardCheckList;
        private CheckedListBox BehaviorsCheckList;
        private Button StartButton;
        private TextBox TestOutPuts;
    }
}
