using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;
using SQLite;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class SqliteTaskGroupsRepository : ITaskGroupsRepository
{
    private readonly SQLiteAsyncConnection _database;

    public SqliteTaskGroupsRepository(NotesDatabase database)
    {
        _database = database.Connection;
    }

    public async Task<List<TaskGroup>> GetAllAsync()
    {
        var groups = await _database.Table<TaskGroupEntity>().ToListAsync();
        var items = await _database.Table<TaskItemEntity>().ToListAsync();

        var itemsByGroup = items
            .GroupBy(i => i.TaskGroupId)
            .ToDictionary(g => g.Key, g => g.OrderBy(i => i.Position).Select(FromItemEntity).ToList());

        return groups.Select(group => new TaskGroup
        {
            Id = group.Id,
            Title = group.Title,
            CreatedAt = group.CreatedAt,
            ReminderAt = group.ReminderAt,
            Items = itemsByGroup.TryGetValue(group.Id, out var groupItems) ? groupItems : []
        }).ToList();
    }

    public async Task<TaskGroup?> GetByIdAsync(int id)
    {
        var group = await _database.FindAsync<TaskGroupEntity>(id);
        if (group is null)
            return null;

        var items = await _database.Table<TaskItemEntity>()
            .Where(i => i.TaskGroupId == id)
            .ToListAsync();

        return new TaskGroup
        {
            Id = group.Id,
            Title = group.Title,
            CreatedAt = group.CreatedAt,
            ReminderAt = group.ReminderAt,
            Items = items.OrderBy(i => i.Position).Select(FromItemEntity).ToList()
        };
    }

    public async Task<int> SaveAsync(TaskGroup group)
    {
        var groupEntity = new TaskGroupEntity
        {
            Id = group.Id,
            Title = group.Title,
            CreatedAt = group.CreatedAt,
            ReminderAt = group.ReminderAt
        };

        await _database.RunInTransactionAsync(connection =>
        {
            if (groupEntity.Id == 0)
                connection.Insert(groupEntity);
            else
                connection.Update(groupEntity);

            var existing = connection.Table<TaskItemEntity>()
                .Where(i => i.TaskGroupId == groupEntity.Id)
                .ToList();

            var keptIds = group.Items.Where(i => i.Id != 0).Select(i => i.Id).ToHashSet();

            foreach (var orphan in existing.Where(e => !keptIds.Contains(e.Id)))
                connection.Delete(orphan);

            for (var position = 0; position < group.Items.Count; position++)
            {
                var item = group.Items[position];
                var itemEntity = new TaskItemEntity
                {
                    Id = item.Id,
                    TaskGroupId = groupEntity.Id,
                    Title = item.Title,
                    IsCompleted = item.IsCompleted,
                    Importance = (int)item.Importance,
                    Position = position
                };

                if (itemEntity.Id == 0)
                    connection.Insert(itemEntity);
                else
                    connection.Update(itemEntity);
            }
        });

        return groupEntity.Id;
    }

    public async Task DeleteAsync(int id)
    {
        await _database.RunInTransactionAsync(connection =>
        {
            connection.Execute("DELETE FROM TaskItemEntity WHERE TaskGroupId = ?", id);
            connection.Delete<TaskGroupEntity>(id);
        });
    }

    private static TaskItem FromItemEntity(TaskItemEntity entity) => new()
    {
        Id = entity.Id,
        Title = entity.Title,
        IsCompleted = entity.IsCompleted,
        Importance = (TaskImportance)entity.Importance
    };
}
