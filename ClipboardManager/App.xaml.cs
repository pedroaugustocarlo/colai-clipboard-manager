using ClipboardManager.Services;
using System.Windows;
using System.Windows.Threading;
using ClipboardManager.Data;
using ClipboardManager.Models;

namespace ClipboardManager;

public partial class App : System.Windows.Application
{

    protected override void OnStartup(StartupEventArgs e)
    {
        // Registrados antes de base.OnStartup para capturar qualquer erro
        // durante a criação/exibição da MainWindow (StartupUri), inclusive
        // erros de parse de XAML.
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        base.OnStartup(e);

        var databaseService = new DatabaseService();

        // Cria o banco e as tabelas caso não existam.
        databaseService.Initialize();
    }

    private void OnDispatcherUnhandledException(
        object sender,
        DispatcherUnhandledExceptionEventArgs e
    )
    {
        System.Windows.MessageBox.Show(
            e.Exception.ToString(),
            "Erro não tratado (UI)"
        );

        e.Handled = true;
    }

    private void OnUnhandledException(
        object sender,
        UnhandledExceptionEventArgs e
    )
    {
        System.Windows.MessageBox.Show(
            (e.ExceptionObject as Exception)?.ToString() ?? "Erro desconhecido",
            "Erro fatal"
        );
    }

}
