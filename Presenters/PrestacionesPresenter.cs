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
                        case "FC":
                            _repository.GuardarCargaParcialFC(listaParaGuardar);
                            break;
                        case "NC":
                            _repository.GuardarCargaParcialNC(listaParaGuardar, _view.FacturaTipo, _view.FacturaLetra, _view.FacturaPuntoDeVenta, _view.FacturaNumero);
                            break;
                        case "ND":
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

        // Centralizamos la extracción de datos (antes estaba repetida en 3 métodos en Form1)
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

                    if (tipoFactura != "FC")
                    {
                        dto.IdNotaDeCredito = Convert.ToInt32(row["id_prestacion"]);
                        dto.Codigo = row.Table.Columns.Contains("codigo") ? row["codigo"]?.ToString() : null;
                    }
                    if (tipoFactura == "FC" || tipoFactura == "ND")
                    {
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