using CafeBarrio.Infrastructure.Security;
using FluentAssertions;
using Xunit;

namespace CafeBarrio.Tests.Unit.Auth;

public class Argon2PasswordHasherTests
{
    private readonly Argon2PasswordHasher _sut;

    public Argon2PasswordHasherTests()
    {
        _sut = new Argon2PasswordHasher();
    }

    [Fact]
    public void Hash_ReturnsNonEmptyString()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash = _sut.Hash(password);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().Contain(":"); // Format base64Salt:base64Hash
    }

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "SecurePassword123!";
        var hash = _sut.Hash(password);

        // Act
        var isValid = _sut.Verify(password, hash);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var password = "SecurePassword123!";
        var wrongPassword = "WrongPassword!";
        var hash = _sut.Hash(password);

        // Act
        var isValid = _sut.Verify(wrongPassword, hash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void Hash_GeneratesDifferentHashesForSamePassword()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash1 = _sut.Hash(password);
        var hash2 = _sut.Hash(password);

        // Assert
        hash1.Should().NotBe(hash2);
    }
}
