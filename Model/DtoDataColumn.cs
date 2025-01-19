using Microsoft.EntityFrameworkCore.Metadata.Internal;

public class DtoDataColumn
{
    public string ColumnName {get; set;} = string.Empty;
    public string DataType {get; set;} = string.Empty;
    public string Key {get; set;} = string.Empty;
    
    public int Maxlength {get; set;} = 0;


}