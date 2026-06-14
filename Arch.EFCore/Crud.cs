using Microsoft.EntityFrameworkCore;

namespace Practic11_12;

/// <summary>
/// Содержит примеры CRUD-операций для сущностей <see cref="Note"/> и <see cref="User"/>.
/// </summary>
/// <remarks>
/// CRUD (Create, Read, Update, Delete) это термин по умолчанию для таких запросов
/// (из которых состоит большая часть работы большинства программ).
/// </remarks>
public class Crud
{
    //  CRUD для Note
    
    /// <summary>
    /// Создаёт новую заметку и сохраняет её в БД
    /// </summary>
    /// <returns>
    /// Сущность новой заметки. После сохранения в БД её свойство <see cref="Note.Id"/>
    /// будет содержать реальный ID из СУБД (не 0)
    /// </returns>
    public static async Task<Note> Create(int id, string text, DateTimeOffset createdAt, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        
        // Создаем или находим пользователя по умолчанию для заметок 
        var defaultUser = await db.Users.FirstOrDefaultAsync(u => u.Email == "default@system.com", ct);
        if (defaultUser == null)
        {
            defaultUser = new User
            {
                Name = "System User",
                Email = "default@system.com",
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(defaultUser);
            await db.SaveChangesAsync(ct);
        }
        
        var note = new Note
        {
            Id = id,
            Text = text,
            CreatedAt = createdAt,
            UserId = defaultUser.Id,
            User = defaultUser
        };
        
        db.Notes.Add(note);
        await db.SaveChangesAsync(ct);
        return note;
    }

    /// <summary>
    /// Получает список заметок с поиском по частичному совпадению текста
    /// </summary>
    public static async Task<List<Note>> Read(string search, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        var result = await db.Notes
            .Include(x => x.User)
            .Where(x => EF.Functions.Like(x.Text, $"%{search}%"))
            .ToListAsync(ct);
        return result;
    }

    /// <summary>
    /// Ищет конкретную заметку по её ID
    /// </summary>
    /// <returns>
    /// Сущность найденной заметки или <see langword="null"/> если такой нет
    /// </returns>
    public static async Task<Note?> Read(int id, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        return await db.Notes
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    /// <summary>
    /// Обновляет сущность заметки в БД
    /// </summary>
    public static async Task Update(Note note, int id, string text, DateTimeOffset createdAt, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        note.Id = id;
        note.Text = text;
        note.CreatedAt = createdAt;
        db.Notes.Update(note);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Удаляет сущность заметки из БД
    /// </summary>
    public static async Task Delete(Note note, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        db.Notes.Remove(note);
        await db.SaveChangesAsync(ct);
    }

    // Новые CRUD методы для Note с поддержкой пользователей
    
    /// <summary>
    /// Создаёт новую заметку для конкретного пользователя
    /// </summary>
    public static async Task<Note> CreateNoteForUser(int userId, string text, DateTimeOffset createdAt, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (user == null)
            throw new ArgumentException($"Пользователь с ID {userId} не найден");
        
        var note = new Note
        {
            Text = text,
            CreatedAt = createdAt,
            UserId = userId,
            User = user
        };
        
        db.Notes.Add(note);
        await db.SaveChangesAsync(ct);
        return note;
    }
    
    /// <summary>
    /// Получает все заметки конкретного пользователя
    /// </summary>
    public static async Task<List<Note>> GetUserNotes(int userId, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        return await db.Notes
            .Include(x => x.User)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    // ==================== CRUD для User ====================
    
    /// <summary>
    /// Создаёт нового пользователя и сохраняет его в БД
    /// </summary>
    public static async Task<User> CreateUser(string name, string email, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        
        var existingUser = await db.Users.FirstOrDefaultAsync(x => x.Email == email, ct);
        if (existingUser != null)
            throw new ArgumentException($"Пользователь с email {email} уже существует");
        
        var user = new User
        {
            Name = name,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };
        
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return user;
    }

    /// <summary>
    /// Получает список пользователей с поиском по частичному совпадению имени или email
    /// </summary>
    public static async Task<List<User>> ReadUsers(string search, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        var result = await db.Users
            .Include(x => x.Notes)
            .Where(x => EF.Functions.Like(x.Name, $"%{search}%") ||
                        EF.Functions.Like(x.Email, $"%{search}%"))
            .ToListAsync(ct);
        return result;
    }

    /// <summary>
    /// Ищет конкретного пользователя по его ID
    /// </summary>
    public static async Task<User?> ReadUser(int id, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        return await db.Users
            .Include(x => x.Notes)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }
    
    /// <summary>
    /// Ищет пользователя по email
    /// </summary>
    public static async Task<User?> ReadUserByEmail(string email, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        return await db.Users
            .Include(x => x.Notes)
            .FirstOrDefaultAsync(x => x.Email == email, ct);
    }

    /// <summary>
    /// Обновляет данные пользователя
    /// </summary>
    public static async Task UpdateUser(User user, string name, string email, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        user.Name = name;
        user.Email = email;
        db.Users.Update(user);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Удаляет пользователя из БД. Все его заметки также будут удалены (каскадное удаление)
    /// </summary>
    public static async Task DeleteUser(User user, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        db.Users.Remove(user);
        await db.SaveChangesAsync(ct);
    }
    
    /// <summary>
    /// Получает количество заметок пользователя
    /// </summary>
    public static async Task<int> GetUserNotesCount(int userId, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        return await db.Notes.CountAsync(x => x.UserId == userId, ct);
    }
}