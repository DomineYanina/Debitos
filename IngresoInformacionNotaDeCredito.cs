using Npgsql;
using System.Windows.Forms;

namespace Debitos
{
    public partial class IngresoInformacionNotaDeCredito : Form
    {
        private String letra = "";
        private String tipoDeArchivo = "";
        private int numeroE, puntoDeVentaE;
        private DateTime fecha;
        private bool puntoDeVentaSeleccionado, letraSeleccionada, tipoDeArchivoSeleccionado, numeroSeleccionado = false;
        string queryDelete = "DELETE FROM auxnc";
        private bool _cargaACompletar;
        private int _facturaNumero;
        private String _facturaLetra;
        private int _facturaPuntoDeVenta;
        private String _facturaTipo;

        NpgsqlConnection conexion = new NpgsqlConnection("Host=172.16.13.219;Port=5432;Username=postgres;Password=postgres;Database=Debitos;");

        public IngresoInformacionNotaDeCredito(bool cargaACompletar, int facturaNumero, String facturaLetra, int facturaPuntoDeVenta, String facturaTipo)
        {
            InitializeComponent();
            _cargaACompletar = cargaACompletar;
            _facturaNumero = facturaNumero;
            _facturaLetra = facturaLetra;
            _facturaPuntoDeVenta = facturaPuntoDeVenta;
            _facturaTipo = facturaTipo;
            btnGuardar.Visible = false;
            fecha = dateTimePicker1.Value;
        }


        private void txtPuntoDeVenta_TextChanged(object sender, EventArgs e)
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

        private void txtLetra_TextChanged(object sender, EventArgs e)
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

        private void txtNumero_TextChanged(object sender, EventArgs e)
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

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            fecha = dateTimePicker1.Value;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            List<object[]> filasAuxnc = new List<object[]>();

            using (conexion)
            {
                conexion.Open();

                string queryCreacionRelacion = @"INSERT INTO relaciones
                    (tipo_doc_origen, ptovta_origen, letra_origen, numero_origen, tipo_doc_destino, ptovta_destino, letra_destino, numero_destino)
                    VALUES
                    (@tipo_doc_origen, @ptovta_origen, @letra_origen, @numero_origen, @tipo_doc_destino, @ptovta_destino, @letra_destino, @numero_destino);";

                string querySelect = "SELECT id_prestacion, motivodedebito, diasfacturados, importedebitado, debitoaceptado, motivoderefactura, importederefactura, prestacionenglobante, usuario, comentarios, tiporegistro FROM auxnc";

                using (NpgsqlCommand comandoSelect = new NpgsqlCommand(querySelect, conexion))
                {
                    using (NpgsqlDataReader lector = comandoSelect.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            filasAuxnc.Add(new object[]
                            {
                                lector["id_prestacion"],
                                lector["motivodedebito"],
                                lector["diasfacturados"],
                                lector["importedebitado"],
                                lector["debitoaceptado"],
                                lector["motivoderefactura"],
                                lector["importederefactura"],
                                lector["prestacionenglobante"],
                                lector["usuario"],
                                lector["comentarios"],
                                lector["tiporegistro"]
                            });
                        }
                    }
                }

                string querySelectIncompletos = @"SELECT id_prestacion FROM cargaincompleta where numero = @FacturaNumero AND tipodocumento = @TipoDocumento AND letra = @FacturaLetra AND ptovta = @FacturaPuntoDeVenta";

                List<object> filasIncompletas = new List<object>();

