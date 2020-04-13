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
            this.rad_status_ready = new System.Windows.Forms.RadioButton();
            this.rad_status_busy = new System.Windows.Forms.RadioButton();
            this.rad_status_unavaible = new System.Windows.Forms.RadioButton();
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
            // rad_status_ready
            // 
            this.rad_status_ready.AutoSize = true;
            this.rad_status_ready.Location = new System.Drawing.Point(12, 14);
            this.rad_status_ready.Name = "rad_status_ready";
            this.rad_status_ready.Size = new System.Drawing.Size(70, 21);
            this.rad_status_ready.TabIndex = 7;
            this.rad_status_ready.TabStop = true;
            this.rad_status_ready.Text = "Ready";
            this.rad_status_ready.UseVisualStyleBackColor = true;
            this.rad_status_ready.CheckedChanged += new System.EventHandler(this.rad_status_ready_CheckedChanged);
            // 
            // rad_status_busy
            // 
            this.rad_status_busy.AutoSize = true;
            this.rad_status_busy.Location = new System.Drawing.Point(12, 41);
            this.rad_status_busy.Name = "rad_status_busy";
            this.rad_status_busy.Size = new System.Drawing.Size(60, 21);
            this.rad_status_busy.TabIndex = 8;
            this.rad_status_busy.TabStop = true;
            this.rad_status_busy.Text = "Busy";
            this.rad_status_busy.UseVisualStyleBackColor = true;
            this.rad_status_busy.CheckedChanged += new System.EventHandler(this.rad_status_busy_CheckedChanged);
            // 
            // rad_status_unavaible
            // 
            this.rad_status_unavaible.AutoSize = true;
            this.rad_status_unavaible.Location = new System.Drawing.Point(12, 68);
            this.rad_status_unavaible.Name = "rad_status_unavaible";
            this.rad_status_unavaible.Size = new System.Drawing.Size(92, 21);
            this.rad_status_unavaible.TabIndex = 9;
            this.rad_status_unavaible.TabStop = true;
            this.rad_status_unavaible.Text = "Unavaible";
            this.rad_status_unavaible.UseVisualStyleBackColor = true;
            this.rad_status_unavaible.CheckedChanged += new System.EventHandler(this.rad_status_unavaible_CheckedChanged);
            // 
            // Main_form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.rad_status_unavaible);
            this.Controls.Add(this.rad_status_busy);
            this.Controls.Add(this.rad_status_ready);
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
        private System.Windows.Forms.RadioButton rad_status_ready;
        private System.Windows.Forms.RadioButton rad_status_busy;
        private System.Windows.Forms.RadioButton rad_status_unavaible;
    }
}