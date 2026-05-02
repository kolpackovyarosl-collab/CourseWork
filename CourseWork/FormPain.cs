using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace CourseWork
{
    public partial class FormPain : Form
    {
        private string dbPath;
        private readonly Random random = new Random();

        private double prevA;
        private double prevE;
        private Dictionary<string, List<string>> blockPoints = new Dictionary<string, List<string>>();
        private Dictionary<string, DataTable> blockPhaseCache = new Dictionary<string, DataTable>();
        private Dictionary<string, DataTable> blockConditionCache = new Dictionary<string, DataTable>();

        private string currentBlock = "";

        public FormPain()
        {
            InitializeComponent();
        }

        private SQLiteConnection GetConnection()
        {
            return new SQLiteConnection($"Data Source={dbPath};Version=3;");
        }

        private void LoadDataColumns()
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                var adapter = new SQLiteDataAdapter(
                    "SELECT * FROM [Данные] LIMIT 1",
                    conn);

                DataTable dt = new DataTable();
                adapter.Fill(dt);

                listBoxAll.Items.Clear();

                for (int i = 1; i < dt.Columns.Count; i++)
                {
                    string point =
                        dt.Columns[i].ColumnName;

                    listBoxAll.Items.Add(point);
                }

                foreach (var block in blockPoints)
                {
                    foreach (string point in block.Value)
                    {
                        if (listBoxAll.Items.Contains(point))
                        {
                            listBoxAll.Items.Remove(point);
                        }
                    }
                }
            }
        }

        private void LoadValues()
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                using (var cmd = new SQLiteCommand(
                    "SELECT * FROM Значения LIMIT 1", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return;

                    prevA = Convert.ToDouble(reader["A"]);
                    prevE = Convert.ToDouble(reader["E"]);

                    ExponentialSmoothingCoefficient.Text = prevA.ToString();
                    MeasurementError.Text = prevE.ToString();

                    if (reader["Схема"] != DBNull.Value)
                    {
                        byte[] imgBytes =
                            (byte[])reader["Схема"];

                        using (var ms = new MemoryStream(imgBytes))
                        using (var image = Image.FromStream(ms))
                        {
                            pictureBox1.Image?.Dispose();

                            pictureBox1.Image =
                                new Bitmap(image);

                            pictureBox1.SizeMode =
                                PictureBoxSizeMode.Zoom;

                            pictureBoxObject.Image?.Dispose();

                            pictureBoxObject.Image =
                                new Bitmap(image);

                            pictureBoxObject.SizeMode =
                                PictureBoxSizeMode.Zoom;
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

        private void LoadBlocks()
        {
            CreateBlocksTable();

            blockPoints.Clear();

            using (var conn = GetConnection())
            {
                conn.Open();

                int blockCount = 0;

                var cmdCount = new SQLiteCommand(
                    "SELECT Количество FROM [Значения] LIMIT 1",
                    conn);

                blockCount =
                    Convert.ToInt32(cmdCount.ExecuteScalar());

                comboBoxSelectBox.Items.Clear();

                char letter = 'А';

                for (int i = 0; i < blockCount; i++)
                {
                    string block =
                        ((char)(letter + i)).ToString();

                    comboBoxSelectBox.Items.Add(block);
                    comboBoxSelectBox2.Items.Add(block);

                    blockPoints[block] =
                        new List<string>();
                }

                var cmd = new SQLiteCommand(
                    "SELECT * FROM [Блоки]",
                    conn);

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string block =
                        reader["Блок"].ToString();

                    string point =
                        reader["Точка"].ToString();

                    if (blockPoints.ContainsKey(block))
                    {
                        blockPoints[block].Add(point);
                    }
                }
            }

            comboBoxSelectBox.SelectedIndex = 0;
        }

        private void SaveCurrentBlock()
        {
            if (string.IsNullOrEmpty(currentBlock))
                return;

            blockPoints[currentBlock].Clear();

            foreach (var item in listBoxBlock.Items)
            {
                blockPoints[currentBlock].Add(
                    item.ToString());
            }
        }
        private void ShowBlock(string block)
        {
            listBoxBlock.Items.Clear();

            foreach (string point in blockPoints[block])
            {
                listBoxBlock.Items.Add(point);
            }
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
                        RecalculatePhase();
                       
                        LoadBlocks();
                        if (comboBoxSelectBox2.Items.Count > 0)
                        {
                            comboBoxSelectBox2.SelectedIndex = 0;
                        }
                        LoadDataColumns();

                        MessageBox.Show("БД загружена");

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка: " + ex.Message);
                    }
                }
            }
        }

        private void RecalculatePhaseLevel2(string block)
        {
            double alpha = prevA;
            double beta = 0.2;

            using (var conn = GetConnection())
            {
                conn.Open();

                var cmd = new SQLiteCommand(
                    "SELECT * FROM [Координаты Z контрольных точек] ORDER BY Эпоха",
                    conn);

                var adapter = new SQLiteDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                var cmdParams = new SQLiteCommand(
                    "SELECT * FROM [Значения] LIMIT 1",
                    conn);

                var reader = cmdParams.ExecuteReader();

                double A = 0;
                double Eps = 0;

                if (reader.Read())
                {
                    A = Convert.ToDouble(reader["A"]);
                    Eps = Convert.ToDouble(reader["E"]);
                }

                reader.Close();


                DataTable phase = new DataTable();
                phase.Columns.Add("Цикл");

                phase.Columns.Add("µ");
                phase.Columns.Add("a");

                phase.Columns.Add("µ+");
                phase.Columns.Add("µ-");

                phase.Columns.Add("a+");
                phase.Columns.Add("a-");

                phase.Columns.Add("µ прогноз");
                phase.Columns.Add("µ прогноз-");
                phase.Columns.Add("µ прогноз+");
                phase.Columns.Add("a прогноз");
                phase.Columns.Add("a прогноз-");
                phase.Columns.Add("a прогноз+");

                DataTable condition = new DataTable();
                condition.Columns.Add("Цикл");
                condition.Columns.Add("R");
                condition.Columns.Add("L");
                condition.Columns.Add("Состояние");
                condition.Columns.Add("Декомпозиция");

                if (!blockPoints.ContainsKey(block))
                    return;

                var columns = blockPoints[block];

                Dictionary<string, double> prevMu = new Dictionary<string, double>();
                Dictionary<string, double> prevA = new Dictionary<string, double>();

                for (int rowIndex = 0; rowIndex < dt.Rows.Count; rowIndex++)
                {
                    DataRow row = dt.Rows[rowIndex];

                    double muSum = 0;
                    double aSum = 0;
                    int valid = 0;

                    foreach (var col in columns)
                    {
                        if (!dt.Columns.Contains(col)) continue;
                        if (row[col] == DBNull.Value) continue;

                        double x = Convert.ToDouble(row[col]);

                        double mu = prevMu.ContainsKey(col)
                            ? alpha * x + (1 - alpha) * prevMu[col]
                            : x;

                        double a = prevA.ContainsKey(col)
                            ? beta * (mu - prevMu[col])
                            : 0;

                        prevMu[col] = mu;
                        prevA[col] = a;

                        muSum += mu;
                        aSum += a;
                        valid++;
                    }

                    if (valid == 0) continue;

                    double muAvg = muSum / valid;
                    double aAvg = aSum / valid;

                    double muPlus = muAvg + A * Eps;
                    double muMinus = muAvg - A * Eps;

                    double aPlus = aAvg + Eps;
                    double aMinus = aAvg - Eps;

                    double muForecast = muAvg + aAvg;
                    double muForecastMinus = muForecast - A * Eps;
                    double muForecastPlus = muForecast + A * Eps;

                    double aForecast = aAvg;
                    double aForecastMinus = aForecast - Eps;
                    double aForecastPlus = aForecast + Eps;

                    double R = A * Eps;
                    double L = Math.Abs(muAvg - muForecast);

                    string state =
                        (L <= R * 0.5) ? "Норма" :
                        (L <= R) ? "Предупреждение" :
                        "Авария";

                    string decomp =
                        (state == "Авария" || Math.Abs(aAvg) > Eps * 2)
                        ? "Переход на следующий уровень"
                        : "Уровень стабилен";

                    DataRow pr = phase.NewRow();

                    pr["Цикл"] = rowIndex;

                    pr["µ"] = Math.Round(muAvg, 4);
                    pr["a"] = Math.Round(aAvg, 4);

                    pr["µ+"] = Math.Round(muPlus, 4);
                    pr["µ-"] = Math.Round(muMinus, 4);

                    pr["a+"] = Math.Round(aPlus, 4);
                    pr["a-"] = Math.Round(aMinus, 4);

                    pr["µ прогноз"] = Math.Round(muForecast, 4);
                    pr["µ прогноз-"] = Math.Round(muForecastMinus, 4);
                    pr["µ прогноз+"] = Math.Round(muForecastPlus, 4);

                    pr["a прогноз"] = Math.Round(aForecast, 4);
                    pr["a прогноз-"] = Math.Round(aForecastMinus, 4);
                    pr["a прогноз+"] = Math.Round(aForecastPlus, 4);

                    phase.Rows.Add(pr);


                    condition.Rows.Add(
                        rowIndex,
                        Math.Round(R, 4),
                        Math.Round(L, 4),
                        state,
                        decomp
                    );
                }

                dataGridView3.DataSource = phase;
                dataGridView2.DataSource = condition;
            }
        }
        private void Recalculate()
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                var adapter = new SQLiteDataAdapter(
                    "SELECT * FROM [Данные] ORDER BY Эпоха", conn);

                var source = new DataTable();
                adapter.Fill(source);

                int n = source.Rows.Count;
                if (n < 2) return;

                int nextEpoch;

                using (var cmd = new SQLiteCommand(
                    "SELECT IFNULL(MAX(Эпоха), -1) FROM [Координаты Z контрольных точек]", conn))
                {
                    object result = cmd.ExecuteScalar();

                    int maxEpoch = Convert.ToInt32(result);

                    nextEpoch = maxEpoch + 1;
                }

                using (var insert = new SQLiteCommand(conn))
                {
                    string columns = "[Эпоха],";
                    string values = "@epoch,";

                    for (int i = 1; i <= 20; i++)
                    {
                        columns += $"[{i}],";
                        values += $"@c{i},";
                    }

                    columns = columns.TrimEnd(',');
                    values = values.TrimEnd(',');

                    insert.CommandText =
                        $"INSERT INTO [Координаты Z контрольных точек] ({columns}) VALUES ({values})";

                    insert.Parameters.AddWithValue("@epoch", nextEpoch);

                    for (int i = 1; i <= 20; i++)
                    {
                        string col = i.ToString();

                        double sum = 0;

                        for (int k = 0; k < n - 1; k++)
                        {
                            double z1 = Convert.ToDouble(source.Rows[k][col]);
                            double z2 = Convert.ToDouble(source.Rows[k + 1][col]);

                            double diff = z2 - z1;
                            sum += diff * diff;
                        }

                        double d = Math.Sqrt(sum / (n - 1));

                        double prevZ = Convert.ToDouble(source.Rows[n - 1][col]);

                        double rnd = (random.NextDouble() * 2.0 - 1.0) * d;

                        double newZ = Math.Round(prevZ + rnd, 4);

                        insert.Parameters.AddWithValue($"@c{i}", newZ);
                    }

                    insert.ExecuteNonQuery();
                }
            }

            LoadTable("Координаты Z контрольных точек");
        }

        private void RecalculatePhase()
        {
            double alpha = prevA;
            double beta = 0.2;

            using (var conn = GetConnection())
            {
                conn.Open();

                var cmd = new SQLiteCommand(
                    "SELECT * FROM [Координаты Z контрольных точек] ORDER BY Эпоха",
                    conn);

                var adapter = new SQLiteDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                var cmdParams = new SQLiteCommand(
                    "SELECT * FROM [Значения] LIMIT 1",
                    conn);

                var reader = cmdParams.ExecuteReader();

                double A = 0;
                double Eps = 0;
                int count = 0;

                if (reader.Read())
                {
                    A = Convert.ToDouble(reader["A"]);
                    Eps = Convert.ToDouble(reader["E"]);
                    count = Convert.ToInt32(reader["Количество"]);
                }

                reader.Close();

                DataTable phase = new DataTable();
                phase.Columns.Add("Цикл");

                phase.Columns.Add("µ");
                phase.Columns.Add("a");

                phase.Columns.Add("µ+");
                phase.Columns.Add("µ-");

                phase.Columns.Add("a+");
                phase.Columns.Add("a-");

                phase.Columns.Add("µ прогноз");
                phase.Columns.Add("µ прогноз-");
                phase.Columns.Add("µ прогноз+");
                phase.Columns.Add("a прогноз");
                phase.Columns.Add("a прогноз-");
                phase.Columns.Add("a прогноз+");

                DataTable condition = new DataTable();
                condition.Columns.Add("Цикл");
                condition.Columns.Add("R");
                condition.Columns.Add("L");
                condition.Columns.Add("Состояние");
                condition.Columns.Add("Декомпозиция");

                Dictionary<string, double> prevMu = new Dictionary<string, double>();
                Dictionary<string, double> prevA = new Dictionary<string, double>();

                for (int rowIndex = 0; rowIndex < dt.Rows.Count; rowIndex++)
                {
                    DataRow row = dt.Rows[rowIndex];

                    double muSum = 0;
                    double aSum = 0;
                    int validCount = 0;

                    for (int i = 1; i <= count; i++)
                    {
                        string col = i.ToString();
                        if (!dt.Columns.Contains(col)) continue;
                        if (row[col] == DBNull.Value) continue;

                        double x = Convert.ToDouble(row[col]);

                        double prevMuVal = prevMu.ContainsKey(col) ? prevMu[col] : x;
                        double prevAVal = prevA.ContainsKey(col) ? prevA[col] : 0;

                        double mu = alpha * x + (1 - alpha) * prevMuVal;

                        double a = beta * (mu - prevMuVal);

                        prevMu[col] = mu;
                        prevA[col] = a;

                        prevMu[col] = mu;
                        prevA[col] = a;

                        muSum += mu;
                        aSum += a;
                        validCount++;
                    }

                    if (validCount == 0) continue;

                    double muAvg = muSum / validCount;
                    double aAvg = aSum / validCount;


                    double muPlus = muAvg + A * Eps;
                    double muMinus = muAvg - A * Eps;

                    double aPlus = aAvg + Eps;
                    double aMinus = aAvg - Eps;


                    double muForecast = muAvg + aAvg;
                    double muForecastMinus = muForecast - A * Eps;
                    double muForecastPlus = muForecast + A * Eps;
                    double aForecast = aAvg;
                    double aForecastMinus = aForecast - Eps;
                    double aForecastPlus = aForecast + Eps;

                    double R = A * Eps;
                    double L = Math.Abs(muAvg - muForecast);

                    string state;

                    if (L <= R * 0.5)
                        state = "Норма";
                    else if (L <= R)
                        state = "Предупреждение";
                    else
                        state = "Авария";

                    string decomp =
                        (state == "Авария" || Math.Abs(aAvg) > Eps * 2)
                        ? "Переход на следующий уровень"
                        : "Уровень стабилен";


                    DataRow pr = phase.NewRow();

                    pr["Цикл"] = rowIndex;

                    pr["µ"] = Math.Round(muAvg, 4);
                    pr["a"] = Math.Round(aAvg, 4);

                    pr["µ+"] = Math.Round(muPlus, 4);
                    pr["µ-"] = Math.Round(muMinus, 4);

                    pr["a+"] = Math.Round(aPlus, 4);
                    pr["a-"] = Math.Round(aMinus, 4);

                    pr["µ прогноз"] = Math.Round(muForecast, 4);
                    pr["µ прогноз-"] = Math.Round(muForecastMinus, 4);
                    pr["µ прогноз+"] = Math.Round(muForecastPlus, 4);
                    pr["a прогноз"] = Math.Round(aForecast, 4);
                    pr["a прогноз-"] = Math.Round(aForecastMinus, 4);
                    pr["a прогноз+"] = Math.Round(aForecastPlus, 4);

                    phase.Rows.Add(pr);

                    condition.Rows.Add(
                        rowIndex,
                        Math.Round(R, 4),
                        Math.Round(L, 4),
                        state,
                        decomp
                    );
                }

                dataGridViewPhase.DataSource = phase;
                dataGridViewСonditionMonitoring.DataSource = condition;

                UpdatePhaseChart();
                UpdateFuncChart();
            }
        }

        private void UpdatePhaseChart()
        {
            chartPhase.Series.Clear();

            if (dataGridViewPhase.DataSource == null) return;

            var dt = (DataTable)dataGridViewPhase.DataSource;

            AddSeriesIfChecked(checkBoxAMu, "a-", dt);
            AddSeriesIfChecked(checkBoxAM, "a", dt);
            AddSeriesIfChecked(checkBoxAMp, "a+", dt);

            AddSeriesIfChecked(checkBoxAMuForecast, "a прогноз-", dt);
            AddSeriesIfChecked(checkBoxAMForecast, "a прогноз", dt);
            AddSeriesIfChecked(checkBoxAMpForecast, "a прогноз+", dt);
        }

        private void UpdateFuncChart()
        {
            chartFunc.Series.Clear();

            if (dataGridViewPhase.DataSource == null) return;

            var dt = (DataTable)dataGridViewPhase.DataSource;

            AddFuncSeries(checkBoxMuT, "µ", dt, false);
            AddFuncSeries(checkBoxMuTm, "µ-", dt, false);
            AddFuncSeries(checkBoxMuTp, "µ+", dt, false);

            AddFuncSeries(checkBoxMuTForecast, "µ прогноз", dt, true);
            AddFuncSeries(checkBoxMuTmForecast, "µ прогноз-", dt, true);
            AddFuncSeries(checkBoxMuTpForecast, "µ прогноз+", dt, true);
        }

        private void RefreshPhaseChartA()
        {
            chartA.Series.Clear();

            if (dataGridViewPhase.DataSource == null)
                return;

            var dt = (DataTable)dataGridViewPhase.DataSource;

            DrawPhaseSeriesA(checkBoxAMlimit, "a-", dt, false);
            DrawPhaseSeriesA(checkBoxAlimit, "a", dt, false);
            DrawPhaseSeriesA(checkBoxAPlimit, "a+", dt, false);

            DrawPhaseSeriesA(checkBoxAMlimitForecast, "a прогноз-", dt, true);
            DrawPhaseSeriesA(checkBoxAlimitForecast, "a прогноз", dt, true);
            DrawPhaseSeriesA(checkBoxAPlimitForecast, "a прогноз+", dt, true);

            AutoScaleChartA();        }

        private void RefreshFuncChartF()
        {
            chartF.Series.Clear();

            if (dataGridViewPhase.DataSource == null)
                return;

            var dt = (DataTable)dataGridViewPhase.DataSource;

            DrawFuncSeriesF(checkBoxFMlimit, "µ-", dt, false);
            DrawFuncSeriesF(checkBoxFlimit, "µ", dt, false);
            DrawFuncSeriesF(checkBoxFPlimit, "µ+", dt, false);

            DrawFuncSeriesF(checkBoxFMlimitForecast, "µ прогноз-", dt, true);
            DrawFuncSeriesF(checkBoxFlimitForecast, "µ прогноз", dt, true);
            DrawFuncSeriesF(checkBoxFPlimitForecast, "µ прогноз+", dt, true);

            AutoScaleChartF();
        }

        private void DrawPhaseSeriesA(CheckBox cb, string column, DataTable dt, bool forecast)
        {
            if (!cb.Checked) return;

            var series = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = cb.Text,
                ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line,
                BorderWidth = 2
            };

            if (forecast)
                series.BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;

            series.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
            series.MarkerSize = 6;

            foreach (DataRow row in dt.Rows)
            {
                if (row["µ"] == DBNull.Value || row[column] == DBNull.Value)
                    continue;

                double x = Convert.ToDouble(row["µ"]);
                double y = Convert.ToDouble(row[column]);

                series.Points.AddXY(x, y);
            }

            chartA.Series.Add(series);
        }

        private void DrawFuncSeriesF(CheckBox cb, string column, DataTable dt, bool forecast)
        {
            if (!cb.Checked) return;

            var series = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = cb.Text,
                ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line,
                BorderWidth = 2
            };

            if (forecast)
                series.BorderDashStyle =
                    System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;

            series.MarkerStyle =
                System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;

            series.MarkerSize = 6;

            foreach (DataRow row in dt.Rows)
            {
                if (row["Цикл"] == DBNull.Value || row[column] == DBNull.Value)
                    continue;

                double x = Convert.ToDouble(row["Цикл"]);
                double y = Convert.ToDouble(row[column]);

                series.Points.AddXY(x, y);
            }

            chartF.Series.Add(series);
        }

        private void AutoScaleChartA()
        {
            var area = chartA.ChartAreas[0];

            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            foreach (var series in chartA.Series)
            {
                foreach (var p in series.Points)
                {
                    double x = p.XValue;
                    double y = p.YValues[0];

                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;

                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }

            if (minX != double.MaxValue)
            {
                double mx = (maxX - minX) * 0.1;
                area.AxisX.Minimum = minX - mx;
                area.AxisX.Maximum = maxX + mx;
            }

            if (minY != double.MaxValue)
            {
                double my = (maxY - minY) * 0.1;
                area.AxisY.Minimum = minY - my;
                area.AxisY.Maximum = maxY + my;
            }

            area.AxisX.IntervalAutoMode =
                System.Windows.Forms.DataVisualization.Charting.IntervalAutoMode.FixedCount;

            area.AxisY.IntervalAutoMode =
                System.Windows.Forms.DataVisualization.Charting.IntervalAutoMode.FixedCount;
        }

        private void AutoScaleChartF()
        {
            var area = chartF.ChartAreas[0];

            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            foreach (var s in chartF.Series)
            {
                foreach (var p in s.Points)
                {
                    double x = p.XValue;
                    double y = p.YValues[0];

                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;

                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }

            if (minX != double.MaxValue)
            {
                double mx = (maxX - minX) * 0.05;
                area.AxisX.Minimum = minX - mx;
                area.AxisX.Maximum = maxX + mx;
            }

            if (minY != double.MaxValue)
            {
                double my = (maxY - minY) * 0.1;
                area.AxisY.Minimum = minY - my;
                area.AxisY.Maximum = maxY + my;
            }

            area.AxisX.Interval = Math.Max(1, (int)((maxX - minX) / 10));
            area.AxisY.IntervalAutoMode =
                System.Windows.Forms.DataVisualization.Charting.IntervalAutoMode.FixedCount;

            chartF.ChartAreas[0].RecalculateAxesScale();
        }

        private void AddFuncSeries(CheckBox cb, string columnName, DataTable dt, bool isForecast)
        {
            if (!cb.Checked) return;

            var series = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = cb.Text,
                ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line,
                BorderWidth = 2
            };

            if (isForecast)
            {
                series.BorderDashStyle =
                    System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;
            }

            series.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
            series.MarkerSize = 6;
            series.MarkerColor = System.Drawing.Color.Black;

            double minY = double.MaxValue;
            double maxY = double.MinValue;

            foreach (DataRow row in dt.Rows)
            {
                if (row["Цикл"] == DBNull.Value || row[columnName] == DBNull.Value)
                    continue;

                double t = Convert.ToDouble(row["Цикл"]);
                double mu = Convert.ToDouble(row[columnName]);

                int idx = series.Points.AddXY(t, mu);

                series.Points[idx].Label = t.ToString();

                if (mu < minY) minY = mu;
                if (mu > maxY) maxY = mu;
            }

            chartFunc.Series.Add(series);

            var area = chartFunc.ChartAreas[0];

            area.AxisX.Minimum = 0;
            area.AxisX.Maximum = 13;

            double margin = (maxY - minY) * 0.1;

            area.AxisY.Minimum = minY - margin;
            area.AxisY.Maximum = maxY + margin;

            area.AxisX.Interval = 1;
            area.AxisY.IntervalAutoMode =
                System.Windows.Forms.DataVisualization.Charting.IntervalAutoMode.FixedCount;
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

            if (columnName.Contains("прогноз"))
            {
                series.BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;
            }

            series.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
            series.MarkerSize = 6;
            series.MarkerColor = System.Drawing.Color.Black;

            int epoch = 0;

            foreach (DataRow row in dt.Rows)
            {
                if (row["µ"] == DBNull.Value || row[columnName] == DBNull.Value)
                    continue;

                double x = Convert.ToDouble(row["µ"]);
                double y = Convert.ToDouble(row[columnName]);

                int idx = series.Points.AddXY(x, y);

                series.Points[idx].Label = epoch.ToString();

                epoch++;
            }

            chartPhase.Series.Add(series);
        }

        private void MoveSelectedItem(
            ListBox source,
            ListBox target
            )
        {
            if (source.SelectedItem == null)
                return;

            string point =
                source.SelectedItem.ToString();

            target.Items.Add(point);

            source.Items.Remove(point);

            SaveCurrentBlock();
        }

        private void CreateBlocksTable()
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                var cmd = new SQLiteCommand(
                @"CREATE TABLE IF NOT EXISTS [Блоки]
        (
            Блок TEXT NOT NULL,
            Точка TEXT NOT NULL
        )", conn);

                cmd.ExecuteNonQuery();
            }
        }

        private void buttonSaveBlock_Click(
    object sender,
    EventArgs e)
        {
            SaveCurrentBlock();

            int size = -1;

            foreach (var block in blockPoints)
            {
                if (size == -1)
                {
                    size = block.Value.Count;
                    continue;
                }

                if (size != block.Value.Count)
                {
                    MessageBox.Show(
                        "Количество точек в блоках должно совпадать.");

                    return;
                }
            }

            using (var conn = GetConnection())
            {
                conn.Open();

                new SQLiteCommand(
                    "DELETE FROM [Блоки]",
                    conn).ExecuteNonQuery();

                foreach (var block in blockPoints)
                {
                    foreach (string point in block.Value)
                    {
                        var cmd = new SQLiteCommand(
                            @"INSERT INTO [Блоки]
                    (Блок, Точка)
                    VALUES
                    (@b,@p)", conn);

                        cmd.Parameters.AddWithValue(
                            "@b",
                            block.Key);

                        cmd.Parameters.AddWithValue(
                            "@p",
                            point);

                        cmd.ExecuteNonQuery();
                    }
                }
            }

            MessageBox.Show(
                "Блоки сохранены.");
        }

        private void OpenTableButton_Click(object sender, EventArgs e)
        {
            if (comboBoxDataTable.SelectedItem == null) return;
            LoadTable(comboBoxDataTable.SelectedItem.ToString());
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

        private void buttonSecontLevel_Click(object sender, EventArgs e)
        {
            tabControl.SelectedTab = tabSecond;
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

        private void buttonCleatAll2_Click(object sender, EventArgs e)
        {
            checkBoxMuT.Checked = false;
            checkBoxAM.Checked = false;
            checkBoxMuTm.Checked = false;

            checkBoxMuTmForecast.Checked = false;
            checkBoxMuTForecast.Checked = false;
            checkBoxMuTpForecast.Checked = false;
        }

        private void buttonClearAllSecond2_Click(object sender, EventArgs e)
        {
            checkBoxFMlimit.Checked = false;
            checkBoxFlimit.Checked = false;
            checkBoxFPlimit.Checked = false;
            checkBoxFlimitForecast.Checked = false;
            checkBoxFPlimitForecast.Checked = false;
            checkBoxFMlimitForecast.Checked = false;
        }

        private void buttonClearAllSecond_Click(object sender, EventArgs e)
        {
            checkBoxAMlimit.Checked = false;
            checkBoxAlimit.Checked = false;
            checkBoxAPlimit.Checked = false;
            checkBoxAlimitForecast.Checked = false;
            checkBoxAPlimitForecast.Checked = false;
            checkBoxAMlimitForecast.Checked = false;
        }

        private void listBoxAll_DoubleClick(object sender, EventArgs e)
        {
            MoveSelectedItem(
                listBoxAll,
                listBoxBlock);
        }

        private void listBoxBlock_DoubleClick(object sender, EventArgs e)
        {
            MoveSelectedItem(
                listBoxBlock,
                listBoxAll);
        }

        private void comboBoxSelectBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveCurrentBlock();

            currentBlock =
                comboBoxSelectBox.SelectedItem.ToString();

            ShowBlock(currentBlock);
        }

        private void comboBoxSelectBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxSelectBox2.SelectedItem == null)
                return;

            string block = comboBoxSelectBox2.SelectedItem.ToString();

            currentBlock = block;

            RecalculatePhaseLevel2(block);
        }

        private void checkBoxAMu_CheckedChanged(object sender, EventArgs e) => UpdatePhaseChart();
        private void checkBoxAM_CheckedChanged(object sender, EventArgs e) => UpdatePhaseChart();
        private void checkBoxAMp_CheckedChanged(object sender, EventArgs e) => UpdatePhaseChart();

        private void checkBoxAMuForecast_CheckedChanged(object sender, EventArgs e) => UpdatePhaseChart();
        private void checkBoxAMForecast_CheckedChanged(object sender, EventArgs e) => UpdatePhaseChart();
        private void checkBoxAMpForecast_CheckedChanged(object sender, EventArgs e) => UpdatePhaseChart();

        private void checkBoxMuTm_CheckedChanged(object sender, EventArgs e) => UpdateFuncChart();
        private void checkBoxMuT_CheckedChanged(object sender, EventArgs e) => UpdateFuncChart();
        private void checkBoxMuTp_CheckedChanged(object sender, EventArgs e) => UpdateFuncChart();

        private void checkBoxMuTmForecast_CheckedChanged(object sender, EventArgs e) => UpdateFuncChart();
        private void checkBoxMuTForecast_CheckedChanged(object sender, EventArgs e) => UpdateFuncChart();
        private void checkBoxMuTpForecast_CheckedChanged(object sender, EventArgs e) => UpdateFuncChart();



        private void checkBoxAMlimit_CheckedChanged(object sender, EventArgs e) => RefreshPhaseChartA();
        private void checkBoxAlimit_CheckedChanged(object sender, EventArgs e) => RefreshPhaseChartA();
        private void checkBoxAPlimit_CheckedChanged(object sender, EventArgs e) => RefreshPhaseChartA();

        private void checkBoxAMlimitForecast_CheckedChanged(object sender, EventArgs e) => RefreshPhaseChartA();
        private void checkBoxAlimitForecast_CheckedChanged(object sender, EventArgs e) => RefreshPhaseChartA();
        private void checkBoxAPlimitForecast_CheckedChanged(object sender, EventArgs e) => RefreshPhaseChartA();

        private void checkBoxFMlimit_CheckedChanged(object sender, EventArgs e) => RefreshFuncChartF();
        private void checkBoxFlimit_CheckedChanged(object sender, EventArgs e) => RefreshFuncChartF();
        private void checkBoxFPlimit_CheckedChanged(object sender, EventArgs e) => RefreshFuncChartF();

        private void checkBoxFMlimitForecast_CheckedChanged(object sender, EventArgs e) => RefreshFuncChartF();
        private void checkBoxFlimitForecast_CheckedChanged(object sender, EventArgs e) => RefreshFuncChartF();
        private void checkBoxFPlimitForecast_CheckedChanged(object sender, EventArgs e) => RefreshFuncChartF();
    }
}