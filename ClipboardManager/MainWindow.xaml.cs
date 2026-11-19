using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

using ClipboardManager.Data;
using ClipboardManager.Helpers;
using ClipboardManager.Services;
using ClipboardManager.ViewModels;

namespace ClipboardManager;

public partial class MainWindow : Window
{

    private readonly MainViewModel _viewModel;
    private readonly ClipboardMonitorService _monitorService;

    private const int WM_CLIPBOARDUPDATE = 0x031D;
    private const int WM_HOTKEY = 0x0312;

    private const uint MOD_WIN = 0x0008;
    private const uint MOD_ALT = 0x0001;

    private const int HOTKEY_ID = 1;

    /// <summary>
    /// Último conteúdo enviado ao clipboard pelo próprio Clipboard Manager
    /// </summary>
    private string? _lastInternalClipboardContent;

    /// <summary>
    /// Momento em que o Clipboard Manager atualizou o clipboard
    /// </summary>
    private DateTime _lastInternalClipboardUpdate;

    /// <summary>
    /// Último conteúdo capturado, utilizado para ignorar eventos repetidos
    /// </summary>
    private string? _lastClipboardContent;

    /// <summary>
    /// Janela que estava em primeiro plano antes da janela principal ser aberta
    /// </summary>
    private IntPtr _previousForegroundWindow;

    private System.Windows.Forms.NotifyIcon? _notifyIcon;

    private bool _isExiting;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool AddClipboardFormatListener(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(
        IntPtr hWnd,
        int id,
        uint fsModifiers,
        uint virtualKey
    );

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(
        IntPtr hWnd,
        int id
    );

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    public MainWindow()
    {
        InitializeComponent();

        var databaseService = new DatabaseService();

        var repository = new ClipboardItemRepository(databaseService);

        var historyService = new ClipboardHistoryService(repository);

        _monitorService = new ClipboardMonitorService();

        _viewModel = new MainViewModel(historyService, _monitorService);

        DataContext = _viewModel;

        InitializeNotifyIcon();

        Loaded += MainWindow_Loaded;
        Deactivated += MainWindow_Deactivated;
    }

    private void InitializeNotifyIcon()
    {
        try
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Text = AppInfo.FullName,
                Icon = new System.Drawing.Icon(
                    System.Windows.Application.GetResourceStream(
                        new Uri("pack://application:,,,/Assets/colai_16_16.ico")
                    )!.Stream
                ),
                Visible = true
            };

            var contextMenu = new System.Windows.Forms.ContextMenuStrip();

            contextMenu.Items.Add(
                "Sobre",
                null,
                AboutMenuItem_Click
            );

            contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

            contextMenu.Items.Add(
                "Encerrar",
                null,
                ExitMenuItem_Click
            );

            _notifyIcon.ContextMenuStrip = contextMenu;

            _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

            _notifyIcon.ShowBalloonTip(
                3000,
                "",
                AppInfo.StartupNotificationMessage,
                System.Windows.Forms.ToolTipIcon.None
            );
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                ex.ToString(),
                "Erro ao carregar ícone"
            );
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        var hwnd = new WindowInteropHelper(this).Handle;

        RegisterHotKey(
            hwnd,
            HOTKEY_ID,
            MOD_WIN | MOD_ALT,
            (uint)KeyInterop.VirtualKeyFromKey(Key.V)
        );

        AddClipboardFormatListener(hwnd);

        var source = HwndSource.FromHwnd(hwnd);

