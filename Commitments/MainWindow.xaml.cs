using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        private Dictionary<IInputElement, Dictionary<ValidationTag, string>> Hints { get; set; } = new();

        private IInputElement? CurrentFocus = null;
        private IInputElement? BackupFocus = null;

        public MainWindow()
        {
            PopulateTypes();
            PopulateFooters();
            InitializeComponent();
            // Header is obligatory, so it should start out with a warning.
            var message = (CommitMessage)DataContext;
            message.PropertyChanged += Message_PropertyChanged;
            ClearAll();
        }

        private void Message_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var message = (CommitMessage)DataContext;
            HeaderChecks(message);
            BodyChecks(message);
            FootersChecks(message);
            UpdateHints();
        }

        private void UpdateHints()
        {
            if (CurrentFocus != null)
            {
                HintStatusTop.Content = GetHints(CurrentFocus);
                HintStatusBottom.Content = GetHints(CurrentFocus);
            }
            else
            {
                HintStatusTop.Content = string.Empty;
                HintStatusBottom.Content = string.Empty;
            }
        }

        private void SetHint(IInputElement element, ValidationTag tag, string hint)
        {
            if (!Hints.ContainsKey(element))
            {
                Hints.Add(element, new());
            }
            Hints[element][tag] = hint;
        }

        private void UnsetHints(IInputElement element)
        {
            Hints.Remove(element);
        }

        private void UnsetHint(IInputElement element, ValidationTag tag)
        {
            if (Hints.ContainsKey(element) && Hints[element].ContainsKey(tag))
            {
                Hints[element].Remove(tag);
            }
        }

        private string GetHints(IInputElement element)
        {
            string output = string.Empty;
            if (Hints.ContainsKey(element))
            {
                output = String.Join(" ", Hints[element].Values);
            }
            return output;
        }

        private enum ValidationTag
        {
            WHITESPACE,
            LENGTH,
            CASE,
            PUNCTUATION
        }

        private void HeaderChecks(CommitMessage message)
        {
            UnsetHints(HeaderTextBox);
            if (message.Header.Length == 0)
            {
                // Header is obligatory.
                HeaderTextBox.Background = WarningColor;
                SetHint(HeaderTextBox, ValidationTag.LENGTH, "Headers are obligatory.");
            }
            else if (message.FullHeader.Length > 0)
            {
                var textBoxBG = DefaultColor;
                var trimmedHeader = message.Header.Trim();
                if (message.Header != trimmedHeader)
                {
                    // Header should (I assume) not have unnecessary white-space characters.
                    textBoxBG = WarningColor;
                    SetHint(HeaderTextBox, ValidationTag.WHITESPACE, "Headers should not start/end with whitespace.");
                }
                else
                {
                    UnsetHint(HeaderTextBox, ValidationTag.WHITESPACE);
                }
                if (message.FullHeader.Length > 50)
                {
                    // Header should not be longer than 50 characters.
                    textBoxBG = WarningColor;
                    SetHint(HeaderTextBox, ValidationTag.LENGTH, "Headers should not be longer than 50 characters.");
                }
                else
                {
                    UnsetHint(HeaderTextBox, ValidationTag.LENGTH);
                }
                if (message.Types.Length == 0)
                {
                    // When not using Conventional Commits, header's first character should be uppercase.
                    var firstChar = trimmedHeader[..1];
                    if (firstChar != firstChar.ToUpper())
                    {
                        textBoxBG = WarningColor;
                        SetHint(HeaderTextBox, ValidationTag.CASE, "Header should be capitalized.");
                    }
                    else
                    {
                        UnsetHint(HeaderTextBox, ValidationTag.CASE);
                    }
                }
                else
                {
                    // Using Conventional Commits, header's first character should be lowercase.
                    var firstChar = trimmedHeader[..1];
                    if (firstChar != firstChar.ToLower())
                    {
                        textBoxBG = WarningColor;
                        SetHint(HeaderTextBox, ValidationTag.CASE, "Header shouldn't be capitalized.");
                    }
                    else
                    {
                        UnsetHint(HeaderTextBox, ValidationTag.CASE);
                    }
                }
                if (Char.IsPunctuation(trimmedHeader[^1]))
                {
                    // Header should not end in punctuation.
                    // TODO: Explore alternatives to Char.IsPunctuation which is a bit too harsh.
                    textBoxBG = WarningColor;
                    SetHint(HeaderTextBox, ValidationTag.PUNCTUATION, "Header should not end in a punctuation character.");
                }
                else
                {
                    UnsetHint(HeaderTextBox, ValidationTag.PUNCTUATION);
                }
                HeaderTextBox.Background = textBoxBG;
            }
            else
            {
                HeaderTextBox.Background = DefaultColor;
            }
        }
        private void BodyChecks(CommitMessage message)
        {
            UnsetHints(BodyTextBox);
            if (message.Body.Length > 0)
            {
                var textBoxBG = DefaultColor;
                if (message.BodyWidth > 72)
                {
                    // Exception: quoted code may be as wide as needed.
                    textBoxBG = WarningColor;
                    SetHint(BodyTextBox, ValidationTag.LENGTH, "Body should not be wider than 72 characters.");
                }
                else
                {
                    UnsetHint(BodyTextBox, ValidationTag.LENGTH);
                }
                {
                    var bodyLines = message.Body.ReplaceLineEndings().Split(Environment.NewLine);
                    var firstLine = bodyLines[0];
                    var lastLine = bodyLines[^1];
                    if (firstLine.Trim() == string.Empty || lastLine.Trim() == string.Empty)
                    {
                        textBoxBG = WarningColor;
                        SetHint(BodyTextBox, ValidationTag.WHITESPACE, "Body should not have leading or trailing empty lines.");
                    }
                    else
                    {
                        UnsetHint(BodyTextBox, ValidationTag.WHITESPACE);
                    }
                }
                BodyTextBox.Background = textBoxBG;
            }
            else
            {
                BodyTextBox.Background = DefaultColor;
            }
        }

        private void FootersChecks(CommitMessage message)
        {
            UnsetHints(FootersTextBox);
            if (message.Footer.Length > 0)
            {
                var textBoxBG = DefaultColor;
                if (message.FooterWidth > 72)
                {
                    // Footers should (I assume) be subject to the same limit as the body.
                    textBoxBG = WarningColor;
                    SetHint(FootersTextBox, ValidationTag.LENGTH, "Footer should not be wider than 72 characters.");
                }
                else
                {
                    UnsetHint(FootersTextBox, ValidationTag.LENGTH);
                }
                {
                    var footers = message.Footer.ReplaceLineEndings().Split(Environment.NewLine);
                    if (footers.Any(s => string.IsNullOrEmpty(s.Trim())))
                    {
                        textBoxBG = WarningColor;
                        SetHint(FootersTextBox, ValidationTag.WHITESPACE, "Footer should not contain empty or whitespace only lines.");
                    }
                    else
                    {
                        UnsetHint(FootersTextBox, ValidationTag.LENGTH);
                    }
                }
                FootersTextBox.Background = textBoxBG;
            }
            else
            {
                FootersTextBox.Background = DefaultColor;
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

        private void ClearAll()
        {
            ClearTextBox(TypesTextBox);
            ClearTextBox(HeaderTextBox);
            ClearTextBox(BodyTextBox);
            ClearTextBox(FootersTextBox);
            BreakingCheckBox.IsChecked = false;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearAll();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            var message = (CommitMessage)this.DataContext;
            Clipboard.SetText(message.Message);
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            var previewWindow = new PreviewWindow(this);
            previewWindow.ShowDialog();
        }

        public void ReturnFromPreview(bool clear)
        {
            if (clear)
            {
                ClearAll();
            }
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
            UpdateHints();
            StatusBarTop.Opacity = 0;
            StatusBarBottom.Opacity = 1;
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
                e.Handled = true;
            }
        }

        private void TypesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.SelectedIndex > 0 && comboBox.IsDropDownOpen)
            {
                ReadComboBoxSetTextBox(comboBox, TypesTextBox, (string)comboBox.SelectedItem, ",");
            }
            else if (!comboBox.IsDropDownOpen)
            {
                string item = (string)comboBox.SelectedItem;
                if (TypeDescriptions.ContainsKey(item))
                {
                    HintStatusTop.Content = TypeDescriptions[item];
                    HintStatusBottom.Content = TypeDescriptions[item];
                }
                else
                {
                    HintStatusTop.Content = string.Empty;
                    HintStatusBottom.Content = string.Empty;
                }
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
            UpdateHints();
            StatusBarTop.Opacity = 0;
            StatusBarBottom.Opacity = 1;
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
                e.Handled = true;
            }
        }

        private void FootersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.SelectedIndex > 0 && comboBox.IsDropDownOpen)
            {
                FootersComboBoxChange(comboBox);
            }
            else if (!comboBox.IsDropDownOpen)
            {
                string item = (string)comboBox.SelectedItem;
                if (FooterExamplesDescriptions.ContainsKey(item))
                {
                    HintStatusTop.Content = FooterExamplesDescriptions[item].Item2;
                    HintStatusBottom.Content = FooterExamplesDescriptions[item].Item2;
                }
                else
                {
                    HintStatusTop.Content = string.Empty;
                    HintStatusBottom.Content = string.Empty;
                }
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

        private void SetHintFocus(IInputElement? element)
        {
            CurrentFocus = element;
            UpdateHints();
        }

        private void UnsetHintFocus(IInputElement element)
        {
            if (CurrentFocus == element)
            {
                CurrentFocus = null;
                UpdateHints();
            }
        }

        private void TextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (CurrentFocus != textBox)
            {
                BackupFocus = CurrentFocus;
                SetHintFocus(textBox);
            }
        }

        private void TextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (CurrentFocus == textBox && !textBox.IsKeyboardFocused)
            {
                CurrentFocus = BackupFocus;
                BackupFocus = null;
                SetHintFocus(CurrentFocus);
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.Opacity = 1;
            SetHintFocus(textBox);
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text.Length == 0)
            {
                textBox.Opacity = 0;
            }
            UnsetHintFocus(textBox);
        }

        private void TypesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TypesTextBox.Text.Length > 0)
            {
                HeaderWatermarkTextBox.Text = HeaderWatermarkTextBox.Text.ToLower();
            }
            else
            {
                HeaderWatermarkTextBox.Text = HeaderWatermarkTextBox.Text.ToUpper()[0] + HeaderWatermarkTextBox.Text[1..];
            }
        }

        private void GoToTypes_Command(object sender, ExecutedRoutedEventArgs e)
        {
            TypesTextBox.Focus();
        }

        private void GoToHeader_Command(object sender, ExecutedRoutedEventArgs e)
        {
            HeaderTextBox.Focus();
        }

        private void GoToBody_Command(object sender, ExecutedRoutedEventArgs e)
        {
            BodyTextBox.Focus();
        }

        private void GoToFooters_Command(object sender, ExecutedRoutedEventArgs e)
        {
            FootersTextBox.Focus();
        }

        private void TypesComboBoxItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            ComboBoxItem comboBoxItem = (ComboBoxItem)sender;
            string item = (string)comboBoxItem.Content;
            if (TypeDescriptions.ContainsKey(item))
            {
                HintStatusTop.Content = TypeDescriptions[item];
                HintStatusBottom.Content = TypeDescriptions[item];
            }
            else
            {
                HintStatusTop.Content = string.Empty;
                HintStatusBottom.Content = string.Empty;
            }
            MoveStatusBar(TypesComboBox);
        }

        private void FootersComboBoxItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            ComboBoxItem comboBoxItem = (ComboBoxItem)sender;
            string item = (string)comboBoxItem.Content;
            if (FooterExamplesDescriptions.ContainsKey(item))
            {
                HintStatusTop.Content = FooterExamplesDescriptions[item].Item2;
                HintStatusBottom.Content = FooterExamplesDescriptions[item].Item2;
            }
            else
            {
                HintStatusTop.Content = string.Empty;
                HintStatusBottom.Content = string.Empty;
            }
            MoveStatusBar(FootersComboBox);
        }

        private void MoveStatusBar(ComboBox comboBox)
        {
            if (comboBox.Template.FindName("PART_Popup", comboBox) is Popup popup)
            {
                if (comboBox.IsDropDownOpen && popup.Child is UIElement dropDownMenu)
                {
                    var dropDownMenuPoint = dropDownMenu.PointToScreen(new Point(0, 0));
                    var dropDownMenuRect = new Rect(dropDownMenuPoint, dropDownMenu.RenderSize);
                    var statusBarPoint = StatusBarBottom.PointToScreen(new Point(0, 0));
                    var statusBarRect = new Rect(statusBarPoint, StatusBarBottom.RenderSize);
                    if (statusBarRect.Top < dropDownMenuRect.Bottom)
                    {
                        StatusBarTop.Opacity = 1;
                        StatusBarBottom.Opacity = 0;
                    }
                }
            }
        }
    }
}
