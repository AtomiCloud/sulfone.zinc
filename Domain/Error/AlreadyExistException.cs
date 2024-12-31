using EntityFramework.Exceptions.Common;

namespace Domain.Error;

public class AlreadyExistException(string? message, UniqueConstraintException e, Type t) : Exception(message, e)
{
  public readonly Type t = t;
}
