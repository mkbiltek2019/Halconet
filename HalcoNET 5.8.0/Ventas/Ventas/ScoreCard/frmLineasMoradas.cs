﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Threading;
using System.Drawing.Printing;


namespace Ventas.Ventas.ScoreCard
{
    public partial class frmLineasMoradas : Form
    {
        Clases.Logs log;
        #region PARÁMETROS

        public SqlConnection conection = new SqlConnection(ClasesSGUV.Propiedades.conectionPJ);

        public string NombreUsuario;
        public string Vendedores;
        public string Sucursales;
        public DateTime Fecha;
        public int RolUsuario;
        public int CodigoVendedor;
        public string Sucursal;
        public decimal DiasMes = 0;
        public decimal DiasTranscurridos = 0;
        public decimal DiasRestantes = 0;
        public decimal AvanceOptimo = 0;
        public DateTime FechaOK;

        /// <summary>
        /// Enumerador de las columnas del Grid
        /// </summary>
        private enum ColumnasGrid
        {
            SlpCode=0, Vendedor=1, Sucursal, Objetivo, Venta, VentavsCuota, Porvender, TendenciaFinMesM, TendenciaFinMesP, oRDEN, Boton
        }

        private enum TipoConsulta
        {
            DiasMes = 1,
            DiasTranscurridos = 2,
            LineasMoradas = 7
        }
        #endregion        

        public frmLineasMoradas(int rolUsuario, int codigoVendedor, string nombreUsuario, string sucursal)
        {

            RolUsuario = rolUsuario;
            NombreUsuario = nombreUsuario;
            CodigoVendedor = codigoVendedor;
            Sucursal = sucursal;
            InitializeComponent();
        }

