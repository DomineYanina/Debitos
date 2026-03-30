using ClosedXML.Excel;
using Debitos.Models;
using Debitos.Presenters;
using Debitos.Repositories;
using Debitos.Views;
using System.Data;

namespace Debitos;

public partial class Form1 : Form, IPrestacionesView // <-- Acá agregamos la interfaz
{
    private PrestacionesPresenter _presenter; // <-- El presentador
    private DebitosRepository _repository;
    public event EventHandler BuscarDocumentoEvent; // <-- El evento que pide la interfaz
    public event EventHandler GuardarParcialmenteEvent;
    public event EventHandler GenerarNotaDeCreditoEvent;
    private IngresoInformacionNotaDeCredito ingresoInformacionNotaDeCredito;

    public string TextoTotalRegistros
    {
        set => lblMontoTotalRegistrosEnPantalla.Text = value;
    }

    public string TextoMontosNoAceptados
    {
        set => lblMontosNoAceptados.Text = value;
    }

    public bool VisibilidadTotales
    {
        set
        {
            lblMontoTotalRegistrosEnPantalla.Visible = value;
            lblMontosNoAceptados.Visible = value;
        }
    }

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

    public string TipoRegistroFiltrado { get; set; } = "";
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

    private DataTable filtroPacienteOriginal;
    private DataTable filtroPrestacionOriginal;
    private DataTable filtroMedicoOriginal;
    private DataTable filtroModuloOriginal;
    private DataTable filtroNumeroDeInternacionOriginal;
    private DataTable filtroFechaOriginal;

    public List<string> ordenFiltros = new List<string>();
    public List<DataTable> tablasFiltradas = new List<DataTable>();

    public List<int> listaPrestacionesYaExistentes = new List<int>();

    private string condicionesFiltro = "";
    
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
    private List<(int idPrestacion, object importeRefactura)> listaValoresParaImporteDeRefactura = new List<(int, object)>();

    // Estructura para almacenar los datos antes de ordenar
    private List<(int idPrestacion, object importeRefactura)> listaValoresParaImporteDeDebito = new List<(int, object)>();
    public Form1(String _usuario)
    {
        InitializeComponent();
        usuario = _usuario;

        dataGridView1.DoubleBuffered(true);
        dataGridView1.AllowUserToResizeColumns = true;

        // Initialize non-nullable fields to default values to satisfy CS8618  
        pacienteFiltro = string.Empty;
        prestacionFiltro = string.Empty;
        profesionalFiltro = string.Empty;
        grupoPrestacionFiltro = string.Empty;
        ingresoInformacionNotaDeCredito = new IngresoInformacionNotaDeCredito(false, 0, string.Empty, 0, string.Empty, usuario);
        ingresoInformacionNotaDeDebito = new IngresoInformacionNotaDeDebito(false, 0, string.Empty, 0, string.Empty, usuario);
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
        _repository = new DebitosRepository(DatabaseConfig.ConnectionString); // <-- Guardamos en la variable global
                                                                              // Al final del constructor, actualizá esta línea:
        _presenter = new PrestacionesPresenter(this, _repository, _usuario);

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

        tablasFiltradas.Clear();

        condicionesFiltro = "";
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

        if (TipoRegistroFiltrado == TipoRegistro.Internados)
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

        _presenter.RecalcularTotales();
        dataGridView1.ResumeLayout();
    }

    private void RestaurarValoresPorTipoFactura()
    {
        switch (FacturaTipo)
        {
            case TipoDocumento.Factura:
                GuardarValoresAntesDeDeshacerFiltro();
                break;
            case TipoDocumento.NotaCredito:
                GuardarValoresAntesDeDeshacerFiltroNC();
                break;
            case TipoDocumento.NotaDebito:
                GuardarValoresAntesDeDeshacerFiltro();
                break;
        }
    }

