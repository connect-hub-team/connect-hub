using System.ComponentModel.DataAnnotations;

namespace Chat.Auth.Data.Dtos;

public class AuthorizeDto
{
  [Display(Name = "Application")]
  public string ApplicationName { get; set; }

  [Display(Name = "Scope")]
  public string Scope { get; set; }
}