# Instruction for add Versioning web api

## Add versioning to Web Api

1. Add package
```cmd
Install-Package Microsoft.AspNetCore.Mvc.Versioning
```

2. In Startup.cs add

```csharp
public void ConfigureServices(IServiceCollection services)
{
	// after: services.AddMvc(); or services.AddControllers();
	services.AddApiVersioning(o => {
		o.ReportApiVersions = true;
		o.AssumeDefaultVersionWhenUnspecified = true;
		o.DefaultApiVersion = new ApiVersion(1, 0);
	});
}
```

- ReportApiVersions = true 
Allow to show in header of response the version info of the api

- AssumeDefaultVersionWhenUnspecified = true;
Allow to go at a specific version when not set, default 1.0

- DefaultApiVersion = new ApiVersion(1, 0);
Set a specific version as default

3. URL Path Based Versioning

In controller add version attribute at class level and set the Route attribute with your own url pattern
Remember to add ```[ApiController]``` and use the placeholder ```{version:apiVersion}``` also if you use only the major version without minor version

```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/{version:apiVersion}/home")]
public class HomeV1Controller : Controller
{
	// your actions
}
```

You can decorate the class controller with more than one version

```csharp
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("1.1")]
[ApiVersion("2.0")]
[Route("api/{version:apiVersion}/home")]
public class HomeV1Controller : Controller
{
	// your actions
}
```

You can choise to use different folder for different version or use another strategy for organize the files

## Configure Swagger for handle multiple version

1. Add package
```cmd
 Install-Package Swashbuckle.AspNetCore.Swagger   
 Install-Package Swashbuckle.AspNetCore.SwaggerGen   
 Install-Package Swashbuckle.AspNetCore.SwaggerUi
```

2. In Startup.cs add

```csharp
public void ConfigureServices(IServiceCollection services)
{
	// after: services.AddApiVersioning();
	services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "Api version  1", Version = "v1", Description = "Test Description", });
        options.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "Api version  2", Version = "v2", Description = "Test Description", });
        /// options.OperationFilter<AddAcceptHeaderParameter>();
        options.OperationFilter<SwaggerParameterFilters>();
        options.DocumentFilter<SwaggerVersionMapping>();
                
        options.DocInclusionPredicate((version, desc) =>
        {
            if (!desc.TryGetMethodInfo(out MethodInfo methodInfo)) return false;
            var versions = methodInfo.DeclaringType.GetCustomAttributes(true).OfType<ApiVersionAttribute>().SelectMany(attr => attr.Versions);
            var maps = methodInfo.GetCustomAttributes(true).OfType<MapToApiVersionAttribute>().SelectMany(attr => attr.Versions).ToArray();
            version = version.Replace("v", "");
            return versions.Any(v => v.ToString() == version && maps.Any(v => v.ToString() == version));
        });
    });
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // after app.UseRouting();

    app.UseSwagger(options => options.RouteTemplate = "swagger/{documentName}/swagger.json");
    app.UseSwaggerUI(options =>
    {
                
        options.DocumentTitle = "Test Title";
        options.SwaggerEndpoint($"/swagger/v1/swagger.json", $"v1");
        options.SwaggerEndpoint($"/swagger/v2/swagger.json", $"v2");
    });
}
```

3. Add three new helper files

