using Npgsql;
using System.Windows.Forms;

namespace Debitos
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            groupBox1 = new GroupBox();
            checkPrestacionesSinDebito = new CheckBox();
            checkPrestacionesSinRefactura = new CheckBox();
            lblNumeroDeInternacionSel = new Label();
            filtroNumeroDeInternacion = new ComboBox();
            comboFiltroFecha = new ComboBox();
            lblFecSel = new Label();
            lblPrestSel = new Label();
            lblProfSel = new Label();
            lblPacSel = new Label();
            btnBuscar = new Button();
            btnBorrarFiltros = new Button();
            numero = new TextBox();
            puntodeventa = new TextBox();
            letra = new TextBox();
            filtroModulo = new ComboBox();
            filtroPrestacion = new ComboBox();
            filtroProfesional = new ComboBox();
            filtroGrupoPrestacion = new ComboBox();
            filtroPaciente = new ComboBox();
            soloPrestacionesValorizadas = new CheckBox();
            filtroTipo = new ComboBox();
            panel1 = new Panel();
            lblModulo = new Label();
            button1 = new Button();
            dataGridView1 = new DataGridView();
            filtroDebitoAceptado = new ComboBox();
            filtroMotivoDeRefactura = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            btnExportar = new Button();
            label6 = new Label();
            filtroMotivoDebito = new ComboBox();
            checkMotivoDebito = new CheckBox();
            checkDebitoAceptado = new CheckBox();
            checkMotivoDeRefactura = new CheckBox();
            lblMontosNoAceptados = new Label();
            lblMontoTotalRegistrosEnPantalla = new Label();
            btnNuevaNotaDeCrédito = new Button();
            btnNuevaNotaDeDébito = new Button();
            btnLimpiarFila = new Button();
            btnGuardarParcialmente = new Button();
            lblCantidadDeRegistrosFiltrados = new Label();
            lblCantidadDeRegistrosConDebitoAceptado = new Label();
            btnBorrarImporteDebito = new Button();
            btnBorrarImporteRefactura = new Button();
            btnBorrarCelda = new Button();
            groupBox1.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.BackColor = SystemColors.ActiveCaptionText;
            groupBox1.Controls.Add(checkPrestacionesSinDebito);
            groupBox1.Controls.Add(checkPrestacionesSinRefactura);
            groupBox1.Controls.Add(lblNumeroDeInternacionSel);
            groupBox1.Controls.Add(filtroNumeroDeInternacion);
            groupBox1.Controls.Add(comboFiltroFecha);
            groupBox1.Controls.Add(lblFecSel);
            groupBox1.Controls.Add(lblPrestSel);
            groupBox1.Controls.Add(lblProfSel);
            groupBox1.Controls.Add(lblPacSel);
            groupBox1.Controls.Add(btnBuscar);
            groupBox1.Controls.Add(btnBorrarFiltros);
            groupBox1.Controls.Add(numero);
            groupBox1.Controls.Add(puntodeventa);
            groupBox1.Controls.Add(letra);
            groupBox1.Controls.Add(filtroModulo);
            groupBox1.Controls.Add(filtroPrestacion);
            groupBox1.Controls.Add(filtroProfesional);
            groupBox1.Controls.Add(filtroGrupoPrestacion);
            groupBox1.Controls.Add(filtroPaciente);
            groupBox1.Controls.Add(soloPrestacionesValorizadas);
            groupBox1.Controls.Add(filtroTipo);
            groupBox1.Controls.Add(panel1);
            groupBox1.Font = new Font("Segoe UI", 12F, FontStyle.Underline);
            groupBox1.ForeColor = SystemColors.ButtonHighlight;
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(1460, 134);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Filtros";
            // 
            // checkPrestacionesSinDebito
            // 
            checkPrestacionesSinDebito.Font = new Font("Segoe UI", 9F);
            checkPrestacionesSinDebito.Location = new Point(346, 41);
            checkPrestacionesSinDebito.Name = "checkPrestacionesSinDebito";
            checkPrestacionesSinDebito.Size = new Size(207, 31);
            checkPrestacionesSinDebito.TabIndex = 49;
            checkPrestacionesSinDebito.Text = "Ver solo prest. sin mot. de débito";
            checkPrestacionesSinDebito.UseVisualStyleBackColor = true;
            checkPrestacionesSinDebito.CheckedChanged += checkPrestacionesSinDebito_CheckedChanged;
            // 
            // checkPrestacionesSinRefactura
            // 
            checkPrestacionesSinRefactura.Font = new Font("Segoe UI", 9F);
            checkPrestacionesSinRefactura.Location = new Point(346, 69);
            checkPrestacionesSinRefactura.Name = "checkPrestacionesSinRefactura";
            checkPrestacionesSinRefactura.Size = new Size(215, 31);
            checkPrestacionesSinRefactura.TabIndex = 48;
            checkPrestacionesSinRefactura.Text = "Ver solo prest. sin mot. de refactura";
            checkPrestacionesSinRefactura.UseVisualStyleBackColor = true;
            checkPrestacionesSinRefactura.CheckedChanged += checkPrestacionesSinRefactura_CheckedChanged;
            // 
            // lblNumeroDeInternacionSel
            // 
            lblNumeroDeInternacionSel.AutoSize = true;
            lblNumeroDeInternacionSel.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblNumeroDeInternacionSel.Location = new Point(1312, 18);
            lblNumeroDeInternacionSel.Name = "lblNumeroDeInternacionSel";
            lblNumeroDeInternacionSel.Size = new Size(100, 15);
            lblNumeroDeInternacionSel.TabIndex = 47;
            lblNumeroDeInternacionSel.Text = "N° de internación";
            lblNumeroDeInternacionSel.Click += lblNumeroDeInternacionSel_Click;
            // 
            // filtroNumeroDeInternacion
            // 
            filtroNumeroDeInternacion.Font = new Font("Segoe UI", 10F);
            filtroNumeroDeInternacion.FormattingEnabled = true;
            filtroNumeroDeInternacion.Location = new Point(1312, 36);
            filtroNumeroDeInternacion.Name = "filtroNumeroDeInternacion";
            filtroNumeroDeInternacion.Size = new Size(129, 25);
            filtroNumeroDeInternacion.TabIndex = 46;
            filtroNumeroDeInternacion.Text = "N° de internación";
            filtroNumeroDeInternacion.SelectedIndexChanged += filtroNumeroDeInternacion_SelectedIndexChanged;
            // 
            // comboFiltroFecha
            // 
            comboFiltroFecha.Font = new Font("Segoe UI", 10F);
            comboFiltroFecha.FormattingEnabled = true;
            comboFiltroFecha.Location = new Point(1312, 100);
            comboFiltroFecha.Name = "comboFiltroFecha";
            comboFiltroFecha.Size = new Size(129, 25);
            comboFiltroFecha.TabIndex = 45;
            comboFiltroFecha.Text = "Fecha";
            comboFiltroFecha.SelectedIndexChanged += comboFiltroFecha_SelectedIndexChanged;
            // 
            // lblFecSel
            // 
            lblFecSel.AutoSize = true;
            lblFecSel.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblFecSel.Location = new Point(1312, 78);
            lblFecSel.Name = "lblFecSel";
            lblFecSel.Size = new Size(38, 15);
            lblFecSel.TabIndex = 44;
            lblFecSel.Text = "Fecha";
            lblFecSel.Click += lblFecSel_Click;
            // 
            // lblPrestSel
            // 
            lblPrestSel.AutoSize = true;
            lblPrestSel.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblPrestSel.Location = new Point(960, 16);
            lblPrestSel.Name = "lblPrestSel";
            lblPrestSel.Size = new Size(62, 15);
            lblPrestSel.TabIndex = 42;
            lblPrestSel.Text = "Prestación";
            lblPrestSel.Click += lblPrestSel_Click;
            // 
            // lblProfSel
            // 
            lblProfSel.AutoSize = true;
            lblProfSel.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblProfSel.Location = new Point(960, 78);
            lblProfSel.Name = "lblProfSel";
            lblProfSel.Size = new Size(66, 15);
            lblProfSel.TabIndex = 41;
            lblProfSel.Text = "Profesional";
            lblProfSel.Click += lblProfSel_Click;
            // 
            // lblPacSel
            // 
            lblPacSel.AutoSize = true;
            lblPacSel.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblPacSel.Location = new Point(1136, 78);
            lblPacSel.Name = "lblPacSel";
            lblPacSel.Size = new Size(52, 15);
            lblPacSel.TabIndex = 40;
            lblPacSel.Text = "Paciente";
            lblPacSel.Click += lblPacSel_Click;
            // 
            // btnBuscar
            // 
            btnBuscar.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Underline);
            btnBuscar.ForeColor = SystemColors.ActiveCaptionText;
            btnBuscar.Location = new Point(23, 103);
            btnBuscar.Name = "btnBuscar";
            btnBuscar.Size = new Size(317, 25);
            btnBuscar.TabIndex = 23;
            btnBuscar.Text = "Buscar";
            btnBuscar.UseVisualStyleBackColor = true;
            btnBuscar.Visible = false;
            btnBuscar.Click += btnBuscar_Click;
            // 
            // btnBorrarFiltros
            // 
            btnBorrarFiltros.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Underline);
            btnBorrarFiltros.ForeColor = SystemColors.ActiveCaptionText;
            btnBorrarFiltros.Location = new Point(346, 104);
            btnBorrarFiltros.Name = "btnBorrarFiltros";
            btnBorrarFiltros.Size = new Size(156, 24);
            btnBorrarFiltros.TabIndex = 19;
            btnBorrarFiltros.Text = "Borrar filtros";
            btnBorrarFiltros.UseVisualStyleBackColor = true;
            btnBorrarFiltros.Click += btnBorrarFiltros_Click;
            // 
            // numero
            // 
            numero.Font = new Font("Segoe UI", 10F);
            numero.Location = new Point(192, 68);
            numero.Name = "numero";
            numero.PlaceholderText = "Número";
            numero.Size = new Size(148, 25);
            numero.TabIndex = 35;
            numero.TextChanged += numero_TextChanged;
            // 
            // puntodeventa
            // 
            puntodeventa.Font = new Font("Segoe UI", 10F);
            puntodeventa.Location = new Point(23, 68);
            puntodeventa.Name = "puntodeventa";
            puntodeventa.PlaceholderText = "Punto de Venta";
            puntodeventa.Size = new Size(148, 25);
            puntodeventa.TabIndex = 34;
            puntodeventa.TextChanged += puntodeventa_TextChanged;
            // 
            // letra
            // 
            letra.Font = new Font("Segoe UI", 10F);
            letra.Location = new Point(192, 28);
            letra.Name = "letra";
            letra.PlaceholderText = "Letra";
            letra.Size = new Size(148, 25);
            letra.TabIndex = 23;
            letra.TextChanged += letra_TextChanged;
            // 
            // filtroModulo
            // 
            filtroModulo.Font = new Font("Segoe UI", 10F);
            filtroModulo.FormattingEnabled = true;
            filtroModulo.Location = new Point(785, 100);
            filtroModulo.Name = "filtroModulo";
            filtroModulo.Size = new Size(148, 25);
            filtroModulo.TabIndex = 33;
            filtroModulo.Text = "Módulo";
            filtroModulo.SelectedIndexChanged += filtroModulo_SelectedIndexChanged;
            // 
            // filtroPrestacion
            // 
            filtroPrestacion.Font = new Font("Segoe UI", 10F);
            filtroPrestacion.FormattingEnabled = true;
            filtroPrestacion.Location = new Point(960, 36);
            filtroPrestacion.Name = "filtroPrestacion";
            filtroPrestacion.Size = new Size(148, 25);
            filtroPrestacion.TabIndex = 30;
            filtroPrestacion.Text = "Prestación";
            filtroPrestacion.Visible = false;
            filtroPrestacion.SelectedIndexChanged += filtroPrestacion_SelectedIndexChanged;
            // 
            // filtroProfesional
            // 
            filtroProfesional.Font = new Font("Segoe UI", 10F);
            filtroProfesional.FormattingEnabled = true;
            filtroProfesional.Location = new Point(960, 100);
            filtroProfesional.Name = "filtroProfesional";
            filtroProfesional.Size = new Size(148, 25);
            filtroProfesional.TabIndex = 29;
            filtroProfesional.Text = "Profesional";
            filtroProfesional.Visible = false;
            filtroProfesional.SelectedIndexChanged += filtroProfesional_SelectedIndexChanged;
            // 
            // filtroGrupoPrestacion
            // 
            filtroGrupoPrestacion.Font = new Font("Segoe UI", 10F);
            filtroGrupoPrestacion.FormattingEnabled = true;
            filtroGrupoPrestacion.Location = new Point(1136, 36);
            filtroGrupoPrestacion.Name = "filtroGrupoPrestacion";
            filtroGrupoPrestacion.Size = new Size(148, 25);
            filtroGrupoPrestacion.TabIndex = 28;
            filtroGrupoPrestacion.Text = "Grupo Prestación";
            // 
            // filtroPaciente
            // 
            filtroPaciente.Font = new Font("Segoe UI", 10F);
            filtroPaciente.FormattingEnabled = true;
            filtroPaciente.Location = new Point(1136, 100);
            filtroPaciente.Name = "filtroPaciente";
            filtroPaciente.Size = new Size(148, 25);
            filtroPaciente.TabIndex = 27;
            filtroPaciente.Text = "Paciente";
            filtroPaciente.Visible = false;
            filtroPaciente.SelectedIndexChanged += filtroPaciente_SelectedIndexChanged;
            // 
            // soloPrestacionesValorizadas
            // 
            soloPrestacionesValorizadas.Font = new Font("Segoe UI", 9F);
            soloPrestacionesValorizadas.Location = new Point(346, 14);
            soloPrestacionesValorizadas.Name = "soloPrestacionesValorizadas";
            soloPrestacionesValorizadas.Size = new Size(207, 31);
            soloPrestacionesValorizadas.TabIndex = 26;
            soloPrestacionesValorizadas.Text = "Ver solo prestaciones valorizadas";
            soloPrestacionesValorizadas.UseVisualStyleBackColor = true;
            soloPrestacionesValorizadas.CheckedChanged += soloPrestacionesValorizadas_CheckedChanged;
            // 
            // filtroTipo
            // 
            filtroTipo.Font = new Font("Segoe UI", 10F);
            filtroTipo.FormattingEnabled = true;
            filtroTipo.Items.AddRange(new object[] { "NC", "ND", "FC" });
            filtroTipo.Location = new Point(23, 28);
            filtroTipo.Name = "filtroTipo";
            filtroTipo.Size = new Size(148, 25);
            filtroTipo.TabIndex = 9;
            filtroTipo.Text = "Tipo";
            filtroTipo.SelectedIndexChanged += filtroTipo_SelectedIndexChanged;
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.ScrollBar;
            panel1.Controls.Add(lblModulo);
            panel1.ForeColor = SystemColors.ActiveCaptionText;
            panel1.Location = new Point(651, 92);
            panel1.Name = "panel1";
            panel1.Size = new Size(290, 36);
            panel1.TabIndex = 42;
            // 
            // lblModulo
            // 
            lblModulo.AutoSize = true;
            lblModulo.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblModulo.Location = new Point(12, 13);
            lblModulo.Name = "lblModulo";
            lblModulo.Size = new Size(49, 15);
            lblModulo.TabIndex = 48;
            lblModulo.Text = "Módulo";
            // 
            // button1
            // 
            button1.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Underline, GraphicsUnit.Point, 0);
            button1.ForeColor = SystemColors.ActiveCaptionText;
            button1.Location = new Point(1057, 677);
            button1.Name = "button1";
            button1.Size = new Size(148, 28);
            button1.TabIndex = 39;
            button1.Text = "Ver historial";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.CausesValidation = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.WhiteSmoke;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle1.NullValue = null;
            dataGridViewCellStyle1.SelectionBackColor = Color.DarkViolet;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.False;
            dataGridView1.DefaultCellStyle = dataGridViewCellStyle1;
            dataGridView1.EditMode = DataGridViewEditMode.EditOnEnter;
            dataGridView1.Location = new Point(12, 152);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
            dataGridView1.ShowCellErrors = false;
            dataGridView1.Size = new Size(1460, 478);
            dataGridView1.TabIndex = 1;
            dataGridView1.CellValueChanged += dataGridView1_CellValueChanged;
            dataGridView1.ColumnHeaderMouseClick += dataGridView1_ColumnHeaderMouseClick;
            dataGridView1.DataError += dataGridView1_DataError;
            dataGridView1.Sorted += DataGridView1_Sorted;
            // 
            // filtroDebitoAceptado
            // 
            filtroDebitoAceptado.Font = new Font("Segoe UI", 10F);
            filtroDebitoAceptado.FormattingEnabled = true;
            filtroDebitoAceptado.Items.AddRange(new object[] { "No", "Si" });
            filtroDebitoAceptado.Location = new Point(136, 699);
            filtroDebitoAceptado.Name = "filtroDebitoAceptado";
            filtroDebitoAceptado.Size = new Size(118, 25);
            filtroDebitoAceptado.TabIndex = 11;
            filtroDebitoAceptado.Text = "Seleccionar";
            filtroDebitoAceptado.SelectedIndexChanged += filtroDebitoAceptado_SelectedIndexChanged;
            // 
            // filtroMotivoDeRefactura
            // 
            filtroMotivoDeRefactura.Font = new Font("Segoe UI", 10F);
            filtroMotivoDeRefactura.FormattingEnabled = true;
            filtroMotivoDeRefactura.Items.AddRange(new object[] { "Borrar", "No aplica", "Casos: Afiliados activos.", "Casos: Discrepancia cobertura pensión.", "Casos: Excepciones refacturadas.", "Casos: Médico externo sin historia clínica.", "Corrección de error de Open", "Débitos Inválidos: Aplicados erróneamente.", "Doc. y Aut.: Autorización recibida posterior al cierre.", "Doc. y Aut.: Autorización vigente.", "Doc. y Aut.: Doc. completa enviada.", "Doc. y Aut.: Facturado en tiempo.", "Doc. y Aut.: Info. filiatoria completa.", "Doc. y Aut.: Justificado en historia clínica.", "Doc. y Aut.: Orden con diagnóstico, se aclara con historia clínica", "Doc. y Aut.: Se envía documentación omitida", "Doc. y Aut.: Se envía troquel/sticker", "Doc. y Aut.: Según normas vigentes.", "Excepciones: Bonificación medicación.", "Excepciones: Bonificación prestación.", "Excepciones: Reclamos/comerciales.", "Gestión: Aclaración procedimiento.", "Gestión: Afiliado dado de baja.", "Gestión: Ajustes en coseguro.", "Gestión: Ajuste por presupuesto.", "Gestión: Aplicación incorrecta de IVA.", "Gestión: Consumos correctos.", "Gestión: Corrección facturación módulos.", "Gestión: Financidor demoró respuesta.", "Gestión: Medicamentos mal facturados.", "Gestión Méd.: Aclaración de diagnóstico", "Gestión Méd.: Ajuste fechas derivación.", "Gestión Méd.: Criterio en diagnósticos.", "Gestión Méd.: Historia clínica firmada.", "Gestión Méd.: Normas sanatoriales.", "Gestión Méd.: Postoperatorios/antibióticos.", "Gestión Méd.: Tratamientos infecciones.", "Gestión Méd.: Tratamientos médicos.", "Gestión Méd.: Urgencia sin consentimiento.", "Normas: Adjunta norma del Nom. Nac.", "Normas: Ajustes valores medicación/material.", "Normas: Aplicación de normas acordadas.", "Normas: Aranceles vigentes Colegio Bioquím.", "Normas: Cambios deben ser acordados.", "Normas: Exclusión no explícita.", "Normas: Facturación según módulos vigentes.", "Normas: Inclusión/Exclusión según acuerdo.", "Normas: Incompatibilidad normativa.", "Normas: Obligación de cobertura por ley", "Normas: Prestación arancel convenido.", "Normas: Prestación no respondida por financ.", "Normas: Prestación según presupuesto.", "Normas: Recargos urgencia según Nac. Nom.", "Normas: Refacturación por IVA.", "Normas: Valores de contrastes vigentes.", "Normas: Valores medicación/material convenio.", "Prestaciones: Aranceles según CEDIM.", "Prestaciones: Consultas previas/post-proced.", "Prestaciones: Homologada.", "Prestaciones: Inclusión incorrecta.", "Prestaciones: No incluidas según Nom. Nac.", "Prestaciones: Material no incluido en base.", "Prestaciones: Procedimientos ampliados.", "Prestaciones: Relacionadas a prestación." });
            filtroMotivoDeRefactura.Location = new Point(765, 638);
            filtroMotivoDeRefactura.Name = "filtroMotivoDeRefactura";
            filtroMotivoDeRefactura.Size = new Size(355, 25);
            filtroMotivoDeRefactura.TabIndex = 12;
            filtroMotivoDeRefactura.Text = "Seleccionar";
            filtroMotivoDeRefactura.SelectedIndexChanged += filtroMotivoDeRefactura_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9.75F);
            label1.Location = new Point(12, 704);
            label1.Name = "label1";
            label1.Size = new Size(118, 17);
            label1.TabIndex = 14;
            label1.Text = "¿Débito aceptado?";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9.75F);
            label2.Location = new Point(635, 641);
            label2.Name = "label2";
            label2.Size = new Size(124, 17);
            label2.TabIndex = 15;
            label2.Text = "Motivo de refactura";
            // 
            // btnExportar
            // 
            btnExportar.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Underline);
            btnExportar.Location = new Point(1365, 718);
            btnExportar.Name = "btnExportar";
            btnExportar.Size = new Size(107, 28);
            btnExportar.TabIndex = 17;
            btnExportar.Text = "Exportar";
            btnExportar.UseVisualStyleBackColor = true;
            btnExportar.Click += btnExportar_Click_1;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 9.75F);
            label6.Location = new Point(12, 641);
            label6.Name = "label6";
            label6.Size = new Size(110, 17);
            label6.TabIndex = 21;
            label6.Text = "Motivo de débito";
            // 
            // filtroMotivoDebito
            // 
            filtroMotivoDebito.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            filtroMotivoDebito.FormattingEnabled = true;
            filtroMotivoDebito.Items.AddRange(new object[] { "Borrar", "No aplica", "Afiliado capitado", "Afiliado dado de baja", "Alta demorada criterio audotoria medica", "Conteo de medicacion erroneo hojas de enfermeria no identificadas con fecha", "Coseguro no cobrado", "Debito 20% urgencia modulos", "Debito 20% urgencia prestaciones", "Debito por diferencia en la inclusiones modulares", "Debito por falta de historia clinica", "Debito por historias clinicas de distintos pacientes en la misma internacion", "Debito por normas contractuales (ejemplo veda+vcc)", "Debito por normas operativas", "Debito segun normas del nomenclador", "Demora en Inter Consulta", "Demora en resolución quirúrgica", "Diagnostico ilegible", "Diagnostico no reconocido", "Diferencia de aranceles", "Diferencia de coseguro", "Diferencia de criterio medico/prestaciones no justificadas", "Diferencia de valor en medicamentos/descartables", "Documentacion adulterada", "Ecografia de partes blandas incluida en ecografia abdominal", "Ecografia renal incluida en abdominal", "Error de carga (codigos-inclusiones)", "Error de Open", "Error en el cálculo de porcentaje de códigos múltiples", "Exceso de facturacion en medicamentos y descartables", "Facturacion duplicada", "Facturado a financiador incorrecto", "Facturado con nota de departamento comercial", "Falta de autorizacion", "Falta de documentacion avalatoria", "Falta de historia/informe.", "Falta de troqueles-stickers de medicacion o materiales", "Falta firma paciente", "Falta firma profesional", "Falta informe", "Historia clinica incompleta", "Honorarios profesionales pagados en forma directa", "Incluido en APB", "Iva mal facturado", "Material/ Medicamentos provistos por O.S.", "Material no utilizado", "Medicación no suministrada", "No indicado", "No reconoce prestación", "Orden sin diagnóstico", "Prestacion fuera de termino", "Prestacion incluida en otra", "Prestacion incluida en otra liquidacion", "Prestacion no homologada", "Prestacion no justificada", "Prestacion sin convenio", "Presupuesto facturado con nota no reconocido", "Presupuesto rechazado y facturados con indicacion comercial", "Rechazo de refactura por mantener motivos de debitos originales", "Supera tope anual" });
            filtroMotivoDebito.Location = new Point(136, 638);
            filtroMotivoDebito.Name = "filtroMotivoDebito";
            filtroMotivoDebito.Size = new Size(378, 25);
            filtroMotivoDebito.TabIndex = 22;
            filtroMotivoDebito.Text = "Seleccionar";
            filtroMotivoDebito.SelectedIndexChanged += filtroMotivoDebito_SelectedIndexChanged;
            // 
            // checkMotivoDebito
            // 
            checkMotivoDebito.Location = new Point(12, 664);
            checkMotivoDebito.Name = "checkMotivoDebito";
            checkMotivoDebito.Size = new Size(226, 20);
            checkMotivoDebito.TabIndex = 23;
            checkMotivoDebito.Text = "Aplicar a toda la columna";
            checkMotivoDebito.UseVisualStyleBackColor = true;
            // 
            // checkDebitoAceptado
            // 
            checkDebitoAceptado.Location = new Point(12, 725);
            checkDebitoAceptado.Name = "checkDebitoAceptado";
            checkDebitoAceptado.Size = new Size(242, 23);
            checkDebitoAceptado.TabIndex = 24;
            checkDebitoAceptado.Text = "Aplicar a toda la columna";
            checkDebitoAceptado.UseVisualStyleBackColor = true;
            // 
            // checkMotivoDeRefactura
            // 
            checkMotivoDeRefactura.Location = new Point(635, 664);
            checkMotivoDeRefactura.Name = "checkMotivoDeRefactura";
            checkMotivoDeRefactura.Size = new Size(254, 18);
            checkMotivoDeRefactura.TabIndex = 29;
            checkMotivoDeRefactura.Text = "Aplicar a toda la columna";
            checkMotivoDeRefactura.UseVisualStyleBackColor = true;
            // 
            // lblMontosNoAceptados
            // 
            lblMontosNoAceptados.AutoSize = true;
            lblMontosNoAceptados.BackColor = Color.Black;
            lblMontosNoAceptados.Font = new Font("Segoe UI", 9.75F);
            lblMontosNoAceptados.ForeColor = Color.White;
            lblMontosNoAceptados.Location = new Point(600, 70);
            lblMontosNoAceptados.Name = "lblMontosNoAceptados";
            lblMontosNoAceptados.Size = new Size(0, 17);
            lblMontosNoAceptados.TabIndex = 31;
            // 
            // lblMontoTotalRegistrosEnPantalla
            // 
            lblMontoTotalRegistrosEnPantalla.AutoSize = true;
            lblMontoTotalRegistrosEnPantalla.BackColor = Color.Black;
            lblMontoTotalRegistrosEnPantalla.Font = new Font("Segoe UI", 9.75F);
            lblMontoTotalRegistrosEnPantalla.ForeColor = Color.White;
            lblMontoTotalRegistrosEnPantalla.Location = new Point(600, 50);
            lblMontoTotalRegistrosEnPantalla.Name = "lblMontoTotalRegistrosEnPantalla";
            lblMontoTotalRegistrosEnPantalla.Size = new Size(0, 17);
            lblMontoTotalRegistrosEnPantalla.TabIndex = 32;
            // 
            // btnNuevaNotaDeCrédito
            // 
            btnNuevaNotaDeCrédito.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Underline, GraphicsUnit.Point, 0);
            btnNuevaNotaDeCrédito.Location = new Point(1211, 677);
            btnNuevaNotaDeCrédito.Name = "btnNuevaNotaDeCrédito";
            btnNuevaNotaDeCrédito.Size = new Size(148, 28);
            btnNuevaNotaDeCrédito.TabIndex = 33;
            btnNuevaNotaDeCrédito.Text = "Nueva Nota de Crédito";
            btnNuevaNotaDeCrédito.UseVisualStyleBackColor = true;
            btnNuevaNotaDeCrédito.Click += btnNuevaNotaDeCrédito_Click;
            // 
            // btnNuevaNotaDeDébito
            // 
            btnNuevaNotaDeDébito.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Underline, GraphicsUnit.Point, 0);
            btnNuevaNotaDeDébito.Location = new Point(1211, 718);
            btnNuevaNotaDeDébito.Name = "btnNuevaNotaDeDébito";
            btnNuevaNotaDeDébito.Size = new Size(148, 28);
            btnNuevaNotaDeDébito.TabIndex = 34;
            btnNuevaNotaDeDébito.Text = "Nueva Nota de Débito";
            btnNuevaNotaDeDébito.UseVisualStyleBackColor = true;
            btnNuevaNotaDeDébito.Click += btnNuevaNotaDeDébito_Click;
            // 
            // btnLimpiarFila
            // 
            btnLimpiarFila.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Underline, GraphicsUnit.Point, 0);
            btnLimpiarFila.Location = new Point(1365, 677);
            btnLimpiarFila.Name = "btnLimpiarFila";
            btnLimpiarFila.Size = new Size(107, 28);
            btnLimpiarFila.TabIndex = 35;
            btnLimpiarFila.Text = "Limpiar fila";
            btnLimpiarFila.UseVisualStyleBackColor = true;
            btnLimpiarFila.Click += btnLimpiarFila_Click;
            // 
            // btnGuardarParcialmente
            // 
            btnGuardarParcialmente.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Underline, GraphicsUnit.Point, 0);
            btnGuardarParcialmente.Location = new Point(1057, 718);
            btnGuardarParcialmente.Name = "btnGuardarParcialmente";
            btnGuardarParcialmente.Size = new Size(148, 28);
            btnGuardarParcialmente.TabIndex = 36;
            btnGuardarParcialmente.Text = "Guardar parcialmente";
            btnGuardarParcialmente.UseVisualStyleBackColor = true;
            btnGuardarParcialmente.Click += btnGuardarParcialmente_Click;
            // 
            // lblCantidadDeRegistrosFiltrados
            // 
            lblCantidadDeRegistrosFiltrados.AutoSize = true;
            lblCantidadDeRegistrosFiltrados.Location = new Point(308, 699);
            lblCantidadDeRegistrosFiltrados.Name = "lblCantidadDeRegistrosFiltrados";
            lblCantidadDeRegistrosFiltrados.Size = new Size(168, 15);
            lblCantidadDeRegistrosFiltrados.TabIndex = 37;
            lblCantidadDeRegistrosFiltrados.Text = "Cantidad de registros filtrados:";
            lblCantidadDeRegistrosFiltrados.Visible = false;
            // 
            // lblCantidadDeRegistrosConDebitoAceptado
            // 
            lblCantidadDeRegistrosConDebitoAceptado.AutoSize = true;
            lblCantidadDeRegistrosConDebitoAceptado.Location = new Point(308, 725);
            lblCantidadDeRegistrosConDebitoAceptado.Name = "lblCantidadDeRegistrosConDebitoAceptado";
            lblCantidadDeRegistrosConDebitoAceptado.Size = new Size(234, 15);
            lblCantidadDeRegistrosConDebitoAceptado.TabIndex = 38;
            lblCantidadDeRegistrosConDebitoAceptado.Text = "Cantidad de registros con débito aceptado:";
            lblCantidadDeRegistrosConDebitoAceptado.Visible = false;
            // 
            // btnBorrarImporteDebito
            // 
            btnBorrarImporteDebito.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Underline, GraphicsUnit.Point, 0);
            btnBorrarImporteDebito.Location = new Point(1324, 638);
            btnBorrarImporteDebito.Name = "btnBorrarImporteDebito";
            btnBorrarImporteDebito.Size = new Size(148, 28);
            btnBorrarImporteDebito.TabIndex = 40;
            btnBorrarImporteDebito.Text = "Borrar importe débito";
            btnBorrarImporteDebito.UseVisualStyleBackColor = true;
            btnBorrarImporteDebito.Click += btnBorrarImporteDebito_Click;
            // 
            // btnBorrarImporteRefactura
            // 
            btnBorrarImporteRefactura.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Underline, GraphicsUnit.Point, 0);
            btnBorrarImporteRefactura.Location = new Point(1139, 638);
            btnBorrarImporteRefactura.Name = "btnBorrarImporteRefactura";
            btnBorrarImporteRefactura.Size = new Size(179, 28);
            btnBorrarImporteRefactura.TabIndex = 41;
            btnBorrarImporteRefactura.Text = "Borrar importe refactura";
            btnBorrarImporteRefactura.UseVisualStyleBackColor = true;
            btnBorrarImporteRefactura.Click += btnBorrarImporteRefactura_Click;
            // 
            // btnBorrarCelda
            // 
            btnBorrarCelda.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Underline);
            btnBorrarCelda.Location = new Point(924, 677);
            btnBorrarCelda.Name = "btnBorrarCelda";
            btnBorrarCelda.Size = new Size(127, 28);
            btnBorrarCelda.TabIndex = 42;
            btnBorrarCelda.Text = "Borrar celda";
            btnBorrarCelda.UseVisualStyleBackColor = true;
            btnBorrarCelda.Click += btnBorrarCelda_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(1484, 751);
            Controls.Add(btnBorrarCelda);
            Controls.Add(btnBorrarImporteRefactura);
            Controls.Add(btnBorrarImporteDebito);
            Controls.Add(button1);
            Controls.Add(lblCantidadDeRegistrosConDebitoAceptado);
            Controls.Add(lblCantidadDeRegistrosFiltrados);
            Controls.Add(btnGuardarParcialmente);
            Controls.Add(btnLimpiarFila);
            Controls.Add(btnNuevaNotaDeDébito);
            Controls.Add(btnNuevaNotaDeCrédito);
            Controls.Add(lblMontoTotalRegistrosEnPantalla);
            Controls.Add(lblMontosNoAceptados);
            Controls.Add(checkMotivoDeRefactura);
            Controls.Add(checkDebitoAceptado);
            Controls.Add(checkMotivoDebito);
            Controls.Add(filtroMotivoDebito);
            Controls.Add(label6);
            Controls.Add(btnExportar);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(filtroMotivoDeRefactura);
            Controls.Add(filtroDebitoAceptado);
            Controls.Add(dataGridView1);
            Controls.Add(groupBox1);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Débitos";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox groupBox1;
        private DataGridView dataGridView1;
        private ComboBox filtroTipo;
        private ComboBox filtroDebitoAceptado;
        private ComboBox filtroMotivoDeRefactura;
        private Label label1;
        private Label label2;
        private Button btnExportar;
        private Button btnBorrarFiltros;
        private CheckBox soloPrestacionesValorizadas;
        private ComboBox filtroPrestacion;
        private ComboBox filtroProfesional;
        private ComboBox filtroGrupoPrestacion;
        private ComboBox filtroPaciente;
        private ComboBox filtroModulo;
        private Label label6;
        private ComboBox filtroMotivoDebito;
        private TextBox letra;
        private Button btnBuscar;
        private TextBox numero;
        private TextBox puntodeventa;
        private CheckBox checkMotivoDebito;
        private CheckBox checkDebitoAceptado;
        private CheckBox checkMotivoDeRefactura;
        private Label lblMontosNoAceptados;
        private Label lblMontoTotalRegistrosEnPantalla;
        private Button btnNuevaNotaDeCrédito;
        private Button btnNuevaNotaDeDébito;
        private Button btnLimpiarFila;
        private Button btnGuardarParcialmente;
        //private Button limpiarPantalla;
        private Label lblCantidadDeRegistrosFiltrados;
        private Label lblCantidadDeRegistrosConDebitoAceptado;
        private Label lblPacSel;
        private Label lblPrestSel;
        private Label lblProfSel;
        private Label lblFecSel;
        private ComboBox comboFiltroFecha;
        private Label lblNumeroDeInternacionSel;
        private ComboBox filtroNumeroDeInternacion;
        private Button button1;
        public Button btnBorrarImporteDebito;
        public Button btnBorrarImporteRefactura;
        private Label lblModulo;
        private Panel panel1;
        private Button btnBorrarCelda;
        private CheckBox checkPrestacionesSinDebito;
        private CheckBox checkPrestacionesSinRefactura;
    }
}
