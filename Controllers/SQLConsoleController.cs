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

    [HttpGet("getColumnNames")]
    public ActionResult getColumnNames(string DBConnectionName,string tableName)
    {
        List<string> columnNames = new List<string>();
        string key=$"ConnectionStrings:{DBConnectionName}";
        string? connectionString = this._configuration[key];
        SqlConnection sqlConnection = new SqlConnection(connectionString);
        sqlConnection.Open();
        String[] tableRestrictions = new String[4];
        tableRestrictions[2]=tableName;
        DataTable schemaColumns = sqlConnection.GetSchema("Columns",tableRestrictions);        
        foreach (DataRow rowColumn in schemaColumns.Rows)
        {
            string c0 = (string)rowColumn["TABLE_CATALOG"];
            string c1 = (string)rowColumn["TABLE_SCHEMA"];
            string c2 = (string)rowColumn["TABLE_NAME"];
            string c3 = (string)rowColumn["COLUMN_NAME"];           
            string c4 = (string)rowColumn["DATA_TYPE"];
            columnNames.Add($"{c0}.{c1}.{c2}.{c3} {c4}");
        }                    
        sqlConnection.Close();
        return Ok(columnNames);
    }    

    [HttpGet("getIndexColumns")]
    public ActionResult getIndexColumns(string DBConnectionName,string tableName)
    {
        List<string> columnNames = new List<string>();
        string key=$"ConnectionStrings:{DBConnectionName}";
        string? connectionString = this._configuration[key];
        SqlConnection sqlConnection = new SqlConnection(connectionString);
        sqlConnection.Open();
        String[] tableRestrictions = new String[4];
        tableRestrictions[2]=tableName;
        DataTable schemaColumns = sqlConnection.GetSchema("IndexColumns",tableRestrictions);        
        foreach (DataRow rowColumn in schemaColumns.Rows)
        {
            string c0 = (string)rowColumn["table_schema"];
            string c1 = (string)rowColumn["table_name"];
            string c2 = (string)rowColumn["column_name"];            
            string c3 = Convert.ToString(rowColumn["constraint_schema"])??"";
            string c4 = Convert.ToString(rowColumn["constraint_name"])??"";            
            string c5 = Convert.ToString(rowColumn["KeyType"])??"";

            columnNames.Add($"{c0}.{c1}.{c2} {c3}.{c4} {c5}");
        }                    
        sqlConnection.Close();
        return Ok(columnNames);
    }  


}