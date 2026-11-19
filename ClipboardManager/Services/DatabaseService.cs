using System.IO;
using Microsoft.Data.Sqlite;

namespace ClipboardManager.Services;

/// <summary>
/// Responsável por gerenciar a conexão e inicialização do banco SQLite.
/// </summary>
public class DatabaseService
{

    // %LOCALAPPDATA%\Colai\clipboard.db: padrão do Windows para dado
    // específico do usuário/máquina (não deve ir em pasta Roaming nem
    // depender do diretório de trabalho de onde o app foi iniciado).
    private static readonly string DatabaseDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Colai"
    );

    private static readonly string ConnectionString =
        $"Data Source={Path.Combine(DatabaseDirectory, "clipboard.db")}";

    /// <summary>
    /// Cria o banco de dados e as tabelas necessárias caso não existam.
    /// </summary>
    public void Initialize()
    {
        Directory.CreateDirectory(DatabaseDirectory);

        using var connection = new SqliteConnection(ConnectionString);

        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText =
        @"
            CREATE TABLE IF NOT EXISTS ClipboardItem
            (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                Content     TEXT NOT NULL,
                CreatedAt   TEXT NOT NULL,
                IsPinned    INTEGER NOT NULL DEFAULT 0,
                SortOrder   INTEGER NOT NULL,
                Name        TEXT NULL
            );
        ";

        command.ExecuteNonQuery();

        AddColumnIfMissing(connection, "ClipboardItem", "Name", "TEXT NULL");
    }

    /// <summary>
    /// Migração simples para bancos criados por versões anteriores:
    /// CREATE TABLE IF NOT EXISTS não altera uma tabela já existente.
    /// </summary>
    private static void AddColumnIfMissing(
        SqliteConnection connection,
        string table,
        string column,
        string definition
    )
    {
        using var checkCommand = connection.CreateCommand();

        checkCommand.CommandText = $"SELECT COUNT(*) FROM pragma_table_info('{table}') WHERE name = $column;";

        checkCommand.Parameters.AddWithValue("$column", column);

        if (Convert.ToInt64(checkCommand.ExecuteScalar()) > 0)
        {
            return;
        }

        using var alterCommand = connection.CreateCommand();

        alterCommand.CommandText = $"ALTER TABLE {table} ADD COLUMN {column} {definition};";

        alterCommand.ExecuteNonQuery();
    }

    /// <summary>
    /// Cria uma nova conexão com o banco SQLite.
    /// </summary>
    public SqliteConnection CreateConnection()
    {
        return new SqliteConnection(ConnectionString);
    }

}
