using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

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

    /// <summary>
    /// Recarrega os itens do histórico a partir do repositório.
    /// </summary>
    public void RefreshItems()
    {
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
