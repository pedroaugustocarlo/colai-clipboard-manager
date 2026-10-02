using ClipboardManager.Models;

namespace ClipboardManager.ViewModels;

/// <summary>
/// Representa um item exibido na interface.
/// </summary>
public class ClipboardItemViewModel
{

    public long Id { get; set; }

    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Versão de exibição do conteúdo, sem quebras de linha, para o card
    /// da lista sempre caber em uma única linha. O texto original (com
    /// quebras) continua em <see cref="Content"/>, usado ao colar.
    /// </summary>
    public string PreviewText => Content
        .Replace("\r\n", " ")
        .Replace('\n', ' ')
        .Replace('\r', ' ');

    public bool IsPinned { get; set; }

    /// <summary>
    /// Ícone exibido no botão de fixação.
    /// </summary>
    public string PinButtonIcon => IsPinned
        ? "📌"
        : "📍";

    /// <summary>
    /// Texto exibido no tooltip do botão de fixação.
    /// </summary>
    public string PinButtonText => IsPinned
        ? "Desafixar (permitir limpeza)"
        : "Fixar (proteger da limpeza do histórico)";

    public DateTime CreatedAt { get; set; }

    public long SortOrder { get; set; }

    public int ContentLength { get; set; }

    public static ClipboardItemViewModel FromModel(ClipboardItem item)
    {
        return new ClipboardItemViewModel
        {
            Id = item.Id,
            Content = item.Content,
            IsPinned = item.IsPinned,
            CreatedAt = item.CreatedAt,
            SortOrder = item.SortOrder,
            ContentLength = item.ContentLength
        };
    }

}
