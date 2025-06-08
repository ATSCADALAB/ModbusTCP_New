
namespace ModbusTCP
{
    partial class ctlChannelAddress
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.grbChannelAddress = new System.Windows.Forms.GroupBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblLifetime = new System.Windows.Forms.Label();
            this.txtLifetime = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.grbChannelAddress.SuspendLayout();
            this.SuspendLayout();
            // 
            // grbChannelAddress
            // 
            this.grbChannelAddress.Controls.Add(this.lblDescription);
            this.grbChannelAddress.Controls.Add(this.lblLifetime);
            this.grbChannelAddress.Controls.Add(this.txtLifetime);
            this.grbChannelAddress.Controls.Add(this.btnOK);
            this.grbChannelAddress.Location = new System.Drawing.Point(8, 2);
            this.grbChannelAddress.Name = "grbChannelAddress";
            this.grbChannelAddress.Size = new System.Drawing.Size(376, 140);
            this.grbChannelAddress.TabIndex = 10;
            this.grbChannelAddress.TabStop = false;
            // 
            // lblDescription
            // 
            this.lblDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblDescription.Location = new System.Drawing.Point(16, 57);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(344, 67);
            this.lblDescription.TabIndex = 28;
            this.lblDescription.Text = "How long (second) a connection lives before it is killed and recreated. A lifetim" +
    "e of 0 means never kill and recreate.";
            // 
            // lblLifetime
            // 
            this.lblLifetime.AutoSize = true;
            this.lblLifetime.Location = new System.Drawing.Point(13, 26);
            this.lblLifetime.Name = "lblLifetime";
            this.lblLifetime.Size = new System.Drawing.Size(114, 13);
            this.lblLifetime.TabIndex = 23;
            this.lblLifetime.Text = "Connection Lifetime (s)";
            // 
            // txtLifetime
            // 
            this.txtLifetime.Location = new System.Drawing.Point(133, 23);
            this.txtLifetime.Name = "txtLifetime";
            this.txtLifetime.Size = new System.Drawing.Size(134, 20);
            this.txtLifetime.TabIndex = 22;
            this.txtLifetime.Text = "3600";
            this.txtLifetime.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnOK
            // 
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOK.Location = new System.Drawing.Point(277, 21);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(83, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // ctlChannelAddress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grbChannelAddress);
            this.Name = "ctlChannelAddress";
            this.Size = new System.Drawing.Size(393, 150);
            this.grbChannelAddress.ResumeLayout(false);
            this.grbChannelAddress.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grbChannelAddress;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblLifetime;
        private System.Windows.Forms.TextBox txtLifetime;
        private System.Windows.Forms.Button btnOK;
    }
}
