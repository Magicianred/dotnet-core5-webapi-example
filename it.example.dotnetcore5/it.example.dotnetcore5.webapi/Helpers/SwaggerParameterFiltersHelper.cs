using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;
using static it.example.dotnetcore5.webapi.Helpers.SwaggerConfigHelper;

namespace it.example.dotnetcore5.webapi.Helpers
{
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
}