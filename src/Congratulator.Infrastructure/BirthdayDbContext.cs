using Congratulator.Application;
using Congratulator.Domain;
using Microsoft.EntityFrameworkCore;

namespace Congratulator.Infrastructure;

public class BirthdayDbContext(DbContextOptions<BirthdayDbContext> options) : DbContext(options), IBirthdayRepository
{
    public DbSet<BirthdayEntry> Birthdays => Set<BirthdayEntry>();

    // Реализация интерфейса
    public async Task AddAsync(BirthdayEntry entry)
    {
        Birthdays.Add(entry);
        await SaveChangesAsync();
    }

    public Task<List<BirthdayEntry>> GetAllAsync() =>
        Birthdays.ToListAsync();

    public async Task UpdateAsync(BirthdayEntry entry)
    {
        Birthdays.Update(entry);
        await SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await Birthdays.FindAsync(id);
        if (entity != null)
        {
            Birthdays.Remove(entity);
            await SaveChangesAsync();
        }
    }
}
