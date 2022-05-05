using ContentStore;
using FastConfig;

const string appId = "contentstore";
var fastConfig = FastConfigClient.FromEnvironment(appId: appId);
var config = await fastConfig.Get<Config>() ?? throw new ArgumentNullException();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRabbitHub(hub =>
  hub
  .Connect(config.Connection)
  .UseDefaultConsumer(cons =>
    cons
    .Queue(config.Queue)
    .DeclareQueue()
    .BindTopics()));

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run(config.Urls);
