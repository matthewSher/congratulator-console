using Congratulator.Domain;

namespace Congratulator.Application;

public interface IBirthdayRepository
{
    Task AddAsync(BirthdayEntry entry);
    Task<List<BirthdayEntry>> GetAllAsync();
    Task UpdateAsync(BirthdayEntry entry);
    Task DeleteAsync(int id);
}
