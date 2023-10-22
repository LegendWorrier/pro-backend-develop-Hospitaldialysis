using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Wasenshi.HemoDialysisPro.Web.Api.Swagger
{
    public static class ConfigureSwagger
    {
        public static void Setup(this SwaggerGenOptions c)
        {
            c.DocumentFilter<SecurityRequirementsDocumentFilter>();
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { In = ParameterLocation.Header, Description = "Please insert JWT with Bearer into field", Name = "Authorization", Type = SecuritySchemeType.ApiKey });
            c.ResolveConflictingActions(d =>
            {
                var descriptions = d as ApiDescription[] ?? d.ToArray();
                var first = descriptions.First(); // build relative to the 1st method
                var parameters = descriptions.SelectMany(d => d.ParameterDescriptions).ToList();

                first.ParameterDescriptions.Clear();
                // add parameters and make them optional
                foreach (var parameter in parameters)
                    if (first.ParameterDescriptions.All(x => x.Name != parameter.Name))
                    {
                        first.ParameterDescriptions.Add(new ApiParameterDescription
                        {
                            ModelMetadata = parameter.ModelMetadata,
                            Name = parameter.Name,
                            ParameterDescriptor = parameter.ParameterDescriptor,
                            Source = parameter.Source,
                            IsRequired = false,
                            DefaultValue = null
                        });
                    }
                return first;
            });
        }
    }
}
