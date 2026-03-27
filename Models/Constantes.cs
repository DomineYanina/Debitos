namespace Debitos.Models
{
    public static class TipoDocumento
    {
        public const string Factura = "FC";
        public const string NotaCredito = "NC";
        public const string NotaDebito = "ND";
    }

    public static class TipoRegistro
    {
        public const string Ambulatorios = "Ambulatorios";
        public const string Internados = "Internados";
    }

    public static class MotivoDebito
    {
        public const string IncluidaEnOtra = "Prestacion incluida en otra";
        public const string NoAplica = "No aplica";
        public const string Borrar = "Borrar";
    }
}