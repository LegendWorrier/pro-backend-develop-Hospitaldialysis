using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;
using System.Threading.Tasks;
using System.Web;

namespace Wasenshi.HemoDialysisPro.Web.Api.Utils
{
    public class DecodePathStringsBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            return context.Metadata.ModelType == typeof(string) && context.BindingInfo.BindingSource == BindingSource.Path ? new BinderTypeModelBinder(typeof(DecodePathStringsBinder)) : null;
        }
    }

    public class DecodePathStringsBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.ModelName;

            // Try to fetch the value of the argument by name
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            var value = valueProviderResult.FirstValue;
            var urlDecode = HttpUtility.UrlDecode(value);

            bindingContext.ModelState.SetModelValue(modelName, urlDecode, value);
            bindingContext.Result = ModelBindingResult.Success(urlDecode);

            return Task.CompletedTask;
        }
    }
}
