using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using WordService;

[ApiController]
[Route("[controller]")]
public class DatabaseController : ControllerBase
{
    private Database database = Database.GetInstance();

    [HttpDelete]
    public void Delete()
    {
        database.DeleteDatabase();
    }

    [HttpPost]
    public void Post()
    {
        database.RecreateDatabase();
    }
}