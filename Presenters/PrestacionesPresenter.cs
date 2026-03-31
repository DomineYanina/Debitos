using Debitos.Models;
using Debitos.Repositories;
using Debitos.Views;
using System;
using System.Data;

namespace Debitos.Presenters
{
    public class PrestacionesPresenter
    {
        private readonly IPrestacionesView _view;
        private readonly DebitosRepository _repository;
        private readonly string _usuarioAuditor;

        public PrestacionesPresenter(IPrestacionesView view, DebitosRepository repository, string usuarioAuditor)
        {
            _view = view;
            _repository = repository;
            _usuarioAuditor = usuarioAuditor;

            // Suscribimos el presentador a los eventos de la vista
            _view.BuscarDocumentoEvent += BuscarDocumento;
            _view.GuardarParcialmenteEvent += GuardarParcialmente;
            _view.GenerarNotaDeCreditoEvent += GenerarNotaDeCredito;
        }

        public void RecalcularTotales()
        {
            DataTable dt = _view.ObtenerDataTableActual();
            if (dt == null || dt.Rows.Count == 0)
            {
                _view.VisibilidadTotales = false;
                return;
            }

            try
            {
                // 1. Contar cuántas filas tienen débito aceptado
                int filasAceptadas = dt.Select("nc_debitoaceptado = true").Length;

                // 2. Sumar el importe total debitado
                object sumDebito = dt.Compute("SUM(nc_importedebitado)", "nc_debitoaceptado = true");
                double totalDebitado = sumDebito != DBNull.Value ? Convert.ToDouble(sumDebito) : 0;

                // 3. NUEVO: Sumar el importe total a refacturar
                // Determinamos la columna dinámicamente según el tipo de documento auditado
                string colRefactura = _view.FacturaTipo == "NC" ? "ND_ImporteDeRefactura" : "NC_ImporteDeRefactura";

                double totalRefacturar = 0;
                if (dt.Columns.Contains(colRefactura))
                {
                    // Sumamos iterando fila por fila para tener un control estricto de los valores DBNull
                    foreach (DataRow row in dt.Rows)
                    {
                        // El importe a refacturar se suma cuando el débito NO está aceptado
                        bool aceptado = row.Table.Columns.Contains("nc_debitoaceptado") &&
                                        row["nc_debitoaceptado"] != DBNull.Value &&
                                        Convert.ToBoolean(row["nc_debitoaceptado"]);

                        if (!aceptado && row[colRefactura] != DBNull.Value)
                        {
                            totalRefacturar += Convert.ToDouble(row[colRefactura]);
                        }
                    }
                }

                // 4. Actualizar la Vista
                _view.TextoTotalRegistros = $"Total Registros: {dt.Rows.Count} | Aceptados: {filasAceptadas}";

                // Inyectamos el nuevo total en la misma etiqueta visual
                _view.TextoMontosNoAceptados = $"Total Debitado: {totalDebitado:C2}   |   Total a Refacturar: {totalRefacturar:C2}";

                _view.VisibilidadTotales = true;
            }
            catch (Exception)
            {
                // Si algo falla, ocultamos por seguridad
                _view.VisibilidadTotales = false;
            }
        }

        private void GenerarNotaDeCredito(object sender, EventArgs e)
        {
            try
            {
                // Lógica de negocio: Limpiar auxiliares
                _repository.LimpiarAuxiliarNC(_usuarioAuditor);

                // Obtenemos los datos para procesar
                DataTable datos = _view.ObtenerDataTableActual();

                // Extraemos los datos de la tabla, AHORA INCLUYENDO EL ID (Ver paso 2)
                var lista = MapearDatosParaAuxiliar(datos);

                // Lógica de decisión según el tipo de documento de origen
                if (_view.FacturaTipo == "FC" || _view.FacturaTipo == Debitos.Models.TipoDocumento.Factura)
                {
                    _repository.InsertarAuxiliarNC_FC(lista, _usuarioAuditor, _view.TipoRegistroFiltrado);
                }
                else if (_view.FacturaTipo == "ND" || _view.FacturaTipo == Debitos.Models.TipoDocumento.NotaDebito)
                {
                    // ACÁ ESTABA EL PROBLEMA: Llamamos al método que guarda en el auxiliar 
                    // la información de la ND, asegurando que el ID viaje a la base de datos
                    _repository.InsertarAuxiliarNC_ND(lista, _usuarioAuditor, _view.TipoRegistroFiltrado);
                }

                // Ordenar acciones a la vista
                _view.AbrirFormularioNotaDeCredito(true, _usuarioAuditor);
                _view.LimpiarUI_PostOperacion();
            }
            catch (Exception ex)
            {
                _view.MostrarMensaje("Error al generar nota: " + ex.Message);
            }
        }

