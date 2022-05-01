using Chat.Auth.Data;
using Chat.Auth.Data.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Auth.Controllers;

// [Authorize]
[Route("account")]
public class AccountController : Controller
{
  private readonly AuthDbContext _context;
  private readonly SignInManager<User> _signInManager;

  public AccountController(AuthDbContext context,
    SignInManager<User> signInManager)
  {
    _context = context;
    _signInManager = signInManager;
  }

  [AllowAnonymous]
  [HttpGet]
  [Route("register")]
  public ViewResult Register() => View();
  

  [HttpPost]
  [Route("register")]
  public async Task Register(RegisterDto model)
  {
    // TODO: data validation and email confirm
    var user = new User()
    {
      Email = model.Email,
      UserName = model.UserName,
      PasswordHash = model.Password,
      EmailConfirmed = false,
    };

    await _context.Users.AddAsync(user);
    await _context.SaveChangesAsync();
  }

  [Route("edit")]
  [HttpGet]
  public ViewResult Edit() => View();

  [Route("edit")]
  [HttpPost]
  public async Task Edit(User user)
  {
    // TODO: user validation

    _context.Update(user);
    await _context.SaveChangesAsync();
  }
}