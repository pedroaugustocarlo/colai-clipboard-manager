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
                SortOrder,
                Name
            FROM
                ClipboardItem
            ORDER BY
                SortOrder DESC;
        ";

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            items.Add(ReadItem(reader));
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
                SortOrder,
                Name
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

        return ReadItem(reader);
    }

    /// <summary>
    /// Define o nome de um item. Nulo remove o nome.
    /// </summary>
    public void SetName(long id, string? name)
    {
        using var connection = _databaseService.CreateConnection();

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText =
        @"
            UPDATE ClipboardItem SET Name = $name WHERE Id = $id;
        ";

        command.Parameters.AddWithValue("$id", id);
        command.Parameters.AddWithValue("$name", (object?)name ?? DBNull.Value);

        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Converte a linha atual do reader em um item. As consultas devem
    /// selecionar as colunas nesta ordem: Id, Content, CreatedAt, IsPinned,
    /// SortOrder, Name.
    /// </summary>
    private static ClipboardItem ReadItem(SqliteDataReader reader)
    {
        return new ClipboardItem
        {
            Id = reader.GetInt64(0),
            Content = reader.GetString(1),
            CreatedAt = DateTime.Parse(reader.GetString(2)),
            IsPinned = reader.GetInt64(3) == 1,
            SortOrder = reader.GetInt64(4),
            Name = reader.IsDBNull(5) ? null : reader.GetString(5)
        };
    }

    /// <summary>
    /// Troca a posição (SortOrder) do item com o vizinho imediato na direção
    /// informada. Apenas os dois itens envolvidos são atualizados, em uma
    /// única transação. Retorna false se o item não existir ou já estiver
    /// no limite da lista.
    /// </summary>
    public bool Move(long id, MoveDirection direction)
    {
        using var connection = _databaseService.CreateConnection();

        connection.Open();

        using var transaction = connection.BeginTransaction();

        using var currentCommand = connection.CreateCommand();

        currentCommand.Transaction = transaction;

        currentCommand.CommandText =
        @"
            SELECT SortOrder FROM ClipboardItem WHERE Id = $id;
        ";

        currentCommand.Parameters.AddWithValue("$id", id);

        var currentResult = currentCommand.ExecuteScalar();

        if (currentResult is null or DBNull)
        {
            return false;
        }

        var currentSortOrder = Convert.ToInt64(currentResult);

        // O vizinho é buscado pelo valor de SortOrder (e não pela posição),
        // então buracos na numeração deixados por exclusões não atrapalham.
        using var neighborCommand = connection.CreateCommand();

        neighborCommand.Transaction = transaction;

        neighborCommand.CommandText = direction == MoveDirection.Up
            ? @"
                SELECT Id, SortOrder FROM ClipboardItem
                WHERE SortOrder > $sortOrder
                ORDER BY SortOrder ASC
                LIMIT 1;
            "
            : @"
                SELECT Id, SortOrder FROM ClipboardItem
                WHERE SortOrder < $sortOrder
                ORDER BY SortOrder DESC
                LIMIT 1;
            ";

        neighborCommand.Parameters.AddWithValue("$sortOrder", currentSortOrder);

        long neighborId;
        long neighborSortOrder;

        using (var reader = neighborCommand.ExecuteReader())
        {
            if (!reader.Read())
            {
                return false;
            }

            neighborId = reader.GetInt64(0);
            neighborSortOrder = reader.GetInt64(1);
        }

        using var updateCommand = connection.CreateCommand();

        updateCommand.Transaction = transaction;

        updateCommand.CommandText =
        @"
            UPDATE ClipboardItem
            SET SortOrder = CASE Id
                WHEN $id THEN $neighborSortOrder
                WHEN $neighborId THEN $currentSortOrder
            END
            WHERE Id IN ($id, $neighborId);
        ";

        updateCommand.Parameters.AddWithValue("$id", id);
        updateCommand.Parameters.AddWithValue("$neighborId", neighborId);
        updateCommand.Parameters.AddWithValue("$currentSortOrder", currentSortOrder);
        updateCommand.Parameters.AddWithValue("$neighborSortOrder", neighborSortOrder);

        updateCommand.ExecuteNonQuery();

        transaction.Commit();

        return true;
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
