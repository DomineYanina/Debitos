using ClosedXML.Excel;
using Debitos.Models;
using Debitos.Presenters;
using Debitos.Repositories;
using Debitos.Views;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using System.Security.Cryptography;

namespace Debitos;

public partial class Form1 : Form, IPrestacionesView // <-- Acá agregamos la interfaz
{
    private PrestacionesPresenter _presenter; // <-- El presentador
    private DebitosRepository _repository;
    public event EventHandler BuscarDocumentoEvent; // <-- El evento que pide la interfaz

    // Implementación de las propiedades para manejar la UI desde el Presentador
    public DataTable DatosGrilla
    {
        get => (DataTable)bindingSource.DataSource;
        set
        {
            bindingSource.DataSource = value;
            dataGridView1.DataSource = bindingSource;
            lblCantidadDeRegistrosFiltrados.Text = "Cantidad de registros filtrados: " + value?.Rows.Count;
            lblCantidadDeRegistrosFiltrados.Visible = true;

            AplicarFormatoYVisibilidadPorTipoFactura(value?.Rows.Count ?? 0);

            // NUEVO: Generar los combos automáticamente desde los datos recibidos
            if (value != null && value.Rows.Count > 0)
            {
                InicializarFiltrosDesdeMemoria(value);

                // Ponemos visibles los filtros
                SetControlesVisibles(true);
            }
        }
    }

    public bool BotonBuscarVisible
    {
        get => btnBuscar.Visible;
        set => btnBuscar.Visible = value;
    }

    // Implementación explícita de la interfaz para no romper la lógica de tus parámetros 'out'
    string IPrestacionesView.FacturaTipo => this.FacturaTipo;
    string IPrestacionesView.FacturaLetra => this.FacturaLetra;
    int IPrestacionesView.FacturaPuntoDeVenta => this.FacturaPuntoDeVenta;
    int IPrestacionesView.FacturaNumero => this.FacturaNumero;

    public void MostrarMensaje(string mensaje)
    {
        MessageBox.Show(mensaje);
    }

    public void MostrarCargando(bool mostrar)
    {
        this.UseWaitCursor = mostrar;
        btnBuscar.Enabled = !mostrar;
    }

    private BindingSource bindingSource = new BindingSource();

    private ToolTip toolTip;
    private ToolTip tooltip1;

    public bool controlFormatting = false;

    public bool cargaCompletada = true;
    public bool cargaACompletar = true;

    protected string TipoRegistroFiltrado = "";
    protected string GrupoPrestacion = "";
    protected string Paciente = "";
    protected string Profesional = "";
    protected string Prestacion = "";
    protected string FacturaTipo = "";
    public string FacturaLetra = "";
    public int FacturaPuntoDeVenta = 0;
    public int FacturaNumero = 0;

    protected string pacienteFiltro, prestacionFiltro, profesionalFiltro, grupoPrestacionFiltro, moduloFiltro = "";

    public string tipoDocumento = "";
    public string letraDocumento = "";

    public string tipoATransmitir = "";
    public string letraATransmitir = "";
    public string numeroATransmitir = "";
    public string ptoVtaATransmitir = "";
    public DateTime fechaATransmitir;
    public decimal importeDeRefacturaATransmitir = 0;

    public int numeroDocumento = 0;
    public int puntoDeVentaDocumento = 0;
    public DateTime fechaDocumento = DateTime.Now;

    public bool DocumentoTipoSeleccionado = false;
    public bool DocumentoLetraSeleccionada = false;
    public bool DocumentoPuntoDeVentaSeleccionado = false;
    public bool DocumentoNumeroSeleccionado = false;
    public bool DocumentoFechaSeleccionada = false;
    public bool buscando = false;

    protected bool FacturaTipoSeleccionado = false;
    protected bool FacturaNumeroSeleccionado = false;
    protected bool FacturaLetraSeleccionado = false;
    protected bool FacturaPuntoDeVentaSeleccionado = false;
    protected bool usuarioSeleccionoFecha = false;
    protected bool cargaLista = false;
    protected bool cargarSoloFiltroMotivoDebito = false;
    protected bool cargaPrimeraVez = true;
    protected bool algunFiltro = false;
    protected bool debitoIndividual = false;

    protected DataTable auxiliar = new DataTable();
    protected DataTable filtros = new DataTable();
    protected DataTable tablaAMostrar = new DataTable();
    protected DataTable prueba = new DataTable();
    protected DataTable dataTablePaciente = new DataTable();
    protected DataTable aUsarParaLimpiarFiltroAnterior = new DataTable();
    protected DataTable tablaCompletaSinFiltros = new DataTable();
    protected DataTable filtroPacienteSinFiltros = new DataTable();
    protected DataTable filtroFechaSinFiltros = new DataTable();
    protected DataTable filtroMedicoSinFiltros = new DataTable();
    protected DataTable filtroPrestacionSinFiltros = new DataTable();
    protected DataTable filtroModuloSinFiltros = new DataTable();
    protected DataTable filtroNumeroDeInternacionSinFiltros = new DataTable();
    protected DataTable filtroNumeroDeInternacionTabla = new DataTable();
    protected DataTable auxFiltros = new DataTable();

    private DataTable filtroPacienteOriginal;
    private DataTable filtroPrestacionOriginal;
    private DataTable filtroMedicoOriginal;
    private DataTable filtroModuloOriginal;
    private DataTable filtroNumeroDeInternacionOriginal;
    private DataTable filtroFechaOriginal;

    private DataTable tablaSinFiltro;

    public List<string> ordenFiltros = new List<string>();
    public List<DataTable> tablasFiltradas = new List<DataTable>();

    public List<int> listaPrestacionesYaExistentes = new List<int>();

    private string condicionesFiltro = "";
    public string comandoSeleccionAmbLiquidado1 = "";
    public string comandoSeleccionTipoDeRegistro = "";
    public string comandoBusquedaDeGuardadoParcialEnTabla = @"SELECT * FROM cargaincompleta where tipodocumento = @tipodocumento AND letra = @letra AND ptovta = @ptovta AND numero = @numero;";

    public string comandoSeleccionAmbLiquidado = "SELECT al.id, al.paciente, al.medico, al.fecha, al.codigo, al.descripcion, al.cantidad, al.total_neto, al.coseguro, al.total, al.cob_factura_tipo, al.cob_factura_letra, al.cob_factura_ptoventa, al.cob_factura_numero, al.porcentaje_especialista, al.porcentaje_ayudante1, al.porcentaje_anestesista, al.porcentaje_gastos, nc.motivodedebito, nc.importedebitado, nc.debitoaceptado FROM amb_liquidado al LEFT JOIN notadecredito nc ON al.id = nc.id_prestacion";
    
    protected bool primerFiltroFecha = false;
    protected bool cargaListaPaciente = false;
    protected bool cargaListaModulo = false;
    protected bool cargaListaProfesional = false;
    protected bool cargaListaPrestacion = false;
    protected bool cargaListaFecha = true;
    protected bool cargaListaFacturaTipo = false;
    protected bool cargaListaFacturaLetra = false;
    protected bool cargaListaFacturaPuntoDeVenta = false;
    protected bool cargaListaFacturaNumero = false;
    protected bool cargaListaNumeroDeInternacion = false;

    public string connectionString = "Host=172.16.13.219;Port=5432;Username=postgres;Password=postgres;Database=Debitos;";
    protected string cargaMotivoDeDebitos = "SELECT DISTINCT descripcion from motivodeldebito ORDER BY descripcion ASC";
    

    protected IngresoInformacionNotaDeCredito ingresoInformacionNotaDeCredito;
    protected IngresoInformacionNotaDeDebito ingresoInformacionNotaDeDebito;
    protected VerHistorialDelDocumento verHistorialDelDocumento;

    private string usuario;

    private bool documentoEncontrado = false;
    private bool cargaParcialPreviamenteCreada = false;

    private List<DataTable> tablasFiltros = new List<DataTable>();
    private List<DataTable> tablasFiltrosPaciente = new List<DataTable>();
    private List<DataTable> tablasFiltrosFecha = new List<DataTable>();
    private List<DataTable> tablasFiltrosPrestacion = new List<DataTable>();
    private List<DataTable> tablasFiltrosModulo = new List<DataTable>();
    private List<DataTable> tablasFiltrosMedico = new List<DataTable>();
    private List<DataTable> tablasFiltrosNumeroDeInternacion = new List<DataTable>();

    private List<(int idPrestacion, object? motivoRefactura, object? motivoDebito, double? importeRefactura, double? importeDebito, string? comentarios, bool debitoAceptado, object? diasFacturados, string? prestacionEnglobante, string? codigo)> listaValoresParaBorradoDeFiltros = new List<(int, object?, object?, double?, double?, string?, bool, object?, string?, string?)>();

    private List<(int idPrestacion, object motivoRefactura, double importeRefactura, string? comentarios, int idNotaDeCredito, string? codigo)> listaValoresParaBorradoDeFiltrosNC = new List<(int, object, double, string?, int, string?)>();

    private List<(int idPrestacion, object? motivoRefactura, object? motivoDebito, double? importeRefactura, double? importeDebito, string? comentarios, bool debitoAceptado, object? diasFacturados, string? prestacionEnglobante, int? idNotaDeDebito)> listaValoresParaBorradoDeFiltrosND = new List<(int, object?, object?, double?, double?, string?, bool, object?, string?, int?)>();

    // Estructura para almacenar los datos antes de ordenar
    private List<(int idPrestacion, object motivoRefactura, object motivoDebito, double importeRefactura, double importeDebito)> listaValores = new List<(int, object, object, double, double)>();

    // Estructura para almacenar los datos antes de ordenar
    private List<(int idPrestacion, object motivoRefactura, object motivoDebito, double importeRefactura, double importeDebito)> listaFiltrada = new List<(int, object, object, double, double)>();

    // Estructura para almacenar los datos antes de ordenar
    private List<(int idPrestacion, object motivoRefactura, object motivoDebito)> valoresParaFiltros = new List<(int, object, object)>();

    // Estructura para almacenar los datos antes de ordenar
    private List<(int idPrestacion, object motivoRefactura, object motivoDebito)> valoresOriginales = new List<(int, object, object)>();

    // Estructura para almacenar los datos antes de ordenar
    private List<(int idPrestacion, object motivoRefactura)> listaValoresNC = new List<(int, object)>();

    // Estructura para almacenar los datos antes de ordenar
    private List<(int idPrestacion, object motivoRefactura)> listaFiltradaNC = new List<(int, object)>();

    // Estructura para almacenar los datos antes de ordenar
    private List<(int idPrestacion, object motivoRefactura)> valoresParaFiltrosNC = new List<(int, object)>();

    // Estructura para almacenar los datos antes de ordenar
    private List<(int idPrestacion, object motivoRefactura)> valoresOriginalesNC = new List<(int, object)>();

    // Estructura para almacenar los datos antes de ordenar
    private List<(int idPrestacion, object importeRefactura)> listaValoresParaImporteDeRefactura = new List<(int, object)>();

    // Estructura para almacenar los datos antes de ordenar
    private List<(int idPrestacion, object importeRefactura)> listaValoresParaImporteDeDebito = new List<(int, object)>();
    public Form1(String _usuario)
    {
        InitializeComponent();
        usuario = _usuario;

        // Initialize non-nullable fields to default values to satisfy CS8618  
        pacienteFiltro = string.Empty;
        prestacionFiltro = string.Empty;
        profesionalFiltro = string.Empty;
        grupoPrestacionFiltro = string.Empty;
        ingresoInformacionNotaDeCredito = new IngresoInformacionNotaDeCredito(false, 0, string.Empty, 0, string.Empty);
        ingresoInformacionNotaDeDebito = new IngresoInformacionNotaDeDebito(false, 0, string.Empty, 0, string.Empty);
        verHistorialDelDocumento = new VerHistorialDelDocumento(0, string.Empty, 0, string.Empty);

        // UI element visibility settings  
        SetControlesVisibles(false);

        cargaListaPaciente = true;
        cargaListaModulo = true;
        cargaListaPrestacion = true;
        cargaListaProfesional = true;
        cargaListaFacturaTipo = true;
        cargaListaFacturaLetra = true;
        cargaListaFacturaPuntoDeVenta = true;
        cargaListaFacturaNumero = true;

        cargaLista = false;
        cargarSoloFiltroMotivoDebito = false;

        toolTip = new ToolTip();
        tooltip1 = new ToolTip();

        tooltip1.IsBalloon = true;
        tooltip1.AutoPopDelay = 5000;
        tooltip1.BackColor = System.Drawing.Color.Yellow;
        tooltip1.ForeColor = System.Drawing.Color.Black;

        panel1.Visible = false;
        btnBorrarCelda.Visible = false;
        btnBorrarFiltros.Visible = false;
        lblModulo.Visible = false;

        dataGridView1.DoubleBuffered(true);

        // (Al final del constructor)
        string connectionString = "Host=172.16.13.219;Port=5432;Username=postgres;Password=postgres;Database=Debitos;";
        _repository = new DebitosRepository(connectionString); // <-- Guardamos en la variable global
        _presenter = new PrestacionesPresenter(this, _repository);

    }

    private void checkPrestacionesSinRefactura_CheckedChanged(object sender, EventArgs e)
    {
        GuardarValoresAntesDeDeshacerFiltro();
        AplicarFiltrosActivos();
    }

    private void checkPrestacionesSinDebito_CheckedChanged(object sender, EventArgs e)
    {
        GuardarValoresAntesDeDeshacerFiltro();
        AplicarFiltrosActivos();
    }

    private void soloPrestacionesValorizadas_CheckedChanged(object sender, EventArgs e)
    {
        GuardarValoresAntesDeDeshacerFiltro();
        AplicarFiltrosActivos();
    }

    public void resetearVariables()
    {
        cargaCompletada = true;
        cargaACompletar = true;

        GrupoPrestacion = "";
        Paciente = "";
        Profesional = "";
        Prestacion = "";
        FacturaTipo = "";
        FacturaLetra = "";
        FacturaPuntoDeVenta = 0;
        FacturaNumero = 0;

        pacienteFiltro = "";
        prestacionFiltro = "";
        profesionalFiltro = "";
        grupoPrestacionFiltro = "";
        moduloFiltro = "";

        tipoDocumento = "";
        letraDocumento = "";

        tipoATransmitir = "";
        letraATransmitir = "";
        numeroATransmitir = "";
        ptoVtaATransmitir = "";
        importeDeRefacturaATransmitir = 0;

        numeroDocumento = 0;
        puntoDeVentaDocumento = 0;
        fechaDocumento = DateTime.Now;

        DocumentoTipoSeleccionado = false;
        DocumentoLetraSeleccionada = false;
        DocumentoPuntoDeVentaSeleccionado = false;
        DocumentoNumeroSeleccionado = false;
        DocumentoFechaSeleccionada = false;

        FacturaTipoSeleccionado = false;
        FacturaNumeroSeleccionado = false;
        FacturaLetraSeleccionado = false;
        FacturaPuntoDeVentaSeleccionado = false;
        usuarioSeleccionoFecha = false;
        cargaLista = false;
        cargarSoloFiltroMotivoDebito = false;
        cargaPrimeraVez = true;

        btnBorrarImporteDebito.Visible = false;
        btnBorrarImporteRefactura.Visible = false;

        auxiliar.Clear();
        filtros.Clear();
        tablaAMostrar.Clear();
        prueba.Clear();
        dataTablePaciente.Clear();
        aUsarParaLimpiarFiltroAnterior.Clear();
        tablaCompletaSinFiltros.Clear();
        filtroPacienteSinFiltros.Clear();
        filtroFechaSinFiltros.Clear();
        filtroMedicoSinFiltros.Clear();
        filtroPrestacionSinFiltros.Clear();
        filtroModuloSinFiltros.Clear();
        filtroNumeroDeInternacionSinFiltros.Clear();

        tablasFiltradas.Clear();

        condicionesFiltro = "";
        comandoSeleccionAmbLiquidado1 = "";
        primerFiltroFecha = false;
        cargaListaPaciente = false;
        cargaListaModulo = false;
        cargaListaProfesional = false;
        cargaListaPrestacion = false;
        cargaListaFecha = true;
        cargaListaFacturaTipo = false;
        cargaListaFacturaLetra = false;
        cargaListaFacturaPuntoDeVenta = false;
        cargaListaFacturaNumero = false;

        documentoEncontrado = false;
        cargaParcialPreviamenteCreada = false;

        tablasFiltros.Clear();
        tablasFiltrosPaciente.Clear();
        tablasFiltrosFecha.Clear();
        tablasFiltrosPrestacion.Clear();
        tablasFiltrosModulo.Clear();
        tablasFiltrosMedico.Clear();
        tablasFiltrosNumeroDeInternacion.Clear();

        // Estructura para almacenar los datos antes de ordenar
        listaValores = new List<(int, object, object, double, double)>();
    }

    private void btnBorrarFiltros_Click(object sender, EventArgs e)
    {
        dataGridView1.SuspendLayout();
        RestaurarValoresPorTipoFactura();
        RestaurarUIFiltros();
        algunFiltro = false;

        // LA SOLUCIÓN: Limpiamos la máscara, NO el DataSource
        bindingSource.Filter = string.Empty;

        // Desmarcamos los checkboxes silenciando los eventos
        checkPrestacionesSinDebito.CheckedChanged -= checkPrestacionesSinDebito_CheckedChanged;
        checkPrestacionesSinRefactura.CheckedChanged -= checkPrestacionesSinRefactura_CheckedChanged;
        soloPrestacionesValorizadas.CheckedChanged -= soloPrestacionesValorizadas_CheckedChanged;

        checkPrestacionesSinDebito.Checked = false;
        checkPrestacionesSinRefactura.Checked = false;
        soloPrestacionesValorizadas.Checked = false;

        checkPrestacionesSinDebito.CheckedChanged += checkPrestacionesSinDebito_CheckedChanged;
        checkPrestacionesSinRefactura.CheckedChanged += checkPrestacionesSinRefactura_CheckedChanged;
        soloPrestacionesValorizadas.CheckedChanged += soloPrestacionesValorizadas_CheckedChanged;

        // Restaurar los DataSource originales
        cargaListaPaciente = true;
        filtroPaciente.DataSource = filtroPacienteOriginal;
        cargaListaPrestacion = true;
        filtroPrestacion.DataSource = filtroPrestacionOriginal;
        cargaListaProfesional = true;
        filtroProfesional.DataSource = filtroMedicoOriginal;
        cargaListaModulo = true;
        filtroModulo.DataSource = filtroModuloOriginal;
        cargaListaNumeroDeInternacion = true;
        filtroNumeroDeInternacion.DataSource = filtroNumeroDeInternacionOriginal;
        cargaListaFecha = true;
        comboFiltroFecha.DataSource = filtroFechaOriginal;

        // Reset de flags
        ordenFiltros.Clear();
        cargaListaPaciente = false;
        cargaListaModulo = false;
        cargaListaProfesional = false;
        cargaListaPrestacion = false;
        cargaListaFecha = false;
        cargaPrimeraVez = true;

        lblFecSel.Text = "Fecha";
        lblProfSel.Text = "Profesional";
        lblNumeroDeInternacionSel.Text = "N° de internación";
        lblPacSel.Text = "Paciente";
        lblPrestSel.Text = "Prestación";
        comboFiltroFecha.Visible = true;

        dataGridView1.Refresh();
        AplicarFormatoYVisibilidadPorTipoFactura(bindingSource.Count);
        ActualizarCantidadDeRegistrosFiltrados();
        restaurarValoresPreviosAFiltro();

        btnBorrarFiltros.Visible = false;

        if (TipoRegistroFiltrado == "Internados")
        {
            filtroNumeroDeInternacion.Visible = true;
            filtroModulo.Visible = true;
            label1.Visible = true;
            lblModulo.Visible = true;
            lblNumeroDeInternacionSel.Visible = true;
        }
        else
        {
            filtroNumeroDeInternacion.Visible = false;
            filtroModulo.Visible = false;
            label1.Visible = false;
            lblModulo.Visible = false;
            lblNumeroDeInternacionSel.Visible = false;
        }

        contarFilasConDebitoAceptado();
        dataGridView1.ResumeLayout();
    }

