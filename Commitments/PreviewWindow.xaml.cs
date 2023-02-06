using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Commitments
{
    /// <summary>
    /// Interaction logic for PreviewWindow.xaml
    /// </summary>
    public partial class PreviewWindow : Window
    {
        private readonly MainWindow _parent;
        public PreviewWindow(MainWindow parent)
        {
            _parent = parent;
            DataContext = _parent.DataContext;
            InitializeComponent();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            var message = (CommitMessage)this.DataContext;
            Clipboard.SetText(message.Message);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
            _parent.ReturnFromPreview(false);
        }

        private void ClearBackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
            _parent.ReturnFromPreview(true);
        }
    }
}
