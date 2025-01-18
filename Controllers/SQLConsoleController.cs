using System.Configuration;
using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

//[Authorize]
[ApiController]
[Route("[controller]")]
public class SQLConsoleController : AppControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SQLConsoleController> _logger;

    public SQLConsoleController(IConfiguration configuration, ILogger<SQLConsoleController> logger){
        this._configuration = configuration; 
        this._logger = logger;
         /*-- printStackTrace--*/
        this.appLogger=_logger;
        /*-- printStackTrace--*/
    }

    [HttpGet("getDBConnectionNames")]
    public ActionResult getDBConnectionNames()
    {           
        var config=this._configuration.AsEnumerable();
        var DBConnectionNames = config.Where(c=>c.Key.Contains("ConnectionStrings:"));                
        return Ok(DBConnectionNames);
    }

    [HttpGet("getTableNames")]
    public ActionResult getTableNames(string DBConnectionName)
    {
        List<string> tableNames = new List<string>();
        string key=$"ConnectionStrings:{DBConnectionName}";
        string? connectionString = this._configuration[key];
        SqlConnection sqlConnection = new SqlConnection(connectionString);
        sqlConnection.Open();
        var dt = sqlConnection.GetSchema("Tables");    
        // Display Data
        foreach (DataRow row in dt.Rows) {
          string tableName=(string)row["TABLE_NAME"];
          tableNames.Add(tableName);
        }
        sqlConnection.Close();
        return Ok(tableNames);
    }


}