using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace ClipboardManager;

/// <summary>
/// Janela "Sobre": exibe nome, versão, mantenedor e repositório do app.
/// </summary>
public partial class AboutWindow : Window
{

    public AboutWindow()
    {
        InitializeComponent();
    }

    private void AboutWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void RepositoryLink_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo(AppInfo.RepositoryUrl)
            {
                UseShellExecute = true
            }
        );
    }

}
