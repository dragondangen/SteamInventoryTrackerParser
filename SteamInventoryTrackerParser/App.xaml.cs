using System.Windows;

namespace SteamInventoryTrackerParser
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Убираем вызов base.OnStartup чтобы избежать двойного окна
            // base.OnStartup(e); // Закомментировано!

            // Создаем и показываем главное окно
            MainWindow = new MainWindow();
            MainWindow.Show();
        }
    }
}