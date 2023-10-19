namespace App.Modules.Users.API.V1;

public record CreateUserReq(string Username);

public record UpdateUserReq(string Username);

public record SearchUserQuery(string? Id, string? Sub, string? Username, int? Limit, int? Skip);

public record UserResp(Guid Id, string Sub, string Username);
