using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Data.SQLite;

using DictionaryDemo.Models;

namespace DictionaryDemo.Helpers {
    /// <summary>
    /// Класс для работы с базой данных
    /// </summary>
    public class SQLiteDbHelper {
        /// <summary>
        /// Строка соединения с БД
        /// </summary>
        static string CONNECT_STR = string.Empty;

        /// <summary>
        /// Словарь с операциями
        /// </summary>
        static Dictionary<OperationType, string> operations = 
            new Dictionary<OperationType, string>();

        /// <summary>
        /// Статический конструктор
        /// </summary>
        static SQLiteDbHelper() {
            //Формирование строки соединения
            CONNECT_STR = string.Format("Data Source={0}",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database.db"));
            //Заполнение словаря операций
            FillOperations();
        }

        /// <summary>
        /// Получение всех терминов из БД
        /// </summary>
        /// <returns>Массив терминов</returns>
        public WordItem[] GetAllWords() {
            string command = "SELECT * FROM words";
            List<WordItem> words = new List<WordItem>();
            using (SQLiteConnection cnn = new SQLiteConnection(CONNECT_STR)) {
                cnn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(command, cnn)) {
                    using (SQLiteDataReader dr = cmd.ExecuteReader()) {
                        while (dr.Read()) {
                            WordItem word = new WordItem {
                                Id=int.Parse(dr["id"].ToString()),
                                RU=(string)dr["ru"],
                                EN=(string)dr["en"]
                            };
                            words.Add(word);
                        }
                    }
                }
            }
            return words.ToArray();
        }

        /// <summary>
        /// Операция с термином
        /// </summary>
        /// <param name="item">Объект термина</param>
        /// <param name="operation">Тип операции</param>
        public void OperateItem(WordItem item, OperationType operation) {
            string command = string.Format(operations[operation], item.Id, item.RU, item.EN);
            using (SQLiteConnection cnn = new SQLiteConnection(CONNECT_STR)) {
                cnn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(command, cnn)) {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Получение объекта термина по его id
        /// </summary>
        /// <param name="id">Id термина</param>
        /// <returns>Объект термина</returns>
        public WordItem GetItemById(int id) {
            string command = string.Format("SELECT * FROM words WHERE id='{0}'", id);
            using (SQLiteConnection cnn = new SQLiteConnection(CONNECT_STR)) {
                cnn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(command, cnn)) {
                    using (SQLiteDataReader dr = cmd.ExecuteReader()) {
                        if (!dr.HasRows) {
                            return null;
                        }
                        dr.Read();
                        return new WordItem {
                            Id=id,
                            RU=(string)dr["ru"],
                            EN=(string)dr["en"]
                        };
                    }
                }
            }
        }

        /// <summary>
        /// Метод заполнения словаря операций
        /// </summary>
        static void FillOperations() {
            operations.Add(OperationType.INSERT, "INSERT INTO words(ru,en) VALUES('{1}','{2}')");
            operations.Add(OperationType.UPDATE, "UPDATE words SET ru='{1}',en='{2}' WHERE id='{0}'");
            operations.Add(OperationType.DELETE, "DELETE FROM words WHERE id='{0}'");
        }
    }

    /// <summary>
    /// Перечисление для типов операций
    /// </summary>
    [Flags]
    public enum OperationType {
        INSERT = 0,
        UPDATE = 1,
        DELETE = 2
    }
}
