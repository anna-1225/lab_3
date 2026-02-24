using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace new2026
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, EventArgs e)
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
                        StringWriter writer = new StringWriter();
                        TextWriter oldOutput = Console.Out;
                        Console.SetOut(writer);

                        try
                        {
                            mainMethod.Invoke(null, null);

                            string output = writer.ToString();

                            if (output == "")
                            {
                                txtOutput.Text = "Программа выполнена!";
                            }
                            else
                            {
                                txtOutput.Text = output;
                            }
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

        private void btnAdd_Click(object sender, EventArgs e)
        {
            txtInput.Text = "";
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Text Files (*.txt)|*.txt|All files (*.*)|*.*";

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                txtInput.Text = File.ReadAllText(openFile.FileName);

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

            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(saveFile.FileName, txtInput.Text);
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
    }
}