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

        public void GuardarCargaParcialNC(List<CargaParcialDTO> listaCarga, string tipoDocumento, string letra, int ptovta, int numero)
        {
            string comandoUpdate = @"UPDATE notadedebito SET motivorefactura = @motivoderefactura, importerefactura = @importederefactura, usuario = @usuario, codigo = @codigo, cargadocompletamente = @cargadocompletamente, comentarios = @comentarios WHERE id_prestacion = @id_prestacion;";
            string comandoInsert = @"INSERT INTO notadedebito (id_notadecredito, motivorefactura, importerefactura, codigo, usuario, id_prestacion, cargadocompletamente, comentarios, tiporegistro) VALUES (@id_notadecredito, @motivoderefactura, @importederefactura, @codigo, @usuario, @id_prestacion, @cargadocompletamente, @comentarios, @tiporegistro);";

            // Para evitar la falta de constraints, borramos el registro si existe y lo volvemos a insertar limpio
            string comandoLimpiarCarga = @"DELETE FROM cargaincompleta WHERE id_prestacion = @id_prestacion AND tipodocumento = @tipodocumento;";
            string comandoInsertarCarga = @"INSERT INTO cargaincompleta (tipodocumento, letra, ptovta, numero, id_prestacion) VALUES (@tipodocumento, @letra, @ptovta, @numero, @id_prestacion);";

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                foreach (var item in listaCarga)
                {
                    // 1. Actualizamos Carga Incompleta
                    using var cmdDelCarga = new NpgsqlCommand(comandoLimpiarCarga, connection, transaction);
                    cmdDelCarga.Parameters.AddWithValue("@id_prestacion", item.IdPrestacion);
                    cmdDelCarga.Parameters.AddWithValue("@tipodocumento", tipoDocumento);
                    cmdDelCarga.ExecuteNonQuery();

                    using var cmdInsCarga = new NpgsqlCommand(comandoInsertarCarga, connection, transaction);
                    cmdInsCarga.Parameters.AddWithValue("@tipodocumento", tipoDocumento);
                    cmdInsCarga.Parameters.AddWithValue("@letra", letra);
                    cmdInsCarga.Parameters.AddWithValue("@ptovta", ptovta);
                    cmdInsCarga.Parameters.AddWithValue("@numero", numero);
                    cmdInsCarga.Parameters.AddWithValue("@id_prestacion", item.IdPrestacion);
                    cmdInsCarga.ExecuteNonQuery();

                    // 2. Intentamos el UPDATE de la Nota de Débito asociada a la NC
                    using var commandUpdate = new NpgsqlCommand(comandoUpdate, connection, transaction);
                    commandUpdate.Parameters.AddWithValue("@id_prestacion", item.IdPrestacion);
                    commandUpdate.Parameters.AddWithValue("@motivoderefactura", item.MotivoRefactura ?? DBNull.Value);
                    commandUpdate.Parameters.AddWithValue("@importederefactura", item.ImporteRefactura ?? DBNull.Value);
                    commandUpdate.Parameters.AddWithValue("@usuario", item.Usuario);
                    commandUpdate.Parameters.AddWithValue("@codigo", (object?)item.Codigo ?? DBNull.Value);
                    commandUpdate.Parameters.AddWithValue("@cargadocompletamente", item.CargadoCompletamente);
                    commandUpdate.Parameters.AddWithValue("@comentarios", (object?)item.Comentarios ?? "");

                    int filasAfectadas = commandUpdate.ExecuteNonQuery();

                    // 3. Si dio 0, no existía. Hacemos el INSERT.
                    if (filasAfectadas == 0)
                    {
                        using var commandInsert = new NpgsqlCommand(comandoInsert, connection, transaction);
                        commandInsert.Parameters.AddWithValue("@id_notadecredito", item.IdNotaDeCredito ?? (object)DBNull.Value);
                        commandInsert.Parameters.AddWithValue("@motivoderefactura", item.MotivoRefactura ?? DBNull.Value);
                        commandInsert.Parameters.AddWithValue("@importederefactura", item.ImporteRefactura ?? DBNull.Value);
                        commandInsert.Parameters.AddWithValue("@codigo", (object?)item.Codigo ?? DBNull.Value);
                        commandInsert.Parameters.AddWithValue("@usuario", item.Usuario);
                        commandInsert.Parameters.AddWithValue("@id_prestacion", item.IdPrestacion);
                        commandInsert.Parameters.AddWithValue("@cargadocompletamente", item.CargadoCompletamente);
                        commandInsert.Parameters.AddWithValue("@comentarios", (object?)item.Comentarios ?? "");
                        commandInsert.Parameters.AddWithValue("@tiporegistro", (object?)item.TipoRegistro ?? DBNull.Value);
                        commandInsert.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void GuardarCargaParcialND(List<CargaParcialDTO> listaCarga, string tipoDocumento, string letra, int ptovta, int numero)
        {
            string comandoUpdate = @"UPDATE notadecredito SET motivodedebito = @motivodedebito, diasfacturados = @diasfacturados, importedebitado = @importedebitado, debitoaceptado = @debitoaceptado, motivoderefactura = @motivoderefactura, importederefactura = @importederefactura, usuario = @usuario, cargadocompletamente = @cargadocompletamente, comentarios = @comentarios WHERE id_prestacion = @id_prestacion;";
            string comandoInsert = @"INSERT INTO notadecredito (id_prestacion, motivodedebito, diasfacturados, importedebitado, debitoaceptado, motivoderefactura, importederefactura, usuario, cargadocompletamente, id_notadedebito, comentarios) VALUES (@id_prestacion, @motivodedebito, @diasfacturados, @importedebitado, @debitoaceptado, @motivoderefactura, @importederefactura, @usuario, @cargadocompletamente, @id_notadedebito, @comentarios);";

            string comandoLimpiarCarga = @"DELETE FROM cargaincompleta WHERE id_prestacion = @id_prestacion AND tipodocumento = @tipodocumento;";
            string comandoInsertarCarga = @"INSERT INTO cargaincompleta (tipodocumento, letra, ptovta, numero, id_prestacion) VALUES (@tipodocumento, @letra, @ptovta, @numero, @id_prestacion);";

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                foreach (var item in listaCarga)
                {
                    using var cmdDelCarga = new NpgsqlCommand(comandoLimpiarCarga, connection, transaction);
                    cmdDelCarga.Parameters.AddWithValue("@id_prestacion", item.IdPrestacion);
                    cmdDelCarga.Parameters.AddWithValue("@tipodocumento", tipoDocumento);
                    cmdDelCarga.ExecuteNonQuery();

                    using var cmdInsCarga = new NpgsqlCommand(comandoInsertarCarga, connection, transaction);
                    cmdInsCarga.Parameters.AddWithValue("@tipodocumento", tipoDocumento);
                    cmdInsCarga.Parameters.AddWithValue("@letra", letra);
                    cmdInsCarga.Parameters.AddWithValue("@ptovta", ptovta);
                    cmdInsCarga.Parameters.AddWithValue("@numero", numero);
                    cmdInsCarga.Parameters.AddWithValue("@id_prestacion", item.IdPrestacion);
                    cmdInsCarga.ExecuteNonQuery();

                    using var commandUpdate = new NpgsqlCommand(comandoUpdate, connection, transaction);
                    commandUpdate.Parameters.AddWithValue("@id_prestacion", item.IdPrestacion);
                    commandUpdate.Parameters.AddWithValue("@motivodedebito", item.MotivoDebito ?? DBNull.Value);
                    commandUpdate.Parameters.AddWithValue("@diasfacturados", item.DiasFacturados ?? DBNull.Value);
                    commandUpdate.Parameters.AddWithValue("@importedebitado", item.ImporteDebitado ?? DBNull.Value);
                    commandUpdate.Parameters.AddWithValue("@debitoaceptado", item.DebitoAceptado);
                    commandUpdate.Parameters.AddWithValue("@motivoderefactura", item.MotivoRefactura ?? DBNull.Value);
                    commandUpdate.Parameters.AddWithValue("@importederefactura", item.ImporteRefactura ?? DBNull.Value);
                    commandUpdate.Parameters.AddWithValue("@usuario", item.Usuario);
                    commandUpdate.Parameters.AddWithValue("@cargadocompletamente", item.CargadoCompletamente);
                    commandUpdate.Parameters.AddWithValue("@comentarios", (object?)item.Comentarios ?? "");

                    int filasAfectadas = commandUpdate.ExecuteNonQuery();

                    if (filasAfectadas == 0)
                    {
                        using var commandInsert = new NpgsqlCommand(comandoInsert, connection, transaction);
                        commandInsert.Parameters.AddWithValue("@id_prestacion", item.IdPrestacion);
                        commandInsert.Parameters.AddWithValue("@motivodedebito", item.MotivoDebito ?? DBNull.Value);
                        commandInsert.Parameters.AddWithValue("@diasfacturados", item.DiasFacturados ?? DBNull.Value);
                        commandInsert.Parameters.AddWithValue("@importedebitado", item.ImporteDebitado ?? DBNull.Value);
                        commandInsert.Parameters.AddWithValue("@debitoaceptado", item.DebitoAceptado);
                        commandInsert.Parameters.AddWithValue("@motivoderefactura", item.MotivoRefactura ?? DBNull.Value);
                        commandInsert.Parameters.AddWithValue("@importederefactura", item.ImporteRefactura ?? DBNull.Value);
                        commandInsert.Parameters.AddWithValue("@usuario", item.Usuario);
                        commandInsert.Parameters.AddWithValue("@cargadocompletamente", item.CargadoCompletamente);
                        commandInsert.Parameters.AddWithValue("@id_notadedebito", item.IdNotaDeDebito ?? (object)DBNull.Value);
                        commandInsert.Parameters.AddWithValue("@comentarios", (object?)item.Comentarios ?? "");
                        commandInsert.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void ProcesarGuardadoNotaDeCredito(string tipoDeArchivo, string letraDestino, int ptovtaDestino, int numeroDestino, DateTime fecha, int facturaNumero, string facturaLetra, int facturaPuntoDeVenta, string facturaTipo)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Extraemos los registros temporales
                string querySelect = "SELECT id_prestacion, motivodedebito, diasfacturados, importedebitado, debitoaceptado, motivoderefactura, importederefactura, prestacionenglobante, usuario, comentarios, tiporegistro FROM auxnc";
                var filasAuxnc = new List<object[]>();
                using (var cmdSelect = new NpgsqlCommand(querySelect, connection, transaction))
                using (var lector = cmdSelect.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        filasAuxnc.Add(new object[] {
                        lector["id_prestacion"], lector["motivodedebito"], lector["diasfacturados"],
                        lector["importedebitado"], lector["debitoaceptado"], lector["motivoderefactura"],
                        lector["importederefactura"], lector["prestacionenglobante"], lector["usuario"],
                        lector["comentarios"], lector["tiporegistro"]
                    });
                    }
                }

                // 2. Extraemos los incompletos
                string querySelectIncompletos = @"SELECT id_prestacion FROM cargaincompleta WHERE numero = @FacturaNumero AND tipodocumento = @TipoDocumento AND letra = @FacturaLetra AND ptovta = @FacturaPuntoDeVenta";
                var filasIncompletas = new HashSet<int>();
                using (var cmdIncompletos = new NpgsqlCommand(querySelectIncompletos, connection, transaction))
                {
                    cmdIncompletos.Parameters.AddWithValue("@FacturaNumero", facturaNumero);
                    cmdIncompletos.Parameters.AddWithValue("@TipoDocumento", facturaTipo);
                    cmdIncompletos.Parameters.AddWithValue("@FacturaLetra", facturaLetra);
                    cmdIncompletos.Parameters.AddWithValue("@FacturaPuntoDeVenta", facturaPuntoDeVenta);
                    using (var lector = cmdIncompletos.ExecuteReader())
                    {
                        while (lector.Read())
                            filasIncompletas.Add(Convert.ToInt32(lector["id_prestacion"]));
                    }
                }

                bool encontrado = false;

                string queryActualizarRegistros = @"UPDATE notadecredito 
                SET tipo = @tipo, letra = @letra, ptovta = @ptovta, numero = @numero, fecha = @fecha, cargadocompletamente = @cargadocompletamente
                WHERE id_prestacion = @id_prestacion AND cargadocompletamente = @cargarcompletamente;";

                string queryDeleteIncompletos = @"DELETE FROM cargaincompleta WHERE id_prestacion = @id_prestacion;";

                string queryUpsertNuevoRegistro = @"INSERT INTO notadecredito 
                (id_prestacion, motivodedebito, diasfacturados, importedebitado, debitoaceptado, motivoderefactura, importederefactura, prestacionenglobante, usuario, tipo, letra, ptovta, numero, fecha, comentarios, tiporegistro, cargadocompletamente) 
                VALUES 
                (@id_prestacion, @motivodedebito, @diasfacturados, @importedebitado, @debitoaceptado, @motivoderefactura, @importederefactura, @prestacionenglobante, @usuario, @tipo, @letra, @ptovta, @numero, @fecha, @comentarios, @tiporegistro, true)
                ON CONFLICT (id_prestacion) 
                DO UPDATE SET 
                tipo = EXCLUDED.tipo, letra = EXCLUDED.letra, ptovta = EXCLUDED.ptovta, 
                numero = EXCLUDED.numero, fecha = EXCLUDED.fecha, cargadocompletamente = true,
                motivodedebito = EXCLUDED.motivodedebito, diasfacturados = EXCLUDED.diasfacturados, 
                importedebitado = EXCLUDED.importedebitado, debitoaceptado = EXCLUDED.debitoaceptado, 
                motivoderefactura = EXCLUDED.motivoderefactura, importederefactura = EXCLUDED.importederefactura, 
                prestacionenglobante = EXCLUDED.prestacionenglobante, usuario = EXCLUDED.usuario, 
                comentarios = EXCLUDED.comentarios, tiporegistro = EXCLUDED.tiporegistro;";

                // 3. Procesamos las prestaciones 
                foreach (var fila in filasAuxnc)
                {
                    int idPrestacion = Convert.ToInt32(fila[0]);

                    if (filasIncompletas.Contains(idPrestacion))
                    {
                        encontrado = true;
                        using (var cmdUpdate = new NpgsqlCommand(queryActualizarRegistros, connection, transaction))
                        {
                            cmdUpdate.Parameters.AddWithValue("@id_prestacion", idPrestacion);
                            cmdUpdate.Parameters.AddWithValue("@tipo", tipoDeArchivo);
                            cmdUpdate.Parameters.AddWithValue("@letra", letraDestino);
                            cmdUpdate.Parameters.AddWithValue("@ptovta", ptovtaDestino);
                            cmdUpdate.Parameters.AddWithValue("@numero", numeroDestino);
                            cmdUpdate.Parameters.AddWithValue("@fecha", fecha);
                            cmdUpdate.Parameters.AddWithValue("@cargadocompletamente", true);
                            cmdUpdate.Parameters.AddWithValue("@cargarcompletamente", false);
                            cmdUpdate.ExecuteNonQuery();
                        }

                        using (var cmdDelInc = new NpgsqlCommand(queryDeleteIncompletos, connection, transaction))
                        {
                            cmdDelInc.Parameters.AddWithValue("@id_prestacion", idPrestacion);
                            cmdDelInc.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (var cmdUpsert = new NpgsqlCommand(queryUpsertNuevoRegistro, connection, transaction))
                        {
                            cmdUpsert.Parameters.AddWithValue("@id_prestacion", fila[0]);
                            cmdUpsert.Parameters.AddWithValue("@motivodedebito", fila[1] ?? DBNull.Value);
                            cmdUpsert.Parameters.AddWithValue("@diasfacturados", fila[2] ?? DBNull.Value);
                            cmdUpsert.Parameters.AddWithValue("@importedebitado", fila[3] ?? DBNull.Value);
                            cmdUpsert.Parameters.AddWithValue("@debitoaceptado", fila[4]);
                            cmdUpsert.Parameters.AddWithValue("@motivoderefactura", fila[5] ?? DBNull.Value);
                            cmdUpsert.Parameters.AddWithValue("@importederefactura", fila[6] ?? DBNull.Value);
                            cmdUpsert.Parameters.AddWithValue("@prestacionenglobante", fila[7] ?? DBNull.Value);
                            cmdUpsert.Parameters.AddWithValue("@usuario", fila[8] ?? DBNull.Value);
                            cmdUpsert.Parameters.AddWithValue("@comentarios", fila[9] ?? DBNull.Value);
                            cmdUpsert.Parameters.AddWithValue("@tiporegistro", fila[10] ?? DBNull.Value);
                            cmdUpsert.Parameters.AddWithValue("@letra", letraDestino);
                            cmdUpsert.Parameters.AddWithValue("@ptovta", ptovtaDestino);
                            cmdUpsert.Parameters.AddWithValue("@numero", numeroDestino);
                            cmdUpsert.Parameters.AddWithValue("@fecha", fecha);
                            cmdUpsert.Parameters.AddWithValue("@tipo", tipoDeArchivo);
                            cmdUpsert.ExecuteNonQuery();
                        }
                    }
                }

                // 4. Ejecutamos la limpieza una sola vez fuera del bucle
                if (filasIncompletas.Count > 0)
                {
                    string queryEliminarFilasArchivoParcial = @"DELETE FROM cargaincompleta WHERE numero = @FacturaNumero AND tipodocumento = @TipoDocumento AND letra = @FacturaLetra AND ptovta = @FacturaPuntoDeVenta";
                    using (var cmdLimpieza = new NpgsqlCommand(queryEliminarFilasArchivoParcial, connection, transaction))
                    {
                        cmdLimpieza.Parameters.AddWithValue("@FacturaNumero", facturaNumero);
                        cmdLimpieza.Parameters.AddWithValue("@TipoDocumento", facturaTipo);
                        cmdLimpieza.Parameters.AddWithValue("@FacturaLetra", facturaLetra);
                        cmdLimpieza.Parameters.AddWithValue("@FacturaPuntoDeVenta", facturaPuntoDeVenta);
                        cmdLimpieza.ExecuteNonQuery();
                    }
                }

                if (!encontrado)
                {
                    string queryCreacionRelacion = @"INSERT INTO relaciones
                    (tipo_doc_origen, ptovta_origen, letra_origen, numero_origen, tipo_doc_destino, ptovta_destino, letra_destino, numero_destino)
                    VALUES
                    (@tipo_doc_origen, @ptovta_origen, @letra_origen, @numero_origen, @tipo_doc_destino, @ptovta_destino, @letra_destino, @numero_destino);";
                    using (var cmdRelacion = new NpgsqlCommand(queryCreacionRelacion, connection, transaction))
                    {
                        cmdRelacion.Parameters.AddWithValue("@numero_origen", facturaNumero);
                        cmdRelacion.Parameters.AddWithValue("@tipo_doc_origen", facturaTipo);
                        cmdRelacion.Parameters.AddWithValue("@letra_origen", facturaLetra);
                        cmdRelacion.Parameters.AddWithValue("@ptovta_origen", facturaPuntoDeVenta);
                        cmdRelacion.Parameters.AddWithValue("@letra_destino", letraDestino);
                        cmdRelacion.Parameters.AddWithValue("@ptovta_destino", ptovtaDestino);
                        cmdRelacion.Parameters.AddWithValue("@numero_destino", numeroDestino);
                        cmdRelacion.Parameters.AddWithValue("@tipo_doc_destino", tipoDeArchivo);
                        cmdRelacion.ExecuteNonQuery();
                    }
                }

                using (var cmdDelete = new NpgsqlCommand("DELETE FROM auxnc", connection, transaction))
                {
                    cmdDelete.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void ProcesarGuardadoNotaDeDebito(string tipoDeArchivo, string letraDestino, int ptovtaDestino, int numeroDestino, DateTime fecha, int facturaNumero, string facturaLetra, int facturaPuntoDeVenta, string facturaTipo)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                string querySelect = "SELECT id_prestacion, motivorefactura, importerefactura, codigo, usuario, id_notadecredito, comentarios, tiporegistro FROM auxnd";
                var filasAuxnd = new List<object[]>();
                using (var cmdSelect = new NpgsqlCommand(querySelect, connection, transaction))
                using (var lector = cmdSelect.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        filasAuxnd.Add(new object[] {
                        lector["id_prestacion"], lector["motivorefactura"], lector["importerefactura"],
                        lector["codigo"], lector["usuario"], lector["id_notadecredito"],
                        lector["comentarios"], lector["tiporegistro"]
                    });
                    }
                }

                string querySelectIncompletos = @"SELECT id_prestacion FROM cargaincompleta WHERE numero = @FacturaNumero AND tipodocumento = @TipoDocumento AND letra = @FacturaLetra AND ptovta = @FacturaPuntoDeVenta";
                var filasIncompletas = new HashSet<int>();
                using (var cmdIncompletos = new NpgsqlCommand(querySelectIncompletos, connection, transaction))
                {
                    cmdIncompletos.Parameters.AddWithValue("@FacturaNumero", facturaNumero);
                    cmdIncompletos.Parameters.AddWithValue("@TipoDocumento", facturaTipo);
                    cmdIncompletos.Parameters.AddWithValue("@FacturaLetra", facturaLetra);
                    cmdIncompletos.Parameters.AddWithValue("@FacturaPuntoDeVenta", facturaPuntoDeVenta);
                    using (var lector = cmdIncompletos.ExecuteReader())
                    {
                        while (lector.Read())
                            filasIncompletas.Add(Convert.ToInt32(lector["id_prestacion"]));
                    }
                }

                bool encontrado = false;

                string queryActualizarRegistros = @"UPDATE notadedebito 
                SET tipo = @tipo, letra = @letra, ptovta = @ptovta, numero = @numero, fecha = @fecha, cargadocompletamente = @cargadocompletamente
                WHERE id_prestacion = @id_prestacion AND cargadocompletamente = @cargarcompletamente;";

                string queryDeleteIncompletos = @"DELETE FROM cargaincompleta WHERE id_prestacion = @id_prestacion;";

                string queryUpsertNuevoRegistro = @"INSERT INTO notadedebito 
                (id_notadecredito, motivorefactura, importerefactura, codigo, usuario, id_prestacion, tipo, letra, ptovta, numero, fecha, comentarios, tiporegistro, cargadocompletamente) 
                VALUES 
                (@id_notadecredito, @motivorefactura, @importerefactura, @codigo, @usuario, @id_prestacion, @tipo, @letra, @ptovta, @numero, @fecha, @comentarios, @tiporegistro, true)
                ON CONFLICT (id_prestacion)
                DO UPDATE SET
                tipo = EXCLUDED.tipo, letra = EXCLUDED.letra, ptovta = EXCLUDED.ptovta, 
                numero = EXCLUDED.numero, fecha = EXCLUDED.fecha, cargadocompletamente = true,
                motivorefactura = EXCLUDED.motivorefactura, importerefactura = EXCLUDED.importerefactura, 
                codigo = EXCLUDED.codigo, usuario = EXCLUDED.usuario, 
                comentarios = EXCLUDED.comentarios, tiporegistro = EXCLUDED.tiporegistro;";

                foreach (var fila in filasAuxnd)
                {
                    int idPrestacion = Convert.ToInt32(fila[0]);

                    if (filasIncompletas.Contains(idPrestacion))
                    {
                        encontrado = true;
                        using (var cmdUpdate = new NpgsqlCommand(queryActualizarRegistros, connection, transaction))
                        {
                            cmdUpdate.Parameters.AddWithValue("@id_prestacion", idPrestacion);
                            cmdUpdate.Parameters.AddWithValue("@tipo", tipoDeArchivo);
                            cmdUpdate.Parameters.AddWithValue("@letra", letraDestino);
                            cmdUpdate.Parameters.AddWithValue("@ptovta", ptovtaDestino);
                            cmdUpdate.Parameters.AddWithValue("@numero", numeroDestino);
                            cmdUpdate.Parameters.AddWithValue("@fecha", fecha);
                            cmdUpdate.Parameters.AddWithValue("@cargadocompletamente", true);
                            cmdUpdate.Parameters.AddWithValue("@cargarcompletamente", false);
                            cmdUpdate.ExecuteNonQuery();
                        }

                        using (var cmdDelInc = new NpgsqlCommand(queryDeleteIncompletos, connection, transaction))
                        {
                            cmdDelInc.Parameters.AddWithValue("@id_prestacion", idPrestacion);
                            cmdDelInc.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (var cmdUpsert = new NpgsqlCommand(queryUpsertNuevoRegistro, connection, transaction))
                        {
                            cmdUpsert.Parameters.AddWithValue("@id_prestacion", fila[0]);
                            cmdUpsert.Parameters.AddWithValue("@motivorefactura", fila[1] ?? DBNull.Value);
                            cmdUpsert.Parameters.AddWithValue("@importerefactura", fila[2] ?? DBNull.Value);
                            cmdUpsert.Parameters.AddWithValue("@codigo", fila[3] ?? DBNull.Value);
                            cmdUpsert.Parameters.AddWithValue("@usuario", fila[4] ?? DBNull.Value);
                            cmdUpsert.Parameters.AddWithValue("@id_notadecredito", fila[5] ?? DBNull.Value);
                            cmdUpsert.Parameters.AddWithValue("@comentarios", fila[6] ?? DBNull.Value);
                            cmdUpsert.Parameters.AddWithValue("@tiporegistro", fila[7] ?? DBNull.Value);
                            cmdUpsert.Parameters.AddWithValue("@letra", letraDestino);
                            cmdUpsert.Parameters.AddWithValue("@ptovta", ptovtaDestino);
                            cmdUpsert.Parameters.AddWithValue("@numero", numeroDestino);
                            cmdUpsert.Parameters.AddWithValue("@fecha", fecha);
                            cmdUpsert.Parameters.AddWithValue("@tipo", tipoDeArchivo);
                            cmdUpsert.ExecuteNonQuery();
                        }
                    }
                }

                if (filasIncompletas.Count > 0)
                {
                    string queryEliminarFilasArchivoParcial = @"DELETE FROM cargaincompleta WHERE numero = @FacturaNumero AND tipodocumento = @TipoDocumento AND letra = @FacturaLetra AND ptovta = @FacturaPuntoDeVenta";
                    using (var cmdLimpieza = new NpgsqlCommand(queryEliminarFilasArchivoParcial, connection, transaction))
                    {
                        cmdLimpieza.Parameters.AddWithValue("@FacturaNumero", facturaNumero);
                        cmdLimpieza.Parameters.AddWithValue("@TipoDocumento", facturaTipo);
                        cmdLimpieza.Parameters.AddWithValue("@FacturaLetra", facturaLetra);
                        cmdLimpieza.Parameters.AddWithValue("@FacturaPuntoDeVenta", facturaPuntoDeVenta);
                        cmdLimpieza.ExecuteNonQuery();
                    }
                }

                if (!encontrado)
                {
                    string queryCreacionRelacion = @"INSERT INTO relaciones
                    (tipo_doc_origen, ptovta_origen, letra_origen, numero_origen, tipo_doc_destino, ptovta_destino, letra_destino, numero_destino)
                    VALUES
                    (@tipo_doc_origen, @ptovta_origen, @letra_origen, @numero_origen, @tipo_doc_destino, @ptovta_destino, @letra_destino, @numero_destino);";
                    using (var cmdRelacion = new NpgsqlCommand(queryCreacionRelacion, connection, transaction))
                    {
                        cmdRelacion.Parameters.AddWithValue("@numero_origen", facturaNumero);
                        cmdRelacion.Parameters.AddWithValue("@tipo_doc_origen", facturaTipo);
                        cmdRelacion.Parameters.AddWithValue("@letra_origen", facturaLetra);
                        cmdRelacion.Parameters.AddWithValue("@ptovta_origen", facturaPuntoDeVenta);
                        cmdRelacion.Parameters.AddWithValue("@letra_destino", letraDestino);
                        cmdRelacion.Parameters.AddWithValue("@ptovta_destino", ptovtaDestino);
                        cmdRelacion.Parameters.AddWithValue("@numero_destino", numeroDestino);
                        cmdRelacion.Parameters.AddWithValue("@tipo_doc_destino", tipoDeArchivo);
                        cmdRelacion.ExecuteNonQuery();
                    }
                }

                using (var cmdDelete = new NpgsqlCommand("DELETE FROM auxnd", connection, transaction))
                {
                    cmdDelete.ExecuteNonQuery();
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