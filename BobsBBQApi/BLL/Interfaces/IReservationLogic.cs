namespace BobsBBQApi.BLL.Interfaces;

public interface IReservationLogic
{
    void ReserveTable(DateTime reservationDate, int timeSlot, int partySize, string note, Guid userId);

    List<DateTime> GetAvailableTimeSlot(DateTime date, int partySize);
    

}