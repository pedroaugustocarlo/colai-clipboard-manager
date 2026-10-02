namespace ClipboardManager.Services;

/// <summary>
/// Responsável por monitorar alterações na área de transferência do Windows.
/// </summary>
public class ClipboardMonitorService
{

    /// <summary>
    /// Disparado quando o conteúdo da área de transferência for alterado.
    /// </summary>
    public event Action<string>? ClipboardChanged;

    /// <summary>
    /// Dispara o evento de alteração do clipboard.
    /// </summary>
    public void OnClipboardChanged(string content)
    {
        ClipboardChanged?.Invoke(content);
    }

}
