public class DtoDataTable
{
    public string TableName {get; set;} = string.Empty;
    public List<DtoDataColumn> dataColumns {get; set;} = new List<DtoDataColumn>();
    public List<DtoDataRow> dataRows {get; set;} = new List<DtoDataRow>();
}