namespace DictionaryDemo.Forms {
    partial class MainForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.wordsGroupBox = new System.Windows.Forms.GroupBox();
            this.wordsGridView = new System.Windows.Forms.DataGridView();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnInsert = new System.Windows.Forms.Button();
            this.wordsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.wordsGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // wordsGroupBox
            // 
            this.wordsGroupBox.Controls.Add(this.wordsGridView);
            this.wordsGroupBox.Location = new System.Drawing.Point(12, 12);
            this.wordsGroupBox.Name = "wordsGroupBox";
            this.wordsGroupBox.Size = new System.Drawing.Size(452, 222);
            this.wordsGroupBox.TabIndex = 0;
            this.wordsGroupBox.TabStop = false;
            this.wordsGroupBox.Text = "Список имеющихся терминов";
            // 
            // wordsGridView
            // 
            this.wordsGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.wordsGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wordsGridView.Location = new System.Drawing.Point(3, 16);
            this.wordsGridView.MultiSelect = false;
            this.wordsGridView.Name = "wordsGridView";
            this.wordsGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.wordsGridView.Size = new System.Drawing.Size(446, 203);
            this.wordsGridView.TabIndex = 0;
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(177, 243);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 1;
            this.btnDelete.Tag = "DELETE";
            this.btnDelete.Text = "Удалить";
            this.btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnUpdate
            // 
            this.btnUpdate.Location = new System.Drawing.Point(96, 243);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(75, 23);
            this.btnUpdate.TabIndex = 2;
            this.btnUpdate.Tag = "UPDATE";
            this.btnUpdate.Text = "Изменить";
            this.btnUpdate.UseVisualStyleBackColor = true;
            // 
            // btnInsert
            // 
            this.btnInsert.Location = new System.Drawing.Point(15, 243);
            this.btnInsert.Name = "btnInsert";
            this.btnInsert.Size = new System.Drawing.Size(75, 23);
            this.btnInsert.TabIndex = 3;
            this.btnInsert.Tag = "INSERT";
            this.btnInsert.Text = "Добавить";
            this.btnInsert.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(476, 278);
            this.Controls.Add(this.btnInsert);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.wordsGroupBox);
            this.Name = "MainForm";
            this.Text = "Словарь";
            this.wordsGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.wordsGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox wordsGroupBox;
        private System.Windows.Forms.DataGridView wordsGridView;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnInsert;
    }
}