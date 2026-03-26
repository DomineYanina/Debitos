using Debitos.Repositories;
using Npgsql;

namespace Debitos
{
    public partial class IngresoInformacionNotaDeDebito : Form
    {
        private DebitosRepository _repository;
        private string _usuarioActual;
        private String letra = "";
        private String tipoDeArchivo = "";
        private int numeroE, puntoDeVentaE;
        private DateTime fecha;
        private bool puntoDeVentaSeleccionado = false;
        private bool letraSeleccionada = false;
        private bool numeroSeleccionado = false;
        private bool tipoDeArchivoSeleccionado = false;
        private int _facturaNumero;
        private string _facturaLetra;
        private int _facturaPuntoDeVenta;
        private string _facturaTipo;
        
        public IngresoInformacionNotaDeDebito(bool cargaACompletar, int facturaNumero, string facturaLetra, int facturaPuntoDeVenta, string facturaTipo, string usuarioActual)
        {
            InitializeComponent();
            _usuarioActual = usuarioActual;
            btnGuardar.Visible = false;
            fecha = dateTimePicker1.Value;
            _facturaNumero = facturaNumero;
            _facturaLetra = facturaLetra;
            _facturaPuntoDeVenta = facturaPuntoDeVenta;
            _facturaTipo = facturaTipo;
            string connectionString = "Host=172.16.13.219;Port=5432;Username=postgres;Password=postgres;Database=Debitos;";
            _repository = new DebitosRepository(connectionString);
        }

        private void txtPuntoDeVenta_TextChanged_1(object sender, EventArgs e)
        {
            if (txtPuntoDeVenta.Text.Length > 0)
            {
                if (int.TryParse(txtPuntoDeVenta.Text, out int puntoDeVenta))
                {
                    puntoDeVentaE = Convert.ToInt32(txtPuntoDeVenta.Text);
                    puntoDeVentaSeleccionado = true;
                }
                else
                {
                    MessageBox.Show("Por favor ingrese un número válido");
                    txtPuntoDeVenta.Text = "";
                }
            }

            else
            {
                puntoDeVentaE = 0;
                puntoDeVentaSeleccionado = false;
            }

            if ((puntoDeVentaSeleccionado) && (letraSeleccionada) && (numeroSeleccionado) && (tipoDeArchivoSeleccionado))
            {
                btnGuardar.Visible = true;
            }

            else
            {
                btnGuardar.Visible = false;
            }
        }

        private void txtLetra_TextChanged_1(object sender, EventArgs e)
        {
            if (txtLetra.Text.Length > 0)
            {
                if (txtLetra.Text.Length > 1)
                {
                    MessageBox.Show("Por favor ingrese una sola letra");
                    txtLetra.Text = "";
                }
                else
                {
                    if (!int.TryParse(txtLetra.Text, out int numero))
                    {
                        letra = txtLetra.Text.ToUpper();
                        letraSeleccionada = true;
                    }
                    else
                    {
                        MessageBox.Show("Por favor ingrese una letra válida");
                        txtLetra.Text = "";
                    }
                }
            }

            else
            {
                letra = "";
                letraSeleccionada = false;
            }

            if ((puntoDeVentaSeleccionado) && (letraSeleccionada) && (numeroSeleccionado) && (tipoDeArchivoSeleccionado))
            {
                btnGuardar.Visible = true;
            }

            else
            {
                btnGuardar.Visible = false;
            }
        }

        private void txtNumero_TextChanged_1(object sender, EventArgs e)
        {
            if (txtNumero.Text.Length > 0)
            {
                if (int.TryParse(txtNumero.Text, out int numero))
                {
                    numeroE = Convert.ToInt32(txtNumero.Text);
                    numeroSeleccionado = true;
                }

                else
                {
                    MessageBox.Show("Por favor ingrese un número válido");
                    txtNumero.Text = "";
                }
            }

            else
            {
                numeroE = 0;
                numeroSeleccionado = false;
            }

            if ((puntoDeVentaSeleccionado) && (letraSeleccionada) && (numeroSeleccionado) && (tipoDeArchivoSeleccionado))
            {
                btnGuardar.Visible = true;
            }

            else
            {
                btnGuardar.Visible = false;
            }
        }

        private void dateTimePicker1_ValueChanged_1(object sender, EventArgs e)
        {
            fecha = dateTimePicker1.Value;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                // Un solo llamado, cero SQL en el formulario
                _repository.ProcesarGuardadoNotaDeDebito(tipoDeArchivo, letra, puntoDeVentaE, numeroE, fecha, _facturaNumero, _facturaLetra, _facturaPuntoDeVenta, _facturaTipo, _usuarioActual);

                MessageBox.Show("Se ha creado correctamente la nota de débito");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar la nota de débito: " + ex.Message);
            }
        }

        private void comboTipoDeArchivo_SelectedIndexChanged(object sender, EventArgs e)
        {
            tipoDeArchivo = comboTipoDeArchivo.SelectedItem.ToString();
            tipoDeArchivoSeleccionado = true;

            if ((puntoDeVentaSeleccionado) && (letraSeleccionada) && (numeroSeleccionado) && (tipoDeArchivoSeleccionado))
            {
                btnGuardar.Visible = true;
            }

            else
            {
                btnGuardar.Visible = false;
            }
        }
    }
}
