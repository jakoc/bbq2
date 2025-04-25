namespace BobsBBQApi.Helpers.Interfaces;

public interface IEmail
{
    Task SendSuccessfullAccountCreationEmail(string toEmail, string firstName);
    Task SendSuccessfullTableReservationEmail(string toEmail, string firstName, 
        DateTime reservationDate, DateTime reservationTime);
}