using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Practic11_12.Tests;

public class CrudTests : IDisposable
{
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    
    public CrudTests()
    {
        // Для SQLite - полная очистка БД
        using var db = new DataContext();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }
    
    public void Dispose()
    {
        using var db = new DataContext();
        db.Database.EnsureDeleted();
    }
    
    #region Тесты для Note (исправленные для SQLite)
    
    [Fact]
    public async Task Create_ShouldCreateNewNote_WhenValidDataProvided()
    {
        // Для SQLite используем уникальные ID
        var id = DateTime.Now.Ticks.GetHashCode();
        var text = "Test note content";
        var createdAt = DateTime.Now;
        
        var note = await Crud.Create(id, text, createdAt, _cancellationToken);
        
        Assert.NotNull(note);
        Assert.Equal(id, note.Id);
        Assert.Equal(text, note.Text);
        Assert.Equal(createdAt, note.CreatedAt);
        Assert.NotEqual(0, note.UserId);
        
        var savedNote = await Crud.Read(id, _cancellationToken);
        Assert.NotNull(savedNote);
        Assert.Equal(text, savedNote.Text);
    }
    
    [Fact]
    public async Task Read_WithSearchText_ShouldReturnMatchingNotes()
    {
        // SQLite LIKE чувствителен к регистру, используем однозначные значения
        var id1 = DateTime.Now.Ticks.GetHashCode();
        var id2 = DateTime.Now.Ticks.GetHashCode();
        var id3 = DateTime.Now.Ticks.GetHashCode();
        
        await Crud.Create(id1, "Hello world test", DateTime.Now, _cancellationToken);
        await Crud.Create(id2, "Goodbye world test", DateTime.Now, _cancellationToken);
        await Crud.Create(id3, "Test message unique", DateTime.Now, _cancellationToken);
        
        var result = await Crud.Read("world", _cancellationToken);
        
        // SQLite может вернуть 2 заметки с "world"
        Assert.True(result.Count >= 2, $"Expected at least 2 notes, got {result.Count}");
        Assert.All(result, n => Assert.Contains("world", n.Text));
    }
    
    [Fact]
    public async Task Read_WithEmptySearch_ShouldReturnAllNotes()
    {
        var id1 = DateTime.Now.Ticks.GetHashCode();
        var id2 = DateTime.Now.Ticks.GetHashCode();
        
        await Crud.Create(id1, "Note 1", DateTime.Now, _cancellationToken);
        await Crud.Create(id2, "Note 2", DateTime.Now, _cancellationToken);
        
        var result = await Crud.Read("", _cancellationToken);
        
        Assert.Equal(2, result.Count);
    }
    
    [Fact]
    public async Task Read_ById_ShouldReturnNote_WhenNoteExists()
    {
        var id = DateTime.Now.Ticks.GetHashCode();
        var expectedNote = await Crud.Create(id, "Test note", DateTime.Now, _cancellationToken);
        
        var actualNote = await Crud.Read(id, _cancellationToken);
        
        Assert.NotNull(actualNote);
        Assert.Equal(expectedNote.Id, actualNote.Id);
        Assert.Equal(expectedNote.Text, actualNote.Text);
        Assert.NotNull(actualNote.User);
    }
    
    [Fact]
    public async Task Read_ById_ShouldReturnNull_WhenNoteDoesNotExist()
    {
        var note = await Crud.Read(999999, _cancellationToken);
        Assert.Null(note);
    }
    
    [Fact]
    public async Task Update_ShouldModifyNoteProperties()
    {
        var id = DateTime.Now.Ticks.GetHashCode();
        var originalNote = await Crud.Create(id, "Original text", DateTime.Now, _cancellationToken);
        var newText = "Updated text";
        var newCreatedAt = DateTime.Now.AddDays(1);
        
        // Для SQLite важно использовать тот же экземпляр
        await Crud.Update(originalNote, id, newText, newCreatedAt, _cancellationToken);
        
        var updatedNote = await Crud.Read(id, _cancellationToken);
        Assert.NotNull(updatedNote);
        Assert.Equal(newText, updatedNote.Text);
        Assert.Equal(newCreatedAt, updatedNote.CreatedAt);
    }
    
    [Fact]
    public async Task Delete_ShouldRemoveNoteFromDatabase()
    {
        var id = DateTime.Now.Ticks.GetHashCode();
        var note = await Crud.Create(id, "Note to delete", DateTime.Now, _cancellationToken);
        
        await Crud.Delete(note, _cancellationToken);
        
        var deletedNote = await Crud.Read(id, _cancellationToken);
        Assert.Null(deletedNote);
    }
    
    #endregion
    
    #region Тесты для User (исправленные для SQLite)
    
    [Fact]
    public async Task CreateUser_ShouldCreateNewUser_WhenValidDataProvided()
    {
        var uniqueEmail = $"test{Guid.NewGuid()}@example.com";
        var name = "John Doe";
        
        var user = await Crud.CreateUser(name, uniqueEmail, _cancellationToken);
        
        Assert.NotNull(user);
        Assert.Equal(name, user.Name);
        Assert.Equal(uniqueEmail, user.Email);
        Assert.NotEqual(0, user.Id);
        Assert.True(user.CreatedAt <= DateTime.UtcNow.AddSeconds(1));
    }
    
