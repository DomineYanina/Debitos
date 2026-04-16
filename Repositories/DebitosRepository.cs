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

        public DataTable ObtenerPrestacionesDocumento(string facturaTipo, string tipoRegistro, string letra, int ptoVta, int numero)
        {
            string query = "";

            if (tipoRegistro == "Ambulatorios")
            {
                if (facturaTipo == "NC")
                    query = @"SELECT 
            al.paciente, al.plan AS Plan, al.efector, al.medico, al.fecha, al.codigo, al.descripcion, al.cantidad, al.total_neto, al.coseguro, al.total, nc.comentarios AS NC_Comentarios,
            al.modulo AS modulo, al.nro_internacion AS Nro_Int, al.fecha_ingreso AS F_Ingreso, al.fecha_egreso AS F_Egreso, 
            al.carnet, al.codigo_cobertura AS Cobertura, 
            al.cob_factura_tipo, al.cob_factura_letra, al.cob_factura_ptoventa, al.cob_factura_numero, al.id AS ID_Prestacion,
            nc.id AS id, nc.debitoaceptado AS NC_DebitoAceptado, nc.motivodedebito AS NC_MotivoDeDebito, nc.importedebitado AS NC_ImporteDebitado, nc.prestacionenglobante AS NC_PrestacionEnglobante, nc.motivoderefactura AS NC_MotivoDeRefactura, nc.importederefactura AS NC_ImporteDeRefactura, 
            nd.motivorefactura AS ND_MotivoDeRefactura, nd.importerefactura AS ND_ImporteDeRefactura, nd.comentarios AS ND_Comentarios
        FROM notadecredito nc
        LEFT JOIN notadedebito nd ON nc.id = nd.id_notadecredito
        JOIN amb_liquidado al ON nc.id_prestacion = al.id
        WHERE nc.letra = @FacturaLetra AND nc.ptovta = @FacturaPuntoVenta AND nc.numero = @FacturaNumero;";

                else if (facturaTipo == "ND")
                    query = @"SELECT 
            al.paciente, al.plan AS Plan, al.efector, al.medico, al.fecha, al.codigo, al.descripcion, al.cantidad, al.total_neto, al.coseguro, al.total, nd.comentarios,
            al.modulo AS modulo, al.nro_internacion AS Nro_Int, al.fecha_ingreso AS F_Ingreso, al.fecha_egreso AS F_Egreso, 
            al.carnet, al.codigo_cobertura AS Cobertura, 
            nc1.letra AS NC_Previo_Letra, nc1.ptovta AS NC_Previo_PuntoDeVenta, nc1.numero AS NC_Previo_Numero, 
            nc1.fecha AS NC_Previo_Fecha, nc1.motivodedebito AS NC_Previo_MotivoDeDebito, nc1.importedebitado AS NC_Previo_ImporteDebitado, 
            nc1.motivoderefactura AS NC_Previo_MotivoDeRefactura, nc1.id_prestacion AS ID_Prestacion, 
            nd.id, nd.motivorefactura, nd.importerefactura, 
            nc.debitoaceptado AS NC_DebitoAceptado, nc.motivodedebito AS NC_MotivoDeDebito, nc.diasfacturados AS NC_DiasFacturados, nc.prestacionenglobante AS NC_PrestacionEnglobante,
            nc.importedebitado AS NC_ImporteDebitado, nc.motivoderefactura AS NC_MotivoDeRefactura, nc.importederefactura AS NC_ImporteDeRefactura, 
            nc.comentarios AS NC_Comentarios 
        FROM notadedebito nd 
        RIGHT JOIN notadecredito nc1 ON nd.id_notadecredito = nc1.id 
        LEFT JOIN notadecredito nc ON nd.id = nc.id_notadedebito 
        LEFT JOIN amb_liquidado al ON al.id = nc1.id_prestacion 
        WHERE nd.letra = @FacturaLetra AND nd.ptovta = @FacturaPuntoVenta AND nd.numero = @FacturaNumero;";

                else if (facturaTipo == "FC")
                    query = @"SELECT 
            al.paciente, al.plan AS Plan, al.efector, al.medico, al.fecha as Fecha, al.codigo, al.descripcion, al.cantidad AS Cantidad, al.total_neto, al.coseguro, al.total, 
            al.porcentaje_especialista, al.porcentaje_ayudante1, al.porcentaje_anestesista, al.porcentaje_gastos, al.id AS ID_Prestacion,
            nc.fecha AS NC_Fecha, nc.letra AS NC_Letra, nc.ptovta AS NC_PuntoDeVenta, nc.numero AS NC_Numero, nc.debitoaceptado AS NC_DebitoAceptado, nc.motivodedebito AS NC_MotivoDeDebito, nc.diasfacturados AS NC_DiasFacturados, nc.importedebitado AS NC_ImporteDebitado, 
            nc.prestacionenglobante AS NC_PrestacionEnglobante, nc.motivoderefactura AS NC_MotivoDeRefactura, nc.importederefactura AS NC_ImporteDeRefactura, nc.cargadocompletamente, nc.comentarios AS NC_Comentarios
        FROM amb_liquidado al
        LEFT JOIN notadecredito nc ON al.id = nc.id_prestacion AND nc.id_notadedebito IS NULL
        WHERE al.cob_factura_letra = @FacturaLetra AND al.cob_factura_ptoventa = @FacturaPuntoVenta AND al.cob_factura_numero = @FacturaNumero;";
            }
            else if (tipoRegistro == "Internados")
            {
                if (facturaTipo == "NC")
                    query = @"SELECT al.modulo AS modulo, al.grupomodulo AS grupomodulo, al.nro_internacion AS Nro_Int, al.fecha_ingreso AS F_Ingreso, al.fecha_egreso AS F_Egreso, 
                al.carnet, al.paciente,  al.codigo_cobertura AS Cobertura, al.plan AS Plan, al.medico, al.fecha, al.codigo, al.descripcion,
                al.cantidad, al.total_neto, al.coseguro, al.total,
                al.cob_factura_tipo, al.cob_factura_letra, al.cob_factura_ptoventa, al.cob_factura_numero, al.id AS ID_Prestacion,
                nc.id AS id, nc.debitoaceptado AS NC_DebitoAceptado, nc.motivodedebito AS NC_MotivoDeDebito, nc.importedebitado AS NC_ImporteDebitado, nc.prestacionenglobante AS NC_PrestacionEnglobante, nc.motivoderefactura AS NC_MotivoDeRefactura, nc.importederefactura AS NC_ImporteDeRefactura, nc.comentarios as NC_Comentarios,
                nd.motivorefactura AS ND_MotivoDeRefactura, nd.importerefactura AS ND_ImporteDeRefactura, nd.comentarios AS ND_Comentarios
            FROM notadecredito nc
            LEFT JOIN notadedebito nd ON nc.id = nd.id_notadecredito
            JOIN amb_liquidado al ON nc.id_prestacion = al.id
            WHERE nc.letra = @FacturaLetra AND nc.ptovta = @FacturaPuntoVenta AND nc.numero = @FacturaNumero;";
                else if (facturaTipo == "ND")
                    query = @"SELECT al.modulo AS modulo, al.grupomodulo AS grupomodulo, al.nro_internacion AS Nro_Int, al.fecha_ingreso AS F_Ingreso, al.fecha_egreso AS F_Egreso,  
                al.carnet, al.paciente, al.plan AS Plan, al.medico, al.fecha, al.codigo, al.descripcion, al.cantidad,  al.total_neto, al.coseguro, al.total,
                al.codigo_cobertura AS Cobertura,
                nc1.letra AS NC_Previo_Letra, nc1.ptovta AS NC_Previo_PuntoDeVenta, nc1.numero AS NC_Previo_Numero, 
                nc1.fecha AS NC_Previo_Fecha, nc1.motivodedebito AS NC_Previo_MotivoDeDebito, nc1.importedebitado AS NC_Previo_ImporteDebitado, 
                nc1.motivoderefactura AS NC_Previo_MotivoDeRefactura, nc1.id_prestacion AS ID_Prestacion, nd.id, nd.motivorefactura, nd.importerefactura, 
                nd.comentarios, nc.debitoaceptado AS NC_DebitoAceptado, nc.motivodedebito AS NC_MotivoDeDebito, nc.diasfacturados AS NC_DiasFacturados, nc.prestacionenglobante AS NC_PrestacionEnglobante,
                nc.importedebitado AS NC_ImporteDebitado, nc.motivoderefactura AS NC_MotivoDeRefactura, nc.importederefactura AS NC_ImporteDeRefactura, 
                nc.comentarios AS NC_Comentarios 
            FROM notadedebito nd 
            RIGHT JOIN notadecredito nc1 ON nd.id_notadecredito = nc1.id 
            LEFT JOIN notadecredito nc ON nd.id = nc.id_notadedebito 
            LEFT JOIN amb_liquidado al ON al.id = nc1.id_prestacion 
            WHERE nd.letra = @FacturaLetra AND nd.ptovta = @FacturaPuntoVenta AND nd.numero = @FacturaNumero;";
                else if (facturaTipo == "FC")
                    query = @"SELECT al.modulo AS modulo, al.grupomodulo AS grupomodulo, al.nro_internacion AS Nro_Int, al.fecha_ingreso AS F_Ingreso, al.fecha_egreso AS F_Egreso, 
                al.carnet, al.paciente, al.codigo_cobertura AS Cobertura, al.plan AS Plan, al.medico, al.fecha, al.codigo, al.descripcion, 
                al.cantidad, al.total_neto, al.coseguro, al.total, 
                al.porcentaje_especialista, al.porcentaje_ayudante1, al.porcentaje_anestesista, al.porcentaje_gastos, al.id AS ID_Prestacion,
                nc.fecha AS NC_Fecha, nc.letra AS NC_Letra, nc.ptovta AS NC_PuntoDeVenta, nc.numero AS NC_Numero, nc.debitoaceptado AS NC_DebitoAceptado, nc.motivodedebito AS NC_MotivoDeDebito, nc.diasfacturados AS NC_DiasFacturados, nc.importedebitado AS NC_ImporteDebitado, 
                nc.prestacionenglobante AS NC_PrestacionEnglobante, nc.motivoderefactura AS NC_MotivoDeRefactura, nc.importederefactura AS NC_ImporteDeRefactura, nc.cargadocompletamente, nc.comentarios AS NC_Comentarios
            FROM amb_liquidado al
            -- CORRECCIÓN ACÁ: Filtramos para que solo una la NC original
            LEFT JOIN notadecredito nc ON al.id = nc.id_prestacion AND nc.id_notadedebito IS NULL
            WHERE al.cob_factura_letra = @FacturaLetra AND al.cob_factura_ptoventa = @FacturaPuntoVenta AND al.cob_factura_numero = @FacturaNumero;";
            }

            NpgsqlParameter[] parameters = {
        new NpgsqlParameter("@FacturaLetra", letra),
        new NpgsqlParameter("@FacturaPuntoVenta", ptoVta),
        new NpgsqlParameter("@FacturaNumero", numero)
    };

            return ExecuteQuery(query, parameters);
        }

        public void LimpiarAuxiliarNC(string usuario)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var command = new NpgsqlCommand("DELETE FROM auxnc WHERE usuario = @usuario", connection);
            command.Parameters.AddWithValue("@usuario", usuario);
            command.ExecuteNonQuery();
        }

        public void LimpiarAuxiliarND(string usuario)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var command = new NpgsqlCommand("DELETE FROM auxnd WHERE usuario = @usuario", connection);
            command.Parameters.AddWithValue("@usuario", usuario);
            command.ExecuteNonQuery();
        }

        public void InsertarAuxiliarNC_FC(List<(int idPrestacion, object? motivoRefactura, object? motivoDebito, double? importeRefactura, double? importeDebito, string? comentarios, bool debitoAceptado, object? diasFacturados, string? prestacionEnglobante, int? idNotaDeDebito, string? codigo)> lista, string usuario, string tipoRegistro)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                string query = @"INSERT INTO auxnc (id_prestacion, motivodedebito, diasfacturados, importedebitado, debitoaceptado, motivoderefactura, importederefactura, prestacionenglobante, usuario, tiporegistro, comentarios) 
                         VALUES (@id_prestacion, @motivodedebito, @diasfacturados, @importedebitado, @debitoaceptado, @motivoderefactura, @importederefactura, @prestacionenglobante, @usuario, @tiporegistro, @comentarios)";

                foreach (var item in lista)
                {
                    using var cmd = new NpgsqlCommand(query, connection, transaction);
                    cmd.Parameters.AddWithValue("@id_prestacion", item.idPrestacion);
                    cmd.Parameters.AddWithValue("@motivodedebito", item.motivoDebito ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@diasfacturados", item.diasFacturados ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@importedebitado", (object?)item.importeDebito ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@debitoaceptado", item.debitoAceptado);
                    cmd.Parameters.AddWithValue("@motivoderefactura", item.motivoRefactura ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@importederefactura", (object?)item.importeRefactura ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@prestacionenglobante", (object?)item.prestacionEnglobante ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@usuario", usuario);
                    cmd.Parameters.AddWithValue("@tiporegistro", tipoRegistro);
                    cmd.Parameters.AddWithValue("@comentarios", (object?)item.comentarios ?? "");
                    cmd.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        
        public void InsertarAuxiliarNC_ND(List<(int idPrestacion, object? motivoRefactura, object? motivoDebito, double? importeRefactura, double? importeDebito, string? comentarios, bool debitoAceptado, object? diasFacturados, string? prestacionEnglobante, int? idNotaDeDebito, string? codigo)> lista, string usuario, string tipoRegistro)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                string query = @"INSERT INTO auxnc (id_prestacion, motivodedebito, diasfacturados, importedebitado, debitoaceptado, motivoderefactura, importederefactura, prestacionenglobante, usuario, tiporegistro, comentarios, id_notadedebito) 
                         VALUES (@id_prestacion, @motivodedebito, @diasfacturados, @importedebitado, @debitoaceptado, @motivoderefactura, @importederefactura, @prestacionenglobante, @usuario, @tiporegistro, @comentarios, @id_notadedebito)";

                foreach (var item in lista)
                {
                    using var cmd = new NpgsqlCommand(query, connection, transaction);
                    cmd.Parameters.AddWithValue("@id_prestacion", item.idPrestacion);
                    cmd.Parameters.AddWithValue("@motivodedebito", item.motivoDebito ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@diasfacturados", item.diasFacturados ?? DBNull.Value);

                    // CORREGIDO: Manejo de nulos para los importes
                    cmd.Parameters.AddWithValue("@importedebitado", (object?)item.importeDebito ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@debitoaceptado", item.debitoAceptado);
                    cmd.Parameters.AddWithValue("@motivoderefactura", item.motivoRefactura ?? DBNull.Value);

                    // CORREGIDO: Manejo de nulos para los importes
                    cmd.Parameters.AddWithValue("@importederefactura", (object?)item.importeRefactura ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@prestacionenglobante", (object?)item.prestacionEnglobante ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@usuario", usuario);
                    cmd.Parameters.AddWithValue("@tiporegistro", tipoRegistro);
                    cmd.Parameters.AddWithValue("@comentarios", (object?)item.comentarios ?? "");

                    // Inyectamos el ID clave para mantener la cadena
                    cmd.Parameters.AddWithValue("@id_notadedebito", item.idNotaDeDebito.HasValue ? (object)item.idNotaDeDebito.Value : DBNull.Value);

                    cmd.ExecuteNonQuery();
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

        public void GuardarCargaParcialFC(List<CargaParcialDTO> listaCarga, string tipoDocumento, string letra, int ptovta, int numero)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                foreach (var item in listaCarga)
                {
                    RegistrarCargaIncompleta(connection, transaction, item.IdPrestacion, tipoDocumento, letra, ptovta, numero);

                    // 1. Intentamos actualizar asumiendo que la NC ya existe y viene directo de la Factura (sin padre)
                    string comandoUpdate = "UPDATE notadecredito SET debitoaceptado = @debitoaceptado, motivodedebito = @motivodedebito, importedebitado = @importedebitado, diasfacturados = @diasfacturados, motivoderefactura = @motivoderefactura, importederefactura = @importederefactura, prestacionenglobante = @prestacionenglobante, comentarios = @comentarios, cargadocompletamente = @cargadocompletamente, usuario = @usuario WHERE id_prestacion = @id_prestacion AND id_notadedebito IS NULL;";

                    using var commandUpdate = new NpgsqlCommand(comandoUpdate, connection, transaction);
                    commandUpdate.Parameters.AddWithValue("@id_prestacion", item.IdPrestacion);
                    commandUpdate.Parameters.AddWithValue("@debitoaceptado", item.DebitoAceptado);
                    commandUpdate.Parameters.AddWithValue("@motivodedebito", (object?)item.MotivoDebito ?? DBNull.Value);
                    commandUpdate.Parameters.AddWithValue("@importedebitado", (object?)item.ImporteDebitado ?? DBNull.Value);
                    commandUpdate.Parameters.AddWithValue("@diasfacturados", (object?)item.DiasFacturados ?? DBNull.Value);
                    commandUpdate.Parameters.AddWithValue("@motivoderefactura", (object?)item.MotivoRefactura ?? DBNull.Value);
                    commandUpdate.Parameters.AddWithValue("@importederefactura", (object?)item.ImporteRefactura ?? DBNull.Value);
                    commandUpdate.Parameters.AddWithValue("@prestacionenglobante", (object?)item.PrestacionEnglobante ?? DBNull.Value);
                    commandUpdate.Parameters.AddWithValue("@comentarios", (object?)item.Comentarios ?? "");
                    commandUpdate.Parameters.AddWithValue("@cargadocompletamente", item.CargadoCompletamente);
                    commandUpdate.Parameters.AddWithValue("@usuario", (object?)item.Usuario ?? DBNull.Value);

                    int filasAfectadas = commandUpdate.ExecuteNonQuery();

                    // 2. Si no actualizó nada, es un registro nuevo, así que Insertamos
                    if (filasAfectadas == 0)
                    {
                        string comandoInsert = "INSERT INTO notadecredito (id_prestacion, debitoaceptado, motivodedebito, importedebitado, diasfacturados, motivoderefactura, importederefactura, prestacionenglobante, comentarios, cargadocompletamente, usuario) VALUES (@id_prestacion, @debitoaceptado, @motivodedebito, @importedebitado, @diasfacturados, @motivoderefactura, @importederefactura, @prestacionenglobante, @comentarios, @cargadocompletamente, @usuario);";

                        using var commandInsert = new NpgsqlCommand(comandoInsert, connection, transaction);
                        commandInsert.Parameters.AddWithValue("@id_prestacion", item.IdPrestacion);
                        commandInsert.Parameters.AddWithValue("@debitoaceptado", item.DebitoAceptado);
                        commandInsert.Parameters.AddWithValue("@motivodedebito", (object?)item.MotivoDebito ?? DBNull.Value);
                        commandInsert.Parameters.AddWithValue("@importedebitado", (object?)item.ImporteDebitado ?? DBNull.Value);
                        commandInsert.Parameters.AddWithValue("@diasfacturados", (object?)item.DiasFacturados ?? DBNull.Value);
                        commandInsert.Parameters.AddWithValue("@motivoderefactura", (object?)item.MotivoRefactura ?? DBNull.Value);
                        commandInsert.Parameters.AddWithValue("@importederefactura", (object?)item.ImporteRefactura ?? DBNull.Value);
                        commandInsert.Parameters.AddWithValue("@prestacionenglobante", (object?)item.PrestacionEnglobante ?? DBNull.Value);
                        commandInsert.Parameters.AddWithValue("@comentarios", (object?)item.Comentarios ?? "");
                        commandInsert.Parameters.AddWithValue("@cargadocompletamente", item.CargadoCompletamente);
                        commandInsert.Parameters.AddWithValue("@usuario", (object?)item.Usuario ?? DBNull.Value);

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
        public void GuardarCargaParcialNC(List<CargaParcialDTO> listaCarga, string tipoDocumento, string letra, int ptovta, int numero)
        {
            string comandoUpdate = @"UPDATE notadedebito SET motivorefactura = @motivoderefactura, importerefactura = @importederefactura, usuario = @usuario, codigo = @codigo, cargadocompletamente = @cargadocompletamente, comentarios = @comentarios WHERE id_prestacion = @id_prestacion;";
            string comandoInsert = @"INSERT INTO notadedebito (id_notadecredito, motivorefactura, importerefactura, codigo, usuario, id_prestacion, cargadocompletamente, comentarios, tiporegistro) VALUES (@id_notadecredito, @motivoderefactura, @importederefactura, @codigo, @usuario, @id_prestacion, @cargadocompletamente, @comentarios, @tiporegistro);";

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                foreach (var item in listaCarga)
                {
                    // 1. Usamos nuestro nuevo método centralizado
                    RegistrarCargaIncompleta(connection, transaction, item.IdPrestacion, tipoDocumento, letra, ptovta, numero);

                    // 2. Intentamos el UPDATE
                    using var commandUpdate = new NpgsqlCommand(comandoUpdate, connection, transaction);
                    commandUpdate.Parameters.AddWithValue("@id_prestacion", item.IdPrestacion);
                    commandUpdate.Parameters.AddWithValue("@motivoderefactura", item.MotivoRefactura ?? DBNull.Value);
                    commandUpdate.Parameters.AddWithValue("@importederefactura", item.ImporteRefactura ?? DBNull.Value);
                    commandUpdate.Parameters.AddWithValue("@usuario", item.Usuario);
                    commandUpdate.Parameters.AddWithValue("@codigo", (object?)item.Codigo ?? DBNull.Value);
                    commandUpdate.Parameters.AddWithValue("@cargadocompletamente", item.CargadoCompletamente);
                    commandUpdate.Parameters.AddWithValue("@comentarios", (object?)item.Comentarios ?? "");

                    int filasAfectadas = commandUpdate.ExecuteNonQuery();

                    // 3. Si no existía, hacemos el INSERT
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
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                foreach (var item in listaCarga)
                {
                    RegistrarCargaIncompleta(connection, transaction, item.IdPrestacion, tipoDocumento, letra, ptovta, numero);

                    int idPadreND = (int)item.IdNotaDeDebito;

                    // Consulta saneada en una sola línea
                    string comandoUpdate = "UPDATE notadecredito SET motivodedebito = @motivodedebito, diasfacturados = @diasfacturados, importedebitado = @importedebitado, debitoaceptado = @debitoaceptado, motivoderefactura = @motivoderefactura, importederefactura = @importederefactura, usuario = @usuario, cargadocompletamente = @cargadocompletamente, comentarios = @comentarios WHERE id_notadedebito = @idPadreND AND id_prestacion = @id_prestacion;";

                    using var commandUpdate = new NpgsqlCommand(comandoUpdate, connection, transaction);
                    commandUpdate.Parameters.AddWithValue("@id_prestacion", item.IdPrestacion);
                    commandUpdate.Parameters.AddWithValue("@idPadreND", idPadreND);
                    commandUpdate.Parameters.AddWithValue("@motivodedebito", (object?)item.MotivoDebito ?? DBNull.Value);
                    commandUpdate.Parameters.AddWithValue("@diasfacturados", (object?)item.DiasFacturados ?? DBNull.Value);
                    commandUpdate.Parameters.AddWithValue("@importedebitado", (object?)item.ImporteDebitado ?? DBNull.Value);
                    commandUpdate.Parameters.AddWithValue("@debitoaceptado", item.DebitoAceptado);
                    commandUpdate.Parameters.AddWithValue("@motivoderefactura", (object?)item.MotivoRefactura ?? DBNull.Value);
                    commandUpdate.Parameters.AddWithValue("@importederefactura", (object?)item.ImporteRefactura ?? DBNull.Value);
                    commandUpdate.Parameters.AddWithValue("@usuario", (object?)item.Usuario ?? DBNull.Value);
                    commandUpdate.Parameters.AddWithValue("@cargadocompletamente", item.CargadoCompletamente);
                    commandUpdate.Parameters.AddWithValue("@comentarios", (object?)item.Comentarios ?? "");

                    int filasAfectadas = commandUpdate.ExecuteNonQuery();

                    if (filasAfectadas == 0)
                    {
                        // Consulta saneada en una sola línea
                        string comandoInsert = "INSERT INTO notadecredito (id_prestacion, motivodedebito, diasfacturados, importedebitado, debitoaceptado, motivoderefactura, importederefactura, usuario, cargadocompletamente, id_notadedebito, comentarios) VALUES (@id_prestacion, @motivodedebito, @diasfacturados, @importedebitado, @debitoaceptado, @motivoderefactura, @importederefactura, @usuario, @cargadocompletamente, @idPadreND, @comentarios);";

                        using var commandInsert = new NpgsqlCommand(comandoInsert, connection, transaction);
                        commandInsert.Parameters.AddWithValue("@id_prestacion", item.IdPrestacion);
                        commandInsert.Parameters.AddWithValue("@idPadreND", idPadreND);
                        commandInsert.Parameters.AddWithValue("@motivodedebito", (object?)item.MotivoDebito ?? DBNull.Value);
                        commandInsert.Parameters.AddWithValue("@diasfacturados", (object?)item.DiasFacturados ?? DBNull.Value);
                        commandInsert.Parameters.AddWithValue("@importedebitado", (object?)item.ImporteDebitado ?? DBNull.Value);
                        commandInsert.Parameters.AddWithValue("@debitoaceptado", item.DebitoAceptado);
                        commandInsert.Parameters.AddWithValue("@motivoderefactura", (object?)item.MotivoRefactura ?? DBNull.Value);
                        commandInsert.Parameters.AddWithValue("@importederefactura", (object?)item.ImporteRefactura ?? DBNull.Value);
                        commandInsert.Parameters.AddWithValue("@usuario", (object?)item.Usuario ?? DBNull.Value);
                        commandInsert.Parameters.AddWithValue("@cargadocompletamente", item.CargadoCompletamente);
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

        public void ProcesarGuardadoNotaDeCredito(string tipoDeArchivo, string letraDestino, int ptovtaDestino, int numeroDestino, DateTime fecha, int facturaNumero, string facturaLetra, int facturaPuntoDeVenta, string facturaTipo, string usuarioAuditor)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. LEEMOS EL id_notadedebito DIRECTAMENTE DESDE EL AUXILIAR
                string querySelect = "SELECT id_prestacion, motivodedebito, diasfacturados, importedebitado, debitoaceptado, motivoderefactura, importederefactura, prestacionenglobante, usuario, comentarios, tiporegistro, id_notadedebito FROM auxnc WHERE usuario = @usuarioAuditor";
                var filasAuxnc = new List<object[]>();
                using (var cmdSelect = new NpgsqlCommand(querySelect, connection, transaction))
                {
                    cmdSelect.Parameters.AddWithValue("@usuarioAuditor", usuarioAuditor);
                    using (var lector = cmdSelect.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            filasAuxnc.Add(new object[] {
                        lector["id_prestacion"], lector["motivodedebito"], lector["diasfacturados"],
                        lector["importedebitado"], lector["debitoaceptado"], lector["motivoderefactura"],
                        lector["importederefactura"], lector["prestacionenglobante"], lector["usuario"],
                        lector["comentarios"], lector["tiporegistro"],
                        lector["id_notadedebito"] // Índice 11: TU VALOR CLAVE
                    });
                        }
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
                        while (lector.Read()) filasIncompletas.Add(Convert.ToInt32(lector["id_prestacion"]));
                    }
                }

                bool relacionCreada = false;

                foreach (var fila in filasAuxnc)
                {
                    int idPrestacion = Convert.ToInt32(fila[0]);

                    // 2. EXTRAEMOS EL ID DEL PADRE DE LA MEMORIA (sin buscar en la base de datos)
                    int? idPadreND = fila[11] != DBNull.Value ? Convert.ToInt32(fila[11]) : (int?)null;
                    bool existeRegistro = false;

                    // 3. ARMAMOS LA CONDICIÓN EXACTA BASADA EN TU LÓGICA
                    string condicionPadre = idPadreND.HasValue ? "id_notadedebito = @id_notadedebito" : "id_notadedebito IS NULL";

                    string queryComprobar = $"SELECT 1 FROM notadecredito WHERE id_prestacion = @id_prestacion AND {condicionPadre};";

                    using (var cmdVerificar = new NpgsqlCommand(queryComprobar, connection, transaction))
                    {
                        cmdVerificar.Parameters.AddWithValue("@id_prestacion", idPrestacion);
                        if (idPadreND.HasValue) cmdVerificar.Parameters.AddWithValue("@id_notadedebito", idPadreND.Value);
                        existeRegistro = cmdVerificar.ExecuteScalar() != null;
                    }

                    Action<NpgsqlCommand> setParametrosComunes = (cmd) =>
                    {
                        cmd.Parameters.AddWithValue("@id_prestacion", idPrestacion);
                        cmd.Parameters.AddWithValue("@id_notadedebito", idPadreND.HasValue ? (object)idPadreND.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@tipo", tipoDeArchivo);
                        cmd.Parameters.AddWithValue("@letra", letraDestino);
                        cmd.Parameters.AddWithValue("@ptovta", ptovtaDestino);
                        cmd.Parameters.AddWithValue("@numero", numeroDestino);
                        cmd.Parameters.AddWithValue("@fecha", fecha);
                        cmd.Parameters.AddWithValue("@motivodedebito", fila[1] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@diasfacturados", fila[2] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@importedebitado", fila[3] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@debitoaceptado", fila[4]);
                        cmd.Parameters.AddWithValue("@motivoderefactura", fila[5] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@importederefactura", fila[6] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@prestacionenglobante", fila[7] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@usuario", fila[8] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@comentarios", fila[9] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@tiporegistro", fila[10] ?? DBNull.Value);
                    };

                    if (existeRegistro)
                    {
                        relacionCreada = true;
                        string queryActualizar = $@"UPDATE notadecredito 
                    SET fecha = @fecha, tipo = @tipo, letra = @letra, ptovta = @ptovta, numero = @numero, cargadocompletamente = true, 
                    motivodedebito = @motivodedebito, diasfacturados = @diasfacturados, importedebitado = @importedebitado, debitoaceptado = @debitoaceptado, 
                    motivoderefactura = @motivoderefactura, importederefactura = @importederefactura, prestacionenglobante = @prestacionenglobante, 
                    usuario = @usuario, comentarios = @comentarios, tiporegistro = @tiporegistro 
                    WHERE id_prestacion = @id_prestacion AND {condicionPadre};";

                        using (var cmdUpdate = new NpgsqlCommand(queryActualizar, connection, transaction))
                        {
                            setParametrosComunes(cmdUpdate);
                            cmdUpdate.ExecuteNonQuery();
                        }

                        if (filasIncompletas.Contains(idPrestacion))
                        {
                            using (var cmdDelInc = new NpgsqlCommand("DELETE FROM cargaincompleta WHERE id_prestacion = @id_prestacion AND tipodocumento = @TipoDocumento AND letra = @FacturaLetra AND ptovta = @FacturaPuntoDeVenta;", connection, transaction))
                            {
                                cmdDelInc.Parameters.AddWithValue("@id_prestacion", idPrestacion);
                                cmdDelInc.Parameters.AddWithValue("@FacturaNumero", facturaNumero);
                                cmdDelInc.Parameters.AddWithValue("@TipoDocumento", facturaTipo);
                                cmdDelInc.Parameters.AddWithValue("@FacturaLetra", facturaLetra);
                                cmdDelInc.Parameters.AddWithValue("@FacturaPuntoDeVenta", facturaPuntoDeVenta);
                                cmdDelInc.ExecuteNonQuery();
                            }
                        }
                    }
                    else
                    {
                        string queryInsertar = @"INSERT INTO notadecredito 
                    (id_prestacion, tipo, letra, ptovta, numero, fecha, cargadocompletamente, motivodedebito, diasfacturados, importedebitado, debitoaceptado, motivoderefactura, importederefactura, prestacionenglobante, usuario, comentarios, tiporegistro, id_notadedebito) 
                    VALUES (@id_prestacion, @tipo, @letra, @ptovta, @numero, @fecha, true, @motivodedebito, @diasfacturados, @importedebitado, @debitoaceptado, @motivoderefactura, @importederefactura, @prestacionenglobante, @usuario, @comentarios, @tiporegistro, @id_notadedebito);";

                        using (var cmdInsert = new NpgsqlCommand(queryInsertar, connection, transaction))
                        {
                            setParametrosComunes(cmdInsert);
                            cmdInsert.ExecuteNonQuery();
                        }
                    }
                }

                if (filasIncompletas.Count > 0)
                {
                    using (var cmdLimpieza = new NpgsqlCommand("DELETE FROM cargaincompleta WHERE numero = @FacturaNumero AND tipodocumento = @TipoDocumento AND letra = @FacturaLetra AND ptovta = @FacturaPuntoDeVenta", connection, transaction))
                    {
                        cmdLimpieza.Parameters.AddWithValue("@FacturaNumero", facturaNumero);
                        cmdLimpieza.Parameters.AddWithValue("@TipoDocumento", facturaTipo);
                        cmdLimpieza.Parameters.AddWithValue("@FacturaLetra", facturaLetra);
                        cmdLimpieza.Parameters.AddWithValue("@FacturaPuntoDeVenta", facturaPuntoDeVenta);
                        cmdLimpieza.ExecuteNonQuery();
                    }
                }

                if (!relacionCreada)
                {
                    InsertarRelacionDocumento(connection, transaction, facturaTipo, facturaLetra, facturaPuntoDeVenta, facturaNumero, tipoDeArchivo, letraDestino, ptovtaDestino, numeroDestino);
                }

                using (var cmdDelete = new NpgsqlCommand("DELETE FROM auxnc WHERE usuario = @usuarioAuditor", connection, transaction))
                {
                    cmdDelete.Parameters.AddWithValue("@usuarioAuditor", usuarioAuditor);
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
        
        private void InsertarRelacionDocumento(NpgsqlConnection connection, NpgsqlTransaction transaction,
    string tipoOrigen, string letraOrigen, int ptovtaOrigen, int numeroOrigen,
    string tipoDestino, string letraDestino, int ptovtaDestino, int numeroDestino)
        {
            string queryCreacionRelacion = @"INSERT INTO relaciones
        (tipo_doc_origen, ptovta_origen, letra_origen, numero_origen, tipo_doc_destino, ptovta_destino, letra_destino, numero_destino)
        VALUES
        (@tipo_doc_origen, @ptovta_origen, @letra_origen, @numero_origen, @tipo_doc_destino, @ptovta_destino, @letra_destino, @numero_destino);";

            using var cmdRelacion = new NpgsqlCommand(queryCreacionRelacion, connection, transaction);
            cmdRelacion.Parameters.AddWithValue("@numero_origen", numeroOrigen);
            cmdRelacion.Parameters.AddWithValue("@tipo_doc_origen", tipoOrigen);
            cmdRelacion.Parameters.AddWithValue("@letra_origen", letraOrigen);
            cmdRelacion.Parameters.AddWithValue("@ptovta_origen", ptovtaOrigen);

            cmdRelacion.Parameters.AddWithValue("@letra_destino", letraDestino);
            cmdRelacion.Parameters.AddWithValue("@ptovta_destino", ptovtaDestino);
            cmdRelacion.Parameters.AddWithValue("@numero_destino", numeroDestino);
            cmdRelacion.Parameters.AddWithValue("@tipo_doc_destino", tipoDestino);

            cmdRelacion.ExecuteNonQuery();
        }

        public void ProcesarGuardadoNotaDeDebito(string tipoDeArchivo, string letraDestino, int ptovtaDestino, int numeroDestino, DateTime fecha, int facturaNumero, string facturaLetra, int facturaPuntoDeVenta, string facturaTipo, string usuarioAuditor)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                string querySelect = "SELECT id_prestacion, motivorefactura, importerefactura, codigo, usuario, id_notadecredito, comentarios, tiporegistro FROM auxnd WHERE usuario = @usuarioAuditor";
                var filasAuxnd = new List<object[]>();
                using (var cmdSelect = new NpgsqlCommand(querySelect, connection, transaction))
                {
                    cmdSelect.Parameters.AddWithValue("@usuarioAuditor", usuarioAuditor);
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
                SET tipo = @tipo, letra = @letra, ptovta = @ptovta, numero = @numero, fecha = @fecha, cargadocompletamente = @cargadocompletamente,
                motivorefactura = @motivorefactura, importerefactura = @importerefactura, codigo = @codigo, usuario = @usuario, 
                comentarios = @comentarios, tiporegistro = @tiporegistro
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

                            cmdUpdate.Parameters.AddWithValue("@motivorefactura", fila[1] ?? DBNull.Value);
                            cmdUpdate.Parameters.AddWithValue("@importerefactura", fila[2] ?? DBNull.Value);
                            cmdUpdate.Parameters.AddWithValue("@codigo", fila[3] ?? DBNull.Value);
                            cmdUpdate.Parameters.AddWithValue("@usuario", fila[4] ?? DBNull.Value);
                            cmdUpdate.Parameters.AddWithValue("@comentarios", fila[6] ?? DBNull.Value);
                            cmdUpdate.Parameters.AddWithValue("@tiporegistro", fila[7] ?? DBNull.Value);

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
                    InsertarRelacionDocumento(connection, transaction, facturaTipo, facturaLetra, facturaPuntoDeVenta, facturaNumero, tipoDeArchivo, letraDestino, ptovtaDestino, numeroDestino);
                }

                using (var cmdDelete = new NpgsqlCommand("DELETE FROM auxnd WHERE usuario = @usuarioAuditor", connection, transaction))
                {
                    cmdDelete.Parameters.AddWithValue("@usuarioAuditor", usuarioAuditor);
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

        public DataTable ObtenerCodigosPrestacionUnicos()
        {
            var dt = new DataTable();
            using var connection = new NpgsqlConnection(_connectionString);
            string query = "SELECT DISTINCT codigo FROM amb_liquidado ORDER BY codigo ASC";
            using var command = new NpgsqlCommand(query, connection);
            using var adapter = new NpgsqlDataAdapter(command);
            adapter.Fill(dt);
            return dt;
        }

        public void InsertarCambiosPrestacionTemporal(string tipoDocumentoTransmitido, string codigoNuevo, List<int> idPrestaciones)
        {
            // Respetamos la lógica original de destinos temporales
            string tablaDestino = tipoDocumentoTransmitido == "NC" ? "temporalnd" : "temporalnc";
            string query = $"INSERT INTO {tablaDestino} (codigonuevo, idprestacion) VALUES (@codigoNuevo, @idPrestacion);";

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                foreach (int idPrestacion in idPrestaciones)
                {
                    using var command = new NpgsqlCommand(query, connection, transaction);
                    command.Parameters.AddWithValue("@codigoNuevo", codigoNuevo);
                    command.Parameters.AddWithValue("@idPrestacion", idPrestacion);
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

        public DataTable ObtenerHistorialDocumento(string tipo, string letra, int ptovta, int numero)
        {
            DataTable historial = new DataTable();
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string tipoActual = tipo;
            string letraActual = letra;
            int ptovtaActual = ptovta;
            int numeroActual = numero;

            // 1. Buscar hacia atrás hasta encontrar la Factura original (FC)
            while (tipoActual != "FC")
            {
                string queryAtras = "SELECT tipo_doc_origen, letra_origen, ptovta_origen, numero_origen FROM relaciones WHERE tipo_doc_destino = @tipo AND letra_destino = @letra AND ptovta_destino = @ptovta AND numero_destino = @numero";
                using var cmd = new NpgsqlCommand(queryAtras, connection);
                cmd.Parameters.AddWithValue("@tipo", tipoActual);
                cmd.Parameters.AddWithValue("@letra", letraActual);
                cmd.Parameters.AddWithValue("@ptovta", ptovtaActual);
                cmd.Parameters.AddWithValue("@numero", numeroActual);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    tipoActual = reader["tipo_doc_origen"].ToString();
                    letraActual = reader["letra_origen"].ToString();
                    ptovtaActual = Convert.ToInt32(reader["ptovta_origen"]);
                    numeroActual = Convert.ToInt32(reader["numero_origen"]);
                }
                else
                {
                    break; // Cortamos si se rompe la cadena hacia atrás
                }
            }

            // 2. Teniendo la FC (la raíz), buscamos hacia adelante armando la cadena completa
            string queryAdelante = "SELECT * FROM relaciones WHERE tipo_doc_origen = @tipo AND letra_origen = @letra AND ptovta_origen = @ptovta AND numero_origen = @numero";
            using var adapter = new NpgsqlDataAdapter(queryAdelante, connection);

            bool hayMas = true;
            var visitados = new HashSet<string>(); // Prevención de bucles infinitos en DB

            while (hayMas)
            {
                string docId = $"{tipoActual}-{letraActual}-{ptovtaActual}-{numeroActual}";
                if (visitados.Contains(docId)) break;
                visitados.Add(docId);

                adapter.SelectCommand.Parameters.Clear();
                adapter.SelectCommand.Parameters.AddWithValue("@tipo", tipoActual);
                adapter.SelectCommand.Parameters.AddWithValue("@letra", letraActual);
                adapter.SelectCommand.Parameters.AddWithValue("@ptovta", ptovtaActual);
                adapter.SelectCommand.Parameters.AddWithValue("@numero", numeroActual);

                DataTable temp = new DataTable();
                adapter.Fill(temp);

                if (temp.Rows.Count > 0)
                {
                    if (historial.Columns.Count == 0)
                        historial = temp.Clone();

                    DataRow row = temp.Rows[0];
                    historial.ImportRow(row);

                    tipoActual = row["tipo_doc_destino"].ToString();
                    letraActual = row["letra_destino"].ToString();
                    ptovtaActual = Convert.ToInt32(row["ptovta_destino"]);
                    numeroActual = Convert.ToInt32(row["numero_destino"]);
                }
                else
                {
                    hayMas = false;
                }
            }

            return historial;
        }

        private void RegistrarCargaIncompleta(NpgsqlConnection connection, NpgsqlTransaction transaction, int idPrestacion, string tipoDocumento, string letra, int ptovta, int numero)
        {
            string comandoLimpiar = @"DELETE FROM cargaincompleta WHERE id_prestacion = @id_prestacion AND tipodocumento = @tipodocumento;";
            using (var cmdDel = new NpgsqlCommand(comandoLimpiar, connection, transaction))
            {
                cmdDel.Parameters.AddWithValue("@id_prestacion", idPrestacion);
                cmdDel.Parameters.AddWithValue("@tipodocumento", tipoDocumento);
                cmdDel.ExecuteNonQuery();
            }

            string comandoInsertar = @"INSERT INTO cargaincompleta (tipodocumento, letra, ptovta, numero, id_prestacion) VALUES (@tipodocumento, @letra, @ptovta, @numero, @id_prestacion);";
            using (var cmdIns = new NpgsqlCommand(comandoInsertar, connection, transaction))
            {
                cmdIns.Parameters.AddWithValue("@tipodocumento", tipoDocumento);
                cmdIns.Parameters.AddWithValue("@letra", letra);
                cmdIns.Parameters.AddWithValue("@ptovta", ptovta);
                cmdIns.Parameters.AddWithValue("@numero", numero);
                cmdIns.Parameters.AddWithValue("@id_prestacion", idPrestacion);
                cmdIns.ExecuteNonQuery();
            }
        }
    
    }
}