
using Moq;

using BobsBBQApi.DAL.Repositories.Interfaces;
using BobsBBQApi.Helpers;
using BobsBBQApi.BE;
using BobsBBQApi.BLL;

namespace UnitTests
{
    [TestFixture]
    public class ReservationLogicTests
    {
        private Mock<IReservationRepository> _reservationRepoMock;
        private Mock<ITableRepository> _tableRepoMock;
        private Mock<IUserRepository> _userRepoMock;
        private ReservationLogic _reservationLogic;

        [SetUp]
        public void Setup()
        {
            _reservationRepoMock = new Mock<IReservationRepository>();
            _tableRepoMock = new Mock<ITableRepository>();

            _reservationLogic = new ReservationLogic(
                _reservationRepoMock.Object,
                _tableRepoMock.Object
            );
        }

        [Test]
        public void GetAvailableTimeSlot_ShouldReturnAvailableSlots_WhenTablesAreFree()
        {
            var date = DateTime.Today.AddDays(1);
            var partySize = 4;
            var defaultSlots = DefaultTimeSlots.GetSlots(date);

            _reservationRepoMock.Setup(r => r.GetReservedSlots(date))
                .Returns(new List<DateTime>());

            _tableRepoMock.Setup(t => t.GetTables())
                .Returns(new List<Table>
                {
                    new Table { TableId = Guid.NewGuid(), TableNumber = 1, Capacity = 4 }
                });

            var result = _reservationLogic.GetAvailableTimeSlot(date, partySize);

            Assert.IsNotEmpty(result);
            Assert.That(defaultSlots.Count, Is.EqualTo(result.Count));
        }

        [Test]
        public void GetAvailableTimeSlot_ShouldThrow_WhenNoTableCanFitPartySize()
        {
            var date = DateTime.Today.AddDays(1);
            var partySize = 10;

            _tableRepoMock.Setup(t => t.GetTables())
                .Returns(new List<Table>
                {
                    new Table { TableId = Guid.NewGuid(), TableNumber = 1, Capacity = 4 }
                });

            _reservationRepoMock.Setup(r => r.GetReservedSlots(date))
                .Returns(new List<DateTime>());

            Assert.Throws<ArgumentException>(() =>
                _reservationLogic.GetAvailableTimeSlot(date, partySize));
        }

        [Test]
        public void ReserveTable_ShouldReserve_WhenInputsAreValid()
        {
            var date = DateTime.Today.AddDays(1);
            var timeSlot = 12;
            var partySize = 4;
            var userId = Guid.NewGuid();


            var tableId = Guid.NewGuid();

            _reservationRepoMock.Setup(r => r.GetReservedSlots(date))
                .Returns(new List<DateTime>());

            _tableRepoMock.Setup(t => t.GetTables())
                .Returns(new List<Table>
                {
                    new Table { TableId = tableId, Capacity = 4 }
                });

            _reservationRepoMock.Setup(r => r.ReserveTable(It.IsAny<Reservation>()))
                .Verifiable();

            _reservationLogic.ReserveTable(date, timeSlot, partySize, "Note", userId);

            _reservationRepoMock.Verify(r => r.ReserveTable(It.Is<Reservation>(res =>
                res.UserId == userId &&
                res.TableId == tableId &&
                res.PartySize == partySize &&
                res.ReservationDate == date &&
                res.TimeSlot == timeSlot
            )), Times.Once);
        }



        [Test]
        public void ReserveTable_ShouldThrow_WhenTimeSlotUnavailable()
        {
            var date = DateTime.Today.AddDays(1);
            var invalidTimeSlot = 6; // outside default slots
            var partySize = 2;
            var userId = Guid.NewGuid();

            _reservationRepoMock.Setup(r => r.GetReservedSlots(date))
                .Returns(DefaultTimeSlots.GetSlots(date)); // All slots taken

            _tableRepoMock.Setup(t => t.GetTables())
                .Returns(new List<Table>
                {
                    new Table { TableId = Guid.NewGuid(), TableNumber = 1, Capacity = 4 }
                });

            var ex = Assert.Throws<ArgumentException>(() =>
                _reservationLogic.ReserveTable(date, invalidTimeSlot, partySize, "Note", userId));

            Assert.That(ex.Message, Is.EqualTo("Time slot must be between 10 AM and 10 PM."));
        }

