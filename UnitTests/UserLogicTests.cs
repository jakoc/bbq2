using BobsBBQApi.BE;
using BobsBBQApi.BLL;
using BobsBBQApi.DAL.Repositories.Interfaces;
using BobsBBQApi.Helpers.Interfaces;
using Moq;

namespace UnitTests
{
    [TestFixture]
    public class UserLogicTests
    {
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IPasswordEncrypter> _mockPasswordEncrypter;
        private Mock<IJwtToken> _mockJwtToken;
        private Mock<IEmail> _mockEmail;
        private UserLogic _userLogic;

        [SetUp]
        public void SetUp()
        {
            // Setup the mocks
            _mockUserRepository = new Mock<IUserRepository>();
            _mockPasswordEncrypter = new Mock<IPasswordEncrypter>();
            _mockJwtToken = new Mock<IJwtToken>();
            _mockEmail = new Mock<IEmail>();

            // Create an instance of UserLogic with mocked dependencies
            _userLogic = new UserLogic(
                _mockUserRepository.Object,
                _mockPasswordEncrypter.Object,
                _mockJwtToken.Object,
                _mockEmail.Object
            );
        }

        [Test]
        public void RegisterUser_ShouldThrowArgumentException_WhenUsernameIsEmpty()
        {
            // Arrange
            var username = "";
            var password = "password123";
            var email = "test@example.com";
            var phoneNumber = 1234567890;
            var note = "Test note";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _userLogic.RegisterUser(username, password, email, phoneNumber, note));
            Assert.That("Username cannot be null or empty.", Is.EqualTo(ex.Message));
        }


        [Test]
        public void RegisterUser_ShouldThrowArgumentException_WhenPasswordIsEmpty()
        {
            // Arrange
            var username = "testuser";
            var password = "";
            var email = "test@example.com";
            var phoneNumber = 1234567890;
            var note = "Test note";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _userLogic.RegisterUser(username, password, email, phoneNumber, note));
            Assert.That("Password cannot be null or empty.", Is.EqualTo(ex.Message));
        }

        [Test]
        public void RegisterUser_ShouldThrowArgumentException_WhenEmailAlreadyExists()
        {
            // Arrange
            var username = "testuser";
            var password = "password123";
            var email = "test@example.com";
            var phoneNumber = 1234567890;
            var note = "Test note";
            _mockUserRepository.Setup(r => r.GetUserByEmail(email)).Returns(new User()); // Simulate existing user

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _userLogic.RegisterUser(username, password, email, phoneNumber, note));
            Assert.That("Email is already taken.", Is.EqualTo(ex.Message));
        }

        [Test]
        public void RegisterUser_ShouldCallSendEmail_WhenUserIsSuccessfullyRegistered()
        {
            // Arrange
            var username = "testuser";
            var password = "password123";
            var email = "test@example.com";
            var phoneNumber = 1234567890;
            var note = "Test note";

            var salt = "randomSalt";
            var hash = "hashedPassword";
            _mockPasswordEncrypter.Setup(p => p.EncryptPassword(password)).Returns((hash, salt));
            _mockUserRepository.Setup(r => r.GetUserByEmail(email)).Returns((User)null); // Simulate no existing user
            _mockUserRepository.Setup(r => r.RegisterUser(It.IsAny<User>()))
                .Returns(true); // Simulate successful registration

            // Act
            var user = _userLogic.RegisterUser(username, password, email, phoneNumber, note);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(user, Is.Not.Null);
                Assert.That(username, Is.EqualTo(user.UserName));
                Assert.That(email, Is.EqualTo(user.Email));
            });
            _mockEmail.Verify(e => e.SendSuccessfullAccountCreationEmail(email, username), Times.Once);
        }



        [Test]
        public void LoginUser_ShouldThrowArgumentException_WhenEmailIsEmpty()
        {
            // Arrange
            var email = "";
            var password = "password123";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _userLogic.LoginUser(email, password));
            Assert.That("Email cannot be null or empty.", Is.EqualTo(ex.Message));
        }

        [Test]
        public void LoginUser_ShouldThrowArgumentException_WhenPasswordIsEmpty()
        {
            // Arrange
            var email = "test@example.com";
            var password = "";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _userLogic.LoginUser(email, password));
            Assert.That("Password cannot be null or empty.", Is.EqualTo(ex.Message));
        }

        [Test]
        public void LoginUser_ShouldThrowArgumentException_WhenUserNotFound()
        {
            // Arrange
            var email = "nonexistent@example.com";
            var password = "password123";
            
            _mockUserRepository.Setup(r => r.GetUserByEmail(email)).Returns((User?)null); // Simulate user not found

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _userLogic.LoginUser(email, password));
            Assert.That("Invalid email or password.", Is.EqualTo(ex.Message));
        }

        [Test]
        public void LoginUser_ShouldThrowArgumentException_WhenPasswordIsIncorrect()
        {
            // Arrange
            var email = "test@example.com";
            var password = "wrongPassword";
            var existingUser = new User { Email = email, UserSalt = "randomSalt", UserHash = "correctHash" };
            _mockUserRepository.Setup(r => r.GetUserByEmail(email)).Returns(existingUser); // Simulate user found

            _mockPasswordEncrypter.Setup(p => p.EncryptPasswordWithUsersSalt(password, existingUser.UserSalt))
                .Returns("wrongHash");

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _userLogic.LoginUser(email, password));
            Assert.That("Invalid email or password.", Is.EqualTo(ex.Message));
        }

        [Test]
        public void LoginUser_ShouldReturnToken_WhenLoginIsSuccessful()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var email = "test@example.com";
            var password = "password123";
            var userSalt = "randomSalt";
            var userHash = "hashedPassword";
            var role = "Customer";
            var existingUser = new User
                { UserId = userId, Email = email, UserSalt = userSalt, UserHash = userHash, UserRole = role };

            _mockUserRepository.Setup(r => r.GetUserByEmail(email)).Returns(existingUser);
            _mockPasswordEncrypter.Setup(p => p.EncryptPasswordWithUsersSalt(password, userSalt)).Returns(userHash);
            _mockJwtToken.Setup(j => j.GenerateJwtToken(userId, email, role)).Returns("fakeToken");

            // Act
            var token = _userLogic.LoginUser(email, password);

            // Assert
            Assert.That("fakeToken", Is.EqualTo(token.ToString())); // Ensure both are of the same type
            _mockJwtToken.Verify(j => j.GenerateJwtToken(userId, email, role), Times.Once);
        }
    }
}