using System.Windows.Forms;

namespace GUI
{
    public partial class SelectionForm : Form
    {
        // store types
        public int SelectedTerrainType { get; private set; }
        public int SelectedRewardType { get; private set; }

        // button controls
        private GroupBox terrainGroup = null!;
        private GroupBox rewardGroup = null!;
        private Button submitButton = null!;

        public SelectionForm()
        {
            InitializeComponent();
            InitializeGUI();
        }

        // initialize form properties
        private void InitializeComponent()
        {
            this.Text = "Terrain and Reward Selection";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        // create and configure the terrain and reward groups and submit button
        private void InitializeGUI()
        {
            // setting up the terrain selection group box
            terrainGroup = new GroupBox
            {
                Text = "Select Terrain Type",
                Location = new Point(20, 20),
                Size = new Size(250, 380)
            };

            // terrain options
            string[] terrainOptions = new[]
            {
                "1. Normal peaks",
                "2. Inverted on Creation",
                "3. Inverted after Creation",
                "4. Only create Peaks around Perimeter",
                "5. Hill with blocker",
                "6. Canyon",
                "7. Hill with perimeter opening",
                "8. Terrain going top left to bottom right",
                "9. Fractal Terrain",
                "10. Inverted Perimeter Opening",
                "11. Mountain"
            };

            // loop to create a radio button for each terrain option
            for (int i = 0; i < terrainOptions.Length; i++)
            {
                RadioButton rb = new RadioButton
                {
                    Text = terrainOptions[i],  // text of radio button
                    Location = new Point(20, 30 + (i * 30)),  // position of radio button
                    Tag = i + 1,  // set the tag to the index + 1 (1-based indexing)
                    AutoSize = true,  // adjust size based on text
                    Name = $"terrainOption{i + 1}"  // unique name for each radio button
                };
                terrainGroup.Controls.Add(rb);  // add the radio button to the terrain group box
            }

            // setting up the reward selection group box
            rewardGroup = new GroupBox
            {
                Text = "Select Reward Type",
                Location = new Point(290, 20),
                Size = new Size(250, 120)
            };

            string[] rewardOptions = new[]
            {
                "1. Normal",
                "2. Centered"
            };

            // loop to create a radio button for each reward option
            for (int i = 0; i < rewardOptions.Length; i++)
            {
                RadioButton rb = new RadioButton
                {
                    Text = rewardOptions[i],
                    Location = new Point(20, 30 + (i * 30)),
                    Tag = i + 1,  // set the tag to the index + 1 (1-based indexing)
                    AutoSize = true,
                    Name = $"rewardOption{i + 1}"  // unique name for each radio button
                };
                rewardGroup.Controls.Add(rb);  // add button to group
            }

            // submit button
            submitButton = new Button
            {
                Text = "Submit",
                Location = new Point(240, 410),  // position of button
                Size = new Size(100, 30)  // size of the button
            };
            submitButton.Click += SubmitButton_Click;  // attach the click event handler to the button

            // add group to forms control
            this.Controls.AddRange(new Control[] { terrainGroup, rewardGroup, submitButton });
        }

        // handle submit button event
        private void SubmitButton_Click(object? sender, EventArgs e)
        {
            // get each category from group
            var selectedTerrainButton = terrainGroup.Controls.OfType<RadioButton>()
                .FirstOrDefault(r => r.Checked);
            
            var selectedRewardButton = rewardGroup.Controls.OfType<RadioButton>()
                .FirstOrDefault(r => r.Checked);

            // if neither is selected
            if (selectedTerrainButton == null || selectedRewardButton == null)
            {
                MessageBox.Show("Please select both a terrain type and a reward type.");
                return;
            }

            // Debug output to verify selections
            Console.WriteLine($"Selected Terrain Type: {selectedTerrainButton.Text} (Value: {selectedTerrainButton.Tag})");
            Console.WriteLine($"Selected Reward Type: {selectedRewardButton.Text} (Value: {selectedRewardButton.Tag})");

            // set tag to selected button (ensuring we get the actual selected values)
            SelectedTerrainType = Convert.ToInt32(selectedTerrainButton.Tag);
            SelectedRewardType = Convert.ToInt32(selectedRewardButton.Tag);
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
