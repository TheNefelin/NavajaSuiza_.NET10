using NavajaSuiza.Core.Models;

namespace NavajaSuiza.Core.Interfaces;

public interface ITaskGroupsRepository
{
    Task<List<TaskGroup>> GetAllAsync();

    Task<TaskGroup?> GetByIdAsync(int id);

    Task<int> SaveAsync(TaskGroup group);

    Task DeleteAsync(int id);
}