        [TestCase(0)]
        [TestCase(11)]
        public void ReserveTable_ShouldThrow_WhenPartySizeOutOfRange(int partySize)
        {
            var date = DateTime.Today.AddDays(1);
            var timeSlot = 12;
            var userId = Guid.NewGuid();

            var table = new Table
            {
                TableId = Guid.NewGuid(),
                Capacity = 12
            };

            var availableSlots = DefaultTimeSlots.GetSlots(date);
            _reservationRepoMock.Setup(r => r.GetReservedSlots(date))
                .Returns(new List<DateTime>()); // All slots free
            _tableRepoMock.Setup(t => t.GetTables())
                .Returns(new List<Table> { table });

            var ex = Assert.Throws<ArgumentException>(() =>
                _reservationLogic.ReserveTable(date, timeSlot, partySize, "Note", userId));

            Assert.That(ex.Message, Does.Contain("Party size must be between 1 and 10"));
        }

        [TestCase(8)] // 8 AM
        [TestCase(23)] // 11 PM
        public void ReserveTable_ShouldThrow_WhenTimeSlotOutsideOperatingHours(int hour)
        {
            var date = DateTime.Today.AddDays(1);
            var timeSlot = hour;
            var partySize = 4;
            var userId = Guid.NewGuid();

            _reservationRepoMock.Setup(r => r.GetReservedSlots(date))
                .Returns(DefaultTimeSlots.GetSlots(date));

            _tableRepoMock.Setup(t => t.GetTables())
                .Returns(new List<Table>
                {
                    new Table { TableId = Guid.NewGuid(), Capacity = 6 }
                });

            var ex = Assert.Throws<ArgumentException>(() =>
                _reservationLogic.ReserveTable(date, timeSlot, partySize, "Note", userId));

            Assert.That(ex.Message, Is.EqualTo("Time slot must be between 10 AM and 10 PM."));
        }

        [Test]
        public void ReserveTable_ShouldThrow_WhenReservationDateInPast()
        {
            var date = DateTime.Today.AddDays(-1); // Past date
            var timeSlot = 12;
            var partySize = 2;
            var userId = Guid.NewGuid();


            _tableRepoMock.Setup(t => t.GetTables())
                .Returns(new List<Table>
                {
                    new Table { TableId = Guid.NewGuid(), Capacity = 4 }
                });

            _reservationRepoMock.Setup(r => r.GetReservedSlots(date))
                .Returns(DefaultTimeSlots.GetSlots(date));

            var ex = Assert.Throws<ArgumentException>(() =>
                _reservationLogic.ReserveTable(date, timeSlot, partySize, "Note", userId));

            Assert.That(ex.Message, Is.EqualTo("Reservation date cannot be in the past."));
        }

        [Test]
        public void ReserveTable_ShouldThrow_WhenUserIdIsEmpty()
        {
            var date = DateTime.Today.AddDays(1);
            var timeSlot = 12;
            var partySize = 2;
            var emptyUserId = Guid.Empty;

            _reservationRepoMock.Setup(r => r.GetReservedSlots(date))
                .Returns(DefaultTimeSlots.GetSlots(date));

            _tableRepoMock.Setup(t => t.GetTables())
                .Returns(new List<Table>
                {
                    new Table { TableId = Guid.NewGuid(), Capacity = 4 }
                });

            var ex = Assert.Throws<ArgumentException>(() =>
                _reservationLogic.ReserveTable(date, timeSlot, partySize, "Note", emptyUserId));

            Assert.That(ex.Message, Is.EqualTo("User ID cannot be empty."));
        }

        [Test]
        public void GetAvailableTimeSlot_ShouldReturnSlots_WhenTablesAreAvailable()
        {
            var date = DateTime.Today.AddDays(1);
            var partySize = 4;

            _tableRepoMock.Setup(t => t.GetTables())
                .Returns(new List<Table>
                {
                    new Table { TableId = Guid.NewGuid(), TableNumber = 1, Capacity = 4 }
                });

            var availableSlots = DefaultTimeSlots.GetSlots(date);

            _reservationRepoMock.Setup(r => r.GetReservedSlots(date))
                .Returns(new List<DateTime>()); // No reserved slots for the test

            var result = _reservationLogic.GetAvailableTimeSlot(date, partySize);

            Assert.That(availableSlots.Count, Is.EqualTo(result.Count));
        }

