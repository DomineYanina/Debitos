using System;
using System.Data;
using Debitos.Views;
using Debitos.Repositories;

namespace Debitos.Presenters
{
    public class PrestacionesPresenter
    {
        private readonly IPrestacionesView _view;
        private readonly DebitosRepository _repository;

        public PrestacionesPresenter(IPrestacionesView view, DebitosRepository repository)
        {
            _view = view;
            _repository = repository;

            // Suscribimos el presentador a los eventos de la vista
            _view.BuscarDocumentoEvent += BuscarDocumento;
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