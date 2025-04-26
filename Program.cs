using System;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;
using System.Runtime.InteropServices;

class ThemeChanger
{
    private static string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

    static void Main(string[] args)
    {
        EnsureConfigFileExists();
        ToggleTheme();
    }

    private static void EnsureConfigFileExists()
    {
        // Проверяем наличие config.txt в папке программы
        if (!File.Exists(configPath))
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(configPath))
                {
                    writer.WriteLine(@"D:\Path\To\LightWallpaper.jpg"); // Пример пути к светлым обоям
                    writer.WriteLine(@"D:\Path\To\DarkWallpaper.jpg");  // Пример пути к тёмным обоям
                }

                Console.WriteLine("Файл конфигурации 'config.txt' создан в папке программы.");
                Console.WriteLine("Пожалуйста, отредактируйте файл и укажите корректные пути к вашим обоям.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании файла конфигурации: {ex.Message}");
            }
        }
    }

    private static void ToggleTheme()
    {
        if (!File.Exists(configPath))
        {
            Console.WriteLine("Файл конфигурации отсутствует. Перезапустите программу, чтобы создать его.");
            return;
        }

        string[] wallpaperPaths = File.ReadAllLines(configPath);

        if (wallpaperPaths.Length < 2)
        {
            Console.WriteLine("Файл конфигурации должен содержать два пути: первый для светлых обоев, второй для тёмных.");
            return;
        }

        string lightWallpaper = wallpaperPaths[0].Trim();
        string darkWallpaper = wallpaperPaths[1].Trim();
        const int SPI_SETDESKWALLPAPER = 0x0014;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDCHANGE = 0x02;

        RegistryKey personalize = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", true);
        RegistryKey dwm = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM", true);

        if (personalize != null && dwm != null)
        {
            int currentTheme = (int)personalize.GetValue("AppsUseLightTheme");
            if (currentTheme == 1)
            {
                // Установка тёмной темы
                personalize.SetValue("AppsUseLightTheme", 0);
                personalize.SetValue("SystemUsesLightTheme", 0);
                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, darkWallpaper, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
                dwm.SetValue("ColorizationColor", 0x000000); // Чёрный цвет элементов
            }
            else
            {
                // Установка светлой темы
                personalize.SetValue("AppsUseLightTheme", 1);
                personalize.SetValue("SystemUsesLightTheme", 1);
                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, lightWallpaper, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
                dwm.SetValue("ColorizationColor", 0xFFFFFF); // Белый цвет элементов
            }

            RestartExplorer();
        }
    }

    private static void RestartExplorer()
    {
        // Завершение процесса Explorer
        foreach (var process in Process.GetProcessesByName("explorer"))
        {
            process.Kill();
        }

        // Запуск проводника заново
        Process.Start("explorer");
    }
}
