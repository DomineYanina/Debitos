using Debitos.Models;
using Npgsql;
using System;
using System.Data;

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

        public void LimpiarAuxiliarNC()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var comando = new NpgsqlCommand("DELETE FROM auxnc", connection);
            comando.ExecuteNonQuery();
        }

        public void LimpiarAuxiliarND()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var comando = new NpgsqlCommand("DELETE FROM auxnd", connection);
            comando.ExecuteNonQuery();
        }

        public void InsertarAuxiliarNC_FC(List<(int idPrestacion, object? motivoRefactura, object? motivoDebito, double? importeRefactura, double? importeDebito, string? comentarios, bool debitoAceptado, object? diasFacturados, string? prestacionEnglobante, string? codigo)> lista, string usuario, string tipoRegistro)
        {
            string comando = @"INSERT INTO auxnc 
        (id_prestacion, motivodedebito, diasfacturados, importedebitado, debitoaceptado, motivoderefactura, importederefactura, prestacionenglobante, usuario, comentarios, tiporegistro) 
        VALUES (@id_prestacion, @motivodedebito, @diasfacturados, @importedebitado, @debitoaceptado, @motivoderefactura, @importederefactura, @prestacionenglobante, @usuario, @comentarios, @tiporegistro);";

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                foreach (var item in lista)
                {
                    using var command = new NpgsqlCommand(comando, connection, transaction);
                    command.Parameters.AddWithValue("@id_prestacion", item.idPrestacion);
                    command.Parameters.AddWithValue("@motivodedebito", item.motivoDebito ?? DBNull.Value);
                    command.Parameters.AddWithValue("@diasfacturados", item.diasFacturados ?? DBNull.Value);
                    command.Parameters.AddWithValue("@importedebitado", item.importeDebito ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@debitoaceptado", item.debitoAceptado);
                    command.Parameters.AddWithValue("@motivoderefactura", item.motivoRefactura ?? DBNull.Value);

                    // Acá está la solución al error:
                    command.Parameters.AddWithValue("@prestacionenglobante", (object?)item.prestacionEnglobante ?? DBNull.Value);

                    command.Parameters.AddWithValue("@importederefactura", item.importeRefactura ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@usuario", usuario);
                    command.Parameters.AddWithValue("@tiporegistro", tipoRegistro);
                    command.Parameters.AddWithValue("@comentarios", (object?)item.comentarios ?? "");
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void InsertarAuxiliarNC_ND(List<(int idPrestacion, object? motivoRefactura, object? motivoDebito, double? importeRefactura, double? importeDebito, string? comentarios, bool debitoAceptado, object? diasFacturados, string? prestacionEnglobante, int? idNotaDeDebito)> lista, string usuario, string tipoRegistro)
        {
            string comandoND = @"INSERT INTO auxnc 
    (id_prestacion, motivodedebito, diasfacturados, importedebitado, debitoaceptado, motivoderefactura, importederefactura, usuario, id_notadedebito, comentarios, tiporegistro) 
    VALUES (@id_prestacion, @motivodedebito, @diasfacturados, @importedebitado, @debitoaceptado, @motivoderefactura, @importederefactura, @usuario, @id_notadedebito, @comentarios, @tiporegistro);";

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                foreach (var item in lista)
                {
                    using var commandND = new NpgsqlCommand(comandoND, connection, transaction);
                    commandND.Parameters.AddWithValue("@id_prestacion", item.idPrestacion);
                    commandND.Parameters.AddWithValue("@motivodedebito", item.motivoDebito ?? DBNull.Value);
                    commandND.Parameters.AddWithValue("@diasfacturados", item.diasFacturados ?? DBNull.Value);
                    commandND.Parameters.AddWithValue("@importedebitado", item.importeDebito ?? (object)DBNull.Value);
                    commandND.Parameters.AddWithValue("@debitoaceptado", item.debitoAceptado);
                    commandND.Parameters.AddWithValue("@motivoderefactura", item.motivoRefactura ?? DBNull.Value);
                    commandND.Parameters.AddWithValue("@importederefactura", item.importeRefactura ?? (object)DBNull.Value);
                    commandND.Parameters.AddWithValue("@usuario", usuario);
                    commandND.Parameters.AddWithValue("@tiporegistro", tipoRegistro);
                    commandND.Parameters.AddWithValue("@id_notadedebito", item.idNotaDeDebito ?? (object)DBNull.Value);
                    commandND.Parameters.AddWithValue("@comentarios", item.comentarios ?? "");
                    commandND.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void InsertarAuxiliarND(List<(int idPrestacion, object motivoRefactura, double importeRefactura, string? comentarios, int idNotaDeCredito, string? codigo)> lista, string usuario, string tipoRegistro)
        {
            string comando = @"INSERT INTO auxnd 
    (id_notadecredito, motivorefactura, importerefactura, codigo, usuario, id_prestacion, comentarios, tiporegistro) 
    VALUES (@id_notadecredito, @motivorefactura, @importerefactura, @codigo, @usuario, @id_prestacion, @comentarios, @tiporegistro);";

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                foreach (var item in lista)
                {
                    using var command = new NpgsqlCommand(comando, connection, transaction);
                    command.Parameters.AddWithValue("@id_notadecredito", item.idNotaDeCredito);
                    command.Parameters.AddWithValue("@motivorefactura", item.motivoRefactura ?? DBNull.Value);
                    command.Parameters.AddWithValue("@importerefactura", item.importeRefactura);
                    command.Parameters.AddWithValue("@codigo", string.IsNullOrEmpty(item.codigo) ? DBNull.Value : item.codigo);
                    command.Parameters.AddWithValue("@usuario", usuario);
                    command.Parameters.AddWithValue("@tiporegistro", tipoRegistro);
                    command.Parameters.AddWithValue("@id_prestacion", item.idPrestacion);
                    command.Parameters.AddWithValue("@comentarios", item.comentarios ?? "");
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void GuardarCargaParcialFC(List<CargaParcialDTO> listaCarga)
        {
            // El comando UPSERT nativo de PostgreSQL
            string comandoUpsert = @"
        INSERT INTO notadecredito 
        (id_prestacion, debitoaceptado, motivodedebito, importedebitado, 
        diasfacturados, motivoderefactura, importederefactura, prestacionenglobante, comentarios, 
        cargadocompletamente, usuario) 
        VALUES (@id_prestacion, @debitoaceptado, @motivodedebito, 
        @importedebitado, @diasfacturados, @motivoderefactura, @importederefactura, @prestacionenglobante, 
        @comentarios, @cargadocompletamente, @usuario)
        ON CONFLICT (id_prestacion) 
        DO UPDATE SET 
        debitoaceptado = EXCLUDED.debitoaceptado,
        motivodedebito = EXCLUDED.motivodedebito,
        importedebitado = EXCLUDED.importedebitado,
        diasfacturados = EXCLUDED.diasfacturados,
        motivoderefactura = EXCLUDED.motivoderefactura,
        importederefactura = EXCLUDED.importederefactura,
        prestacionenglobante = EXCLUDED.prestacionenglobante,
        comentarios = EXCLUDED.comentarios,
        cargadocompletamente = EXCLUDED.cargadocompletamente,
        usuario = EXCLUDED.usuario;";

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                foreach (var item in listaCarga)
                {
                    using var command = new NpgsqlCommand(comandoUpsert, connection, transaction);

                    command.Parameters.AddWithValue("@id_prestacion", item.IdPrestacion);
                    command.Parameters.AddWithValue("@debitoaceptado", item.DebitoAceptado);
                    command.Parameters.AddWithValue("@motivodedebito", item.MotivoDebito ?? DBNull.Value);
                    command.Parameters.AddWithValue("@importedebitado", item.ImporteDebitado ?? DBNull.Value);
                    command.Parameters.AddWithValue("@diasfacturados", item.DiasFacturados ?? DBNull.Value);
                    command.Parameters.AddWithValue("@motivoderefactura", item.MotivoRefactura ?? DBNull.Value);
                    command.Parameters.AddWithValue("@importederefactura", item.ImporteRefactura ?? DBNull.Value);
                    command.Parameters.AddWithValue("@prestacionenglobante", (object?)item.PrestacionEnglobante ?? DBNull.Value);
                    command.Parameters.AddWithValue("@comentarios", (object?)item.Comentarios ?? "");
                    command.Parameters.AddWithValue("@cargadocompletamente", item.CargadoCompletamente);
                    command.Parameters.AddWithValue("@usuario", item.Usuario);

                    command.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}