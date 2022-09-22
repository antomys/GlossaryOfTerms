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
    public partial class AddUpdateForm : Form {
        int _id = 0;
        OperationType _operation;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="id">Id термина</param>
        /// <param name="operation">Тип операции</param>
        public AddUpdateForm(int id, OperationType operation) {
            InitializeComponent();
            _id = id;
            _operation = operation;

            this.Load += delegate {
                LoadForm(_id);
            };

            btnOk.Click += new EventHandler(btnOk_Click);
        }

        /// <summary>
        /// Обработчик нажатия кнопки ОК
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnOk_Click(object sender, EventArgs e) {
            if (string.IsNullOrWhiteSpace(txtRU.Text) || string.IsNullOrWhiteSpace(txtEN.Text)) {
                MessageBox.Show("Заполните обязательные поля", "Ошибка");
                return;
            }
            WordItem item = new WordItem {
                Id=_id,
                RU=txtRU.Text,
                EN=txtEN.Text
            };
            SQLiteDbHelper dbHelper = new SQLiteDbHelper();
            dbHelper.OperateItem(item, _operation);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Обработчик загрузки формы вставки/редактирования
        /// </summary>
        /// <param name="id"></param>
        void LoadForm(int id) {
            if (id == -1) {
                return;
            }
            SQLiteDbHelper dbHelper = new SQLiteDbHelper();
            WordItem item = dbHelper.GetItemById(id);
            if (item != null) {
                txtId.Text = item.Id.ToString();
                txtRU.Text = item.RU;
                txtEN.Text = item.EN;
            }
        }
    }
}
