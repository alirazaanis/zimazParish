using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Data.SqlClient;


namespace zimazParish
{

    public partial class Target : Form
    {

        #region Variables
        float initWidth;
        float initHeight;

        string bpp_FilterMain = "";
        readonly string bpp_FilterDetails = "ItemCategory";

        string tgt_FilterSTDA = "";
        string tgt_FilterSTD = "";

        string etgt_FilterSTDA = "";
        string etgt_FilterSTD = "";

        string stgt_FilterEdit = "";
        readonly string connectionString = "Data Source=59.103.166.127;Initial Catalog=Parish2017;" +
            "Persist Security Info=True;User ID=parish;Password=parish";

        bool cmb_Initialized = false;
        readonly int bpp_expandBtnIndex = 0;
        int bpp_expandedRow = 0;
        readonly string bpp_expandBtnImageName = "expand.png";
        readonly string bpp_collapseBtnImageName = "collapse.png";
        string bpp_expandBtnImage = "expand.png";
        readonly string bpp_expandBtnName = "+";
        readonly string bpp_expandBtnImageLoc = "C:/myapps/zimazParish/zimazParish/Images/";

        DataTable bpp_DetailDT = new DataTable();
        private readonly DataGridView bpp_MainDGV = new DataGridView();
        private readonly DataGridView bpp_DetailDGV = new DataGridView();
        private readonly DataGridView tgt_STDDGV = new DataGridView();
        private readonly DataGridView tgt_STDADGV = new DataGridView();
        private readonly DataGridView etgt_PRV = new DataGridView();
        private readonly DataGridView etgt_STDDGV = new DataGridView();
        private readonly DataGridView etgt_STDADGV = new DataGridView();
        private readonly DataGridView stgt_EditDGV = new DataGridView();
        #endregion

        public Target()
        {
            InitializeComponent();
            // bpp
            bpp_MainDGV.CellValueChanged += Bpp_DGVCellValueChanged;
            bpp_DetailDGV.CellValueChanged += Bpp_DGVCellValueChanged;

            etgt_STDDGV.CellValueChanged += Etgt_DGVCellValueChanged;
            etgt_STDADGV.CellValueChanged += Etgt_DGVCellValueChanged;

            stgt_EditDGV.CellValueChanged += Stgt_DGVCellValueChanged;

            bpp_MainDGV.CellValidating += Bpp_DGVCellValidating;
            bpp_DetailDGV.CellValidating += Bpp_DGVCellValidating;

            etgt_STDDGV.CellValidating += Bpp_DGVCellValidating;
            etgt_STDADGV.CellValidating += Bpp_DGVCellValidating;

            stgt_EditDGV.CellValidating += Bpp_DGVCellValidating;

            bpp_MainDGV.CellContentClick += new
                DataGridViewCellEventHandler(Bpp_MainDGV_CellContentClick);
            bpp_cmbRptWarehouse.SelectedIndexChanged += Bpp_cmbRpt;
            bpp_cmbRptStartDate.SelectedIndexChanged += Bpp_cmbRpt;
            bpp_cmbRptEndDate.SelectedIndexChanged += Bpp_cmbRpt;

            //tgt
            tgt_cmbWarehouse.SelectedIndexChanged += Tgt_cmbChanged;
            tgt_cmbPRVStartDate.SelectedIndexChanged += Tgt_cmbChanged;
            tgt_cmbPRVEndDate.SelectedIndexChanged += Tgt_cmbChanged;
            tgt_cmbSTDStartDate.SelectedIndexChanged += Tgt_cmbChanged;
            tgt_cmbSTDEndDate.SelectedIndexChanged += Tgt_cmbChanged;
            tgt_cmbSTDADate.SelectedIndexChanged += Tgt_cmbChanged;

            //etgt
            etgt_cmbWarehouse.SelectedIndexChanged += Etgt_cmbChanged;
            etgt_cmbPRVStartDate.SelectedIndexChanged += Etgt_cmbChanged;
            etgt_cmbPRVEndDate.SelectedIndexChanged += Etgt_cmbChanged;
            etgt_cmbSTDStartDate.SelectedIndexChanged += Etgt_cmbChanged;
            etgt_cmbSTDEndDate.SelectedIndexChanged += Etgt_cmbChanged;
            etgt_cmbSTDADate.SelectedIndexChanged += Etgt_cmbChanged;

            stgt_cmbWarehouse.SelectedIndexChanged += Stgt_cmbChanged;
            stgt_cmbStartDate.SelectedIndexChanged += Stgt_cmbChanged;
            stgt_cmbEndDate.SelectedIndexChanged += Stgt_cmbChanged;

        }

