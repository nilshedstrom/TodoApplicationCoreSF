using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using TodoActor.Interfaces;

namespace Web
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
            services.AddMvc();

            services.AddSwaggerGen(c =>
            {
                c.SchemaFilter<SchemaFilter>();
                c.OperationFilter<OperationFilter>();
                c.DescribeAllEnumsAsStrings();
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.EnableAnnotations();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
            });


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }



    public class SchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            switch (context.SystemType.FullName)
            {
                case "System.DateTime":
                    schema.Example = DateTime.Now;
                    break;
                case "TodoActor.Interfaces.TodoItem":
                    schema.Example = new TodoItem
                    {
                        Description = "A simple task 2",
                        DateAdded = DateTime.Now.AddDays(-7),
                        DateFinished = DateTime.Now.AddMinutes(-10),
                        Finished = true
                    };
                    break;
                case "System.Guid":
                    schema.Example = Guid.NewGuid();
                    break;
            }
        }
    }

    public class OperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            switch (operation.OperationId)
            {
                case "ApiTodoGet":
                    operation.Responses["200"].Examples = new List<TodoItem>
                    {
                        new TodoItem
                        {
                            Description = "A simple task",
                            DateAdded = DateTime.Now.AddDays(-7),
                            DateFinished = DateTime.Now.AddMinutes(-10),
                            Finished = true
                        }
                    };
                    break;
                case "ApiTodoPost":
                    operation.Parameters.Cast<NonBodyParameter>().Where(x => x.Name == "Id").ToList().ForEach(x => x.Default= Guid.NewGuid());
                    break;
            }
        }
    }
}