        [Test]
        public void GetAvailableTimeSlot_ShouldThrow_WhenNoTablesCanFitPartySize()
        {
            var date = DateTime.Today.AddDays(1);
            var partySize = 10;

            _tableRepoMock.Setup(t => t.GetTables())
                .Returns(new List<Table>
                {
                    new Table { TableId = Guid.NewGuid(), TableNumber = 1, Capacity = 4 }
                });

            _reservationRepoMock.Setup(r => r.GetReservedSlots(date))
                .Returns(new List<DateTime>()); // No reserved slots for the test

            Assert.Throws<ArgumentException>(() =>
                _reservationLogic.GetAvailableTimeSlot(date, partySize));
        }

        [Test]
        public void GetAvailableTimeSlot_ShouldReturnEmptyList_WhenAllSlotsAreTaken()
        {
            var date = DateTime.Today.AddDays(1);
            var partySize = 4;

            var tableId = Guid.NewGuid();

            _tableRepoMock.Setup(t => t.GetTables())
                .Returns(new List<Table>
                {
                    new Table { TableId = tableId, TableNumber = 1, Capacity = 4 }
                });

            _reservationRepoMock
                .Setup(r => r.IsTableReservedAt(It.IsAny<Guid>(), date, It.IsAny<int>()))
                .Returns(true); // Simulate all slots are taken

            var result = _reservationLogic.GetAvailableTimeSlot(date, partySize);

            Assert.That(result, Is.Empty);
        }

        [TestCase(8)] // 8 AM
        [TestCase(23)] // 11 PM
        public void ReserveTable_ShouldThrow_WhenTimeSlotIsOutsideOperatingHours(int timeSlot)
        {
            var date = DateTime.Today.AddDays(1);
            var partySize = 4;
            var userId = Guid.NewGuid();

            _tableRepoMock.Setup(t => t.GetTables())
                .Returns(new List<Table>
                {
                    new Table { TableId = Guid.NewGuid(), Capacity = 4 }
                });

            _reservationRepoMock.Setup(r => r.GetReservedSlots(date))
                .Returns(new List<DateTime>()); // All slots free

            var ex = Assert.Throws<ArgumentException>(() =>
                _reservationLogic.ReserveTable(date, timeSlot, partySize, "Note", userId));

            Assert.That(ex.Message, Is.EqualTo("Time slot must be between 10 AM and 10 PM."));
        }

        [Test]
        public void ReserveTable_ShouldThrow_WhenNoAvailableTableForPartySize()
        {
            var date = DateTime.Today.AddDays(1);
            var timeSlot = 12;
            var partySize = 10; // No table can accommodate this party size
            var userId = Guid.NewGuid();

            _tableRepoMock.Setup(t => t.GetTables())
                .Returns(new List<Table>
                {
                    new Table { TableId = Guid.NewGuid(), Capacity = 4 } // Only small tables
                });

            _reservationRepoMock.Setup(r => r.GetReservedSlots(date))
                .Returns(new List<DateTime>()); // All slots free

            var ex = Assert.Throws<ArgumentException>(() =>
                _reservationLogic.ReserveTable(date, timeSlot, partySize, "Note", userId));

            Assert.That(ex.Message, Is.EqualTo("No tables available for this party size."));
        }

        [Test]
        public void ReserveTable_ShouldThrow_WhenAllTablesAreReservedForTimeSlot()
        {
            var date = DateTime.Today.AddDays(1);
            var timeSlot = 12;
            var partySize = 4;
            var userId = Guid.NewGuid();

            var tableId = Guid.NewGuid();

            _tableRepoMock.Setup(t => t.GetTables())
                .Returns(new List<Table>
                {
                    new Table { TableId = tableId, Capacity = 4 }
                });

            _reservationRepoMock.Setup(r =>
                    r.IsTableReservedAt(tableId, date, timeSlot))
                .Returns(true); // Table is reserved

            var ex = Assert.Throws<ArgumentException>(() =>
                _reservationLogic.ReserveTable(date, timeSlot, partySize, "Note", userId));

            Assert.That(ex.Message, Is.EqualTo("The selected time slot is not available."));
        }

