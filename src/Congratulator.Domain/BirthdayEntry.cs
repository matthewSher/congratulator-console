namespace Congratulator.Domain;

public class BirthdayEntry
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }

    public BirthdayEntry(int id, string name, DateOnly dateOfBirth)
    {
        Id = id;
        Name = name;
        DateOfBirth = dateOfBirth;
    }
}