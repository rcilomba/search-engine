using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using WordService;



[ApiController]
[Route("[controller]")]
public class OccurrenceController : ControllerBase
{
    private Database database = Database.GetInstance();

    [HttpPost]
    public void Post(int docId, [FromBody] ISet<int> wordIds)
    {
        database.InsertAllOcc(docId, wordIds);
    }
}