namespace ClipboardManager.Models;

/// <summary>
/// Representa um item armazenado no histórico da área de transferência.
/// </summary>
public class ClipboardItem
{

    public long Id { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public bool IsPinned { get; set; }

    /// <summary>
    /// Controla a posição do item na listagem.
    /// Quanto maior o valor, mais acima o item aparecerá.
    /// </summary>
    public long SortOrder { get; set; }

    /// <summary>
    /// Indica a quantidade de caracteres do conteúdo.
    /// </summary>
    public int ContentLength => Content.Length;

}
