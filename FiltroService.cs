/*using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Debitos
{
    public class FiltroService
    {
        public static DataTable FiltrarPorValor(DataTable tabla, string columna, string valor)
        {
            var tablaFiltrada = tabla.Clone();
            var filas = tabla.Select($"{columna} = '{valor}'");
            foreach (var fila in filas)
                tablaFiltrada.ImportRow(fila);

            return tablaFiltrada;
        }

        public static DataTable CrearTablaUnica(DataTable origen, string columna, string encabezado = null)
        {
            var tablaUnica = new DataTable();
            tablaUnica.Columns.Add(columna);

            if (!string.IsNullOrWhiteSpace(encabezado))
                tablaUnica.Rows.Add(encabezado);

            var unicos = new HashSet<string>(
                origen.AsEnumerable()
                      .Where(f => !f.IsNull(columna))
                      .Select(f => f[columna]?.ToString())
                      .Where(v => !string.IsNullOrEmpty(v)));

            foreach (var valor in unicos)
                tablaUnica.Rows.Add(valor);

            return tablaUnica;
        }

        public static void RecargarCombo(ComboBox combo, DataTable dataSource, bool visible, string labelText = null, Label label = null)
        {
            combo.DataSource = dataSource;
            combo.Visible = visible;

            if (label != null && labelText != null)
            {
                label.Text = labelText;
                label.Visible = true;
            }
        }
    }
}
*/