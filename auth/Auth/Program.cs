using Chat.Auth;
using Chat.Auth.Data;
using Chat.Auth.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using FastConfig;
using static OpenIddict.Abstractions.OpenIddictConstants;

const string appId = "auth";
var fastConfig = FastConfigClient.FromEnvironment(appId: appId);
var config = await fastConfig.Get<Config>() ?? throw new ArgumentNullException();

var builder = WebApplication.CreateBuilder(args);

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
  options.UseNpgsql(config.Database);
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
    // for prod we should use assymetric key (or even cert) with storage in
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


// Add services to the container.
// builder.Services.AddRazorPages();
// builder.Services.AddControllersWithViews();


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

// app.UseEndpoints(endpoints =>
// {
//   endpoints.MapRazorPages();
//   endpoints.MapControllers();
// });

app.Run(config.Urls);
