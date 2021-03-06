using System.Text.Json;
using Chat.Auth;
using Chat.Auth.Data;
using Chat.Auth.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FastConfig;
using Quartz;
using static OpenIddict.Abstractions.OpenIddictConstants;

const string appId = "auth";

var serializerOptions = new JsonSerializerOptions
{
  PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
  Converters =
  {
    new CustomHashSetConverter<string>((str) => str),
    new CustomHashSetConverter<Uri>((str) => new Uri(str)),
  },
};
var fastConfig = FastConfigClient.FromEnvironment(
  appId: appId,
  serializerOptions: serializerOptions);
var config = await fastConfig.Get<Config>();

if (config is null)
  throw new ApplicationException("Can't fetch config from FastConfig!");

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddFastConfig(fastConfig);


builder.Services.AddFastConfig(fastConfig);

// adding & configure quartz scheduled tasks
builder.Services.AddQuartz(options =>
{
  options.UseMicrosoftDependencyInjectionJobFactory();
  options.UseSimpleTypeLoader();
  options.UseInMemoryStore();
});

// hosted services
builder.Services.AddQuartzHostedService(options =>
  options.WaitForJobsToComplete = true);
builder.Services.AddHostedService<Worker>();

builder.Services.AddCors();


// persistent storage
builder.Services.AddDbContext<AuthDbContext>(options =>
{
  // TODO: fuck this shit
  options.UseNpgsql(config.ConnectionString);
  options.UseOpenIddict();
});

// add Identity
builder.Services.AddDefaultIdentity<User>()
  .AddEntityFrameworkStores<AuthDbContext>()
  .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
  options.ClaimsIdentity.UserNameClaimType = Claims.Name;
  options.ClaimsIdentity.EmailClaimType = Claims.Email;
  options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
});

builder.Services.AddEmailSender();

// configure OpenIddict
builder.Services.AddOpenIddict()
  .AddCore(options
    => options
      .UseEntityFrameworkCore()
      .UseDbContext<AuthDbContext>()
  )
  .AddServer(options =>
  {
    options
      .SetAuthorizationEndpointUris("/connect/auth")
      .SetTokenEndpointUris("/connect/token");

    // use Authorization code flow
    // see: https://developer.okta.com/docs/concepts/oauth-openid/#authorization-code-flow
    // in future we could use other flows for auth purposes
    options
      .AllowAuthorizationCodeFlow()
      .AllowRefreshTokenFlow();

    // for tests here using a symmetric and hardcoded key
    // for prod we should use asymetric key (or even cert) with storage in
    // FastConfig

    options.AddEncryptionKey(new SymmetricSecurityKey(
      Convert.FromBase64String(config.SymmetricSecurityKey)));

    //options.AddEphemeralEncryptionKey();

    options.AddDevelopmentSigningCertificate();

    options
      .UseAspNetCore()
      .EnableAuthorizationEndpointPassthrough()
      .EnableStatusCodePagesIntegration()
      .EnableTokenEndpointPassthrough()
      .EnableLogoutEndpointPassthrough();
  })
  .AddValidation(options =>
  {
    options.UseLocalServer();
    options.UseAspNetCore();
  });

builder.Services.AddMvc(options => options.EnableEndpointRouting = false);

var app = builder.Build();

app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseRouting();
app.UseCors(builder
  => builder
    .AllowAnyHeader()
    .AllowAnyMethod()
    .SetIsOriginAllowed((host) => true)
    .AllowCredentials()
);

app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
  name: "default",
  pattern: "{controller=Account}/{action=Index}/{id?}"
);
app.UseMvc();

app.Run(config.Urls);
