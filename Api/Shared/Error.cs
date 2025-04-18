namespace Api.Shared;

public class Error
{
    private string _erorContext { get; set;}
    private string _errorMessage { get; set; }

    public Error(string ErrorContext, string ErrorMessage)
    {
        _erorContext = ErrorContext;
        _errorMessage = ErrorMessage;
    } 
}