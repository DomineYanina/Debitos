/*using Npgsql;
using System.Data;

namespace Debitos; // Considera usar un namespace más específico para la capa de datos

public class DebitosRepository
{
    private readonly string _connectionString;

    public DebitosRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Ejecuta una consulta SQL y retorna los resultados en un DataTable.
    /// Utiliza este método para todas las operaciones de SELECT.
    /// </summary>
    /// <param name="query">La consulta SQL a ejecutar.</param>
    /// <param name="parameters">Parámetros opcionales para la consulta, para prevenir inyección SQL.</param>
    /// <returns>Un DataTable con los resultados de la consulta.</returns>
    private DataTable ExecuteQuery(string query, NpgsqlParameter[]? parameters = null)
    {
        DataTable dataTable = new DataTable();
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new NpgsqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }
                using (var adapter = new NpgsqlDataAdapter(command))
                {
                    adapter.Fill(dataTable);
                }
            }
        }
        return dataTable;
    }

    /// <summary>
    /// Obtiene los datos de facturación iniciales para el DataGridView.
    /// </summary>
    public DataTable GetDatosFacturacion(bool soloValorizadas)
    {
        string query = @"
            SELECT
                t.id, t.fecha as ""Fecha Factura"", t.paciente, t.facturanro AS ""Factura"", t.letrado AS ""Médico"",
                t.prestacion, t.prestaciondescripcion AS ""Descripción Prestación"", t.cantidad,
                t.importe AS ""Importe Factura"",
                CASE WHEN t.solo_valorizadas = true THEN 'Sí' ELSE 'No' END AS ""Solo Valorizadas"",
                COALESCE(nr.nc_numeroregistro, 0) AS ""Nro de NC"", nr.nc_tipodocumento AS ""NC Tipo Documento"",
                nr.nc_letracredito AS ""NC Letra Crédito"", nr.nc_numerodocumento AS ""NC Nro Documento"",
                nr.nc_puntoventa AS ""NC Punto Venta"", nr.nc_fecharegistro AS ""NC Fecha Registro"",
                nr.nc_fechadebito AS ""NC Fecha Débito"", nr.nc_motivodefactura AS ""NC Motivo Factura"",
                nr.nc_subtipodefactura AS ""NC Subtipo Factura"", nr.nc_motivoderefactura AS ""NC Motivo Refactura"",
                nr.nc_observaciones AS ""NC Observaciones"", nr.nc_estadorefactura AS ""NC Estado Refactura"",
                nr.nc_importederefactura AS ""NC Importe Refactura"", nr.nc_importedebitado AS ""NC Importe Debitado"",
                nr.nc_importeanulado AS ""NC Importe Anulado"", nr.nc_importepagado AS ""NC Importe Pagado"",
                nr.nc_importedebito AS ""NC Importe Débito"", nr.nc_importenota AS ""NC Importe Nota"",
                nr.nc_motivoanulacion AS ""NC Motivo Anulación"", nr.nc_importenotaliquidada AS ""NC Importe Nota Liquidada"",
                nr.nc_tiporegistro AS ""NC Tipo Registro"", nr.nc_codigodeclinica AS ""NC Código Clínica"",
                nr.nc_id_original AS ""NC ID Original"", nr.nc_fechamodificacion AS ""NC Fecha Modificación""
            FROM
                debito.tabla t
            LEFT JOIN
                debito.notasregistro nr ON t.id = nr.nc_id_original
            WHERE
                (t.solo_valorizadas = @SoloValorizadas OR @SoloValorizadas IS NULL)
            ORDER BY
                t.fecha DESC;
        ";
        NpgsqlParameter[] parameters = new NpgsqlParameter[]
        {
            new NpgsqlParameter("@SoloValorizadas", soloValorizadas ? (object)true : DBNull.Value)
        };
        return ExecuteQuery(query, parameters);
    }


    /// <summary>
    /// Obtiene la lista de pacientes para el filtro.
    /// </summary>
    public DataTable GetPacientes()
    {
        string query = "SELECT DISTINCT paciente FROM debito.tabla ORDER BY paciente;";
        return ExecuteQuery(query);
    }

    /// <summary>
    /// Obtiene la lista de profesionales para el filtro.
    /// </summary>
    public DataTable GetProfesionales()
    {
        string query = "SELECT DISTINCT letrado FROM debito.tabla ORDER BY letrado;";
        return ExecuteQuery(query);
    }

    /// <summary>
    /// Obtiene la lista de prestaciones para el filtro.
    /// </summary>
    public DataTable GetPrestaciones()
    {
        string query = "SELECT DISTINCT prestacion FROM debito.tabla ORDER BY prestacion;";
        return ExecuteQuery(query);
    }

    /// <summary>
    /// Obtiene los tipos de factura para el filtro.
    /// </summary>
    public DataTable GetTiposFactura()
    {
        string query = "SELECT DISTINCT facturatipo FROM debito.tabla ORDER BY facturatipo;";
        return ExecuteQuery(query);
    }

    /// <summary>
    /// Obtiene las letras de factura para el filtro.
    /// </summary>
    public DataTable GetLetrasFactura()
    {
        string query = "SELECT DISTINCT facturaletra FROM debito.tabla ORDER BY facturaletra;";
        return ExecuteQuery(query);
    }

    /// <summary>
    /// Obtiene los puntos de venta de factura para el filtro.
    /// </summary>
    public DataTable GetPuntosVentaFactura()
    {
        string query = "SELECT DISTINCT facturapuntodeventa FROM debito.tabla ORDER BY facturapuntodeventa;";
        return ExecuteQuery(query);
    }

    /// <summary>
    /// Obtiene los números de factura para el filtro.
    /// </summary>
    public DataTable GetNumerosFactura()
    {
        string query = "SELECT DISTINCT facturanro FROM debito.tabla ORDER BY facturanro;";
        return ExecuteQuery(query);
    }


    // Método de ejemplo para guardar datos (ajustar según necesidad real)
    // Este es solo un ejemplo, la lógica de guardado puede ser mucho más compleja
    // y debería manejar inserciones/actualizaciones/eliminaciones específicas.
    public void UpdateDebitoRecord(int id, string column, object value)
    {
        string query = $"UPDATE debito.tabla SET {column} = @value WHERE id = @id;";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@value", value ?? DBNull.Value);
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }
        }
    }
}*/