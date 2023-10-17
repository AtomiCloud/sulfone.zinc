namespace App.Modules.Users.API.V1;

public record CreateUserReq(string Name, string Email);

public record UserResp(Guid Id, string Name, string Email);
