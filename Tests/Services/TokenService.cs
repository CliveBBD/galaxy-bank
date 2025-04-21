using Api.Models;
using Api.Services;
using Xunit;

namespace Api.Tests.Services
{
    public class TokenServiceTest
    {
        [Fact]
        public void StoreToken_ShouldAddNewToken_WhenKeyDoesNotExist()
        {
            // Arrange
            var tokenService = new TokenService();
            var userKey = "user1";
            var token = new StoredToken { IdToken = "abc123", Role = "customer", SessionId = "session1" };

            // Act
            tokenService.StoreToken(userKey, token);

            // Xunit.Assert
            var storedToken = tokenService.GetToken(userKey);
            Xunit.Assert.NotNull(storedToken);
            Xunit.Assert.Equal("abc123", storedToken?.IdToken);
        }

        [Fact]
        public void StoreToken_ShouldUpdateToken_WhenKeyAlreadyExists()
        {
            // Arrange
            var tokenService = new TokenService();
            var userKey = "user1";
            var initialToken = new StoredToken { IdToken = "abc123", Role = "customer", SessionId = "session1" };
            var updatedToken = new StoredToken { IdToken = "xyz789", Role = "admin", SessionId = "session2" };

            tokenService.StoreToken(userKey, initialToken);

            // Act
            tokenService.StoreToken(userKey, updatedToken);

            // Xunit.Assert
            var storedToken = tokenService.GetToken(userKey);
            Xunit.Assert.NotNull(storedToken);
            Xunit.Assert.Equal("xyz789", storedToken?.IdToken);
        }

        [Fact]
        public void GetToken_ShouldReturnToken_WhenKeyExists()
        {
            // Arrange
            var tokenService = new TokenService();
            var userKey = "user1";
            var token = new StoredToken { IdToken = "abc123", Role = "customer", SessionId = "session1" };

            tokenService.StoreToken(userKey, token);

            // Act
            var storedToken = tokenService.GetToken(userKey);

            // Xunit.Assert
            Xunit.Assert.NotNull(storedToken);
            Xunit.Assert.Equal("abc123", storedToken?.IdToken);
        }

        [Fact]
        public void GetToken_ShouldReturnNull_WhenKeyDoesNotExist()
        {
            // Arrange
            var tokenService = new TokenService();
            var userKey = "nonexistent";

            // Act
            var storedToken = tokenService.GetToken(userKey);

            // Xunit.Assert
            Xunit.Assert.Null(storedToken);
        }

        [Fact]
        public void RemoveToken_ShouldRemoveToken_WhenKeyExists()
        {
            // Arrange
            var tokenService = new TokenService();
            var userKey = "user1";
            var token = new StoredToken { IdToken = "abc123", Role = "customer", SessionId = "session1" };

            tokenService.StoreToken(userKey, token);

            // Act
            tokenService.RemoveToken(userKey);

            // Xunit.Assert
            var storedToken = tokenService.GetToken(userKey);
            Xunit.Assert.Null(storedToken);
        }

        [Fact]
        public void RemoveToken_ShouldDoNothing_WhenKeyDoesNotExist()
        {
            // Arrange
            var tokenService = new TokenService();
            var userKey = "nonexistent";

            // Act
            tokenService.RemoveToken(userKey);

            // Xunit.Assert
            var storedToken = tokenService.GetToken(userKey);
            Xunit.Assert.Null(storedToken); // Ensure no exception is thrown and nothing changes
        }
    }
}