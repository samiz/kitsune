using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kitsune
{
    public partial class EditProcDefForm : Form
    {
        private EditProcDefController controller;
        int paintCount = 0;
        public EditProcDefForm()
        {
            InitializeComponent();
            this.SetControlRoundRectRegion();
            panel1.Location = new Point((this.ClientSize.Width - panel1.Width) / 2, 10);

            if (!panel1.Controls.Contains(btnErase))
            {
                this.Controls.Remove(btnErase);
                panel1.Controls.Add(btnErase);
            }
        }

        public void SetController(EditProcDefController controller)
        {
            this.controller = controller;
            this.controller.Changed += new EditProcDefControllerChangedEvent(controller_Changed);
            controller.SetGrapics(panel1.CreateGraphics());
        }

        public TextBox MakeTextBox()
        {
            TextBox tb = new TextBox();
            tb.BorderStyle = BorderStyle.FixedSingle;
            tb.Parent = panel1;
            panel1.Controls.Add(tb);
            return tb;
        }

        void controller_Changed(object sender)
        {
            controller.Redraw();
        }
        
        private void btnAddLabel_Click(object sender, EventArgs e)
        {
            controller.AddText();
        }

        private void btnAddNumericParam_Click(object sender, EventArgs e)
        {
            controller.AddArg(DataType.Number);
        }

        private void btnAddTextParam_Click(object sender, EventArgs e)
        {
            controller.AddArg(DataType.Text);
        }

        private void btnAddObjectParam_Click(object sender, EventArgs e)
        {
            controller.AddArg(DataType.Object);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void EditProcDef_FormClosed(object sender, FormClosedEventArgs e)
        {
            controller.Done();
        }

         internal Button GetEraseButton()
        {
            return btnErase;
        }

        private void EditProcDefForm_Load(object sender, EventArgs e)
        {
            // Add an initial label for the proc name
            controller.AddText();
        }

        private void btnTestPeformance_Click(object sender, EventArgs e)
        {
            controller.TestAddArgPerformance();
            MessageBox.Show("Performance test done");
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            controller.Redraw(e.Graphics);
            this.Text = "Paint # " + paintCount++;
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            controller.MouseDown(e.Location);
        }

        private void EditProcDefForm_Click(object sender, EventArgs e)
        {
            controller.AddArg(DataType.Number);
        }

    }
}
