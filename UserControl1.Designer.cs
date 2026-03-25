namespace Debitos
{
    partial class UserControl1
    {
        /// <summary> 
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            txtUsuario = new TextBox();
            txtClave = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            button1 = new Button();
            lblUsuarioIncorrecto = new Label();
            lblClaveIncorrecta = new Label();
            SuspendLayout();
            // 
            // txtUsuario
            // 
            txtUsuario.Font = new Font("Segoe UI", 12F);
            txtUsuario.Location = new Point(113, 113);
            txtUsuario.Name = "txtUsuario";
            txtUsuario.Size = new Size(202, 29);
            txtUsuario.TabIndex = 0;
            txtUsuario.TextChanged += txtUsuario_TextChanged;
            // 
            // txtClave
            // 
            txtClave.Font = new Font("Segoe UI", 12F);
            txtClave.Location = new Point(113, 214);
            txtClave.Name = "txtClave";
            txtClave.PasswordChar = '*';
            txtClave.Size = new Size(202, 29);
            txtClave.TabIndex = 1;
            txtClave.UseSystemPasswordChar = true;
            txtClave.TextChanged += txtClave_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F);
            label1.Location = new Point(18, 116);
            label1.Name = "label1";
            label1.Size = new Size(64, 21);
            label1.TabIndex = 2;
            label1.Text = "Usuario";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F);
            label2.Location = new Point(18, 217);
            label2.Name = "label2";
            label2.Size = new Size(89, 21);
            label2.TabIndex = 3;
            label2.Text = "Contraseña";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            label3.Location = new Point(113, 44);
            label3.Name = "label3";
            label3.Size = new Size(136, 28);
            label3.TabIndex = 4;
            label3.Text = "Iniciar sesión";
            // 
            // button1
            // 
            button1.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            button1.Location = new Point(38, 297);
            button1.Name = "button1";
            button1.Size = new Size(252, 47);
            button1.TabIndex = 5;
            button1.Text = "Ingresar";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // lblUsuarioIncorrecto
            // 
            lblUsuarioIncorrecto.AutoSize = true;
            lblUsuarioIncorrecto.ForeColor = Color.Red;
            lblUsuarioIncorrecto.Location = new Point(113, 161);
            lblUsuarioIncorrecto.Name = "lblUsuarioIncorrecto";
            lblUsuarioIncorrecto.Size = new Size(0, 15);
            lblUsuarioIncorrecto.TabIndex = 6;
            // 
            // lblClaveIncorrecta
            // 
            lblClaveIncorrecta.AutoSize = true;
            lblClaveIncorrecta.ForeColor = Color.Red;
            lblClaveIncorrecta.Location = new Point(113, 259);
            lblClaveIncorrecta.Name = "lblClaveIncorrecta";
            lblClaveIncorrecta.Size = new Size(0, 15);
            lblClaveIncorrecta.TabIndex = 7;
            // 
            // UserControl1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(lblClaveIncorrecta);
            Controls.Add(lblUsuarioIncorrecto);
            Controls.Add(button1);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtClave);
            Controls.Add(txtUsuario);
            Name = "UserControl1";
            Size = new Size(439, 407);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtUsuario;
        private TextBox txtClave;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button button1;
        private Label lblUsuarioIncorrecto;
        private Label lblClaveIncorrecta;
    }
}