    private void InicializarFiltrosDesdeMemoria(DataTable datos)
    {
        ActualizarFiltrosDisponibles(datos.DefaultView);

        filtroPacienteOriginal = (DataTable)filtroPaciente.DataSource;
        filtroPrestacionOriginal = (DataTable)filtroPrestacion.DataSource;
        filtroMedicoOriginal = (DataTable)filtroProfesional.DataSource;
        filtroModuloOriginal = (DataTable)filtroModulo.DataSource;
        filtroNumeroDeInternacionOriginal = (DataTable)filtroNumeroDeInternacion.DataSource;
        filtroFechaOriginal = comboFiltroFecha.DataSource as DataTable;

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
        filtroNumeroDeInternacion.Visible = TipoRegistroFiltrado == TipoRegistro.Internados;

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

    private void ActualizarCantidadDeRegistrosFiltrados()
    {
        lblCantidadDeRegistrosFiltrados.Text = "Cantidad de registros filtrados: " + bindingSource.Count;
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
            if (FacturaTipo == TipoDocumento.Factura || FacturaTipo == TipoDocumento.NotaDebito)
                filtros.Add("(nc_motivoderefactura IS NULL OR nc_motivoderefactura = '')");
            else if (FacturaTipo == TipoDocumento.NotaCredito)
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
        _presenter.RecalcularTotales();

        DataView vistaFiltrada = new DataView(dt);
        vistaFiltrada.RowFilter = filtroFinal;
        ActualizarFiltrosDisponibles(vistaFiltrada);

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
            case TipoDocumento.Factura:
                colorearColumnasFC();
                filtroMotivoDebito.Visible = true;
                checkMotivoDebito.Visible = true;
                label6.Visible = true;
                break;
            case TipoDocumento.NotaCredito:
                colorearColumnasNC();
                filtroMotivoDebito.Visible = false;
                checkMotivoDebito.Visible = false;
                label6.Visible = false;
                break;
            case TipoDocumento.NotaDebito:
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

    private void filtroPrestacion_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cargaListaPrestacion || filtroPrestacion.SelectedIndex <= 0) return;
        GuardarValoresAntesDeDeshacerFiltro();
        lblPrestSel.Text = "Prestación: " + filtroPrestacion.Text;
        lblPrestSel.Visible = true;
        filtroPrestacion.Visible = false;
        AplicarFiltrosActivos();
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
        dataGridView1.AllowUserToResizeColumns = true;
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
            if (config.Width.HasValue)
            {
                // Al establecer None, permitimos que el usuario arrastre el ancho con el mouse
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                col.Width = config.Width.Value;
            }

            if ((FacturaTipo == TipoDocumento.Factura && config.Name == "nc_comentarios") || (FacturaTipo == TipoDocumento.NotaDebito && config.Name == "nc_comentarios") || (FacturaTipo == TipoDocumento.NotaCredito && config.Name == "nd_comentarios"))
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

            if (config.Name == "paciente" && FacturaTipo == TipoDocumento.NotaCredito)
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
        if (TipoRegistroFiltrado == TipoRegistro.Internados)
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
        if (TipoRegistroFiltrado == TipoRegistro.Internados)
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
        if (TipoRegistroFiltrado == TipoRegistro.Internados)
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

    private void filtroMotivoDebito_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cargarSoloFiltroMotivoDebito == false)
        {
            if (filtroMotivoDebito.SelectedIndex != -1)
            {
                string motivoDebito = filtroMotivoDebito.SelectedItem.ToString();

                // 1. Verificamos si alguna de las celdas seleccionadas ya tiene un motivo asignado
                bool hayMotivosPrevios = false;
                foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
                {
                    var row = dataGridView1.Rows[cell.RowIndex];
                    string motivoActual = row.Cells["NC_MotivoDeDebito"].Value?.ToString();

                    if (!string.IsNullOrWhiteSpace(motivoActual))
                    {
                        hayMotivosPrevios = true;
                        break; // Apenas encontramos uno, detenemos la búsqueda
                    }
                }

                bool reemplazarMotivosPrevios = false;

                // 2. Si encontramos motivos previos, pedimos confirmación al usuario
                if (hayMotivosPrevios)
                {
                    DialogResult resultado = MessageBox.Show(
                        "Algunas de las prestaciones seleccionadas ya tienen un motivo de débito asignado. ¿Desea reemplazarlo?\n\nSeleccione 'Sí' para reemplazar todos, o 'No' para aplicar el motivo solo a las celdas vacías.",
                        "Confirmación de reemplazo",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    reemplazarMotivosPrevios = (resultado == DialogResult.Yes);
                }

                // 3. Iteramos nuevamente para aplicar el valor según la decisión tomada
                foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
                {
                    var row = dataGridView1.Rows[cell.RowIndex];
                    string motivoActual = row.Cells["NC_MotivoDeDebito"].Value?.ToString();

                    // Aplicamos el nuevo motivo si la celda está vacía, o si el usuario eligió sobreescribir todo
                    if (string.IsNullOrWhiteSpace(motivoActual) || reemplazarMotivosPrevios)
                    {
                        row.Cells["NC_MotivoDeDebito"].Value = motivoDebito;
                    }
                }

                checkMotivoDebito.Checked = false;
                filtroMotivoDebito.SelectedItem = null;
                GuardarValoresParaActualizarMontoAuditados();

                // Es importante detener la edición para que el valor quede fijado visualmente
                dataGridView1.EndEdit();
                _presenter.RecalcularTotales();
            }
        }
        cargarSoloFiltroMotivoDebito = false;
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

        if ((FacturaTipo == TipoDocumento.Factura || FacturaTipo == TipoDocumento.NotaDebito) && EsColumna(fila, e.ColumnIndex, "NC_MotivoDeRefactura"))
            ProcesarCambioMotivoDeRefactura(fila);

        if ((FacturaTipo == TipoDocumento.Factura || FacturaTipo == TipoDocumento.NotaDebito) && (EsColumna(fila, e.ColumnIndex, "NC_ImporteDeRefactura") || EsColumna(fila, e.ColumnIndex, "NC_ImporteDebitado")))
            ValidarYActualizarImporte(fila, e.ColumnIndex);

        if ((FacturaTipo == TipoDocumento.Factura || FacturaTipo == TipoDocumento.NotaDebito) && EsColumna(fila, e.ColumnIndex, "NC_DiasFacturados"))
            ProcesarCambioDiasFacturados(fila);

        if (FacturaTipo == TipoDocumento.NotaCredito && EsColumna(fila, e.ColumnIndex, "ND_MotivoDeRefactura"))
            ProcesarCambioMotivoDeRefacturaNC(fila);

        ProcesarReadOnlyYEstilosPorDebitoAceptado(fila, e.ColumnIndex);

        cargaPrimeraVez = false;
        dataGridView1.Refresh();
        if (debitoIndividual)
            _presenter.RecalcularTotales();
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

        if (motivoDebito == MotivoDebito.IncluidaEnOtra)
        {
            fila.Cells["NC_PrestacionEnglobante"].ReadOnly = false;
        }
        else if (FacturaTipo == TipoDocumento.NotaCredito)
        {
            fila.Cells["NC_PrestacionEnglobante"].ReadOnly = true;
            fila.Cells["NC_PrestacionEnglobante"].Style.BackColor = System.Drawing.Color.Gray;

            bool control = false;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                string motivoDebitoFilaActual = row.Cells["NC_MotivoDeDebito"].Value?.ToString();
                if (motivoDebitoFilaActual == MotivoDebito.IncluidaEnOtra)
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
            case TipoDocumento.Factura:
                if (fila.Cells["NC_ImporteDeRefactura"].Value != null && fila.Cells["NC_ImporteDeRefactura"].Value != DBNull.Value)
                {
                    fila.Cells["NC_ImporteDeRefactura"].Value = fila.Cells["total_neto"].Value;
                    fila.Cells["NC_Comentarios"].ReadOnly = false;
                    fila.Cells["NC_Comentarios"].Style.BackColor = System.Drawing.Color.LightGray;
                }
                break;
            case TipoDocumento.NotaDebito:
                fila.Cells["NC_ImporteDeRefactura"].Value = fila.Cells["importerefactura"].Value;
                fila.Cells["NC_Comentarios"].ReadOnly = false;
                fila.Cells["NC_Comentarios"].Style.BackColor = System.Drawing.Color.LightGray;
                break;
        }

        string motivoDeRefactura = fila.Cells["NC_MotivoDeRefactura"].Value?.ToString();
        if (motivoDeRefactura == MotivoDebito.IncluidaEnOtra)
        {
            string codigoViejo = fila.Cells["codigo"].Value.ToString();
            tipoATransmitir = TipoDocumento.NotaDebito;
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
        if (FacturaTipo == TipoDocumento.Factura)
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
        else if (FacturaTipo == TipoDocumento.NotaCredito)
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
        else if (FacturaTipo == TipoDocumento.NotaDebito)
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
                case TipoDocumento.Factura:
                    columnaImporteTotal = dataGridView1.Columns["total"].Index;
                    break;
                case TipoDocumento.NotaDebito:
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
                        case TipoDocumento.Factura:
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

                        case TipoDocumento.NotaCredito:
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

                        case TipoDocumento.NotaDebito:
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
            _presenter.RecalcularTotales();
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
                        if (FacturaTipo == TipoDocumento.Factura)
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
                            case TipoDocumento.Factura:
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

                            case TipoDocumento.NotaCredito:
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

                            case TipoDocumento.NotaDebito:
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

                _presenter.RecalcularTotales();

            }
            else
            {
                MessageBox.Show("Por favor, seleccione una o más celdas en el DataGridView para aplicar el valor.");
            }
        }

