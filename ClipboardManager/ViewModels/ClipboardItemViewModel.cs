using System.ComponentModel;
using System.Runtime.CompilerServices;

using ClipboardManager.Models;

namespace ClipboardManager.ViewModels;

/// <summary>
/// Representa um item exibido na interface.
/// </summary>
public class ClipboardItemViewModel : INotifyPropertyChanged
{

    private const int NameToolTipMaxLength = 300;

    private string? _name;

    private bool _isEditing;

    private string _editName = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

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

    /// <summary>
    /// Nome opcional dado pelo usuário.
    /// </summary>
    public string? Name
    {
        get => _name;
        set
        {
            if (_name == value)
            {
                return;
            }

            _name = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(HasName));
            OnPropertyChanged(nameof(DisplayText));
            OnPropertyChanged(nameof(DisplayToolTip));
        }
    }

    public bool HasName => !string.IsNullOrEmpty(Name);

    /// <summary>
    /// Texto exibido no card: o nome, quando houver; senão, o conteúdo.
    /// </summary>
    public string DisplayText => HasName
        ? Name!
        : PreviewText;

    /// <summary>
    /// Para itens com nome, o tooltip mostra o início do conteúdo real,
    /// já que ele não aparece mais no card. Sem nome, não há tooltip.
    /// </summary>
    public string? DisplayToolTip => HasName
        ? (PreviewText.Length > NameToolTipMaxLength
            ? PreviewText[..NameToolTipMaxLength] + "…"
            : PreviewText)
        : null;

    /// <summary>
    /// Indica se o card está em modo de edição do nome.
    /// </summary>
    public bool IsEditing
    {
        get => _isEditing;
        set
        {
            if (_isEditing == value)
            {
                return;
            }

            _isEditing = value;

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Texto digitado na caixa de edição, ainda não salvo.
    /// </summary>
    public string EditName
    {
        get => _editName;
        set
        {
            if (_editName == value)
            {
                return;
            }

            _editName = value;

            OnPropertyChanged();
        }
    }

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
            Name = item.Name,
            IsPinned = item.IsPinned,
            CreatedAt = item.CreatedAt,
            SortOrder = item.SortOrder,
            ContentLength = item.ContentLength
        };
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
