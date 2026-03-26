using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Debitos
{
    public partial class VerHistorialDelDocumento : Form
    {
        private int _facturaNumero;
        private String _facturaLetra;
        private int _facturaPuntoDeVenta;
        private String _facturaTipo;
        private String comandoBusqueda = "";
        private DataTable resultadoAux = new DataTable();
        private DataTable resultadoAux2 = new DataTable();
        private DataTable resultadoAuxSig = new DataTable();
        private DataTable resultado = new DataTable();

        NpgsqlConnection conexion = new NpgsqlConnection(DatabaseConfig.ConnectionString);

        private string tipo_doc = "";
        private string letra = "";
        private int numero = 0;
        private int ptovta = 0;

        public VerHistorialDelDocumento(int facturaNumero, String facturaLetra, int facturaPuntoDeVenta, String facturaTipo)
        {
            InitializeComponent();
            _facturaNumero = facturaNumero;
            _facturaLetra = facturaLetra;
            _facturaPuntoDeVenta = facturaPuntoDeVenta;
            _facturaTipo = facturaTipo;
            tipo_doc = "";
            letra = "";
            numero = 0;
            ptovta = 0;
            main();
        }

        public void main()
        {
            /*using (conexion)
            {
                conexion.Open();
                if (_facturaTipo == "FC")
                {
                    buscarRegistroFactura();
                }
                else
                {
                    buscarArchivosAnteriores();
                }

                yaEncontradaLaFactura();
            }

            dataGridView1.DataSource = resultado;
            dataGridView1.Columns["id"].Visible = false;*/
        }

        private void buscarRegistroFactura()
        {
            comandoBusqueda = @"SELECT * FROM relaciones WHERE tipo_doc_origen = @tipoDocOrigen AND letra_origen = @letraOrigen AND numero_origen = @numeroDocOrigen AND ptovta_origen = @puntoVentaOrigen;";

            using (NpgsqlCommand comandoBusquedaPrevia = new NpgsqlCommand(comandoBusqueda, conexion))
            {
                comandoBusquedaPrevia.Parameters.AddWithValue("@tipoDocOrigen", _facturaTipo);
                comandoBusquedaPrevia.Parameters.AddWithValue("@letraOrigen", _facturaLetra);
                comandoBusquedaPrevia.Parameters.AddWithValue("@numeroDocOrigen", _facturaNumero);
                comandoBusquedaPrevia.Parameters.AddWithValue("@puntoVentaOrigen", _facturaPuntoDeVenta);

                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(comandoBusquedaPrevia))
                {
                    adapter.Fill(resultado);
                    foreach (DataRow row in resultado.Rows)
                    {
                        tipo_doc = row["tipo_doc_destino"].ToString(); // Convierte a string
                        numero = Convert.ToInt32(row["numero_destino"]);
                        ptovta = Convert.ToInt32(row["ptovta_destino"]);
                        letra = row["letra_destino"].ToString();
                    }
                }
            }
            //conexion.Close();
            //}
        }

        private void yaEncontradaLaFactura()
        {
            comandoBusqueda = @"SELECT * FROM relaciones WHERE tipo_doc_origen = @tipoDocOrigen AND letra_origen = @letraOrigen AND numero_origen = @numeroDocOrigen AND ptovta_origen = @puntoVentaOrigen;";

            int cantidadDeRelaciones;

            string queryObtenerCantidadDeRelacionesCreadas = "SELECT COUNT(*) FROM relaciones;";
            //using (conexion)
            //{
            //conexion.Open();
            using (NpgsqlCommand averiguarCantidadDeRelaciones = new NpgsqlCommand(queryObtenerCantidadDeRelacionesCreadas, conexion))
            {
                cantidadDeRelaciones = Convert.ToInt32(averiguarCantidadDeRelaciones.ExecuteScalar());
            }

            bool controlador = false;

            do
            {
                using (NpgsqlCommand comandoBuscarDocumentosSiguientes = new NpgsqlCommand(comandoBusqueda, conexion))
                {
                    comandoBuscarDocumentosSiguientes.Parameters.AddWithValue("@letraOrigen", letra);
                    comandoBuscarDocumentosSiguientes.Parameters.AddWithValue("@tipoDocOrigen", tipo_doc);
                    comandoBuscarDocumentosSiguientes.Parameters.AddWithValue("@numeroDocOrigen", numero);
                    comandoBuscarDocumentosSiguientes.Parameters.AddWithValue("@puntoVentaOrigen", ptovta);

                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(comandoBuscarDocumentosSiguientes))
                    {
                        adapter.Fill(resultadoAuxSig);
                    }

                    if (resultadoAuxSig.Rows.Count > 0)
                    {
                        foreach (DataRow row in resultadoAuxSig.Rows)
                        {
                            resultado.Rows.Add(row.ItemArray); // Agrega los valores de la fila
                            tipo_doc = row["tipo_doc_destino"].ToString(); // Convierte a string
                            numero = Convert.ToInt32(row["numero_destino"]);
                            ptovta = Convert.ToInt32(row["ptovta_destino"]);
                            letra = row["letra_destino"].ToString();
                        }
                        resultadoAuxSig.Clear();
                    }
                    else
                    {
                        controlador = true;
                    }
                }

            } while (!controlador);
            //}

        }

        private void buscarArchivosAnteriores()
        {
            comandoBusqueda = @"SELECT * FROM relaciones WHERE tipo_doc_destino = @tipo_doc_destino AND letra_destino = @letra_destino AND numero_destino = @numero_destino AND ptovta_destino = @ptovta_destino;";

            //using (conexion)
            //{
            //conexion.Open();

            using (NpgsqlCommand comandoBusquedaPrevia = new NpgsqlCommand(comandoBusqueda, conexion))
            {
                comandoBusquedaPrevia.Parameters.AddWithValue("@tipo_doc_destino", _facturaTipo);
                comandoBusquedaPrevia.Parameters.AddWithValue("@letra_destino", _facturaLetra);
                comandoBusquedaPrevia.Parameters.AddWithValue("@numero_destino", _facturaNumero);
                comandoBusquedaPrevia.Parameters.AddWithValue("@ptovta_destino", _facturaPuntoDeVenta);

                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(comandoBusquedaPrevia))
                {
                    adapter.Fill(resultadoAux);
                }
            }

            bool encontrado = false;

            if (resultadoAux != null && resultadoAux.Rows.Count > 0) // Verifica que haya datos
            {
                foreach (DataRow fila in resultadoAux.Rows)
                {
                    if (fila["tipo_doc_origen"] != DBNull.Value) // Verifica que el campo no sea nulo
                    {
                        string tipoDocOrigen = fila["tipo_doc_origen"].ToString(); // Convierte a string
                        int puntoVentaOrigen = Convert.ToInt32(fila["ptovta_origen"]);
                        int numeroDocOrigen = Convert.ToInt32(fila["numero_origen"]);
                        string letraOrigen = fila["letra_origen"].ToString();

                        if (tipoDocOrigen == "FC")
                        {
                            DataRow nuevaFila = resultado.NewRow();
                            nuevaFila.ItemArray = fila.ItemArray.Clone() as object[]; // Copia los valores
                            resultado.Rows.Add(nuevaFila);

                            encontrado = true;
                            break;
                        }
                        else
                        {
                            comandoBusqueda = @"SELECT * FROM relaciones WHERE tipo_doc_destino = @tipoDocOrigen AND letra_destino = @letraOrigen AND numero_destino = @numeroDocOrigen AND ptovta_destino = @puntoVentaOrigen;";

                            do
                            {
                                resultadoAux2.Clear();
                                using (NpgsqlCommand comandoABuscarLaFuckingFactura = new NpgsqlCommand(comandoBusqueda, conexion))
                                {
                                    comandoABuscarLaFuckingFactura.Parameters.AddWithValue("@letraOrigen", letraOrigen);
                                    comandoABuscarLaFuckingFactura.Parameters.AddWithValue("@puntoVentaOrigen", puntoVentaOrigen);
                                    comandoABuscarLaFuckingFactura.Parameters.AddWithValue("@numeroDocOrigen", numeroDocOrigen);
                                    comandoABuscarLaFuckingFactura.Parameters.AddWithValue("@tipoDocOrigen", tipoDocOrigen);

                                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(comandoABuscarLaFuckingFactura))
                                    {
                                        adapter.Fill(resultadoAux2);
                                    }

                                    foreach (DataRow row in resultadoAux2.Rows)
                                    {
                                        tipoDocOrigen = row["tipo_doc_origen"].ToString(); // Convierte a string
                                        numeroDocOrigen = Convert.ToInt32(row["numero_origen"]);
                                        puntoVentaOrigen = Convert.ToInt32(row["ptovta_origen"]);
                                        letraOrigen = row["letra_origen"].ToString();

                                        if (tipoDocOrigen == "FC")
                                        {
                                            //resultado.Rows.Add(row);
                                            resultado = resultadoAux2;
                                            tipo_doc = row["tipo_doc_destino"].ToString(); // Convierte a string
                                            numero = Convert.ToInt32(row["numero_destino"]);
                                            ptovta = Convert.ToInt32(row["ptovta_destino"]);
                                            letra = row["letra_destino"].ToString();
                                            encontrado = true;
                                        }
                                    }
                                }
                            } while (!encontrado);
                        }
                    }
                }
            }

            //}


        }
    }
}
