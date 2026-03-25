/*using System.Data;
using System.Windows.Forms;

namespace Debitos
{
    public class FiltroManager
    {
        public List<DataTable> FiltrosPaciente { get; private set; } = new List<DataTable>();
        public List<DataTable> FiltrosMedico { get; private set; } = new List<DataTable>();
        public List<DataTable> FiltrosPrestacion { get; private set; } = new List<DataTable>();
        public List<DataTable> FiltrosInternacion { get; private set; } = new List<DataTable>();

        public bool ListaPacienteCargada { get; set; } = false;
        public bool ListaMedicoCargada { get; set; } = false;
        public bool ListaPrestacionCargada { get; set; } = false;
        public bool ListaInternacionCargada { get; set; } = false;

        public void RecargarFiltroPaciente(DataTable origen, ComboBox combo, Label label = null)
        {
            var dtUnico = FiltroService.CrearTablaUnica(origen, "paciente", "Paciente");
            FiltrosPaciente.Add(dtUnico);
            ListaPacienteCargada = true;
            FiltroService.RecargarCombo(combo, dtUnico, true, "Paciente", label);
        }

        public void RecargarFiltroMedico(DataTable origen, ComboBox combo, Label label = null)
        {
            var dtUnico = FiltroService.CrearTablaUnica(origen, "medico", "Profesional");
            FiltrosMedico.Add(dtUnico);
            ListaMedicoCargada = true;
            FiltroService.RecargarCombo(combo, dtUnico, true, "Profesional", label);
        }

        public void RecargarFiltroPrestacion(DataTable origen, ComboBox combo, Label label = null)
        {
            var dtUnico = FiltroService.CrearTablaUnica(origen, "codigo", "Prestación");
            FiltrosPrestacion.Add(dtUnico);
            ListaPrestacionCargada = true;
            FiltroService.RecargarCombo(combo, dtUnico, true, "Prestación", label);
        }

        public void RecargarFiltroInternacion(DataTable origen, ComboBox combo, Label label = null)
        {
            var dtUnico = FiltroService.CrearTablaUnica(origen, "Nro_Int", "nro_internacion");
            FiltrosInternacion.Add(dtUnico);
            ListaInternacionCargada = true;
            FiltroService.RecargarCombo(combo, dtUnico, true, "nro_internacion", label);
        }

        public void ResetearFiltros()
        {
            FiltrosPaciente.Clear();
            FiltrosMedico.Clear();
            FiltrosPrestacion.Clear();
            FiltrosInternacion.Clear();

            ListaPacienteCargada = false;
            ListaMedicoCargada = false;
            ListaPrestacionCargada = false;
            ListaInternacionCargada = false;
        }
    }
}
*/