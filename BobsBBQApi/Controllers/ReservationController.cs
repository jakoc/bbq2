using BobsBBQApi.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BobsBBQApi.Controllers;

public class ReservationController : Controller
{
   private readonly IReservationLogic _reservationLogic;
   public ReservationController(IReservationLogic reservationLogic)
   {
       _reservationLogic = reservationLogic;
   }
   [HttpPost("[action]")]
    public IActionResult ReserveTable(DateTime reservationDate, int timeSlot, int partySize, string note, Guid userId)
    {
         if (reservationDate == default || timeSlot == default || partySize <= 0 || partySize > 10 || string.IsNullOrWhiteSpace(note) || userId == Guid.Empty )
         {
              return BadRequest("All fields are required.");
         }
    
         try
         {
              _reservationLogic.ReserveTable(reservationDate, timeSlot, partySize, note, userId );
              return Ok("Reservation created successfully.");
         }
         catch (Exception ex)
         {
              return StatusCode(500, "Error creating reservation: " + ex.Message);
         }
    }
    
    [HttpPost("[action]")]
    public IActionResult GetAvailableTimeSlots(DateTime date, int partySize)
    {
        if (partySize <= 0 || partySize > 10)
        {
            return BadRequest("Party size must be between 1 and 10.");
        }
        if (date == default)
        {
            return BadRequest("Date is required.");
        }

        try
        {
            var availableTimeSlots = _reservationLogic.GetAvailableTimeSlot(date, partySize);
            return Ok(availableTimeSlots);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error retrieving available time slots: " + ex.Message);
        }
    }


   
}