/**************************************************************************
*                           MIT License
* 
* Copyright (C) 2017 Frederic Chaxel <fchaxel@free.fr>
*
* Permission is hereby granted, free of charge, to any person obtaining
* a copy of this software and associated documentation files (the
* "Software"), to deal in the Software without restriction, including
* without limitation the rights to use, copy, modify, merge, publish,
* distribute, sublicense, and/or sell copies of the Software, and to
* permit persons to whom the Software is furnished to do so, subject to
* the following conditions:
*
* The above copyright notice and this permission notice shall be included
* in all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
* IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
* CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
* TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
* SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*
*********************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.EnIPStack;
using System.CodeDom.Compiler;

namespace EnIPExplorer
{
    public partial class DecoderEditor : Form
    {
        CodeDomProvider SyntaxValider = CodeDomProvider.CreateProvider("C#");

        public DecoderEditor(List<UserType> UserTypeList)
        {
            InitializeComponent();
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;

            // Associate the last colum with the CIPType enum
            AType.DataSource = Enum.GetValues(typeof(CIPType));
            AType.ValueType = typeof(CIPType);

            AttGridView.DataError += new DataGridViewDataErrorEventHandler(GridView_DataError);

            // Put all type in the Grid
            foreach (UserType t in UserTypeList)
            {
                int rowid=AttGridView.Rows.Add(new object[] { t, null, null });
                AttGridView.Rows[rowid].Cells[1].ReadOnly = true;
                AttGridView.Rows[rowid].Cells[2].ReadOnly = true;

                foreach (UserAttribut ua in t.Lattr)
                    AttGridView.Rows.Add(new object[] { null, ua.name, ua.type });
            }
        }

        // Build back a UserType List with the grid content
        List<UserType> GetEdition()
        {
            List<UserType> UserTypeList = new List<UserType>();
            UserType? ut=null;

            foreach (DataGridViewRow dtr in AttGridView.Rows)
            {
                try
                {
                    if ((dtr.Cells[0].Value != null) && (dtr.Cells[0].Value.ToString() != ""))
                    {
                        if (ut != null)
                            UserTypeList.Add(ut.Value);
                        ut = new UserType(dtr.Cells[0].Value.ToString());
                    }
                    else
                        if ((dtr.Cells[1].Value != null) && (dtr.Cells[1].ToString() != ""))
                        {
                            UserAttribut ua = new UserAttribut(dtr.Cells[1].Value.ToString(), (CIPType)dtr.Cells[2].Value);
                            ut.Value.AddAtt(ua);
                        }
                }
                catch { }
            }
            if (ut != null)
                UserTypeList.Add(ut.Value);

            return UserTypeList;
        }

        void GridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
        }

        private void AttGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode==Keys.Insert)
                AttGridView.Rows.Insert(AttGridView.CurrentCell.RowIndex);
        }

        // check grid entry, modify readonly cells if required, put a CIPtype if not present
        private void AttGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            // Syntax checking
            if ((e.FormattedValue.ToString() == "") || (e.ColumnIndex == 2))
                e.Cancel = false;
            else
                e.Cancel = !SyntaxValider.IsValidIdentifier(e.FormattedValue.ToString());

            // Column 0 with value : a Type
            if ((e.ColumnIndex == 0)&&(e.FormattedValue.ToString()!="")) 
            {
                AttGridView.Rows[e.RowIndex].Cells[1].ReadOnly = true;
                AttGridView.Rows[e.RowIndex].Cells[1].Value = "";
                AttGridView.Rows[e.RowIndex].Cells[2].ReadOnly = true;
                AttGridView.Rows[e.RowIndex].Cells[2].Value = "";
            }
            // Column 0 without value : a Field
            if ((e.ColumnIndex == 0) && (e.FormattedValue.ToString() == ""))
            {
                AttGridView.Rows[e.RowIndex].Cells[1].ReadOnly = false;
                AttGridView.Rows[e.RowIndex].Cells[2].ReadOnly = false;
            }
            // Column 1 with value : Puts CIPtype.BOOL if not present
            if ((e.ColumnIndex == 1) && (AttGridView.Rows[e.RowIndex].Cells[2].Value == null))
            {
                AttGridView.Rows[e.RowIndex].Cells[2].Value = CIPType.BOOL;
            }
        }

        private void btAbort_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            List<UserType> UserTypeList = GetEdition();

            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.FileName = Properties.Settings.Default.UserAttributsDecodersFile;
                dlg.Filter = "txt|*.txt";
                if (dlg.ShowDialog(this) != System.Windows.Forms.DialogResult.OK) return;

                string filename = dlg.FileName;
                // put the maybe new filename in the settings
                Properties.Settings.Default.UserAttributsDecodersFile = filename;

                // save all
                UserType.SaveUserTypes(filename, UserTypeList);

                // bye
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();

            }
            catch 
            {
                MessageBox.Show("File Error", "EnIPExplorer", MessageBoxButtons.OK, MessageBoxIcon.Error);            
            };
        }
    }
}
