namespace ninjaTax.Models.Services;

/// <summary>
/// Domain business exception for Vietnamese accounting operations.
/// Conveys user-facing, actionable error messages with optional field targeting.
/// </summary>
public class BusinessException : Exception
{
    public string UserMessage { get; }
    public string? FieldName { get; }

    public BusinessException(string userMessage, string? fieldName = null)
        : base(userMessage)
    {
        UserMessage = userMessage;
        FieldName = fieldName;
    }
}
