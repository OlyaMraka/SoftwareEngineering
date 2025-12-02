using System.Windows;
using System.Windows.Controls;

namespace KeyKeepersClient.Helpers
{
    /// <summary>
    /// Helper class to enable data binding for TextBox and PasswordBox text tracking.
    /// Provides HasText attached property that can be used in DataTriggers.
    /// </summary>
    public static class TextBoxHelper
    {
        // Attached property to track if TextBox/PasswordBox has text
        public static readonly DependencyProperty HasTextProperty =
            DependencyProperty.RegisterAttached(
                "HasText",
                typeof(bool),
                typeof(TextBoxHelper),
                new PropertyMetadata(false));

        // Attached property to enable monitoring
        public static readonly DependencyProperty MonitorTextProperty =
            DependencyProperty.RegisterAttached(
                "MonitorText",
                typeof(bool),
                typeof(TextBoxHelper),
                new PropertyMetadata(false, OnMonitorTextChanged));

        // Attached property to track validation state
        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.RegisterAttached(
                "IsValid",
                typeof(bool?),
                typeof(TextBoxHelper),
                new PropertyMetadata(null));

        public static bool GetHasText(DependencyObject obj)
        {
            return (bool)obj.GetValue(HasTextProperty);
        }

        public static void SetHasText(DependencyObject obj, bool value)
        {
            obj.SetValue(HasTextProperty, value);
        }

        public static bool GetMonitorText(DependencyObject obj)
        {
            return (bool)obj.GetValue(MonitorTextProperty);
        }

        public static void SetMonitorText(DependencyObject obj, bool value)
        {
            obj.SetValue(MonitorTextProperty, value);
        }

        public static bool? GetIsValid(DependencyObject obj)
        {
            return (bool?)obj.GetValue(IsValidProperty);
        }

        public static void SetIsValid(DependencyObject obj, bool? value)
        {
            obj.SetValue(IsValidProperty, value);
        }

        private static void OnMonitorTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                if ((bool)e.NewValue)
                {
                    textBox.TextChanged += TextBox_TextChanged;
                    UpdateHasText(textBox, textBox.Text);
                }
                else
                {
                    textBox.TextChanged -= TextBox_TextChanged;
                }
            }
            else if (d is PasswordBox passwordBox)
            {
                if ((bool)e.NewValue)
                {
                    passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
                    UpdateHasText(passwordBox, passwordBox.Password);
                }
                else
                {
                    passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
                }
            }
        }

        private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                UpdateHasText(textBox, textBox.Text);
            }
        }

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null)
            {
                UpdateHasText(passwordBox, passwordBox.Password);
            }
        }

        private static void UpdateHasText(DependencyObject obj, string text)
        {
            SetHasText(obj, !string.IsNullOrEmpty(text));
        }
    }
}
