namespace Telefon_klient
{
    partial class Form1
    {
        /// <summary>
        /// Wymagana zmienna projektanta.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Wyczyść wszystkie używane zasoby.
        /// </summary>
        /// <param name="disposing">prawda, jeżeli zarządzane zasoby powinny zostać zlikwidowane; Fałsz w przeciwnym wypadku.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kod generowany przez Projektanta formularzy systemu Windows

        /// <summary>
        /// Metoda wymagana do obsługi projektanta — nie należy modyfikować
        /// jej zawartości w edytorze kodu.
        /// </summary>
        private void InitializeComponent()
        {
            this.txt_login = new System.Windows.Forms.TextBox();
            this.txt_pass = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btn_Sign_in = new System.Windows.Forms.Button();
            this.btn_register = new System.Windows.Forms.Button();
            this.txt_server_add = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txt_login
            // 
            this.txt_login.Location = new System.Drawing.Point(471, 119);
            this.txt_login.Name = "txt_login";
            this.txt_login.Size = new System.Drawing.Size(209, 22);
            this.txt_login.TabIndex = 0;
            // 
            // txt_pass
            // 
            this.txt_pass.Location = new System.Drawing.Point(471, 223);
            this.txt_pass.Name = "txt_pass";
            this.txt_pass.Size = new System.Drawing.Size(209, 22);
            this.txt_pass.TabIndex = 1;
            this.txt_pass.UseSystemPasswordChar = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label1.Location = new System.Drawing.Point(348, 124);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "Login";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label2.Location = new System.Drawing.Point(348, 223);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "Password";
            // 
            // btn_Sign_in
            // 
            this.btn_Sign_in.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.btn_Sign_in.Location = new System.Drawing.Point(568, 311);
            this.btn_Sign_in.Name = "btn_Sign_in";
            this.btn_Sign_in.Size = new System.Drawing.Size(89, 29);
            this.btn_Sign_in.TabIndex = 4;
            this.btn_Sign_in.Text = "Sign in";
            this.btn_Sign_in.UseVisualStyleBackColor = true;
            this.btn_Sign_in.Click += new System.EventHandler(this.btn_Sign_in_Click);
            // 
            // btn_register
            // 
            this.btn_register.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.btn_register.Location = new System.Drawing.Point(52, 321);
            this.btn_register.Name = "btn_register";
            this.btn_register.Size = new System.Drawing.Size(89, 29);
            this.btn_register.TabIndex = 5;
            this.btn_register.Text = "Register";
            this.btn_register.UseVisualStyleBackColor = true;
            this.btn_register.Click += new System.EventHandler(this.btn_register_Click);
            // 
            // txt_server_add
            // 
            this.txt_server_add.Location = new System.Drawing.Point(328, 30);
            this.txt_server_add.Name = "txt_server_add";
            this.txt_server_add.Size = new System.Drawing.Size(209, 22);
            this.txt_server_add.TabIndex = 6;
            this.txt_server_add.Text = "127.0.0.1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label3.Location = new System.Drawing.Point(183, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(125, 20);
            this.label3.TabIndex = 7;
            this.label3.Text = "Server Address";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txt_server_add);
            this.Controls.Add(this.btn_register);
            this.Controls.Add(this.btn_Sign_in);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txt_pass);
            this.Controls.Add(this.txt_login);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_login;
        private System.Windows.Forms.TextBox txt_pass;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btn_Sign_in;
        private System.Windows.Forms.Button btn_register;
        private System.Windows.Forms.TextBox txt_server_add;
        private System.Windows.Forms.Label label3;
    }
}

