namespace App.Error;

public class DomainProblemException : Exception
{
  public DomainProblemException(IDomainProblem problem)
  {
    this.Problem = problem;
  }

  public IDomainProblem Problem { get; set; }
}