        #region SQLServerFunctions
        private void ExecuteQuery(string sql)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(
                           connectionString))
                {

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Connection.Open();
                        cmd.ExecuteNonQuery();
                        cmd.Connection.Close();
                    }

                }
            }
            catch { }
        }

        public DataSet GetDataSet(string procName,
                    params SqlParameter[] paramters)
        {
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.CommandText = procName;
                            if (paramters != null)
                            {
                                cmd.Parameters.AddRange(paramters);
                            }
                            da.Fill(ds, "T");
                        }
                    }
                    conn.Close();
                }
            }
            catch { }
            return ds;
        }

        private DataSet GetDataSet(string sql)
        {
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                    {
                        da.Fill(ds, "T");
                    }
                    conn.Close();
                }
            }
            catch { }
            return ds;
        }

        private List<string> GetColumnInList(string sql)
        {
            List<string> l = new List<string>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand()
                    { Connection = conn, CommandText = sql })
                    {
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                l = dr.Cast<IDataRecord>()
                                     .Select(x => (string)x[0]).ToList();
                            }
                        }
                    }
                    conn.Close();
                }
            }
            catch { }
            return l;
        }
        #endregion

        #region VariousFunctions
        void Bpp_SetFilter()
        {
            if (bpp_cmbWarehouse.Items.Count > 0 && bpp_cmbEmployee.Items.Count > 0 &&
                bpp_cmbWarehouse.SelectedIndex > -1 && bpp_cmbEmployee.SelectedIndex > -1)
            {
                string FilterWarehouse = string.Format("WareHouseName LIKE '%{0}%'",
                bpp_cmbWarehouse.SelectedItem.ToString());
                string FilterEmployee = string.Format("EmployeeName LIKE '%{0}%'",
                    bpp_cmbEmployee.SelectedItem.ToString());
                bpp_FilterMain = FilterWarehouse + " AND " + FilterEmployee;
            }
        }

        void Tgt_SetFilterSTD()
        {
            if (tgt_cmbWarehouse.Items.Count > 0 &&
                tgt_cmbWarehouse.SelectedIndex > -1 &&
                tgt_cmbSTDStartDate.Items.Count > 0 &&
                tgt_cmbSTDStartDate.SelectedIndex > -1 &&
                tgt_cmbSTDEndDate.Items.Count > 0 &&
                tgt_cmbSTDEndDate.SelectedIndex > -1 &&
                tgt_cmbSTDADate.Items.Count > 0 &&
                tgt_cmbSTDADate.SelectedIndex > -1)
            {
                string FilterWarehouse = string.Format("t1.WareHouseName LIKE '%{0}%'",
                tgt_cmbWarehouse.SelectedItem.ToString());
                string FilterDate = string.Format("t1.DATE BETWEEN '{0}' AND '{1}'",
                    tgt_cmbSTDStartDate.SelectedItem.ToString(),
                    tgt_cmbSTDEndDate.SelectedItem.ToString());
                tgt_FilterSTD = FilterWarehouse + " AND " + FilterDate;
                tgt_FilterSTDA = FilterWarehouse + " AND " + "t1.DATE = '" + tgt_cmbSTDADate.SelectedItem.ToString() + "'";
            }
        }

        private void Etgt_SetFilterSTD()
        {
            if (etgt_cmbWarehouse.Items.Count > 0 &&
                etgt_cmbWarehouse.SelectedIndex > -1 &&
                etgt_cmbSTDStartDate.Items.Count > 0 &&
                etgt_cmbSTDStartDate.SelectedIndex > -1 &&
                etgt_cmbSTDEndDate.Items.Count > 0 &&
                etgt_cmbSTDEndDate.SelectedIndex > -1 &&
                etgt_cmbSTDADate.Items.Count > 0 &&
                etgt_cmbSTDADate.SelectedIndex > -1)
            {
                string FilterWarehouse = string.Format("WareHouseName LIKE '%{0}%'",
                etgt_cmbWarehouse.SelectedItem.ToString());
                string FilterDate = string.Format("DATE BETWEEN '{0}' AND '{1}'",
                    etgt_cmbSTDStartDate.SelectedItem.ToString(),
                    etgt_cmbSTDEndDate.SelectedItem.ToString());
                etgt_FilterSTD = FilterWarehouse + " AND " + FilterDate;
                etgt_FilterSTDA = FilterWarehouse + " AND " + "DATE = '" + etgt_cmbSTDADate.SelectedItem.ToString() + "'";
            }
        }

        void Stgt_SetFilter()
        {
            if (stgt_cmbWarehouse.Items.Count > 0 &&
                stgt_cmbWarehouse.SelectedIndex > -1)
            {
                string FilterWarehouse = string.Format("WareHouseName LIKE '%{0}%'",
                stgt_cmbWarehouse.SelectedItem.ToString());
                stgt_FilterEdit = FilterWarehouse;
            }
        }

        void SetDGV(DataGridView dgv, string Name)
        {
            dgv.Name = Name;
            dgv.ScrollBars = ScrollBars.Both;
            dgv.EditMode = 0;
            dgv.Columns.Clear();
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgv.AllowUserToResizeColumns = false;
            dgv.AllowUserToResizeRows = false;
            dgv.AutoGenerateColumns = true;
            dgv.RowHeadersVisible = false;
            dgv.AllowUserToAddRows = false;
            dgv.Location = new Point(0, 0);
        }

        void Bpp_MainDGV_Initialize()
        {
            SetDGV(bpp_MainDGV, "bpp_MainDGV");
            bpp_MainDGV.Size = new Size((int)initWidth - 25, 240);
            bpp_pnlGrid.Controls.Add(bpp_MainDGV);
            DataGridViewImageColumn expandBtn = new DataGridViewImageColumn
            {
                Name = bpp_expandBtnName,
                Image = Image.FromFile(bpp_expandBtnImageLoc + bpp_expandBtnImageName),
                Visible = Visible,
                Width = 26,
                SortMode = DataGridViewColumnSortMode.Automatic,
                Resizable = DataGridViewTriState.True
            };
            expandBtn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            expandBtn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            bpp_MainDGV.Columns.Add(expandBtn);
            string sql = "SELECT ItemCategoryId, ItemCategory, Bonus FROM " +
                "SaleManBonusCategoryWise Where SubCategory = '' AND " + bpp_FilterMain +
                " Order By ItemCategory";
            DataTable dt = GetDataSet(sql).Tables["T"];
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    bpp_MainDGV.DataSource = dt.DefaultView;
                    bpp_MainDGV.Columns[1].Width = 0;
                    bpp_MainDGV.Columns[0].ReadOnly = true;
                    bpp_MainDGV.Columns[1].ReadOnly = true;
                    bpp_MainDGV.Columns[2].ReadOnly = true;
                    bpp_MainDGV.Columns[3].ReadOnly = false;
                    bpp_MainDGV.Columns[3].DefaultCellStyle.Format = "#";
                }
            }
        }

        void Tgt_STDDGV_Initialize()
        {
            SetDGV(tgt_STDDGV, "tgt_STDDGV");
            tgt_STDDGV.Dock = DockStyle.Fill;
            tgt_pnlGridSTD.Controls.Add(tgt_STDDGV);
            string sql = "With Sales " +
            "As(SELECT format(CreatedOn, 'yyyy-MM-dd') AS DATE, " +
            "           WareHouseId, " +
            "           Format(SUM(Quantity),'#') AS Quantity, " +
            "           Format(SUM(GrossAmount),'#') AS Sale " +
            "    FROM SalesDetail " +
            "    GROUP BY format(CreatedOn, 'yyyy-MM-dd'), " +
            "             WareHouseId " +
            "   ), " +
            "     WarehouseSales " +
            "as (Select DATE, " +
            "           WareHouseName, " +
            "           Quantity, " +
            "           Sale " +
            "    from Sales t1 " +
            "        LEFT JOIN WareHouse t2 " +
            "            ON t2.WareHouseId = t1.WareHouseId " +
            "   ) " +
            "SELECT t1.DATE, " +
            "       t1.Quantity, " +
            "       t2.QuantityTarget, " +
            "       t1.Sale, " +
            "       Format(t2.SaleTarget,'#') As SaleTarget, " +
            "       CASE " +
            "           WHEN Quantity >= QuantityTarget " +
            "                or Sale >= SaleTarget then " +
            "               'Completed' " +
            "           ELSE " +
            "                'Not Completed' " +
            "       END AS Target " +
            "FROM WarehouseSales t1 " +
            "    LEFT JOIN SaleTargetDaily t2 " +
            "        ON t2.WareHouseName = t1.WareHouseName " +
            "           AND t1.DATE = t2.Date " +
            "WHERE " + tgt_FilterSTD + " " +
            "ORDER BY t1.DATE DESC";
            DataTable dt = GetDataSet(sql).Tables["T"];
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    tgt_STDDGV.DataSource = dt.DefaultView;
                    tgt_STDDGV.ReadOnly = true;
                }
            }
        }

        void Tgt_STDADGV_Initialize()
        {
            SetDGV(tgt_STDADGV, "tgt_STDADGV");
            tgt_STDADGV.Dock = DockStyle.Fill;
            tgt_pnlGridSTDA.Controls.Add(tgt_STDADGV);
            string sql = "With Sales " +
            "As(SELECT format(CreatedOn, 'yyyy-MM-dd') AS DATE, " +
            "           WareHouseId, " +
            "           SUM(Quantity) AS Quantity, " +
            "           SUM(GrossAmount) AS Sale " +
            "    FROM SalesDetail " +
            "    GROUP BY format(CreatedOn, 'yyyy-MM-dd'), " +
            "             WareHouseId " +
            "   ), " +
            "     WarehouseSales " +
            "as (Select DATE, " +
            "           WareHouseName, " +
            "           Quantity, " +
            "           Sale " +
            "    from Sales t1 " +
            "        LEFT JOIN WareHouse t2 " +
            "            ON t2.WareHouseId = t1.WareHouseId " +
            "   ), " +
            "     targetCompleted " +
            "as (select case " +
            "               when Quantity >= QuantityTarget " +
            "                    or sale >= SaleTarget then " +
            "                   'Completed' " +
            "               else " +
            "                'Not Completed' " +
            "           end as Target " +
            "    from WarehouseSales t1 " +
            "        LEFT JOIN SaleTargetDaily t2 " +
            "            ON t2.WareHouseName = t1.WareHouseName " +
            "               and t1.DATE = t2.Date " +
            "    where " + tgt_FilterSTDA +
            "   ) " +
            "SELECT Format(Date,'yyyy-MM-dd') As Date, " +
            "       EmployeeName, " +
            "       Case " +
            "           WHEN t2.Target = 'Completed' THEN " +
            "               Format(Amount,'#') " +
            "           ELSE " +
            "               0 " +
            "       END AS Amount " +
            "FROM SaleTargetDailyAmount t1, " +
            "     targetCompleted t2 " +
            "WHERE " + tgt_FilterSTDA;
            DataTable dt = GetDataSet(sql).Tables["T"];
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    tgt_STDADGV.DataSource = dt.DefaultView;
                    tgt_STDADGV.ReadOnly = true;
                }
            }
        }

        void Etgt_PRV_Initialize()
        {
            SetDGV(etgt_PRV, "etgt_PRV");
            etgt_PRV.Dock = DockStyle.Fill;
            etgt_pnlGridPrv.Controls.Add(etgt_PRV);
            SqlParameter[] paramsq;
            if (etgt_cmbWarehouse.Items.Count > 0 &&
                etgt_cmbWarehouse.SelectedIndex > -1 &&
                etgt_cmbPRVStartDate.Items.Count > 0 &&
                etgt_cmbPRVStartDate.SelectedIndex > -1 &&
                etgt_cmbPRVEndDate.Items.Count > 0 &&
                etgt_cmbPRVEndDate.SelectedIndex > -1)
            {
                paramsq = new SqlParameter[] {
                    new SqlParameter("@P_WareHouseName", etgt_cmbWarehouse.SelectedItem.ToString()),
                    new SqlParameter("@P_StartDate", etgt_cmbPRVStartDate.SelectedItem.ToString()),
                    new SqlParameter("@P_EndDate", etgt_cmbPRVEndDate.SelectedItem.ToString())
                };
            }
            else
            {
                paramsq = new SqlParameter[] {
                    new SqlParameter("@P_WareHouseName", "''"),
                    new SqlParameter("@P_StartDate", "''"),
                    new SqlParameter("@P_EndDate", "''")
                };
            }
            DataTable dt = GetDataSet("stp_RPT_PreviousTargetCrossTab", paramsq).Tables["T"];
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    etgt_PRV.DataSource = dt.DefaultView;
                    etgt_PRV.ReadOnly = true;
                }
            }
        }

        void Etgt_STDDGV_Initialize()
        {
            SetDGV(etgt_STDDGV, "etgt_STDDGV");
            etgt_STDDGV.Dock = DockStyle.Fill;
            etgt_pnlGridSTD.Controls.Add(etgt_STDDGV);
            string sql = "SELECT Date, QuantityTarget, SaleTarget FROM " +
                "SaleTargetDaily Where " + etgt_FilterSTD +
                " Order By Date ASC";
            DataTable dt = GetDataSet(sql).Tables["T"];
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    etgt_STDDGV.DataSource = dt.DefaultView;
                    etgt_STDDGV.Columns[0].ReadOnly = true;
                    etgt_STDDGV.Columns[1].ReadOnly = false;
                    etgt_STDDGV.Columns[1].DefaultCellStyle.Format = "#";
                    etgt_STDDGV.Columns[2].ReadOnly = false;
                    etgt_STDDGV.Columns[2].DefaultCellStyle.Format = "#";
                }
            }
        }

        void Etgt_STDADGV_Initialize()
        {
            SetDGV(etgt_STDADGV, "etgt_STDADGV");
            etgt_STDADGV.Dock = DockStyle.Fill;
            etgt_pnlGridSTDA.Controls.Add(etgt_STDADGV);
            string sql = "SELECT EmployeeName, Amount FROM " +
                "SaleTargetDailyAmount WHERE " + etgt_FilterSTDA +
                " Order By Date Desc";
            DataTable dt = GetDataSet(sql).Tables["T"];
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    etgt_STDADGV.DataSource = dt.DefaultView;
                    etgt_STDADGV.Columns[0].ReadOnly = true;
                    etgt_STDADGV.Columns[1].ReadOnly = false;
                    etgt_STDADGV.Columns[1].DefaultCellStyle.Format = "#";
                }
            }
        }

        void Stgt_EditDGV_Initialize()
        {
            SetDGV(stgt_EditDGV, "stgt_EditDGV");
            stgt_EditDGV.Dock = DockStyle.Fill;
            stgt_pnlGridEdit.Controls.Add(stgt_EditDGV);
            string sql = "SELECT EmployeeName, WOMEN, GENTS, KIDS, BAGS, ACCESSORIES, Goal, Amount FROM " +
                "SalemanTarget WHERE " + stgt_FilterEdit;
            DataTable dt = GetDataSet(sql).Tables["T"];
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    stgt_EditDGV.DataSource = dt.DefaultView;
                    stgt_EditDGV.Columns[0].ReadOnly = true;
                    for (int i = 1; i < 8; i++)
                    {
                        stgt_EditDGV.Columns[i].ReadOnly = false;
                        stgt_EditDGV.Columns[i].DefaultCellStyle.Format = "#";
                    }
                }
            }
        }

        void Bpp_DetailDGV_Initialize()
        {
            SetDGV(bpp_DetailDGV, "bpp_DetailDGV");
            bpp_pnlGrid.Controls.Add(bpp_DetailDGV);
            string sql = "SELECT ItemCategoryId, ItemCategory, SubCategory, Bonus FROM " +
                "SaleManBonusCategoryWise Where SubCategory <> '' AND " + bpp_FilterMain +
                " Order By ItemCategory, SubCategory";
            bpp_DetailDT = GetDataSet(sql).Tables["T"];
        }

        void Bpp_rptLoad()
        {
            if (bpp_cmbRptWarehouse.Items.Count > 0 &&
                bpp_cmbRptStartDate.Items.Count > 0 &&
                bpp_cmbRptEndDate.Items.Count > 0 &&
                bpp_cmbRptWarehouse.SelectedIndex > -1 &&
                bpp_cmbRptStartDate.SelectedIndex > -1 &&
                bpp_cmbRptEndDate.SelectedIndex > -1)
            {
                try
                {
                    RptBonusPerPiece rd = new RptBonusPerPiece();
                    rd.SetDatabaseLogon(DbConnectionInfo.UserName, DbConnectionInfo.Password,
                        DbConnectionInfo.ServerName, DbConnectionInfo.InitialCatalog);
                    rd.SetParameterValue("@P_WareHouseName", bpp_cmbRptWarehouse.SelectedItem.ToString());
                    rd.SetParameterValue("@P_StartDate", bpp_cmbRptStartDate.SelectedItem.ToString());
                    rd.SetParameterValue("@P_EndDate", bpp_cmbRptEndDate.SelectedItem.ToString());
                    bpp_rptViewer.ReportSource = rd;
                }
                catch { }
            }
        }

        void Tgt_rptLoad()
        {
            if (bpp_cmbRptWarehouse.Items.Count > 0 &&
                bpp_cmbRptStartDate.Items.Count > 0 &&
                bpp_cmbRptEndDate.Items.Count > 0 &&
                bpp_cmbRptWarehouse.SelectedIndex > -1 &&
                bpp_cmbRptStartDate.SelectedIndex > -1 &&
                bpp_cmbRptEndDate.SelectedIndex > -1)
            {
                try
                {
                    RptTarget rd = new RptTarget();
                    rd.SetDatabaseLogon(DbConnectionInfo.UserName, DbConnectionInfo.Password,
                        DbConnectionInfo.ServerName, DbConnectionInfo.InitialCatalog);
                    rd.SetParameterValue("@P_WareHouseName", tgt_cmbWarehouse.SelectedItem.ToString());
                    rd.SetParameterValue("@P_StartDate", tgt_cmbPRVStartDate.SelectedItem.ToString());
                    rd.SetParameterValue("@P_EndDate", tgt_cmbPRVEndDate.SelectedItem.ToString());
                    tgt_rptViewer.ReportSource = rd;
                }
                catch { }
            }
        }

        void Stgt_rptLoad()
        {
            if (stgt_cmbWarehouse.Items.Count > 0 &&
                stgt_cmbStartDate.Items.Count > 0 &&
                stgt_cmbEndDate.Items.Count > 0 &&
                stgt_cmbWarehouse.SelectedIndex > -1 &&
                stgt_cmbStartDate.SelectedIndex > -1 &&
                stgt_cmbEndDate.SelectedIndex > -1)
            {
                try
                {
                    RptSalemanTgt rd = new RptSalemanTgt();
                    rd.SetDatabaseLogon(DbConnectionInfo.UserName, DbConnectionInfo.Password,
                        DbConnectionInfo.ServerName, DbConnectionInfo.InitialCatalog);
                    rd.SetParameterValue("@P_WareHouseName", stgt_cmbWarehouse.SelectedItem.ToString());
                    rd.SetParameterValue("@P_StartDate", stgt_cmbStartDate.SelectedItem.ToString());
                    rd.SetParameterValue("@P_EndDate", stgt_cmbEndDate.SelectedItem.ToString());
                    stgt_rptViewer.ReportSource = rd;
                }
                catch { }
            }
        }
        #endregion

        #region Events
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            float size1 = this.Size.Width / initWidth;
            float size2 = this.Size.Height / initHeight;
            _ = new SizeF(size1, size2);
            initWidth = this.Size.Width;
            initHeight = this.Size.Height;
            bpp_MainDGV.Size = new Size((int)initWidth - 25, 240);
            bpp_DetailDGV.Size = new Size(bpp_MainDGV.Width - 200, 180);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            DbConnectionInfo.ParseConnectionString(connectionString);
            initWidth = this.Size.Width;
            initHeight = this.Size.Height;

            GetDates(out string prvStartDate, out string prvEndDate, out string stdStartDate, out string stdEndDate, out string stdaDate);

            GetColumnInList(
                "SELECT DISTINCT WareHouseName FROM SaleManBonusCategoryWise"
                ).ForEach(x => bpp_cmbWarehouse.Items.Add(x));
            GetColumnInList(
                "SELECT DISTINCT EmployeeName FROM SaleManBonusCategoryWise"
                ).ForEach(x => bpp_cmbEmployee.Items.Add(x));

            if (bpp_cmbWarehouse.Items.Count > 0) { bpp_cmbWarehouse.SelectedIndex = 0; }
            if (bpp_cmbEmployee.Items.Count > 0) { bpp_cmbEmployee.SelectedIndex = 0; }

            GetColumnInList(
                "SELECT DISTINCT WareHouseName FROM SaleManBonusCategoryWise"
                ).ForEach(x => bpp_cmbRptWarehouse.Items.Add(x));
            GetColumnInList(
                "SELECT DISTINCT TOP 100000 FORMAT(CreatedOn, 'yyyy-MM-dd') AS Date " +
                "FROM SalesDetail ORDER BY FORMAT(CreatedOn, 'yyyy-MM-dd') DESC"
                ).ForEach(x => bpp_cmbRptStartDate.Items.Add(x));
            GetColumnInList(
                "SELECT DISTINCT TOP 100000 FORMAT(CreatedOn, 'yyyy-MM-dd') AS Date " +
                "FROM SalesDetail ORDER BY FORMAT(CreatedOn, 'yyyy-MM-dd') DESC"
                ).ForEach(x => bpp_cmbRptEndDate.Items.Add(x));

            if (bpp_cmbRptWarehouse.Items.Count > 0) { bpp_cmbRptWarehouse.SelectedIndex = 0; }
            if (bpp_cmbRptStartDate.Items.Count > 0) { bpp_cmbRptStartDate.SelectedIndex = 6; }
            if (bpp_cmbRptEndDate.Items.Count > 0) { bpp_cmbRptEndDate.SelectedIndex = 0; }

            Bpp_SetFilter();
            Bpp_MainDGV_Initialize();
            Bpp_DetailDGV_Initialize();
            Bpp_rptLoad();

            GetColumnInList(
                "SELECT DISTINCT WareHouseName FROM WareHouse WHERE WareHouseName In ('Parish','Zimaz')"
                ).ForEach(x => tgt_cmbWarehouse.Items.Add(x));

            GetColumnInList(
                "SELECT DISTINCT TOP 100000 FORMAT(Date, 'yyyy-MM-dd') AS Date " +
                "FROM SaleTargetDaily ORDER BY FORMAT(Date, 'yyyy-MM-dd') DESC"
                ).ForEach(x => tgt_cmbPRVStartDate.Items.Add(x));
            GetColumnInList(
                "SELECT DISTINCT TOP 100000 FORMAT(Date, 'yyyy-MM-dd') AS Date " +
                "FROM SaleTargetDaily ORDER BY FORMAT(Date, 'yyyy-MM-dd') DESC"
                ).ForEach(x => tgt_cmbPRVEndDate.Items.Add(x));

            GetColumnInList(
                "SELECT DISTINCT TOP 100000 FORMAT(Date, 'yyyy-MM-dd') AS Date " +
                "FROM SaleTargetDaily ORDER BY FORMAT(Date, 'yyyy-MM-dd') DESC"
                ).ForEach(x => tgt_cmbSTDStartDate.Items.Add(x));
            GetColumnInList(
                "SELECT DISTINCT TOP 100000 FORMAT(Date, 'yyyy-MM-dd') AS Date " +
                "FROM SaleTargetDaily ORDER BY FORMAT(Date, 'yyyy-MM-dd') DESC"
                ).ForEach(x => tgt_cmbSTDEndDate.Items.Add(x));

            GetColumnInList(
                "SELECT DISTINCT TOP 100000 FORMAT(Date, 'yyyy-MM-dd') AS Date " +
                "FROM SaleTargetDaily ORDER BY FORMAT(Date, 'yyyy-MM-dd') DESC"
                ).ForEach(x => tgt_cmbSTDADate.Items.Add(x));

            if (tgt_cmbWarehouse.Items.Count > 0) { tgt_cmbWarehouse.SelectedIndex = 0; }
            if (tgt_cmbPRVStartDate.Items.Count > 0) { tgt_cmbPRVStartDate.SelectedItem = prvStartDate; }
            if (tgt_cmbPRVEndDate.Items.Count > 0) { tgt_cmbPRVEndDate.SelectedItem = prvEndDate; }
            if (tgt_cmbSTDStartDate.Items.Count > 0) { tgt_cmbSTDStartDate.SelectedItem = stdStartDate; }
            if (tgt_cmbSTDEndDate.Items.Count > 0) { tgt_cmbSTDEndDate.SelectedItem = stdEndDate; }
            if (tgt_cmbSTDADate.Items.Count > 0) { tgt_cmbSTDADate.SelectedItem = stdaDate; }

            Tgt_SetFilterSTD();
            Tgt_STDDGV_Initialize();
            Tgt_STDADGV_Initialize();
            Tgt_rptLoad();

            GetColumnInList(
                "SELECT DISTINCT WareHouseName FROM WareHouse WHERE WareHouseName In ('Parish','Zimaz')"
                ).ForEach(x => etgt_cmbWarehouse.Items.Add(x));

            GetColumnInList(
                "SELECT DISTINCT TOP 100000 FORMAT(Date, 'yyyy-MM-dd') AS Date " +
                "FROM SaleTargetDaily ORDER BY FORMAT(Date, 'yyyy-MM-dd') DESC"
                ).ForEach(x => etgt_cmbPRVStartDate.Items.Add(x));
            GetColumnInList(
                "SELECT DISTINCT TOP 100000 FORMAT(Date, 'yyyy-MM-dd') AS Date " +
                "FROM SaleTargetDaily ORDER BY FORMAT(Date, 'yyyy-MM-dd') DESC"
                ).ForEach(x => etgt_cmbPRVEndDate.Items.Add(x));

            GetColumnInList(
                "SELECT DISTINCT TOP 100000 FORMAT(Date, 'yyyy-MM-dd') AS Date " +
                "FROM SaleTargetDaily ORDER BY FORMAT(Date, 'yyyy-MM-dd') DESC"
                ).ForEach(x => etgt_cmbSTDStartDate.Items.Add(x));
            GetColumnInList(
                "SELECT DISTINCT TOP 100000 FORMAT(Date, 'yyyy-MM-dd') AS Date " +
                "FROM SaleTargetDaily ORDER BY FORMAT(Date, 'yyyy-MM-dd') DESC"
                ).ForEach(x => etgt_cmbSTDEndDate.Items.Add(x));

            GetColumnInList(
                "SELECT DISTINCT TOP 100000 FORMAT(Date, 'yyyy-MM-dd') AS Date " +
                "FROM SaleTargetDaily ORDER BY FORMAT(Date, 'yyyy-MM-dd') DESC"
                ).ForEach(x => etgt_cmbSTDADate.Items.Add(x));

            if (etgt_cmbWarehouse.Items.Count > 0) { etgt_cmbWarehouse.SelectedIndex = 0; }
            if (etgt_cmbPRVStartDate.Items.Count > 0) { etgt_cmbPRVStartDate.SelectedItem = prvStartDate; }
            if (etgt_cmbPRVEndDate.Items.Count > 0) { etgt_cmbPRVEndDate.SelectedItem = prvEndDate; }
            if (etgt_cmbSTDStartDate.Items.Count > 0) { etgt_cmbSTDStartDate.SelectedItem = stdStartDate; }
            if (etgt_cmbSTDEndDate.Items.Count > 0) { etgt_cmbSTDEndDate.SelectedItem = stdEndDate; }
            if (etgt_cmbSTDADate.Items.Count > 0) { etgt_cmbSTDADate.SelectedItem = stdaDate; }

            Etgt_SetFilterSTD();
            Etgt_STDDGV_Initialize();
            Etgt_STDADGV_Initialize();
            Etgt_PRV_Initialize();


            GetColumnInList(
                "SELECT DISTINCT WareHouseName FROM WareHouse WHERE WareHouseName In ('Parish','Zimaz')"
                ).ForEach(x => stgt_cmbWarehouse.Items.Add(x));
            GetColumnInList(
                "SELECT DISTINCT TOP 100000 FORMAT(CreatedOn, 'yyyy-MM-dd') AS Date " +
                "FROM SalesDetail ORDER BY FORMAT(CreatedOn, 'yyyy-MM-dd') DESC"
                ).ForEach(x => stgt_cmbStartDate.Items.Add(x));
            GetColumnInList(
                "SELECT DISTINCT TOP 100000 FORMAT(CreatedOn, 'yyyy-MM-dd') AS Date " +
                "FROM SalesDetail ORDER BY FORMAT(CreatedOn, 'yyyy-MM-dd') DESC"
                ).ForEach(x => stgt_cmbEndDate.Items.Add(x));

            if (stgt_cmbWarehouse.Items.Count > 0) { stgt_cmbWarehouse.SelectedIndex = 0; }
            if (stgt_cmbStartDate.Items.Count > 0) { stgt_cmbStartDate.SelectedIndex = 6; }
            if (stgt_cmbEndDate.Items.Count > 0) { stgt_cmbEndDate.SelectedIndex = 0; }

            Stgt_SetFilter();
            Stgt_EditDGV_Initialize();
            Stgt_rptLoad();
            cmb_Initialized = true;
        }
        private void Bpp_DGVCellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            string headerText = dgv.Columns[e.ColumnIndex].HeaderText;
            if (!headerText.Equals("Bonus") ||
                !headerText.Equals("QuantityTarget") ||
                !headerText.Equals("SaleTarget") ||
                !headerText.Equals("WOMEN") ||
                !headerText.Equals("GENTS") ||
                !headerText.Equals("KIDS") ||
                !headerText.Equals("BAGS") ||
                !headerText.Equals("ACCESSORIES") ||
                !headerText.Equals("Goal") ||
                !headerText.Equals("Amount")) return;
            if (!decimal.TryParse(e.FormattedValue.ToString() == "" ? "0" :
                e.FormattedValue.ToString(), out decimal output))
            {
                MessageBox.Show("Must be numeric");
                e.Cancel = true;
            }
            else if (output < 0)
            {
                MessageBox.Show("Must not be negative");
                e.Cancel = true;
            }
        }
        private void Bpp_DGVCellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            ExecuteQuery("Update SaleManBonusCategoryWise Set Bonus = " +
                dgv.Rows[e.RowIndex].Cells[3].Value.ToString() +
                " WHERE ItemCategoryId = " +
                dgv.Rows[e.RowIndex].Cells[dgv.Name == "bpp_MainDGV" ? 1 : 0].Value.ToString() +
                " AND " + bpp_FilterMain);
        }
        private void Etgt_DGVCellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            if (dgv.Name == "etgt_STDADGV")
            {
                ExecuteQuery("Update SaleTargetDailyAmount Set Amount = " +
                    dgv.Rows[e.RowIndex].Cells[1].Value.ToString() +
                    " WHERE EmployeeName = '" +
                    dgv.Rows[e.RowIndex].Cells[0].Value.ToString() +
                    "' AND " + etgt_FilterSTDA);
            }
            else
            {
                ExecuteQuery("Update SaleTargetDaily Set QuantityTarget = " +
                  dgv.Rows[e.RowIndex].Cells[1].Value.ToString() +
                  ", SaleTarget = " +
                  dgv.Rows[e.RowIndex].Cells[2].Value.ToString() +
                  " WHERE Date = '" +
                  DateTime.Parse(dgv.Rows[e.RowIndex].Cells[0].Value.ToString()).ToString("yyyy-MM-dd") +
                  "' AND WareHouseName = '" +
                  etgt_cmbWarehouse.SelectedItem.ToString() + "'");
            }
        }

        private void Stgt_DGVCellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            ExecuteQuery("Update SalemanTarget Set " +
                "WOMEN = " +
              dgv.Rows[e.RowIndex].Cells[1].Value.ToString() +
              ", GENTS = " +
              dgv.Rows[e.RowIndex].Cells[2].Value.ToString() +
              ", KIDS = " +
              dgv.Rows[e.RowIndex].Cells[3].Value.ToString() +
              ", BAGS = " +
              dgv.Rows[e.RowIndex].Cells[4].Value.ToString() +
              ", ACCESSORIES = " +
              dgv.Rows[e.RowIndex].Cells[5].Value.ToString() +
              ", Goal = " +
              dgv.Rows[e.RowIndex].Cells[6].Value.ToString() +
              ", Amount = " +
              dgv.Rows[e.RowIndex].Cells[7].Value.ToString() +
              " WHERE EmployeeName = '" +
              dgv.Rows[e.RowIndex].Cells[0].Value.ToString() +
              "' AND WareHouseName = '" +
              stgt_cmbWarehouse.SelectedItem.ToString() + "'");
        }

        private void Bpp_MainDGV_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == bpp_expandBtnIndex)
            {
                foreach (DataGridViewRow row in bpp_MainDGV.Rows)
                {
                    row.Cells[bpp_expandBtnIndex].Value =
                        Image.FromFile(bpp_expandBtnImageLoc + bpp_expandBtnImageName);
                }
                if (bpp_expandBtnImage == bpp_expandBtnImageName || bpp_expandedRow != e.RowIndex)
                {
                    if (e.RowIndex > -1)
                    {
                        bpp_expandBtnImage = bpp_collapseBtnImageName;
                        bpp_expandedRow = e.RowIndex;
                        bpp_MainDGV.Rows[e.RowIndex].Cells[e.ColumnIndex].Value =
                            Image.FromFile(bpp_expandBtnImageLoc + bpp_expandBtnImage);
                        bpp_DetailDGV.Visible = true;
                        string FilterExpression =
                            bpp_MainDGV.Rows[e.RowIndex].Cells[bpp_FilterDetails].Value.ToString();
                        bpp_MainDGV.Controls.Add(bpp_DetailDGV);
                        Rectangle dgvRectangle = bpp_MainDGV.GetCellDisplayRectangle(1, e.RowIndex, true);
                        bpp_DetailDGV.Size = new Size(bpp_MainDGV.Width - 200, 180 - dgvRectangle.Y + 20);
                        bpp_DetailDGV.Location = new Point(dgvRectangle.X, dgvRectangle.Y + 20);
                        DataView detailView = new DataView(bpp_DetailDT)
                        {
                            RowFilter = bpp_FilterDetails + " = '" + FilterExpression + "'"
                        };
                        if (detailView.Count > 0)
                        {
                            bpp_DetailDGV.DataSource = detailView;
                            bpp_DetailDGV.Columns[0].Width = 0;
                            bpp_DetailDGV.Columns[1].Width = 0;
                            bpp_DetailDGV.Columns[2].Width = 300;
                            bpp_DetailDGV.Columns[3].ReadOnly = false;
                            bpp_DetailDGV.Columns[0].ReadOnly = true;
                            bpp_DetailDGV.Columns[1].ReadOnly = true;
                            bpp_DetailDGV.Columns[2].ReadOnly = true;
                            bpp_DetailDGV.Columns[3].ReadOnly = false;
                            bpp_DetailDGV.Columns[3].DefaultCellStyle.Format = "#";
                        }
                    }
                }
                else
                {
                    bpp_expandBtnImage = bpp_expandBtnImageName;
                    bpp_MainDGV.Rows[e.RowIndex].Cells[e.ColumnIndex].Value =
                        Image.FromFile(bpp_expandBtnImageLoc + bpp_expandBtnImage);
                    bpp_DetailDGV.Visible = false;
                }
            }
            else
            {
                bpp_DetailDGV.Visible = false;
            }
        }
        private void Bpp_pnlGridPaint(object sender, PaintEventArgs e)
        {

        }
        private void Bpp_cmbSelectedIndexChanged(object sender, EventArgs e)
        {
            if (bpp_cmbWarehouse.Items.Count > 0 &&
                bpp_cmbEmployee.Items.Count > 0 &&
                bpp_cmbWarehouse.SelectedIndex > -1 &&
                bpp_cmbEmployee.SelectedIndex > -1 &&
                cmb_Initialized)
            {
                Bpp_SetFilter();
                Bpp_MainDGV_Initialize();
                Bpp_DetailDGV_Initialize();
            }
        }

        private void Bpp_cmbRpt(object sender, EventArgs e)
        {
            if (bpp_cmbRptWarehouse.Items.Count > 0 &&
                bpp_cmbRptStartDate.Items.Count > 0 &&
                bpp_cmbRptEndDate.Items.Count > 0 &&
                bpp_cmbRptWarehouse.SelectedIndex > -1 &&
                bpp_cmbRptStartDate.SelectedIndex > -1 &&
                bpp_cmbRptEndDate.SelectedIndex > -1 &&
                cmb_Initialized)
            {

                Bpp_rptLoad();
            }
        }

        private void Tgt_cmbChanged(object sender, EventArgs e)
        {
            if (tgt_cmbWarehouse.Items.Count > 0 &&
                tgt_cmbWarehouse.SelectedIndex > -1 &&
                tgt_cmbPRVStartDate.Items.Count > 0 &&
                tgt_cmbPRVStartDate.SelectedIndex > -1 &&
                tgt_cmbPRVEndDate.Items.Count > 0 &&
                tgt_cmbPRVEndDate.SelectedIndex > -1 &&
                tgt_cmbSTDStartDate.Items.Count > 0 &&
                tgt_cmbSTDStartDate.SelectedIndex > -1 &&
                tgt_cmbSTDEndDate.Items.Count > 0 &&
                tgt_cmbSTDEndDate.SelectedIndex > -1 &&
                tgt_cmbSTDADate.Items.Count > 0 &&
                tgt_cmbSTDADate.SelectedIndex > -1 &&
                cmb_Initialized)
            {
                Tgt_SetFilterSTD();
                Tgt_STDDGV_Initialize();
                Tgt_STDADGV_Initialize();
                Tgt_rptLoad();
            }
        }

        private void Etgt_cmbChanged(object sender, EventArgs e)
        {

            if (etgt_cmbWarehouse.Items.Count > 0 &&
                etgt_cmbWarehouse.SelectedIndex > -1 &&
                etgt_cmbPRVStartDate.Items.Count > 0 &&
                etgt_cmbPRVStartDate.SelectedIndex > -1 &&
                etgt_cmbPRVEndDate.Items.Count > 0 &&
                etgt_cmbPRVEndDate.SelectedIndex > -1 &&
                etgt_cmbSTDStartDate.Items.Count > 0 &&
                etgt_cmbSTDStartDate.SelectedIndex > -1 &&
                etgt_cmbSTDEndDate.Items.Count > 0 &&
                etgt_cmbSTDEndDate.SelectedIndex > -1 &&
                etgt_cmbSTDADate.Items.Count > 0 &&
                etgt_cmbSTDADate.SelectedIndex > -1 &&
                cmb_Initialized)
            {
                Etgt_SetFilterSTD();
                Etgt_STDDGV_Initialize();
                Etgt_STDADGV_Initialize();
                Etgt_PRV_Initialize();
            }
        }

        private void Stgt_cmbChanged(object sender, EventArgs e)
        {
            if (stgt_cmbWarehouse.Items.Count > 0 &&
                stgt_cmbWarehouse.SelectedIndex > -1 &&
                stgt_cmbStartDate.Items.Count > 0 &&
                stgt_cmbStartDate.SelectedIndex > -1 &&
                stgt_cmbEndDate.Items.Count > 0 &&
                stgt_cmbEndDate.SelectedIndex > -1 &&
                cmb_Initialized)
            {
                Stgt_SetFilter();
                Stgt_EditDGV_Initialize();
                Stgt_rptLoad();
            }
        }


        private void Button2_Click(object sender, EventArgs e)
        {
            GetDates(out string prvStartDate, out string prvEndDate, out string stdStartDate, out string stdEndDate, out string stdaDate);

            if (tgt_cmbWarehouse.Items.Count > 0) { tgt_cmbWarehouse.SelectedIndex = 0; }
            if (tgt_cmbPRVStartDate.Items.Count > 0) { tgt_cmbPRVStartDate.SelectedItem = prvStartDate; }
            if (tgt_cmbPRVEndDate.Items.Count > 0) { tgt_cmbPRVEndDate.SelectedItem = prvEndDate; }
            if (tgt_cmbSTDStartDate.Items.Count > 0) { tgt_cmbSTDStartDate.SelectedItem = stdStartDate; }
            if (tgt_cmbSTDEndDate.Items.Count > 0) { tgt_cmbSTDEndDate.SelectedItem = stdEndDate; }
            if (tgt_cmbSTDADate.Items.Count > 0) { tgt_cmbSTDADate.SelectedItem = stdaDate; }

            Tgt_SetFilterSTD();
            Tgt_STDDGV_Initialize();
            Tgt_STDADGV_Initialize();
            Tgt_rptLoad();
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            GetDates(out string prvStartDate, out string prvEndDate, out string stdStartDate, out string stdEndDate, out string stdaDate);

            if (etgt_cmbWarehouse.Items.Count > 0) { etgt_cmbWarehouse.SelectedIndex = 0; }
            if (etgt_cmbPRVStartDate.Items.Count > 0) { etgt_cmbPRVStartDate.SelectedItem = prvStartDate; }
            if (etgt_cmbPRVEndDate.Items.Count > 0) { etgt_cmbPRVEndDate.SelectedItem = prvEndDate; }
            if (etgt_cmbSTDStartDate.Items.Count > 0) { etgt_cmbSTDStartDate.SelectedItem = stdStartDate; }
            if (etgt_cmbSTDEndDate.Items.Count > 0) { etgt_cmbSTDEndDate.SelectedItem = stdEndDate; }
            if (etgt_cmbSTDADate.Items.Count > 0) { etgt_cmbSTDADate.SelectedItem = stdaDate; }

            Etgt_SetFilterSTD();
            Etgt_STDDGV_Initialize();
            Etgt_STDADGV_Initialize();
            Etgt_PRV_Initialize();
        }


        void GetDates(out string prvStartDate, out string prvEndDate, out string stdStartDate, out string stdEndDate, out string stdaDate)
        {
            int CurrentMonthYear = DateTime.Now.Year;
            int CurrentMonth = DateTime.Now.Month;
            int PreivousMonthYear = DateTime.Now.AddMonths(-1).Year;
            int PreivousMonth = DateTime.Now.AddMonths(-1).Month;
            int lastDayOfCurrentMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            int lastDayOfPreviousMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month);
            prvStartDate = PreivousMonthYear.ToString("D4") + "-" + PreivousMonth.ToString("D2") + "-01";
            prvEndDate = PreivousMonthYear.ToString("D4") + "-" + PreivousMonth.ToString("D2") + "-" + lastDayOfPreviousMonth.ToString("D2");
            stdStartDate = CurrentMonthYear.ToString("D4") + "-" + CurrentMonth.ToString("D2") + "-01";
            stdEndDate = CurrentMonthYear.ToString("D4") + "-" + CurrentMonth.ToString("D2") + "-" + lastDayOfCurrentMonth.ToString("D2");
            stdaDate = DateTime.Now.ToString("yyyy-MM-dd");
        }
        #endregion

    }
}
