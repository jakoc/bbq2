namespace BobsBBQApi.BLL.Interfaces;

public interface IReservationLogic
{
    void ReserveTable(DateTime reservationDate, DateTime timeSlot, int partySize, string note, Guid userId,
        int tableNumber);

    List<DateTime> GetAvailableTimeSlot(DateTime date, int partySize);
    

}