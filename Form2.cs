namespace Debitos
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();

            // Crear una instancia de UserControl1
            UserControl1 userControl1 = new UserControl1();

            // Ajustar el tamaño del UserControl para que ocupe todo el formulario
            userControl1.Dock = DockStyle.Fill;

            // Agregar el UserControl al formulario
            this.Controls.Add(userControl1);
        }

    }
}
