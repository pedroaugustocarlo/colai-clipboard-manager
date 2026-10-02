namespace ClipboardManager;

/// <summary>
/// Metadados e textos fixos do aplicativo. Ponto único para strings usadas
/// em mais de um lugar (tray icon, janela Sobre, notificações etc), em vez
/// de espalhar literais pelo código.
/// </summary>
public static class AppInfo
{
    public const string Name = "Colaí";

    public const string FullName = "Colaí - Clipboard Manager";

    public const string Version = "1.0.0";

    /// <summary>
    /// Nome do executável publicado (sem acento/extensão — vira "ExecutableName.exe").
    /// Lido pelo publish.ps1 para nomear o .exe final, sem precisar repetir o valor nos dois lugares.
    /// </summary>
    public const string ExecutableName = "colai_clipboard_manager";

    // TODO: dados mocados — ajustar depois.
    public const string Author = "Pedro Augusto Carlo";

    // TODO: dados mocados — ajustar depois.
    public const string RepositoryUrl = "https://github.com/pedroaugustocarlo/colai-clipboard-manager/";

    public const string StartupNotificationMessage = "Colaí - Clipboard Manager em execução...";
}
