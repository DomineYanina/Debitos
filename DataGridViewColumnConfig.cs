using System.Windows.Forms;

public class DataGridViewColumnConfig
{
    public string Name { get; set; }
    public string HeaderText { get; set; } // Nueva propiedad
    public bool? Visible { get; set; }
    public bool? ReadOnly { get; set; }
    public System.Drawing.Color? BackColor { get; set; }
    public int? Width { get; set; }

    public static List<DataGridViewColumnConfig> GetFCColumnConfigs()
    {
        var cyan = System.Drawing.Color.LightCyan;
        var gray = System.Drawing.Color.LightGray;
        var coral = System.Drawing.Color.Coral;

        return new List<DataGridViewColumnConfig>
        {
            new() { Name = "paciente", HeaderText = "Paciente", ReadOnly = true, BackColor = cyan, Width=100},
            new() { Name = "carnet", Visible = false, Width=100 },
            new() { Name = "medico", HeaderText = "Médico", ReadOnly = true, BackColor = cyan, Width=100 },
            new() { Name = "fecha", HeaderText = "Fecha", ReadOnly = true, BackColor = cyan, Width=70 },
            new() { Name = "codigo", HeaderText = "Código", ReadOnly = true, BackColor = cyan, Width=70 },
            new() { Name = "descripcion", HeaderText = "Descripción", ReadOnly = true, BackColor = cyan, Width=100 },
            new() { Name = "cantidad", HeaderText = "Cant.", ReadOnly = true, BackColor = cyan },
            new() { Name = "modulo", HeaderText = "Módulo", ReadOnly = true, BackColor = cyan },
            new() { Name = "grupomodulo", HeaderText = "Grupo\nMódulo", ReadOnly = true, BackColor = cyan },
            new() { Name = "total_neto", HeaderText = "Total\nNeto", ReadOnly = true, BackColor = cyan },
            new() { Name = "coseguro", HeaderText = "Coseguro", ReadOnly = true, BackColor = cyan },
            new() { Name = "total", HeaderText = "Total", ReadOnly = true, BackColor = cyan },
            new() { Name = "porcentaje_especialista", HeaderText = "Porc.\nEspecialista", Visible = false, ReadOnly = true, BackColor = cyan },
            new() { Name = "porcentaje_ayudante1", HeaderText = "Porc.\nAyudante", ReadOnly = true, BackColor = cyan },
            new() { Name = "porcentaje_anestesista", HeaderText = "Porc.\nAnestesista", Visible = false, ReadOnly = true, BackColor = cyan },
            new() { Name = "porcentaje_gastos", HeaderText = "Porc.\nGastos", Visible = false, ReadOnly = true, BackColor = cyan },
            new() { Name = "nc_motivodedebito", HeaderText = "Motivo de\nDébito", ReadOnly = true, BackColor = gray, Width=100 },
            new() { Name = "nc_motivoderefactura", HeaderText = "Motivo de\nRefactura", ReadOnly = true, BackColor = gray, Width=100 },
            new() { Name = "nc_comentarios", HeaderText = "Comentarios", BackColor = gray },
            new() { Name = "Plan", HeaderText = "Plan", BackColor = cyan },
            new() { Name = "porcentaje_especialista", HeaderText = "Porc.\nEspecialista", Visible = false, ReadOnly = true, BackColor = cyan },
            new() { Name = "porcentaje_ayudante1", HeaderText = "Porc.\nAyudante", Visible = false, ReadOnly = true, BackColor = cyan },
            new() { Name = "porcentaje_anestesista", HeaderText = "Porc.\nAnestesista", Visible = false, ReadOnly = true, BackColor = cyan },
            new() { Name = "porcentaje_gastos", HeaderText = "Porc.\nGastos", Visible = false, ReadOnly = true, BackColor = cyan },
            new() { Name = "Cobertura", HeaderText = "Cobertura", Visible = false },
            new() { Name = "NC_Fecha", HeaderText = "NC\nFecha", Visible = false },
            new() { Name = "NC_Letra", HeaderText = "NC\nLetra", Visible = false },
            new() { Name = "NC_PuntoDeVenta", HeaderText = "NC\nPunto de Venta", Visible = false },
            new() { Name = "NC_Numero", HeaderText = "NC\nNúmero", Visible = false },
            new() { Name = "NC_PrestacionEnglobante", HeaderText = "Prestación\nEnglobante", Visible = false },
            new() { Name = "cargadocompletamente", HeaderText = "Carga\nCompleta", Visible = false },
            new() { Name = "id_prestacion", HeaderText = "ID\nPrestación", Visible = false },
            new() { Name = "nro_int", HeaderText = "Número de\ninternación", Visible = false },
            new() { Name = "f_ingreso", HeaderText = "Fecha de\ningreso", Visible = false },
            new() { Name = "f_egreso", HeaderText = "Fecha de\negreso", Visible = false },
            // Grises
            new() { Name = "NC_MotivoDeDebito", HeaderText = "Motivo de\nDébito", BackColor = gray, Width=100 },
            new() { Name = "NC_DiasFacturados", HeaderText = "Días\nFact.", BackColor = gray },
            new() { Name = "NC_ImporteDebitado", HeaderText = "Importe\nDebitado", BackColor = gray },
            new() { Name = "NC_MotivoDeRefactura", HeaderText = "Motivo de\nRefactura", BackColor = gray, Width=100 },
            new() { Name = "NC_ImporteDeRefactura", HeaderText = "Importe de\nRefactura", BackColor = gray },
            new() { Name = "NC_DebitoAceptado", HeaderText = "Débito\nAceptado", BackColor = gray }
        };
    }

