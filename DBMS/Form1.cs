using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;


namespace DBMS
{
    public partial class MainForm : Form
    {
        SqlConnection conn;
        SqlDataAdapter daParent, daChild;
        DataSet ds;
        BindingSource bsParent, bsChild;
        SqlCommandBuilder cb;
        Configuration config;

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            try
            {
                daChild.Update(ds, config.childTableName);
            }
            catch(SqlException sqlException)
            {
                Console.WriteLine("Update failed, cause:" + sqlException.Message);
            }
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            config = Configuration.readConfiguration("C:/Users/Marian-VM/source/repos/DBMS/DBMS/configurations.xml");

            parentLabel.Text = config.parentTableName;
            childLabel.Text = config.childTableName;

            conn = new SqlConnection(config.connectionString);

            daChild = new SqlDataAdapter(config.childSelectString, conn);
            daParent = new SqlDataAdapter(config.parentSelectString, conn);

            ds = new DataSet();
            daChild.Fill(ds, config.childTableName);
            daParent.Fill(ds, config.parentTableName);

            cb = new SqlCommandBuilder(daChild);
            cb.GetInsertCommand();
            cb.GetUpdateCommand();
            cb.GetDeleteCommand();

            string relationName = "FK_" + config.parentTableName + "_" + config.childTableName;

            ds.Relations.Add(relationName, ds.Tables[config.parentTableName].Columns[config.parentPrimaryKeyName],
                                           ds.Tables[config.childTableName].Columns[config.childForeignKeyName]);

            bsParent = new BindingSource();
            bsParent.DataSource = ds;
            bsParent.DataMember = config.parentTableName;

            bsChild = new BindingSource();
            bsChild.DataSource = bsParent;
            bsChild.DataMember = relationName;

            parentGrid.DataSource = bsParent;
            childGrid.DataSource = bsChild;
        }
    }
}
