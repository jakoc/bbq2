using BobsBBQApi.BE;

namespace BobsBBQApi.DAL.Repositories.Interfaces;

public interface IReservationRepository
{
    public List<DateTime> GetReservedSlots(DateTime date);
    void ReserveTable(Reservation reservation);
}