    public static List<DataGridViewColumnConfig> GetNCColumnConfigs()
    {
        var cyan = System.Drawing.Color.LightCyan;
        var gray = System.Drawing.Color.LightGray;

        return new List<DataGridViewColumnConfig>
        {
            // Visibilidad para Internados se maneja aparte
            new() { Name = "id", Visible = false },
            new() { Name = "carnet", Visible = false },
            new() { Name = "paciente", HeaderText = "Paciente", ReadOnly = true, BackColor = cyan},
            new() { Name = "medico", HeaderText = "Médico", ReadOnly = true, BackColor = cyan},
            new() { Name = "fecha", HeaderText = "Fecha", ReadOnly = true, BackColor = cyan },
            new() { Name = "codigo", HeaderText = "Código", ReadOnly = true, BackColor = cyan},
            new() { Name = "descripcion", HeaderText = "Descripción", ReadOnly = true, BackColor = cyan },
            new() { Name = "cantidad", HeaderText = "Cant.", ReadOnly = true, BackColor = cyan },
            new() { Name = "Cobertura", HeaderText = "Cobertura", Visible = false },
            new() { Name = "total_neto", HeaderText = "Total\nNeto", ReadOnly = true, BackColor = cyan },
            new() { Name = "coseguro", HeaderText = "Coseguro", ReadOnly = true, BackColor = cyan },
            new() { Name = "plan", HeaderText = "Plan", ReadOnly = true, BackColor = cyan },
            new() { Name = "total", HeaderText = "Total", ReadOnly = true, BackColor = cyan },
            new() { Name = "modulo", HeaderText = "Módulo", ReadOnly = true, BackColor = cyan, Visible = false  },
            new() { Name = "grupomodulo", HeaderText = "Grupo\nMódulo", ReadOnly = true, BackColor = cyan },
            new() { Name = "cob_factura_tipo", Visible = false, ReadOnly = true, BackColor = cyan },
            new() { Name = "cob_factura_letra", Visible = false, ReadOnly = true, BackColor = cyan },
            new() { Name = "cob_factura_ptoventa", Visible = false, ReadOnly = true, BackColor = cyan },
            new() { Name = "NC_PrestacionEnglobante", Visible = false, ReadOnly = true, BackColor = cyan },
            new() { Name = "cob_factura_numero", Visible = false, ReadOnly = true, BackColor = cyan },
            new() { Name = "nc_MotivoDeDebito", Visible = false, ReadOnly = true, BackColor = cyan, Width=100 },
            new() { Name = "nc_importedebitado", Visible = false, ReadOnly = true, BackColor = cyan },
            new() { Name = "nc_debitoaceptado", Visible = false, ReadOnly = true, BackColor = cyan },
            new() { Name = "nc_MotivoDeRefactura", Visible = false, ReadOnly = true, BackColor = cyan, Width=100 },
            new() { Name = "nc_ImporteDeRefactura", Visible = false, ReadOnly = true, BackColor = cyan },
            new() { Name = "nc_comentarios", HeaderText = "Comentario\nPrevio", ReadOnly = true, BackColor = Color.DarkCyan},
            new() { Name = "id_prestacion", Visible = false },
            new() { Name = "nro_int", HeaderText = "Número de\ninternación", Visible = false },
            new() { Name = "f_ingreso", HeaderText = "Fecha de\ningreso", Visible = false },
            new() { Name = "f_egreso", HeaderText = "Fecha de\negreso", Visible = false },
            // Grises para ND
            new() { Name = "nd_MotivoDeRefactura", HeaderText = "Motivo de\nRefactura", ReadOnly = true, BackColor = gray },
            new() { Name = "nd_importederefactura", HeaderText = "Importe de\nRefactura", BackColor = gray },
            new() { Name = "nd_comentarios", HeaderText = "Comentarios", BackColor = gray }
        };
    }

