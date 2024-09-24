// files to create gui
using System.Windows.Forms;

namespace GUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        bool[] terrains;

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        // handle events for terrain checkboxes
        private void TerrainCheckList_MouseClick(object sender, MouseEventArgs e)
        {
            terrains = new bool[TerrainCheckList.Items.Count];

            // set all booleans to false
            for (int i = 0; i < terrains.Length; i++)
            {
                terrains[i] = false;
            }

            // only 1 terrain can be checked
            int idx = TerrainCheckList.SelectedIndex;

            for (int i = 0; i < TerrainCheckList.Items.Count; i++)
            {
                terrains[idx] = true;
                if (i != idx)
                {
                    TerrainCheckList.SetItemChecked(i, false);
                    terrains[i] = false;
                }
            }
        }

        // handle events for reward checkboxes
        private void RewardCheckList_MouseClick(object sender, MouseEventArgs e)
        {
            int indexCheck = RewardCheckList.SelectedIndex;

            for (int i = 0; i < RewardCheckList.Items.Count; ++i)
                if (i != indexCheck)
                {
                    RewardCheckList.SetItemChecked(i, false);
                }
        }

        // handle events for behavior checkboxes
        private void BehaviorsCheckList_MouseClick(object sender, MouseEventArgs e)
        {
            // climb and descend cannot be checked at the same time but collect can be for both
            int idx = BehaviorsCheckList.SelectedIndex;

            if (idx == 0)
            {
                BehaviorsCheckList.SetItemChecked(1, false);
            }
            else if (idx == 1)
            {
                BehaviorsCheckList.SetItemChecked(0, false);
            }
        }

        // gather info on checklist then run test
        private void StartButton_MouseClick(object sender, MouseEventArgs e)
        {

        }
    }
}
