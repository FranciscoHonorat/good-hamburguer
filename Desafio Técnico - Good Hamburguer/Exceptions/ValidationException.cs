namespace Desafio_Técnico___Good_Hamburguer.Exceptions;

public sealed class ValidationException(string message) : AppException(message, StatusCodes.Status400BadRequest);