    private void RestaurarValoresPorTipoFactura()
    {
        switch (FacturaTipo)
        {
            case "FC":
                GuardarValoresAntesDeDeshacerFiltro();
                break;
            case "NC":
                GuardarValoresAntesDeDeshacerFiltroNC();
                break;
            case "ND":
                GuardarValoresAntesDeDeshacerFiltro();
                break;
        }
    }

    private void InicializarFiltrosDesdeMemoria(DataTable datos)
    {
        ActualizarFiltrosDisponibles(datos);

        filtroPacienteOriginal = (DataTable)filtroPaciente.DataSource;
        filtroPrestacionOriginal = (DataTable)filtroPrestacion.DataSource;
        filtroMedicoOriginal = (DataTable)filtroProfesional.DataSource;
        filtroModuloOriginal = (DataTable)filtroModulo.DataSource;
        filtroNumeroDeInternacionOriginal = (DataTable)filtroNumeroDeInternacion.DataSource;
        filtroFechaOriginal = comboFiltroFecha.DataSource as DataTable;

        tablaCompletaSinFiltros = datos.Copy();
        tablaSinFiltro = datos.Copy();
        auxFiltros = datos.Copy();

        filtroPacienteSinFiltros = filtroPacienteOriginal;
        filtroMedicoSinFiltros = filtroMedicoOriginal;
        filtroPrestacionSinFiltros = filtroPrestacionOriginal;
        filtroModuloSinFiltros = filtroModuloOriginal;
        filtroNumeroDeInternacionSinFiltros = filtroNumeroDeInternacionOriginal;

        // EL DESBLOQUEO CRÍTICO: Esto vuelve a habilitar los clics del usuario
        cargaListaPaciente = false;
        cargaListaProfesional = false;
        cargaListaPrestacion = false;
        cargaListaModulo = false;
        cargaListaNumeroDeInternacion = false;
        cargaListaFecha = false;
    }

    private void RestaurarUIFiltros()
    {
        btnBorrarImporteDebito.Visible = true;
        btnBorrarImporteRefactura.Visible = true;

        // Mostrar nuevamente los filtros
        filtroPaciente.Visible = true;
        filtroPrestacion.Visible = true;
        filtroProfesional.Visible = true;
        filtroModulo.Visible = true;
        filtroNumeroDeInternacion.Visible = TipoRegistroFiltrado == "Internados";

        // Ocultar etiquetas de filtros seleccionados
        lblPacSel.Visible = false;
        lblPrestSel.Visible = false;
        lblProfSel.Visible = false;
        lblModulo.Visible = false;
        lblNumeroDeInternacionSel.Visible = false;

        // Resetear flags
        ordenFiltros.Clear();
        cargaListaPaciente = false;
        cargaListaModulo = false;
        cargaListaFecha = false;
        cargaListaPrestacion = false;
        cargaListaProfesional = false;
        cargaPrimeraVez = true;

        lblModulo.TextAlign = ContentAlignment.TopLeft;
        lblModulo.Text = "Módulo";
    }

    private void RecargarFiltrosYDatos()
    {
        tablaAMostrar = tablasFiltros[0];
        dataGridView1.DataSource = tablaAMostrar;
        filtroPaciente.DataSource = filtroPacienteSinFiltros;
        filtroPrestacion.DataSource = filtroPrestacionSinFiltros;
        filtroProfesional.DataSource = filtroMedicoSinFiltros;
        filtroModulo.DataSource = filtroModuloSinFiltros;
        filtroNumeroDeInternacion.DataSource = filtroNumeroDeInternacionSinFiltros;
        tablasFiltrosMedico.Clear();
        tablasFiltrosPaciente.Clear();
        tablasFiltrosPrestacion.Clear();
        tablasFiltrosModulo.Clear();
        tablasFiltrosMedico.Add(filtroMedicoSinFiltros);
        tablasFiltrosPaciente.Add(filtroPacienteSinFiltros);
        tablasFiltrosPrestacion.Add(filtroPrestacionSinFiltros);
        tablasFiltrosModulo.Add(filtroModuloSinFiltros);
        if (TipoRegistroFiltrado == "Internados")
        {
            filtroNumeroDeInternacion.Visible = true;
        }
        dataGridView1.Refresh();
    }

    private void RestaurarColoresPorTipoFactura()
    {
        switch (FacturaTipo)
        {
            case "FC":
                restaurarValoresAlBorrarFiltros();
                colorearColumnasFC();
                break;
            case "NC":
                restaurarValoresAlBorrarFiltrosNC();
                colorearColumnasNC();
                break;
            case "ND":
                restaurarValoresAlBorrarFiltros();
                colorearColumnasND();
                break;
        }
    }

    private void ActualizarCantidadDeRegistrosFiltrados()
    {
        lblCantidadDeRegistrosFiltrados.Text = "Cantidad de registros filtrados: " + bindingSource.Count;
    }

    private void actualizarCantidadDeDebitosAceptados()
    {
        int cantidadDeDebitosAceptados = 0;
        foreach (DataGridViewRow row in dataGridView1.Rows)
        {
            bool debitoAceptado = row.Cells["NC_DebitoAceptado"].Value != DBNull.Value && Convert.ToBoolean(row.Cells["NC_DebitoAceptado"].Value);
            if (debitoAceptado)
            {
                cantidadDeDebitosAceptados++;
            }
        }
        lblCantidadDeRegistrosConDebitoAceptado.Text = "Cantidad de débitos aceptados: " + cantidadDeDebitosAceptados;
    }

    private void restaurarValoresAlBorrarFiltrosNC()
    {
        foreach (DataGridViewRow row in dataGridView1.Rows)
        {
            if (row.Cells["id"].Value != null)
            {
                int idPrestacion = Convert.ToInt32(row.Cells["id"].Value);

                // Buscar el valor correspondiente en la lista almacenada
                var item = listaValoresParaBorradoDeFiltrosNC.FirstOrDefault(x => x.idPrestacion == idPrestacion);

                // Restaurar el valor si existe en la lista almacenada
                if (item.idPrestacion == idPrestacion)
                {
                    cargaPrimeraVez = true;
                    row.Cells["NC_MotivoDeRefactura"].Value = item.motivoRefactura;
                    cargaPrimeraVez = true;

                    if (item.motivoRefactura == "No aplica")
                    {
                        row.Cells["NC_ImporteDeRefactura"].Value = DBNull.Value;
                    }
                    else
                    {
                        row.Cells["NC_ImporteDeRefactura"].Value = item.importeRefactura;
                    }

                    cargaPrimeraVez = true;
                    row.Cells["NC_Comentarios"].Value = item.comentarios;
                }
            }
        }
    }

    private void restaurarValoresAlBorrarFiltros()
    {
        foreach (DataGridViewRow row in dataGridView1.Rows)
        {
            if (row.Cells["id_prestacion"].Value != null)
            {
                int idPrestacion = Convert.ToInt32(row.Cells["id_prestacion"].Value);

                // Buscar el valor correspondiente en la lista almacenada
                var item = listaValoresParaBorradoDeFiltros.FirstOrDefault(x => x.idPrestacion == idPrestacion);

                // Restaurar el valor si existe en la lista almacenada
                if (item.idPrestacion == idPrestacion)
                {
                    cargaPrimeraVez = true;
                    row.Cells["NC_MotivoDeRefactura"].Value = item.motivoRefactura;
                    cargaPrimeraVez = true;
                    row.Cells["NC_MotivoDeDebito"].Value = item.motivoDebito;
                    cargaPrimeraVez = true;

                    if (item.motivoRefactura?.ToString() == "No aplica")
                    {
                        row.Cells["NC_ImporteDeRefactura"].Value = DBNull.Value;
                    }
                    else
                    {
                        row.Cells["NC_ImporteDeRefactura"].Value = item.importeRefactura;
                    }

                    cargaPrimeraVez = true;
                    row.Cells["NC_ImporteDebitado"].Value = item.importeDebito;
                    cargaPrimeraVez = true;
                    row.Cells["NC_Comentarios"].Value = item.comentarios;
                    cargaPrimeraVez = true;
                    row.Cells["NC_DebitoAceptado"].Value = item.debitoAceptado;
                }
            }
        }
    }