        private List<(int idPrestacion, object? motivoRefactura, object? motivoDebito, double? importeRefactura, double? importeDebito, string? comentarios, bool debitoAceptado, object? diasFacturados, string? prestacionEnglobante, int? idNotaDeDebito, string? codigo)> MapearDatosParaAuxiliar(DataTable dt)
        {
            var lista = new List<(int, object?, object?, double?, double?, string?, bool, object?, string?, int?, string?)>();

            foreach (DataRow row in dt.Rows)
            {
                bool debitoAceptado = row["NC_DebitoAceptado"] != DBNull.Value && Convert.ToBoolean(row["NC_DebitoAceptado"]);
                string? motivoRefactura = row["NC_MotivoDeRefactura"]?.ToString();
                string? motivoDebito = row["NC_MotivoDeDebito"]?.ToString();

                // Solo procesamos las filas donde el auditor cargó información
                if (debitoAceptado || !string.IsNullOrEmpty(motivoRefactura) || !string.IsNullOrEmpty(motivoDebito))
                {
                    lista.Add((
                        Convert.ToInt32(row["ID_Prestacion"]),
                        string.IsNullOrEmpty(motivoRefactura) ? DBNull.Value : motivoRefactura,
                        string.IsNullOrEmpty(motivoDebito) ? DBNull.Value : motivoDebito,
                        row["NC_ImporteDeRefactura"] != DBNull.Value ? Convert.ToDouble(row["NC_ImporteDeRefactura"]) : (double?)null,
                        row["NC_ImporteDebitado"] != DBNull.Value ? Convert.ToDouble(row["NC_ImporteDebitado"]) : (double?)null,
                        row["NC_Comentarios"]?.ToString()?.Replace("\0", "").Trim(),
                        debitoAceptado,
                        row.Table.Columns.Contains("NC_DiasFacturados") && row["NC_DiasFacturados"] != DBNull.Value ? Convert.ToInt32(row["NC_DiasFacturados"]) : DBNull.Value,
                        row["NC_PrestacionEnglobante"]?.ToString(),

                        // ESTA ES LA CLAVE: Extraemos el ID del DataTable para usarlo como id_notadedebito
                        row.Table.Columns.Contains("id") && row["id"] != DBNull.Value ? Convert.ToInt32(row["id"]) : (int?)null,

                        row.Table.Columns.Contains("codigo") ? row["codigo"]?.ToString() : null
                    ));
                }
            }
            return lista;
        }

        private void GuardarParcialmente(object sender, EventArgs e)
        {
            _view.PrepararUI_GuardadoParcial();

            var vistaDatos = _view.ObtenerDatosFiltrados();
            if (vistaDatos == null || vistaDatos.Count == 0) return;

            var listaParaGuardar = MapearDatosACargaParcial(vistaDatos, _view.FacturaTipo);

            if (listaParaGuardar.Count > 0)
            {
                try
                {
                    // Lógica de decisión centralizada en el Presentador
                    switch (_view.FacturaTipo)
                    {
                        case TipoDocumento.Factura:
                            _repository.GuardarCargaParcialFC(listaParaGuardar);
                            break;
                        case TipoDocumento.NotaCredito:
                            _repository.GuardarCargaParcialNC(listaParaGuardar, _view.FacturaTipo, _view.FacturaLetra, _view.FacturaPuntoDeVenta, _view.FacturaNumero);
                            break;
                        case TipoDocumento.NotaDebito:
                            _repository.GuardarCargaParcialND(listaParaGuardar, _view.FacturaTipo, _view.FacturaLetra, _view.FacturaPuntoDeVenta, _view.FacturaNumero);
                            break;
                    }

                    _view.MostrarMensaje("Se ha almacenado de forma correcta parcialmente el documento");
                }
                catch (Exception ex)
                {
                    _view.MostrarMensaje("Error al guardar: " + ex.Message);
                }
            }
        }

