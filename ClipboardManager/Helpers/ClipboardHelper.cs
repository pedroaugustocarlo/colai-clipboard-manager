namespace ClipboardManager.Helpers;

/// <summary>
/// Utilitário para manipulação segura da área de transferência.
/// </summary>
public static class ClipboardHelper
{

    /// <summary>
    /// Escreve no clipboard usando o retry nativo do Windows
    /// (OleSetClipboard) para lidar com bloqueios temporários causados por
    /// outros processos (histórico do Windows, antivírus, RDP etc).
    /// Bem mais rápido que um retry manual com Thread.Sleep.
    /// </summary>
    public static bool SetText(string text)
    {
        try
        {
            System.Windows.Forms.Clipboard.SetDataObject(
                text,
                copy: true,
                retryTimes: 10,
                retryDelay: 50
            );

            return true;
        }
        catch
        {
            return false;
        }
    }

}