    private void filtroModulo_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cargaListaModulo || filtroModulo.SelectedIndex <= 0) return;
        GuardarValoresAntesDeDeshacerFiltro();
        lblModulo.Text = "Módulo: " + filtroModulo.Text;
        lblModulo.TextAlign = ContentAlignment.TopRight;
        lblModulo.Visible = true;
        filtroModulo.Visible = false;
        AplicarFiltrosActivos();
    }

    private void filtroPaciente_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cargaListaPaciente || filtroPaciente.SelectedIndex <= 0) return;
        GuardarValoresAntesDeDeshacerFiltro();
        lblPacSel.Text = "Paciente: " + filtroPaciente.Text;
        lblPacSel.Visible = true;
        filtroPaciente.Visible = false;
        AplicarFiltrosActivos();
    }

    private void AplicarFiltrosActivos()
    {
        if (bindingSource.DataSource == null) return;

        List<string> filtros = new List<string>();
        DataTable dt = (DataTable)bindingSource.DataSource;

        if (lblPacSel.Visible)
            filtros.Add($"paciente = '{lblPacSel.Text.Replace("Paciente: ", "").Trim().Replace("'", "''")}'");

        if (lblProfSel.Visible)
            filtros.Add($"medico = '{lblProfSel.Text.Replace("Profesional: ", "").Trim().Replace("'", "''")}'");

        if (lblPrestSel.Visible)
            filtros.Add($"codigo = '{lblPrestSel.Text.Replace("Prestación: ", "").Trim().Replace("'", "''")}'");

        if (lblModulo.Visible && lblModulo.Text.StartsWith("Módulo:"))
        {
            string mod = lblModulo.Text.Replace("Módulo: ", "").Trim().Replace("'", "''");
            if (dt.Columns.Contains("grupomodulo"))
                filtros.Add($"(grupomodulo = '{mod}' OR modulo = '{mod}')");
            else
                filtros.Add($"modulo = '{mod}'");
        }

        if (lblNumeroDeInternacionSel.Visible)
        {
            string nro = lblNumeroDeInternacionSel.Text.Replace("Número de internación: ", "").Trim().Replace("'", "''");
            string colNro = dt.Columns.Contains("nro_internacion") ? "nro_internacion" : "nro_int";
            filtros.Add($"{colNro} = '{nro}'");
        }

        if (lblFecSel.Visible)
        {
            string fec = lblFecSel.Text.Replace("Fecha: ", "").Trim();
            if (DateTime.TryParse(fec, out DateTime dateValue))
                filtros.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, "fecha = #{0:MM/dd/yyyy}#", dateValue));
        }

        // --- CHECKBOXES INTEGRADAS ---
        if (checkPrestacionesSinRefactura.Checked)
        {
            if (FacturaTipo == "FC" || FacturaTipo == "ND")
                filtros.Add("(nc_motivoderefactura IS NULL OR nc_motivoderefactura = '')");
            else if (FacturaTipo == "NC")
                filtros.Add("(nd_motivoderefactura IS NULL OR nd_motivoderefactura = '')");
        }

        if (checkPrestacionesSinDebito.Checked)
            filtros.Add("(nc_motivodedebito IS NULL OR nc_motivodedebito = '')");

        if (soloPrestacionesValorizadas.Checked)
            filtros.Add("total <> 0");
        // -----------------------------

        string filtroFinal = string.Join(" AND ", filtros);

        try
        {
            bindingSource.Filter = filtroFinal;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error aplicando el filtro: " + ex.Message);
            return;
        }

        algunFiltro = filtros.Count > 0;
        btnBorrarFiltros.Visible = algunFiltro;
        AplicarFormatoYVisibilidadPorTipoFactura(bindingSource.Count);
        ActualizarCantidadDeRegistrosFiltrados();
        contarFilasConDebitoAceptado();

        DataView vistaFiltrada = new DataView(dt);
        vistaFiltrada.RowFilter = filtroFinal;
        ActualizarFiltrosDisponibles(vistaFiltrada.ToTable());

        cargaListaPaciente = false;
        cargaListaProfesional = false;
        cargaListaPrestacion = false;
        cargaListaModulo = false;
        cargaListaNumeroDeInternacion = false;
        cargaListaFecha = false;
    }

    private void AplicarFormatoYVisibilidadPorTipoFactura(int cantidadFilas)
    {
        switch (FacturaTipo)
        {
            case "FC":
                colorearColumnasFC();
                filtroMotivoDebito.Visible = true;
                checkMotivoDebito.Visible = true;
                label6.Visible = true;
                break;
            case "NC":
                colorearColumnasNC();
                filtroMotivoDebito.Visible = false;
                checkMotivoDebito.Visible = false;
                label6.Visible = false;
                break;
            case "ND":
                colorearColumnasND();
                filtroMotivoDebito.Visible = true;
                checkMotivoDebito.Visible = true;
                label6.Visible = true;
                break;
        }
        lblCantidadDeRegistrosFiltrados.Text = "Cantidad de registros filtrados: " + cantidadFilas;
    }
    private void filtroProfesional_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cargaListaProfesional || filtroProfesional.SelectedIndex <= 0) return;
        GuardarValoresAntesDeDeshacerFiltro();
        lblProfSel.Text = "Profesional: " + filtroProfesional.Text;
        lblProfSel.Visible = true;
        filtroProfesional.Visible = false;
        AplicarFiltrosActivos();
    }

    private void habilitarFiltros()
    {
        cargaListaFecha = false;
        cargaListaModulo = false;
        cargaListaNumeroDeInternacion = false;
        cargaListaPaciente = false;
        cargaListaPrestacion = false;
        cargaListaProfesional = false;
    }

    private void filtroPrestacion_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cargaListaPrestacion || filtroPrestacion.SelectedIndex <= 0) return;
        GuardarValoresAntesDeDeshacerFiltro();
        lblPrestSel.Text = "Prestación: " + filtroPrestacion.Text;
        lblPrestSel.Visible = true;
        filtroPrestacion.Visible = false;
        AplicarFiltrosActivos();
    }

    private void evaluarPrestacionEnglobante()
    {
        bool ocultar = false;
        foreach (DataGridViewRow fila in dataGridView1.Rows)
        {
            if (fila.Cells["NC_MotivoDeDebito"].Value == "Prestacion incluida en otra")
            {
                ocultar = true;
                break;
            }
        }
        if (ocultar)
        {
            dataGridView1.Columns["nc_prestacionenglobante"].Visible = true;
        }
        else
        {
            dataGridView1.Columns["nc_prestacionenglobante"].Visible = false;
        }
    }

    private void filtroTipo_SelectedIndexChanged(object sender, EventArgs e)
    {
        FacturaTipo = filtroTipo.Text;

        FacturaTipoSeleccionado = true;

        if ((FacturaTipoSeleccionado == true) && (FacturaNumeroSeleccionado == true) && (FacturaLetraSeleccionado == true) && (FacturaPuntoDeVentaSeleccionado == true))
        {
            btnBuscar.Visible = true;
        }
    }

    private void numero_TextChanged(object sender, EventArgs e)
    {
        if (numero.Text.Length > 0)
        {
            FacturaNumeroSeleccionado = TryParseInt(numero, out FacturaNumero, "Por favor, ingrese un número válido.");
        }
        else
        {
            FacturaNumeroSeleccionado = false;
        }
        ActualizarVisibilidadBtnBuscar();
    }

    private void puntodeventa_TextChanged(object sender, EventArgs e)
    {
        if (puntodeventa.Text.Length > 0)
        {
            FacturaPuntoDeVentaSeleccionado = TryParseInt(puntodeventa, out FacturaPuntoDeVenta, "Por favor, ingrese un número válido.");
        }
        else
        {
            FacturaPuntoDeVentaSeleccionado = false;
        }
        ActualizarVisibilidadBtnBuscar();
    }

    private void letra_TextChanged(object sender, EventArgs e)
    {
        if (letra.Text.Length > 0)
        {
            FacturaLetraSeleccionado = TryParseLetra(letra, out FacturaLetra, "Por favor, ingrese una letra válida.");
        }
        else
        {
            FacturaLetraSeleccionado = false;
        }
        ActualizarVisibilidadBtnBuscar();
    }

    private void ActualizarVisibilidadBtnBuscar()
    {
        btnBuscar.Visible = FacturaTipoSeleccionado && FacturaNumeroSeleccionado && FacturaLetraSeleccionado && FacturaPuntoDeVentaSeleccionado;
    }

    private void ConfigurarColumnasDataGridView(List<DataGridViewColumnConfig> configs)
    {
        foreach (var config in configs)
        {
            if (!dataGridView1.Columns.Contains(config.Name))
                continue;

            var col = dataGridView1.Columns[config.Name];
            if (config.Visible.HasValue)
                col.Visible = config.Visible.Value;
            if (config.ReadOnly.HasValue)
                col.ReadOnly = config.ReadOnly.Value;
            if (!string.IsNullOrEmpty(config.HeaderText))
                col.HeaderText = config.HeaderText;

            if ((FacturaTipo == "FC" && config.Name == "nc_comentarios") || (FacturaTipo == "ND" && config.Name == "nc_comentarios") || (FacturaTipo == "NC" && config.Name == "nd_comentarios"))
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            if (config.Name == "NC_ImporteDebitado" || config.Name == "NC_ImporteDeRefactura")
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            if (config.Name == "NC_MotivoDeRefactura" || config.Name == "NC_MotivoDeDebito" || config.Name == "nd_MotivoDeRefactura")
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            }

            if (config.Name == "paciente" && FacturaTipo == "NC")
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
        }

        foreach (DataGridViewRow row in dataGridView1.Rows)
        {
            if (row.IsNewRow) continue;
            foreach (var config in configs)
            {
                if (!dataGridView1.Columns.Contains(config.Name))
                    continue;
                var cell = row.Cells[config.Name];
                if (config.BackColor.HasValue)
                    cell.Style.BackColor = config.BackColor.Value;
            }
        }
    }

    private void colorearColumnasFC()
    {
        if (TipoRegistroFiltrado == "Internados")
        {
            if (dataGridView1.Columns.Contains("F_Ingreso")) dataGridView1.Columns["F_Ingreso"].Visible = false;
            if (dataGridView1.Columns.Contains("F_Egreso")) dataGridView1.Columns["F_Egreso"].Visible = false;
            if (dataGridView1.Columns.Contains("Nro_Int")) dataGridView1.Columns["Nro_Int"].Visible = false;
        }

        ConfigurarColumnasDataGridView(DataGridViewColumnConfig.GetFCColumnConfigs());

        dataGridView1.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
        dataGridView1.ColumnHeadersHeight = 40;
        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.ColumnHeader;

        // Lógica especial: celdas coral si nc_motivoderefactura está vacía
        foreach (DataGridViewRow row in dataGridView1.Rows)
        {
            if (!row.IsNewRow && dataGridView1.Columns.Contains("nc_motivoderefactura") && dataGridView1.Columns.Contains("nc_comentarios"))
            {
                if ((row.Cells["nc_motivoderefactura"].Value?.ToString() ?? "") == "")
                {
                    row.Cells["nc_comentarios"].ReadOnly = true;
                    row.Cells["nc_comentarios"].Style.BackColor = System.Drawing.Color.Coral;
                }
            }
        }
    }

    private void colorearColumnasNC()
    {
        if (TipoRegistroFiltrado == "Internados")
        {
            if (dataGridView1.Columns.Contains("F_Ingreso")) dataGridView1.Columns["F_Ingreso"].Visible = false;
            if (dataGridView1.Columns.Contains("F_Egreso")) dataGridView1.Columns["F_Egreso"].Visible = false;
            if (dataGridView1.Columns.Contains("Nro_Int")) dataGridView1.Columns["Nro_Int"].Visible = false;
        }

        ConfigurarColumnasDataGridView(DataGridViewColumnConfig.GetNCColumnConfigs());

        dataGridView1.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
        dataGridView1.ColumnHeadersHeight = 40;
        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.ColumnHeader;

        // Lógica especial: celdas coral si nd_motivoderefactura está vacía
        foreach (DataGridViewRow row in dataGridView1.Rows)
        {
            if (!row.IsNewRow && dataGridView1.Columns.Contains("nd_motivoderefactura") && dataGridView1.Columns.Contains("nd_comentarios"))
            {
                if ((row.Cells["nd_motivoderefactura"].Value?.ToString() ?? "") == "")
                {
                    row.Cells["nd_comentarios"].ReadOnly = true;
                    row.Cells["nd_comentarios"].Style.BackColor = System.Drawing.Color.Coral;
                }
            }
        }
        btnBorrarFiltros.Visible = false;
    }

    private void colorearColumnasND()
    {
        if (TipoRegistroFiltrado == "Internados")
        {
            if (dataGridView1.Columns.Contains("F_Ingreso")) dataGridView1.Columns["F_Ingreso"].Visible = false;
            if (dataGridView1.Columns.Contains("F_Egreso")) dataGridView1.Columns["F_Egreso"].Visible = false;
            if (dataGridView1.Columns.Contains("Nro_Int")) dataGridView1.Columns["Nro_Int"].Visible = false;
        }

        ConfigurarColumnasDataGridView(DataGridViewColumnConfig.GetNDColumnConfigs());

        dataGridView1.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
        dataGridView1.ColumnHeadersHeight = 40;
        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.ColumnHeader;

        // Lógica especial: celdas coral si nc_motivoderefactura está vacía
        foreach (DataGridViewRow row in dataGridView1.Rows)
        {
            if (!row.IsNewRow && dataGridView1.Columns.Contains("nc_motivoderefactura") && dataGridView1.Columns.Contains("nc_comentarios"))
            {
                if ((row.Cells["nc_motivoderefactura"].Value?.ToString() ?? "") == "")
                {
                    row.Cells["nc_comentarios"].ReadOnly = true;
                    row.Cells["nc_comentarios"].Style.BackColor = System.Drawing.Color.Coral;
                }
            }
        }
        btnBorrarFiltros.Visible = false;
    }

    public void contarFilasConDebitoAceptado()
    {
        DataTable dataTableActual = null;
        if (dataGridView1.DataSource is BindingSource bs)
            dataTableActual = bs.DataSource as DataTable;
        else if (dataGridView1.DataSource is DataTable dt)
            dataTableActual = dt;
        DataRow[] filasFiltradas = dataTableActual.Select("nc_debitoaceptado = true");

        lblCantidadDeRegistrosConDebitoAceptado.Text = ("Cantidad de registros con débito aceptado: " + filasFiltradas.Length);
    }


    private void guardarRegistrosPreviosEnBDD(DataTable tablaAMostrar)
    {
        foreach (DataRow fila in tablaAMostrar.Rows)
        {
            // Verifica que nc_debitoaceptado no sea DBNull y que nc_motivodedebito tampoco sea DBNull
            if (fila["nc_debitoaceptado"] != DBNull.Value && fila["nc_motivodedebito"] != DBNull.Value)
            {
                listaPrestacionesYaExistentes.Add((int)fila["id_prestacion"]);
            }
        }
    }


    private void ConfigurarUIPorTipoFactura()
    {
        switch (FacturaTipo)
        {
            case "FC":
                colorearColumnasFC();
                filtroMotivoDebito.Visible = true;
                checkMotivoDebito.Visible = true;
                filtroDebitoAceptado.Visible = true;
                filtroMotivoDebito.Visible = true;
                filtroMotivoDeRefactura.Visible = true;
                checkDebitoAceptado.Visible = true;
                checkMotivoDeRefactura.Visible = true;
                btnBorrarImporteDebito.Visible = true;
                btnBorrarImporteRefactura.Visible = true;
                label1.Visible = true;
                label2.Visible = true;
                label6.Visible = true;
                break;
            case "NC":
                colorearColumnasNC();
                filtroMotivoDeRefactura.Visible = true;
                checkMotivoDeRefactura.Visible = true;
                btnBorrarImporteDebito.Visible = false;
                btnBorrarImporteRefactura.Visible = true;
                label2.Visible = true;
                break;
            case "ND":
                colorearColumnasND();
                filtroMotivoDebito.Visible = true;
                checkMotivoDebito.Visible = true;
                filtroDebitoAceptado.Visible = true;
                filtroMotivoDebito.Visible = true;
                filtroMotivoDeRefactura.Visible = true;
                checkDebitoAceptado.Visible = true;
                checkMotivoDeRefactura.Visible = true;
                btnBorrarImporteDebito.Visible = true;
                btnBorrarImporteRefactura.Visible = true;
                label1.Visible = true;
                label2.Visible = true;
                label6.Visible = true;
                break;
        }
    }

    private void ManejarDocumentoNoEncontrado()
    {
        SetControlesVisibles(false);
        btnNuevaNotaDeCrédito.Visible = false;
        btnNuevaNotaDeDébito.Visible = false;
        btnGuardarParcialmente.Visible = false;
        documentoEncontrado = false;
        MessageBox.Show("No se ha encontrado el documento ingresado.");
    }

    private void PrepararBusqueda()
    {
        lblPacSel.Visible = false;
        lblPrestSel.Visible = false;
        lblProfSel.Visible = false;
        lblNumeroDeInternacionSel.Visible = false;

        listaPrestacionesYaExistentes.Clear();
        listaValoresParaBorradoDeFiltrosNC.Clear();
        listaValoresParaBorradoDeFiltros.Clear();
        listaValoresParaBorradoDeFiltrosND.Clear();
        documentoEncontrado = false;
        //tablaAMostrar.Clear();
        tablasFiltradas.Clear();
    }


    public void actualizarImporteRefactura()
    {
        double? sumaNoAceptados = 0.0;

        if (listaValoresParaBorradoDeFiltros.Count > 0)
        {
            foreach (var item in listaValoresParaBorradoDeFiltros)
            {
                if (!item.debitoAceptado)
                {
                    sumaNoAceptados += item.importeRefactura ?? 0;
                }
            }
        }
        else
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!Convert.IsDBNull(row.Cells["NC_DebitoAceptado"].Value) && !Convert.ToBoolean(row.Cells["NC_DebitoAceptado"].Value))
                {
                    double importe = Convert.IsDBNull(row.Cells["NC_ImporteDeRefactura"].Value)
                                     ? 0
                                     : Convert.ToDouble(row.Cells["NC_ImporteDeRefactura"].Value);

                    sumaNoAceptados += importe;
                }
            }
        }

        lblMontosNoAceptados.Text = "Suma total de débitos a refacturar: " + sumaNoAceptados;
        lblMontosNoAceptados.Visible = true;
    }

    private void filtroMotivoDebito_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cargarSoloFiltroMotivoDebito == false)
        {
            if (filtroMotivoDebito.SelectedIndex != -1)
            {
                // Obtenemos el motivo seleccionado
                string motivoDebito = filtroMotivoDebito.SelectedItem.ToString();

                // Iteramos sobre TODAS las celdas seleccionadas, igual que en el filtroMotivoDeRefactura
                foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
                {
                    var row = dataGridView1.Rows[cell.RowIndex];
                    row.Cells["NC_MotivoDeDebito"].Value = motivoDebito;
                    row.Cells["NC_DebitoAceptado"].Value = false; // Automáticamente se marca que no es un débito aceptado
                }

                checkMotivoDebito.Checked = false;
                filtroMotivoDebito.SelectedItem = null;
                GuardarValoresParaActualizarMontoAuditados();

                // Es importante detener la edición para que el valor "quede fijado" en la grilla visual
                dataGridView1.EndEdit();
                contarFilasConDebitoAceptado();
            }
        }
        cargarSoloFiltroMotivoDebito = false;
    }

    private void AplicarPrestacionIncluidaEnOtraATodasLasFilas()
    {
        var idPrestaciones = new List<int>();
        foreach (DataGridViewRow fila in dataGridView1.Rows)
        {
            if (!fila.IsNewRow)
                idPrestaciones.Add(Convert.ToInt32(fila.Cells["ID_Prestacion"].Value));
        }

        tipoATransmitir = FacturaTipo switch
        {
            "FC" => "NC",
            "NC" => "ND",
            "ND" => "NC",
            _ => ""
        };

        var form2 = new IngresoEnCasoDeCambioDePrestacion
        {
            codigoViejo = "0",
            idPrestaciones = idPrestaciones,
            tipoDocumentoTransmitido = tipoATransmitir
        };
        form2.ShowDialog();

        dataGridView1.Columns["NC_PrestacionEnglobante"].Visible = true;
        foreach (DataGridViewRow fila in dataGridView1.Rows)
        {
            if (!fila.IsNewRow)
            {
                cargaPrimeraVez = true;
                fila.Cells["NC_PrestacionEnglobante"].Value = form2.codigoNuevo;
                fila.Cells["NC_MotivoDeDebito"].Value = filtroMotivoDebito.Text;
            }
        }
        dataGridView1.Refresh();
    }

    private void AplicarMotivoDebitoATodasLasFilas(string motivoSeleccionado)
    {
        bool reemplazarMotivosPrevios = false;
        int columnaMotivoDebito = dataGridView1.Columns["NC_MotivoDeDebito"].Index;
        foreach (DataGridViewRow fila in dataGridView1.Rows)
        {
            // Modifica la condición para que verifique si la celda está vacía (nula o string vacío)
            if (fila.Cells[columnaMotivoDebito].Value != null && fila.Cells[columnaMotivoDebito].Value != DBNull.Value && fila.Cells[columnaMotivoDebito].Value.ToString().Trim() != "")
            {
                DialogResult resultado = MessageBox.Show(
                    "Existen prestaciones con motivo de débito previo. \n" +
                    "¿Desea reemplazar esos motivos preexistentes?\n\n" +
                    "Al seleccionar SI, se reemplazarán los datos preexistentes.\n" +
                    "Al seleccionar NO, se ingresará la información en las celdas vacías.",
                    "Confirmación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2
                );
                reemplazarMotivosPrevios = (resultado == DialogResult.Yes);
                break;
            }
        }

        foreach (DataGridViewRow fila in dataGridView1.Rows)
        {
            if (fila.Cells[columnaMotivoDebito].Value != null && fila.Cells[columnaMotivoDebito].Value != DBNull.Value && fila.Cells[columnaMotivoDebito].Value.ToString().Trim() != "")
            {
                if (reemplazarMotivosPrevios)
                {
                    if (!fila.IsNewRow)
                        fila.Cells[columnaMotivoDebito].Value = motivoSeleccionado;
                }
            }
            else
            {
                if (!fila.IsNewRow)
                    fila.Cells[columnaMotivoDebito].Value = motivoSeleccionado;
            }
        }
        EvaluarPrestacionEnglobanteEnGrilla(motivoSeleccionado);
        dataGridView1.Refresh();
    }

    private void AplicarMotivoDebitoACeldasSeleccionadas(string motivoSeleccionado)
    {
        if (dataGridView1.SelectedCells.Count == 0)
        {
            MessageBox.Show("Por favor, seleccione una o más celdas en el DataGridView para aplicar el valor.");
            return;
        }

        // Analizar si hay celdas seleccionadas con motivo de débito previo
        bool hayMotivosPrevios = false;
        foreach (DataGridViewCell celda in dataGridView1.SelectedCells)
        {
            if (dataGridView1.Columns[celda.ColumnIndex].Name == "nc_motivodedebito")
            {
                if (celda.Value != null && celda.Value != DBNull.Value && celda.Value.ToString().Trim() != "")
                {
                    hayMotivosPrevios = true;
                    break;
                }
            }
        }

        bool reemplazarMotivosPrevios = false;
        if (hayMotivosPrevios)
        {
            DialogResult resultado = MessageBox.Show(
                "Existen prestaciones seleccionadas con motivo de débito previo. \n" +
                "¿Desea reemplazar esos motivos preexistentes?\n\n" +
                "Al seleccionar SI, se reemplazarán los datos preexistentes.\n" +
                "Al seleccionar NO, se ingresará la información en las celdas vacías.",
                "Confirmación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2
            );
            reemplazarMotivosPrevios = (resultado == DialogResult.Yes);
        }

        // Aplicar el motivo según la decisión del usuario
        foreach (DataGridViewCell celda in dataGridView1.SelectedCells)
        {
            if (dataGridView1.Columns[celda.ColumnIndex].Name == "nc_motivodedebito")
            {
                if (reemplazarMotivosPrevios ||
                    celda.Value == null ||
                    celda.Value == DBNull.Value ||
                    celda.Value.ToString().Trim() == "")
                {
                    celda.Value = motivoSeleccionado;
                }
            }
        }

        if (motivoSeleccionado == "Prestacion incluida en otra")
        {
            AplicarPrestacionIncluidaEnOtraASoloUnaSeleccion();
        }
        else
        {
            EvaluarPrestacionEnglobanteEnGrilla(motivoSeleccionado);
        }
        dataGridView1.Refresh();
    }

    private void AplicarPrestacionIncluidaEnOtraASoloUnaSeleccion()
    {
        if ((dataGridView1.SelectedRows.Count == 1) || (dataGridView1.SelectedCells.Count == 1))
        {
            DataGridViewRow filaSeleccionada = dataGridView1.SelectedRows.Count == 1
                ? dataGridView1.SelectedRows[0]
                : dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex];

            string codigoViejo = filaSeleccionada.Cells["codigo"].Value.ToString();
            int idPrestacion = Convert.ToInt32(filaSeleccionada.Cells["ID_Prestacion"].Value);

            tipoATransmitir = FacturaTipo switch
            {
                "FC" => "NC",
                "NC" => "ND",
                "ND" => "NC",
                _ => ""
            };

            var form2 = new IngresoEnCasoDeCambioDePrestacion
            {
                codigoViejo = codigoViejo,
                idPrestacion = idPrestacion,
                idPrestaciones = new List<int> { idPrestacion },
                tipoDocumentoTransmitido = tipoATransmitir
            };
            form2.ShowDialog();

            // Asignar el valor seleccionado al DataGridView
            filaSeleccionada.Cells["nc_prestacionenglobante"].Value = form2.codigoNuevo;

            dataGridView1.Columns["NC_PrestacionEnglobante"].Visible = true;
            dataGridView1.Columns["NC_PrestacionEnglobante"].ReadOnly = true;
        }
        else
        {
            MessageBox.Show("Debe seleccionar un solo registro para ésta operación");

            // Borrar el contenido de las celdas seleccionadas de la columna nc_motivodedebito
            foreach (DataGridViewCell celda in dataGridView1.SelectedCells)
            {
                if (dataGridView1.Columns[celda.ColumnIndex].Name == "nc_motivodedebito")
                {
                    celda.Value = DBNull.Value;
                }
            }
        }
    }

    private void EvaluarPrestacionEnglobanteEnGrilla(string motivoSeleccionado)
    {
        bool hayPrestacionIncluida = false;
        foreach (DataGridViewRow row in dataGridView1.Rows)
        {
            if (!row.IsNewRow)
            {
                string motDeb = row.Cells["NC_MotivoDeDebito"].Value?.ToString();
                if (FacturaTipo != "ND" && motDeb == "Prestacion incluida en otra")
                {
                    hayPrestacionIncluida = true;
                    break;
                }
            }
        }

        if (hayPrestacionIncluida)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow)
                {
                    string motivoDebito = row.Cells["NC_MotivoDeDebito"].Value?.ToString();
                    if (FacturaTipo != "ND" && motivoDebito == "Prestacion incluida en otra")
                    {
                        row.Cells["NC_PrestacionEnglobante"].ReadOnly = false;
                    }
                    else
                    {
                        row.Cells["NC_PrestacionEnglobante"].ReadOnly = true;
                        row.Cells["NC_PrestacionEnglobante"].Style.BackColor = System.Drawing.Color.Gray;
                    }
                }
            }
            dataGridView1.Columns["NC_PrestacionEnglobante"].Visible = true;
        }
        else
        {
            if (FacturaTipo != "ND")
                dataGridView1.Columns["NC_PrestacionEnglobante"].Visible = false;
        }
    }


    private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
        if (cargaPrimeraVez)
            return;

        cargaPrimeraVez = true;

        if (e.RowIndex < 0 || e.RowIndex >= dataGridView1.Rows.Count)
        {
            cargaPrimeraVez = false;
            return;
        }

        var fila = dataGridView1.Rows[e.RowIndex];

        if (EsColumna(fila, e.ColumnIndex, "NC_MotivoDeDebito"))
            ProcesarCambioMotivoDeDebito(fila);

        if ((FacturaTipo == "FC" || FacturaTipo == "ND") && EsColumna(fila, e.ColumnIndex, "NC_MotivoDeRefactura"))
            ProcesarCambioMotivoDeRefactura(fila);

        if ((FacturaTipo == "FC" || FacturaTipo == "ND") && (EsColumna(fila, e.ColumnIndex, "NC_ImporteDeRefactura") || EsColumna(fila, e.ColumnIndex, "NC_ImporteDebitado")))
            ValidarYActualizarImporte(fila, e.ColumnIndex);

        if ((FacturaTipo == "FC" || FacturaTipo == "ND") && EsColumna(fila, e.ColumnIndex, "NC_DiasFacturados"))
            ProcesarCambioDiasFacturados(fila);

        if (FacturaTipo == "NC" && EsColumna(fila, e.ColumnIndex, "ND_MotivoDeRefactura"))
            ProcesarCambioMotivoDeRefacturaNC(fila);

        ProcesarReadOnlyYEstilosPorDebitoAceptado(fila, e.ColumnIndex);

        cargaPrimeraVez = false;
        dataGridView1.Refresh();
        if (debitoIndividual)
            actualizarCantidadDeDebitosAceptados();
    }

    private bool EsColumna(DataGridViewRow fila, int columnIndex, string nombreColumna)
    {
        return dataGridView1.Columns[columnIndex].Name == nombreColumna;
    }

    private void ProcesarCambioMotivoDeDebito(DataGridViewRow fila)
    {
        string motivoDebito = fila.Cells["NC_MotivoDeDebito"].Value?.ToString();

        fila.Cells["NC_DebitoAceptado"].ReadOnly = false;
        fila.Cells["NC_DebitoAceptado"].Style.BackColor = System.Drawing.Color.LightCyan;

        if (motivoDebito == "Prestacion incluida en otra")
        {
            fila.Cells["NC_PrestacionEnglobante"].ReadOnly = false;
        }
        else if (FacturaTipo == "NC")
        {
            fila.Cells["NC_PrestacionEnglobante"].ReadOnly = true;
            fila.Cells["NC_PrestacionEnglobante"].Style.BackColor = System.Drawing.Color.Gray;

            bool control = false;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                string motivoDebitoFilaActual = row.Cells["NC_MotivoDeDebito"].Value?.ToString();
                if (motivoDebitoFilaActual == "Prestacion incluida en otra")
                {
                    control = true;
                    break;
                }
            }
            if (control)
                dataGridView1.Columns["NC_PrestacionEnglobante"].Visible = true;
        }

        GuardarValoresParaActualizarMontoAuditados();
    }

    private void ProcesarCambioMotivoDeRefactura(DataGridViewRow fila)
    {
        switch (FacturaTipo)
        {
            case "FC":
                if (fila.Cells["NC_ImporteDeRefactura"].Value != null && fila.Cells["NC_ImporteDeRefactura"].Value != DBNull.Value)
                {
                    fila.Cells["NC_ImporteDeRefactura"].Value = fila.Cells["total_neto"].Value;
                    fila.Cells["NC_Comentarios"].ReadOnly = false;
                    fila.Cells["NC_Comentarios"].Style.BackColor = System.Drawing.Color.LightGray;
                }
                break;
            case "ND":
                fila.Cells["NC_ImporteDeRefactura"].Value = fila.Cells["importerefactura"].Value;
                fila.Cells["NC_Comentarios"].ReadOnly = false;
                fila.Cells["NC_Comentarios"].Style.BackColor = System.Drawing.Color.LightGray;
                break;
        }

        string motivoDeRefactura = fila.Cells["NC_MotivoDeRefactura"].Value?.ToString();
        if (motivoDeRefactura == "Prestacion incluida en otra")
        {
            string codigoViejo = fila.Cells["codigo"].Value.ToString();
            tipoATransmitir = "ND";
            IngresoEnCasoDeCambioDePrestacion form2 = new IngresoEnCasoDeCambioDePrestacion
            {
                codigoViejo = codigoViejo
            };
            form2.Show();
        }
    }

    private void ValidarYActualizarImporte(DataGridViewRow fila, int columnIndex)
    {
        var cell = fila.Cells[columnIndex];
        if (decimal.TryParse(cell.Value?.ToString(), out decimal nuevoImporte))
        {
            // Aquí puedes agregar lógica adicional si es necesario
        }
        else
        {
            MessageBox.Show("Por favor ingrese un número válido");
            cargaPrimeraVez = true;
            cell.Value = 0;
            cargaPrimeraVez = false;
        }
    }

    private void ProcesarCambioDiasFacturados(DataGridViewRow fila)
    {
        if (int.TryParse(fila.Cells["NC_DiasFacturados"].Value?.ToString(), out int nuevaCantidad))
        {
            double cantidadOriginal = Convert.ToDouble(fila.Cells["total_neto"].Value);
            double cantidad = Convert.ToDouble(fila.Cells["cantidad"].Value);

            if (cantidadOriginal > 0 && cantidad > 0)
            {
                fila.Cells["NC_ImporteDebitado"].Value = (cantidadOriginal / cantidad) * nuevaCantidad;
            }
            else
            {
                fila.Cells["NC_ImporteDebitado"].Value = 0;
            }
            fila.Cells["NC_Comentarios"].ReadOnly = false;
            fila.Cells["NC_Comentarios"].Style.BackColor = System.Drawing.Color.LightGray;
        }
        else
        {
            MessageBox.Show("Por favor ingrese un número válido");
        }
    }

    private void ProcesarCambioMotivoDeRefacturaNC(DataGridViewRow fila)
    {
        fila.Cells["nd_importederefactura"].Value = fila.Cells["nc_importederefactura"].Value;
        fila.Cells["nd_comentarios"].ReadOnly = false;
        fila.Cells["nd_comentarios"].Style.BackColor = System.Drawing.Color.LightGray;
    }

    private void ProcesarReadOnlyYEstilosPorDebitoAceptado(DataGridViewRow fila, int columnIndex)
    {
        // FC
        if (FacturaTipo == "FC")
        {
            if (EsColumna(fila, columnIndex, "nc_importederefactura"))
                GuardarValoresParaActualizarMontoDeRefactura();

            if (EsColumna(fila, columnIndex, "nc_debitoaceptado"))
            {
                bool aceptado = Convert.ToBoolean(fila.Cells["nc_debitoaceptado"].Value);
                fila.Cells["NC_MotivoDeRefactura"].ReadOnly = aceptado;
                fila.Cells["nc_importederefactura"].ReadOnly = aceptado;
                fila.Cells["NC_MotivoDeRefactura"].Style.BackColor = aceptado ? System.Drawing.Color.LightGray : System.Drawing.Color.LightCyan;
                fila.Cells["nc_importederefactura"].Style.BackColor = aceptado ? System.Drawing.Color.LightGray : System.Drawing.Color.LightCyan;
            }
            else if (EsColumna(fila, columnIndex, "nc_importedebitado"))
            {
                GuardarValoresParaActualizarMontoAuditados();
            }
        }
        // NC
        else if (FacturaTipo == "NC")
        {
            if (EsColumna(fila, columnIndex, "nd_importederefactura"))
                GuardarValoresParaActualizarMontoDeRefactura();

            if (EsColumna(fila, columnIndex, "nc_debitoaceptado"))
            {
                bool aceptado = Convert.ToBoolean(fila.Cells["nc_debitoaceptado"].Value);
                fila.Cells["ND_MotivoDeRefactura"].ReadOnly = aceptado;
                fila.Cells["nd_importederefactura"].ReadOnly = aceptado;
                fila.Cells["ND_MotivoDeRefactura"].Style.BackColor = aceptado ? System.Drawing.Color.LightGray : System.Drawing.Color.LightCyan;
                fila.Cells["nd_importederefactura"].Style.BackColor = aceptado ? System.Drawing.Color.LightGray : System.Drawing.Color.LightCyan;
            }
            if (EsColumna(fila, columnIndex, "ND_MotivoDeRefactura"))
            {
                fila.Cells["nd_importederefactura"].Value = fila.Cells["nc_importederefactura"].Value;
            }
        }
        // ND
        else if (FacturaTipo == "ND")
        {
            if (EsColumna(fila, columnIndex, "nc_importederefactura"))
                GuardarValoresParaActualizarMontoDeRefactura();

            if (EsColumna(fila, columnIndex, "nc_debitoaceptado"))
            {
                bool aceptado = Convert.ToBoolean(fila.Cells["nc_debitoaceptado"].Value);
                fila.Cells["NC_MotivoDeRefactura"].ReadOnly = aceptado;
                fila.Cells["nc_importederefactura"].ReadOnly = aceptado;
                fila.Cells["NC_MotivoDeRefactura"].Style.BackColor = aceptado ? System.Drawing.Color.LightGray : System.Drawing.Color.LightCyan;
                fila.Cells["nc_importederefactura"].Style.BackColor = aceptado ? System.Drawing.Color.LightGray : System.Drawing.Color.LightCyan;
            }
        }
    }


    private void filtroDebitoAceptado_SelectedIndexChanged(object sender, EventArgs e)
    {
        bool debitoAceptado = false;
        debitoIndividual = false;

        if (filtroDebitoAceptado.SelectedItem.ToString() == "Si")
        {
            debitoAceptado = true;
        }
        else
        {
            debitoAceptado = false;
        }

        if (checkDebitoAceptado.Checked == true)
        {
            int columnaDebitoAceptado = dataGridView1.Columns["nc_debitoaceptado"].Index;
            int columnaImporteDebitado = dataGridView1.Columns["nc_importedebitado"].Index;
            int columnaImporteTotal = 0;
            switch (FacturaTipo)
            {
                case "FC":
                    columnaImporteTotal = dataGridView1.Columns["total"].Index;
                    break;
                case "ND":
                    columnaImporteTotal = dataGridView1.Columns["importerefactura"].Index;
                    break;
            }

            foreach (DataGridViewRow fila in dataGridView1.Rows)
            {
                if (!fila.IsNewRow)
                {
                    fila.Cells[columnaDebitoAceptado].Value = debitoAceptado;
                    fila.Cells[columnaImporteDebitado].Value = fila.Cells[columnaImporteTotal].Value;

                    switch (FacturaTipo)
                    {
                        case "FC":
                            if (debitoAceptado)
                            {
                                fila.Cells["NC_MotivoDeRefactura"].ReadOnly = true;
                                fila.Cells["nc_importederefactura"].ReadOnly = true;
                                fila.Cells["NC_MotivoDeRefactura"].Style.BackColor = System.Drawing.Color.LightGray;
                                fila.Cells["nc_importederefactura"].Style.BackColor = System.Drawing.Color.LightGray;
                            }
                            else
                            {
                                fila.Cells["NC_MotivoDeRefactura"].ReadOnly = false;
                                fila.Cells["nc_importederefactura"].ReadOnly = false;
                                fila.Cells["NC_MotivoDeRefactura"].Style.BackColor = System.Drawing.Color.LightCyan;
                                fila.Cells["nc_importederefactura"].Style.BackColor = System.Drawing.Color.LightCyan;
                            }

                            break;

                        case "NC":
                            if (debitoAceptado)
                            {
                                fila.Cells["ND_MotivoDeRefactura"].ReadOnly = true;
                                fila.Cells["nd_importederefactura"].ReadOnly = true;
                                fila.Cells["ND_MotivoDeRefactura"].Style.BackColor = System.Drawing.Color.LightGray;
                                fila.Cells["nd_importederefactura"].Style.BackColor = System.Drawing.Color.LightGray;
                            }
                            else
                            {
                                fila.Cells["ND_MotivoDeRefactura"].ReadOnly = false;
                                fila.Cells["nd_importederefactura"].ReadOnly = false;
                                fila.Cells["ND_MotivoDeRefactura"].Style.BackColor = System.Drawing.Color.LightCyan;
                                fila.Cells["nd_importederefactura"].Style.BackColor = System.Drawing.Color.LightCyan;
                            }
                            break;

                        case "ND":
                            if (debitoAceptado)
                            {
                                fila.Cells["NC_MotivoDeRefactura"].ReadOnly = true;
                                fila.Cells["nc_importederefactura"].ReadOnly = true;
                                fila.Cells["NC_MotivoDeRefactura"].Style.BackColor = System.Drawing.Color.LightGray;
                                fila.Cells["nc_importederefactura"].Style.BackColor = System.Drawing.Color.LightGray;
                            }
                            else
                            {
                                fila.Cells["NC_MotivoDeRefactura"].ReadOnly = false;
                                fila.Cells["nc_importederefactura"].ReadOnly = false;
                                fila.Cells["NC_MotivoDeRefactura"].Style.BackColor = System.Drawing.Color.LightGray;
                                fila.Cells["nc_importederefactura"].Style.BackColor = System.Drawing.Color.LightGray;
                            }

                            break;
                    }
                }
            }

            dataGridView1.Refresh();
            actualizarCantidadDeDebitosAceptados();
        }
        else
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                foreach (DataGridViewCell celda in dataGridView1.SelectedCells)
                {
                    if (dataGridView1.Columns[celda.ColumnIndex].Name == "nc_debitoaceptado")
                    {
                        celda.Value = debitoAceptado;
                        int columnaImporteTotal = 0;
                        int columnaImporteDebitado = dataGridView1.Columns["nc_importedebitado"].Index;
                        if (FacturaTipo == "FC")
                        {
                            columnaImporteTotal = dataGridView1.Columns["total"].Index;
                        }
                        else
                        {
                            columnaImporteTotal = dataGridView1.Columns["importerefactura"].Index;
                        }



                        dataGridView1.Rows[celda.RowIndex].Cells[columnaImporteDebitado].Value = dataGridView1.Rows[celda.RowIndex].Cells[columnaImporteTotal].Value;

                        switch (FacturaTipo)
                        {
                            case "FC":
                                if (debitoAceptado)
                                {
                                    dataGridView1.Rows[celda.RowIndex].Cells["NC_MotivoDeRefactura"].ReadOnly = true;
                                    dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].ReadOnly = true;
                                    dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].Style.BackColor = System.Drawing.Color.LightGray;
                                    dataGridView1.Rows[celda.RowIndex].Cells["NC_MotivoDeRefactura"].Style.BackColor = System.Drawing.Color.LightGray;
                                }
                                else
                                {
                                    dataGridView1.Rows[celda.RowIndex].Cells["NC_MotivoDeRefactura"].ReadOnly = false;
                                    dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].ReadOnly = false;
                                    dataGridView1.Rows[celda.RowIndex].Cells["NC_MotivoDeRefactura"].Style.BackColor = System.Drawing.Color.LightCyan;
                                    dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].Style.BackColor = System.Drawing.Color.LightCyan;
                                }

                                break;

                            case "NC":
                                if (debitoAceptado)
                                {
                                    dataGridView1.Rows[celda.RowIndex].Cells["ND_MotivoDeRefactura"].ReadOnly = true;
                                    dataGridView1.Rows[celda.RowIndex].Cells["nd_importederefactura"].ReadOnly = true;
                                    dataGridView1.Rows[celda.RowIndex].Cells["ND_MotivoDeRefactura"].Style.BackColor = System.Drawing.Color.LightGray;
                                    dataGridView1.Rows[celda.RowIndex].Cells["nd_importederefactura"].Style.BackColor = System.Drawing.Color.LightGray;
                                }
                                else
                                {
                                    dataGridView1.Rows[celda.RowIndex].Cells["ND_MotivoDeRefactura"].ReadOnly = false;
                                    dataGridView1.Rows[celda.RowIndex].Cells["nd_importederefactura"].ReadOnly = false;
                                    dataGridView1.Rows[celda.RowIndex].Cells["ND_MotivoDeRefactura"].Style.BackColor = System.Drawing.Color.LightCyan;
                                    dataGridView1.Rows[celda.RowIndex].Cells["nd_importederefactura"].Style.BackColor = System.Drawing.Color.LightCyan;
                                }
                                break;

                            case "ND":
                                if (debitoAceptado)
                                {
                                    dataGridView1.Rows[celda.RowIndex].Cells["NC_MotivoDeRefactura"].ReadOnly = true;
                                    dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].ReadOnly = true;
                                    dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].Style.BackColor = System.Drawing.Color.LightGray;
                                    dataGridView1.Rows[celda.RowIndex].Cells["NC_MotivoDeRefactura"].Style.BackColor = System.Drawing.Color.LightGray;
                                }
                                else
                                {
                                    dataGridView1.Rows[celda.RowIndex].Cells["NC_MotivoDeRefactura"].ReadOnly = false;
                                    dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].ReadOnly = false;
                                    dataGridView1.Rows[celda.RowIndex].Cells["NC_MotivoDeRefactura"].Style.BackColor = System.Drawing.Color.LightCyan;
                                    dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].Style.BackColor = System.Drawing.Color.LightCyan;
                                }

                                break;
                        }
                    }
                }
                dataGridView1.Refresh();

                actualizarCantidadDeDebitosAceptados();

            }
            else
            {
                MessageBox.Show("Por favor, seleccione una o más celdas en el DataGridView para aplicar el valor.");
            }
        }

        debitoIndividual = true;
        //contarFilasConDebitoAceptado();

    }

    private void filtroMotivoDeRefactura_SelectedIndexChanged(object sender, EventArgs e)
    {
        string motivoDeRefacturaSeleccionadaNC = filtroMotivoDeRefactura.Text;

        if (motivoDeRefacturaSeleccionadaNC == "Borrar")
        {
            motivoDeRefacturaSeleccionadaNC = "";
        }

        bool reemplazarMotivosPrevios = false;

        switch (FacturaTipo)
        {
            case "FC":

                if (checkMotivoDeRefactura.Checked == true)
                {
                    int columnaMotivoDeRefacturaNC = 0;
                    int columnaImporteDeRefactura = dataGridView1.Columns["nc_importederefactura"].Index;
                    columnaMotivoDeRefacturaNC = dataGridView1.Columns["NC_MotivoDeRefactura"].Index;

                    foreach (DataGridViewRow fila in dataGridView1.Rows)
                    {
                        if (fila.Cells[columnaMotivoDeRefacturaNC].Value != null && fila.Cells[columnaMotivoDeRefacturaNC].Value != DBNull.Value && fila.Cells[columnaMotivoDeRefacturaNC].Value.ToString().Trim() != "")
                        {
                            DialogResult resultado = MessageBox.Show(
                                "Existen prestaciones con motivo de refactura previo. \n" +
                                "¿Desea reemplazar esos motivos preexistentes?\n\n" +
                                "Al seleccionar SI, se reemplazarán los datos preexistentes.\n" +
                                "Al seleccionar NO, se ingresará la información en las celdas vacías.",
                                "Confirmación",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2
                            );
                            reemplazarMotivosPrevios = (resultado == DialogResult.Yes);
                            break;
                        }
                    }

                    foreach (DataGridViewRow fila in dataGridView1.Rows)
                    {
                        if (fila.Cells[columnaMotivoDeRefacturaNC].Value != null && fila.Cells[columnaMotivoDeRefacturaNC].Value != DBNull.Value && fila.Cells[columnaMotivoDeRefacturaNC].Value.ToString().Trim() != "")
                        {
                            if (reemplazarMotivosPrevios)
                            {
                                if (!fila.IsNewRow)
                                {
                                    fila.Cells[columnaMotivoDeRefacturaNC].Value = motivoDeRefacturaSeleccionadaNC;
                                    if (motivoDeRefacturaSeleccionadaNC == "")
                                    {
                                        fila.Cells["nc_importederefactura"].Value = DBNull.Value;
                                        fila.Cells["nc_comentarios"].ReadOnly = true;
                                        fila.Cells["nc_comentarios"].Style.BackColor = System.Drawing.Color.Coral;
                                    }
                                }


                            }
                        }
                        else
                        {
                            fila.Cells[columnaMotivoDeRefacturaNC].Value = motivoDeRefacturaSeleccionadaNC;
                            if (motivoDeRefacturaSeleccionadaNC == "")
                            {
                                fila.Cells["nc_importederefactura"].Value = DBNull.Value;
                                fila.Cells["nc_comentarios"].ReadOnly = true;
                                fila.Cells["nc_comentarios"].Style.BackColor = System.Drawing.Color.Coral;
                            }
                            else
                            {
                                //fila.Cells["nc_importederefactura"].Value = fila.Cells["total_neto"].Value;
                                fila.Cells["nc_comentarios"].ReadOnly = false;
                                fila.Cells["nc_comentarios"].Style.BackColor = System.Drawing.Color.LightGray;
                            }
                        }

                    }

                    dataGridView1.Refresh();
                }
                else
                {
                    if (dataGridView1.SelectedCells.Count > 0)
                    {
                        bool hayMotivosPrevios = false;
                        foreach (DataGridViewCell celda in dataGridView1.SelectedCells)
                        {
                            if (dataGridView1.Columns[celda.ColumnIndex].Name == "nc_motivoderefactura")
                            {
                                if (celda.Value != null && celda.Value != DBNull.Value && celda.Value.ToString().Trim() != "")
                                {
                                    hayMotivosPrevios = true;
                                    break;
                                }
                            }
                        }

                        if (hayMotivosPrevios)
                        {
                            DialogResult resultado = MessageBox.Show(
                                "Existen prestaciones con motivo de refactura previo. \n" +
                                "¿Desea reemplazar esos motivos preexistentes?\n\n" +
                                "Al seleccionar SI, se reemplazarán los datos preexistentes.\n" +
                                "Al seleccionar NO, se ingresará la información en las celdas vacías.",
                                "Confirmación",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2
                            );
                            reemplazarMotivosPrevios = (resultado == DialogResult.Yes);
                        }

                        foreach (DataGridViewCell celda in dataGridView1.SelectedCells)
                        {
                            if (dataGridView1.Columns[celda.ColumnIndex].Name == "nc_motivoderefactura")
                            {
                                if (celda.Value == null || celda.Value == DBNull.Value || celda.Value.ToString().Trim() == "")
                                {
                                    if (motivoDeRefacturaSeleccionadaNC == "")
                                    {
                                        dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].Value = DBNull.Value;
                                        dataGridView1.Rows[celda.RowIndex].Cells["nc_comentarios"].ReadOnly = true;
                                        dataGridView1.Rows[celda.RowIndex].Cells["nc_comentarios"].Style.BackColor = System.Drawing.Color.Coral;
                                    }
                                    else
                                    {
                                        if (!(motivoDeRefacturaSeleccionadaNC == "No aplica"))
                                        {
                                            cargaPrimeraVez = true;

                                            dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].ReadOnly = false;
                                            //dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].Value = dataGridView1.Rows[celda.RowIndex].Cells["total_neto"].Value;
                                            dataGridView1.Rows[celda.RowIndex].Cells["nc_comentarios"].ReadOnly = false;
                                            dataGridView1.Rows[celda.RowIndex].Cells["nc_comentarios"].Style.BackColor = System.Drawing.Color.LightGray;
                                        }
                                        else
                                        {
                                            cargaPrimeraVez = true;

                                            dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].ReadOnly = true;
                                            dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].Value = DBNull.Value;
                                            dataGridView1.Rows[celda.RowIndex].Cells["nc_comentarios"].ReadOnly = false;
                                            dataGridView1.Rows[celda.RowIndex].Cells["nc_comentarios"].Style.BackColor = System.Drawing.Color.LightGray;
                                        }
                                    }

                                    celda.Value = motivoDeRefacturaSeleccionadaNC;
                                }
                                else
                                {
                                    if (reemplazarMotivosPrevios)
                                    {
                                        celda.Value = motivoDeRefacturaSeleccionadaNC;
                                        if (motivoDeRefacturaSeleccionadaNC == "")
                                        {
                                            dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].Value = DBNull.Value;
                                            dataGridView1.Rows[celda.RowIndex].Cells["nc_comentarios"].ReadOnly = true;
                                            dataGridView1.Rows[celda.RowIndex].Cells["nc_comentarios"].Style.BackColor = System.Drawing.Color.Coral;
                                            dataGridView1.Refresh();
                                        }
                                        else
                                        {
                                            if (!(motivoDeRefacturaSeleccionadaNC == "No aplica"))
                                            {
                                                cargaPrimeraVez = true;

                                                dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].ReadOnly = false;
                                                //dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].Value = dataGridView1.Rows[celda.RowIndex].Cells["total_neto"].Value;
                                                dataGridView1.Rows[celda.RowIndex].Cells["nc_comentarios"].ReadOnly = false;
                                                dataGridView1.Rows[celda.RowIndex].Cells["nc_comentarios"].Style.BackColor = System.Drawing.Color.LightGray;
                                            }
                                            else
                                            {
                                                cargaPrimeraVez = true;

                                                dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].ReadOnly = true;
                                                dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].Value = DBNull.Value;
                                                dataGridView1.Rows[celda.RowIndex].Cells["nc_comentarios"].ReadOnly = false;
                                                dataGridView1.Rows[celda.RowIndex].Cells["nc_comentarios"].Style.BackColor = System.Drawing.Color.LightGray;
                                            }
                                        }

                                    }
                                }
                            }
                        }

                        cargaPrimeraVez = false;
                        dataGridView1.Refresh();
                    }
                    else
                    {
                        MessageBox.Show("Por favor, seleccione una o más celdas en el DataGridView para aplicar el valor.");
                    }
                }
                break;

            case "NC":
                if (checkMotivoDeRefactura.Checked == true)
                {
                    int columnaMotivoDeRefacturaNC = 0;

                    columnaMotivoDeRefacturaNC = dataGridView1.Columns["ND_MotivoDeRefactura"].Index;

                    foreach (DataGridViewRow fila in dataGridView1.Rows)
                    {
                        if (fila.Cells[columnaMotivoDeRefacturaNC].Value != null && fila.Cells[columnaMotivoDeRefacturaNC].Value != DBNull.Value && fila.Cells[columnaMotivoDeRefacturaNC].Value.ToString().Trim() != "")
                        {
                            DialogResult resultado = MessageBox.Show(
                                "Existen prestaciones con motivo de refactura previo. \n" +
                                "¿Desea reemplazar esos motivos preexistentes?\n\n" +
                                "Al seleccionar SI, se reemplazarán los datos preexistentes.\n" +
                                "Al seleccionar NO, se ingresará la información en las celdas vacías.",
                                "Confirmación",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2
                            );
                            reemplazarMotivosPrevios = (resultado == DialogResult.Yes);
                            break;
                        }
                    }

                    columnaMotivoDeRefacturaNC = dataGridView1.Columns["ND_MotivoDeRefactura"].Index;

                    foreach (DataGridViewRow fila in dataGridView1.Rows)
                    {
                        if (fila.Cells[columnaMotivoDeRefacturaNC].Value != null && fila.Cells[columnaMotivoDeRefacturaNC].Value != DBNull.Value && fila.Cells[columnaMotivoDeRefacturaNC].Value.ToString().Trim() != "")
                        {
                            if (reemplazarMotivosPrevios)
                            {
                                if (!fila.IsNewRow)
                                    fila.Cells[columnaMotivoDeRefacturaNC].Value = motivoDeRefacturaSeleccionadaNC;
                            }
                        }
                        else
                        {
                            fila.Cells[columnaMotivoDeRefacturaNC].Value = motivoDeRefacturaSeleccionadaNC;
                            if (motivoDeRefacturaSeleccionadaNC != "No aplica")
                            {
                                //fila.Cells["nd_importederefactura"].Value = fila.Cells["nc_importederefactura"].Value;
                                fila.Cells["nd_comentarios"].ReadOnly = false;
                                fila.Cells["nd_comentarios"].Style.BackColor = System.Drawing.Color.LightGray;
                            }
                        }
                    }

                    dataGridView1.Refresh();
                }
                else
                {
                    if (dataGridView1.SelectedCells.Count > 0)
                    {
                        bool hayMotivosPrevios = false;
                        foreach (DataGridViewCell celda in dataGridView1.SelectedCells)
                        {
                            if (dataGridView1.Columns[celda.ColumnIndex].Name == "nd_motivoderefactura")
                            {
                                if (celda.Value != null && celda.Value != DBNull.Value && celda.Value.ToString().Trim() != "")
                                {
                                    hayMotivosPrevios = true;
                                    break;
                                }
                            }
                        }

                        if (hayMotivosPrevios)
                        {
                            DialogResult resultado = MessageBox.Show(
                                "Existen prestaciones con motivo de refactura previo. \n" +
                                "¿Desea reemplazar esos motivos preexistentes?\n\n" +
                                "Al seleccionar SI, se reemplazarán los datos preexistentes.\n" +
                                "Al seleccionar NO, se ingresará la información en las celdas vacías.",
                                "Confirmación",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2
                            );
                            reemplazarMotivosPrevios = (resultado == DialogResult.Yes);
                        }

                        foreach (DataGridViewCell celda in dataGridView1.SelectedCells)
                        {
                            if (dataGridView1.Columns[celda.ColumnIndex].Name == "nd_motivoderefactura")
                            {
                                celda.Value = motivoDeRefacturaSeleccionadaNC;
                                if (celda.Value == null || celda.Value == DBNull.Value || celda.Value.ToString().Trim() == "")
                                {
                                    if (reemplazarMotivosPrevios)
                                    {
                                        if (!(motivoDeRefacturaSeleccionadaNC == "No aplica"))
                                        {
                                            cargaPrimeraVez = true;

                                            //dataGridView1.Rows[celda.RowIndex].Cells["nd_importederefactura"].Value = dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].Value;
                                            dataGridView1.Rows[celda.RowIndex].Cells["nd_comentarios"].ReadOnly = false;
                                            dataGridView1.Rows[celda.RowIndex].Cells["nd_comentarios"].Style.BackColor = System.Drawing.Color.LightGray;
                                        }
                                        else
                                        {
                                            cargaPrimeraVez = true;

                                            //dataGridView1.Rows[celda.RowIndex].Cells["nd_importederefactura"].Value = dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].Value;
                                            dataGridView1.Rows[celda.RowIndex].Cells["nd_comentarios"].ReadOnly = false;
                                            dataGridView1.Rows[celda.RowIndex].Cells["nd_comentarios"].Style.BackColor = System.Drawing.Color.LightGray;
                                        }
                                        celda.Value = motivoDeRefacturaSeleccionadaNC;
                                    }
                                }
                                else
                                {
                                    if (!(motivoDeRefacturaSeleccionadaNC == "No aplica"))
                                    {
                                        cargaPrimeraVez = true;

                                        //dataGridView1.Rows[celda.RowIndex].Cells["nd_importederefactura"].Value = dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].Value;
                                        dataGridView1.Rows[celda.RowIndex].Cells["nd_comentarios"].ReadOnly = false;
                                        dataGridView1.Rows[celda.RowIndex].Cells["nd_comentarios"].Style.BackColor = System.Drawing.Color.LightGray;
                                    }
                                    else
                                    {
                                        cargaPrimeraVez = true;

                                        //dataGridView1.Rows[celda.RowIndex].Cells["nd_importederefactura"].Value = dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].Value;
                                        dataGridView1.Rows[celda.RowIndex].Cells["nd_comentarios"].ReadOnly = false;
                                        dataGridView1.Rows[celda.RowIndex].Cells["nd_comentarios"].Style.BackColor = System.Drawing.Color.LightGray;
                                    }
                                    celda.Value = motivoDeRefacturaSeleccionadaNC;
                                }
                            }
                        }

                        dataGridView1.Refresh();
                    }
                    else
                    {
                        MessageBox.Show("Por favor, seleccione una o más celdas en el DataGridView para aplicar el valor.");
                    }
                }
                break;

            case "ND":
                if (checkMotivoDeRefactura.Checked == true)
                {
                    int columnaMotivoDeRefacturaNC = 0;
                    columnaMotivoDeRefacturaNC = dataGridView1.Columns["NC_MotivoDeRefactura"].Index;

                    foreach (DataGridViewRow fila in dataGridView1.Rows)
                    {
                        if (fila.Cells[columnaMotivoDeRefacturaNC].Value != null && fila.Cells[columnaMotivoDeRefacturaNC].Value != DBNull.Value && fila.Cells[columnaMotivoDeRefacturaNC].Value.ToString().Trim() != "")
                        {
                            DialogResult resultado = MessageBox.Show(
                                "Existen prestaciones con motivo de refactura previo. \n" +
                                "¿Desea reemplazar esos motivos preexistentes?\n\n" +
                                "Al seleccionar SI, se reemplazarán los datos preexistentes.\n" +
                                "Al seleccionar NO, se ingresará la información en las celdas vacías.",
                                "Confirmación",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2
                            );
                            reemplazarMotivosPrevios = (resultado == DialogResult.Yes);
                            break;
                        }
                    }

                    foreach (DataGridViewRow fila in dataGridView1.Rows)
                    {
                        if (fila.Cells[columnaMotivoDeRefacturaNC].Value != null && fila.Cells[columnaMotivoDeRefacturaNC].Value != DBNull.Value && fila.Cells[columnaMotivoDeRefacturaNC].Value.ToString().Trim() != "")
                        {
                            if (reemplazarMotivosPrevios)
                            {
                                if (!fila.IsNewRow)
                                    fila.Cells[columnaMotivoDeRefacturaNC].Value = motivoDeRefacturaSeleccionadaNC;
                            }
                        }
                        else
                        {
                            fila.Cells[columnaMotivoDeRefacturaNC].Value = motivoDeRefacturaSeleccionadaNC;
                            if (motivoDeRefacturaSeleccionadaNC != "No aplica")
                            {
                                //fila.Cells["nc_importederefactura"].Value = fila.Cells["importerefactura"].Value;
                                fila.Cells["nc_comentarios"].ReadOnly = false;
                                fila.Cells["nc_comentarios"].Style.BackColor = System.Drawing.Color.LightGray;
                            }
                        }

                    }

                    dataGridView1.Refresh();
                }
                else
                {
                    if (dataGridView1.SelectedCells.Count > 0)
                    {
                        bool hayMotivosPrevios = false;
                        foreach (DataGridViewCell celda in dataGridView1.SelectedCells)
                        {
                            if (dataGridView1.Columns[celda.ColumnIndex].Name == "nc_motivoderefactura")
                            {
                                if (celda.Value != null && celda.Value != DBNull.Value && celda.Value.ToString().Trim() != "")
                                {
                                    hayMotivosPrevios = true;
                                    break;
                                }
                            }
                        }

                        if (hayMotivosPrevios)
                        {
                            DialogResult resultado = MessageBox.Show(
                                "Existen prestaciones con motivo de refactura previo. \n" +
                                "¿Desea reemplazar esos motivos preexistentes?\n\n" +
                                "Al seleccionar SI, se reemplazarán los datos preexistentes.\n" +
                                "Al seleccionar NO, se ingresará la información en las celdas vacías.",
                                "Confirmación",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2
                            );
                            reemplazarMotivosPrevios = (resultado == DialogResult.Yes);
                        }

                        foreach (DataGridViewCell celda in dataGridView1.SelectedCells)
                        {
                            if (dataGridView1.Columns[celda.ColumnIndex].Name == "nc_motivoderefactura")
                            {
                                celda.Value = motivoDeRefacturaSeleccionadaNC;
                                if (celda.Value == null || celda.Value == DBNull.Value || celda.Value.ToString().Trim() == "")
                                {
                                    if (reemplazarMotivosPrevios)
                                    {
                                        if (!(motivoDeRefacturaSeleccionadaNC == "No aplica"))
                                        {
                                            cargaPrimeraVez = true;

                                            dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].ReadOnly = false;
                                            //dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].Value = dataGridView1.Rows[celda.RowIndex].Cells["importerefactura"].Value;
                                            dataGridView1.Rows[celda.RowIndex].Cells["nc_comentarios"].ReadOnly = false;
                                            dataGridView1.Rows[celda.RowIndex].Cells["nc_comentarios"].Style.BackColor = System.Drawing.Color.LightGray;
                                        }
                                        else
                                        {
                                            cargaPrimeraVez = true;

                                            dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].ReadOnly = true;
                                            dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].Value = DBNull.Value;
                                            dataGridView1.Rows[celda.RowIndex].Cells["nc_comentarios"].ReadOnly = false;
                                            dataGridView1.Rows[celda.RowIndex].Cells["nc_comentarios"].Style.BackColor = System.Drawing.Color.LightGray;
                                        }
                                        celda.Value = motivoDeRefacturaSeleccionadaNC;
                                    }
                                }
                                else
                                {
                                    if (!(motivoDeRefacturaSeleccionadaNC == "No aplica"))
                                    {
                                        cargaPrimeraVez = true;

                                        dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].ReadOnly = false;
                                        //dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].Value = dataGridView1.Rows[celda.RowIndex].Cells["importerefactura"].Value;
                                        dataGridView1.Rows[celda.RowIndex].Cells["nc_comentarios"].ReadOnly = false;
                                        dataGridView1.Rows[celda.RowIndex].Cells["nc_comentarios"].Style.BackColor = System.Drawing.Color.LightGray;
                                    }
                                    else
                                    {
                                        cargaPrimeraVez = true;

                                        dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].ReadOnly = true;
                                        dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].Value = DBNull.Value;
                                        dataGridView1.Rows[celda.RowIndex].Cells["nc_comentarios"].ReadOnly = false;
                                        dataGridView1.Rows[celda.RowIndex].Cells["nc_comentarios"].Style.BackColor = System.Drawing.Color.LightGray;
                                    }
                                    celda.Value = motivoDeRefacturaSeleccionadaNC;
                                }
                            }
                        }

                        cargaPrimeraVez = false;
                        dataGridView1.Refresh();
                    }
                    else
                    {
                        MessageBox.Show("Por favor, seleccione una o más celdas en el DataGridView para aplicar el valor.");
                    }
                }
                break;
        }

    }

    private void btnNuevaNotaDeCrédito_Click(object sender, EventArgs e)
    {
        _repository.LimpiarAuxiliarNC();

        switch (FacturaTipo)
        {
            case "FC":
                GuardarValoresAntesDeDeshacerFiltro();
                _repository.InsertarAuxiliarNC_FC(listaValoresParaBorradoDeFiltros, usuario, TipoRegistroFiltrado);
                break;
            case "ND":
                GuardarValoresAntesDeDeshacerFiltroND();
                _repository.InsertarAuxiliarNC_ND(listaValoresParaBorradoDeFiltrosND, usuario, TipoRegistroFiltrado);
                break;
        }

        AbrirFormularioNotaDeCredito();
        limpiarPantall();
        panel1.Visible = false;
        lblModulo.Visible = false;
        btnBorrarCelda.Visible = false;
        lblCantidadDeRegistrosFiltrados.Visible = false;
    }

    private void AbrirFormularioNotaDeCredito()
    {
        ingresoInformacionNotaDeCredito = new IngresoInformacionNotaDeCredito(
            cargaACompletar, FacturaNumero, FacturaLetra, FacturaPuntoDeVenta, FacturaTipo);
        ingresoInformacionNotaDeCredito.Show();
    }


    private void btnNuevaNotaDeDébito_Click(object sender, EventArgs e)
    {
        GuardarValoresAntesDeDeshacerFiltroNC();
        _repository.LimpiarAuxiliarND();
        _repository.InsertarAuxiliarND(listaValoresParaBorradoDeFiltrosNC, usuario, TipoRegistroFiltrado);

        AbrirFormularioNotaDeDebito();
        limpiarPantall();
        panel1.Visible = false;
        lblModulo.Visible = false;
        btnBorrarCelda.Visible = false;
        lblCantidadDeRegistrosFiltrados.Visible = false;
    }

    private void AbrirFormularioNotaDeDebito()
    {
        ingresoInformacionNotaDeDebito = new IngresoInformacionNotaDeDebito(
            cargaACompletar, FacturaNumero, FacturaLetra, FacturaPuntoDeVenta, FacturaTipo);
        ingresoInformacionNotaDeDebito.Show();
    }


    public void resetearBusqueda()
    {
        dataGridView1.DataSource = null;
        SetControlesVisibles(false);

        filtroTipo.SelectedItem = null;
        letra.Text = string.Empty;
        puntodeventa.Text = string.Empty;
        numero.Text = string.Empty;

        FacturaLetraSeleccionado = false;
        FacturaNumeroSeleccionado = false;
        FacturaPuntoDeVentaSeleccionado = false;
        FacturaTipoSeleccionado = false;

        tablaAMostrar.Clear();
        lblCantidadDeRegistrosConDebitoAceptado.Visible = false;
        lblCantidadDeRegistrosFiltrados.Visible = false;
    }

    private void btnLimpiarFila_Click(object sender, EventArgs e)
    {
        switch (FacturaTipo)
        {
            case "FC":
                LimpiarCeldasSeleccionadasFC();
                break;
            case "NC":
                LimpiarCeldasSeleccionadasNC();
                break;
            case "ND":
                LimpiarCeldasSeleccionadasND();
                break;
        }
        GuardarValoresParaActualizarMontoDeRefactura();
    }

    private void LimpiarCeldasSeleccionadasFC()
    {
        foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
        {
            var row = dataGridView1.Rows[cell.RowIndex];
            row.Cells["nc_importederefactura"].Value = DBNull.Value;
            row.Cells["nc_debitoaceptado"].Value = false;
            row.Cells["NC_MotivoDeRefactura"].Value = DBNull.Value;
            row.Cells["NC_MotivoDeDebito"].Value = DBNull.Value;
            row.Cells["nc_importedebitado"].Value = DBNull.Value;
        }
        contarFilasConDebitoAceptado();
    }

    private void LimpiarCeldasSeleccionadasNC()
    {
        foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
        {
            var row = dataGridView1.Rows[cell.RowIndex];
            row.Cells["NC_MotivoDeRefactura"].Value = DBNull.Value;
            row.Cells["nc_importederefactura"].Value = DBNull.Value;
        }
    }

    private void LimpiarCeldasSeleccionadasND()
    {
        foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
        {
            var row = dataGridView1.Rows[cell.RowIndex];
            row.Cells["nc_importederefactura"].Value = DBNull.Value;
            row.Cells["nc_debitoaceptado"].Value = false;
            row.Cells["NC_MotivoDeRefactura"].Value = DBNull.Value;
            row.Cells["NC_MotivoDeDebito"].Value = DBNull.Value;
            row.Cells["nc_importedebitado"].Value = DBNull.Value;
        }
        contarFilasConDebitoAceptado();
    }

    private void btnExportar_Click_1(object sender, EventArgs e)
    {
        // Crear un nuevo libro de trabajo de Excel
        using (XLWorkbook workbook = new XLWorkbook())
        {
            // Agregar una nueva hoja de trabajo
            IXLWorksheet worksheet = workbook.Worksheets.Add("Hoja1");

            // Copiar los encabezados del DataGridView
            for (int i = 1; i <= dataGridView1.Columns.Count; i++)
            {
                worksheet.Cell(1, i).Value = dataGridView1.Columns[i - 1].HeaderText;
            }

            // Copiar los datos del DataGridView
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                for (int j = 0; j < dataGridView1.Columns.Count; j++)
                {
                    object cellValue = dataGridView1.Rows[i].Cells[j].Value;

                    if (cellValue is Double or float or Int32)
                    {
                        worksheet.Cell(i + 2, j + 1).Value = Convert.ToDouble(cellValue);
                    }
                    else if (cellValue is DateTime)
                    {
                        worksheet.Cell(i + 2, j + 1).Value = (DateTime)cellValue;
                    }
                    else if (cellValue is bool)
                    {
                        worksheet.Cell(i + 2, j + 1).Value = (bool)cellValue;
                    }
                    else
                    {
                        worksheet.Cell(i + 2, j + 1).Value = Convert.ToString(cellValue);
                    }
                }
            }

            // Guardar el archivo de Excel
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Excel Files (*.xlsx)|*.xlsx";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)

            {
                workbook.SaveAs(saveFileDialog1.FileName);
                MessageBox.Show("Datos exportados a Excel correctamente.");
            }
        }
    }

    private void btnGuardarParcialmente_Click(object sender, EventArgs e)
    {
        PrepararGuardadoParcial();

        switch (FacturaTipo)
        {
            case "FC":
                GuardarParcialFC();
                break;
            case "NC":
                GuardarParcialNC();
                break;
            case "ND":
                GuardarParcialND();
                break;
        }

        MostrarMensajeGuardadoParcial();
    }

    private void PrepararGuardadoParcial()
    {
        cargaCompletada = false;
        lblMontosNoAceptados.Visible = false;
        lblMontoTotalRegistrosEnPantalla.Visible = false;
    }

    private void MostrarMensajeGuardadoParcial()
    {
        MessageBox.Show("Se ha almacenado de forma correcta parcialmente el documento");
    }

    private void btnBuscar_Click(object sender, EventArgs e)
    {
        BuscarDocumentoEvent?.Invoke(this, EventArgs.Empty);
    }

    private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
    {
        // Guardar los valores antes de ordenar
        switch (FacturaTipo)
        {
            case "FC":
            case "ND":
                GuardarValoresAntesDeOrdenar();
                break;
            case "NC":
                GuardarValoresAntesDeOrdenarNC();
                break;
        }

        // Luego, aplicar el formato de columnas
        switch (FacturaTipo)
        {
            case "FC":
                colorearColumnasFC();
                break;
            case "NC":
                colorearColumnasNC();
                break;
            case "ND":
                colorearColumnasND();
                break;
        }
    }


    private void GuardarParcialFC()
    {
        dataGridView1.SuspendLayout();
        var listaParaGuardar = new List<CargaParcialDTO>();

        foreach (DataGridViewRow row in dataGridView1.Rows)
        {
            bool debitoAceptado = row.Cells["NC_DebitoAceptado"].Value != DBNull.Value && Convert.ToBoolean(row.Cells["NC_DebitoAceptado"].Value);
            string motivoDebito = row.Cells["NC_MotivoDeDebito"].Value?.ToString() ?? "";
            string motivoRefactura = row.Cells["NC_MotivoDeRefactura"].Value?.ToString() ?? "";

            if (debitoAceptado || !string.IsNullOrEmpty(motivoDebito) || !string.IsNullOrEmpty(motivoRefactura))
            {
                var dto = new CargaParcialDTO
                {
                    IdPrestacion = Convert.ToInt32(row.Cells["ID_Prestacion"].Value),
                    DebitoAceptado = debitoAceptado,
                    MotivoDebito = row.Cells["NC_MotivoDeDebito"].Value,
                    ImporteDebitado = row.Cells["NC_ImporteDebitado"].Value,
                    DiasFacturados = row.Cells["NC_DiasFacturados"].Value,
                    MotivoRefactura = row.Cells["NC_MotivoDeRefactura"].Value,
                    ImporteRefactura = row.Cells["NC_ImporteDeRefactura"].Value,
                    PrestacionEnglobante = row.Cells["NC_PrestacionEnglobante"].Value?.ToString(),
                    Comentarios = row.Cells["NC_Comentarios"].Value?.ToString(),
                    CargadoCompletamente = false,
                    Usuario = usuario
                    // Eliminamos la asignación de EsActualizacion
                };

                listaParaGuardar.Add(dto);
            }
        }

        dataGridView1.ResumeLayout();

        if (listaParaGuardar.Count > 0)
        {
            try
            {
                _repository.GuardarCargaParcialFC(listaParaGuardar);
                MessageBox.Show("Carga parcial guardada correctamente.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message);
            }
        }
    }

    private void GuardarParcialNC()
    {
        GuardarValoresAntesDeDeshacerFiltroNC();
        string queryObtenerNC = @"SELECT * FROM notadedebito WHERE id_prestacion = @id_prestacion;";
        string queryNoCompleto = @"INSERT INTO cargaincompleta (tipodocumento, letra, ptovta, numero, id_prestacion) VALUES (@tipodocumento, @letra, @ptovta, @numero, @id_prestacion)";
        string queryInsert = @"INSERT INTO notadedebito (id_notadecredito, motivorefactura, importerefactura, codigo, usuario, id_prestacion, cargadocompletamente, comentarios, tiporegistro) VALUES (@id_notadecredito, @motivoderefactura, @importederefactura, @codigo, @usuario, @id_prestacion, @cargadocompletamente, @comentarios, @tiporegistro);";
        string queryUpdate = @"UPDATE notadedebito SET motivorefactura = @motivoderefactura, importerefactura = @importederefactura, usuario = @usuario, codigo = @codigo, cargadocompletamente = @cargarcompletamente, comentarios = @comentarios WHERE id_prestacion = @id_prestacion AND cargadocompletamente = @cargadocompletamente;";

        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        foreach (var item in listaValoresParaBorradoDeFiltrosNC)
        {
            if (cargaParcialPreviamenteCreada && ExisteRegistro(connection, queryObtenerNC, item.idPrestacion))
            {
                ActualizarNotaDeDebito(connection, queryUpdate, item, item.codigo, item.idNotaDeCredito, item.idPrestacion, item.motivoRefactura, item.importeRefactura, item.comentarios);
            }
            else
            {
                InsertarCargaIncompleta(connection, queryNoCompleto, item.idPrestacion);
                InsertarNotaDeDebito(connection, queryInsert, item, item.codigo, item.idNotaDeCredito, item.idPrestacion, item.motivoRefactura, item.importeRefactura, item.comentarios);
            }
        }
    }

    private void GuardarParcialND()
    {
        string queryObtenerND = @"SELECT * FROM notadecredito WHERE id_prestacion = @id_prestacion;";
        string queryNoCompleto = @"INSERT INTO cargaincompleta (tipodocumento, letra, ptovta, numero, id_prestacion) VALUES (@tipodocumento, @letra, @ptovta, @numero, @id_prestacion)";
        string queryInsert = @"INSERT INTO notadecredito (id_prestacion, motivodedebito, diasfacturados, importedebitado, debitoaceptado, motivoderefactura, importederefactura, usuario, cargadocompletamente, id_notadedebito, comentarios) VALUES (@id_prestacion, @motivodedebito, @diasfacturados, @importedebitado, @debitoaceptado, @motivoderefactura, @importederefactura, @usuario, @cargadocompletamente, @id_notadedebito, @comentarios)";
        string queryUpdate = @"UPDATE notadecredito SET motivodedebito = @motivodedebito, diasfacturados = @diasfacturados, importedebitado = @importedebitado, debitoaceptado = @debitoaceptado, motivoderefactura = @motivoderefactura, importederefactura = @importederefactura, usuario = @usuario, cargadocompletamente = @cargarcompletamente, comentarios = @comentarios WHERE id_prestacion = @id_prestacion AND id_notadedebito = @id_notadedebito;";

        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        foreach (var item in listaValoresParaBorradoDeFiltrosND)
        {
            if (cargaParcialPreviamenteCreada && ExisteRegistro(connection, queryObtenerND, item.idPrestacion))
            {
                ActualizarNotadeCreditoND(connection, queryUpdate, item);
            }
            else
            {
                InsertarCargaIncompleta(connection, queryNoCompleto, item.idPrestacion);
                InsertarNotadeCreditoND(connection, queryInsert, item);
            }
        }
    }

    private bool ExisteRegistro(NpgsqlConnection connection, string query, int idPrestacion)
    {
        using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@id_prestacion", idPrestacion);
        using var adapter = new NpgsqlDataAdapter(cmd);
        var dt = new DataTable();
        adapter.Fill(dt);
        return dt.Rows.Count > 0;
    }

    private void InsertarCargaIncompleta(NpgsqlConnection connection, string query, int idPrestacion)
    {
        using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@tipodocumento", FacturaTipo);
        cmd.Parameters.AddWithValue("@letra", FacturaLetra);
        cmd.Parameters.AddWithValue("@ptovta", FacturaPuntoDeVenta);
        cmd.Parameters.AddWithValue("@numero", FacturaNumero);
        cmd.Parameters.AddWithValue("@id_prestacion", idPrestacion);
        cmd.ExecuteNonQuery();
    }

    private void InsertarNotadeCredito(NpgsqlConnection connection, string query, dynamic item, int idPrestacion, object motivoRefactura, object motivoDebito, double? importeDebito, double? importeRefactura, object diasFacturados, bool debitoAceptado, string prestacionEnglobante, string comentarios, string tiporegistro)
    {
        using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@id_prestacion", idPrestacion);
        cmd.Parameters.AddWithValue("@motivodedebito", motivoDebito ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@diasfacturados", diasFacturados);
        cmd.Parameters.AddWithValue("@importedebitado", (object)importeDebito ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@debitoaceptado", debitoAceptado);
        cmd.Parameters.AddWithValue("@motivoderefactura", motivoRefactura ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@importederefactura", (object)importeRefactura ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@prestacionenglobante", prestacionEnglobante);
        cmd.Parameters.AddWithValue("@usuario", usuario);
        cmd.Parameters.AddWithValue("@cargadocompletamente", false);
        cmd.Parameters.AddWithValue("@comentarios", comentarios);
        cmd.Parameters.AddWithValue("@tiporegistro", tiporegistro);
        cmd.ExecuteNonQuery();
    }

    private void ActualizarNotadeCredito(NpgsqlConnection connection, string query, dynamic item, int idPrestacion, object motivoRefactura, object motivoDebito, double? importeDebito, double? importeRefactura, object diasFacturados, bool debitoAceptado, string prestacionEnglobante, string comentarios)
    {
        using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@motivodedebito", motivoDebito ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@importedebitado", (object?)importeDebito ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@debitoaceptado", debitoAceptado);
        cmd.Parameters.AddWithValue("@motivoderefactura", motivoRefactura ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@importederefactura", importeRefactura ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@prestacionenglobante", prestacionEnglobante);
        cmd.Parameters.AddWithValue("@id_prestacion", idPrestacion);
        cmd.Parameters.AddWithValue("@diasFacturados", diasFacturados);
        cmd.Parameters.AddWithValue("@comentarios", comentarios ?? "");
        cmd.Parameters.AddWithValue("@usuario", usuario);
        cmd.Parameters.AddWithValue("@cargadocompletamente", false);
        cmd.Parameters.AddWithValue("@cargarcompletamente", false);
        cmd.ExecuteNonQuery();
    }

    private void InsertarNotaDeDebito(NpgsqlConnection connection, string query, dynamic item, string codigo, int idNotaDeCredito, int idPrestacion, object motivoRefactura, double? importeRefactura, string comentarios)
    {
        using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@motivoderefactura", motivoRefactura);
        cmd.Parameters.AddWithValue("@importederefactura", importeRefactura);
        cmd.Parameters.AddWithValue("@codigo", codigo);
        cmd.Parameters.AddWithValue("@id_prestacion", idPrestacion);
        cmd.Parameters.AddWithValue("@comentarios", comentarios);
        cmd.Parameters.AddWithValue("@usuario", usuario);
        cmd.Parameters.AddWithValue("@cargadocompletamente", false);
        cmd.Parameters.AddWithValue("@id_notadecredito", idNotaDeCredito);
        cmd.Parameters.AddWithValue("@tiporegistro", TipoRegistroFiltrado);
        cmd.ExecuteNonQuery();
    }

    private void ActualizarNotaDeDebito(NpgsqlConnection connection, string query, dynamic item, string codigo, int idNotaDeCredito, int idPrestacion, object motivoRefactura, double? importeRefactura, string comentarios)
    {
        using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@motivoderefactura", motivoRefactura);
        cmd.Parameters.AddWithValue("@importederefactura", importeRefactura);
        cmd.Parameters.AddWithValue("@codigo", codigo);
        cmd.Parameters.AddWithValue("@id_prestacion", idPrestacion);
        cmd.Parameters.AddWithValue("@comentarios", comentarios);
        cmd.Parameters.AddWithValue("@usuario", usuario);
        cmd.Parameters.AddWithValue("@cargadocompletamente", false);
        cmd.Parameters.AddWithValue("@cargarcompletamente", false);
        cmd.ExecuteNonQuery();
    }

    private void InsertarNotadeCreditoND(NpgsqlConnection connection, string query, dynamic item)
    {
        using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@id_prestacion", item.idPrestacion);
        cmd.Parameters.AddWithValue("@diasfacturados", item.diasFacturados ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@motivodedebito", item.motivoDebito ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@importedebitado", item.importeDebito ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@debitoaceptado", item.debitoAceptado);
        cmd.Parameters.AddWithValue("@motivoderefactura", item.motivoRefactura ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@importederefactura", item.importeRefactura ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@id_notadedebito", item.idNotaDeDebito ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@comentarios", item.comentarios ?? "");
        cmd.Parameters.AddWithValue("@usuario", usuario);
        cmd.Parameters.AddWithValue("@cargadocompletamente", false);
        cmd.ExecuteNonQuery();
    }

    private void ActualizarNotadeCreditoND(NpgsqlConnection connection, string query, dynamic item)
    {
        using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@motivodedebito", item.motivoDebito ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@diasfacturados", item.diasFacturados ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@importedebitado", item.importeDebito ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@debitoaceptado", item.debitoAceptado);
        cmd.Parameters.AddWithValue("@motivoderefactura", item.motivoRefactura ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@importederefactura", item.importeRefactura ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@id_prestacion", item.idPrestacion);
        cmd.Parameters.AddWithValue("@id_notadedebito", item.idNotaDeDebito ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@comentarios", item.comentarios ?? "");
        cmd.Parameters.AddWithValue("@usuario", usuario);
        cmd.Parameters.AddWithValue("@cargadocompletamente", false);
        cmd.Parameters.AddWithValue("@cargarcompletamente", false);
        cmd.ExecuteNonQuery();
    }

    private void limpiarPantall()
    {
        tablasFiltrosMedico.Clear();
        tablasFiltrosPaciente.Clear();
        tablasFiltrosPrestacion.Clear();
        tablasFiltrosModulo.Clear();
        tablaAMostrar.Clear();
        tablaAMostrar.Rows.Clear();
        tablaAMostrar.Columns.Clear();
        FacturaTipo = "";
        FacturaLetra = "";
        FacturaPuntoDeVenta = 0;
        FacturaNumero = 0;
        dataGridView1.DataSource = tablaAMostrar;

        button1.Visible = false;

        SetControlesVisibles(false);

        cargaListaPaciente = true;
        cargaListaModulo = true;
        cargaListaPrestacion = true;
        cargaListaProfesional = true;
        cargaListaFacturaTipo = true;
        cargaListaFacturaLetra = true;
        cargaListaFacturaPuntoDeVenta = true;
        cargaListaFacturaNumero = true;
        cargaListaNumeroDeInternacion = true;

        cargaLista = false;
        cargarSoloFiltroMotivoDebito = false;

        dataTablePaciente.Clear();
        aUsarParaLimpiarFiltroAnterior.Clear();
        tablaCompletaSinFiltros.Clear();
        filtroPacienteSinFiltros.Clear();
        filtroMedicoSinFiltros.Clear();
        filtroPrestacionSinFiltros.Clear();
        filtroModuloSinFiltros.Clear();


        letra.Text = "";
        numero.Text = "";
        puntodeventa.Text = "";
        filtroTipo.Text = "Tipo";
        cargaPrimeraVez = true;

        resetearVariables();
    }

    // Evento cuando el usuario selecciona una fecha en el ComboBox

    private void comboFiltroFecha_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cargaListaFecha || buscando || comboFiltroFecha.SelectedIndex <= 0) return;
        GuardarValoresAntesDeDeshacerFiltro();
        lblFecSel.Text = "Fecha: " + comboFiltroFecha.Text;
        lblFecSel.Visible = true;
        comboFiltroFecha.Visible = false;
        AplicarFiltrosActivos();
    }

    // Método para guardar los valores antes de ordenar
    private void GuardarValoresAntesDeOrdenar()
    {
        listaValores.Clear();

        foreach (DataGridViewRow row in dataGridView1.Rows)
        {
            if (row.Cells["id_prestacion"].Value != null && row.Cells["NC_MotivoDeRefactura"].Value != null && row.Cells["NC_MotivoDeDebito"].Value != null)
            {
                int idPrestacion = Convert.ToInt32(row.Cells["id_prestacion"].Value);
                object motivoRefactura = row.Cells["NC_MotivoDeRefactura"].Value;
                object motivoDebito = row.Cells["NC_MotivoDeDebito"].Value;
                double importeRefactura = row.Cells["NC_ImporteDeRefactura"].Value == DBNull.Value ? 0.0 : Convert.ToDouble(row.Cells["NC_ImporteDeRefactura"].Value);
                double importeDebito = row.Cells["NC_ImporteDebitado"].Value == DBNull.Value ? 0.0 : Convert.ToDouble(row.Cells["NC_ImporteDebitado"].Value);
                listaValores.Add((idPrestacion, motivoRefactura, motivoDebito, importeRefactura, importeDebito));
                valoresOriginales.Add((idPrestacion, motivoRefactura, motivoDebito));
            }
        }
    }

    private void GuardarValoresParaActualizarMontoAuditados()
    {
        foreach (DataGridViewRow row in dataGridView1.Rows)
        {
            object importeValue = row.Cells["NC_ImporteDebitado"].Value;
            object debitoAceptadoValue = row.Cells["NC_DebitoAceptado"].Value;

            double importeAux = 0.0;
            bool tieneImporte = importeValue != DBNull.Value &&
                                double.TryParse(importeValue.ToString(), out importeAux) &&
                                importeAux > 0;

            bool aceptaDebito = debitoAceptadoValue != DBNull.Value &&
                                Convert.ToBoolean(debitoAceptadoValue);

            // ?? La condición final: solo continuar si ambas condiciones son verdaderas
            if (tieneImporte && aceptaDebito)
            {
                int idPrestacion = Convert.ToInt32(row.Cells["id_prestacion"].Value);

                bool existe = listaValoresParaImporteDeDebito.Any(item => item.idPrestacion == idPrestacion);

                if (!existe)
                {
                    listaValoresParaImporteDeDebito.Add((idPrestacion, importeAux));
                }
                else
                {
                    int index = listaValoresParaImporteDeDebito.FindIndex(item => item.idPrestacion == idPrestacion);
                    listaValoresParaImporteDeDebito[index] = (idPrestacion, importeAux);
                }
            }
        }

        // Calcular la suma fuera del bucle para mayor eficiencia
        double importe = listaValoresParaImporteDeDebito.Sum(item => (double)item.importeRefactura);

        lblMontoTotalRegistrosEnPantalla.Text = ("Suma total de débitos auditados: " + importe.ToString("C"));

        lblMontoTotalRegistrosEnPantalla.Visible = true;
    }

    private void GuardarValoresParaActualizarMontoDeRefactura()
    {
        foreach (DataGridViewRow row in dataGridView1.Rows)
        {
            if ((row.Cells["NC_ImporteDeRefactura"].Value != DBNull.Value) && ((row.Cells["NC_DebitoAceptado"].Value == DBNull.Value) || (!Convert.ToBoolean(row.Cells["NC_DebitoAceptado"].Value))))
            {
                int idPrestacion = Convert.ToInt32(row.Cells["id_prestacion"].Value);

                // Verificar si el idPrestacion ya está en la lista
                bool existe = listaValoresParaImporteDeRefactura.Any(item => item.idPrestacion == idPrestacion);

                if (!existe) // Si no existe, agregarlo
                {
                    double importeRefactura = 0.0;
                    if (row.Cells["NC_ImporteDeRefactura"].Value != null)
                    {
                        importeRefactura = row.Cells["NC_ImporteDeRefactura"].Value == DBNull.Value ? 0.0 : Convert.ToDouble(row.Cells["NC_ImporteDeRefactura"].Value);
                    }

                    listaValoresParaImporteDeRefactura.Add((idPrestacion, importeRefactura));
                }
                else
                {
                    int index = listaValoresParaImporteDeRefactura.FindIndex(item => item.idPrestacion == idPrestacion);
                    double nuevoImporte = row.Cells["NC_ImporteDeRefactura"].Value == DBNull.Value ? 0.0 : Convert.ToDouble(row.Cells["NC_ImporteDeRefactura"].Value);
                    listaValoresParaImporteDeRefactura[index] = (idPrestacion, nuevoImporte);
                }
            }
        }

        // Calcular la suma fuera del bucle para mayor eficiencia
        double importe = listaValoresParaImporteDeRefactura.Sum(item => (double)item.importeRefactura);

        lblMontosNoAceptados.Text = ("Suma total de débitos a refacturar: " + importe.ToString("C")); // "C" para formato de moneda

        lblMontosNoAceptados.Visible = true;
    }

    private void GuardarValoresAntesDeDeshacerFiltro()
    {
        DataTable dataTableActual = null;
        if (dataGridView1.DataSource is BindingSource bs)
            dataTableActual = bs.DataSource as DataTable;
        else if (dataGridView1.DataSource is DataTable dt)
            dataTableActual = dt;
        // 1. Usa la función Select para filtrar solo las filas relevantes
        //    Esto es mucho más rápido que recorrer toda la grilla.
        DataRow[] filasFiltradas = dataTableActual.Select(
            "NC_MotivoDeRefactura <> '' OR NC_MotivoDeDebito <> ''"
        );

        // 2. Itera únicamente sobre el subconjunto de filas filtradas
        foreach (DataRow row in filasFiltradas)
        {
            int idPrestacion = Convert.ToInt32(row["id_prestacion"]);

            object motivoRefactura = row["NC_MotivoDeRefactura"] != DBNull.Value
                ? row["NC_MotivoDeRefactura"]
                : "";

            object motivoDebito = row["NC_MotivoDeDebito"] != DBNull.Value
                ? row["NC_MotivoDeDebito"]
                : "";

            double? importeRefactura = row["NC_ImporteDeRefactura"] != DBNull.Value
                ? Convert.ToDouble(row["NC_ImporteDeRefactura"])
                : (double?)null;

            double? importeDebito = row["NC_ImporteDebitado"] != DBNull.Value
                ? Convert.ToDouble(row["NC_ImporteDebitado"])
                : (double?)null;

            string? comentarios = row["NC_Comentarios"] != DBNull.Value
                      ? row["NC_Comentarios"].ToString().Replace('\0', ' ').Trim() // Reemplazar \0 por un espacio
                      : "";

            bool debitoAceptado = row["NC_DebitoAceptado"] != DBNull.Value &&
                                  Convert.ToBoolean(row["NC_DebitoAceptado"]);

            object diasFacturados = DBNull.Value;
            if (FacturaTipo != "NC" && row["NC_DiasFacturados"] != DBNull.Value)
            {
                diasFacturados = Convert.ToInt32(row["NC_DiasFacturados"]);
            }

            string prestacionEnglobante = row["NC_PrestacionEnglobante"] != DBNull.Value ? row["NC_PrestacionEnglobante"].ToString() : "";

            string codigo = string.Empty;
            if (FacturaTipo == "ND" && row["codigo"] != DBNull.Value)
            {
                codigo = row["codigo"].ToString();
            }

            // Buscar si ya existe en la lista de valores para borrado de filtros
            // Nota: Se puede optimizar esta parte con un Dictionary si la lista es grande
            int index = listaValoresParaBorradoDeFiltros.FindIndex(x => x.idPrestacion == idPrestacion);

            var nuevoElemento = (idPrestacion, motivoRefactura, motivoDebito, importeRefactura, importeDebito, comentarios, debitoAceptado, diasFacturados, prestacionEnglobante, codigo);

            if (index == -1)
            {
                listaValoresParaBorradoDeFiltros.Add(nuevoElemento);
            }
            else
            {
                listaValoresParaBorradoDeFiltros[index] = nuevoElemento;
            }
        }
    }

    private void restaurarValoresPreviosAFiltro()
    {
        // Pausar el redibujado de la grilla
        dataGridView1.SuspendLayout();

        try
        {
            // 1. Crear un Dictionary para una búsqueda rápida
            var diccionarioValores = listaValoresParaBorradoDeFiltros.ToDictionary(x => x.idPrestacion);

            // 2. Recorrer la grilla una sola vez
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                int idPrestacion = Convert.ToInt32(row.Cells["id_prestacion"].Value);

                // 3. Usar el Dictionary para una búsqueda instantánea
                if (diccionarioValores.TryGetValue(idPrestacion, out var elemento))
                {
                    // Si se encuentra, restaurar los valores
                    row.Cells["NC_MotivoDeRefactura"].Value = elemento.motivoRefactura ?? DBNull.Value;
                    row.Cells["NC_MotivoDeDebito"].Value = elemento.motivoDebito ?? DBNull.Value;
                    row.Cells["NC_ImporteDeRefactura"].Value = elemento.importeRefactura;
                    row.Cells["NC_ImporteDebitado"].Value = elemento.importeDebito;
                    row.Cells["NC_Comentarios"].Value = elemento.comentarios;
                    row.Cells["NC_DebitoAceptado"].Value = elemento.debitoAceptado;
                    if (FacturaTipo != "NC")
                    {
                        row.Cells["NC_DiasFacturados"].Value = elemento.diasFacturados ?? DBNull.Value;
                    }
                    row.Cells["NC_PrestacionEnglobante"].Value = elemento.prestacionEnglobante;
                }
            }
        }
        finally
        {
            // Reanudar el redibujado de la grilla
            dataGridView1.ResumeLayout();
        }
    }

    private void GuardarValoresAntesDeDeshacerFiltroNC()
    {
        // Obtener el DataTable subyacente del DataGridView.
        // Esto es más rápido que recorrer las filas de la UI.
        DataTable? dataTableActual = null;
        if (dataGridView1.DataSource is BindingSource bs)
        {
            dataTableActual = bs.DataSource as DataTable;
        }
        else if (dataGridView1.DataSource is DataTable dt)
        {
            dataTableActual = dt;
        }

        if (dataTableActual == null)
        {
            // No hay un DataTable, la lógica de guardado no puede proceder.
            return;
        }

        // Usar la función Select para filtrar solo las filas que tienen un valor en la columna
        // "ND_MotivoDeRefactura". Esta es la condición original del 'if' en el foreach.
        DataRow[] filasFiltradas = dataTableActual.Select(
            "ND_MotivoDeRefactura IS NOT NULL AND ND_MotivoDeRefactura <> ''"
        );

        // Iterar sobre el subconjunto de filas filtradas para mayor eficiencia.
        foreach (DataRow row in filasFiltradas)
        {
            int idPrestacion = Convert.ToInt32(row["id"]);
            object motivoRefactura = row["ND_MotivoDeRefactura"] != DBNull.Value ? row["ND_MotivoDeRefactura"] : null;

            double importeRefactura = row["ND_ImporteDeRefactura"] != DBNull.Value ? Convert.ToDouble(row["ND_ImporteDeRefactura"]) : 0.0;

            string? comentarios = row["ND_Comentarios"] != DBNull.Value
                      ? row["NC_Comentarios"].ToString().Replace('\0', ' ').Trim() // Reemplazar \0 por un espacio
                      : "";

            // Nota: En el código original se volvía a obtener "id" como "idNotaDeCredito".
            // Lo mantengo, pero es redundante ya que es el mismo valor que "idPrestacion".
            int idNotaDeCredito = Convert.ToInt32(row["id"]);

            string? codigo = row["codigo"] != DBNull.Value ? row["codigo"].ToString() : "";

            // Buscar si ya existe en la lista para evitar duplicados y actualizar.
            int index = listaValoresParaBorradoDeFiltrosNC.FindIndex(x => x.idPrestacion == idPrestacion);

            // Crear una tupla para almacenar los valores.
            var nuevoElemento = (idPrestacion, motivoRefactura, importeRefactura, comentarios, idNotaDeCredito, codigo);

            if (index == -1)
            {
                // No existe, lo agregamos.
                listaValoresParaBorradoDeFiltrosNC.Add(nuevoElemento);
            }
            else
            {
                // Ya existe, lo actualizamos.
                listaValoresParaBorradoDeFiltrosNC[index] = nuevoElemento;
            }
        }
    }

    private void GuardarValoresAntesDeDeshacerFiltroND()
    {
        // Obtener el DataTable que actúa como fuente de datos del DataGridView.
        // Esto es mucho más eficiente que recorrer las filas de la UI.
        DataTable? dataTableActual = null;
        if (dataGridView1.DataSource is BindingSource bs)
        {
            dataTableActual = bs.DataSource as DataTable;
        }
        else if (dataGridView1.DataSource is DataTable dt)
        {
            dataTableActual = dt;
        }

        if (dataTableActual == null)
        {
            // Si no se encuentra un DataTable, no podemos continuar.
            return;
        }

        // Usar Select para filtrar las filas que cumplen la condición original
        // del método. Esto es más rápido que un bucle 'foreach'.
        DataRow[] filasFiltradas = dataTableActual.Select(
            "NC_MotivoDeRefactura IS NOT NULL AND NC_MotivoDeRefactura <> '' OR " +
            "NC_MotivoDeDebito IS NOT NULL AND NC_MotivoDeDebito <> ''"
        );

        // Iterar solo sobre el subconjunto de filas filtradas.
        foreach (DataRow row in filasFiltradas)
        {
            int idPrestacion = Convert.ToInt32(row["id_prestacion"]);

            object motivoRefactura = row["NC_MotivoDeRefactura"] != DBNull.Value
                ? row["NC_MotivoDeRefactura"]
                : "";

            object motivoDebito = row["NC_MotivoDeDebito"] != DBNull.Value
                ? row["NC_MotivoDeDebito"]
                : "";

            double? importeRefactura = row["NC_ImporteDeRefactura"] != DBNull.Value
                ? Convert.ToDouble(row["NC_ImporteDeRefactura"])
                : (double?)null;

            double? importeDebito = row["NC_ImporteDebitado"] != DBNull.Value
                ? Convert.ToDouble(row["NC_ImporteDebitado"])
                : (double?)null;

            string? comentarios = row["NC_Comentarios"] != DBNull.Value
                      ? row["NC_Comentarios"].ToString().Replace("\0", "") // Eliminar el caracter nulo
                      : "";

            bool debitoAceptado = row["NC_DebitoAceptado"] != DBNull.Value &&
                                  Convert.ToBoolean(row["NC_DebitoAceptado"]);

            object diasFacturados = row["NC_DiasFacturados"] != DBNull.Value
                ? Convert.ToInt32(row["NC_DiasFacturados"])
                : (object)DBNull.Value;

            string? prestacionEnglobante = row["NC_PrestacionEnglobante"] != DBNull.Value ? row["NC_PrestacionEnglobante"].ToString() : "";

            // En el código original se usa id para idNotaDeDebito, respetamos el nombre del campo.
            int? idNotaDeDebito = Convert.ToInt32(row["id"]);

            // Buscar si ya existe el elemento en la lista.
            int index = listaValoresParaBorradoDeFiltrosND.FindIndex(x => x.idPrestacion == idPrestacion);

            // Se crea la tupla con los valores extraídos.
            var nuevoElemento = (idPrestacion, motivoRefactura, motivoDebito, importeRefactura, importeDebito, comentarios, debitoAceptado, diasFacturados, prestacionEnglobante, idNotaDeDebito);

            if (index == -1)
            {
                // Si el elemento no existe, se añade.
                listaValoresParaBorradoDeFiltrosND.Add(nuevoElemento);
            }
            else
            {
                // Si ya existe, se actualiza en su posición.
                listaValoresParaBorradoDeFiltrosND[index] = nuevoElemento;
            }
        }
    }

    private void GuardarValoresAntesDeOrdenarNC()
    {
        listaValores.Clear();

        switch (FacturaTipo)
        {
            case "FC":
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells["id_prestacion"].Value != null && row.Cells["ND_MotivoDeRefactura"].Value != null && row.Cells["ND_MotivoDeDebito"].Value != null)
                    {
                        int idPrestacion = Convert.ToInt32(row.Cells["id_prestacion"].Value);
                        object motivoRefactura = row.Cells["ND_MotivoDeRefactura"].Value;
                        object motivoDebito = row.Cells["ND_MotivoDeDebito"].Value;
                        double importeRefactura = row.Cells["NC_ImporteRefactura"].Value == DBNull.Value ? 0.0 : Convert.ToDouble(row.Cells["NC_ImporteRefactura"].Value);
                        double importeDebito = row.Cells["NC_ImporteDebitado"].Value == DBNull.Value ? 0.0 : Convert.ToDouble(row.Cells["NC_ImporteDebitado"].Value);
                        listaValores.Add((idPrestacion, motivoRefactura, motivoDebito, importeRefactura, importeDebito));
                        valoresOriginales.Add((idPrestacion, motivoRefactura, motivoDebito));
                    }
                }
                break;
            case "NC":
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells["id_prestacion"].Value != null && row.Cells["ND_MotivoDeRefactura"].Value != null)
                    {
                        int idPrestacion = Convert.ToInt32(row.Cells["id_prestacion"].Value);
                        object motivoRefactura = row.Cells["ND_MotivoDeRefactura"].Value;
                        listaValoresNC.Add((idPrestacion, motivoRefactura));
                        valoresOriginalesNC.Add((idPrestacion, motivoRefactura));
                    }
                }
                break;
            case "ND":
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells["id_prestacion"].Value != null && row.Cells["ND_MotivoDeRefactura"].Value != null && row.Cells["ND_MotivoDeDebito"].Value != null)
                    {
                        int idPrestacion = Convert.ToInt32(row.Cells["id_prestacion"].Value);
                        object motivoRefactura = row.Cells["ND_MotivoDeRefactura"].Value;
                        object motivoDebito = row.Cells["ND_MotivoDeDebito"].Value;
                        double importeRefactura = row.Cells["NC_ImporteRefactura"].Value == DBNull.Value ? 0.0 : Convert.ToDouble(row.Cells["NC_ImporteRefactura"].Value);
                        double importeDebito = row.Cells["NC_ImporteDebitado"].Value == DBNull.Value ? 0.0 : Convert.ToDouble(row.Cells["NC_ImporteDebitado"].Value);
                        listaValores.Add((idPrestacion, motivoRefactura, motivoDebito, importeRefactura, importeDebito));
                        valoresOriginales.Add((idPrestacion, motivoRefactura, motivoDebito));
                    }
                }
                break;
        }

    }

    private void RestaurarValoresDespuesDeOrdenar()
    {
        foreach (DataGridViewRow row in dataGridView1.Rows)
        {
            if (row.Cells["id_prestacion"].Value != null)
            {
                int idPrestacion = Convert.ToInt32(row.Cells["id_prestacion"].Value);

                // Buscar el valor correspondiente en la lista almacenada
                var item = listaValores.FirstOrDefault(x => x.idPrestacion == idPrestacion);

                // Restaurar el valor si existe en la lista almacenada
                if (item.idPrestacion == idPrestacion)
                {
                    cargaPrimeraVez = true;
                    row.Cells["NC_MotivoDeRefactura"].Value = item.motivoRefactura;
                    cargaPrimeraVez = true;
                    row.Cells["NC_MotivoDeDebito"].Value = item.motivoDebito;
                    cargaPrimeraVez = false;
                }
            }
        }
    }

    private void RestaurarValoresDespuesDeOrdenarNC()
    {
        foreach (DataGridViewRow row in dataGridView1.Rows)
        {
            if (row.Cells["id_prestacion"].Value != null)
            {
                int idPrestacion = Convert.ToInt32(row.Cells["id_prestacion"].Value);

                // Buscar el valor correspondiente en la lista almacenada
                var item = listaValores.FirstOrDefault(x => x.idPrestacion == idPrestacion);

                // Restaurar el valor si existe en la lista almacenada
                if (item.idPrestacion == idPrestacion)
                {
                    cargaPrimeraVez = true;
                    row.Cells["ND_MotivoDeRefactura"].Value = item.motivoRefactura;
                    cargaPrimeraVez = true;
                    row.Cells["ND_MotivoDeDebito"].Value = item.motivoDebito;
                }
            }
        }
    }

    private void DataGridView1_Sorted(object sender, EventArgs e)
    {
        switch (FacturaTipo)
        {
            case "FC":
                RestaurarValoresDespuesDeOrdenar();
                break;
            case "NC":
                RestaurarValoresDespuesDeOrdenarNC();
                break;
            case "ND":
                RestaurarValoresDespuesDeOrdenar();
                break;
        }
    }

    private void filtroNumeroDeInternacion_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cargaListaNumeroDeInternacion || filtroNumeroDeInternacion.SelectedIndex <= 0) return;
        GuardarValoresAntesDeDeshacerFiltro();
        lblNumeroDeInternacionSel.Text = "Número de internación: " + filtroNumeroDeInternacion.Text;
        lblNumeroDeInternacionSel.Visible = true;
        filtroNumeroDeInternacion.Visible = false;
        AplicarFiltrosActivos();
    }

    private void button1_Click(object sender, EventArgs e)
    {
        verHistorialDelDocumento = new VerHistorialDelDocumento(FacturaNumero, FacturaLetra, FacturaPuntoDeVenta, FacturaTipo);
        verHistorialDelDocumento.Show();
    }

    private void dataGridView1_CellFormatting_1(object sender, DataGridViewCellFormattingEventArgs e)
    {
        if (controlFormatting)
        {
            // Verificar si la celda pertenece a una columna numérica
            if (dataGridView1.Columns[e.ColumnIndex].ValueType == typeof(decimal) ||
                dataGridView1.Columns[e.ColumnIndex].ValueType == typeof(double) ||
                dataGridView1.Columns[e.ColumnIndex].ValueType == typeof(float))
            {
                if (e.Value != null && e.Value != DBNull.Value)
                {
                    e.Value = string.Format("{0:0.00}", e.Value); // Mostrar con 2 decimales
                    e.FormattingApplied = true; // Indicar que el formato fue aplicado
                }
            }
        }

    }

    private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
    {
        if ((dataGridView1.Columns[e.ColumnIndex].Name == "nc_importederefactura") ||
            (dataGridView1.Columns[e.ColumnIndex].Name == "nc_importedebitado"))
        {
            cargaPrimeraVez = true;
            dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = DBNull.Value;
            dataGridView1.Refresh();
            e.ThrowException = false;
        }
    }

    private void btnBorrarImporteDebito_Click(object sender, EventArgs e)
    {
        if (dataGridView1.SelectedCells.Count > 0)
        {
            // Iterar sobre las celdas seleccionadas en el DataGridView
            foreach (DataGridViewCell celda in dataGridView1.SelectedCells)
            {
                // Verificar si la celda está en la columna "motivodedebito"
                if (dataGridView1.Columns[celda.ColumnIndex].Name == "nc_importedebitado")
                {
                    cargaPrimeraVez = true;
                    dataGridView1.Rows[celda.RowIndex].Cells[celda.ColumnIndex].Value = DBNull.Value;
                }
            }
        }
        else
        {
            MessageBox.Show("Por favor seleccione las celdas cuyo valor desea borrar.");
        }
    }

    private void btnBorrarImporteRefactura_Click(object sender, EventArgs e)
    {
        if (dataGridView1.SelectedCells.Count > 0)
        {
            // Iterar sobre las celdas seleccionadas en el DataGridView
            foreach (DataGridViewCell celda in dataGridView1.SelectedCells)
            {
                // Verificar si la celda está en la columna "motivodedebito"
                if (dataGridView1.Columns[celda.ColumnIndex].Name == "nc_importederefactura")
                {
                    cargaPrimeraVez = true;
                    dataGridView1.Rows[celda.RowIndex].Cells[celda.ColumnIndex].Value = DBNull.Value;
                }
            }
        }
        else
        {
            MessageBox.Show("Por favor seleccione las celdas cuyo valor desea borrar.");
        }
    }

    private void SetControlesVisibles(bool visible)
    {
        checkPrestacionesSinRefactura.Visible = visible;
        filtroPaciente.Visible = visible;
        filtroProfesional.Visible = visible;
        filtroNumeroDeInternacion.Visible = visible;
        if (TipoRegistroFiltrado == "Ambulatorios")
            filtroNumeroDeInternacion.Visible = false;
        filtroPrestacion.Visible = visible;
        filtroMotivoDeRefactura.Visible = visible;
        if (visible)
        {
            if (FacturaTipo == "NC")
            {
                filtroMotivoDebito.Visible = !visible;
                checkPrestacionesSinDebito.Visible = !visible;
                filtroDebitoAceptado.Visible = !visible;
                checkMotivoDebito.Visible = !visible;
                label6.Visible = !visible;
                checkDebitoAceptado.Visible = !visible;
                label1.Visible = !visible;
            }
            else
            {
                filtroMotivoDebito.Visible = visible;
                checkPrestacionesSinDebito.Visible = visible;
                filtroDebitoAceptado.Visible = visible;
                checkMotivoDebito.Visible = visible;
                label6.Visible = visible;
                checkDebitoAceptado.Visible = visible;
                label1.Visible = visible;
            }
        }
        else
        {
            filtroMotivoDebito.Visible = visible;
            checkPrestacionesSinDebito.Visible = visible;
            filtroDebitoAceptado.Visible = visible;
            checkMotivoDebito.Visible = visible;
            label6.Visible = visible;
            checkDebitoAceptado.Visible = visible;
            label1.Visible = visible;
        }

        lblCantidadDeRegistrosFiltrados.Visible = visible;
        filtroModulo.Visible = visible;
        if (TipoRegistroFiltrado == "Ambulatorios")
            filtroModulo.Visible = false;
        button1.Visible = visible;
        label2.Visible = visible;
        comboFiltroFecha.Visible = visible;
        lblFecSel.Visible = visible;
        checkMotivoDeRefactura.Visible = visible;
        btnExportar.Visible = visible;
        btnBorrarCelda.Visible = visible;
        btnNuevaNotaDeCrédito.Visible = visible;
        btnNuevaNotaDeDébito.Visible = visible;
        btnLimpiarFila.Visible = visible;
        btnGuardarParcialmente.Visible = visible;
        soloPrestacionesValorizadas.Visible = visible;
        filtroGrupoPrestacion.Visible = visible;
        btnBorrarImporteDebito.Visible = visible;
        btnBorrarImporteRefactura.Visible = visible;
        lblCantidadDeRegistrosConDebitoAceptado.Visible = visible;
        lblPacSel.Visible = visible;
        lblPrestSel.Visible = visible;
        lblProfSel.Visible = visible;
        lblNumeroDeInternacionSel.Visible = visible;
        panel1.Visible = visible;
        if (TipoRegistroFiltrado == "Ambulatorios")
            panel1.Visible = false;
        if (TipoRegistroFiltrado == "Ambulatorios")
            lblNumeroDeInternacionSel.Visible = false;
        lblModulo.Visible = visible;
        if (TipoRegistroFiltrado == "Ambulatorios")
            lblModulo.Visible = false;

    }

    private void RecargarFiltroGenerico(DataTable dtFiltrado, string columna, ComboBox combo, List<DataTable> listaFiltros, string displayName)
    {
        var dtUnico = new DataTable();
        dtUnico.Columns.Add(columna, typeof(string));

        // Agregamos el texto por defecto para que sea el índice 0 (placeholder)
        dtUnico.Rows.Add(displayName);

        var valoresUnicos = new HashSet<string>();
        foreach (DataRow fila in dtFiltrado.Rows)
        {
            if (!fila.IsNull(columna))
            {
                string valor = fila[columna].ToString();
                if (!string.IsNullOrWhiteSpace(valor))
                    valoresUnicos.Add(valor);
            }
        }

        foreach (var valor in valoresUnicos.OrderBy(x => x, StringComparer.CurrentCultureIgnoreCase))
        {
            dtUnico.Rows.Add(valor);
        }

        listaFiltros.Add(dtUnico);
        evitarErrores(columna);
        combo.DataSource = dtUnico;
        evitarErrores(columna);
        combo.DisplayMember = columna;
        combo.ValueMember = columna;
        combo.SelectedIndex = 0; // Seleccionar el placeholder por defecto
    }

    private void evitarErrores(string nombreColumna)
    {
        switch (nombreColumna.ToLower())
        {
            case "paciente": cargaListaPaciente = true; break;
            case "nro_internacion":
            case "nro_int": cargaListaNumeroDeInternacion = true; break;
            case "medico": cargaListaProfesional = true; break;
            case "codigo": cargaListaPrestacion = true; break;
            case "modulo": cargaListaModulo = true; break;
        }
    }

    private bool TryParseInt(TextBox textBox, out int value, string mensajeError)
    {
        if (int.TryParse(textBox.Text, out value))
        {
            return true;
        }
        else
        {
            MessageBox.Show(mensajeError);
            textBox.Text = "";
            value = 0;
            return false;
        }
    }

    private bool TryParseLetra(TextBox textBox, out string letra, string mensajeError)
    {
        letra = textBox.Text.Trim().ToUpper();
        if (!string.IsNullOrEmpty(letra) && letra.All(char.IsLetter))
        {
            return true;
        }
        else
        {
            MessageBox.Show(mensajeError);
            textBox.Text = "";
            letra = "";
            return false;
        }
    }

    private void ActualizarFiltrosDisponibles(DataTable dataTableFiltrada)
    {
        // Limpiamos listas para evitar acumulación
        tablasFiltrosPaciente.Clear();
        tablasFiltrosPrestacion.Clear();
        tablasFiltrosMedico.Clear();
        tablasFiltrosModulo.Clear();
        tablasFiltrosNumeroDeInternacion.Clear();
        tablasFiltrosFecha.Clear();

        RecargarFiltroGenerico(dataTableFiltrada, "paciente", filtroPaciente, tablasFiltrosPaciente, "Paciente");
        RecargarFiltroGenerico(dataTableFiltrada, "codigo", filtroPrestacion, tablasFiltrosPrestacion, "Prestación");
        RecargarFiltroGenerico(dataTableFiltrada, "medico", filtroProfesional, tablasFiltrosMedico, "Profesional");

        if (dataTableFiltrada.Columns.Contains("modulo"))
            RecargarFiltroGenerico(dataTableFiltrada, "modulo", filtroModulo, tablasFiltrosModulo, "Módulo");

        string colNroInternacion = dataTableFiltrada.Columns.Contains("nro_internacion") ? "nro_internacion" :
                                   dataTableFiltrada.Columns.Contains("nro_int") ? "nro_int" : null;
        if (colNroInternacion != null)
            RecargarFiltroGenerico(dataTableFiltrada, colNroInternacion, filtroNumeroDeInternacion, tablasFiltrosNumeroDeInternacion, "N° de internación");

        if (dataTableFiltrada.Columns.Contains("fecha"))
        {
            var dtUnico = new DataTable();
            dtUnico.Columns.Add("fecha", typeof(string));
            dtUnico.Rows.Add("Fecha");

            var fechasUnicas = new HashSet<string>();
            foreach (DataRow fila in dataTableFiltrada.Rows)
            {
                if (!fila.IsNull("fecha"))
                    fechasUnicas.Add(Convert.ToDateTime(fila["fecha"]).ToString("dd/MM/yyyy"));
            }
            foreach (var fecha in fechasUnicas.OrderBy(x => x))
                dtUnico.Rows.Add(fecha);

            tablasFiltrosFecha.Add(dtUnico);
            cargaListaFecha = true;
            comboFiltroFecha.DataSource = dtUnico;
            comboFiltroFecha.DisplayMember = "fecha";
            comboFiltroFecha.ValueMember = "fecha";
            comboFiltroFecha.SelectedIndex = 0;
            cargaListaFecha = false;
        }
    }

    private void btnBorrarCelda_Click(object sender, EventArgs e)
    {
        if (dataGridView1.SelectedCells.Count == 0)
        {
            MessageBox.Show("Por favor, seleccione una o más celdas en el DataGridView para aplicar el valor.");
            return;
        }

        switch (FacturaTipo)
        {
            case "FC":
                borrarCeldaFC();
                break;
            case "NC":
                borrarCeldaNC();
                break;
            case "ND":
                borrarCeldaFC();
                break;
        }
        dataGridView1.Refresh();
    }

    private void borrarCeldaFC()
    {
        // Lista de nombres de columnas a borrar
        var columnasABorrar = new HashSet<string>
        {
            "nc_comentarios",
            "nc_prestacionenglobante",
            "nc_importederefactura",
            "nc_motivoderefactura",
            "nc_importedebitado",
            "nc_diasfacturados",
            "nc_motivodedebito",
            "nc_debitoaceptado"
        };

        foreach (DataGridViewCell celda in dataGridView1.SelectedCells)
        {
            string nombreColumna = dataGridView1.Columns[celda.ColumnIndex].Name;
            if (columnasABorrar.Contains(nombreColumna))
            {
                if (nombreColumna == "nc_debitoaceptado")
                {
                    celda.Value = false;
                }
                else
                {
                    celda.Value = DBNull.Value;

                    // Si se borra nc_motivoderefactura, también borrar nc_importederefactura
                    if (nombreColumna == "nc_motivoderefactura")
                    {
                        dataGridView1.Rows[celda.RowIndex].Cells["nc_importederefactura"].Value = DBNull.Value;
                        dataGridView1.Rows[celda.RowIndex].Cells["nc_comentarios"].ReadOnly = true;
                        dataGridView1.Rows[celda.RowIndex].Cells["nc_comentarios"].Style.BackColor = System.Drawing.Color.Coral;
                    }
                    // Si se borra nc_motivodedebito, también borrar nc_importedebitado
                    if (nombreColumna == "nc_motivodedebito")
                    {
                        dataGridView1.Rows[celda.RowIndex].Cells["nc_importedebitado"].Value = DBNull.Value;
                    }
                }
            }
        }
    }

    private void borrarCeldaNC()
    {
        // Lista de nombres de columnas a borrar
        var columnasABorrar = new HashSet<string>
        {
            "nd_motivoderefactura",
            "nd_importederefactura",
            "nd_comentarios"
        };

        foreach (DataGridViewCell celda in dataGridView1.SelectedCells)
        {
            string nombreColumna = dataGridView1.Columns[celda.ColumnIndex].Name;
            if (columnasABorrar.Contains(nombreColumna))
            {
                celda.Value = DBNull.Value;

                // Si se borra nc_motivoderefactura, también borrar nc_importederefactura
                if (nombreColumna == "nd_motivoderefactura")
                {
                    dataGridView1.Rows[celda.RowIndex].Cells["nd_importederefactura"].Value = DBNull.Value;
                    dataGridView1.Rows[celda.RowIndex].Cells["nd_comentarios"].ReadOnly = true;
                    dataGridView1.Rows[celda.RowIndex].Cells["nd_comentarios"].Style.BackColor = System.Drawing.Color.Coral;
                }
            }
        }
    }

}