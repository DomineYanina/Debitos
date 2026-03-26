using System;

namespace Debitos.Models
{
    public class CargaParcialDTO
    {
        public int IdPrestacion { get; set; }
        public bool DebitoAceptado { get; set; }
        public object? MotivoDebito { get; set; }
        public object? ImporteDebitado { get; set; }
        public object? DiasFacturados { get; set; }
        public object? MotivoRefactura { get; set; }
        public object? ImporteRefactura { get; set; }
        public string? PrestacionEnglobante { get; set; }
        public string? Comentarios { get; set; }
        public bool CargadoCompletamente { get; set; }
        public string Usuario { get; set; }

    }
}