namespace Debitos
{
    partial class IngresoEnCasoDeCambioDePrestacion
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
            lblTotal = new Label();
            label9 = new Label();
            comboBox1 = new ComboBox();
            lblTotalImporte = new Label();
            lblTotalPorcentaje = new Label();
            btnGuardar = new Button();
            label10 = new Label();
            lblCodigoViejo = new Label();
            SuspendLayout();
            // 
            // lblTotal
            // 
            lblTotal.AutoSize = true;
            lblTotal.Font = new Font("Segoe UI", 12F);
            lblTotal.Location = new Point(172, 390);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(0, 21);
            lblTotal.TabIndex = 9;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI", 12F);
            label9.Location = new Point(172, 9);
            label9.Name = "label9";
            label9.Size = new Size(107, 21);
            label9.TabIndex = 10;
            label9.Text = "Nuevo código";
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(172, 37);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(180, 23);
            comboBox1.TabIndex = 11;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // lblTotalImporte
            // 
            lblTotalImporte.AutoSize = true;
            lblTotalImporte.Location = new Point(118, 356);
            lblTotalImporte.Name = "lblTotalImporte";
            lblTotalImporte.Size = new Size(0, 15);
            lblTotalImporte.TabIndex = 12;
            // 
            // lblTotalPorcentaje
            // 
            lblTotalPorcentaje.AutoSize = true;
            lblTotalPorcentaje.Location = new Point(251, 357);
            lblTotalPorcentaje.Name = "lblTotalPorcentaje";
            lblTotalPorcentaje.Size = new Size(0, 15);
            lblTotalPorcentaje.TabIndex = 13;
            // 
            // btnGuardar
            // 
            btnGuardar.Location = new Point(21, 71);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(331, 29);
            btnGuardar.TabIndex = 14;
            btnGuardar.Text = "Guardar";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Visible = false;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Segoe UI", 12F);
            label10.Location = new Point(21, 9);
            label10.Name = "label10";
            label10.Size = new Size(97, 21);
            label10.TabIndex = 15;
            label10.Text = "Código viejo";
            // 
            // lblCodigoViejo
            // 
            lblCodigoViejo.AutoSize = true;
            lblCodigoViejo.Font = new Font("Segoe UI", 12F);
            lblCodigoViejo.Location = new Point(137, 35);
            lblCodigoViejo.Name = "lblCodigoViejo";
            lblCodigoViejo.Size = new Size(0, 21);
            lblCodigoViejo.TabIndex = 16;
            // 
            // IngresoEnCasoDeCambioDePrestacion
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(369, 110);
            Controls.Add(lblCodigoViejo);
            Controls.Add(label10);
            Controls.Add(btnGuardar);
            Controls.Add(lblTotalPorcentaje);
            Controls.Add(lblTotalImporte);
            Controls.Add(comboBox1);
            Controls.Add(label9);
            Controls.Add(lblTotal);
            Name = "IngresoEnCasoDeCambioDePrestacion";
            Text = "Form2";
            Load += IngresoEnCasoDeCambioDePrestacion_Load_1;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label lblTotal;
        private Label label9;
        private ComboBox comboBox1;
        private Label lblTotalImporte;
        private Label lblTotalPorcentaje;
        private Button btnGuardar;
        private Label label10;
        private Label lblCodigoViejo;
    }
}