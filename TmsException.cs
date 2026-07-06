public class TmsException : Exception
{
    public TmsException(string message)
    : base(message)
    {
    }
    public TmsException(string message, Exception inner)
    : base(message, inner)
    {
    }
}