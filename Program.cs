using ApiVersioning;
using Asp.Versioning;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(options => options.OperationFilter<SwaggerDefaultValues>());
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1); //define uma versão default quando não especificado
        options.AssumeDefaultVersionWhenUnspecified = true;
        //options.ApiVersionReader = new HeaderApiVersionReader("api-version"); //espera o valor no Header
        options.ApiVersionReader = new UrlSegmentApiVersionReader(); //espera o valor na uRL
    })
    .AddApiExplorer(options =>
    {
        // Agrupar por numero de versao
        options.GroupNameFormat = "'v'VVV";

        // Necessario para o correto funcionamento das rotas
        options.SubstituteApiVersionInUrl = true;
    })
    .EnableApiVersionBinding();



var app = builder.Build();

var versionSet = app.NewApiVersionSet()
    .HasApiVersion(new Asp.Versioning.ApiVersion(1))
    .HasApiVersion(new Asp.Versioning.ApiVersion(2))
    .ReportApiVersions() //mostra as versões disponiveis
    .Build();

app.MapGet("v{version:apiVersion}/hello", (HttpContext context) =>
//app.MapGet("hello", (HttpContext context) =>
{
    var apiVersion = context.GetRequestedApiVersion();
    return $"Hello world versão {apiVersion!.ToString()}";
})
    .WithApiVersionSet(versionSet)
    .HasDeprecatedApiVersion(1)
    .MapToApiVersion(1);

//app.MapGet("hello", (HttpContext context) =>
app.MapGet("v{version:apiVersion}/hello", (HttpContext context) =>
{
    var apiVersion = context.GetRequestedApiVersion();
    return $"Hello world versão {apiVersion!.ToString()}";
})
.WithApiVersionSet(versionSet)
.MapToApiVersion(2);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(
        options =>
        {
            var descriptions = app.DescribeApiVersions();
            foreach (var description in descriptions)
            {
                var url = $"/swagger/{description.GroupName}/swagger.json";
                var name = description.GroupName.ToUpperInvariant();
                options.SwaggerEndpoint(url, name);
            }
        });

}

app.Run();
