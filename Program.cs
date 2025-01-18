using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        #region Swagger/OpenAPI
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SQL Console Web API",
                    Description = "SQL Console Web API Authorize with JWT Token",
                    Version = "1.0.0"
                });


                
                //var xmlFilename = System.IO.Path.Combine(System.AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
                //option.IncludeXmlComments(xmlFilename);

                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type=ReferenceType.SecurityScheme,
                            Id="Bearer"
                        }
                    },
                    new string[]{}
                }
                });
                
            });

        #endregion





        #region  JWT Bearer

        using var loggerFactory = LoggerFactory.Create(b => b.SetMinimumLevel(LogLevel.Trace).AddConsole());

        Task LogAttempt(IHeaderDictionary headers, string eventType)
        {

            var logger = loggerFactory.CreateLogger<Program>();

            var authorizationHeader = headers["Authorization"].FirstOrDefault();

            if (authorizationHeader is null)
                logger.LogInformation($"{eventType}. JWT not present");
            else
            {
                string jwtString = authorizationHeader.Substring("Bearer ".Length);

                var jwt = new JwtSecurityToken(jwtString);

                string SystemTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", Time.TH);
                string ValidTo = jwt.ValidTo.ToString("yyyy-MM-dd HH:mm:ss", Time.TH);


                //logger.LogInformation($"{eventType}.");
                //logger.LogInformation($"jwt.ValidTo:{ValidTo} System time:{SystemTime}");

            }

            return Task.CompletedTask;
        }

        var secret = builder.Configuration["JWT:Secret"] ?? throw new InvalidOperationException("Secret not configured");


        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                ValidAudience = builder.Configuration["JWT:ValidAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ClockSkew = new TimeSpan(0, 0, 5)
            };
            options.Events = new JwtBearerEvents
            {
                OnChallenge = ctx => LogAttempt(ctx.Request.Headers, "OnChallenge"),
                OnTokenValidated = ctx => LogAttempt(ctx.Request.Headers, "OnTokenValidated")
            };
        });
        #endregion

        #region Policy
        const string policy = "defaultPolicy";

        var AllowedHosts = builder.Configuration["AllowedHosts"];

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(policy,
                            p =>
                            {
                                p.WithOrigins("*");
                                p.AllowAnyHeader();
                                p.AllowAnyMethod();
                                p.AllowAnyOrigin();
                            });
        });
        #endregion

        #region ตั้งค่า Default File
        var options = new DefaultFilesOptions();
        options.DefaultFileNames.Clear();
        options.DefaultFileNames.Add("index.html");
        #endregion

        #region AddProblemDetails
        builder.Services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = (context) =>
            {
                if (!context.ProblemDetails.Extensions.ContainsKey("traceId"))
                {
                    string? traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
                    context.ProblemDetails.Extensions.Add(new KeyValuePair<string, object?>("traceId", traceId));
                }
            }
        );
        #endregion


        var app = builder.Build();


        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Write streamlined request completion events, instead of the more verbose ones from the framework.
        // To use the default framework request logging instead, remove this line and set the "Microsoft"
        // level in appsettings.json to "Information".
        //app.UseSerilogRequestLogging();
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors(policy);
        app.UseDefaultFiles(options);
        app.UseStaticFiles();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