                using (NpgsqlCommand comandoSelectIncompletos = new NpgsqlCommand(querySelectIncompletos, conexion))
                {
                    // Parámetro para la consulta SQL
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

                bool encontrado = false;

                foreach (object[] fila in filasAuxnc)
                {

                    object idPrestacion = fila[0];

                    if (filasIncompletas.Contains(idPrestacion))
                    {
                        encontrado = true;
                        string queryActualizarRegistros = @"UPDATE notadecredito 
                                        SET tipo = @tipo, letra = @letra, ptovta = @ptovta, numero = @numero, fecha = @fecha, cargadocompletamente = @cargadocompletamente
                                        WHERE id_prestacion = @id_prestacion AND cargadocompletamente = @cargarcompletamente;";

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
                        string queryInsertarNuevoRegistro = @"INSERT INTO notadecredito 
                (id_prestacion, motivodedebito, diasfacturados, importedebitado, debitoaceptado, motivoderefactura, importederefactura, prestacionenglobante, usuario, tipo, letra, ptovta, numero, fecha, comentarios, tiporegistro, cargadocompletamente) 
                VALUES 
                (@id_prestacion, @motivodedebito, @diasfacturados, @importedebitado, @debitoaceptado, @motivoderefactura, @importederefactura, @prestacionenglobante, @usuario, @tipo, @letra, @ptovta, @numero, @fecha, @comentarios, @tiporegistro, true)
                ON CONFLICT (id_prestacion) 
                DO UPDATE SET 
                tipo = EXCLUDED.tipo, 
                letra = EXCLUDED.letra, 
                ptovta = EXCLUDED.ptovta, 
                numero = EXCLUDED.numero, 
                fecha = EXCLUDED.fecha,
                cargadocompletamente = true,
                motivodedebito = EXCLUDED.motivodedebito, 
                diasfacturados = EXCLUDED.diasfacturados, 
                importedebitado = EXCLUDED.importedebitado, 
                debitoaceptado = EXCLUDED.debitoaceptado, 
                motivoderefactura = EXCLUDED.motivoderefactura, 
                importederefactura = EXCLUDED.importederefactura, 
                prestacionenglobante = EXCLUDED.prestacionenglobante, 
                usuario = EXCLUDED.usuario, 
                comentarios = EXCLUDED.comentarios, 
                tiporegistro = EXCLUDED.tiporegistro;";

                        using (NpgsqlCommand comandoInsert = new NpgsqlCommand(queryInsertarNuevoRegistro, conexion))
                        {
                            comandoInsert.Parameters.AddWithValue("@id_prestacion", fila[0]);
                            comandoInsert.Parameters.AddWithValue("@motivodedebito", fila[1]);
                            // ... (el resto de tus AddWithValue quedan exactamente igual que antes) ...
                            comandoInsert.Parameters.AddWithValue("@diasfacturados", fila[2]);
                            comandoInsert.Parameters.AddWithValue("@importedebitado", fila[3]);
                            comandoInsert.Parameters.AddWithValue("@debitoaceptado", fila[4]);
                            comandoInsert.Parameters.AddWithValue("@motivoderefactura", fila[5]);
                            comandoInsert.Parameters.AddWithValue("@importederefactura", fila[6]);
                            comandoInsert.Parameters.AddWithValue("@prestacionenglobante", fila[7]);
                            comandoInsert.Parameters.AddWithValue("@usuario", fila[8]);
                            comandoInsert.Parameters.AddWithValue("@comentarios", fila[9]);
                            comandoInsert.Parameters.AddWithValue("@tiporegistro", fila[10]);
                            comandoInsert.Parameters.AddWithValue("@letra", letra);
                            comandoInsert.Parameters.AddWithValue("@ptovta", puntoDeVentaE);
                            comandoInsert.Parameters.AddWithValue("@numero", numeroE);
                            comandoInsert.Parameters.AddWithValue("@fecha", fecha);
                            comandoInsert.Parameters.AddWithValue("@tipo", tipoDeArchivo);

                            comandoInsert.ExecuteNonQuery();
                        }
                    }
                    if (filasIncompletas.Count > 0) {
                        string queryEliminarFilasArchivoParcial = @"DELETE FROM cargaincompleta WHERE numero = @FacturaNumero AND tipodocumento = @TipoDocumento AND letra = @FacturaLetra AND ptovta = @FacturaPuntoDeVenta";
                        using (NpgsqlCommand comandoLimpieza = new NpgsqlCommand(queryEliminarFilasArchivoParcial,conexion))
                        {
                            comandoLimpieza.Parameters.AddWithValue("@FacturaNumero", _facturaNumero);
                            comandoLimpieza.Parameters.AddWithValue("@TipoDocumento", _facturaTipo);
                            comandoLimpieza.Parameters.AddWithValue("@FacturaLetra", _facturaLetra);
                            comandoLimpieza.Parameters.AddWithValue("@FacturaPuntoDeVenta", _facturaPuntoDeVenta);

                            comandoLimpieza.ExecuteNonQuery ();
                        }
                    }
                    
                }

                if (!encontrado)
                {
                    using (NpgsqlCommand comandoNuevaRelacion = new NpgsqlCommand(queryCreacionRelacion, conexion))
                    {
                        comandoNuevaRelacion.Parameters.AddWithValue("@numero_origen", _facturaNumero);
                        comandoNuevaRelacion.Parameters.AddWithValue("@tipo_doc_origen", _facturaTipo);
                        comandoNuevaRelacion.Parameters.AddWithValue("@letra_origen", _facturaLetra);
                        comandoNuevaRelacion.Parameters.AddWithValue("@ptovta_origen", _facturaPuntoDeVenta);
                        comandoNuevaRelacion.Parameters.AddWithValue("@letra_destino", letra);
                        comandoNuevaRelacion.Parameters.AddWithValue("@ptovta_destino", puntoDeVentaE);
                        comandoNuevaRelacion.Parameters.AddWithValue("@numero_destino", numeroE);
                        comandoNuevaRelacion.Parameters.AddWithValue("@tipo_doc_destino", tipoDeArchivo);

                        comandoNuevaRelacion.ExecuteNonQuery();
                    }
                }

                using (NpgsqlCommand comandoDelete = new NpgsqlCommand(queryDelete, conexion))
                {
                    comandoDelete.ExecuteNonQuery();
                }

                conexion.Close();

                MessageBox.Show("Se ha creado correctamente la nota de crédito");

                IngresoInformacionNotaDeCredito.ActiveForm.Close();

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
