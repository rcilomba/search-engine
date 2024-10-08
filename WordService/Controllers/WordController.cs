using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using WordService;
using Serilog; // Lägg till Serilog

[ApiController]
[Route("[controller]")]
public class WordController : ControllerBase
{
    private Database database = Database.GetInstance();

    // GET: /Word
    [HttpGet]
    public Dictionary<string, int> Get()
    {
        // Logga att en GET-förfrågan mottar
        Log.Information("GET request received to fetch all words.");

        var result = database.GetAllWords();

        // Logga att GET-förfrågan behandlas
        Log.Information("GET request completed. Returned {Count} words.", result.Count);

        return result;
    }

    // POST: /Word
    [HttpPost]
    public IActionResult Post([FromBody] Dictionary<string, int> res)
    {
        // Logga att en POST-förfrågan mottar
        Log.Information("POST request received to insert words.");

        try
        {
            database.InsertAllWords(res);
            // Logga att POST-operationen lyckas
            Log.Information("POST request succeeded. Inserted {Count} words.", res.Count);

            return Ok();
        }
        catch (SqlException ex)
        {
            // Logga om ett fel inträffar
            Log.Error(ex, "Error occurred while inserting words.");
            return StatusCode(500, "Internal server error while inserting words.");
        }
    }
}
