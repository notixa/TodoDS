using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TodoDS.Models;
using TodoDS.Services;

namespace TodoDS;

public partial class PetWindow : Window
{
    private static double? LastLeft;
    private static double? LastTop;

    private readonly TodoStore _store;
    private readonly TodoItem? _list;
    private readonly TodoItem? _parentList;
    private readonly Action<PetWindow, TodoItem?, TodoItem?, Point>? _navigate;
    private readonly Point? _initialPosition;

    public PetWindow(
        TodoStore store,
        TodoItem? list = null,
        TodoItem? parentList = null,
        Action<PetWindow, TodoItem?, TodoItem?, Point>? navigate = null,
        Point? initialPosition = null)
    {
        _store = store;
        _list = list;
        _parentList = parentList;
        _navigate = navigate;
        _initialPosition = initialPosition;
        InitializeComponent();
        Loaded += (_, _) => PositionWindow();
        UpdateHeader();
        Refresh();
    }

    public void Refresh()
    {
        var items = _store.GetSorted(_list, includeCompleted: true).ToList();
        ItemsHost.ItemsSource = items;
        EmptyText.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateHeader()
    {
        if (_list == null)
        {
            ListTitleText.Text = "TODO";
            ListSubtitleText.Text = "桌面置顶";
        }
        else
        {
            ListTitleText.Text = _list.Title;
            ListSubtitleText.Text = "子列表";
        }

        BackButton.Visibility = _list == null ? Visibility.Collapsed : Visibility.Visible;
    }

    private void PositionWindow()
    {
        var workArea = SystemParameters.WorkArea;
        if (_initialPosition.HasValue)
        {
            Left = _initialPosition.Value.X;
            Top = _initialPosition.Value.Y;
            ClampWithin(workArea);
        }
        else if (LastLeft.HasValue && LastTop.HasValue)
        {
            Left = LastLeft.Value;
            Top = LastTop.Value;
            ClampWithin(workArea);
        }
        else
        {
            Left = workArea.Right - Width;
            Top = workArea.Top + 80;
        }

        RememberPosition();
    }

    private void ClampWithin(Rect workArea)
    {
        var maxLeft = Math.Max(workArea.Left, workArea.Right - Width);
        var maxTop = Math.Max(workArea.Top, workArea.Bottom - Height);
        Left = Math.Max(workArea.Left, Math.Min(Left, maxLeft));
        Top = Math.Max(workArea.Top, Math.Min(Top, maxTop));
    }

    private void RememberPosition()
    {
        LastLeft = Left;
        LastTop = Top;
    }

    private void DockLeft()
    {
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Left;
        Top = ClampTop(Top, workArea);
        RememberPosition();
    }

    private void DockRight()
    {
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - Width;
        Top = ClampTop(Top, workArea);
        RememberPosition();
    }

    private double ClampTop(double top, Rect workArea)
    {
        return Math.Max(workArea.Top, Math.Min(top, workArea.Bottom - Height));
    }

    private void SnapToEdge()
    {
        var workArea = SystemParameters.WorkArea;
        const double snap = 40;
        if (Left - workArea.Left <= snap)
        {
            DockLeft();
            return;
        }

        if (workArea.Right - (Left + Width) <= snap)
        {
            DockRight();
        }
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void BackButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_navigate == null)
        {
            return;
        }

        if (_parentList == null)
        {
            _navigate(this, null, null, new Point(Left, Top));
            return;
        }

        var parentParent = _store.FindParent(_parentList.Id);
        _navigate(this, _parentList, parentParent, new Point(Left, Top));
    }

    private void ItemsHost_OnSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (ItemsHost.SelectedItem is not TodoItem item)
        {
            return;
        }

        ItemsHost.SelectedItem = null;
        if (!item.IsList)
        {
            return;
        }

        if (_navigate == null)
        {
            return;
        }

        _navigate(this, item, _list, new Point(Left, Top));
    }

    private void TodoCheck_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox checkBox || checkBox.DataContext is not TodoItem item)
        {
            return;
        }

        if (item.IsList)
        {
            return;
        }

        item.Completed = checkBox.IsChecked == true;
        _store.Update(item);
        Refresh();
    }

    private void Border_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState != MouseButtonState.Pressed)
        {
            return;
        }

        try
        {
            DragMove();
            SnapToEdge();
            RememberPosition();
        }
        catch (InvalidOperationException)
        {
            // Ignore failed drag.
        }
    }
}
