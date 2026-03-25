using Npgsql;

namespace Debitos
{
    public partial class UserControl1 : UserControl
    {
        string usuario, clave;

        bool usIng = false;
        bool clIng = false;

        public UserControl1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((usIng) && (clIng))
            {
                string connectionString = "Host=172.16.13.219;Port=5432;Username=postgres;Password=postgres;Database=Debitos;";

                string query = "SELECT * FROM Usuarios WHERE usuario = @usuario";

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@usuario", usuario);

                            using (NpgsqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    string claveEnBaseDeDatos = reader["clave"].ToString();

                                    if (clave == claveEnBaseDeDatos)
                                    {
                                        MessageBox.Show("Inicio de sesión correcto", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                        Form1 form1 = new Form1(usuario);
                                        form1.Show();
                                        return;
                                    }
                                    else
                                    {
                                        MessageBox.Show("Contraseña incorrecta", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        txtClave.Text = "";
                                        clIng = false;
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Usuario incorrecto", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    usIng = false;
                                    clIng = false;
                                    txtUsuario.Text = "";
                                    txtClave.Text = "";
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al conectarse a la base de datos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        if (connection.State == System.Data.ConnectionState.Open)
                        {
                            connection.Close();
                        }
                    }
                }
            }
            else
            {
                if (!clIng)
                {
                    lblClaveIncorrecta.Text = "Debe ingresar la clave.";
                }
                else
                {
                    lblClaveIncorrecta.Text = "";
                }
                if (!usIng)
                {
                    lblUsuarioIncorrecto.Text = "Debe ingresar el usuario.";
                }
                else
                {
                    lblUsuarioIncorrecto.Text = "";
                }
            }
        }

        private void txtUsuario_TextChanged(object sender, EventArgs e)
        {
            if (txtUsuario.Text.Length > 0)
            {
                usIng = true;
                usuario = txtUsuario.Text;
            }
            else
            {
                usIng = false;
                usuario = "";
            }
        }

        private void txtClave_TextChanged(object sender, EventArgs e)
        {
            if (txtClave.Text.Length > 0)
            {
                clIng = true;
                clave = txtClave.Text;
            }
            else
            {
                clIng = false;
                clave = "";
            }
        }
    }
}
