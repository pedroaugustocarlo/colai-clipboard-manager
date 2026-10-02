using ClipboardManager.Data;
using ClipboardManager.Models;

namespace ClipboardManager.Services;

/// <summary>
/// Responsável pelas regras de negócio do histórico da área de transferência.
/// </summary>
public class ClipboardHistoryService
{

    private readonly ClipboardItemRepository _repository;

    public ClipboardHistoryService(ClipboardItemRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Adiciona um novo item ao histórico.
    /// </summary>
    public void AddItem(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        var nextSortOrder = _repository.GetMaxSortOrder() + 1;

        var item = new ClipboardItem
        {
            Content = content,
            CreatedAt = DateTime.Now,
            IsPinned = false,
            SortOrder = nextSortOrder
        };

        _repository.Insert(item);
    }

    /// <summary>
    /// Retorna todos os itens do histórico ordenados do mais recente para o mais antigo.
    /// </summary>
    public List<ClipboardItem> GetItems()
    {
        return _repository.GetAll();
    }

    /// <summary>
    /// Remove todos os itens não fixados do histórico.
    /// </summary>
    public void ClearHistory()
    {
        _repository.DeleteAllUnpinned();
    }

    /// <summary>
    /// Fixa ou desafixa um item do histórico.
    /// </summary>
    public void SetPinned(long id, bool isPinned)
    {
        _repository.SetPinned(id, isPinned);
    }

    /// <summary>
    /// Retorna um item do histórico pelo identificador.
    /// </summary>
    public ClipboardItem? GetItem(long id)
    {
        return _repository.GetById(id);
    }

    /// <summary>
    /// Remove um item do histórico.
    /// </summary>
    public void DeleteItem(long id)
    {
        _repository.Delete(id);
    }

}