        private void LineasMoradas_Load(object sender, EventArgs e)
        {
            try
            {
                this.Icon = ClasesSGUV.Propiedades.IconHalcoNET;
                log = new Clases.Logs(ClasesSGUV.Login.NombreUsuario, this.AccessibleDescription, 0);
                Restricciones();
                CargarSucursales();
                CargarVendedores();
               // CargarDias();
                dateTimePicker1.Value = DateTime.Now;
                gridLineas.DataSource = null;
                gridLineas.Columns.Clear();
                gridTotales.DataSource = null;

                dateTimePicker1.Value = DateTime.Now;
                txtAvanceOptimo.Clear();
                txtDiasMes.Clear();
                txtDiasRestantes.Clear();
                txtDiasTranscurridos.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "HalcoNET", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }



        #region METODOS

        public void Restricciones()
        {
            //Rol Vendedor
            if (RolUsuario == (int)ClasesSGUV.Propiedades.RolesHalcoNET.GerenteVentasSucursal)
            {
                //ocultat sucursalesa
                clbSucursal.Visible = false;
                lblSucursal.Visible = false;
                //Point l1 = new Point(label10.Location.X, label10.Location.Y);
                //Point c1 = new Point(clbLinea.Location.X, clbLinea.Location.Y);

                //Point l2 = new Point(lblSucursal.Location.X, lblSucursal.Location.Y);
                //Point c2 = new Point(clbSucursal.Location.X, clbSucursal.Location.Y);

                //Point l3 = new Point(lblVendedor.Location.X, lblVendedor.Location.Y);
                //Point c3 = new Point(clbVendedor.Location.X, clbVendedor.Location.Y);

                //lblVendedor.Location = l1;
                //clbVendedor.Location = c1;

                //label10.Location = l2;
                //clbLinea.Location = c2;

                //lblCanal.Location = l3;
                //clbCanal.Location = c3;
            }

           
            //Rol Ventas Especial
            if (RolUsuario == (int)ClasesSGUV.Propiedades.RolesHalcoNET.Ventas)
            {

                //label10.Location = new Point(lblSucursal.Location.X, lblSucursal.Location.Y);
                //clbLinea.Location = new Point(clbSucursal.Location.X, clbSucursal.Location.Y);

                //lblCanal.Location = new Point(label10.Location.X, label10.Location.Y);
                //clbCanal.Location = new Point(clbLinea.Location.X, clbLinea.Location.Y);

                lblVendedor.Visible = false;
                clbVendedor.Visible = false;
                clbSucursal.Visible = false;
                lblSucursal.Visible = false;
                Vendedores = "," + CodigoVendedor.ToString();
            }

        }

        /// <summary>
        /// Método que carga las sucursales en el cbSucursal
        /// </summary>
        private void CargarSucursales()
        {
            if (RolUsuario == (int)ClasesSGUV.Propiedades.RolesHalcoNET.Administrador || RolUsuario == (int)ClasesSGUV.Propiedades.RolesHalcoNET.GerenteVentas || RolUsuario == (int)ClasesSGUV.Propiedades.RolesHalcoNET.Zulma)
            {
                SqlCommand command = new SqlCommand("PJ_ConsultasVariasSGUV", conection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@TipoConsulta", (int)Constantes.ConsultasVariasPJ.Sucursales);
                command.Parameters.AddWithValue("@Sucursal", string.Empty);
                command.Parameters.AddWithValue("@SlpCode", 0);

                DataTable table = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = command;
                adapter.Fill(table);

                DataRow row = table.NewRow();
                row["Nombre"] = "TODAS";
                row["Codigo"] = "0";
                table.Rows.InsertAt(row, 0);

                clbSucursal.DataSource = table;
                clbSucursal.DisplayMember = "Nombre";
                clbSucursal.ValueMember = "Codigo";
            }
            else if (RolUsuario == (int)ClasesSGUV.Propiedades.RolesHalcoNET.GerenteVentasSucursal || RolUsuario == (int)ClasesSGUV.Propiedades.RolesHalcoNET.Ventas)
            {
                SqlCommand command = new SqlCommand("PJ_ConsultasVariasSGUV", conection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@TipoConsulta", 12);
                command.Parameters.AddWithValue("@Sucursal", Sucursal.Trim());
                command.Parameters.AddWithValue("@SlpCode", 0);
                DataTable table = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = command;
                adapter.Fill(table);

                DataRow row = table.NewRow();
                row["Nombre"] = "TODAS";
                row["Codigo"] = "0";
                table.Rows.InsertAt(row, 0);

                clbSucursal.DataSource = table;
                clbSucursal.DisplayMember = "Nombre";
                clbSucursal.ValueMember = "Codigo";
            }
        }

        /// <summary>
        /// Método que carga los Vendedores en el clbVendedor
        /// </summary>
        private void CargarVendedores()
        {
            if (RolUsuario == (int)ClasesSGUV.Propiedades.RolesHalcoNET.Administrador || RolUsuario == (int)ClasesSGUV.Propiedades.RolesHalcoNET.GerenteVentas || RolUsuario == (int)ClasesSGUV.Propiedades.RolesHalcoNET.Zulma)
            {
                SqlCommand command = new SqlCommand("PJ_ConsultasVariasSGUV", conection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@TipoConsulta", (int)Constantes.ConsultasVariasPJ.Vendedores);
                command.Parameters.AddWithValue("@Sucursal", Sucursal);
                command.Parameters.AddWithValue("@SlpCode", CodigoVendedor);
                DataTable table = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = command;
                adapter.Fill(table);

                DataRow row = table.NewRow();
                row["Nombre"] = "TODAS";
                row["Codigo"] = "0";
                table.Rows.InsertAt(row, 0);

                clbVendedor.DataSource = table;
                clbVendedor.DisplayMember = "Nombre";
                clbVendedor.ValueMember = "Codigo";
            }
            else if (RolUsuario == (int)ClasesSGUV.Propiedades.RolesHalcoNET.GerenteVentasSucursal)
            {
                SqlCommand command = new SqlCommand("PJ_ConsultasVariasSGUV", conection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@TipoConsulta", 11);
                command.Parameters.AddWithValue("@Sucursal", Sucursal);
                command.Parameters.AddWithValue("@SlpCode", CodigoVendedor);
                DataTable table = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = command;
                adapter.Fill(table);

                DataRow row = table.NewRow();
                row["Nombre"] = "TODAS";
                row["Codigo"] = "0";
                table.Rows.InsertAt(row, 0);

                clbVendedor.DataSource = table;
                clbVendedor.DisplayMember = "Nombre";
                clbVendedor.ValueMember = "Codigo";
            }
        }

        /// <sumary> 
        /// Metodos que crea una cadena con el formato ,num1,nm2 etc
        /// La cadena se crea apartir de los elementos seleccionados en un CheckedListBox
        /// Si no se selecciono ningún elemento del CheckListBox se crea una cadena 
        /// que contiene todos los elmentos del mismo
        /// </sumary>
        private string CrearCadena(CheckedListBox clb)
        {
            StringBuilder stb = new StringBuilder();
            foreach (DataRowView item in clb.CheckedItems)
            {
                if (item["Codigo"].ToString() != "0")
                {
                    if (!clb.ToString().Equals(string.Empty))
                    {
                        stb.Append(",");
                    }
                    stb.Append(item["Codigo"].ToString());
                }
            }
            if (clb.CheckedItems.Count == 0)
            {
                foreach (DataRowView item in clb.Items)
                {
                    if (item["Codigo"].ToString() != "0")
                    {
                        if (!clb.ToString().Equals(string.Empty))
                        {
                            stb.Append(",");
                        }
                        stb.Append(item["Codigo"].ToString());
                    }
                }
            }
            return stb.ToString();
        }

        /// <sumary> 
        /// Metodos para cambiar la apariencia del cursor
        /// </sumary>
        private void Esperar()
        {

            foreach (Control item in this.Controls)
            {
                item.Cursor = Cursors.WaitCursor;
            }
        }
        private void Continuar()
        {

            foreach (Control item in this.Controls)
            {
                item.Cursor = Cursors.Arrow;
            }
        }

        /// <sumary> 
        /// Metodos para cambiar la apariencia del cursor
        /// </sumary>
        private void DarFormatoGrid()
        {
            DataGridViewButtonColumn botonDetails = new DataGridViewButtonColumn();
            {
                botonDetails.Name = "Detalle";
                botonDetails.HeaderText = "Detalle";
                botonDetails.Text = "Detalle";
                botonDetails.Width = 130;
                botonDetails.UseColumnTextForButtonValue = true;
            }
            gridLineas.Columns.Add(botonDetails);

            gridLineas.Columns[(int)ColumnasGrid.oRDEN].Visible = false;
            gridLineas.Columns[(int)ColumnasGrid.SlpCode].Visible = false;

            gridLineas.Columns[(int)ColumnasGrid.Vendedor].Width = 180;
            gridLineas.Columns[(int)ColumnasGrid.Sucursal].Width = 100;
            gridLineas.Columns[(int)ColumnasGrid.Objetivo].Width = 100;
            gridLineas.Columns[(int)ColumnasGrid.Venta].Width = 100;
            gridLineas.Columns[(int)ColumnasGrid.VentavsCuota].Width = 100;
            gridLineas.Columns[(int)ColumnasGrid.Porvender].Width = 100;
            gridLineas.Columns[(int)ColumnasGrid.TendenciaFinMesM].Width = 100;
            gridLineas.Columns[(int)ColumnasGrid.TendenciaFinMesP].Width = 100;

            gridLineas.Columns[(int)ColumnasGrid.Objetivo].DefaultCellStyle.Format = "C0";
            gridLineas.Columns[(int)ColumnasGrid.Venta].DefaultCellStyle.Format = "C0";
            gridLineas.Columns[(int)ColumnasGrid.VentavsCuota].DefaultCellStyle.Format = "P2";
            gridLineas.Columns[(int)ColumnasGrid.Porvender].DefaultCellStyle.Format = "C0";
            gridLineas.Columns[(int)ColumnasGrid.TendenciaFinMesM].DefaultCellStyle.Format = "C0";
            gridLineas.Columns[(int)ColumnasGrid.TendenciaFinMesP].DefaultCellStyle.Format = "P2";

            gridLineas.Columns[(int)ColumnasGrid.Objetivo].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridLineas.Columns[(int)ColumnasGrid.Venta].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridLineas.Columns[(int)ColumnasGrid.VentavsCuota].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridLineas.Columns[(int)ColumnasGrid.Porvender].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridLineas.Columns[(int)ColumnasGrid.TendenciaFinMesM].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridLineas.Columns[(int)ColumnasGrid.TendenciaFinMesP].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;


            gridLineas.Columns[(int)ColumnasGrid.Objetivo].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 204);
            gridLineas.Columns[(int)ColumnasGrid.TendenciaFinMesM].DefaultCellStyle.BackColor = Color.FromArgb(252, 213, 180);
        }

        ///<sumary>
        /// Método que llena los TextBox Dias
        /// </sumary>
        private void CargarDias()
        {
            SqlCommand command1 = new SqlCommand("PJ_ScoreCardEfectividad", conection);
            command1.CommandType = CommandType.StoredProcedure;
            command1.Parameters.AddWithValue("@TipoConsulta", TipoConsulta.DiasMes);
            command1.Parameters.AddWithValue("@Cliente", string.Empty);
            command1.Parameters.AddWithValue("@Sucursales", string.Empty);
            command1.Parameters.AddWithValue("@Vendedores", string.Empty);
            command1.Parameters.AddWithValue("@Presupuesto", 0);
            command1.Parameters.AddWithValue("@Fecha", Fecha);

            SqlCommand command = new SqlCommand("PJ_ScoreCardEfectividad", conection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@TipoConsulta", TipoConsulta.DiasTranscurridos);
            command.Parameters.AddWithValue("@Cliente", string.Empty);
            command.Parameters.AddWithValue("@Sucursales", string.Empty);
            command.Parameters.AddWithValue("@Vendedores", string.Empty);
            command.Parameters.AddWithValue("@Presupuesto", 0);
            command.Parameters.AddWithValue("@Fecha", Fecha);

            try
            {
                conection.Open();
                SqlDataReader reader = command1.ExecuteReader();
                if (reader.Read())
                    DiasMes = Convert.ToDecimal(reader[0]);

                SqlDataReader reader1 = command.ExecuteReader();
                if (reader1.Read())
                    DiasTranscurridos = Convert.ToDecimal(reader1[0]);

                DiasRestantes = DiasMes - DiasTranscurridos;
                AvanceOptimo = DiasTranscurridos / DiasMes;

                txtDiasMes.Text = DiasMes.ToString();
                txtDiasTranscurridos.Text = DiasTranscurridos.ToString();
                txtDiasRestantes.Text = DiasRestantes.ToString();
                txtAvanceOptimo.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "HalcoNET", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conection.Close();
            }
        }

        private void CargarDiasReales()
        {
            SqlCommand command1 = new SqlCommand("PJ_ScoreCardEfectividad", conection);
            command1.CommandType = CommandType.StoredProcedure;
            command1.Parameters.AddWithValue("@TipoConsulta", 10);
            command1.Parameters.AddWithValue("@Cliente", string.Empty);
            command1.Parameters.AddWithValue("@Sucursales", string.Empty);
            command1.Parameters.AddWithValue("@Vendedores", string.Empty);
            command1.Parameters.AddWithValue("@Presupuesto", 0);
            command1.Parameters.AddWithValue("@Fecha", Fecha);

            SqlCommand command = new SqlCommand("PJ_ScoreCardEfectividad", conection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@TipoConsulta", 11);
            command.Parameters.AddWithValue("@Cliente", string.Empty);
            command.Parameters.AddWithValue("@Sucursales", string.Empty);
            command.Parameters.AddWithValue("@Vendedores", string.Empty);
            command.Parameters.AddWithValue("@Presupuesto", 0);
            command.Parameters.AddWithValue("@Fecha", Fecha);

            try
            {
                conection.Open();
                SqlDataReader reader = command1.ExecuteReader();
                if (reader.Read())
                    DiasMes = Convert.ToDecimal(reader[0]);

                SqlDataReader reader1 = command.ExecuteReader();
                if (reader1.Read())
                    DiasTranscurridos = Convert.ToDecimal(reader1[0]);

                DiasRestantes = DiasMes - DiasTranscurridos;
                AvanceOptimo = DiasTranscurridos / DiasMes;
                txtAvanceOptimo.Text = AvanceOptimo.ToString("P1");
                //txtDiasMes.Text = DiasMes.ToString();
                //txtDiasTranscurridos.Text = DiasTranscurridos.ToString();
                //txtDiasRestantes.Text = DiasRestantes.ToString();
                //txtAvanceOptimo.Text = AvanceOptimo.ToString("P2");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "HalcoNET", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conection.Close();
            }
        }
        #endregion


        #region EVENTOS
        private void btnPresentar_Click(object sender, EventArgs e)
        {
            Esperar();
            Fecha = DateTime.Parse(dateTimePicker1.Text);
            CargarDias();
            CargarDiasReales();
            Sucursales = CrearCadena(clbSucursal);
            if (RolUsuario != (int)ClasesSGUV.Propiedades.RolesHalcoNET.Ventas)
            {
                Vendedores = CrearCadena(clbVendedor);
            }
            try
            {
                gridLineas.DataSource = null;
                gridTotales.DataSource = null;
                gridLineas.Columns.Clear();

                SqlCommand commandLienasMoradas = new SqlCommand("PJ_ScoreCardEfectividad", conection);
                commandLienasMoradas.CommandType = CommandType.StoredProcedure;
                commandLienasMoradas.Parameters.AddWithValue("@TipoConsulta", TipoConsulta.LineasMoradas);
                commandLienasMoradas.Parameters.AddWithValue("@Cliente", string.Empty);
                commandLienasMoradas.Parameters.AddWithValue("@Sucursales", Sucursales);
                commandLienasMoradas.Parameters.AddWithValue("@Vendedores", Vendedores);
                commandLienasMoradas.Parameters.AddWithValue("@Presupuesto", 0);
                commandLienasMoradas.Parameters.AddWithValue("@Fecha", Fecha);
                FechaOK = Fecha;
                commandLienasMoradas.CommandTimeout = 0;
                DataTable table = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = commandLienasMoradas;
                adapter.Fill(table);
                gridLineas.DataSource = table;

                DarFormatoGrid();

                decimal Objetivo = 0;
                decimal Venta = 0;
                decimal VentavsCuota = 0;
                decimal PorVender = 0;
                decimal PronosticoFinMesM = 0;
                decimal PronosticoFinMesP = 0;

                foreach (DataGridViewRow item in gridLineas.Rows)
                {
                    Objetivo += Convert.ToDecimal(item.Cells[(int)ColumnasGrid.Objetivo].Value);
                    Venta += Convert.ToDecimal(item.Cells[(int)ColumnasGrid.Venta].Value);
                    PorVender += Convert.ToDecimal(item.Cells[(int)ColumnasGrid.Porvender].Value);
                    PronosticoFinMesM += Convert.ToDecimal(item.Cells[(int)ColumnasGrid.TendenciaFinMesM].Value);
                }

                if (Objetivo != 0 && DiasTranscurridos != 0)
                {
                    VentavsCuota = Venta / Objetivo;
                    PronosticoFinMesP = ((Venta / DiasTranscurridos) * DiasMes) /Objetivo;
                }

                DataTable tblTotales = new DataTable();
                tblTotales.Columns.Add("Objetivo");
                tblTotales.Columns.Add("Venta");
                tblTotales.Columns.Add("Venta vs Cuota");
                tblTotales.Columns.Add("Por vender");
                tblTotales.Columns.Add("Tendencia fin mes($)");
                tblTotales.Columns.Add("Tendencia fin mes(%)");

                DataRow registro = tblTotales.NewRow();
                registro["Objetivo"] = Objetivo.ToString("C0");
                registro["Venta"] = Venta.ToString("C0");
                registro["Venta vs Cuota"] = VentavsCuota.ToString("P2");
                registro["Por vender"] = PorVender.ToString("C0");
                registro["Tendencia fin mes($)"] = PronosticoFinMesM.ToString("C0");
                registro["Tendencia fin mes(%)"] = PronosticoFinMesP.ToString("P2");
                tblTotales.Rows.Add(registro);
                gridTotales.DataSource = tblTotales;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "HalcoNET", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Continuar();
            }
        }

        /// <summary>
        /// Evento que ocurre al hacer click en el checkedlistbox
        /// Selecciona todas o deselecciona todas
        /// </summary>
        /// <param name="sender">Objeto que produce el evento</param>
        /// <param name="e">Parámetros del evento</param>
        private void clbVendedor_Click(object sender, EventArgs e)
        {
            if (clbVendedor.SelectedIndex == 0)
            {
                if (clbVendedor.CheckedIndices.Contains(0))
                {
                    for (int item = 1; item < clbVendedor.Items.Count; item++)
                    {
                        clbVendedor.SetItemChecked(item, false);
                    }
                }
                else
                {
                    for (int item = 1; item < clbVendedor.Items.Count; item++)
                    {
                        clbVendedor.SetItemChecked(item, true);
                    }
                }
            }
        }
        
        /// <summary>
        /// Evento que ocurre al hacer click en el checkedlistbox
        /// Selecciona todas o deselecciona todas
        /// </summary>
        /// <param name="sender">Objeto que produce el evento</param>
        /// <param name="e">Parámetros del evento</param>
        private void clbSucursal_Click(object sender, EventArgs e)
        {
            if (clbSucursal.SelectedIndex == 0)
            {
                if (clbSucursal.CheckedIndices.Contains(0))
                {
                    for (int item = 1; item < clbSucursal.Items.Count; item++)
                    {
                        clbSucursal.SetItemChecked(item, false);
                    }
                }
                else
                {
                    for (int item = 1; item < clbSucursal.Items.Count; item++)
                    {
                        clbSucursal.SetItemChecked(item, true);
                    }
                }
            }
        }

        private void mcFecha_DateChanged(object sender, DateRangeEventArgs e)
        {

        }

        private void gridLineas_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in gridLineas.Rows)
            {
                //pinta venta %
                if (Convert.ToDecimal(row.Cells[(int)ColumnasGrid.Venta].Value) > Convert.ToDecimal(row.Cells[(int)ColumnasGrid.Objetivo].Value))
                {
                    row.Cells[(int)ColumnasGrid.Venta].Style.BackColor = Color.FromArgb(0, 176, 80);//green
                }

                //pinta pronostico fin de mes %
                if (Convert.ToDecimal(row.Cells[(int)ColumnasGrid.TendenciaFinMesP].Value) * 100 >= 100)
                {
                    row.Cells[(int)ColumnasGrid.TendenciaFinMesP].Style.ForeColor = Color.FromArgb(0, 176, 80);//green
                }
                else if (Convert.ToDecimal(row.Cells[(int)ColumnasGrid.TendenciaFinMesP].Value) * 100 < 100)
                {
                    row.Cells[(int)ColumnasGrid.TendenciaFinMesP].Style.ForeColor = Color.FromArgb(178, 34, 34);//red
                }

            }

        }
        #endregion 

        DataGridViewPrinter MyDataGridViewPrinter;
     
        ///   PrintDocument printDocument1;
  

        private bool SetUpTherinting()
        {
            PrintDialog MyPrintDialog = new PrintDialog();
            printDocument1 = new PrintDocument();
            MyPrintDialog.AllowCurrentPage = false;
            MyPrintDialog.AllowPrintToFile = false;
            MyPrintDialog.AllowSelection = false;
            MyPrintDialog.AllowSomePages = false;
            MyPrintDialog.PrintToFile = false;
            MyPrintDialog.ShowHelp = false;
            MyPrintDialog.ShowNetwork = false;
            if (MyPrintDialog.ShowDialog() != DialogResult.OK)
                return false;

            printDocument1.DocumentName = "Reporte";
            printDocument1.PrinterSettings = MyPrintDialog.PrinterSettings;
            printDocument1.DefaultPageSettings = MyPrintDialog.PrinterSettings.DefaultPageSettings;

            printDocument1.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);
            printDocument1.DefaultPageSettings.Landscape = true;

            if (MessageBox.Show("Desea ver el reporte centrado en la pagina", "HalcoNET", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                MyDataGridViewPrinter = new DataGridViewPrinter(gridLineas, printDocument1, true, true, "MI REPORTE", new Font("Arial", 12, FontStyle.Bold, GraphicsUnit.Point), Color.Black, true);
            else
                MyDataGridViewPrinter = new DataGridViewPrinter(gridLineas, printDocument1, false, true, "MI REPORTE", new Font("Arial", 12, FontStyle.Bold, GraphicsUnit.Point), Color.Black, true);
            return true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (SetUpTherinting())
                printDocument1.Print();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (SetUpTherinting())
            {
                PrintPreviewDialog m = new PrintPreviewDialog();
                m.Document = printDocument1;
                m.ShowDialog();
            }

           
        }

        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {

            bool more = MyDataGridViewPrinter.DrawDataGridView(e.Graphics);
            if (more)
                e.HasMorePages = true;

        }

        private void gridLineas_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex != -1)
                {
                    if (((System.Windows.Forms.DataGridView)(sender)).CurrentCell.ColumnIndex == (int)ColumnasGrid.Boton)
                    {
                        int SlpCode = Convert.ToInt32(gridLineas.Rows[((System.Windows.Forms.DataGridView)(sender)).CurrentCell.RowIndex].Cells[(int)ColumnasGrid.SlpCode].Value);
                        string nombre = Convert.ToString(gridLineas.Rows[((System.Windows.Forms.DataGridView)(sender)).CurrentCell.RowIndex].Cells[(int)ColumnasGrid.Vendedor].Value);
                        frmDetalleLineasHalcon details = new frmDetalleLineasHalcon(SlpCode, FechaOK, nombre, 0);
                        details.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inesperado: " + ex.Message, "HalcoNET", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            try
            {
                ExportarAExcel ex = new ExportarAExcel();
                ex.ExportarCobranza(gridLineas);
                MessageBox.Show("El archivo se creo con exito", "HalcoNET", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "HalcoNET", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void U_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == (int)Keys.Enter)
            {
                btnPresentar_Click(sender, e);
            }

            if (e.KeyChar == (char)Keys.Escape)
            {
                LineasMoradas_Load(sender, e);
            }
        }

        private void form_Shown(object sender, EventArgs e)
        {
            try
            {
                log.ID = log.Inicio();
            }
            catch (Exception)
            {

            }
        }

        private void form_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                log.Fin();
            }
            catch (Exception)
            {

            }
        }
    }
}
