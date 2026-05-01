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

        private double prevA;
        private double prevE;

        public Form1()
        {
            InitializeComponent();
        }

        private SQLiteConnection GetConnection()
        {
            return new SQLiteConnection($"Data Source={dbPath};Version=3;");
        }

        private void SelectDataBase_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "SQLite база данных (*.sqlite)|*.sqlite";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    dbPath = dialog.FileName;

                    try
                    {
                        using (var conn = GetConnection())
                        {
                            conn.Open();

                            DataTable tables = conn.GetSchema("Tables");
                            comboBoxDataTable.Items.Clear();

                            foreach (DataRow row in tables.Rows)
                                comboBoxDataTable.Items.Add(row["TABLE_NAME"].ToString());

                            if (comboBoxDataTable.Items.Count > 0)
                                comboBoxDataTable.SelectedIndex = 0;
                        }

                        LoadValues();
                        LoadTable("Координаты Z контрольных точек");

                        MessageBox.Show("БД загружена");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка: " + ex.Message);
                    }
                }
            }
        }

        private void LoadValues()
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                var cmd = new SQLiteCommand("SELECT * FROM Значения LIMIT 1", conn);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    prevA = Convert.ToDouble(reader["A"]);
                    prevE = Convert.ToDouble(reader["E"]);

                    ExponentialSmoothingCoefficient.Text = prevA.ToString();
                    MeasurementError.Text = prevE.ToString();

                    if (reader["Схема"] != DBNull.Value)
                    {
                        byte[] imgBytes = (byte[])reader["Схема"];
                        using (MemoryStream ms = new MemoryStream(imgBytes))
                        {
                            pictureBox1.Image = Image.FromStream(ms);
                            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                        }
                    }
                }
            }
        }

        private void LoadTable(string tableName)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                var adapter = new SQLiteDataAdapter($"SELECT * FROM [{tableName}]", conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                dataGridView1.DataSource = dt;
            }
        }

        private void OpenTableButton_Click(object sender, EventArgs e)
        {
            if (comboBoxDataTable.SelectedItem == null) return;
            LoadTable(comboBoxDataTable.SelectedItem.ToString());
        }

        private void Recalculate()
        {
            double alpha = prevA;

            using (var conn = GetConnection())
            {
                conn.Open();

                var selectCmd = new SQLiteCommand("SELECT * FROM [Данные] ORDER BY Эпоха", conn);

                var adapter = new SQLiteDataAdapter(selectCmd);
                var sourceDt = new DataTable();
                adapter.Fill(sourceDt);

                double[] prevValues = new double[20];
                bool firstRow = true;

                var resultDt = sourceDt.Copy();

                foreach (DataRow row in resultDt.Rows)
                {
                    for (int i = 1; i <= 20; i++)
                    {
                        string col = i.ToString();

                        if (row[col] == DBNull.Value)
                            continue;

                        double x = Convert.ToDouble(row[col]);
                        double s;

                        if (firstRow)
                            s = x;
                        else
                            s = alpha * x + (1 - alpha) * prevValues[i - 1];

                        prevValues[i - 1] = s;
                        row[col] = s;
                    }

                    firstRow = false;
                }

                foreach (DataRow row in resultDt.Rows)
                {
                    var update = new SQLiteCommand(@"
UPDATE [Координаты Z контрольных точек]
SET 
[1]=@c1,[2]=@c2,[3]=@c3,[4]=@c4,[5]=@c5,
[6]=@c6,[7]=@c7,[8]=@c8,[9]=@c9,[10]=@c10,
[11]=@c11,[12]=@c12,[13]=@c13,[14]=@c14,[15]=@c15,
[16]=@c16,[17]=@c17,[18]=@c18,[19]=@c19,[20]=@c20
WHERE Эпоха=@epoch", conn);

                    update.Parameters.AddWithValue("@epoch", row["Эпоха"]);

                    for (int i = 1; i <= 20; i++)
                        update.Parameters.AddWithValue($"@c{i}", row[i.ToString()]);

                    update.ExecuteNonQuery();
                }
            }
        }

        private void RecalculatePhase()
        {
            double E = prevE;

            using (var conn = GetConnection())
            {
                conn.Open();

                var cmd = new SQLiteCommand("SELECT * FROM [Координаты Z контрольных точек] ORDER BY Эпоха", conn);

                var adapter = new SQLiteDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                DataTable phase = new DataTable();
                phase.Columns.Add("Цикл");
                phase.Columns.Add("µ(t)");
                phase.Columns.Add("µ(t)-");
                phase.Columns.Add("µ(t)+");
                phase.Columns.Add("a(µ)");
                phase.Columns.Add("a(µ)-");
                phase.Columns.Add("a(µ)+");
                phase.Columns.Add("µ прогноз");
                phase.Columns.Add("µ прогноз-");
                phase.Columns.Add("µ прогноз+");
                phase.Columns.Add("a прогноз");
                phase.Columns.Add("a прогноз-");
                phase.Columns.Add("a прогноз+");

                DataTable monitor = new DataTable();
                monitor.Columns.Add("Цикл");
                monitor.Columns.Add("R");
                monitor.Columns.Add("L");
                monitor.Columns.Add("Состояние");

                double[] prevVector = new double[20];
                double prevMu = 0;
                bool first = true;
                int cycle = 0;

                foreach (DataRow row in dt.Rows)
                {
                    double[] vector = new double[20];

                    for (int i = 1; i <= 20; i++)
                    {
                        object val = row[i.ToString()];
                        vector[i - 1] = val == DBNull.Value ? 0.0 : Convert.ToDouble(val);
                    }

                    double mu = 0;

                    for (int i = 0; i < 20; i++)
                        mu += vector[i] * vector[i];

                    mu = Math.Sqrt(mu);

                    double a = first ? 0 : (mu - prevMu);

                    double muForecast = mu + a;
                    double aForecast = a;

                    double muMinus = mu - E;
                    double muPlus = mu + E;

                    double aMinus = a - E;
                    double aPlus = a + E;

                    var pr = phase.NewRow();

                    pr["Цикл"] = cycle;
                    pr["µ(t)"] = mu;
                    pr["µ(t)-"] = muMinus;
                    pr["µ(t)+"] = muPlus;
                    pr["a(µ)"] = a;
                    pr["a(µ)-"] = aMinus;
                    pr["a(µ)+"] = aPlus;
                    pr["µ прогноз"] = muForecast;
                    pr["µ прогноз-"] = muForecast - E;
                    pr["µ прогноз+"] = muForecast + E;
                    pr["a прогноз"] = aForecast;
                    pr["a прогноз-"] = aForecast - E;
                    pr["a прогноз+"] = aForecast + E;

                    phase.Rows.Add(pr);

                    double R = Math.Abs(mu - muForecast);

                    string state =
                        R < E ? "Норма" :
                        R < 2 * E ? "Предупреждение" :
                        "Авария";

                    var mr = monitor.NewRow();
                    mr["Цикл"] = cycle;
                    mr["R"] = R;
                    mr["L"] = E;
                    mr["Состояние"] = state;

                    monitor.Rows.Add(mr);

                    prevMu = mu;
                    Array.Copy(vector, prevVector, 20);

                    first = false;
                    cycle++;
                }

                dataGridViewPhase.DataSource = phase;
                dataGridViewСonditionMonitoring.DataSource = monitor;

                UpdatePhaseChart();
            }
        }

        private void apply_Click(object sender, EventArgs e)
        {
            var culture = CultureInfo.GetCultureInfo("ru-RU");

            bool okA = double.TryParse(
                ExponentialSmoothingCoefficient.Text,
                NumberStyles.Any,
                culture,
                out double A);

            bool okE = double.TryParse(
                MeasurementError.Text,
                NumberStyles.Any,
                culture,
                out double E);

            if (!okA || !okE || A < 0 || A > 1 || E < 0)
            {
                ExponentialSmoothingCoefficient.Text = prevA.ToString(culture);
                MeasurementError.Text = prevE.ToString(culture);

                MessageBox.Show("Некорректные значения. Восстановлены предыдущие.");
                return;
            }

            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction()) 
                    {
                        var cmd = new SQLiteCommand(
                            "UPDATE Значения SET A=@A, E=@E", conn);

                        cmd.Parameters.AddWithValue("@A", A);
                        cmd.Parameters.AddWithValue("@E", E);

                        cmd.ExecuteNonQuery();

                        transaction.Commit();
                    }
                }

                prevA = A;
                prevE = E;

                Recalculate();
                RecalculatePhase();
                LoadTable("Координаты Z контрольных точек");

                MessageBox.Show("Изменения применены");
            }
            catch (Exception ex)
            {
                ExponentialSmoothingCoefficient.Text = prevA.ToString(culture);
                MeasurementError.Text = prevE.ToString(culture);

                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }

        private void UpdatePhaseChart()
        {
            chartPhase.Series.Clear();

            if (dataGridViewPhase.DataSource == null) return;

            var dt = (DataTable)dataGridViewPhase.DataSource;

            AddSeriesIfChecked(checkBoxAMu, "a(µ)-", dt);
            AddSeriesIfChecked(checkBoxAM, "a(µ)", dt);
            AddSeriesIfChecked(checkBoxAMp, "a(µ)+", dt);

            AddSeriesIfChecked(checkBoxAMuForecast, "a прогноз-", dt);
            AddSeriesIfChecked(checkBoxAMForecast, "a прогноз", dt);
            AddSeriesIfChecked(checkBoxAMpForecast, "a прогноз+", dt);
        }

        private void AddSeriesIfChecked(CheckBox cb, string columnName, DataTable dt)
        {
            if (!cb.Checked) return;

            var series = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = cb.Text,
                ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line,
                BorderWidth = 2
            };

            foreach (DataRow row in dt.Rows)
            {
                if (row["Цикл"] == DBNull.Value || row[columnName] == DBNull.Value)
                    continue;

                double x = Convert.ToDouble(row["Цикл"]);
                double y = Convert.ToDouble(row[columnName]);

                series.Points.AddXY(x, y);
            }

            chartPhase.Series.Add(series);
        }

        private void addCycleWatch_Click(object sender, EventArgs e)
        {
            string tableName = "Координаты Z контрольных точек";

            using (var conn = GetConnection())
            {
                conn.Open();

                var cmd = new SQLiteCommand(
                    $"INSERT INTO [{tableName}] DEFAULT VALUES", conn);
                cmd.ExecuteNonQuery();
            }

            LoadTable(tableName);
        }

        private void deleteCycleWatch_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null) return;

            int id = Convert.ToInt32(dataGridView1.CurrentRow.Cells[0].Value);

            string tableName = "Координаты Z контрольных точек";

            using (var conn = GetConnection())
            {
                conn.Open();

                var cmd = new SQLiteCommand(
                    $"DELETE FROM [{tableName}] WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            LoadTable(tableName);
        }

        private void ClearCheckBox_Click(object sender, EventArgs e)
        {
            checkBoxAMu.Checked = false;
            checkBoxAM.Checked = false;
            checkBoxAMp.Checked = false;

            checkBoxAMuForecast.Checked = false;
            checkBoxAMForecast.Checked = false;
            checkBoxAMpForecast.Checked = false;
        }

        private void buttonSecontLevel_Click(object sender, EventArgs e)
        {
            tabControl.SelectedTab = tabSecond;
        }

        private void buttonCleatAll2_Click(object sender, EventArgs e)
        {
            checkBoxMuT.Checked = false;
            checkBoxAM.Checked = false;
            checkBoxMuTm.Checked = false;

            checkBoxMuTmForecast.Checked = false;
            checkBoxMuTForecast.Checked = false;
            checkBoxMuTpForecast.Checked = false;
        }

        private void checkBoxAMu_CheckedChanged(object sender, EventArgs e) => UpdatePhaseChart();
        private void checkBoxAM_CheckedChanged(object sender, EventArgs e) => UpdatePhaseChart();
        private void checkBoxAMp_CheckedChanged(object sender, EventArgs e) => UpdatePhaseChart();

        private void checkBoxAMuForecast_CheckedChanged(object sender, EventArgs e) => UpdatePhaseChart();
        private void checkBoxAMForecast_CheckedChanged(object sender, EventArgs e) => UpdatePhaseChart();
        private void checkBoxAMpForecast_CheckedChanged(object sender, EventArgs e) => UpdatePhaseChart();
    }
}