using Practic11_12;

Console.WriteLine("Hello, World!");
await using var db = new DataContext();
await db.Database.EnsureCreatedAsync();