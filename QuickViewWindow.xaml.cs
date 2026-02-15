using System;
using System.Linq;
using System.Windows;
using TodoDS.Services;

namespace TodoDS;

public partial class QuickViewWindow : Window
{
    private readonly TodoStore _store;
    private readonly Action _openSettings;

    public QuickViewWindow(TodoStore store, Action openSettings)
    {
        _store = store;
        _openSettings = openSettings;
        InitializeComponent();
        Loaded += (_, _) => PositionWindow();
        Refresh();
    }

    public void Refresh()
    {
        var items = _store.GetFlattenedTodos(includeCompleted: false).Take(8).ToList();
        ItemsHost.ItemsSource = items;
        EmptyText.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void PositionWindow()
    {
        var workArea = SystemParameters.WorkArea;
        Left = Math.Max(0, workArea.Right - Width - 20);
        Top = Math.Max(0, workArea.Bottom - Height - 20);
    }

    private void Window_OnDeactivated(object sender, EventArgs e)
    {
        Close();
    }

    private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        _openSettings();
        Close();
    }
}
