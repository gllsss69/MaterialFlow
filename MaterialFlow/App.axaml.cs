using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace MaterialFlow
{
    /// <summary>
    /// Головний клас застосунку MaterialFlow, що відповідає за ініціалізацію та життєвий цикл програми.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Ініціалізує компоненти застосунку, завантажуючи пов'язані ресурси XAML.
        /// </summary>
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Викликається після завершення ініціалізації середовища виконання.
        /// Створює та встановлює головне вікно для десктопного інтерфейсу.
        /// </summary>
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}