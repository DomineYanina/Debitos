using Npgsql;
using System.Data;
using System;

namespace Debitos.Repositories
{
    public class DebitosRepository
    {
        private readonly string _connectionString;

        public DebitosRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        // Método genérico para ejecutar consultas (basado en el que ya tenías)
        private DataTable ExecuteQuery(string query, NpgsqlParameter[] parameters = null)
        {
            DataTable dataTable = new DataTable();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(query, connection))
                {
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            return dataTable;
        }

        // Movemos la lógica de BuscarDocumentoYTipoRegistro acá
        public string ObtenerTipoRegistro(string tipoFactura, string letra, int ptoVta, int numero)
        {
            string query = tipoFactura switch
            {
                "FC" => "SELECT DISTINCT tiporegistro FROM amb_liquidado WHERE cob_factura_letra = @letra AND cob_factura_ptoventa = @ptovta AND cob_factura_numero = @numero;",
                "NC" => "SELECT DISTINCT tiporegistro FROM notadecredito WHERE letra = @letra AND ptovta = @ptovta AND numero = @numero",
                "ND" => "SELECT DISTINCT tiporegistro FROM notadedebito WHERE letra = @letra AND ptovta = @ptovta AND numero = @numero",
                _ => throw new InvalidOperationException("Tipo de factura desconocido")
            };

            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@letra", letra),
                new NpgsqlParameter("@ptovta", ptoVta),
                new NpgsqlParameter("@numero", numero)
            };

            var dt = ExecuteQuery(query, parameters);
            return dt.Rows.Count > 0 ? dt.Rows[0][0].ToString() : null;
        }

        // Movemos la lógica de CargarDatosDocumento acá
        public DataTable ObtenerPrestacionesDocumento(string comandoSqlServer, string letra, int ptoVta, int numero)
        {
            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@FacturaLetra", letra),
                new NpgsqlParameter("@FacturaPuntoVenta", ptoVta),
                new NpgsqlParameter("@FacturaNumero", numero)
            };

            return ExecuteQuery(comandoSqlServer, parameters);
        }
    }
}