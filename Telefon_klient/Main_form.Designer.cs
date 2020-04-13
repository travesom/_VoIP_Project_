namespace Telefon_klient
{
    partial class Main_form
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
            this.btn_log_out = new System.Windows.Forms.Button();
            this.label_username = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btn_log_out
            // 
            this.btn_log_out.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.btn_log_out.Location = new System.Drawing.Point(654, 31);
            this.btn_log_out.Name = "btn_log_out";
            this.btn_log_out.Size = new System.Drawing.Size(89, 29);
            this.btn_log_out.TabIndex = 5;
            this.btn_log_out.Text = "log out";
            this.btn_log_out.UseVisualStyleBackColor = true;
            this.btn_log_out.Click += new System.EventHandler(this.button1_Click);
            // 
            // label_username
            // 
            this.label_username.AutoSize = true;
            this.label_username.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label_username.Location = new System.Drawing.Point(368, 9);
            this.label_username.Name = "label_username";
            this.label_username.Size = new System.Drawing.Size(0, 25);
            this.label_username.TabIndex = 6;
            // 
            // Main_form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label_username);
            this.Controls.Add(this.btn_log_out);
            this.Name = "Main_form";
            this.Text = "Main_form";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_log_out;
        private System.Windows.Forms.Label label_username;
    }
}