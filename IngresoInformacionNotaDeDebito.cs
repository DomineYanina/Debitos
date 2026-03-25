using Npgsql;

namespace Debitos
{
    public partial class IngresoInformacionNotaDeDebito : Form
    {
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
        string queryDelete = "DELETE FROM auxnd";
        NpgsqlConnection conexion = new NpgsqlConnection("Host=172.16.13.219;Port=5432;Username=postgres;Password=postgres;Database=Debitos;");

        public IngresoInformacionNotaDeDebito(bool cargaACompletar, int facturaNumero, string facturaLetra, int facturaPuntoDeVenta, string facturaTipo)
        {
            InitializeComponent();
            btnGuardar.Visible = false;
            fecha = dateTimePicker1.Value;
            _facturaNumero = facturaNumero;
            _facturaLetra = facturaLetra;
            _facturaPuntoDeVenta = facturaPuntoDeVenta;
            _facturaTipo = facturaTipo;
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

        private void btnGuardar_Click_1(object sender, EventArgs e)
        {

            List<object[]> filasAuxnd = new List<object[]>();

            using (conexion)
            {
                conexion.Open();

                // Selecciona registros de la tabla auxnd
                string querySelect = "SELECT id_notadecredito, motivorefactura, importerefactura, codigo, usuario, id_prestacion, comentarios, tiporegistro FROM auxnd";

                using (NpgsqlCommand comandoSelect = new NpgsqlCommand(querySelect, conexion))
                {
                    using (NpgsqlDataReader lector = comandoSelect.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            filasAuxnd.Add(new object[]
                            {
                                lector["id_notadecredito"],
                                lector["motivorefactura"],
                                lector["importerefactura"],
                                lector["codigo"],
                                lector["usuario"],
                                lector["id_prestacion"],
                                lector["comentarios"],
                                lector["tiporegistro"]
                            });
                        }
                    }
                }

                // Consulta para obtener las filas incompletas
                string querySelectIncompletos = @"SELECT id_prestacion FROM cargaincompleta 
                                      WHERE numero = @FacturaNumero AND tipodocumento = @TipoDocumento 
                                      AND letra = @FacturaLetra AND ptovta = @FacturaPuntoDeVenta";

                List<object> filasIncompletas = new List<object>();

                using (NpgsqlCommand comandoSelectIncompletos = new NpgsqlCommand(querySelectIncompletos, conexion))
                {
                    comandoSelectIncompletos.Parameters.AddWithValue("@FacturaNumero", _facturaNumero);
                    comandoSelectIncompletos.Parameters.AddWithValue("@TipoDocumento", _facturaTipo);
                    comandoSelectIncompletos.Parameters.AddWithValue("@FacturaLetra", _facturaLetra);
                    comandoSelectIncompletos.Parameters.AddWithValue("@FacturaPuntoDeVenta", _facturaPuntoDeVenta);

                    using (NpgsqlDataReader lectorIncompletos = comandoSelectIncompletos.ExecuteReader())
                    {
                        while (lectorIncompletos.Read())
                        {
                            filasIncompletas.Add(lectorIncompletos["id_prestacion"]);
                        }
                    }
                }

                foreach (var fila in filasAuxnd)
                {
                    object idPrestacion = fila[5];

                    if (filasIncompletas.Contains(idPrestacion))
                    {
                        string queryActualizarRegistros = @"UPDATE notadedebito 
                                                SET tipo = @tipo, letra = @letra, ptovta = @ptovta, numero = @numero, fecha = @fecha, cargadocompletamente = @cargadocompletamente
                                                WHERE id_prestacion = @id_prestacion AND cargadocompletamente = @cargarcompletamente";

                        using (NpgsqlCommand comandoActualizar = new NpgsqlCommand(queryActualizarRegistros, conexion))
                        {
                            comandoActualizar.Parameters.AddWithValue("@id_prestacion", idPrestacion);
                            comandoActualizar.Parameters.AddWithValue("@tipo", tipoDeArchivo);
                            comandoActualizar.Parameters.AddWithValue("@letra", letra);
                            comandoActualizar.Parameters.AddWithValue("@ptovta", puntoDeVentaE);
                            comandoActualizar.Parameters.AddWithValue("@numero", numeroE);
                            comandoActualizar.Parameters.AddWithValue("@fecha", fecha);
                            comandoActualizar.Parameters.AddWithValue("@cargadocompletamente", true);
                            comandoActualizar.Parameters.AddWithValue("@cargarcompletamente", false);

                            comandoActualizar.ExecuteNonQuery();
                        }

                        string queryDeleteIncompletos = @"DELETE FROM cargaincompleta WHERE id_prestacion = @id_prestacion";

                        using (NpgsqlCommand comandoDeleteIncompletos = new NpgsqlCommand(queryDeleteIncompletos, conexion))
                        {
                            comandoDeleteIncompletos.Parameters.AddWithValue("@id_prestacion", idPrestacion);
                            comandoDeleteIncompletos.ExecuteNonQuery();
                        }
                    }

                    else
                    {
                        string queryInsertarNuevoRegistro = @"INSERT INTO notadedebito 
                                                  (id_notadecredito, motivorefactura, importerefactura, codigo, usuario, tipo, letra, ptovta, numero, fecha, id_prestacion, comentarios, tiporegistro) 
                                                  VALUES 
                                                  (@id_notadecredito, @motivorefactura, @importerefactura, @codigo, @usuario, @tipo, @letra, @ptovta, @numero, @fecha, @id_prestacion, @comentarios, @tiporegistro)";

                        using (NpgsqlCommand comandoInsert = new NpgsqlCommand(queryInsertarNuevoRegistro, conexion))
                        {
                            comandoInsert.Parameters.AddWithValue("@id_notadecredito", fila[0]);
                            comandoInsert.Parameters.AddWithValue("@motivorefactura", fila[1]);
                            comandoInsert.Parameters.AddWithValue("@importerefactura", fila[2]);
                            comandoInsert.Parameters.AddWithValue("@codigo", fila[3]);
                            comandoInsert.Parameters.AddWithValue("@usuario", fila[4]);
                            comandoInsert.Parameters.AddWithValue("@comentarios", fila[6]);
                            comandoInsert.Parameters.AddWithValue("@tiporegistro", fila[7]);
                            comandoInsert.Parameters.AddWithValue("@tipo", tipoDeArchivo);
                            comandoInsert.Parameters.AddWithValue("@letra", letra);
                            comandoInsert.Parameters.AddWithValue("@ptovta", puntoDeVentaE);
                            comandoInsert.Parameters.AddWithValue("@numero", numeroE);
                            comandoInsert.Parameters.AddWithValue("@fecha", fecha);
                            comandoInsert.Parameters.AddWithValue("@id_prestacion", fila[5]);

                            comandoInsert.ExecuteNonQuery();
                        }
                    }
                }

                // Eliminar registros de auxnd una vez procesados
                using (NpgsqlCommand comandoDelete = new NpgsqlCommand("DELETE FROM auxnd", conexion))
                {
                    comandoDelete.ExecuteNonQuery();
                }

                if (filasIncompletas.Count > 0)
                {
                    string queryEliminarFilasArchivoParcial = @"DELETE FROM cargaincompleta WHERE numero = @FacturaNumero AND tipodocumento = @TipoDocumento AND letra = @FacturaLetra AND ptovta = @FacturaPuntoDeVenta";
                    using (NpgsqlCommand comandoLimpieza = new NpgsqlCommand(queryEliminarFilasArchivoParcial, conexion))
                    {
                        comandoLimpieza.Parameters.AddWithValue("@FacturaNumero", _facturaNumero);
                        comandoLimpieza.Parameters.AddWithValue("@TipoDocumento", _facturaTipo);
                        comandoLimpieza.Parameters.AddWithValue("@FacturaLetra", _facturaLetra);
                        comandoLimpieza.Parameters.AddWithValue("@FacturaPuntoDeVenta", _facturaPuntoDeVenta);

                        comandoLimpieza.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Se ha creado correctamente la nota de débito");
                IngresoInformacionNotaDeDebito.ActiveForm.Close();
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
