using Chat.Auth;
using Chat.Auth.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quartz;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

// adding & configure quartz scheduled tasks
builder.Services.AddQuartz(options =>
{
  options.UseMicrosoftDependencyInjectionJobFactory();
  options.UseSimpleTypeLoader();
  options.UseInMemoryStore();
});

builder.Services.AddQuartzHostedService(options =>
  options.WaitForJobsToComplete = true);
builder.Services.AddHostedService<Worker>();


// persistent storage
builder.Services.AddDbContext<AuthDbContext>(options =>
{
  // TODO: fuck this shit
  options.UseNpgsql("Server=127.0.0.1;Port=5432;Database=authdb;User Id=auth;Password=password123;"); 
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
});

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
      .SetAuthorizationEndpointUris("/auth")
      .SetTokenEndpointUris("/token");

    // use Authorization code flow
    // see: https://developer.okta.com/docs/concepts/oauth-openid/#authorization-code-flow
    // in future we could use other flows for auth purposes
    options.AllowAuthorizationCodeFlow();

    // for tests here using a symmetric and hardcoded key
    // for prod we should use assymetric key (or even cert) with storage in
    // FastConfig
    options.AddEphemeralEncryptionKey();

    options.AddDevelopmentSigningCertificate();

    options
      .UseAspNetCore()
      .EnableAuthorizationEndpointPassthrough()
      .EnableTokenEndpointPassthrough()
      .EnableLogoutEndpointPassthrough();
  })
  .AddValidation(options =>
  {
    options.UseLocalServer();
    //options.UseAspNetCore(); // ДА ЕБАНЫЙ В РОТ
  });

builder.Services.AddAuthorization();
builder.Services.AddAuthentication();
builder.Services.AddMvc();


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();


var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();
app.UseAuthentication();

app.UseEndpoints(endpoints =>
{
  endpoints.MapRazorPages();
  endpoints.MapControllers();
});

app.Run();
