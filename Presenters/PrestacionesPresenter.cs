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

                // 2. Definir el comando SQL según el tipo (Esto simplifica tu método ConfigurarComandosYFiltrosPorTipoRegistro)
                string comandoSql = GenerarComandoSql(_view.FacturaTipo, tipoRegistro);

                // 3. Obtener los datos reales
                DataTable datos = _repository.ObtenerPrestacionesDocumento(
                    comandoSql,
                    _view.FacturaLetra,
                    _view.FacturaPuntoDeVenta,
                    _view.FacturaNumero);

                // 4. Mandar los datos a la vista
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

        private string GenerarComandoSql(string facturaTipo, string tipoRegistro)
        {
            switch (tipoRegistro)
            {
                case "Ambulatorios":
                    switch (facturaTipo)
                    {
                        case "NC":
                            return @"
                    SELECT al.modulo AS modulo, al.nro_internacion AS Nro_Int, al.fecha_ingreso AS F_Ingreso, al.fecha_egreso AS F_Egreso, 
                        al.carnet, al.paciente, al.codigo_cobertura AS Cobertura, al.plan AS Plan, al.medico, al.fecha, al.codigo, al.descripcion,
                        al.cantidad, al.total_neto, al.coseguro, al.total,
                        al.cob_factura_tipo, al.cob_factura_letra, al.cob_factura_ptoventa, al.cob_factura_numero, al.id AS ID_Prestacion,
                        nc.id AS id, nc.debitoaceptado AS NC_DebitoAceptado, nc.motivodedebito AS NC_MotivoDeDebito, nc.importedebitado AS NC_ImporteDebitado, nc.prestacionenglobante AS NC_PrestacionEnglobante, nc.motivoderefactura AS NC_MotivoDeRefactura, nc.importederefactura AS NC_ImporteDeRefactura, nc.comentarios as NC_Comentarios,
                        nd.motivorefactura AS ND_MotivoDeRefactura, nd.importerefactura AS ND_ImporteDeRefactura, nd.comentarios AS ND_Comentarios
                    FROM notadecredito nc
                    LEFT JOIN notadedebito nd ON nc.id = nd.id_notadecredito
                    JOIN amb_liquidado al ON nc.id_prestacion = al.id
                    WHERE nc.letra = @FacturaLetra
                      AND nc.ptovta = @FacturaPuntoVenta
                      AND nc.numero = @FacturaNumero;";

                        case "ND":
                            return @"
                    SELECT al.modulo AS modulo, al.nro_internacion AS Nro_Int, al.fecha_ingreso AS F_Ingreso, al.fecha_egreso AS F_Egreso, 
                        al.codigo, al.carnet, al.paciente, al.codigo_cobertura AS Cobertura, al.plan AS Plan, al.medico, nc1.letra AS NC_Previo_Letra, nc1.ptovta AS NC_Previo_PuntoDeVenta, nc1.numero AS NC_Previo_Numero, 
                        nc1.fecha AS NC_Previo_Fecha, nc1.motivodedebito AS NC_Previo_MotivoDeDebito, nc1.importedebitado AS NC_Previo_ImporteDebitado, 
                        nc1.motivoderefactura AS NC_Previo_MotivoDeRefactura, nc1.id_prestacion AS ID_Prestacion, nd.id, nd.motivorefactura, nd.importerefactura, 
                        nd.fecha, nd.comentarios, nc.debitoaceptado AS NC_DebitoAceptado, nc.motivodedebito AS NC_MotivoDeDebito, nc.diasfacturados AS NC_DiasFacturados, nc.prestacionenglobante AS NC_PrestacionEnglobante,
                        nc.importedebitado AS NC_ImporteDebitado, nc.motivoderefactura AS NC_MotivoDeRefactura, nc.importederefactura AS NC_ImporteDeRefactura, 
                        nc.comentarios AS NC_Comentarios 
                    FROM notadedebito nd 
                    RIGHT JOIN notadecredito nc1 ON nd.id_notadecredito = nc1.id 
                    LEFT JOIN notadecredito nc 
                    ON nd.id = nc.id_notadedebito 
                    LEFT JOIN amb_liquidado al ON al.id = nc1.id_prestacion 
                    WHERE nd.letra = @FacturaLetra 
                        AND nd.ptovta = @FacturaPuntoVenta 
                        AND nd.numero = @FacturaNumero;";

                        case "FC":
                            return @"
                    SELECT al.modulo AS modulo, al.nro_internacion AS Nro_Int, al.fecha_ingreso AS F_Ingreso, al.fecha_egreso AS F_Egreso, 
                        al.carnet, al.paciente, al.codigo_cobertura AS Cobertura, al.plan AS Plan, al.medico, al.fecha, al.codigo, al.descripcion, 
                        al.cantidad, al.total_neto, al.coseguro, al.total, 
                        al.porcentaje_especialista, al.porcentaje_ayudante1, al.porcentaje_anestesista, al.porcentaje_gastos, al.id AS ID_Prestacion,
                        nc.fecha AS NC_Fecha, nc.letra AS NC_Letra, nc.ptovta AS NC_PuntoDeVenta, nc.numero AS NC_Numero, nc.debitoaceptado AS NC_DebitoAceptado, nc.motivodedebito AS NC_MotivoDeDebito, nc.diasfacturados AS NC_DiasFacturados, nc.importedebitado AS NC_ImporteDebitado, 
                        nc.prestacionenglobante AS NC_PrestacionEnglobante, nc.motivoderefactura AS NC_MotivoDeRefactura, nc.importederefactura AS NC_ImporteDeRefactura, nc.cargadocompletamente, nc.comentarios AS NC_Comentarios
                    FROM amb_liquidado al
                    LEFT JOIN notadecredito nc ON al.id = nc.id_prestacion
                    WHERE al.cob_factura_letra = @FacturaLetra
                      AND al.cob_factura_ptoventa = @FacturaPuntoVenta
                      AND al.cob_factura_numero = @FacturaNumero;";
                    }
                    break;

                case "Internados":
                    switch (facturaTipo)
                    {
                        case "NC":
                            return @"
                    SELECT al.modulo AS modulo, al.grupomodulo AS grupomodulo, al.nro_internacion AS Nro_Int, al.fecha_ingreso AS F_Ingreso, al.fecha_egreso AS F_Egreso, 
                        al.carnet, al.paciente,  al.codigo_cobertura AS Cobertura, al.plan AS Plan, al.medico, al.fecha, al.codigo, al.descripcion,
                        al.cantidad, al.total_neto, al.coseguro, al.total,
                        al.cob_factura_tipo, al.cob_factura_letra, al.cob_factura_ptoventa, al.cob_factura_numero, al.id AS ID_Prestacion,
                        nc.id AS id, nc.debitoaceptado AS NC_DebitoAceptado, nc.motivodedebito AS NC_MotivoDeDebito, nc.importedebitado AS NC_ImporteDebitado, nc.prestacionenglobante AS NC_PrestacionEnglobante, nc.motivoderefactura AS NC_MotivoDeRefactura, nc.importederefactura AS NC_ImporteDeRefactura, nc.comentarios as NC_Comentarios,
                        nd.motivorefactura AS ND_MotivoDeRefactura, nd.importerefactura AS ND_ImporteDeRefactura, nd.comentarios AS ND_Comentarios
                    FROM notadecredito nc
                    LEFT JOIN notadedebito nd ON nc.id = nd.id_notadecredito
                    JOIN amb_liquidado al ON nc.id_prestacion = al.id
                    WHERE nc.letra = @FacturaLetra
                      AND nc.ptovta = @FacturaPuntoVenta
                      AND nc.numero = @FacturaNumero;";

                        case "ND":
                            return @"
                    SELECT al.modulo AS modulo, al.grupomodulo AS grupomodulo, al.nro_internacion AS Nro_Int, al.fecha_ingreso AS F_Ingreso, al.fecha_egreso AS F_Egreso,  
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
                    LEFT JOIN notadecredito nc 
                    ON nd.id = nc.id_notadedebito 
                    LEFT JOIN amb_liquidado al ON al.id = nc1.id_prestacion 
                    WHERE nd.letra = @FacturaLetra 
                        AND nd.ptovta = @FacturaPuntoVenta 
                        AND nd.numero = @FacturaNumero;";

                        case "FC":
                            return @"
                    SELECT al.modulo AS modulo, al.grupomodulo AS grupomodulo, al.nro_internacion AS Nro_Int, al.fecha_ingreso AS F_Ingreso, al.fecha_egreso AS F_Egreso, 
                        al.carnet, al.paciente, al.codigo_cobertura AS Cobertura, al.plan AS Plan, al.medico, al.fecha, al.codigo, al.descripcion, 
                        al.cantidad, al.total_neto, al.coseguro, al.total, 
                        al.porcentaje_especialista, al.porcentaje_ayudante1, al.porcentaje_anestesista, al.porcentaje_gastos, al.id AS ID_Prestacion,
                        nc.fecha AS NC_Fecha, nc.letra AS NC_Letra, nc.ptovta AS NC_PuntoDeVenta, nc.numero AS NC_Numero, nc.debitoaceptado AS NC_DebitoAceptado, nc.motivodedebito AS NC_MotivoDeDebito, nc.diasfacturados AS NC_DiasFacturados, nc.importedebitado AS NC_ImporteDebitado, 
                        nc.prestacionenglobante AS NC_PrestacionEnglobante, nc.motivoderefactura AS NC_MotivoDeRefactura, nc.importederefactura AS NC_ImporteDeRefactura, nc.cargadocompletamente, nc.comentarios AS NC_Comentarios
                    FROM amb_liquidado al
                    LEFT JOIN notadecredito nc ON al.id = nc.id_prestacion
                    WHERE al.cob_factura_letra = @FacturaLetra
                      AND al.cob_factura_ptoventa = @FacturaPuntoVenta
                      AND al.cob_factura_numero = @FacturaNumero;";
                    }
                    break;
            }

            return "";
        }
    }
}