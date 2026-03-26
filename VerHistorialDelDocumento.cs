using System;
using System.Data;
using System.Windows.Forms;
using Debitos.Repositories; // Asegurate de que apunte a tu carpeta de Repositorios

namespace Debitos
{
    public partial class VerHistorialDelDocumento : Form
    {
        private DebitosRepository _repository;
        private int _facturaNumero;
        private string _facturaLetra;
        private int _facturaPuntoDeVenta;
        private string _facturaTipo;

        public VerHistorialDelDocumento(int facturaNumero, string facturaLetra, int facturaPuntoDeVenta, string facturaTipo)
        {
            InitializeComponent();
            _facturaNumero = facturaNumero;
            _facturaLetra = facturaLetra;
            _facturaPuntoDeVenta = facturaPuntoDeVenta;
            _facturaTipo = facturaTipo;

            _repository = new DebitosRepository(DatabaseConfig.ConnectionString);
            CargarHistorial();
        }

        private void CargarHistorial()
        {
            try
            {
                DataTable historial = _repository.ObtenerHistorialDocumento(_facturaTipo, _facturaLetra, _facturaPuntoDeVenta, _facturaNumero);

                // Habilitamos la grilla que tenías comentada
                dataGridView1.DataSource = historial;
                if (dataGridView1.Columns.Contains("id"))
                {
                    dataGridView1.Columns["id"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el historial del documento: " + ex.Message);
            }
        }
    }
}