        private List<CargaParcialDTO> MapearDatosACargaParcial(DataView vistaDatos, string tipoFactura)
        {
            var lista = new List<CargaParcialDTO>();

            foreach (DataRowView rowView in vistaDatos)
            {
                DataRow row = rowView.Row;

                bool debitoAceptado = row.Table.Columns.Contains("nc_debitoaceptado") && row["nc_debitoaceptado"] != DBNull.Value && Convert.ToBoolean(row["nc_debitoaceptado"]);
                string motivoDebito = row.Table.Columns.Contains("nc_motivodedebito") ? row["nc_motivodedebito"]?.ToString() ?? "" : "";

                string columnaRefactura = tipoFactura == "NC" ? "nd_motivoderefactura" : "nc_motivoderefactura";
                string motivoRefactura = row.Table.Columns.Contains(columnaRefactura) ? row[columnaRefactura]?.ToString() ?? "" : "";

                if (debitoAceptado || !string.IsNullOrEmpty(motivoDebito) || !string.IsNullOrEmpty(motivoRefactura))
                {
                    var dto = new CargaParcialDTO
                    {
                        // ¡ACÁ ESTABA EL ERROR QUE ARROJABA EL 178801!
                        // Antes mapeábamos "id_prestacion" que traía el código, ahora mapeamos el ID correcto
                        IdPrestacion = Convert.ToInt32(row["id_prestacion"]),
                        DebitoAceptado = debitoAceptado,
                        MotivoDebito = motivoDebito,
                        ImporteDebitado = row.Table.Columns.Contains("nc_importedebitado") ? row["nc_importedebitado"] : DBNull.Value,
                        MotivoRefactura = motivoRefactura,
                        ImporteRefactura = tipoFactura == "NC" ? row["nd_importederefactura"] : row["nc_importederefactura"],
                        Comentarios = tipoFactura == "NC" ? row["nd_comentarios"]?.ToString() : row["nc_comentarios"]?.ToString(),
                        CargadoCompletamente = false,
                        Usuario = _usuarioAuditor,
                        TipoRegistro = _view.TipoRegistroFiltrado
                    };

                    if (tipoFactura == "NC")
                    {
                        dto.IdNotaDeCredito = Convert.ToInt32(row["id"]); // Mantenemos coherencia
                        dto.Codigo = row.Table.Columns.Contains("codigo") ? row["codigo"]?.ToString() : null;
                    }
                    if (tipoFactura == "FC" || tipoFactura == "ND")
                    {
                        dto.IdNotaDeDebito = Convert.ToInt32(row["id"]);
                        dto.PrestacionEnglobante = row.Table.Columns.Contains("nc_prestacionenglobante") ? row["nc_prestacionenglobante"]?.ToString() : null;
                        dto.DiasFacturados = row.Table.Columns.Contains("nc_diasfacturados") ? row["nc_diasfacturados"] : DBNull.Value;
                    }

                    lista.Add(dto);
                }
            }
            return lista;
        }

        private void BuscarDocumento(object sender, EventArgs e)
        {
            try
            {
                _view.MostrarCargando(true);

                // 1. Obtener tipo de registro
                string tipoRegistro = _repository.ObtenerTipoRegistro(
                    _view.FacturaTipo,
                    _view.FacturaLetra,
                    _view.FacturaPuntoDeVenta,
                    _view.FacturaNumero);

                if (string.IsNullOrEmpty(tipoRegistro))
                {
                    _view.MostrarMensaje("No se ha encontrado el documento ingresado.");
                    return;
                }

                _view.TipoRegistroFiltrado = tipoRegistro;

                // 2. Obtener los datos reales (Delegamos la responsabilidad 100% al Repositorio)
                DataTable datos = _repository.ObtenerPrestacionesDocumento(
                    _view.FacturaTipo,
                    tipoRegistro,
                    _view.FacturaLetra,
                    _view.FacturaPuntoDeVenta,
                    _view.FacturaNumero);

                // 3. Mandar los datos a la vista
                _view.DatosGrilla = datos;
            }
            catch (Exception ex)
            {
                _view.MostrarMensaje("Error al buscar el documento: " + ex.Message);
            }
            finally
            {
                _view.MostrarCargando(false);
            }
        }

    }
}