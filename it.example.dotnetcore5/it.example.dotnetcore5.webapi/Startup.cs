using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

// for Domain layer
using it.example.dotnetcore5.domain.Extensions;

// for BL layer
using it.example.dotnetcore5.bl.Extensions;

// for fake repository
using it.example.dotnetcore5.dal.fake.Extentions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using it.example.dotnetcore5.webapi.Helpers;
using System.Linq;

// for dapper
//using it.example.dotnetcore5.dal.dapper.Extentions;

// for json file
//using it.example.dotnetcore5.dal.json.Repositories;

// for ef sql server repository
//using it.example.dotnetcore5.dal.ef.sqlserver.Extensions;

// for ef mysql repository
//using it.example.dotnetcore5.dal.ef.mysql.Extensions;

// for ef sqlite repository
//using it.example.dotnetcore5.dal.ef.sqlite.Extensions;

namespace it.example.dotnetcore5.webapi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddApiVersioning(options => {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
            });
            //services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new OpenApiInfo { Title = "it.example.dotnetcore5.webapi", Version = "v1" });
            //});

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "Api version  1", Version = "v1", Description = "Test Description", });
                options.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "Api version  2", Version = "v2", Description = "Test Description", });
                options.SwaggerDoc("v3", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "Api version  3", Version = "v3", Description = "Test Description", });
                /// options.OperationFilter<AddAcceptHeaderParameter>();
                options.OperationFilter<SwaggerParameterFiltersHelper>();
                options.DocumentFilter<SwaggerVersionMappingHelper>();

                options.DocInclusionPredicate((version, desc) =>
                {
                    if (!desc.TryGetMethodInfo(out MethodInfo methodInfo)) return false;
                    var versions = methodInfo.DeclaringType.GetCustomAttributes(true).OfType<ApiVersionAttribute>().SelectMany(attr => attr.Versions);
                    var maps = methodInfo.GetCustomAttributes(true).OfType<MapToApiVersionAttribute>().SelectMany(attr => attr.Versions).ToArray();
                    version = version.Replace("v", "");
                    return versions.Any(v => v.ToString() == version && maps.Any(v => v.ToString() == version));
                });
            });

            services.AddDomain();

            // add fake dal
            services.AddDalFake();

            // add dapper dal
            //services.AddDalDapper();

            // add json dal
            //services.AddDalJson();

            // add mysql dal
            //services.AddDalMySql(Configuration.GetConnectionString("myBlog_mysql"));

            // add sqlite dal
            //services.AddDalSqlite(Configuration.GetConnectionString("myBlog_sqlite"));

            // Configuration for Sql Server
            //services.AddDalSqlServer(Configuration.GetConnectionString("myBlog_mssql"));

            // add BL layer
            services.AddBL();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseSwagger();
                //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "it.example.dotnetcore5.webapi v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseSwagger(options => options.RouteTemplate = "swagger/{documentName}/swagger.json");
            app.UseSwaggerUI(options =>
            {

                options.DocumentTitle = "Test Title";
                options.SwaggerEndpoint($"/swagger/v1/swagger.json", $"v1");
                options.SwaggerEndpoint($"/swagger/v2/swagger.json", $"v2");
                options.SwaggerEndpoint($"/swagger/v3/swagger.json", $"v3");
            });

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
