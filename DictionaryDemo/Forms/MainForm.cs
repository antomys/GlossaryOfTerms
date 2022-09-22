using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DictionaryDemo.Models;
using DictionaryDemo.Helpers;

namespace DictionaryDemo.Forms {
    public partial class MainForm : Form {
        /// <summary>
        /// Конструктор основной формы
        /// </summary>
        public MainForm() {
            InitializeComponent();
            this.Load += new EventHandler(MainForm_Load);
            btnInsert.Click += new EventHandler(operate_Click);
            btnUpdate.Click += new EventHandler(operate_Click);
            btnDelete.Click += new EventHandler(operate_Click);
        }

        /// <summary>
        /// Обработчик нажатия кнопок (общий)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void operate_Click(object sender, EventArgs e) {
            int selectedId = GetSelectedId();
            OperationType operation = (OperationType)Enum.Parse(typeof(OperationType), (sender as Button).Tag.ToString());
            switch (operation) {
                case OperationType.DELETE:
                    if (selectedId != -1) {
                        DeleteItem(selectedId);
                    }
                    break;
                case OperationType.INSERT:
                    InsertUpdate(-1, operation);
                    break;
                case OperationType.UPDATE:
                    if (selectedId != -1) {
                        InsertUpdate(selectedId, operation);
                    }
                    break;
            }
        }

        /// <summary>
        /// Обработчик загрузки приложения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainForm_Load(object sender, EventArgs e) {
            UpdateView();
        }

        /// <summary>
        /// Обновление отображения
        /// </summary>
        void UpdateView() {
            SQLiteDbHelper dbHelper = new SQLiteDbHelper();
            wordsGridView.DataSource = null;
            wordsGridView.DataSource = dbHelper.GetAllWords();
        }

        /// <summary>
        /// Получение Id выделенного элемента
        /// </summary>
        /// <returns></returns>
        int GetSelectedId() {
            if (wordsGridView.SelectedRows != null && wordsGridView.SelectedRows.Count != 0) {
                return int.Parse(wordsGridView.SelectedRows[0].Cells[0].Value.ToString());
            }
            return -1;
        }

        /// <summary>
        /// Удаление элемента из словаря
        /// </summary>
        /// <param name="id">Id элемента</param>
        void DeleteItem(int id) {
            DialogResult res = MessageBox.Show("Элемент будет удален. Продолжить?", "Подтверждение операции", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes) {
                SQLiteDbHelper dbHelper = new SQLiteDbHelper();
                WordItem item = new WordItem { Id = id };
                dbHelper.OperateItem(item, OperationType.DELETE);

                //Обновление отображения
                UpdateView();
            } 
        }

        /// <summary>
        /// Вставка/Изменение элемента
        /// </summary>
        /// <param name="id">Id элемента</param>
        void InsertUpdate(int id, OperationType operation) {
            using (AddUpdateForm addUpdate = new AddUpdateForm(id, operation)) {
                addUpdate.StartPosition = FormStartPosition.Manual;
                addUpdate.Location = this.Location +
                    SystemInformation.CaptionButtonSize +
                    SystemInformation.FrameBorderSize;
                DialogResult result = addUpdate.ShowDialog();
                if (result == DialogResult.OK) {
                    UpdateView();
                }
            }
        }
    }
}
