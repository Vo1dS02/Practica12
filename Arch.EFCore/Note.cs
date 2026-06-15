namespace Practic11_12;

public class Note
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    // Внешний ключ
    public int UserId { get; set; }
    
    // Навигационное свойство
    public virtual User User { get; set; } = null!;
}
