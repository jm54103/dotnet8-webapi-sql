using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

public class AppControllerBase : ControllerBase
{
    protected ILogger<Object>? appLogger {get;set;}
    public AppControllerBase(){}

    
    protected string getClaimUserId(){
        var userContext = HttpContext.User;
        var claim = userContext.Claims.Where(c => c.Type == ClaimTypes.Name).FirstOrDefault();
        if (claim == null)
        {
            return "";
        }
        else
        {
            var value = claim.Value;
            return value;
        }
    }
    protected string getCliamUserCode()
    {

        IEnumerable<Claim> Claims = HttpContext.User.Claims;

        /*
        string? authenticationType = HttpContext.User.Identity?.AuthenticationType;
        foreach(var Claim in Claims){
            _logger.LogCritical($"{Claim.Subject}:{Claim.Type}:{Claim.Value}");
        }
        */

        var userContext = HttpContext.User;
        var claim = userContext.Claims.Where(c => c.Type == ClaimTypes.Name).FirstOrDefault();
        if (claim == null)
        {
            //this.appLogger.LogError($"cliam is null");
            return "";
        }
        else
        {
            var value = claim.Value.Trim();
            //this.appLogger.LogError($"{claim.Subject}|{claim.Type}|{claim.Value}|");
            return value.Length <= 5 ? value : value.Substring(value.Length - 5);
        }

    }


    protected void printStackTrace(string functionName,string TraceId,Exception ex){
        if(appLogger==null){
            throw new Exception("appLogger is null!");
        }else{
            appLogger.LogError("function:{0}\r\nTraceId:{1}\r\nMessage:{2}\r\nSource\r\n{3}\r\nStackTrace\r\n{3}"
                                ,functionName
                                ,TraceId
                                ,ex.Message
                                ,ex.Source
                                ,ex.StackTrace);
        }
    }
}