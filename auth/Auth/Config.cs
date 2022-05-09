using OpenIddict.Abstractions;

namespace Chat.Auth;

public class Config
{
  public string Urls { get; set; }
  public string ConnectionString { get; set; }
  public string SymmetricSecurityKey { get; set; }
  public IEnumerable<CustomApplicationDescriptor> Clients { get; set; }
  public IEnumerable<CustomScopeDescriptor> Scopes { get; set; }

}

public class CustomScopeDescriptor : OpenIddictScopeDescriptor
{
  public new HashSet<string> Resources { get; init; }
}

public class CustomApplicationDescriptor : OpenIddictApplicationDescriptor
{
  public new HashSet<string> Permissions { get; init; }
  public new HashSet<Uri> RedirectUris { get; init; }
}