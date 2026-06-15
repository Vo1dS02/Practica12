namespace Practic11_12
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Навигационное свойство для связи с заметками
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}