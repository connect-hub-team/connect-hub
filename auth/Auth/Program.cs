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

// hosted services
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
  options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
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
    options.AddEphemeralEncryptionKey();

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
    options.UseAspNetCore(); // ДА ЕБАНЫЙ В РОТ
  });

builder.Services.AddMvc(options => options.EnableEndpointRouting = false);


// Add services to the container.
// builder.Services.AddRazorPages();
// builder.Services.AddControllersWithViews();


var app = builder.Build();

app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
// app.MapControllerRoute(
//   name: "default",
//   pattern: "{controller}/{action=Index}/{id?}"
// );
app.UseMvc();

// app.UseEndpoints(endpoints =>
// {
//   endpoints.MapRazorPages();
//   endpoints.MapControllers();
// });

app.Run();
