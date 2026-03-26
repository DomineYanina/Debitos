using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Debitos.Repositories; // Asegurate de que apunte a tu carpeta de Repositorios

namespace Debitos
{
    public partial class IngresoEnCasoDeCambioDePrestacion : Form
    {
        private DebitosRepository _repository;

        public string tipoATransmitir { get; set; }
        public long idPrestacion { get; set; }
        public List<int> idPrestaciones { get; set; }
        public string tipoDocumentoTransmitido { get; set; }

        public string codigoViejo;
        public string codigoNuevo = "";
        public bool cargaCompleta = false;

        public IngresoEnCasoDeCambioDePrestacion()
        {
            InitializeComponent();
            _repository = new DebitosRepository(DatabaseConfig.ConnectionString);

            try
            {
                // Carga limpia desde la base de datos
                DataTable dataTablePrestacion = _repository.ObtenerCodigosPrestacionUnicos();
                comboBox1.DataSource = dataTablePrestacion;
                comboBox1.DisplayMember = "codigo";
                comboBox1.ValueMember = "codigo";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los datos: " + ex.Message);
            }
        }

        private void IngresoEnCasoDeCambioDePrestacion_Load_1(object sender, EventArgs e)
        {
            lblCodigoViejo.Text = codigoViejo;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cargaCompleta)
            {
                codigoNuevo = comboBox1.Text;
                btnGuardar.Visible = !(lblCodigoViejo.Text).Equals(codigoNuevo);
            }
            else
            {
                cargaCompleta = true;
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                // Guardado transaccional y rápido
                _repository.InsertarCambiosPrestacionTemporal(tipoDocumentoTransmitido, codigoNuevo, idPrestaciones);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar en la base de datos: " + ex.Message);
            }
        }
    }
}