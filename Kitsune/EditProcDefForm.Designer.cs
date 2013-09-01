﻿namespace Kitsune
{
    partial class EditProcDefForm
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnAddNumericParam = new System.Windows.Forms.Button();
            this.btnAddTextParam = new System.Windows.Forms.Button();
            this.btnAddObjectParam = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnAddLabel = new System.Windows.Forms.Button();
            this.btnErase = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.White;
            this.pictureBox1.Location = new System.Drawing.Point(2, 2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(390, 147);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
            // 
            // btnAddNumericParam
            // 
            this.btnAddNumericParam.Location = new System.Drawing.Point(163, 180);
            this.btnAddNumericParam.Name = "btnAddNumericParam";
            this.btnAddNumericParam.Size = new System.Drawing.Size(75, 23);
            this.btnAddNumericParam.TabIndex = 1;
            this.btnAddNumericParam.Text = "Add &Number";
            this.btnAddNumericParam.UseVisualStyleBackColor = true;
            this.btnAddNumericParam.Click += new System.EventHandler(this.btnAddNumericParam_Click);
            // 
            // btnAddTextParam
            // 
            this.btnAddTextParam.Location = new System.Drawing.Point(163, 209);
            this.btnAddTextParam.Name = "btnAddTextParam";
            this.btnAddTextParam.Size = new System.Drawing.Size(75, 23);
            this.btnAddTextParam.TabIndex = 2;
            this.btnAddTextParam.Text = "Add &Text";
            this.btnAddTextParam.UseVisualStyleBackColor = true;
            this.btnAddTextParam.Click += new System.EventHandler(this.btnAddTextParam_Click);
            // 
            // btnAddObjectParam
            // 
            this.btnAddObjectParam.Location = new System.Drawing.Point(163, 238);
            this.btnAddObjectParam.Name = "btnAddObjectParam";
            this.btnAddObjectParam.Size = new System.Drawing.Size(75, 23);
            this.btnAddObjectParam.TabIndex = 3;
            this.btnAddObjectParam.Text = "Add &Object";
            this.btnAddObjectParam.UseVisualStyleBackColor = true;
            this.btnAddObjectParam.Click += new System.EventHandler(this.btnAddObjectParam_Click);
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(13, 208);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 4;
            this.btnOk.Text = "&Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(13, 237);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnAddLabel
            // 
            this.btnAddLabel.Location = new System.Drawing.Point(163, 155);
            this.btnAddLabel.Name = "btnAddLabel";
            this.btnAddLabel.Size = new System.Drawing.Size(75, 23);
            this.btnAddLabel.TabIndex = 6;
            this.btnAddLabel.Text = "Add &Label";
            this.btnAddLabel.UseVisualStyleBackColor = true;
            this.btnAddLabel.Click += new System.EventHandler(this.btnAddLabel_Click);
            // 
            // btnErase
            // 
            this.btnErase.AutoSize = true;
            this.btnErase.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnErase.BackColor = System.Drawing.Color.Transparent;
            this.btnErase.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnErase.FlatAppearance.BorderSize = 0;
            this.btnErase.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnErase.Image = global::Kitsune.Properties.Resources.eraseButton;
            this.btnErase.Location = new System.Drawing.Point(73, 71);
            this.btnErase.Name = "btnErase";
            this.btnErase.Size = new System.Drawing.Size(16, 16);
            this.btnErase.TabIndex = 7;
            this.btnErase.UseVisualStyleBackColor = false;
            // 
            // EditProcDefForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gainsboro;
            this.ClientSize = new System.Drawing.Size(404, 273);
            this.Controls.Add(this.btnErase);
            this.Controls.Add(this.btnAddLabel);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnAddObjectParam);
            this.Controls.Add(this.btnAddTextParam);
            this.Controls.Add(this.btnAddNumericParam);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "EditProcDefForm";
            this.Text = "Define a new procedure";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EditProcDef_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnAddNumericParam;
        private System.Windows.Forms.Button btnAddTextParam;
        private System.Windows.Forms.Button btnAddObjectParam;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnAddLabel;
        private System.Windows.Forms.Button btnErase;
    }
}