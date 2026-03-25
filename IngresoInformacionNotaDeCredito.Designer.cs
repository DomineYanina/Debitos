namespace Debitos
{
    partial class IngresoInformacionNotaDeCredito
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
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            txtPuntoDeVenta = new TextBox();
            txtLetra = new TextBox();
            txtNumero = new TextBox();
            dateTimePicker1 = new DateTimePicker();
            label5 = new Label();
            btnGuardar = new Button();
            label6 = new Label();
            comboTipoDeArchivo = new ComboBox();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 15F);
            label1.Location = new Point(27, 151);
            label1.Name = "label1";
            label1.Size = new Size(144, 28);
            label1.TabIndex = 0;
            label1.Text = "Punto de venta";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 15F);
            label2.Location = new Point(27, 218);
            label2.Name = "label2";
            label2.Size = new Size(55, 28);
            label2.TabIndex = 1;
            label2.Text = "Letra";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 15F);
            label3.Location = new Point(27, 290);
            label3.Name = "label3";
            label3.Size = new Size(84, 28);
            label3.TabIndex = 2;
            label3.Text = "Número";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 15F);
            label4.Location = new Point(27, 350);
            label4.Name = "label4";
            label4.Size = new Size(62, 28);
            label4.TabIndex = 3;
            label4.Text = "Fecha";
            // 
            // txtPuntoDeVenta
            // 
            txtPuntoDeVenta.Font = new Font("Segoe UI", 15F);
            txtPuntoDeVenta.Location = new Point(192, 148);
            txtPuntoDeVenta.Name = "txtPuntoDeVenta";
            txtPuntoDeVenta.Size = new Size(211, 34);
            txtPuntoDeVenta.TabIndex = 4;
            txtPuntoDeVenta.TextChanged += txtPuntoDeVenta_TextChanged;
            // 
            // txtLetra
            // 
            txtLetra.Font = new Font("Segoe UI", 15F);
            txtLetra.Location = new Point(192, 218);
            txtLetra.Name = "txtLetra";
            txtLetra.Size = new Size(211, 34);
            txtLetra.TabIndex = 5;
            txtLetra.TextChanged += txtLetra_TextChanged;
            // 
            // txtNumero
            // 
            txtNumero.Font = new Font("Segoe UI", 15F);
            txtNumero.Location = new Point(192, 290);
            txtNumero.Name = "txtNumero";
            txtNumero.Size = new Size(211, 34);
            txtNumero.TabIndex = 6;
            txtNumero.TextChanged += txtNumero_TextChanged;
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Location = new Point(192, 355);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new Size(211, 23);
            dateTimePicker1.TabIndex = 7;
            dateTimePicker1.ValueChanged += dateTimePicker1_ValueChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 15F, FontStyle.Underline, GraphicsUnit.Point, 0);
            label5.Location = new Point(40, 32);
            label5.Name = "label5";
            label5.Size = new Size(358, 28);
            label5.TabIndex = 8;
            label5.Text = "Ingresar información de nota de crédito";
            // 
            // btnGuardar
            // 
            btnGuardar.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnGuardar.Location = new Point(27, 403);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(376, 37);
            btnGuardar.TabIndex = 9;
            btnGuardar.Text = "Guardar";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 15F);
            label6.Location = new Point(27, 83);
            label6.Name = "label6";
            label6.Size = new Size(150, 28);
            label6.TabIndex = 10;
            label6.Text = "Tipo de Archivo";
            // 
            // comboTipoDeArchivo
            // 
            comboTipoDeArchivo.FormattingEnabled = true;
            comboTipoDeArchivo.Items.AddRange(new object[] { "NC", "NCE" });
            comboTipoDeArchivo.Location = new Point(192, 88);
            comboTipoDeArchivo.Name = "comboTipoDeArchivo";
            comboTipoDeArchivo.Size = new Size(211, 23);
            comboTipoDeArchivo.TabIndex = 11;
            comboTipoDeArchivo.SelectedIndexChanged += comboTipoDeArchivo_SelectedIndexChanged;
            // 
            // IngresoInformacionNotaDeCredito
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(440, 468);
            Controls.Add(comboTipoDeArchivo);
            Controls.Add(label6);
            Controls.Add(btnGuardar);
            Controls.Add(label5);
            Controls.Add(dateTimePicker1);
            Controls.Add(txtNumero);
            Controls.Add(txtLetra);
            Controls.Add(txtPuntoDeVenta);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "IngresoInformacionNotaDeCredito";
            Text = "Nueva nota de crédito";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private TextBox txtPuntoDeVenta;
        private TextBox txtLetra;
        private TextBox txtNumero;
        private DateTimePicker dateTimePicker1;
        private Label label5;
        private Button btnGuardar;
        private Label label6;
        private ComboBox comboTipoDeArchivo;
    }
}