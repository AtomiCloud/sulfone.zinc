using System.Net;
using App.Error.V1;
using App.Modules.Common;
using App.Modules.Users.Data;
using App.StartUp.Database;
using App.StartUp.Registry;
using App.Utility;
using Asp.Versioning;
using CSharp_Result;
using Kirinnee.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Implementations;
using Index = Meilisearch.Index;

namespace App.Modules.Users.API.V1;

/// <summary>
/// V1 controller
/// </summary>
[ApiVersion(1.0)]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class UserController : AtomiControllerBase

{
  private readonly ILogger<UserController> _logger;
  private readonly MainDbContext _dbContext;
  private readonly CreateUserReqValidator _userReqValidator;
  private readonly IFileRepository _fileRepository;
  private readonly IRedisClientFactory _factory;


  public UserController(ILogger<UserController> logger, MainDbContext dbContext,
    CreateUserReqValidator userReqValidator, IFileRepository fileRepository, IRedisClientFactory redis)
  {
    this._logger = logger;
    this._dbContext = dbContext;
    this._userReqValidator = userReqValidator;
    this._fileRepository = fileRepository;
    this._factory = redis;
  }

  [Authorize(Policy = AuthPolicies.OnlyAdmin), HttpGet("cache-main/{id}")]
  public async Task<object?> Cache(string id)
  {
    return await this._factory.GetRedisClient(Caches.Main).Db0.GetAsync<object>(id);
  }

  [HttpPost("cache-main/{id}/{value}")]
  public async Task<bool?> Cache(string id, string value)
  {
    return await this._factory.GetRedisClient(Caches.Main).Db0.AddAsync(id, new { Name = value });
  }

  [HttpGet("cache-alt/{id}")]
  public async Task<object?> CacheAlt(string id)
  {
    return await this._factory.GetRedisClient(Caches.Alt).Db0.GetAsync<object>(id);
  }

  [HttpPost("cache-alt/{id}/{value}")]
  public async Task<bool> CacheAlt(string id, string value)
  {
    return await this._factory.GetRedisClient(Caches.Alt).Db0.AddAsync(id, new { Name = value });
  }


  [HttpPost]
  public async Task<ActionResult<UserResp>> Post([FromBody] CreateUserReq req)
  {
    this._logger.LogInformation("Creating User");
    var a = await this._userReqValidator.ValidateAsync(req);
    if (!a.IsValid)
    {
      return this.Error<UserResp>(HttpStatusCode.NotFound,
        new ValidationError("Create User Request is invalid", a.ToDictionary())
      );
    }

    var user = new UserData
    {
      Id = Guid.NewGuid(),
      Name = req.Name,
      Email = req.Email,
      Age = 5
    };

    var u = await this._dbContext.Users.AddAsync(user);
    await this._dbContext.SaveChangesAsync();
    var updated = u.Entity;

    return this.Ok(new UserResp(updated.Id, updated.Name, updated.Email));
  }

  [HttpGet("{id:guid}")]
  public UserResp Get(Guid id)
  {
    this._logger.LogInformation("Getting User");
    return this._dbContext.Users
      .Where(x => x.Id == id)
      .Select(x => new UserResp(x.Id, x.Name, x.Email))
      .First();
  }

  [HttpPost("upload")]
  public async Task<ActionResult<string[]>> Upload(List<IFormFile> files)
  {
    var a = files.Select(async x =>
    {
      if (x.Length <= 0) return Result.ToResult<string?>(null);
      using var memStream = new MemoryStream();
      await x.CopyToAsync(memStream);
      this._logger.LogInformation("Stream Size: {StreamSize}", memStream.Length);
      return await this
        ._fileRepository
        .Save(BlockStorages.Main, "sample", Guid.NewGuid().ToString(), memStream, true)
        .ThenAwait(key =>
          key == null
            ? Task.FromResult(Result.ToResult<string?>(null))
            : this._fileRepository.Link(BlockStorages.Main, key));
    });

    var blobResult = await Task.WhenAll(a);
    var links = blobResult.ToResultOfSeq();

    if (links.IsSuccess())
    {
      return this.Ok(links.SuccessOrDefault());
    }

    return this.BadRequest(links.FailureOrDefault());
  }
}
