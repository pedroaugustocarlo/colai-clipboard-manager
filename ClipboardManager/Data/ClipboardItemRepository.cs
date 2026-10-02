using ClipboardManager.Models;
using ClipboardManager.Services;
using Microsoft.Data.Sqlite;

namespace ClipboardManager.Data;

/// <summary>
/// Responsável pelas operações de persistência dos itens da área de transferência.
/// </summary>
public class ClipboardItemRepository
{

    private readonly DatabaseService _databaseService;

    public ClipboardItemRepository(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    /// <summary>
    /// Insere um novo item no histórico.
    /// </summary>
    public void Insert(ClipboardItem item)
    {
        using var connection = _databaseService.CreateConnection();

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText =
        @"
            INSERT INTO ClipboardItem(
                Content,
                CreatedAt,
                IsPinned,
                SortOrder
            ) VALUES (
                $content,
                $createdAt,
                $isPinned,
                $sortOrder
            );
        ";

        command.Parameters.AddWithValue("$content", item.Content);
        command.Parameters.AddWithValue("$createdAt", item.CreatedAt);
        command.Parameters.AddWithValue("$isPinned", item.IsPinned ? 1 : 0);
        command.Parameters.AddWithValue("$sortOrder", item.SortOrder);

        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Retorna o maior valor de ordenação existente.
    /// </summary>
    public long GetMaxSortOrder()
    {
        using var connection = _databaseService.CreateConnection();

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText =
        @"
            SELECT IFNULL(MAX(SortOrder), 0) FROM ClipboardItem;
        ";

        var result = command.ExecuteScalar();

        return Convert.ToInt64(result);
    }

    /// <summary>
    /// Retorna todos os itens do histórico ordenados do mais recente para o mais antigo.
    /// </summary>
    public List<ClipboardItem> GetAll()
    {
        var items = new List<ClipboardItem>();

        using var connection = _databaseService.CreateConnection();

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText =
        @"
            SELECT
                Id,
                Content,
                CreatedAt,
                IsPinned,
                SortOrder
            FROM
                ClipboardItem
            ORDER BY
                SortOrder DESC;
        ";

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            items.Add(new ClipboardItem
                {
                    Id = reader.GetInt64(0),
                    Content = reader.GetString(1),
                    CreatedAt = DateTime.Parse(reader.GetString(2)),
                    IsPinned = reader.GetInt64(3) == 1,
                    SortOrder = reader.GetInt64(4)
                }
            );
        }

        return items;
    }

    /// <summary>
    /// Remove todos os itens não fixados do histórico.
    /// </summary>
    public void DeleteAllUnpinned()
    {
        using var connection = _databaseService.CreateConnection();

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText =
        @"
            DELETE FROM ClipboardItem WHERE IsPinned = 0;
        ";

        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Atualiza o estado de fixação de um item.
    /// </summary>
    public void SetPinned(long id, bool isPinned)
    {
        using var connection = _databaseService.CreateConnection();

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText =
        @"
            UPDATE ClipboardItem SET IsPinned = $isPinned WHERE Id = $id;
        ";

        command.Parameters.AddWithValue("$id", id);
        command.Parameters.AddWithValue("$isPinned", isPinned ? 1 : 0);

        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Retorna um item pelo identificador.
    /// </summary>
    public ClipboardItem? GetById(long id)
    {
        using var connection = _databaseService.CreateConnection();

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText =
        @"
            SELECT
                Id,
                Content,
                CreatedAt,
                IsPinned,
                SortOrder
            FROM
                ClipboardItem
            WHERE
                Id = $id;
        ";

        command.Parameters.AddWithValue("$id", id);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
        {
            return null;
        }

        return new ClipboardItem
        {
            Id = reader.GetInt64(0),
            Content = reader.GetString(1),
            CreatedAt = DateTime.Parse(reader.GetString(2)),
            IsPinned = reader.GetInt64(3) == 1,
            SortOrder = reader.GetInt64(4)
        };
    }

    /// <summary>
    /// Remove um item do histórico.
    /// </summary>
    public void Delete(long id)
    {
        using var connection = _databaseService.CreateConnection();

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText =
        @"
            DELETE FROM ClipboardItem WHERE Id = $id;
        ";

        command.Parameters.AddWithValue("$id", id);

        command.ExecuteNonQuery();
    }

}
