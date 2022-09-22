using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace DictionaryDemo.Models {
    /// <summary>
    /// Класс модели словарного элемента
    /// </summary>
    public class WordItem {
        /// <summary>
        /// Id
        /// </summary>
        [DisplayName("Идентификатор")]
        public int Id { get; set; }

        /// <summary>
        /// Русский термин
        /// </summary>
        [DisplayName("Русский термин")]
        public string RU { get; set; }

        /// <summary>
        /// Английский термин
        /// </summary>
        [DisplayName("Английский термин")]
        public string EN { get; set; }
    }
}
