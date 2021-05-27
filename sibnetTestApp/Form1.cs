using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;
using System.IO;


namespace sibnetTestApp
{
    public partial class Form1 : Form
    {
        private static string Host = "127.0.0.1";
        private static string User = "postgres";
        private static string DBname = "usersdb";
        private static string Password = "1";
        private static string Port = "5432";
        private string connString;
        private List<string> procesessTypes = new List<string>();
        private List<string> departmentList = new List<string>();
        private DataTable dt;
        public Form1()
        {
            InitializeComponent();
            connString =
                String.Format(
                    "Server={0}; User Id={1}; Database={2}; Port={3}; Password={4};SSLMode=Prefer",
                    Host,
                    User,
                    DBname,
                    Port,
                    Password);

        }

        private void fileSelectBTN_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "csv files (*.csv)|*.csv";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    dt = CsvImport.NewDataTable(openFileDialog.FileName, ";", checkBox1.Checked, checkBox2.Checked);
                    dgItems.DataSource = dt;
                    dgItems.Refresh();
                    for (int j = 0; j < 2; j++)
                    {
                        for (int i = 0; i < dgItems.Rows.Count; i++)
                        {
                            if (!procesessTypes.Contains(dgItems.Rows[i].Cells[2].Value.ToString()) && dgItems.Rows[i].Cells[2].Value.ToString() != String.Empty)
                            {
                                procesessTypes.Add(dgItems.Rows[i].Cells[2].Value.ToString());
                            }
                            if (String.IsNullOrEmpty(dgItems.Rows[i].Cells[0].Value.ToString()) && String.IsNullOrEmpty(dgItems.Rows[i].Cells[1].Value.ToString()))
                            {
                                if (dgItems.Rows[i].Cells[2].Value.ToString() == String.Empty)
                                {
                                    dgItems.Rows.Remove(dgItems.Rows[i]);
                                }
                                else
                                {
                                    dgItems.Rows[i].Cells[0].Value = dgItems.Rows[i - 1].Cells[0].Value;
                                    dgItems.Rows[i].Cells[1].Value = dgItems.Rows[i - 1].Cells[1].Value;
                                }
                            }

                            if (dgItems.Rows[i].Cells[1].Value.ToString() != String.Empty && (dgItems.Rows[i].Cells[0].Value.ToString() == String.Empty && dgItems.Rows[i].Cells[2].Value.ToString() == String.Empty))
                            {
                                departmentList.Add(dgItems.Rows[i].Cells[1].Value.ToString());
                                dgItems.Rows.Remove(dgItems.Rows[i]);

                            }
                        }
                       
                    }



                }
            }
        }

        private void SubmitBTN_Click(object sender, EventArgs e)
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var command = new NpgsqlCommand("BEGIN ;" +
                    "DROP TABLE IF EXISTS \"processes\" ;" +
                    "DROP TABLE IF EXISTS \"processesTypes\" ;" +
                    "DROP TABLE IF EXISTS \"departmentList\" ;" +
                    "COMMIT ;", conn))
                {
                    command.ExecuteNonQuery();
                }
                System.Threading.Thread.Sleep(100);
                using (var command = new NpgsqlCommand("BEGIN ;" +
                    "CREATE TABLE  \"processes\" (code varchar(15), pName varchar(150), department varchar(30)) ;" +
                    "CREATE TABLE  \"processesTypes\" (id serial NOT NULL, pName varchar(150)) ;" +
                    "CREATE TABLE  \"departmentList\" (id serial NOT NULL, department varchar(150)) ;" +
                    "COMMIT ;", conn))
                {
                    command.ExecuteNonQuery();
                }
                System.Threading.Thread.Sleep(100);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    using (var command = new NpgsqlCommand("INSERT INTO processes (code, pName, department) VALUES (@code, @pName, @department)", conn))
                    {
                        command.Parameters.AddWithValue("code", dt.Rows[i].ItemArray[0].ToString());
                        command.Parameters.AddWithValue("pName", dt.Rows[i].ItemArray[1].ToString());
                        command.Parameters.AddWithValue("department", dt.Rows[i].ItemArray[2].ToString());
                        command.ExecuteNonQuery();
                    }
                }
                for (int i = 0; i < procesessTypes.Count; i++)
                {
                    using (var command = new NpgsqlCommand("INSERT INTO \"processesTypes\" (pName) VALUES (@pName)", conn))
                    {
                        command.Parameters.AddWithValue("pName", procesessTypes[i]);
                        command.ExecuteNonQuery();
                    }
                }
                for (int i = 0; i < departmentList.Count; i++)
                {
                    using (var command = new NpgsqlCommand("INSERT INTO \"departmentList\" (department) VALUES (@department)", conn))
                    {
                        command.Parameters.AddWithValue("department", departmentList[i]);
                        command.ExecuteNonQuery();
                    }
                }

            }


        }
    }
}

