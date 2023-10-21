namespace App.Modules.Users.API.V1;

public record CreateTokenReq(string Name);

public record UpdateTokenReq(string Name);

public record TokenPrincipalResp(Guid Id, string Name);

/**
 * One Time Token, only first time on creation
 */
public record TokenOTPrincipalResp(Guid Id, string Name, string ApiKey);

/**
 * One time Token with owner, only first time on creation
 */
public record TokenOTResp(TokenOTPrincipalResp Token, UserPrincipalResp Owner);

public record TokenResp(TokenPrincipalResp Token, UserPrincipalResp Owner);
