using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Commitments
{
    public class WatermarkBehavior : Behavior<TextBox>
    {
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register(
            "Watermark", typeof(string), typeof(WatermarkBehavior), new PropertyMetadata(string.Empty));

        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        private Brush? OriginalForeground;
        private FontStyle OriginalFontStyle;
        private bool IsWatermarked = false;

        protected override void OnAttached()
        {
            base.OnAttached();
            OriginalForeground = AssociatedObject.Foreground;
            OriginalFontStyle = AssociatedObject.FontStyle;
            AssociatedObject.GotFocus += OnGotFocus;
            AssociatedObject.LostFocus += OnLostFocus;
            SetWatermark();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.GotFocus -= OnGotFocus;
            AssociatedObject.LostFocus -= OnLostFocus;
            UnsetWatermark();
        }

        private void SetWatermark()
        {
            AssociatedObject.Foreground = Brushes.Gray;
            AssociatedObject.FontStyle = FontStyles.Italic;
            AssociatedObject.Text = Watermark;
            IsWatermarked = true;
        }

        private void UnsetWatermark()
        {
            AssociatedObject.Foreground = OriginalForeground;
            AssociatedObject.FontStyle = OriginalFontStyle;
            AssociatedObject.Text = string.Empty;
            IsWatermarked = false;
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (IsWatermarked)
            {
                UnsetWatermark();
            }
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(AssociatedObject.Text))
            {
                SetWatermark();
            }
        }
    }
}
