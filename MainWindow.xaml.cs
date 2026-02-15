using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TodoDS.Models;
using TodoDS.Services;

namespace TodoDS;

public partial class MainWindow : Window
{
    public static readonly RoutedUICommand RenameCommand = new("Rename", "RenameCommand", typeof(MainWindow));
    public static readonly RoutedUICommand EditTimeCommand = new("EditTime", "EditTimeCommand", typeof(MainWindow));
    public static readonly RoutedUICommand DeleteCommand = new("Delete", "DeleteCommand", typeof(MainWindow));

    private readonly TodoStore _store;
    private TodoItem? _selected;
    private string? _pendingTimeItemId;
    private bool _isPromptingTime;

    public MainWindow(TodoStore store)
    {
        _store = store;
        InitializeComponent();

        ReloadTree();
    }

    private void ReloadTree()
    {
        TodoTree.ItemsSource = _store.GetSorted(null, includeCompleted: true);
    }

    private void TodoTree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        _selected = e.NewValue as TodoItem;
    }

    private void AddTodoButton_OnClick(object sender, RoutedEventArgs e)
    {
        AddItem(isList: false);
    }

    private void AddListButton_OnClick(object sender, RoutedEventArgs e)
    {
        AddItem(isList: true);
    }

    private void AddItem(bool isList)
    {
        var parent = GetSelectedList();
        var title = isList ? "新建列表" : "新待办";
        var item = _store.Add(title, null, parent, isList);
        _pendingTimeItemId = isList ? null : item.Id;
        ReloadTree();
        FocusTitleEditor(item);
    }

    private TodoItem? GetSelectedList()
    {
        if (_selected == null)
        {
            return null;
        }

        if (_selected.IsList)
        {
            return _selected;
        }

        return _store.FindParent(_selected.Id);
    }

    private void TitleEditor_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        CommitTitle(sender as TextBox);
        e.Handled = true;
    }

    private void TitleEditor_OnLostFocus(object sender, RoutedEventArgs e)
    {
        CommitTitle(sender as TextBox);
    }

    private void CommitTitle(TextBox? editor)
    {
        if (editor?.DataContext is not TodoItem item)
        {
            return;
        }

        var title = editor.Text.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            title = item.IsList ? "未命名列表" : "未命名待办";
            editor.Text = title;
        }

        item.Title = title;
        _store.Update(item);
        MaybePromptForTime(item);
    }

    private void MaybePromptForTime(TodoItem item)
    {
        if (_pendingTimeItemId != item.Id || item.IsList || _isPromptingTime)
        {
            return;
        }

        _pendingTimeItemId = null;
        PromptForTime(item);
    }

    private void PromptForTime(TodoItem item)
    {
        _isPromptingTime = true;
        try
        {
            var picker = new TimePickerWindow(item.DueTime)
            {
                Owner = this,
            };
            var result = picker.ShowDialog();
            if (result == true)
            {
                item.DueTime = picker.SelectedTime;
                _store.Update(item);
                ReloadTree();
            }
        }
        finally
        {
            _isPromptingTime = false;
        }
    }

    private void CompletedCheck_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox checkBox || checkBox.DataContext is not TodoItem item)
        {
            return;
        }

        item.Completed = checkBox.IsChecked == true;
        _store.Update(item);
        ReloadTree();
    }

    private void RenameCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Parameter is not TodoItem item)
        {
            return;
        }

        FocusTitleEditor(item);
    }

    private void EditTimeCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Parameter is not TodoItem item)
        {
            return;
        }

        PromptForTime(item);
    }

    private void DeleteCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Parameter is not TodoItem item)
        {
            return;
        }

        if (item.IsList && item.Children.Count > 0)
        {
            var result = MessageBox.Show("删除列表将同时删除子待办，是否继续？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }
        }

        _store.Delete(item.Id);
        ReloadTree();
    }

    private void TodoItem_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject source)
        {
            return;
        }

        var item = ItemsControl.ContainerFromElement(TodoTree, source) as TreeViewItem;
        if (item != null)
        {
            item.IsSelected = true;
        }
    }

    private void FocusTitleEditor(TodoItem item)
    {
        Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() =>
            {
                var container = FindContainer(TodoTree, item);
                if (container == null)
                {
                    return;
                }

                container.IsSelected = true;
                container.BringIntoView();
                var editor = FindVisualChild<TextBox>(container, "TitleEditor");
                if (editor != null)
                {
                    editor.Focus();
                    editor.SelectAll();
                }
            }));
    }

    private static TreeViewItem? FindContainer(ItemsControl parent, object item)
    {
        if (parent.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem container)
        {
            return container;
        }

        foreach (var child in parent.Items)
        {
            if (parent.ItemContainerGenerator.ContainerFromItem(child) is not TreeViewItem childContainer)
            {
                continue;
            }

            childContainer.IsExpanded = true;
            var result = FindContainer(childContainer, item);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static T? FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T element && (string.IsNullOrEmpty(name) || element.Name == name))
            {
                return element;
            }

            var result = FindVisualChild<T>(child, name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

}
