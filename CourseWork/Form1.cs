using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace CourseWork
{
    public partial class Form1 : Form
    {
        private string dbPath;

        public Form1()
        {
            InitializeComponent();
        }

        private void SelectDataBase_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "SQLite база данных (*.sqlite)|*.sqlite";
                dialog.Title = "Выберите файл базы данных";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    dbPath = dialog.FileName;

                    try
                    {
                        using (SQLiteConnection conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                        {
                            conn.Open();

                            DataTable tables = conn.GetSchema("Tables");

                            comboBoxDataTable.Items.Clear();

                            foreach (DataRow row in tables.Rows)
                            {
                                comboBoxDataTable.Items.Add(row["TABLE_NAME"].ToString());
                            }

                            if (comboBoxDataTable.Items.Count > 0)
                                comboBoxDataTable.SelectedIndex = 0;
                        }

                        LoadValues();

                        MessageBox.Show("База данных успешно загружена");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка подключения: " + ex.Message);
                    }
                }
            }
        }

        private void LoadValues()
        {
            using (SQLiteConnection conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();

                string query = "SELECT * FROM Значения LIMIT 1";
                SQLiteCommand cmd = new SQLiteCommand(query, conn);

                SQLiteDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    ExponentialSmoothingCoefficient.Text = reader["A"].ToString();
                    MeasurementError.Text = reader["E"].ToString();

                    if (reader["Схема"] != DBNull.Value)
                    {
                        byte[] imgBytes = (byte[])reader["Схема"];

                        using (MemoryStream ms = new MemoryStream(imgBytes))
                        {
                            pictureBox1.Image = Image.FromStream(ms);
                            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                    }
                }
            }
        }

        private void OpenTableButton_Click(object sender, EventArgs e)
        {
            if (comboBoxDataTable.SelectedItem == null) return;

            string tableName = comboBoxDataTable.SelectedItem.ToString();

            using (SQLiteConnection conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();

                SQLiteDataAdapter adapter =
                    new SQLiteDataAdapter($"SELECT * FROM [{tableName}]", conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    for (int i = 1; i < dt.Columns.Count; i++) // пропускаем "Эпоха"
                    {
                        if (row[i] != DBNull.Value)
                        {
                            string s = row[i].ToString();

                            if (double.TryParse(s, System.Globalization.NumberStyles.Any,
                                                 System.Globalization.CultureInfo.GetCultureInfo("ru-RU"),
                                                 out double val))
                            {
                                row[i] = val;
                            }
                        }
                    }
                }

                dataGridView1.DataSource = dt;
            }
        }
    }
}