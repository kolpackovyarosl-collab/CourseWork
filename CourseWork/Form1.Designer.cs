namespace CourseWork
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.DataPage = new System.Windows.Forms.TabPage();
            this.tabFirst = new System.Windows.Forms.TabPage();
            this.tabSecond = new System.Windows.Forms.TabPage();
            this.tabTried = new System.Windows.Forms.TabPage();
            this.tabFour = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.MeasurementError = new System.Windows.Forms.TextBox();
            this.ExponentialSmoothingCoefficient = new System.Windows.Forms.TextBox();
            this.apply = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.SelectDataBase = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.deleteCycleWatch = new System.Windows.Forms.Button();
            this.OpenTableButton = new System.Windows.Forms.Button();
            this.addCycleWatch = new System.Windows.Forms.Button();
            this.comboBoxDataTable = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.tabControl.SuspendLayout();
            this.DataPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.DataPage);
            this.tabControl.Controls.Add(this.tabFirst);
            this.tabControl.Controls.Add(this.tabSecond);
            this.tabControl.Controls.Add(this.tabTried);
            this.tabControl.Controls.Add(this.tabFour);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(901, 494);
            this.tabControl.TabIndex = 0;
            // 
            // DataPage
            // 
            this.DataPage.Controls.Add(this.splitContainer1);
            this.DataPage.Location = new System.Drawing.Point(4, 22);
            this.DataPage.Name = "DataPage";
            this.DataPage.Padding = new System.Windows.Forms.Padding(3);
            this.DataPage.Size = new System.Drawing.Size(893, 468);
            this.DataPage.TabIndex = 0;
            this.DataPage.Text = "Данные";
            this.DataPage.UseVisualStyleBackColor = true;
            // 
            // tabFirst
            // 
            this.tabFirst.Location = new System.Drawing.Point(4, 22);
            this.tabFirst.Name = "tabFirst";
            this.tabFirst.Padding = new System.Windows.Forms.Padding(3);
            this.tabFirst.Size = new System.Drawing.Size(893, 468);
            this.tabFirst.TabIndex = 1;
            this.tabFirst.Text = "Первый уровень декомпозиции";
            this.tabFirst.UseVisualStyleBackColor = true;
            // 
            // tabSecond
            // 
            this.tabSecond.Cursor = System.Windows.Forms.Cursors.Hand;
            this.tabSecond.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.tabSecond.Location = new System.Drawing.Point(4, 22);
            this.tabSecond.Name = "tabSecond";
            this.tabSecond.Padding = new System.Windows.Forms.Padding(3);
            this.tabSecond.Size = new System.Drawing.Size(893, 468);
            this.tabSecond.TabIndex = 2;
            this.tabSecond.Text = "Второй уровень декомпозиции";
            this.tabSecond.UseVisualStyleBackColor = true;
            // 
            // tabTried
            // 
            this.tabTried.Location = new System.Drawing.Point(4, 22);
            this.tabTried.Name = "tabTried";
            this.tabTried.Padding = new System.Windows.Forms.Padding(3);
            this.tabTried.Size = new System.Drawing.Size(893, 468);
            this.tabTried.TabIndex = 3;
            this.tabTried.Text = "Третий уровень декомпозиции";
            this.tabTried.UseVisualStyleBackColor = true;
            // 
            // tabFour
            // 
            this.tabFour.Location = new System.Drawing.Point(4, 22);
            this.tabFour.Name = "tabFour";
            this.tabFour.Padding = new System.Windows.Forms.Padding(3);
            this.tabFour.Size = new System.Drawing.Size(893, 468);
            this.tabFour.TabIndex = 4;
            this.tabFour.Text = "Четвертый уровень декомпозиции";
            this.tabFour.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "База данных";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Location = new System.Drawing.Point(4, 5);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.MeasurementError);
            this.splitContainer1.Panel1.Controls.Add(this.ExponentialSmoothingCoefficient);
            this.splitContainer1.Panel1.Controls.Add(this.apply);
            this.splitContainer1.Panel1.Controls.Add(this.label6);
            this.splitContainer1.Panel1.Controls.Add(this.label5);
            this.splitContainer1.Panel1.Controls.Add(this.SelectDataBase);
            this.splitContainer1.Panel1.Controls.Add(this.label4);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.deleteCycleWatch);
            this.splitContainer1.Panel1.Controls.Add(this.OpenTableButton);
            this.splitContainer1.Panel1.Controls.Add(this.addCycleWatch);
            this.splitContainer1.Panel1.Controls.Add(this.comboBoxDataTable);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.label8);
            this.splitContainer1.Panel2.Controls.Add(this.label7);
            this.splitContainer1.Panel2.Controls.Add(this.pictureBox1);
            this.splitContainer1.Panel2.Controls.Add(this.dataGridView1);
            this.splitContainer1.Size = new System.Drawing.Size(884, 459);
            this.splitContainer1.SplitterDistance = 294;
            this.splitContainer1.TabIndex = 11;
            // 
            // MeasurementError
            // 
            this.MeasurementError.Location = new System.Drawing.Point(153, 286);
            this.MeasurementError.Name = "MeasurementError";
            this.MeasurementError.Size = new System.Drawing.Size(122, 20);
            this.MeasurementError.TabIndex = 13;
            // 
            // ExponentialSmoothingCoefficient
            // 
            this.ExponentialSmoothingCoefficient.Location = new System.Drawing.Point(153, 240);
            this.ExponentialSmoothingCoefficient.Name = "ExponentialSmoothingCoefficient";
            this.ExponentialSmoothingCoefficient.Size = new System.Drawing.Size(122, 20);
            this.ExponentialSmoothingCoefficient.TabIndex = 12;
            // 
            // apply
            // 
            this.apply.Location = new System.Drawing.Point(15, 324);
            this.apply.Name = "apply";
            this.apply.Size = new System.Drawing.Size(260, 23);
            this.apply.TabIndex = 11;
            this.apply.Text = "Применить";
            this.apply.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 293);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(134, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Погрешность измерения";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(12, 240);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(134, 44);
            this.label5.TabIndex = 9;
            this.label5.Text = "Коэффициент экспонециального сглаживания";
            // 
            // SelectDataBase
            // 
            this.SelectDataBase.Location = new System.Drawing.Point(15, 7);
            this.SelectDataBase.Name = "SelectDataBase";
            this.SelectDataBase.Size = new System.Drawing.Size(260, 23);
            this.SelectDataBase.TabIndex = 1;
            this.SelectDataBase.Text = "Выбрать БД";
            this.SelectDataBase.UseVisualStyleBackColor = true;
            this.SelectDataBase.Click += new System.EventHandler(this.SelectDataBase_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 216);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(183, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Параметры имитационной модели";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Выберите таблицу";
            // 
            // deleteCycleWatch
            // 
            this.deleteCycleWatch.Location = new System.Drawing.Point(15, 177);
            this.deleteCycleWatch.Name = "deleteCycleWatch";
            this.deleteCycleWatch.Size = new System.Drawing.Size(260, 23);
            this.deleteCycleWatch.TabIndex = 7;
            this.deleteCycleWatch.Text = "Удалить цикл наблюдения";
            this.deleteCycleWatch.UseVisualStyleBackColor = true;
            // 
            // OpenTableButton
            // 
            this.OpenTableButton.Location = new System.Drawing.Point(15, 76);
            this.OpenTableButton.Name = "OpenTableButton";
            this.OpenTableButton.Size = new System.Drawing.Size(260, 23);
            this.OpenTableButton.TabIndex = 3;
            this.OpenTableButton.Text = "Открыть таблицу";
            this.OpenTableButton.UseVisualStyleBackColor = true;
            this.OpenTableButton.Click += new System.EventHandler(this.OpenTableButton_Click);
            // 
            // addCycleWatch
            // 
            this.addCycleWatch.Location = new System.Drawing.Point(15, 148);
            this.addCycleWatch.Name = "addCycleWatch";
            this.addCycleWatch.Size = new System.Drawing.Size(260, 23);
            this.addCycleWatch.TabIndex = 6;
            this.addCycleWatch.Text = "Добавить цикл наблюдения";
            this.addCycleWatch.UseVisualStyleBackColor = true;
            // 
            // comboBoxDataTable
            // 
            this.comboBoxDataTable.FormattingEnabled = true;
            this.comboBoxDataTable.Location = new System.Drawing.Point(118, 49);
            this.comboBoxDataTable.Name = "comboBoxDataTable";
            this.comboBoxDataTable.Size = new System.Drawing.Size(157, 21);
            this.comboBoxDataTable.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 118);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(165, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Имитационное моделирование";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 218);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(155, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "Схема техногенного объекта";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 5);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(179, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "Координаты Z контрольных точек";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(3, 234);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(580, 222);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(3, 21);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(580, 191);
            this.dataGridView1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(916, 511);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Автоматизированная система мониторинга пространственно-временого сосояния техноге" +
    "нного объекта";
            this.tabControl.ResumeLayout(false);
            this.DataPage.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage DataPage;
        private System.Windows.Forms.TabPage tabFirst;
        private System.Windows.Forms.TabPage tabSecond;
        private System.Windows.Forms.TabPage tabTried;
        private System.Windows.Forms.TabPage tabFour;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox MeasurementError;
        private System.Windows.Forms.TextBox ExponentialSmoothingCoefficient;
        private System.Windows.Forms.Button apply;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button SelectDataBase;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button deleteCycleWatch;
        private System.Windows.Forms.Button OpenTableButton;
        private System.Windows.Forms.Button addCycleWatch;
        private System.Windows.Forms.ComboBox comboBoxDataTable;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.DataGridView dataGridView1;
    }
}

