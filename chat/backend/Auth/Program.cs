using Chat.Auth.Data;
using Microsoft.EntityFrameworkCore;
using Quartz;

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


// persistent storage
builder.Services.AddDbContext<AuthDbContext>(options =>
{
  options.UseNpgsql("Server=127.0.0.1;Port=5432;Database=auth;User Id=auth;Password=password123;");
  options.UseOpenIddict();
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
      .EnableAuthorizationEndpointPassthrough();
  })
  .AddValidation(options =>
  {
    options.UseLocalServer();
    //options.UseAspNetCore(); ДА ЕБАНЫЙ В РОТ
  });

builder.Services.AddAuthorization();


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
/*builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();*/

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//   app.UseSwagger();
//   app.UseSwaggerUI();
// }

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
