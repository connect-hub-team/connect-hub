using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chat.Auth.Data;

public class AuthDbContext : IdentityDbContext<User>
{
  public AuthDbContext(DbContextOptions<AuthDbContext> options)
    : base(options)
  { }
}