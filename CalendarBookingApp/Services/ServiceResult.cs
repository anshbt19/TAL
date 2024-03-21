namespace CalendarBookingApp.Services
{
    public class ServiceResult
    {
        public bool IsSuccess { get; private set; }
        public string Message { get; private set; }

        public ServiceResult(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        public static ServiceResult Success(string message) => new(true, message);
        public static ServiceResult Failure(string message) => new(false, message);
    }
}
