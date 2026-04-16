using System.Drawing;
using System.Windows.Forms;

namespace Debitos
{
    public static class DataGridFormatter
    {
        public static void AplicarColorCelda(DataGridViewCell cell, Color color, bool readOnly = true)
        {
            cell.Style.BackColor = color;
            cell.ReadOnly = readOnly;
        }

        public static void FormatearFilaParaFC(DataGridViewRow row)
        {
            if (row.IsNewRow) return;

            AplicarColorCelda(row.Cells["paciente"], Color.LightCyan);
            AplicarColorCelda(row.Cells["Plan"], Color.LightCyan);
            AplicarColorCelda(row.Cells["efector"], Color.LightCyan);
            AplicarColorCelda(row.Cells["medico"], Color.LightCyan);
            AplicarColorCelda(row.Cells["fecha"], Color.LightCyan);
            AplicarColorCelda(row.Cells["codigo"], Color.LightCyan);
            AplicarColorCelda(row.Cells["descripcion"], Color.LightCyan);
            AplicarColorCelda(row.Cells["cantidad"], Color.LightCyan);
            AplicarColorCelda(row.Cells["total_neto"], Color.LightCyan);
            AplicarColorCelda(row.Cells["coseguro"], Color.LightCyan);
            AplicarColorCelda(row.Cells["total"], Color.LightCyan);

            AplicarColorCelda(row.Cells["nc_MotivoDeRefactura"], Color.LightGray, false);
            AplicarColorCelda(row.Cells["nc_MotivoDeDebito"], Color.LightGray, false);
            AplicarColorCelda(row.Cells["nc_importedebitado"], Color.LightGray, false);
            AplicarColorCelda(row.Cells["nc_importederefactura"], Color.LightGray, false);
            AplicarColorCelda(row.Cells["nc_debitoaceptado"], Color.LightGray, false);
            AplicarColorCelda(row.Cells["NC_DiasFacturados"], Color.LightGray, false);

            var motivorefactura = row.Cells["nc_motivoderefactura"]?.Value?.ToString();
            var colorComentario = string.IsNullOrEmpty(motivorefactura) ? Color.Coral : Color.LightGray;
            AplicarColorCelda(row.Cells["nc_comentarios"], colorComentario, false);
        }

        public static void FormatearFilaParaNC(DataGridViewRow row)
        {
            if (row.IsNewRow) return;

            AplicarColorCelda(row.Cells["paciente"], Color.LightCyan);
            AplicarColorCelda(row.Cells["efector"], Color.LightCyan);
            AplicarColorCelda(row.Cells["medico"], Color.LightCyan);
            AplicarColorCelda(row.Cells["fecha"], Color.LightCyan);
            AplicarColorCelda(row.Cells["codigo"], Color.LightCyan);
            AplicarColorCelda(row.Cells["descripcion"], Color.LightCyan);
            AplicarColorCelda(row.Cells["cantidad"], Color.LightCyan);
            AplicarColorCelda(row.Cells["total_neto"], Color.LightCyan);
            AplicarColorCelda(row.Cells["coseguro"], Color.LightCyan);
            AplicarColorCelda(row.Cells["total"], Color.LightCyan);
            AplicarColorCelda(row.Cells["Plan"], Color.LightCyan);
            AplicarColorCelda(row.Cells["cob_factura_tipo"], Color.LightCyan);
            AplicarColorCelda(row.Cells["cob_factura_letra"], Color.LightCyan);
            AplicarColorCelda(row.Cells["cob_factura_ptoventa"], Color.LightCyan);
            AplicarColorCelda(row.Cells["cob_factura_numero"], Color.LightCyan);
            AplicarColorCelda(row.Cells["NC_PrestacionEnglobante"], Color.LightCyan);
            AplicarColorCelda(row.Cells["nc_MotivoDeDebito"], Color.LightCyan);
            AplicarColorCelda(row.Cells["nc_importedebitado"], Color.LightCyan);
            AplicarColorCelda(row.Cells["nc_debitoaceptado"], Color.LightCyan);
            AplicarColorCelda(row.Cells["nc_MotivoDeRefactura"], Color.LightCyan);
            AplicarColorCelda(row.Cells["nc_ImporteDeRefactura"], Color.LightCyan);
            AplicarColorCelda(row.Cells["nc_comentarios"], Color.LightCyan);

            AplicarColorCelda(row.Cells["nd_MotivoDeRefactura"], Color.LightGray);
            AplicarColorCelda(row.Cells["nd_importederefactura"], Color.LightGray);

            var motivorefactura = row.Cells["nd_motivoderefactura"]?.Value?.ToString();
            var colorComentario = string.IsNullOrEmpty(motivorefactura) ? Color.Coral : Color.LightGray;
            AplicarColorCelda(row.Cells["nd_comentarios"], colorComentario, false);
        }

        public static void FormatearFilaParaND(DataGridViewRow row)
        {
            if (row.IsNewRow) return;

            AplicarColorCelda(row.Cells["NC_Previo_Letra"], Color.LightCyan);
            AplicarColorCelda(row.Cells["NC_Previo_PuntoDeVenta"], Color.LightCyan);
            AplicarColorCelda(row.Cells["NC_Previo_Numero"], Color.LightCyan);
            AplicarColorCelda(row.Cells["NC_Previo_Fecha"], Color.LightCyan);
            AplicarColorCelda(row.Cells["NC_Previo_MotivoDeDebito"], Color.LightCyan);
            AplicarColorCelda(row.Cells["NC_Previo_ImporteDebitado"], Color.LightCyan);
            AplicarColorCelda(row.Cells["NC_Previo_MotivoDeRefactura"], Color.LightCyan);
            AplicarColorCelda(row.Cells["fecha"], Color.LightCyan);
            AplicarColorCelda(row.Cells["motivorefactura"], Color.LightCyan);
            AplicarColorCelda(row.Cells["Plan"], Color.LightCyan);
            AplicarColorCelda(row.Cells["importerefactura"], Color.LightCyan);
            AplicarColorCelda(row.Cells["comentarios"], Color.LightCyan);

            AplicarColorCelda(row.Cells["NC_MotivoDeRefactura"], Color.LightGray);
            AplicarColorCelda(row.Cells["NC_prestacionenglobante"], Color.LightGray);
            AplicarColorCelda(row.Cells["NC_DiasFacturados"], Color.LightGray);
            AplicarColorCelda(row.Cells["NC_ImporteDeRefactura"], Color.LightGray);
            AplicarColorCelda(row.Cells["NC_DebitoAceptado"], Color.LightGray);
            AplicarColorCelda(row.Cells["NC_MotivoDeDebito"], Color.LightGray);
            AplicarColorCelda(row.Cells["NC_ImporteDebitado"], Color.LightGray);

            var motivorefactura = row.Cells["nc_motivoderefactura"]?.Value?.ToString();
            var colorComentario = string.IsNullOrEmpty(motivorefactura) ? Color.Coral : Color.LightGray;
            AplicarColorCelda(row.Cells["nc_comentarios"], colorComentario, false);
        }
    }
}
