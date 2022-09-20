using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Antlr4.Runtime;
using src;

namespace neobemgui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _file;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new Microsoft.Win32.OpenFileDialog();
            var result = fileDialog.ShowDialog();
            if (result == true)
            {
                _file = fileDialog.FileName;
                InputFile.Text = _file;
                SampleInput.Text = File.ReadAllText(_file);
            }
        }

        private void CompileButtonClick(object sender, RoutedEventArgs e)
        {

            FileInfo fileInfo = new FileInfo(_file);

            if (!fileInfo.Exists)
            {
                SampleOutput.Text = $"{_file} does not exist.";
            }
            else
            {
                var fileText = File.ReadAllText(_file);
                SampleOutput.Text = fileText;

                AntlrInputStream inputStream = new AntlrInputStream(fileText);

                IdfplusLexer lexer = new IdfplusLexer(inputStream);

                CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);

                IdfplusParser parser = new IdfplusParser(commonTokenStream);

                IdfPlusVisitor visitor = new IdfPlusVisitor(fileInfo.DirectoryName);

                IdfplusParser.IdfContext tree = parser.idf();

                string result;
                try
                {
                    result = visitor.Visit(tree);
                    SampleOutput.Text = result;
                }
                catch (Exception exception)
                {
                    SampleOutput.Text = exception.Message;
                }

            }

        }
    }
}
