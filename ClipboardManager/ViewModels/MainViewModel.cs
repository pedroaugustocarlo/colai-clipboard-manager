using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using ClipboardManager.Models;
using ClipboardManager.Services;

namespace ClipboardManager.ViewModels;

/// <summary>
/// ViewModel principal da aplicação.
/// </summary>
public class MainViewModel : INotifyPropertyChanged
{

    private readonly ClipboardHistoryService _historyService;

    private int _totalItems;

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainViewModel(
        ClipboardHistoryService historyService,
        ClipboardMonitorService monitorService)
    {
        _historyService = historyService;

        monitorService.ClipboardChanged += OnClipboardChanged;

        PinCommand = new RelayCommand(OnPin);
        DeleteCommand = new RelayCommand(OnDelete);
        ClearHistoryCommand = new RelayCommand(_ => OnClearHistory());

        RenameCommand = new RelayCommand(OnStartRename);

        MoveUpCommand = new RelayCommand(
            parameter => OnMove(parameter, MoveDirection.Up),
            parameter => CanMove(parameter, MoveDirection.Up)
        );

        MoveDownCommand = new RelayCommand(
            parameter => OnMove(parameter, MoveDirection.Down),
            parameter => CanMove(parameter, MoveDirection.Down)
        );

        RefreshItems();
    }

    public string Title => AppInfo.FullName;

    /// <summary>
    /// Itens exibidos na interface.
    /// </summary>
    public ObservableCollection<ClipboardItemViewModel> Items { get; } = new();

    /// <summary>
    /// Quantidade total de itens armazenados no histórico.
    /// </summary>
    public int TotalItems
    {
        get => _totalItems;
        private set
        {
            if (_totalItems == value)
            {
                return;
            }

            _totalItems = value;

            OnPropertyChanged();
        }
    }

    public ICommand PinCommand { get; }

    public ICommand DeleteCommand { get; }

    public ICommand ClearHistoryCommand { get; }

    public ICommand RenameCommand { get; }

    public int MaxNameLength => ClipboardHistoryService.MaxNameLength;

    public ICommand MoveUpCommand { get; }

    public ICommand MoveDownCommand { get; }

    /// <summary>
    /// Recarrega os itens do histórico a partir do repositório.
    /// </summary>
    public void RefreshItems()
    {
        // Os cards são recriados; salva antes uma edição de nome em andamento.
        CommitPendingRename();

        var items = _historyService.GetItems();

        Items.Clear();

        foreach (var item in items)
        {
            Items.Add(ClipboardItemViewModel.FromModel(item));
        }

        TotalItems = Items.Count;
    }

    private void OnClipboardChanged(string content)
    {
        _historyService.AddItem(content);

        RefreshItems();
    }

    private void OnPin(object? parameter)
    {
        if (parameter is not long id)
        {
            return;
        }

        var item = _historyService.GetItem(id);

        if (item is null)
        {
            return;
        }

        // Fixar apenas protege o item da limpeza do histórico;
        // não altera a posição dele na lista.
        _historyService.SetPinned(id, !item.IsPinned);

        RefreshItems();
    }

    private void OnDelete(object? parameter)
    {
        if (parameter is not long id)
        {
            return;
        }

        _historyService.DeleteItem(id);

        RefreshItems();
    }

    /// <summary>
    /// Coloca o card em modo de edição do nome. Só um card fica em edição
    /// por vez: qualquer outro em edição é salvo antes.
    /// </summary>
    private void OnStartRename(object? parameter)
    {
        if (parameter is not long id)
        {
            return;
        }

        var index = IndexOf(id);

        if (index < 0)
        {
            return;
        }

        CommitPendingRename();

        var item = Items[index];

        item.EditName = item.Name ?? string.Empty;
        item.IsEditing = true;
    }

    /// <summary>
    /// Salva o nome digitado. Nome vazio remove o nome do item.
    /// </summary>
    public void CommitRename(ClipboardItemViewModel item)
    {
        // Sai do modo de edição antes de salvar: a caixa de texto perde o
        // foco ao ser escondida, e isso chamaria este método de novo.
        if (!item.IsEditing)
        {
            return;
        }

        item.IsEditing = false;

        item.Name = _historyService.RenameItem(item.Id, item.EditName);
    }

    /// <summary>
    /// Descarta o que foi digitado e mantém o nome anterior.
    /// </summary>
    public void CancelRename(ClipboardItemViewModel item)
    {
        item.IsEditing = false;
    }

    /// <summary>
    /// Salva a edição em andamento, se houver (ex.: ao esconder a janela).
    /// </summary>
    public void CommitPendingRename()
    {
        foreach (var item in Items)
        {
            if (item.IsEditing)
            {
                CommitRename(item);
            }
        }
    }

    private int IndexOf(long id)
    {
        for (var i = 0; i < Items.Count; i++)
        {
            if (Items[i].Id == id)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Desabilita a seta para cima no primeiro item e a seta para baixo no último.
    /// </summary>
    private bool CanMove(object? parameter, MoveDirection direction)
    {
        if (parameter is not long id)
        {
            return false;
        }

        var index = IndexOf(id);

        if (index < 0)
        {
            return false;
        }

        return direction == MoveDirection.Up
            ? index > 0
            : index < Items.Count - 1;
    }

    private void OnMove(object? parameter, MoveDirection direction)
    {
        if (parameter is not long id)
        {
            return;
        }

        var index = IndexOf(id);

        if (index < 0)
        {
            return;
        }

        var newIndex = direction == MoveDirection.Up
            ? index - 1
            : index + 1;

        if (newIndex < 0 || newIndex >= Items.Count)
        {
            return;
        }

        if (!_historyService.MoveItem(id, direction))
        {
            // Banco e tela divergiram; recarrega para ficar consistente.
            RefreshItems();

            return;
        }

        // Atualiza só os dois itens envolvidos em vez de recarregar a lista
        // inteira, preservando a posição de rolagem da ListBox.
        var neighbor = Items[newIndex];

        (Items[index].SortOrder, neighbor.SortOrder) = (neighbor.SortOrder, Items[index].SortOrder);

        Items.Move(index, newIndex);

        CommandManager.InvalidateRequerySuggested();
    }

    private void OnClearHistory()
    {
        _historyService.ClearHistory();

        RefreshItems();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
