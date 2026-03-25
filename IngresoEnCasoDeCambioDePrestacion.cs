using Npgsql;
using System.Data;

namespace Debitos
{
    public partial class IngresoEnCasoDeCambioDePrestacion : Form
    {
        public string tipoATransmitir { get; set; }
        public long idPrestacion { get; set; }
        public List<int> idPrestaciones { get; set; }
        public string tipoDocumentoTransmitido {  get; set; }

        public string codigoViejo;
        public string codigoNuevo = "";

        public bool cargaCompleta = false;

        public IngresoEnCasoDeCambioDePrestacion()
        {
            InitializeComponent();

            // Lógica para cargar datos en el comboBox1 (sin mostrar los valores aún)
            string cadenaConexion = "Host=172.16.13.219;Port=5432;Username=postgres;Password=postgres;Database=Debitos;";
            string comandoLlenadoFiltroPrestacion = "SELECT DISTINCT codigo FROM amb_liquidado ORDER BY codigo ASC";
            DataTable dataTablePrestacion = new DataTable();

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(cadenaConexion))
                {
                    connection.Open();  // Asegúrate de abrir la conexión antes de usarla

                    using (NpgsqlDataAdapter adapterCargandoSelectorPrestacion = new NpgsqlDataAdapter(comandoLlenadoFiltroPrestacion, connection))
                    {
                        // Llenar el DataTable con los datos obtenidos de la consulta
                        adapterCargandoSelectorPrestacion.Fill(dataTablePrestacion);

                        // Asignar los datos al filtro
                        comboBox1.DataSource = dataTablePrestacion;
                        comboBox1.DisplayMember = "codigo";  // Este debe coincidir con el nombre de la columna de la consulta
                        comboBox1.ValueMember = "codigo";    // Asignar el mismo valor si es necesario
                    }
                }  // No es necesario cerrar la conexión manualmente, using lo maneja
            }
            catch (Exception ex)
            {
                // Manejo de excepciones
                MessageBox.Show("Error al cargar los datos: " + ex.Message);
            }
        }

        private void IngresoEnCasoDeCambioDePrestacion_Load_1(object sender, EventArgs e)
        {

            lblCodigoViejo.Text = codigoViejo;

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cargaCompleta)
            {
                codigoNuevo = comboBox1.Text;
                if ((lblCodigoViejo.Text).Equals(codigoNuevo))
                {
                    btnGuardar.Visible = false;
                }
                else
                {
                    btnGuardar.Visible = true;
                }
            }
            else
            {
                cargaCompleta = true;
            }

        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            string comandoInsercion = "";
            switch (tipoDocumentoTransmitido)
            {
                case "FC":
                    comandoInsercion = "INSERT INTO temporalnc (codigonuevo, idprestacion) " +
                              "VALUES (@codigoNuevo, @idPrestacion);";

                    break;
                case "NC":
                    comandoInsercion = "INSERT INTO temporalnd (codigonuevo, idprestacion) " +
                              "VALUES (@codigoNuevo, @idPrestacion);";

                    break;
                case "ND":
                    comandoInsercion = "INSERT INTO temporalnc (codigonuevo, idprestacion) " +
                              "VALUES (@codigoNuevo, @idPrestacion);";

                    break;
            }

            string cadenaConexion = "Host=172.16.13.219;Port=5432;Username=postgres;Password=postgres;Database=Debitos;";
            
            foreach(int prestacion in idPrestaciones)
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(cadenaConexion))
                    {
                        connection.Open();

                        using (NpgsqlCommand command = new NpgsqlCommand(comandoInsercion, connection))
                        {
                            command.Parameters.AddWithValue("@codigoNuevo", codigoNuevo);
                            command.Parameters.AddWithValue("@idPrestacion", prestacion);

                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al guardar en la base de datos: " + ex.Message);
                }
            }
            this.Close();
        }

    }
}
