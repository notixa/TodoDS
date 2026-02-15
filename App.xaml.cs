using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using TodoDS.Models;
using TodoDS.Services;

namespace TodoDS;

public partial class App : Application
{
    private TaskbarIcon? _trayIcon;
    private MainWindow? _settingsWindow;
    private QuickViewWindow? _quickViewWindow;
    private PetWindow? _petWindow;

    public TodoStore Store { get; } = new(AppPaths.DataFile);

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Store.Load();
        Store.Changed += (_, _) => RefreshAuxWindows();

        _trayIcon = (TaskbarIcon)FindResource("TrayIcon");
        _trayIcon.Icon = TrayIconFactory.Create();
        Exit += (_, _) => _trayIcon?.Dispose();
    }

    private void RefreshAuxWindows()
    {
        _quickViewWindow?.Refresh();
        _petWindow?.Refresh();
    }

    private void ShowQuickView()
    {
        if (_quickViewWindow == null || !_quickViewWindow.IsVisible)
        {
            _quickViewWindow = new QuickViewWindow(Store, ShowSettings);
            _quickViewWindow.Show();
            _quickViewWindow.Activate();
            return;
        }

        _quickViewWindow.Activate();
    }

    private void ShowSettings()
    {
        if (_settingsWindow == null || !_settingsWindow.IsVisible)
        {
            _settingsWindow = new MainWindow(Store);
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;
            _settingsWindow.Show();
            _settingsWindow.Activate();
            return;
        }

        _settingsWindow.Activate();
    }

    private void TogglePet()
    {
        if (_petWindow == null || !_petWindow.IsVisible)
        {
            OpenPetWindow(null, null, null);
            return;
        }

        _petWindow.Close();
        _petWindow = null;
    }

    private void NavigatePet(PetWindow current, TodoItem? list, TodoItem? parentList, Point position)
    {
        OpenPetWindow(list, parentList, position);
        current.Close();
    }

    private void OpenPetWindow(TodoItem? list, TodoItem? parentList, Point? position)
    {
        var window = new PetWindow(Store, list, parentList, NavigatePet, position);

        window.Closed += (_, _) =>
        {
            if (_petWindow == window)
            {
                _petWindow = null;
            }
        };
        _petWindow = window;
        window.Show();
        window.Activate();
    }

    private void ToggleAutostart()
    {
        var enabled = AutostartService.IsEnabled();
        AutostartService.SetEnabled(!enabled);
    }

    private void ShutdownApp()
    {
        _trayIcon?.Dispose();
        Shutdown();
    }

    private void UpdateMenuChecks(ContextMenu menu)
    {
        foreach (var item in menu.Items.OfType<MenuItem>())
        {
            var tag = item.Tag?.ToString() ?? "";
            if (tag == "autostart")
            {
                item.IsChecked = AutostartService.IsEnabled();
            }
            else if (tag == "pet-toggle")
            {
                var visible = _petWindow?.IsVisible == true;
                item.IsChecked = visible;
                item.Header = visible ? "隐藏置顶窗口" : "显示置顶窗口";
            }
        }
    }

    private void TrayIcon_OnTrayLeftMouseUp(object sender, RoutedEventArgs e)
    {
        ShowQuickView();
    }

    private void TrayMenu_OnOpened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
        {
            UpdateMenuChecks(menu);
        }
    }

    private void TrayMenu_OnOpenClick(object sender, RoutedEventArgs e)
    {
        ShowQuickView();
    }

    private void TrayMenu_OnSettingsClick(object sender, RoutedEventArgs e)
    {
        ShowSettings();
    }

    private void TrayMenu_OnAutostartClick(object sender, RoutedEventArgs e)
    {
        ToggleAutostart();
        if (sender is MenuItem item)
        {
            item.IsChecked = AutostartService.IsEnabled();
        }
    }

    private void TrayMenu_OnPetClick(object sender, RoutedEventArgs e)
    {
        TogglePet();
        if (sender is MenuItem item)
        {
            var visible = _petWindow?.IsVisible == true;
            item.IsChecked = visible;
            item.Header = visible ? "隐藏置顶窗口" : "显示置顶窗口";
        }
    }

    private void TrayMenu_OnExitClick(object sender, RoutedEventArgs e)
    {
        ShutdownApp();
    }
}
