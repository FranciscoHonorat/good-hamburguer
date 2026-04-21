namespace Desafio_Técnico___Good_Hamburguer.Exceptions;

public sealed class NotFoundException(string message) : AppException(message, StatusCodes.Status404NotFound);
