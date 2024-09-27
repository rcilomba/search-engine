using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using WordService;


[ApiController]
[Route("[controller]")]
public class WordController : ControllerBase
{
    private Database database = Database.GetInstance();

    [HttpGet]
    public Dictionary<string, int> Get()
    {
        return database.GetAllWords();
    }

    [HttpPost]
    public void Post([FromBody] Dictionary<string, int> res)
    {
        database.InsertAllWords(res);
    }
}