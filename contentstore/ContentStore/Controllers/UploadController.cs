using Microsoft.AspNetCore.Mvc;

namespace ContentStore.Controllers;

[ApiController]
[Route("file/upload")]
public class UploadController : ControllerBase
{
  [HttpPost(Name = "PostFile")]
  public async Task<IActionResult> Post(List<IFormFile> files)
  {
    foreach (var formFile in Request.Form.Files)
    {
      if (formFile.Length == 0)
        continue;

      var filePath = Path.GetTempFileName();

      using (var stream = System.IO.File.Create(filePath))
      {
        await formFile.CopyToAsync(stream);
      }
    }

    return Ok();
  }
}