        source?.AddHook(WndProc);
    }

    private IntPtr WndProc(
        IntPtr hwnd,
        int msg,
        IntPtr wParam,
        IntPtr lParam,
        ref bool handled
    )
    {
        if (msg == WM_HOTKEY)
        {
            // Guarda a janela ativa antes de exibir o Colaí.
            _previousForegroundWindow = GetForegroundWindow();

            ShowNearCursor();

            handled = true;

            return IntPtr.Zero;
        }

        if (msg != WM_CLIPBOARDUPDATE)
        {
            return IntPtr.Zero;
        }

        if (!System.Windows.Clipboard.ContainsText())
        {
            return IntPtr.Zero;
        }

        var content = System.Windows.Clipboard.GetText().Trim();

        // Ignora temporariamente o conteúdo enviado pelo próprio Clipboard Manager.
        if (
            _lastInternalClipboardContent == content
            && DateTime.Now - _lastInternalClipboardUpdate < TimeSpan.FromSeconds(10)
        )
        {
            return IntPtr.Zero;
        }

        // Ignora eventos consecutivos com o mesmo conteúdo.
        if (_lastClipboardContent == content)
        {
            return IntPtr.Zero;
        }

        _lastClipboardContent = content;

        _monitorService.OnClipboardChanged(content);

        handled = true;

        return IntPtr.Zero;
    }

    private async void ClipboardItem_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e
    )
    {
        if (e.ClickCount != 2)
        {
            return;
        }

        if (sender is not Border border)
        {
            return;
        }

        if (border.DataContext is not ClipboardItemViewModel item)
        {
            return;
        }

        var content = item.Content;

        _lastInternalClipboardContent = content;
        _lastInternalClipboardUpdate = DateTime.Now;

        var clipboardUpdated = ClipboardHelper.SetText(content);

        if (!clipboardUpdated)
        {
            return;
        }

        Hide();

        if (_previousForegroundWindow == IntPtr.Zero)
        {
            return;
        }

        SetForegroundWindow(_previousForegroundWindow);

        // Aguarda a janela anterior recuperar efetivamente o foco.
        await Task.Delay(300);

        System.Windows.Forms.SendKeys.SendWait("^v");
    }

    /// <summary>
    /// Ao entrar em modo de edição, foca a caixa e seleciona o texto,
    /// para o usuário já poder digitar o novo nome.
    /// </summary>
    private void NameTextBox_IsVisibleChanged(
        object sender,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (sender is not System.Windows.Controls.TextBox textBox || e.NewValue is not true)
        {
            return;
        }

        // Adiado até o layout terminar; antes disso o Focus() não tem efeito.
        Dispatcher.BeginInvoke(
            System.Windows.Threading.DispatcherPriority.Input,
            () =>
            {
                textBox.Focus();
                textBox.SelectAll();
            }
        );
    }

    private void NameTextBox_PreviewKeyDown(
        object sender,
        System.Windows.Input.KeyEventArgs e
    )
    {
        if (sender is not FrameworkElement { DataContext: ClipboardItemViewModel item })
        {
            return;
        }

        if (e.Key == Key.Enter)
        {
            _viewModel.CommitRename(item);

            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            // Tratado aqui (Preview) para o Esc não chegar ao OnKeyDown da
            // janela, que a esconderia.
            _viewModel.CancelRename(item);

            e.Handled = true;
        }
    }

    private void NameTextBox_LostKeyboardFocus(
        object sender,
        KeyboardFocusChangedEventArgs e
    )
    {
        if (sender is FrameworkElement { DataContext: ClipboardItemViewModel item })
        {
            _viewModel.CommitRename(item);
        }
    }

    private void ArrowButtons_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e
    )
    {
        e.Handled = true;
    }

    private void ExitMenuItem_Click(
        object? sender,
        EventArgs e
    )
    {
        _isExiting = true;

        if (_notifyIcon is not null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }

        var hwnd = new WindowInteropHelper(this).Handle;

        UnregisterHotKey(hwnd, HOTKEY_ID);

        System.Windows.Application.Current.Shutdown();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (!_isExiting)
        {
            e.Cancel = true;

            Hide();

            return;
        }

        base.OnClosing(e);
    }

    private void NotifyIcon_DoubleClick(
        object? sender,
        EventArgs e
    )
    {
        ShowNearCursor();
    }

    private void AboutMenuItem_Click(
        object? sender,
        EventArgs e
    )
    {
        var aboutWindow = new AboutWindow();

        aboutWindow.ShowDialog();
    }

    private void HideButton_Click(
        object sender,
        RoutedEventArgs e
    )
    {
        Hide();
    }

    private void Header_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e
    )
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
    {
        // e.Handled: o Esc já foi usado para cancelar a edição de um nome.
        if (e.Key == Key.Escape && !e.Handled)
        {
            Hide();

            e.Handled = true;

            return;
        }

        base.OnKeyDown(e);
    }

    private void MainWindow_Loaded(
        object sender,
        RoutedEventArgs e
    )
    {
        Hide();
    }

    private void MainWindow_Deactivated(
        object? sender,
        EventArgs e
    )
    {
        // Ao trocar de janela no meio da edição, o nome digitado é salvo.
        _viewModel.CommitPendingRename();

        Hide();
    }

    /// <summary>
    /// Exibe a janela próxima ao cursor do mouse, dentro dos limites da
    /// área útil do monitor onde o cursor está (funciona com múltiplos
    /// monitores e com escalas de DPI diferentes entre eles).
    /// </summary>
    private void ShowNearCursor()
    {
        var cursorPosition = System.Windows.Forms.Cursor.Position;

        var workArea = System.Windows.Forms.Screen
            .FromPoint(cursorPosition)
            .WorkingArea;

        var transformFromDevice =
            PresentationSource.FromVisual(this)?.CompositionTarget?.TransformFromDevice
            ?? Matrix.Identity;

        var cursor = transformFromDevice.Transform(
            new System.Windows.Point(cursorPosition.X, cursorPosition.Y))
        ;

        var workAreaTopLeft = transformFromDevice.Transform(
            new System.Windows.Point(workArea.Left, workArea.Top)
        );

        var workAreaBottomRight = transformFromDevice.Transform(
            new System.Windows.Point(workArea.Right, workArea.Bottom)
        );

        var left = Math.Min(cursor.X, workAreaBottomRight.X - Width);
        var top = Math.Min(cursor.Y, workAreaBottomRight.Y - Height);

        left = Math.Max(left, workAreaTopLeft.X);
        top = Math.Max(top, workAreaTopLeft.Y);

        Left = left;
        Top = top;

        Show();

        WindowState = WindowState.Normal;

        Activate();
    }

}
