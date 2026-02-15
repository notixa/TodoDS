using System;
using System.Globalization;
using System.Windows;

namespace TodoDS;

public partial class TimePickerWindow : Window
{
    private static readonly string[] TimeFormats = { "H:mm", "HH:mm" };

    public DateTime? SelectedTime { get; private set; }

    public TimePickerWindow(DateTime? initialTime)
    {
        InitializeComponent();
        if (initialTime.HasValue)
        {
            DatePicker.SelectedDate = initialTime.Value.Date;
            TimeBox.Text = initialTime.Value.ToString("HH:mm", CultureInfo.InvariantCulture);
        }
        else
        {
            DatePicker.SelectedDate = DateTime.Today;
            TimeBox.Text = DateTime.Now.AddHours(1).ToString("HH:mm", CultureInfo.InvariantCulture);
        }
    }

    private void SkipButton_OnClick(object sender, RoutedEventArgs e)
    {
        SelectedTime = null;
        DialogResult = true;
    }

    private void ConfirmButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (DatePicker.SelectedDate is not DateTime date)
        {
            MessageBox.Show("请选择日期。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var input = TimeBox.Text.Trim();
        if (!DateTime.TryParseExact(
                input,
                TimeFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out var parsed))
        {
            MessageBox.Show("时间格式不正确，请输入 HH:mm，例如 09:30。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        SelectedTime = date.Date.Add(parsed.TimeOfDay);
        DialogResult = true;
    }
}
