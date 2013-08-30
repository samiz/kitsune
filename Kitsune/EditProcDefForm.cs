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
        public EditProcDefForm(EditProcDefController controller)
        {
            InitializeComponent();
            this.SetControlRoundRectRegion();
            pictureBox1.Location = new Point((this.ClientSize.Width - pictureBox1.Width) / 2, 10);
            pictureBox1.BackColor = this.BackColor;
            this.controller = controller;
            this.controller.Changed += new EditProcDefControllerChangedEvent(controller_Changed);
        }

        void controller_Changed(object sender)
        {
            pictureBox1.Invalidate();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            controller.Redraw(e.Graphics, pictureBox1.Size);
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
    }
}
