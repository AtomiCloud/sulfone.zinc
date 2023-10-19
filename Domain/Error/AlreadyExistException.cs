using EntityFramework.Exceptions.Common;

namespace Domain.Error;

public class AlreadyExistException : Exception
{
  public readonly Type t;

  public AlreadyExistException(string? message, UniqueConstraintException e, Type t) : base(message, e)
  {
    this.t = t;
  }
}
