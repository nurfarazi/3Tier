using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UserManagement.Shared.Models.Entities;

/// <summary>
/// User domain entity representing a registered user in the system.
/// This entity is persisted in MongoDB with specific collection mapping.
/// </summary>
[BsonCollection("users")]
public class User
{
    /// <summary>
    /// Unique MongoDB object ID for the user.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    /// <summary>
    /// User's email address - unique identifier for authentication and communication.
    /// </summary>
    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password using BCrypt algorithm for secure storage.
    /// Never store plain-text passwords.
    /// </summary>
    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// User's first name.
    /// </summary>
    [BsonElement("firstName")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name.
    /// </summary>
    [BsonElement("lastName")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Optional phone number for contact purposes.
    /// </summary>
    [BsonElement("phoneNumber")]
    [BsonIgnoreIfNull]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Timestamp when the user was created (UTC).
    /// </summary>
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the user was last updated (UTC).
    /// </summary>
    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// MongoDB collection attribute for convention-based mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class BsonCollectionAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the BsonCollectionAttribute class.
    /// </summary>
    /// <param name="collectionName">Name of the MongoDB collection this entity maps to.</param>
    public BsonCollectionAttribute(string collectionName)
    {
        CollectionName = collectionName;
    }

    /// <summary>
    /// Gets the name of the MongoDB collection.
    /// </summary>
    public string CollectionName { get; }
}
