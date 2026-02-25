
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace new2026
{
    public partial class Form1 : Form
    {
        private ToolStripStatusLabel toolStripStatusLabel;
        private StatusStrip statusStrip;


        public Form1()
        {
            InitializeComponent();
            txtInput.AllowDrop = true;
            txtInput.DragEnter += TxtInput_DragEnter;
            txtInput.DragDrop += TxtInput_DragDrop;
            InitializeStatusStrip();

        }
        private void InitializeStatusStrip()
        {
            statusStrip = new StatusStrip();

            // Создаем метку для отображения текста
            toolStripStatusLabel = new ToolStripStatusLabel();
            toolStripStatusLabel.Text = "Готов";
            toolStripStatusLabel.TextAlign = ContentAlignment.MiddleLeft;

            // Добавляем метку в строку состояния
            statusStrip.Items.Add(toolStripStatusLabel);

            // Добавляем строку состояния на форму
            this.Controls.Add(statusStrip);
        }

        private void UpdateStatus(string message)
        {
            if (toolStripStatusLabel != null)
            {
                toolStripStatusLabel.Text = message;
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            UpdateStatus("Компиляция");
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

                CSharpCodeProvider compiler = new CSharpCodeProvider();

                CompilerParameters parameters = new CompilerParameters();
                parameters.GenerateExecutable = true;
                parameters.GenerateInMemory = true;
                parameters.ReferencedAssemblies.Add("System.dll");

                CompilerResults results = compiler.CompileAssemblyFromSource(parameters, code);

                if (results.Errors.Count > 0)
                {
                    foreach (CompilerError error in results.Errors)
                    {
                        txtOutput.Text = txtOutput.Text + "Ошибка: " + error.ErrorText + "\n";
                    }
                }
                else
                {
                    MethodInfo mainMethod = results.CompiledAssembly.EntryPoint;

                    if (mainMethod != null)
                    {
                        UpdateStatus("Готово");
                        StringWriter writer = new StringWriter();
                        TextWriter oldOutput = Console.Out;
                        Console.SetOut(writer);

                        try
                        {
                            mainMethod.Invoke(null, null);

                            string output = writer.ToString();

                            if (output == "")
                            {
                                txtOutput.Text = "Программа выполнена";
                            }
                            else
                            {
                                txtOutput.Text = output;
                            }
                        }
                        catch (Exception ex)
                        {
                            UpdateStatus("Ошибка");
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

        private void btnAdd_Click(object sender, EventArgs e)
        {
            txtInput.Text = "";
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Text Files (*.txt)|*.txt|All files (*.*)|*.*";
            UpdateStatus("Файл загружен");

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                txtInput.Text = System.IO.File.ReadAllText(openFile.FileName);

            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "Text Files (*.txt)|*.txt|All files (*.*)|*.*";
            UpdateStatus("Файл сохранен");

            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllText(saveFile.FileName, txtInput.Text);
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (txtInput.SelectedText != "")
            {
                Clipboard.SetText(txtInput.SelectedText);
            }
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                txtInput.Text = txtInput.Text + Clipboard.GetText();
            }
        }

        private void btnCut_Click(object sender, EventArgs e)
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

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (txtInput.CanUndo)
            {
                txtInput.Undo();
            }
        }

        private void btnRepeat_Click(object sender, EventArgs e)
        {
            if (txtInput.CanRedo)
            {
                txtInput.Redo();
            }
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

        private void statusStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }

}