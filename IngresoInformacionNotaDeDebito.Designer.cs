namespace Debitos
{
    partial class IngresoInformacionNotaDeDebito
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
            btnGuardar = new Button();
            label5 = new Label();
            dateTimePicker1 = new DateTimePicker();
            txtNumero = new TextBox();
            txtLetra = new TextBox();
            txtPuntoDeVenta = new TextBox();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            label6 = new Label();
            comboTipoDeArchivo = new ComboBox();
            SuspendLayout();
            // 
            // btnGuardar
            // 
            btnGuardar.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnGuardar.Location = new Point(29, 395);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(376, 37);
            btnGuardar.TabIndex = 19;
            btnGuardar.Text = "Guardar";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 15F, FontStyle.Underline, GraphicsUnit.Point, 0);
            label5.Location = new Point(42, 24);
            label5.Name = "label5";
            label5.Size = new Size(354, 28);
            label5.TabIndex = 18;
            label5.Text = "Ingresar información de nota de débito";
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Location = new Point(194, 347);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new Size(211, 23);
            dateTimePicker1.TabIndex = 17;
            dateTimePicker1.ValueChanged += dateTimePicker1_ValueChanged_1;
            // 
            // txtNumero
            // 
            txtNumero.Font = new Font("Segoe UI", 15F);
            txtNumero.Location = new Point(194, 280);
            txtNumero.Name = "txtNumero";
            txtNumero.Size = new Size(211, 34);
            txtNumero.TabIndex = 16;
            txtNumero.TextChanged += txtNumero_TextChanged_1;
            // 
            // txtLetra
            // 
            txtLetra.Font = new Font("Segoe UI", 15F);
            txtLetra.Location = new Point(194, 213);
            txtLetra.Name = "txtLetra";
            txtLetra.Size = new Size(211, 34);
            txtLetra.TabIndex = 15;
            txtLetra.TextChanged += txtLetra_TextChanged_1;
            // 
            // txtPuntoDeVenta
            // 
            txtPuntoDeVenta.Font = new Font("Segoe UI", 15F);
            txtPuntoDeVenta.Location = new Point(194, 144);
            txtPuntoDeVenta.Name = "txtPuntoDeVenta";
            txtPuntoDeVenta.Size = new Size(211, 34);
            txtPuntoDeVenta.TabIndex = 14;
            txtPuntoDeVenta.TextChanged += txtPuntoDeVenta_TextChanged_1;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 15F);
            label4.Location = new Point(29, 342);
            label4.Name = "label4";
            label4.Size = new Size(62, 28);
            label4.TabIndex = 13;
            label4.Text = "Fecha";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 15F);
            label3.Location = new Point(29, 280);
            label3.Name = "label3";
            label3.Size = new Size(84, 28);
            label3.TabIndex = 12;
            label3.Text = "Número";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 15F);
            label2.Location = new Point(29, 213);
            label2.Name = "label2";
            label2.Size = new Size(55, 28);
            label2.TabIndex = 11;
            label2.Text = "Letra";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 15F);
            label1.Location = new Point(29, 147);
            label1.Name = "label1";
            label1.Size = new Size(144, 28);
            label1.TabIndex = 10;
            label1.Text = "Punto de venta";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 15F);
            label6.Location = new Point(29, 84);
            label6.Name = "label6";
            label6.Size = new Size(150, 28);
            label6.TabIndex = 20;
            label6.Text = "Tipo de Archivo";
            // 
            // comboTipoDeArchivo
            // 
            comboTipoDeArchivo.FormattingEnabled = true;
            comboTipoDeArchivo.Items.AddRange(new object[] { "ND", "NDE" });
            comboTipoDeArchivo.Location = new Point(194, 89);
            comboTipoDeArchivo.Name = "comboTipoDeArchivo";
            comboTipoDeArchivo.Size = new Size(211, 23);
            comboTipoDeArchivo.TabIndex = 21;
            comboTipoDeArchivo.SelectedIndexChanged += comboTipoDeArchivo_SelectedIndexChanged;
            // 
            // IngresoInformacionNotaDeDebito
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(436, 450);
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
            Name = "IngresoInformacionNotaDeDebito";
            Text = "IngresoInformacionNotaDeDebito";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnGuardar;
        private Label label5;
        private DateTimePicker dateTimePicker1;
        private TextBox txtNumero;
        private TextBox txtLetra;
        private TextBox txtPuntoDeVenta;
        private Label label4;
        private Label label3;
        private Label label2;
        private Label label1;
        private Label label6;
        private ComboBox comboTipoDeArchivo;
    }
}