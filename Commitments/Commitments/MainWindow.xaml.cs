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
        private static readonly Brush DefaultColor = System.Windows.SystemColors.WindowBrush;
        private static readonly Brush WarningColor = new SolidColorBrush(Color.FromRgb(244, 204, 204));
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
            var message = (CommitMessage)DataContext;
            message.PropertyChanged += Message_PropertyChanged;
        }

        private void Message_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var message = (CommitMessage)DataContext;
            if (message.FullHeader.Length > 0)
            {
                var textBoxBG = DefaultColor;
                var trimmedMessage = message.Header.Trim();
                if (message.Header != trimmedMessage)
                {
                    // Header should (I assume) not have unnecessary white-space characters.
                    textBoxBG = WarningColor;
                }
                if (message.FullHeader.Length > 50)
                {
                    // Header should not be longer than 50 characters.
                    textBoxBG = WarningColor;
                }
                if (message.Types.Length == 0)
                {
                    // When not using Conventional Commits, header's first character should be uppercase.
                    var firstChar = trimmedMessage[..1];
                    if (firstChar != firstChar.ToUpper())
                    {
                        textBoxBG = WarningColor;
                    }
                }
                else
                {
                    // Using Conventional Commits, header's first character should be lowercase.
                    var firstChar = trimmedMessage[..1];
                    if (firstChar != firstChar.ToLower())
                    {
                        textBoxBG = WarningColor;
                    }
                }
                if (Char.IsPunctuation(trimmedMessage[^1]))
                {
                    // Header should not end in interpunctuation.
                    // TODO: Explore alternatives to Char.IsPunctuation which is a bit too harsh.
                    textBoxBG = WarningColor;
                }
                HeaderTextBox.Background = textBoxBG;
            }
            else
            {
                HeaderTextBox.Background = DefaultColor;
            }
        }

        private static bool IsBreakingChangeToken(string s)
        {
            return s == "BREAKING CHANGE" || s == "BREAKING-CHANGE";
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
            // Trigger Watermark behavior by getting and losing focus.
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

        private void ReadComboBoxSetTextBox(ComboBox comboBox, TextBox textBox, string textToAdd, string separator)
        {
            comboBox.SelectedIndex = 0;
            FocusManager.SetFocusedElement(this, textBox);
            if (textBox.Text.Length > 0)
            {
                textBox.Text += separator;
            }
            textBox.Text += textToAdd;
            textBox.CaretIndex = textBox.Text.Length;
            comboBox.IsDropDownOpen = false;
            FocusManager.SetFocusedElement(this, textBox);
        }

        private void TypesComboBox_DropDownClosed(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.SelectedIndex != 0)
            {
                ReadComboBoxSetTextBox(comboBox, TypesTextBox, (string)comboBox.SelectedItem, ",");
            }
        }

        private void TypesComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ComboBox comboBox = (ComboBox)sender;
                if (comboBox.IsDropDownOpen == false && comboBox.SelectedIndex != 0)
                {
                    ReadComboBoxSetTextBox(comboBox, TypesTextBox, (string)comboBox.SelectedItem, ",");
                }
            }
        }

        private void TypesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.SelectedIndex > 0 && comboBox.IsDropDownOpen)
            {
                ReadComboBoxSetTextBox(comboBox, TypesTextBox, (string)comboBox.SelectedItem, ",");
            }
        }

        private void FootersComboBoxChange(ComboBox comboBox)
        {
            string item = (string)comboBox.SelectedItem;
            string textToAdd = $"{item}{FooterExamplesDescriptions[item].Item1}";
            ReadComboBoxSetTextBox(comboBox, FootersTextBox, textToAdd, Environment.NewLine);
            BreakingCheckBox.IsChecked |= IsBreakingChangeToken(item);
        }

        private void FootersComboBox_DropDownClosed(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.SelectedIndex != 0)
            {
                FootersComboBoxChange(comboBox);
            }
        }

        private void FootersComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ComboBox comboBox = (ComboBox)sender;
                if (comboBox.IsDropDownOpen == false && comboBox.SelectedIndex != 0)
                {
                    FootersComboBoxChange(comboBox);
                }
            }
        }

        private void FootersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.SelectedIndex > 0 && comboBox.IsDropDownOpen)
            {
                FootersComboBoxChange(comboBox);
            }
        }

        private void BreakingChangeButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < FooterTokens.Count; i++)
            {
                var token = FooterTokens[i];
                if (IsBreakingChangeToken(token))
                {
                    FootersComboBox.SelectedIndex = i;
                    FootersComboBoxChange(FootersComboBox);
                    break;
                }
            }
        }
    }
}
