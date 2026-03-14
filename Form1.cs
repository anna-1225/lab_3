using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace new2026
{
    public partial class Form1 : Form
    {
        private string _currentFilePath = "";
        private List<Token> _lastTokens;

        public Form1()
        {
            InitializeComponent();
            txtInput.AllowDrop = true;

            txtInput.DragEnter += TxtInput_DragEnter;
            txtInput.DragDrop += TxtInput_DragDrop;

            SetupDataGridView();
            dataGridView1.CellDoubleClick += DataGridView1_CellDoubleClick;
        }

        private void SetupDataGridView()
        {
            dataGridView1.Columns.Clear();
            dataGridView1.Columns.Add("Code", "Код");
            dataGridView1.Columns.Add("Type", "Тип");
            dataGridView1.Columns.Add("Value", "Лексема");
            dataGridView1.Columns.Add("Location", "Местоположение");

        }

        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _lastTokens == null || e.RowIndex >= _lastTokens.Count) return;

            var token = _lastTokens[e.RowIndex];

            txtInput.Focus();

            int pos = 0;
            string[] lines = txtInput.Text.Split('\n');
            for (int i = 0; i < token.Line - 1; i++)
                pos += lines[i].Length + 1;

            txtInput.SelectionStart = pos + token.Position;
            txtInput.SelectionLength = token.Value.Length;
            txtInput.ScrollToCaret();
        }


        private bool AskToSave()
        {
            if (string.IsNullOrWhiteSpace(txtInput.Text))
                return true;

            DialogResult result = MessageBox.Show(
                "Сохранить изменения?",
                "Подтверждение",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                SaveButton();
                return true;
            }
            else if (result == DialogResult.No)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void StartButton()
        {
            dataGridView1.Rows.Clear();

            try
            {
                string code = txtInput.Text;
                Scanner scanner = new Scanner();
                var tokens = scanner.Analyze(code);

                _lastTokens = tokens;

                bool hasErrors = false;

                foreach (var token in tokens)
                {
                    string location;
                    int endPosition = token.Position + token.Value.Length - 1;

                    if (token.Position == endPosition)
                        location = $"строка {token.Line}, {token.Position}-{endPosition}";
                    else
                        location = $"строка {token.Line}, {token.Position}-{endPosition}";

                    int rowIndex = dataGridView1.Rows.Add(
                        token.Code,
                        token.Type,
                        token.Value,
                        location
                    );

                    if (token.IsError)
                    {
                        dataGridView1.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.Red;
                        hasErrors = true;
                    }
                }

                int summaryRowIndex = dataGridView1.Rows.Add(
                    $"Всего лексем: {tokens.Count}",
                    hasErrors ? "Есть ошибки!" : "Ошибок нет",
                    "",
                    ""
                );
            }
            catch (Exception ex)
            {
                dataGridView1.Rows.Add("ОШИБКА:", "", ex.Message, "");
            }
        }

        private void OpenButton()
        {
            if (!AskToSave())
                return;

            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Text Files (*.txt)|*.txt|All files (*.*)|*.*";

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                txtInput.Text = System.IO.File.ReadAllText(openFile.FileName);
                _currentFilePath = openFile.FileName;
            }
        }

        private void AddButton()
        {
            if (AskToSave())
            {
                txtInput.Text = "";
                _currentFilePath = "";
            }
        }

        private void SaveButton()
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                SaveAsButton();
            }
            else
            {
                try
                {
                    System.IO.File.WriteAllText(_currentFilePath, txtInput.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SaveAsButton()
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "Text Files (*.txt)|*.txt|All files (*.*)|*.*";

            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllText(saveFile.FileName, txtInput.Text);
                _currentFilePath = saveFile.FileName;
            }
        }

        private void CopyButton()
        {
            if (txtInput.SelectedText != "")
            {
                Clipboard.SetText(txtInput.SelectedText);
            }
        }

        private void InsertButton()
        {
            if (Clipboard.ContainsText())
            {
                txtInput.Text = txtInput.Text + Clipboard.GetText();
            }
        }

        private void CutButton()
        {
            if (txtInput.SelectedText != "")
            {
                Clipboard.SetText(txtInput.SelectedText);

                int selectionStart = txtInput.SelectionStart;
                int selectionLength = txtInput.SelectionLength;

                txtInput.Text = txtInput.Text.Remove(selectionStart, selectionLength);
                txtInput.SelectionStart = selectionStart;
            }
        }

        private void CancelButton()
        {
            if (txtInput.CanUndo)
            {
                txtInput.Undo();
            }
        }

        private void RepeatButton()
        {
            if (txtInput.CanRedo)
            {
                txtInput.Redo();
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            StartButton();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddButton();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenButton();
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveAsButton();
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            CopyButton();
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            InsertButton();
        }

        private void btnCut_Click(object sender, EventArgs e)
        {
            CutButton();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            CancelButton();
        }

        private void btnRepeat_Click(object sender, EventArgs e)
        {
            RepeatButton();
        }

        private void btnSize_ValueChanged(object sender, EventArgs e)
        {
            float newSize = (float)btnSize.Value;
            txtInput.Font = new Font(txtInput.Font.FontFamily, newSize, txtInput.Font.Style);
            dataGridView1.Font = new Font(dataGridView1.Font.FontFamily, newSize, dataGridView1.Font.Style);
        }

        private void TxtInput_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void TxtInput_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
                txtInput.Text = System.IO.File.ReadAllText(files[0]);
        }

        private void btnEnglish_Click(object sender, EventArgs e)
        {
            btnStart.Text = "Run";
            btnAdd.Text = "New";
            btnOpen.Text = "Open";
            btnSave.Text = "Save";
            btnCopy.Text = "Copy";
            btnInsert.Text = "Paste";
            btnCut.Text = "Cut";
            btnCancel.Text = "Cancel";
            btnRepeat.Text = "Repeat";
            Exit.Text = "Exit";
            File.Text = "File";
            menuAdd.Text = "Create";
            menuOpen.Text = "Open";
            menuSave.Text = "Save";
            menuSaveAs.Text = "Save as";
            Edit.Text = "Editing";
            menuCancel.Text = "Cancel";
            menuRepeat.Text = "Repeat";
            menuCut.Text = "Cut";
            menuCopy.Text = "Copy";
            menuInsert.Text = "Insert";
            menuDelete.Text = "Delete";
            menuDeleteAll.Text = "Delete all";
            Start.Text = "Start";
            Reference.Text = "Reference";
            menuReference.Text = "Call for help";
            menuAbout.Text = "About program";
            Language.Text = "Language";
            Font.Text = "Font size";
        }

        private void btnRussian_Click(object sender, EventArgs e)
        {
            btnStart.Text = "Запуск";
            btnAdd.Text = "Новый";
            btnOpen.Text = "Открыть";
            btnSave.Text = "Сохранить";
            btnCopy.Text = "Копировать";
            btnInsert.Text = "Вставить";
            btnCut.Text = "Вырезать";
            btnCancel.Text = "Отменить";
            btnRepeat.Text = "Повторить";
            Exit.Text = "Выход";
            File.Text = "Файл";
            menuAdd.Text = "Создать";
            menuOpen.Text = "Открыть";
            menuSave.Text = "Сохранить";
            menuSaveAs.Text = "Сохранить как";
            Edit.Text = "Правка";
            menuCancel.Text = "Отмена";
            menuRepeat.Text = "Возврат";
            menuCut.Text = "Вырезать";
            menuCopy.Text = "Копировать";
            menuInsert.Text = "Вставить";
            menuDelete.Text = "Удалить";
            menuDeleteAll.Text = "Удалить все";
            Start.Text = "Пуск";
            Reference.Text = "Справка";
            menuReference.Text = "Вызов справки";
            menuAbout.Text = "О программе";
            Language.Text = "Язык";
            Font.Text = "Размер шрифта";
        }

        private void menuAdd_Click(object sender, EventArgs e)
        {
            AddButton();
        }

        private void menuOpen_Click(object sender, EventArgs e)
        {
            OpenButton();
        }

        private void menuSaveAs_Click(object sender, EventArgs e)
        {
            SaveAsButton();
        }

        private void menuCancel_Click(object sender, EventArgs e)
        {
            CancelButton();
        }

        private void menuRepeat_Click(object sender, EventArgs e)
        {
            RepeatButton();
        }

        private void menuCut_Click(object sender, EventArgs e)
        {
            CutButton();
        }

        private void menuCopy_Click(object sender, EventArgs e)
        {
            CopyButton();
        }

        private void menuInsert_Click(object sender, EventArgs e)
        {
            InsertButton();
        }

        private void menuDelete_Click(object sender, EventArgs e)
        {
            if (txtInput.SelectedText != "")
            {
                int start = txtInput.SelectionStart;
                int length = txtInput.SelectionLength;
                txtInput.Text = txtInput.Text.Remove(start, length);
                txtInput.SelectionStart = start;
            }
        }

        private void menuDeleteAll_Click(object sender, EventArgs e)
        {
            txtInput.Text = "";
        }

        private void Start_Click(object sender, EventArgs e)
        {
            StartButton();
        }

        private void menuReference_Click_1(object sender, EventArgs e)
        {
            string helpText =
                "Описание функций приложения\n" +
                "Основные функции компилятора:\n" +
                "- Запуск кода - выполняет лексический анализ\n" +
                "- Результат отображается в таблице\n\n" +

                "Работа с файлами:\n" +
                "- Создать - очищает поле ввода\n" +
                "- Открыть - загружает код из текстового файла\n" +
                "- Сохранить - сохраняет код в файл\n\n" +

                "Редактирование текста:\n" +
                "- Отменить/Повторить - отмена/повтор действий\n" +
                "- Вырезать/Копировать/Вставить - работа с буфером\n" +
                "- Удалить/Удалить все - удаление текста\n\n" +

                "Таблица результатов:\n" +
                "- Двойной клик по строке - переход к месту в коде\n" +
                "- Ошибки выделены красным цветом\n" +
                "- В последней строке итоговая информация\n\n" +

                "Дополнительно:\n" +
                "- Изменение размера шрифта\n" +
                "- Смена языка интерфейса";

            MessageBox.Show(helpText, "Справка по функциям",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show(
        "Вы действительно хотите выйти из приложения?",
        "Подтверждение выхода",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void menuSave_Click(object sender, EventArgs e)
        {
            SaveButton();
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            string helpText =
                "Описание функций приложения\n" +
                "Основные функции компилятора:\n" +
                "- Запуск кода - выполняет лексический анализ\n" +
                "- Результат отображается в таблице\n\n" +

                "Работа с файлами:\n" +
                "- Создать - очищает поле ввода\n" +
                "- Открыть - загружает код из текстового файла\n" +
                "- Сохранить - сохраняет код в файл\n\n" +

                "Редактирование текста:\n" +
                "- Отменить/Повторить - отмена/повтор действий\n" +
                "- Вырезать/Копировать/Вставить - работа с буфером\n" +
                "- Удалить/Удалить все - удаление текста\n\n" +

                "Таблица результатов:\n" +
                "- Двойной клик по строке - переход к месту в коде\n" +
                "- Ошибки выделены красным цветом\n" +
                "- В последней строке итоговая информация\n\n" +

                "Дополнительно:\n" +
                "- Изменение размера шрифта\n" +
                "- Смена языка интерфейса";

            MessageBox.Show(helpText, "Справка по функциям",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}