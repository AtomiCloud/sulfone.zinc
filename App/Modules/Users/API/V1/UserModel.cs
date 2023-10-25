using Domain.Model;

namespace App.Modules.Users.API.V1;

public record CreateUserReq(string Username);

public record UpdateUserReq(string Username);

public record SearchUserQuery(string? Id, string? Username, int? Limit, int? Skip);

public record UserExistResp(bool Exists);

public record UserPrincipalResp(string Id, string Username);

public record UserResp(UserPrincipalResp Principal, IEnumerable<TokenPrincipalResp> Tokens);
