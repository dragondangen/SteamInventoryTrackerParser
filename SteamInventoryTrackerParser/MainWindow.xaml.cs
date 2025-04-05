using System.Windows;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Media;

namespace SteamInventoryTrackerParser
{
    public partial class MainWindow : Window
    {
        private readonly SteamInventoryParser _parser;
        private ObservableCollection<InventoryItem> _items;
        private bool _isDarkTheme = false;
        public MainWindow()
        {
            InitializeComponent();
            ApplyTheme();
            _parser = new SteamInventoryParser();
            _items = new ObservableCollection<InventoryItem>();
            lvInventory.ItemsSource = _items;
        }

        private async void BtnCheck_Click(object sender, RoutedEventArgs e)
        {
            string steamId = txtSteamId.Text.Trim();
            if (!long.TryParse(steamId, out _) || steamId.Length != 17)
            {
                MessageBox.Show("SteamID должен содержать 17 цифр");
                return;
            }

            if (!int.TryParse(txtAppId.Text, out int appId) || appId <= 0)
            {
                MessageBox.Show("AppID должен быть положительным числом");
                return;
            }

            try
            {
                btnCheck.IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;

                _items.Clear();
                var inventory = await _parser.ParseInventoryAsync(steamId, appId);

                if (inventory.Count == 0)
                {
                    MessageBox.Show("Инвентарь пуст или недоступен");
                    return;
                }

                // Добавляем предметы в список
                foreach (var item in inventory)
                    _items.Add(item);

                // Обновляем статистику
                UpdateStats(inventory);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnCheck.IsEnabled = true;
                Mouse.OverrideCursor = null;
            }
        }

        private void UpdateStats(List<InventoryItem> items)
        {
            int totalCount = items.Sum(item => item.Count);
            int uniqueCount = items.Count;

            txtTotalItems.Text = totalCount.ToString("N0");
            txtUniqueItems.Text = uniqueCount.ToString("N0");
        }
        private bool _isSortedByCountAscending = true;

        private void SortByCount_Click(object sender, RoutedEventArgs e)
        {
            var view = CollectionViewSource.GetDefaultView(lvInventory.ItemsSource);

            if (_isSortedByCountAscending)
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("Count", ListSortDirection.Descending));
                _isSortedByCountAscending = false;
            }
            else
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("Count", ListSortDirection.Ascending));
                _isSortedByCountAscending = true;
            }

            // Обновляем отображение
            view.Refresh();
        }



        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            _isDarkTheme = !_isDarkTheme;
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            // Обновляем кнопку
            themeToggleBtn.Content = _isDarkTheme ? "☀ Светлая тема" : "🌙 Тёмная тема";

            // Цвета для тем
            var background = _isDarkTheme ? Color.FromRgb(45, 45, 48) : Colors.White;
            var foreground = _isDarkTheme ? Colors.White : Colors.Black;
            var panelColor = _isDarkTheme ? Color.FromRgb(37, 37, 38) : Color.FromRgb(238, 238, 238);

            // Применяем к окну
            this.Background = new SolidColorBrush(background);
            this.Foreground = new SolidColorBrush(foreground);

            // Применяем к ListView
            lvInventory.Background = new SolidColorBrush(background);
            lvInventory.Foreground = new SolidColorBrush(foreground);

            // Применяем к StackPanel (исправленная часть)
            foreach (var child in mainGrid.Children)
            {
                if (child is StackPanel stackPanel) // Изменили имя переменной с panel на stackPanel
                {
                    stackPanel.Background = new SolidColorBrush(_isDarkTheme ? panelColor : Colors.Transparent);
                }
            }
        }
    }
}
