using System;
using System.Data;

namespace Debitos.Views
{
    public interface IPrestacionesView
    {
        // Propiedades que la vista debe exponer (lo que el usuario ingresa)
        string FacturaTipo { get; }
        string FacturaLetra { get; }
        int FacturaPuntoDeVenta { get; }
        int FacturaNumero { get; }

        // Propiedades para mostrar datos (lo que el presentador le manda a la vista)
        DataTable DatosGrilla { get; set; }
        bool BotonBuscarVisible { get; set; }

        // Métodos para mostrar mensajes
        void MostrarMensaje(string mensaje);
        void MostrarCargando(bool mostrar);

        // Eventos que la vista disparará y el presentador escuchará
        event EventHandler BuscarDocumentoEvent;
    }
}