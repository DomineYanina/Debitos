using System;
using System.Data;

namespace Debitos.Views
{
    public interface IPrestacionesView
    {
        // Propiedades que el presentador necesita leer o escribir
        string FacturaTipo { get; }
        string FacturaLetra { get; }
        int FacturaPuntoDeVenta { get; }
        int FacturaNumero { get; }
        string TipoRegistroFiltrado { get; set; }
        DataTable DatosGrilla { get; set; }

        // Eventos para avisarle al presentador que el usuario hizo algo
        event EventHandler BuscarDocumentoEvent;
        event EventHandler GuardarParcialmenteEvent; // <-- NUEVO EVENTO
        event EventHandler GenerarNotaDeCreditoEvent;

        // Métodos para que el presentador controle la UI
        void MostrarMensaje(string mensaje);
        void MostrarCargando(bool mostrar);
        void PrepararUI_GuardadoParcial(); // <-- NUEVO MÉTODO
        void AbrirFormularioNotaDeCredito(bool cargaACompletar, string usuario);
        void LimpiarUI_PostOperacion();

        // Método para entregarle los datos en memoria al presentador
        DataView ObtenerDatosFiltrados(); // <-- NUEVO MÉTODO
        DataTable ObtenerDataTableActual();
    }
}