        debitoIndividual = true;

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
            case TipoDocumento.Factura:

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

            case TipoDocumento.NotaCredito:
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

            case TipoDocumento.NotaDebito:
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
        // Solo notificamos. No hay lógica de repositorio aquí.
        GenerarNotaDeCreditoEvent?.Invoke(this, EventArgs.Empty);
    }

    public void AbrirFormularioNotaDeCredito(bool cargaACompletar, string usuario)
    {
        // Usamos el nombre de la variable que definimos en el constructor: ingresoInformacionNotaDeCredito
        ingresoInformacionNotaDeCredito = new IngresoInformacionNotaDeCredito(
            cargaACompletar, FacturaNumero, FacturaLetra, FacturaPuntoDeVenta, FacturaTipo, usuario);
        ingresoInformacionNotaDeCredito.Show();
    }

    public DataTable ObtenerDataTableActual()
    {
        return (DataTable)bindingSource.DataSource;
    }

    public void LimpiarUI_PostOperacion()
    {
        limpiarPantall(); // Tu método que ya limpia los TextBox
        panel1.Visible = false;
        lblModulo.Visible = false;
        btnBorrarCelda.Visible = false;
        lblCantidadDeRegistrosFiltrados.Visible = false;
        // Agregá cualquier otro control que necesites resetear
    }

    private void btnNuevaNotaDeDébito_Click(object sender, EventArgs e)
    {
        GuardarValoresAntesDeDeshacerFiltroNC();
        _repository.LimpiarAuxiliarND(usuario);
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
            cargaACompletar, FacturaNumero, FacturaLetra, FacturaPuntoDeVenta, FacturaTipo, usuario);
        ingresoInformacionNotaDeDebito.Show();
    }

    private void btnLimpiarFila_Click(object sender, EventArgs e)
    {
        // 1. Forzar cierre de edición
        if (dataGridView1.IsCurrentCellInEditMode)
        {
            dataGridView1.EndEdit();
        }

        if (dataGridView1.SelectedCells.Count == 0) return;

        var filasUnicas = dataGridView1.SelectedCells.Cast<DataGridViewCell>()
            .Select(c => c.RowIndex)
            .Distinct()
            .Select(i => dataGridView1.Rows[i])
            .ToList();

        foreach (var row in filasUnicas)
        {
            if (FacturaTipo == TipoDocumento.Factura || FacturaTipo == TipoDocumento.NotaDebito)
            {
                string[] columnas = { "nc_motivodedebito", "nc_importedebitado", "nc_debitoaceptado", "nc_motivoderefactura", "nc_importederefactura", "nc_comentarios", "nc_prestacionenglobante", "nc_diasfacturados" };

                foreach (string col in columnas)
                {
                    if (row.DataGridView.Columns.Contains(col))
                        row.Cells[col].Value = col == "nc_debitoaceptado" ? false : DBNull.Value;
                }

                if (row.DataGridView.Columns.Contains("nc_comentarios"))
                {
                    row.Cells["nc_comentarios"].ReadOnly = true;
                    row.Cells["nc_comentarios"].Style.BackColor = System.Drawing.Color.Coral;
                }
            }
            else if (FacturaTipo == TipoDocumento.NotaCredito)
            {
                string[] columnas = { "nd_motivoderefactura", "nd_importederefactura", "nd_comentarios" };

                foreach (string col in columnas)
                {
                    if (row.DataGridView.Columns.Contains(col))
                        row.Cells[col].Value = DBNull.Value;
                }

                if (row.DataGridView.Columns.Contains("nd_comentarios"))
                {
                    row.Cells["nd_comentarios"].ReadOnly = true;
                    row.Cells["nd_comentarios"].Style.BackColor = System.Drawing.Color.Coral;
                }
            }
        }

        // 2. Actualizamos todos los totales
        GuardarValoresParaActualizarMontoAuditados();
        GuardarValoresParaActualizarMontoDeRefactura();
        _presenter.RecalcularTotales();
        dataGridView1.Refresh();
    }

    private void btnExportar_Click_1(object sender, EventArgs e)
    {
        if (dataGridView1.Rows.Count == 0)
        {
            MessageBox.Show("No hay datos para exportar.");
            return;
        }

        this.UseWaitCursor = true;

        try
        {
            // 1. Crear una tabla en memoria para la exportación
            DataTable dtExport = new DataTable();

            // 2. Definir el orden obligatorio de las primeras 4 columnas (Nombres internos del DataTable)
            var columnasAExportar = new List<string> { "carnet", "paciente", "Cobertura", "Plan" };

            // 3. Agregar el resto de las columnas que son visibles en la interfaz
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                // Si la columna es visible y no es una de las 4 iniciales, la agregamos al final del listado
                if (col.Visible && !columnasAExportar.Contains(col.Name, StringComparer.OrdinalIgnoreCase))
                {
                    columnasAExportar.Add(col.Name);
                }
            }

            // 4. Crear las columnas en el DataTable de exportación con sus respectivos títulos
            foreach (string internalName in columnasAExportar)
            {
                // Buscamos la configuración en la grilla para obtener el HeaderText traducido
                DataGridViewColumn gridCol = dataGridView1.Columns.Cast<DataGridViewColumn>()
                    .FirstOrDefault(c => string.Equals(c.Name, internalName, StringComparison.OrdinalIgnoreCase));

                string headerText = gridCol != null ? gridCol.HeaderText : internalName;

                // Asignamos nombres amigables para las columnas que suelen estar ocultas
                if (internalName.Equals("carnet", StringComparison.OrdinalIgnoreCase)) headerText = "Carnet";
                if (internalName.Equals("Cobertura", StringComparison.OrdinalIgnoreCase)) headerText = "Cobertura";

                // Limpiamos el texto de cabecera (quitamos saltos de línea para Excel)
                headerText = headerText.Replace("\n", " ").Replace("\r", " ").Trim();

                // Evitamos errores de nombres duplicados en el DataTable
                string finalHeader = headerText;
                int counter = 1;
                while (dtExport.Columns.Contains(finalHeader))
                    finalHeader = headerText + "_" + (counter++);

                // Intentamos detectar el tipo de dato original para mantener el formato numérico en Excel
                Type tipoDato = typeof(object);
                if (gridCol != null && gridCol.ValueType != null)
                {
                    tipoDato = Nullable.GetUnderlyingType(gridCol.ValueType) ?? gridCol.ValueType;
                }
                else if (bindingSource.DataSource is DataTable sourceDt && sourceDt.Columns.Contains(internalName))
                {
                    tipoDato = Nullable.GetUnderlyingType(sourceDt.Columns[internalName].DataType) ?? sourceDt.Columns[internalName].DataType;
                }

                dtExport.Columns.Add(finalHeader, tipoDato);
            }

            // 5. Poblar el DataTable con los datos reales del BindingSource
            foreach (DataRowView rowView in bindingSource)
            {
                DataRow newRow = dtExport.NewRow();
                for (int i = 0; i < columnasAExportar.Count; i++)
                {
                    string colName = columnasAExportar[i];
                    if (rowView.Row.Table.Columns.Contains(colName))
                    {
                        newRow[i] = rowView[colName] ?? DBNull.Value;
                    }
                }
                dtExport.Rows.Add(newRow);
            }

            // 6. Generar el archivo Excel usando ClosedXML
            using (XLWorkbook workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Auditoría");
                var tablaExcel = worksheet.Cell(1, 1).InsertTable(dtExport);
                tablaExcel.Theme = XLTableTheme.TableStyleMedium2;
                worksheet.Columns().AdjustToContents();

                SaveFileDialog saveFileDialog1 = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = $"Auditoria_{FacturaTipo}_{FacturaLetra}-{FacturaPuntoDeVenta:D4}-{FacturaNumero:D8}.xlsx"
                };

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    workbook.SaveAs(saveFileDialog1.FileName);
                    MessageBox.Show("Datos exportados a Excel correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Hubo un error al intentar exportar: " + ex.Message, "Error de Exportación", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            this.UseWaitCursor = false;
        }
    }

    private void btnGuardarParcialmente_Click(object sender, EventArgs e)
    {
        // La vista avisa que hicieron clic, nada más.
        GuardarParcialmenteEvent?.Invoke(this, EventArgs.Empty);
    }

    public void PrepararUI_GuardadoParcial()
    {
        cargaCompletada = false;
        lblMontosNoAceptados.Visible = false;
        lblMontoTotalRegistrosEnPantalla.Visible = false;
    }

    public DataView ObtenerDatosFiltrados()
    {
        if (bindingSource.DataSource is DataTable dt)
        {
            return dt.DefaultView;
        }
        return null;
    }

    private void btnBuscar_Click(object sender, EventArgs e)
    {
        BuscarDocumentoEvent?.Invoke(this, EventArgs.Empty);
    }

    private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
    {
        // La grilla ahora se ordena nativamente gracias al BindingSource.
        // Solo aplicamos nuevamente el formato visual.
        switch (FacturaTipo)
        {
            case TipoDocumento.Factura:
                colorearColumnasFC();
                break;
            case TipoDocumento.NotaCredito:
                colorearColumnasNC();
                break;
            case TipoDocumento.NotaDebito:
                colorearColumnasND();
                break;
        }
    }

    private void limpiarPantall()
    {
        tablasFiltrosMedico.Clear();
        tablasFiltrosPaciente.Clear();
        tablasFiltrosPrestacion.Clear();
        tablasFiltrosModulo.Clear();

        // Simplemente desconectamos la grilla
        dataGridView1.DataSource = null;

        FacturaTipo = "";
        FacturaLetra = "";
        FacturaPuntoDeVenta = 0;
        FacturaNumero = 0;

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

        letra.Text = "";
        numero.Text = "";
        puntodeventa.Text = "";
        filtroTipo.Text = "Tipo";
        cargaPrimeraVez = true;

        resetearVariables();
    }

    private void comboFiltroFecha_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cargaListaFecha || buscando || comboFiltroFecha.SelectedIndex <= 0) return;
        GuardarValoresAntesDeDeshacerFiltro();
        lblFecSel.Text = "Fecha: " + comboFiltroFecha.Text;
        lblFecSel.Visible = true;
        comboFiltroFecha.Visible = false;
        AplicarFiltrosActivos();
    }

    private void GuardarValoresParaActualizarMontoAuditados()
    {
        listaValoresParaImporteDeDebito.Clear();
        double importeTotal = 0.0;

        // Iteramos sobre la capa de memoria (súper rápido) en lugar de la UI
        foreach (DataRowView rowView in bindingSource)
        {
            DataRow row = rowView.Row;

            object importeValue = row["nc_importedebitado"];
            object debitoAceptadoValue = row["nc_debitoaceptado"];

            double importeAux = 0.0;
            bool tieneImporte = importeValue != DBNull.Value && double.TryParse(importeValue.ToString(), out importeAux) && importeAux > 0;
            bool aceptaDebito = debitoAceptadoValue != DBNull.Value && Convert.ToBoolean(debitoAceptadoValue);

            if (tieneImporte && aceptaDebito)
            {
                int idPrestacion = Convert.ToInt32(row["id_prestacion"]);
                listaValoresParaImporteDeDebito.Add((idPrestacion, importeAux));
                importeTotal += importeAux; // Sumatoria directa
            }
        }

        lblMontoTotalRegistrosEnPantalla.Text = "Suma total de débitos auditados: " + importeTotal.ToString("C");
        lblMontoTotalRegistrosEnPantalla.Visible = true;
    }

    private void GuardarValoresParaActualizarMontoDeRefactura()
    {
        listaValoresParaImporteDeRefactura.Clear();
        double importeTotal = 0.0;

        foreach (DataRowView rowView in bindingSource)
        {
            DataRow row = rowView.Row;

            object importeValue = row["nc_importederefactura"];
            object debitoAceptadoValue = row["nc_debitoaceptado"];

            bool tieneImporte = importeValue != DBNull.Value;
            bool noAceptaDebito = debitoAceptadoValue == DBNull.Value || !Convert.ToBoolean(debitoAceptadoValue);

            if (tieneImporte && noAceptaDebito)
            {
                int idPrestacion = Convert.ToInt32(row["id_prestacion"]);
                double importeRefactura = Convert.ToDouble(importeValue);

                listaValoresParaImporteDeRefactura.Add((idPrestacion, importeRefactura));
                importeTotal += importeRefactura;
            }
        }

        lblMontosNoAceptados.Text = "Suma total de débitos a refacturar: " + importeTotal.ToString("C");
        lblMontosNoAceptados.Visible = true;
    }

    private void GuardarValoresAntesDeDeshacerFiltro()
    {
        DataTable dataTableActual = null;
        if (dataGridView1.DataSource is BindingSource bs)
            dataTableActual = bs.DataSource as DataTable;
        else if (dataGridView1.DataSource is DataTable dt)
            dataTableActual = dt;

        if (dataTableActual == null) return;

        // CORRECCIÓN: Ahora incluimos explícitamente el Debito Aceptado y evitamos bugs con NULL
        DataRow[] filasFiltradas = dataTableActual.Select(
            "(NC_MotivoDeRefactura IS NOT NULL AND NC_MotivoDeRefactura <> '') OR " +
            "(NC_MotivoDeDebito IS NOT NULL AND NC_MotivoDeDebito <> '') OR " +
            "(NC_DebitoAceptado = True)"
        );

        foreach (DataRow row in filasFiltradas)
        {
            int idPrestacion = Convert.ToInt32(row["id_prestacion"]);
            object motivoRefactura = row["NC_MotivoDeRefactura"] != DBNull.Value ? row["NC_MotivoDeRefactura"] : "";
            object motivoDebito = row["NC_MotivoDeDebito"] != DBNull.Value ? row["NC_MotivoDeDebito"] : "";
            double? importeRefactura = row["NC_ImporteDeRefactura"] != DBNull.Value ? Convert.ToDouble(row["NC_ImporteDeRefactura"]) : (double?)null;
            double? importeDebito = row["NC_ImporteDebitado"] != DBNull.Value ? Convert.ToDouble(row["NC_ImporteDebitado"]) : (double?)null;
            string? comentarios = row["NC_Comentarios"] != DBNull.Value ? row["NC_Comentarios"].ToString().Replace('\0', ' ').Trim() : "";
            bool debitoAceptado = row["NC_DebitoAceptado"] != DBNull.Value && Convert.ToBoolean(row["NC_DebitoAceptado"]);
            object diasFacturados = DBNull.Value;

            if (FacturaTipo != TipoDocumento.NotaCredito && row["NC_DiasFacturados"] != DBNull.Value)
                diasFacturados = Convert.ToInt32(row["NC_DiasFacturados"]);

            string prestacionEnglobante = row["NC_PrestacionEnglobante"] != DBNull.Value ? row["NC_PrestacionEnglobante"].ToString() : "";
            string codigo = string.Empty;

            if (FacturaTipo == TipoDocumento.NotaDebito && row["codigo"] != DBNull.Value)
                codigo = row["codigo"].ToString();

            int index = listaValoresParaBorradoDeFiltros.FindIndex(x => x.idPrestacion == idPrestacion);
            var nuevoElemento = (idPrestacion, motivoRefactura, motivoDebito, importeRefactura, importeDebito, comentarios, debitoAceptado, diasFacturados, prestacionEnglobante, codigo);

            if (index == -1)
                listaValoresParaBorradoDeFiltros.Add(nuevoElemento);
            else
                listaValoresParaBorradoDeFiltros[index] = nuevoElemento;
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
                    if (FacturaTipo != TipoDocumento.NotaCredito)
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

    private void lblPacSel_Click(object sender, EventArgs e)
    {
        lblPacSel.Visible = false;
        filtroPaciente.Visible = true;
        filtroPaciente.SelectedIndex = 0;
        AplicarFiltrosActivos();
    }

    private void lblProfSel_Click(object sender, EventArgs e)
    {
        lblProfSel.Visible = false;
        filtroProfesional.Visible = true;
        filtroProfesional.SelectedIndex = 0;
        AplicarFiltrosActivos();
    }

    private void lblPrestSel_Click(object sender, EventArgs e)
    {
        lblPrestSel.Visible = false;
        filtroPrestacion.Visible = true;
        filtroPrestacion.SelectedIndex = 0;
        AplicarFiltrosActivos();
    }

    private void lblNumeroDeInternacionSel_Click(object sender, EventArgs e)
    {
        lblNumeroDeInternacionSel.Visible = false;
        filtroNumeroDeInternacion.Visible = true;
        filtroNumeroDeInternacion.SelectedIndex = 0;
        AplicarFiltrosActivos();
    }

    private void lblFecSel_Click(object sender, EventArgs e)
    {
        lblFecSel.Visible = false;
        comboFiltroFecha.Visible = true;
        comboFiltroFecha.SelectedIndex = 0;
        AplicarFiltrosActivos();
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
            if (FacturaTipo == TipoDocumento.NotaCredito)
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

        // CORRECCIÓN: La etiqueta de fecha seleccionada arranca oculta
        lblFecSel.Visible = false;

        checkMotivoDeRefactura.Visible = visible;
        btnExportar.Visible = visible;
        btnBorrarCelda.Visible = visible;
        // CORRECCIÓN: Lógica condicional para los botones de nueva nota
        if (visible)
        {
            // Si es FC o ND, solo se puede hacer Nota de Crédito
            btnNuevaNotaDeCrédito.Visible = (FacturaTipo == TipoDocumento.Factura || FacturaTipo == TipoDocumento.NotaDebito);

            // Si es NC, solo se puede hacer Nota de Débito
            btnNuevaNotaDeDébito.Visible = (FacturaTipo == TipoDocumento.NotaCredito);
        }
        else
        {
            btnNuevaNotaDeCrédito.Visible = false;
            btnNuevaNotaDeDébito.Visible = false;
        }
        btnLimpiarFila.Visible = visible;
        btnGuardarParcialmente.Visible = visible;
        soloPrestacionesValorizadas.Visible = visible;
        filtroGrupoPrestacion.Visible = visible;
        btnBorrarImporteDebito.Visible = visible;
        btnBorrarImporteRefactura.Visible = visible;
        lblCantidadDeRegistrosConDebitoAceptado.Visible = visible;

        // CORRECCIÓN: Todas las etiquetas de los filtros arrancan ocultas
        lblPacSel.Visible = false;
        lblPrestSel.Visible = false;
        lblProfSel.Visible = false;
        lblNumeroDeInternacionSel.Visible = false;

        panel1.Visible = visible;
        if (TipoRegistroFiltrado == "Ambulatorios")
            panel1.Visible = false;

        lblModulo.Visible = visible;
        if (TipoRegistroFiltrado == "Ambulatorios")
            lblModulo.Visible = false;
    }

    private void RecargarFiltroGenerico(DataView vistaFiltrada, string columna, ComboBox combo, List<DataTable> listaFiltros, string displayName)
    {
        var dtUnico = new DataTable();
        dtUnico.Columns.Add(columna, typeof(string));
        dtUnico.Rows.Add(displayName);

        var valoresUnicos = new HashSet<string>();
        foreach (DataRowView rowView in vistaFiltrada)
        {
            if (rowView.Row[columna] != DBNull.Value)
            {
                string valor = rowView.Row[columna].ToString();
                if (!string.IsNullOrWhiteSpace(valor))
                    valoresUnicos.Add(valor);
            }
        }

        foreach (var valor in valoresUnicos.OrderBy(x => x, StringComparer.CurrentCultureIgnoreCase))
        {
            dtUnico.Rows.Add(valor);
        }

        listaFiltros.Add(dtUnico);

        // Bloqueamos el evento, bindeamos, y liberamos inmediatamente
        SetFlagFiltro(columna, true);
        combo.DataSource = dtUnico;
        combo.DisplayMember = columna;
        combo.ValueMember = columna;
        combo.SelectedIndex = 0;
        SetFlagFiltro(columna, false);
    }

    private void SetFlagFiltro(string nombreColumna, bool estado)
    {
        switch (nombreColumna.ToLower())
        {
            case "paciente": cargaListaPaciente = estado; break;
            case "nro_internacion":
            case "nro_int": cargaListaNumeroDeInternacion = estado; break;
            case "medico": cargaListaProfesional = estado; break;
            case "codigo": cargaListaPrestacion = estado; break;
            case "modulo": cargaListaModulo = estado; break;
            case "fecha": cargaListaFecha = estado; break;
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

    private void ActualizarFiltrosDisponibles(DataView vistaFiltrada)
    {
        tablasFiltrosPaciente.Clear();
        tablasFiltrosPrestacion.Clear();
        tablasFiltrosMedico.Clear();
        tablasFiltrosModulo.Clear();
        tablasFiltrosNumeroDeInternacion.Clear();
        tablasFiltrosFecha.Clear();

        RecargarFiltroGenerico(vistaFiltrada, "paciente", filtroPaciente, tablasFiltrosPaciente, "Paciente");
        RecargarFiltroGenerico(vistaFiltrada, "codigo", filtroPrestacion, tablasFiltrosPrestacion, "Prestación");
        RecargarFiltroGenerico(vistaFiltrada, "medico", filtroProfesional, tablasFiltrosMedico, "Profesional");

        if (vistaFiltrada.Table.Columns.Contains("modulo"))
            RecargarFiltroGenerico(vistaFiltrada, "modulo", filtroModulo, tablasFiltrosModulo, "Módulo");

        string colNroInternacion = vistaFiltrada.Table.Columns.Contains("nro_internacion") ? "nro_internacion" :
                                   vistaFiltrada.Table.Columns.Contains("nro_int") ? "nro_int" : null;
        if (colNroInternacion != null)
            RecargarFiltroGenerico(vistaFiltrada, colNroInternacion, filtroNumeroDeInternacion, tablasFiltrosNumeroDeInternacion, "N° de internación");

        if (vistaFiltrada.Table.Columns.Contains("fecha"))
        {
            var dtUnico = new DataTable();
            dtUnico.Columns.Add("fecha", typeof(string));
            dtUnico.Rows.Add("Fecha");

            var fechasUnicas = new HashSet<string>();
            foreach (DataRowView rowView in vistaFiltrada)
            {
                if (rowView.Row["fecha"] != DBNull.Value)
                    fechasUnicas.Add(Convert.ToDateTime(rowView.Row["fecha"]).ToString("dd/MM/yyyy"));
            }
            foreach (var fecha in fechasUnicas.OrderBy(x => x))
                dtUnico.Rows.Add(fecha);

            tablasFiltrosFecha.Add(dtUnico);
            SetFlagFiltro("fecha", true);
            comboFiltroFecha.DataSource = dtUnico;
            comboFiltroFecha.DisplayMember = "fecha";
            comboFiltroFecha.ValueMember = "fecha";
            comboFiltroFecha.SelectedIndex = 0;
            SetFlagFiltro("fecha", false);
        }
    }

    private void btnBorrarCelda_Click(object sender, EventArgs e)
    {
        // 1. Forzamos el cierre del editor activo (TextBox) para que la UI se actualice al instante
        if (dataGridView1.IsCurrentCellInEditMode)
        {
            dataGridView1.EndEdit();
        }

        if (dataGridView1.SelectedCells.Count == 0)
        {
            MessageBox.Show("Por favor, seleccione una o más celdas en el DataGridView para aplicar el valor.");
            return;
        }

        var columnasAuditables = (FacturaTipo == TipoDocumento.Factura || FacturaTipo == TipoDocumento.NotaDebito)
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nc_comentarios", "nc_prestacionenglobante", "nc_importederefactura", "nc_motivoderefactura", "nc_importedebitado", "nc_diasfacturados", "nc_motivodedebito", "nc_debitoaceptado" }
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nd_motivoderefactura", "nd_importederefactura", "nd_comentarios" };

        foreach (DataGridViewCell celda in dataGridView1.SelectedCells)
        {
            string nombreColumna = dataGridView1.Columns[celda.ColumnIndex].Name.ToLower();

            if (columnasAuditables.Contains(nombreColumna))
            {
                var row = dataGridView1.Rows[celda.RowIndex];

                if (nombreColumna == "nc_debitoaceptado")
                {
                    celda.Value = false;
                }
                else
                {
                    celda.Value = DBNull.Value;

                    // Reglas de negocio en cascada
                    if (nombreColumna == "nc_motivoderefactura")
                    {
                        row.Cells["nc_importederefactura"].Value = DBNull.Value;
                        if (row.DataGridView.Columns.Contains("nc_comentarios"))
                        {
                            row.Cells["nc_comentarios"].ReadOnly = true;
                            row.Cells["nc_comentarios"].Style.BackColor = System.Drawing.Color.Coral;
                        }
                    }
                    else if (nombreColumna == "nc_motivodedebito")
                    {
                        row.Cells["nc_importedebitado"].Value = DBNull.Value;
                    }
                    else if (nombreColumna == "nd_motivoderefactura")
                    {
                        row.Cells["nd_importederefactura"].Value = DBNull.Value;
                        if (row.DataGridView.Columns.Contains("nd_comentarios"))
                        {
                            row.Cells["nd_comentarios"].ReadOnly = true;
                            row.Cells["nd_comentarios"].Style.BackColor = System.Drawing.Color.Coral;
                        }
                    }
                }
            }
        }

        // 2. Actualizamos todos los totales de la pantalla
        GuardarValoresParaActualizarMontoAuditados();
        GuardarValoresParaActualizarMontoDeRefactura();
        _presenter.RecalcularTotales();
        dataGridView1.Refresh();
    }

}