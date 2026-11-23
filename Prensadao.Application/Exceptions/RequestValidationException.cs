namespace Prensadao.Application.Exceptions;

public sealed class RequestValidationException(string message) : ArgumentException(message);