    public static List<DataGridViewColumnConfig> GetNDColumnConfigs()
    {
        var cyan = System.Drawing.Color.LightCyan;
        var gray = System.Drawing.Color.LightGray;

        return new List<DataGridViewColumnConfig>
        {
            // Visibilidad para Internados se maneja aparte
            new() { Name = "id_prestacion", Visible = false },
            new() { Name = "id", Visible = false },
            new() { Name = "NC_PrestacionEnglobante", Visible = false },
            new() { Name = "carnet", Visible = false },
            new() { Name = "NC_Previo_Letra", Visible = false, ReadOnly = true, BackColor = cyan },
            new() { Name = "NC_Previo_PuntoDeVenta", Visible = false, ReadOnly = true, BackColor = cyan },
            new() { Name = "NC_Previo_Numero", Visible = false, ReadOnly = true, BackColor = cyan },
            new() { Name = "NC_Previo_Fecha", Visible = false, ReadOnly = true, BackColor = cyan },
            new() { Name = "NC_Previo_MotivoDeDebito", Visible = false, ReadOnly = true, BackColor = cyan },
            new() { Name = "NC_Previo_ImporteDebitado", Visible = false, ReadOnly = true, BackColor = cyan },
            new() { Name = "NC_Previo_MotivoDeRefactura", Visible = false, ReadOnly = true, BackColor = cyan },
            new() { Name = "modulo", HeaderText = "Módulo", ReadOnly = true, BackColor = cyan, Visible = false  },
            new() { Name = "grupomodulo", HeaderText = "Grupo\nMódulo", ReadOnly = true, BackColor = cyan },
            new() { Name = "fecha", HeaderText = "Fecha", ReadOnly = true, BackColor = cyan },
            new() { Name = "motivorefactura", Visible = false, ReadOnly = true, BackColor = cyan },
            new() { Name = "Plan", HeaderText = "Plan", Visible = true, BackColor = cyan },
            new() { Name = "importerefactura", Visible = false, ReadOnly = true, BackColor = cyan },
            new() { Name = "comentarios", HeaderText = "Comentario\nPrevio", ReadOnly = true, BackColor = Color.DarkCyan },
            new() { Name = "codigo", HeaderText = "Código", Visible = true, BackColor = cyan  },
            new() { Name = "Cobertura", HeaderText = "Cobertura", Visible = false },
            new() { Name = "paciente", HeaderText = "Paciente", Visible = true, BackColor = cyan  },
            new() { Name = "medico", HeaderText = "Médico", Visible = true, BackColor = cyan  },
            new() { Name = "descripcion", HeaderText = "Descripción", ReadOnly = true, BackColor = cyan },
            new() { Name = "cantidad", HeaderText = "Cant.", ReadOnly = true, BackColor = cyan },
            new() { Name = "total_neto", HeaderText = "Total\nNeto", ReadOnly = true, BackColor = cyan },
            new() { Name = "coseguro", HeaderText = "Coseguro", ReadOnly = true, BackColor = cyan },
            new() { Name = "total", HeaderText = "Total", ReadOnly = true, BackColor = cyan },
            new() { Name = "nro_int", HeaderText = "Número de\ninternación", Visible = false },
            new() { Name = "f_ingreso", HeaderText = "Fecha de\ningreso", Visible = false },
            new() { Name = "f_egreso", HeaderText = "Fecha de\negreso", Visible = false },

            // Grises
            new() { Name = "NC_prestacionenglobante", HeaderText = "Prestación\nEnglobante", BackColor = gray },
            new() { Name = "NC_MotivoDeDebito", HeaderText = "Motivo de\nDébito", BackColor = gray },
            new() { Name = "NC_DiasFacturados", HeaderText = "Días\nFact.", BackColor = gray },
            new() { Name = "NC_ImporteDebitado", HeaderText = "Importe\nDebitado", BackColor = gray },
            new() { Name = "NC_MotivoDeRefactura", HeaderText = "Motivo de\nRefactura", BackColor = gray },
            new() { Name = "NC_ImporteDeRefactura", HeaderText = "Importe de\nRefactura", BackColor = gray },
            new() { Name = "NC_DebitoAceptado", HeaderText = "Débito\nAceptado", BackColor = gray },
            new() { Name = "nc_comentarios", HeaderText = "Comentarios", BackColor = gray }
        };
    }


}
