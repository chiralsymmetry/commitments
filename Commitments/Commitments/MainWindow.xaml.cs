using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
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

namespace Commitments
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string Nonselection = " ";
        public List<string> TypeNames { get; set; } = new()
        {
            Nonselection
        };
        public List<string> FooterTokens { get; set; } = new()
        {
            Nonselection
        };

        public Dictionary<string, string> TypeDescriptions { get; set; } = new();
        public Dictionary<string, Tuple<string, string>> FooterExamplesDescriptions { get; set; } = new();

        public MainWindow()
        {
            PopulateTypes();
            PopulateFooters();
            InitializeComponent();
        }

        private void PopulateTypes()
        {
            var config = ((App)Application.Current).Config;
            if (config != null)
            {
                var pairs = config.GetSection("Types").Get<string[][]>();
                if (pairs != null)
                {
                    foreach (var pair in pairs)
                    {
                        if (pair.Length < 1)
                        {
                            continue;
                        }
                        var name = pair[0];
                        var description = string.Empty;
                        if (pair.Length > 1)
                        {
                            description = pair[1];
                        }
                        TypeNames.Add(name);
                        TypeDescriptions[name] = description;
                    }
                }
            }
        }

        private void PopulateFooters()
        {
            var config = ((App)Application.Current).Config;
            if (config != null)
            {
                var tuples = config.GetSection("Footers").Get<string[][]>();
                if (tuples != null)
                {
                    foreach (var tuple in tuples)
                    {
                        if (tuples.Length < 2)
                        {
                            continue;
                        }
                        var token = tuple[0];
                        var example = tuple[1];
                        var description = string.Empty;
                        if (tuples.Length > 2)
                        {
                            description = tuple[2];
                        }
                        FooterTokens.Add(token);
                        FooterExamplesDescriptions.Add(token, new(example, description));
                    }
                }
            }
        }

        private void ClearTextBox(TextBox textBox)
        {
            textBox.Clear();
            var focusedElement = FocusManager.GetFocusedElement(this);
            FocusManager.SetFocusedElement(this, textBox);
            FocusManager.SetFocusedElement(this, focusedElement);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearTextBox(TypesTextBox);
            ClearTextBox(HeaderTextBox);
            ClearTextBox(BodyTextBox);
            ClearTextBox(FootersTextBox);
            BreakingCheckBox.IsChecked = false;
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            var message = (CommitMessage)this.DataContext;
            Clipboard.SetText(message.Message);
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            var previewWindow = new PreviewWindow((CommitMessage)this.DataContext);
            previewWindow.ShowDialog();
        }
    }
}
