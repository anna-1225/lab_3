using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace new2026
{
    public partial class Form1 : Form
    {
        private string _currentFilePath = "";
        private Color _highlightColor = Color.Yellow;
        private int _currentHighlightStart = -1;
        private int _currentHighlightLength = -1;
        private Label lblMatchCount;

        public Form1()
        {
            InitializeComponent();
            SetupDataGridView();
            txtInput.AllowDrop = true;
            txtInput.DragEnter += TxtInput_DragEnter;
            txtInput.DragDrop += TxtInput_DragDrop;
            SetupMatchCountLabel();
        }
        private void SetupMatchCountLabel() 
        {
            lblMatchCount = new Label();
            lblMatchCount.Text = "Найдено: 0";
            this.Controls.Add(lblMatchCount);
        }


        private void SetupDataGridView()
        {
            dgvResults.Columns.Clear();
            dgvResults.Columns.Add("MatchText", "Найденная подстрока");
            dgvResults.Columns.Add("Position", "Позиция (строка, символ)");
            dgvResults.Columns.Add("Length", "Длина");
            dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvResults.ReadOnly = true;
            dgvResults.AllowUserToAddRows = false;
            dgvResults.SelectionChanged += DgvResults_SelectionChanged;
        }

        private void SearchForLogins()
        {
            try
            {
                string text = txtInput.Text;

                if (string.IsNullOrWhiteSpace(text))
                {
                    MessageBox.Show("Введите текст для поиска логинов.",
                        "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                dgvResults.Rows.Clear();
                RemoveHighlight();

                string pattern = @"^[a-zA-Z][a-zA-Z0-9._-]*$";

                string[] lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                RegexOptions options = RegexOptions.None;
                Regex regex = new Regex(pattern, options);

                int matchCount = 0;

                for (int lineNum = 0; lineNum < lines.Length; lineNum++)
                {
                    string line = lines[lineNum];

                    if (regex.IsMatch(line))
                    {
                        matchCount++;
                        int rowIndex = dgvResults.Rows.Add(
                            line,
                            $"{lineNum + 1}, 1",
                            line.Length
                        );

                        int globalPosition = GetGlobalPosition(text, lineNum, 0);

                        dgvResults.Rows[rowIndex].Tag = new MatchInfo
                        {
                            Index = globalPosition,
                            Length = line.Length,
                            Value = line
                        };
                    }
                }

                lblMatchCount.Text = $"Найдено логинов: {matchCount}";

                if (matchCount == 0)
                {
                    lblMatchCount.ForeColor = Color.Red;
                    MessageBox.Show("Корректных логинов не найдено.", "Результат поиска",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    lblMatchCount.ForeColor = Color.Green;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblMatchCount.Text = "Ошибка поиска";
                lblMatchCount.ForeColor = Color.Red;
            }
        }

        private void SearchForNumbers()
        {
            try
            {
                string text = txtInput.Text;

                if (string.IsNullOrWhiteSpace(text))
                {
                    MessageBox.Show("Введите текст для поиска чисел.",
                        "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                dgvResults.Rows.Clear();
                RemoveHighlight();

                string pattern = @"[-+]?\d+(?:[.,]\d+)?(?:[eE][+-]?\d+)?";

                RegexOptions options = RegexOptions.None;
                Regex regex = new Regex(pattern, options);

                MatchCollection matches = regex.Matches(text);
                int matchCount = 0;

                foreach (Match match in matches)
                {
                    if (!string.IsNullOrWhiteSpace(match.Value))
                    {
                        matchCount++;
                        string position = GetLineAndColumnPosition(text, match.Index);

                        int rowIndex = dgvResults.Rows.Add(
                            match.Value,
                            position,
                            match.Length
                        );

                        dgvResults.Rows[rowIndex].Tag = new MatchInfo
                        {
                            Index = match.Index,
                            Length = match.Length,
                            Value = match.Value
                        };
                    }
                }

                lblMatchCount.Text = $"Найдено чисел: {matchCount}";

                if (matchCount == 0)
                {
                    lblMatchCount.ForeColor = Color.Red;
                    MessageBox.Show("Чисел не найдено.", "Результат поиска",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    lblMatchCount.ForeColor = Color.Green;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске чисел: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblMatchCount.Text = "Ошибка поиска";
                lblMatchCount.ForeColor = Color.Red;
            }
        }

        private void SearchForMoney()
        {
            try
            {
                string text = txtInput.Text;

                if (string.IsNullOrWhiteSpace(text))
                {
                    MessageBox.Show("Введите текст для поиска денежных сумм.",
                        "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                dgvResults.Rows.Clear();
                RemoveHighlight();


                string pattern = @"(?:USD|EUR|RUB|GBP|JPY|CNY|UAH|KZT|BYN|PLN|CHF|CAD|AUD|SGD|HKD|TRY|INR|BRL|ZAR|NOK|SEK|DKK|CZK|HUF|RON|MXN|ILS|SAR|AED|KRW|THB|MYR|IDR|PHP|VND|EGP|NGN|PKR|BDT|LKR|MMK|UZS|TMT|GEL|AZN|AMD|MDL|KGS|TJS)[-]?\s*(?:[+-]?\d+(?:[.,]\d+)?)(?:\s*(?:тыс|млн|млрд|trillion|billion|million|thousand))?|(?:[€$£¥₽₴₸฿₩₪₫₦₲₵₡₨₮₯₰₱₲₳₴₵₶₷₸₹₺₻₼₽₾₿])\s*(?:[+-]?\d+(?:[.,]\d+)?)(?:\s*(?:тыс|млн|млрд|trillion|billion|million|thousand))?|(?:[+-]?\d+(?:[.,]\d+)?)\s*(?:USD|EUR|RUB|GBP|JPY|CNY|UAH|KZT|BYN|PLN|CHF|CAD|AUD|SGD|HKD|TRY|INR|BRL|ZAR|NOK|SEK|DKK|CZK|HUF|RON|MXN|ILS|SAR|AED|KRW|THB|MYR|IDR|PHP|VND|EGP|NGN|PKR|BDT|LKR|MMK|UZS|TMT|GEL|AZN|AMD|MDL|KGS|TJS)(?:\s*(?:тыс|млн|млрд|trillion|billion|million|thousand))?|(?:[+-]?\d+(?:[.,]\d+)?)\s*(?:[€$£¥₽₴₸฿₩₪₫₦₲₵₡₨₮₯₰₱₲₳₴₵₶₷₸₹₺₻₼₽₾₿])(?:\s*(?:тыс|млн|млрд|trillion|billion|million|thousand))?";

                RegexOptions options = RegexOptions.None;
                Regex regex = new Regex(pattern, options);

                MatchCollection matches = regex.Matches(text);
                int matchCount = 0;

                foreach (Match match in matches)
                {
                    if (!string.IsNullOrWhiteSpace(match.Value))
                    {
                        matchCount++;
                        string position = GetLineAndColumnPosition(text, match.Index);

                        int rowIndex = dgvResults.Rows.Add(
                            match.Value,
                            position,
                            match.Length
                        );

                        dgvResults.Rows[rowIndex].Tag = new MatchInfo
                        {
                            Index = match.Index,
                            Length = match.Length,
                            Value = match.Value
                        };
                    }
                }

                lblMatchCount.Text = $"Найдено денежных сумм: {matchCount}";

                if (matchCount == 0)
                {
                    lblMatchCount.ForeColor = Color.Red;
                    MessageBox.Show("Денежных сумм не найдено.", "Результат поиска",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    lblMatchCount.ForeColor = Color.Green;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске денежных сумм: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblMatchCount.Text = "Ошибка поиска";
                lblMatchCount.ForeColor = Color.Red;
            }
        }

        private int GetGlobalPosition(string text, int lineNumber, int columnNumber)
        {
            string[] lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            int position = 0;

            for (int i = 0; i < lineNumber && i < lines.Length; i++)
            {
                position += lines[i].Length + Environment.NewLine.Length;
            }

            position += columnNumber;
            return position;
        }

        private string GetLineAndColumnPosition(string text, int index)
        {
            int line = 1;
            int column = 1;

            for (int i = 0; i < index && i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    line++;
                    column = 1;
                }
                else if (text[i] != '\r')
                {
                    column++;
                }
            }

            return $"{line}, {column}";
        }

        private void HighlightMatch(int startIndex, int length)
        {
            try
            {
                if (startIndex < 0 || length <= 0 || startIndex + length > txtInput.Text.Length)
                    return;

                RemoveHighlight();

                txtInput.Focus();
                txtInput.Select(startIndex, length);
                txtInput.SelectionBackColor = _highlightColor;

                _currentHighlightStart = startIndex;
                _currentHighlightLength = length;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подсветки: {ex.Message}");
            }
        }

        private void RemoveHighlight()
        {
            if (_currentHighlightStart >= 0 && _currentHighlightLength > 0)
            {
                txtInput.Select(_currentHighlightStart, _currentHighlightLength);
                txtInput.SelectionBackColor = Color.White;
                txtInput.SelectionLength = 0;
                _currentHighlightStart = -1;
                _currentHighlightLength = -1;
            }
        }

        private void DgvResults_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvResults.SelectedRows.Count > 0)
            {
                MatchInfo matchInfo = dgvResults.SelectedRows[0].Tag as MatchInfo;
                if (matchInfo != null)
                {
                    HighlightMatch(matchInfo.Index, matchInfo.Length);
                }
            }
        }

        private class MatchInfo
        {
            public int Index { get; set; }
            public int Length { get; set; }
            public string Value { get; set; }
        }

        private void StartButton()
        {
            txtOutput.Text = "";

            try
            {
                string code = txtInput.Text;

                if (code.Contains("Main") == false)
                {
                    code = "using System;\n";
                    code = code + "class Program\n";
                    code = code + "{\n";
                    code = code + "    static void Main()\n";
                    code = code + "    {\n";
                    code = code + "        " + txtInput.Text + "\n";
                    code = code + "    }\n";
                    code = code + "}\n";
                }

                Microsoft.CSharp.CSharpCodeProvider compiler = new Microsoft.CSharp.CSharpCodeProvider();

                System.CodeDom.Compiler.CompilerParameters parameters = new System.CodeDom.Compiler.CompilerParameters();
                parameters.GenerateExecutable = true;
                parameters.GenerateInMemory = true;
                parameters.ReferencedAssemblies.Add("System.dll");

                System.CodeDom.Compiler.CompilerResults results = compiler.CompileAssemblyFromSource(parameters, code);

                if (results.Errors.Count > 0)
                {
                    foreach (System.CodeDom.Compiler.CompilerError error in results.Errors)
                    {
                        txtOutput.Text = txtOutput.Text + "Ошибка: " + error.ErrorText + "\n";
                    }
                }
                else
                {
                    MethodInfo mainMethod = results.CompiledAssembly.EntryPoint;

                    if (mainMethod != null)
                    {
                        StringWriter writer = new StringWriter();
                        TextWriter oldOutput = Console.Out;
                        Console.SetOut(writer);

                        try
                        {
                            mainMethod.Invoke(null, null);
                            string output = writer.ToString();
                            txtOutput.Text = output;
                        }
                        catch (Exception ex)
                        {
                            txtOutput.Text = "Ошибка: " + ex.Message;
                        }
                        finally
                        {
                            Console.SetOut(oldOutput);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                txtOutput.Text = "Ошибка: " + ex.Message;
            }
        }

        private void OpenButton()
        {
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
            txtInput.Text = "";
            dgvResults.Rows.Clear();
            lblMatchCount.Text = "Найдено: 0";
            RemoveHighlight();
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
            txtOutput.Font = new Font(txtOutput.Font.FontFamily, newSize, txtOutput.Font.Style);
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
            {
                txtInput.Text = System.IO.File.ReadAllText(files[0]);
            }
        }

        private void btnSearch_Click_3(object sender, EventArgs e)
        {
            SearchForLogins();
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

            if (lblMatchCount != null)
                lblMatchCount.Text = $"Found: {int.Parse(lblMatchCount.Text.Split(':')[1].Trim())}";
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
            dgvResults.Rows.Clear();
            lblMatchCount.Text = "Найдено: 0";
            RemoveHighlight();
        }

        private void Start_Click(object sender, EventArgs e)
        {
            StartButton();
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

        private void btnSearchNumbers_Click_1(object sender, EventArgs e)
        {
            SearchForNumbers();
        }

        private void btnSearchMoney_Click_1(object sender, EventArgs e)
        {
            SearchForMoney();
        }
    }
}