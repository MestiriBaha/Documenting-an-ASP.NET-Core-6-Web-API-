using Library.API.Authentication;
using Library.API.Contexts;
using Library.API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(configure =>
{
    configure.ReturnHttpNotAcceptable = true;
    configure.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status400BadRequest));
    configure.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status406NotAcceptable));
    configure.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status500InternalServerError));
    configure.Filters.Add(new AuthorizeFilter()); 
}).AddNewtonsoftJson(setupAction =>
{
    setupAction.SerializerSettings.ContractResolver =
       new CamelCasePropertyNamesContractResolver();
});

// configure the NewtonsoftJsonOutputFormatter
builder.Services.Configure<MvcOptions>(configureOptions => 
{
    var jsonOutputFormatter = configureOptions.OutputFormatters
        .OfType<NewtonsoftJsonOutputFormatter>().FirstOrDefault();
    if (jsonOutputFormatter != null)
    {
        // remove text/json as it isn't the approved media type
        // for working with JSON at API level
        if (jsonOutputFormatter.SupportedMediaTypes.Contains("text/json"))
        {
            jsonOutputFormatter.SupportedMediaTypes.Remove("text/json");
        }
    }
}); 

builder.Services.AddDbContext<LibraryContext>(
    dbContextOptions => dbContextOptions.UseSqlite(
        builder.Configuration["ConnectionStrings:LibraryDBConnectionString"]));

builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddApiVersioning(
    setupAction =>
    {
        setupAction.AssumeDefaultVersionWhenUnspecified = true;
        setupAction.DefaultApiVersion = new ApiVersion(1,0); 
        setupAction.ReportApiVersions = true;
        //setupAction.ApiVersionReader = new MediaTypeApiVersionReader("V"); 
        }

     );
builder.Services.AddVersionedApiExplorer(setupAction => {
    setupAction.GroupNameFormat = "'v'VV"; 
   // setupAction.SubstituteApiVersionInUrl = true;   
});
var apiVersionDescriptionProvider = builder.Services.BuildServiceProvider().GetService<IApiVersionDescriptionProvider>();

builder.Services.AddSwaggerGen(
    setupAction =>
    {
        setupAction.AddSecurityDefinition("BasicAuth", new()
        {
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "basic",
            Description = "Input Username and Password for accessing this Api "

        });
        setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "BasicAuth"
                }
            } , new List<string>()
            }
            
        }) ;
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            setupAction.SwaggerDoc($"LibraryOpenApiSpecification{description.GroupName}", new()
            {
                Title = "Library Management API",
                Version = description.ApiVersion.ToString(),
                Description = "Official Documentation of the LibraryAPI   ",
                Contact = new()
                {
                    Name = "Mohamed baha mestiri"
                },
                License = new()
                {
                    Name = "MIT License"
                }

            });
        
        setupAction.DocInclusionPredicate((documentName, apiDescription)
           =>
        {
            var actionApiVersionModel = apiDescription.ActionDescriptor
               .GetApiVersionModel(
                    ApiVersionMapping.Explicit | ApiVersionMapping.Implicit);

            if (actionApiVersionModel == null)
            {
                return true;
            }

            if (actionApiVersionModel.DeclaredApiVersions.Any())
            {
                return actionApiVersionModel.DeclaredApiVersions.Any(v =>
                $"LibraryOpenApiSpecification{v}" == documentName);
            }
            return actionApiVersionModel.ImplementedApiVersions.Any(v =>
                $"LibraryOpenApiSpecification{v}" == documentName);
        });
        //setupAction.DocInclusionPredicate((documentName, apiDescription) =>
        // {
        /*  var actionApiVersionModel = apiDescription.ActionDescriptor.GetApiVersionModel(ApiVersionMapping.Explicit | ApiVersionMapping.Implicit);
             if (actionApiVersionModel == null)
             {
                 return true;
             }
             if (actionApiVersionModel.DeclaredApiVersions.Any())
             {
                 return actionApiVersionModel.DeclaredApiVersions.Any(v =>
                   $"LibraryOpenApiSpecification{v}" == documentName);
             }
             return actionApiVersionModel.ImplementedApiVersions.Any(implemented => $"LibraryOpenApiSpecification{implemented}" == documentName);
         });  */



        //setupAction.SwaggerDoc("Books", new()
        //{
        //    Title = "LibraryAPI(Books)",
        //    Version = "1.0.0",
        //    Description = "Official Documentation of the LibraryAPI Books Controller ",
        //    Contact= new ()
        //    {
        //        Name = "Mohamed baha mestiri"
        //    },
        //    License = new()
        //    {
        //        Name = "MIT License"
        //    }

        //});
        //setupAction.SwaggerDoc("Authors", new()
        //{
        //    Title = "LibraryAPI(Author)",
        //    Version = "1.0.0",
        //    Description = "Official Documentation of the LibraryAPI Authors Controller ",
        //    License = new()
        //    {
        //        Name = "MIT License"
        //    }

        //});
    }
        setupAction.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
        var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);

        setupAction.IncludeXmlComments(xmlCommentsFullPath);
    }
    );
builder.Services.AddAuthentication("Basic").AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic",null); 
var app = builder.Build();
app.UseSwagger();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwaggerUI(
        setupAction => {
            foreach(var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                setupAction.SwaggerEndpoint($"/swagger/LibraryOpenApiSpecification{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant()); 
            }
            //setupAction.SwaggerEndpoint("/swagger/LibraryAPI/swagger.json", "LibraryApiVersion1");
            //setupAction.SwaggerEndpoint("/swagger/Books/swagger.json", "Books");
            //setupAction.SwaggerEndpoint("/swagger/Authors/swagger.json", "Authors");

            setupAction.RoutePrefix = String.Empty;
        }
   );
}


// Configure the HTTP request pipeline.
//After the web Application has been build ! 


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();
