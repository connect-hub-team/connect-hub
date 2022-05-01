using Chat.Auth.Services;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Chat.Auth.Helpers;

public static class EmailSenderIOC
{
  public static IServiceCollection AddEmailSender(this IServiceCollection services)
  {
    var conf = ""; // TODO: reading config
    return services.AddScoped<IEmailSender>(sp
      => new EmailSender("", 587, true, "", ""));
  }
}