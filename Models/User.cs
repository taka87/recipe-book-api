using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class User
{
    public int Id { get; set; }

    [JsonPropertyName("first_name")] // 🔑 Мапва JSON ключа
    [Column("first_name")]
    public required string FirstName { get; set; }

    [JsonPropertyName("last_name")]
    [Column("last_name")]
    public required string LastName { get; set; }

    public required string Email { get; set; }

    [JsonPropertyName("password_hash")]
    [Column("password_hash")]
    public required string PasswordHash { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } // 🔑 Премахни "required"

    //[Column("role")]
    public string Role { get; set; } = "user"; // По подразбиране е "user"
}