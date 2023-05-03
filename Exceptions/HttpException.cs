namespace TestWeb.Exceptions
{
    public class HttpException : Exception
    {
        public int StatusCode { get; set; }
        public HttpException(int statusCode, string message) : base() 
        {
            statusCode = StatusCode;
        }
    }
}