        [Test]
        public void ReserveTable_ShouldThrow_WhenReservationDateIsInThePast()
        {
            var date = DateTime.Today.AddDays(-1); // Past date
            var timeSlot = 12;
            var partySize = 4;
            var userId = Guid.NewGuid();

            _tableRepoMock.Setup(t => t.GetTables())
                .Returns(new List<Table>
                {
                    new Table { TableId = Guid.NewGuid(), Capacity = 4 }
                });

            _reservationRepoMock.Setup(r => r.GetReservedSlots(date))
                .Returns(new List<DateTime>()); // All slots free

            var ex = Assert.Throws<ArgumentException>(() =>
                _reservationLogic.ReserveTable(date, timeSlot, partySize, "Note", userId));

            Assert.That(ex.Message, Is.EqualTo("Reservation date cannot be in the past."));
        }

        [Test]
        public void ReserveTable_ShouldThrowArgumentException_WhenReservationDateIsInThePast()
        {
            // Arrange
            var reservationDate = DateTime.Now.AddDays(-1); // A date in the past
            var timeSlot = 10;
            var partySize = 4;
            var userId = Guid.NewGuid();
            var note = "Test note";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _reservationLogic.ReserveTable(reservationDate, timeSlot, partySize, note, userId));

            Assert.That("Reservation date cannot be in the past.", Is.EqualTo(ex.Message));
        }

        [Test]
        public void ReserveTable_ShouldThrowArgumentException_WhenTimeSlotIsBefore10AM()
        {
            // Arrange
            var reservationDate = DateTime.Today.AddDays(1); // A valid date in the future
            var timeSlot = 9; // An invalid time slot (before 10 AM)
            var partySize = 4;
            var userId = Guid.NewGuid();
            var note = "Test note";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _reservationLogic.ReserveTable(reservationDate, timeSlot, partySize, note, userId));

            Assert.That("Time slot must be between 10 AM and 10 PM.", Is.EqualTo(ex.Message));
        }

        [Test]
        public void ReserveTable_ShouldThrowArgumentException_WhenTimeSlotIsAfter10PM()
        {
            // Arrange
            var reservationDate = DateTime.Today.AddDays(1); // A valid date in the future
            var timeSlot = 23; // An invalid time slot (after 10 PM)
            var partySize = 4;
            var userId = Guid.NewGuid();
            var note = "Test note";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _reservationLogic.ReserveTable(reservationDate, timeSlot, partySize, note, userId));

            Assert.That("Time slot must be between 10 AM and 10 PM.", Is.EqualTo(ex.Message));
        }

        [Test]
        public void ReserveTable_ShouldNotThrow_WhenInputsAreValid()
        {
            // Arrange
            var reservationDate = DateTime.Today.AddDays(1); // A valid date in the future
            var timeSlot = 12; // A valid time slot (between 10 AM and 10 PM)
            var partySize = 4;
            var userId = Guid.NewGuid();
            var note = "Test note";
            var tables = new List<Table>
            {
                new Table { TableId = Guid.NewGuid(), Capacity = 4, TableNumber = 1 },
            };
            _tableRepoMock.Setup(t => t.GetTables()).Returns(tables);

            // Mock GetReservedSlots to return no reservations for the date and time slot
            _reservationRepoMock.Setup(r => r.GetReservedSlots(reservationDate)).Returns(new List<DateTime>());

            // Act & Assert
            Assert.DoesNotThrow(() =>
                _reservationLogic.ReserveTable(reservationDate, timeSlot, partySize, note, userId));
        }

        [Test]
        public void ReserveTable_ShouldThrowArgumentException_WhenPartySizeIsLessThanOrEqualToZero()
        {
            // Arrange
            var reservationDate = DateTime.Today.AddDays(1); // A valid date in the future
            var timeSlot = 12; // A valid time slot (between 10 AM and 10 PM)
            var partySize = 0; // An invalid party size (less than or equal to zero)
            var userId = Guid.NewGuid();
            var note = "Test note";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _reservationLogic.ReserveTable(reservationDate, timeSlot, partySize, note, userId));

            Assert.That("Party size must be between 1 and 10.", Is.EqualTo(ex.Message));
        }
    }
}

