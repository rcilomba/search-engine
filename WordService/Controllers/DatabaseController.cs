using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WordService;

[ApiController]
[Route("[controller]")]
public class DatabaseController : ControllerBase
{
    private Database database = Database.GetInstance();

    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        await Task.Run(() => database.DeleteDatabase());
        return NoContent(); // Returnera en 204 No Content response
    }

    [HttpPost]
    public async Task<IActionResult> Post()
    {
        await Task.Run(() => database.RecreateDatabase());
        return CreatedAtAction(nameof(Post), null); // Returnera en 201 Created response
    }
}
