using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EnIPExplorer
{
    public partial class GetId : Form
    {
        public GetId(String LblText)
        {

            InitializeComponent();
            DialogResult = DialogResult.Cancel;
            label1.Text = LblText;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