- SwaggerConfigHelper.cs
```csharp
public class SwaggerConfigHelper
    {
        public enum VersioningType
        {
            None, CustomHeader, QueryString, AcceptHeader
        }
        public static String QueryStringParam { get; private set; }
        public static String CustomHeaderParam { get; private set; }
        public static String AcceptHeaderParam { get; private set; }
        public static VersioningType CurrentVersioningMethod { get => currentVersioningMethod; set => currentVersioningMethod = value; }

        private const VersioningType none = VersioningType.None;
        private static VersioningType currentVersioningMethod = none;

        public static void UseCustomHeaderApiVersion(string parameterName)
        {
            CurrentVersioningMethod = VersioningType.CustomHeader;
            CustomHeaderParam = parameterName;
        }

        public static void UseQueryStringApiVersion()
        {
            QueryStringParam = "api-version";
            CurrentVersioningMethod = VersioningType.QueryString;
        }
        public static void UseQueryStringApiVersion(string parameterName)
        {
            CurrentVersioningMethod = VersioningType.QueryString;
            QueryStringParam = parameterName;
        }
        public static void UseAcceptHeaderApiVersion(String paramName)
        {
            CurrentVersioningMethod = VersioningType.AcceptHeader;
            AcceptHeaderParam = paramName;
        }
    }
```

- SwaggerVersionMappingHelper.cs
```csharp
public class SwaggerVersionMappingHelper : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var pathLists = new OpenApiPaths();
            //IDictionary<string, OpenApiPaths> paths = new Dictionary<string, OpenApiPaths>();
            //var version = swaggerDoc.Info.Version.Replace("v", "").Replace("version", "").Replace("ver", "").Replace(" ", "");
            foreach (var path in swaggerDoc.Paths)
            {
                pathLists.Add(path.Key.Replace("v{version}", swaggerDoc.Info.Version), path.Value);
            }
            swaggerDoc.Paths = pathLists;
        }
    }
```

- SwaggerParameterFiltersHelper.cs
```csharp
public class SwaggerParameterFiltersHelper : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            try
            {
                var maps = context.MethodInfo.GetCustomAttributes(true).OfType<MapToApiVersionAttribute>().SelectMany(attr => attr.Versions).ToList();
                var version = maps[0].MajorVersion;
                if (SwaggerConfigHelper.CurrentVersioningMethod == VersioningType.CustomHeader && !context.ApiDescription.RelativePath.Contains("{version}"))
                {
                    operation.Parameters.Add(new OpenApiParameter { Name = SwaggerConfigHelper.CustomHeaderParam, In = ParameterLocation.Header, Required = false, Schema = new OpenApiSchema { Type = "String", Default = new OpenApiString(version.ToString()) } });
                }
                else if (SwaggerConfigHelper.CurrentVersioningMethod == VersioningType.QueryString && !context.ApiDescription.RelativePath.Contains("{version}"))
                {
                    operation.Parameters.Add(new OpenApiParameter { Name = SwaggerConfigHelper.QueryStringParam, In = ParameterLocation.Query, Schema = new OpenApiSchema { Type = "String", Default = new OpenApiString(version.ToString()) } });
                }
                else if (SwaggerConfigHelper.CurrentVersioningMethod == VersioningType.AcceptHeader && !context.ApiDescription.RelativePath.Contains("{version}"))
                {

                    operation.Parameters.Add(new OpenApiParameter { Name = "Accept", In = ParameterLocation.Header, Required = false, Schema = new OpenApiSchema { Type = "String", Default = new OpenApiString($"application/json;{SwaggerConfigHelper.AcceptHeaderParam}=" + version.ToString()) } });

                }

                var versionParameter = operation.Parameters.FirstOrDefault(p => p.Name == "version");

                if (versionParameter != null)
                {
                    operation.Parameters.Remove(versionParameter);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
```

4. Add MapToApiVersion attribute to any action for show it in Swagger

```csharp
[HttpGet("{id}")]
[MapToApiVersion("3")]
public IPost Get(int id)
{
    var post = _postsService.GetById(id);

    return post;
}
```

You can use more than one attribute for the action

```csharp
[HttpGet("{id}")]
[MapToApiVersion("1")]
[MapToApiVersion("2")]
public IPost Get(int id)
{
    var post = _postsService.GetById(id);

    return post;
}
```

## References

https://dotnetcoretutorials.com/2017/01/17/api-versioning-asp-net-core/
https://medium.com/@esty_c/versioning-asp-net-core-apis-with-swagger-3c44825b1baf