    [Fact]
    public async Task CreateUser_ShouldThrowException_WhenEmailAlreadyExists()
    {
        var email = $"duplicate{Guid.NewGuid()}@example.com";
        await Crud.CreateUser("First User", email, _cancellationToken);
        
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => Crud.CreateUser("Second User", email, _cancellationToken)
        );
        Assert.Contains(email, exception.Message);
    }
    
    [Fact]
    public async Task ReadUsers_WithSearchText_ShouldReturnMatchingUsers()
    {
        var uniqueEmail1 = $"alice{Guid.NewGuid()}@test.com";
        var uniqueEmail2 = $"bob{Guid.NewGuid()}@test.com";
        
        await Crud.CreateUser("Alice Smith", uniqueEmail1, _cancellationToken);
        await Crud.CreateUser("Bob Johnson", uniqueEmail2, _cancellationToken);
        
        var result = await Crud.ReadUsers("Alice", _cancellationToken);
        
        Assert.Single(result);
        Assert.Equal("Alice Smith", result[0].Name);
    }
    
    [Fact]
    public async Task ReadUsers_WithEmailSearch_ShouldReturnMatchingUsers()
    {
        var uniqueEmail = $"test{Guid.NewGuid()}@example.com";
        await Crud.CreateUser("Test User", uniqueEmail, _cancellationToken);
        
        var result = await Crud.ReadUsers("@example.com", _cancellationToken);
        
        Assert.Contains(result, u => u.Email == uniqueEmail);
    }
    
    [Fact]
    public async Task ReadUser_ById_ShouldReturnUser_WhenUserExists()
    {
        var uniqueEmail = $"test{Guid.NewGuid()}@test.com";
        var expectedUser = await Crud.CreateUser("Test User", uniqueEmail, _cancellationToken);
        
        var actualUser = await Crud.ReadUser(expectedUser.Id, _cancellationToken);
        
        Assert.NotNull(actualUser);
        Assert.Equal(expectedUser.Id, actualUser.Id);
        Assert.Equal(expectedUser.Name, actualUser.Name);
        Assert.Equal(expectedUser.Email, actualUser.Email);
    }
    
    [Fact]
    public async Task ReadUser_ById_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var user = await Crud.ReadUser(999999, _cancellationToken);
        Assert.Null(user);
    }
    
    [Fact]
    public async Task ReadUserByEmail_ShouldReturnUser_WhenEmailExists()
    {
        var uniqueEmail = $"unique{Guid.NewGuid()}@email.com";
        var expectedUser = await Crud.CreateUser("Test User", uniqueEmail, _cancellationToken);
        
        var actualUser = await Crud.ReadUserByEmail(uniqueEmail, _cancellationToken);
        
        Assert.NotNull(actualUser);
        Assert.Equal(expectedUser.Id, actualUser.Id);
    }
    
    [Fact]
    public async Task ReadUserByEmail_ShouldReturnNull_WhenEmailDoesNotExist()
    {
        var user = await Crud.ReadUserByEmail($"nonexistent{Guid.NewGuid()}@email.com", _cancellationToken);
        Assert.Null(user);
    }
    
    [Fact]
    public async Task UpdateUser_ShouldModifyUserProperties()
    {
        var uniqueEmail = $"old{Guid.NewGuid()}@email.com";
        var user = await Crud.CreateUser("Old Name", uniqueEmail, _cancellationToken);
        var newName = "New Name";
        var newEmail = $"new{Guid.NewGuid()}@email.com";
        
        await Crud.UpdateUser(user, newName, newEmail, _cancellationToken);
        
        var updatedUser = await Crud.ReadUser(user.Id, _cancellationToken);
        Assert.NotNull(updatedUser);
        Assert.Equal(newName, updatedUser.Name);
        Assert.Equal(newEmail, updatedUser.Email);
    }
    
    [Fact]
    public async Task DeleteUser_ShouldRemoveUserAndTheirNotes()
    {
        var uniqueEmail = $"delete{Guid.NewGuid()}@test.com";
        var user = await Crud.CreateUser("User to Delete", uniqueEmail, _cancellationToken);
        await Crud.CreateNoteForUser(user.Id, "Note 1", DateTime.Now, _cancellationToken);
        await Crud.CreateNoteForUser(user.Id, "Note 2", DateTime.Now, _cancellationToken);
        
        await Crud.DeleteUser(user, _cancellationToken);
        
        var deletedUser = await Crud.ReadUser(user.Id, _cancellationToken);
        Assert.Null(deletedUser);
        
        var userNotes = await Crud.GetUserNotes(user.Id, _cancellationToken);
        Assert.Empty(userNotes);
    }
    
    [Fact]
    public async Task GetUserNotesCount_ShouldReturnCorrectCount()
    {
        var uniqueEmail = $"count{Guid.NewGuid()}@test.com";
        var user = await Crud.CreateUser("Test User", uniqueEmail, _cancellationToken);
        await Crud.CreateNoteForUser(user.Id, "Note 1", DateTime.Now, _cancellationToken);
        await Crud.CreateNoteForUser(user.Id, "Note 2", DateTime.Now, _cancellationToken);
        
        var count = await Crud.GetUserNotesCount(user.Id, _cancellationToken);
        
        Assert.Equal(2, count);
    }
    
    #endregion
    
    #region Тесты для связей Note-User
    
    [Fact]
    public async Task CreateNoteForUser_ShouldCreateNote_WhenUserExists()
    {
        var uniqueEmail = $"user{Guid.NewGuid()}@notes.com";
        var user = await Crud.CreateUser("Test User", uniqueEmail, _cancellationToken);
        var text = "User's note";
        var createdAt = DateTime.Now;
        
        var note = await Crud.CreateNoteForUser(user.Id, text, createdAt, _cancellationToken);
        
        Assert.NotNull(note);
        Assert.Equal(user.Id, note.UserId);
        Assert.Equal(text, note.Text);
        Assert.Equal(createdAt, note.CreatedAt);
        Assert.NotEqual(0, note.Id);
    }
    
    [Fact]
    public async Task CreateNoteForUser_ShouldThrowException_WhenUserDoesNotExist()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => Crud.CreateNoteForUser(999999, "Note text", DateTime.Now, _cancellationToken)
        );
        Assert.Contains("999", exception.Message);
    }
    
    [Fact]
    public async Task GetUserNotes_ShouldReturnAllNotesForSpecificUser()
    {
        var uniqueEmail1 = $"user1{Guid.NewGuid()}@test.com";
        var uniqueEmail2 = $"user2{Guid.NewGuid()}@test.com";
        
        var user1 = await Crud.CreateUser("User 1", uniqueEmail1, _cancellationToken);
        var user2 = await Crud.CreateUser("User 2", uniqueEmail2, _cancellationToken);
        
        await Crud.CreateNoteForUser(user1.Id, "User1 Note 1", DateTime.Now, _cancellationToken);
        await Crud.CreateNoteForUser(user1.Id, "User1 Note 2", DateTime.Now, _cancellationToken);
        await Crud.CreateNoteForUser(user2.Id, "User2 Note 1", DateTime.Now, _cancellationToken);
        
        var user1Notes = await Crud.GetUserNotes(user1.Id, _cancellationToken);
        
        Assert.Equal(2, user1Notes.Count);
        Assert.All(user1Notes, note => Assert.Equal(user1.Id, note.UserId));
    }
    
    [Fact]
    public async Task GetUserNotes_ShouldReturnNotesInDescendingOrder()
    {
        var uniqueEmail = $"order{Guid.NewGuid()}@test.com";
        var user = await Crud.CreateUser("Test User", uniqueEmail, _cancellationToken);
        
        var now = DateTime.Now;
        await Crud.CreateNoteForUser(user.Id, "First", now.AddSeconds(-2), _cancellationToken);
        await Task.Delay(10);
        await Crud.CreateNoteForUser(user.Id, "Second", now.AddSeconds(-1), _cancellationToken);
        await Task.Delay(10);
        await Crud.CreateNoteForUser(user.Id, "Third", now, _cancellationToken);
        
        var notes = await Crud.GetUserNotes(user.Id, _cancellationToken);
        
        Assert.Equal(3, notes.Count);
        // Проверяем, что заметки отсортированы по убыванию (новые первые)
        for (int i = 0; i < notes.Count - 1; i++)
        {
            Assert.True(notes[i].CreatedAt >= notes[i + 1].CreatedAt, 
                $"Note {i} is not newer than note {i+1}");
        }
    }
    
    [Fact]
    public async Task Read_ShouldIncludeUserDataInNotes()
    {
        var uniqueEmail = $"john{Guid.NewGuid()}@notes.com";
        var user = await Crud.CreateUser("John Doe", uniqueEmail, _cancellationToken);
        var note = await Crud.CreateNoteForUser(user.Id, "Test note", DateTime.Now, _cancellationToken);
        
        var retrievedNote = await Crud.Read(note.Id, _cancellationToken);
        
        Assert.NotNull(retrievedNote);
        Assert.NotNull(retrievedNote.User);
        Assert.Equal(user.Name, retrievedNote.User.Name);
        Assert.Equal(user.Email, retrievedNote.User.Email);
    }
    
    [Fact]
    public async Task ReadUsers_ShouldIncludeNotesInUsers()
    {
        var uniqueEmail = $"notes{Guid.NewGuid()}@user.com";
        var user = await Crud.CreateUser("User with notes", uniqueEmail, _cancellationToken);
        await Crud.CreateNoteForUser(user.Id, "Note 1", DateTime.Now, _cancellationToken);
        await Crud.CreateNoteForUser(user.Id, "Note 2", DateTime.Now, _cancellationToken);
        
        var users = await Crud.ReadUsers("User with notes", _cancellationToken);
        
        var retrievedUser = users.First();
        Assert.NotNull(retrievedUser.Notes);
        Assert.Equal(2, retrievedUser.Notes.Count);
    }
    
    #endregion
}