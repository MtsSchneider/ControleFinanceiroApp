using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;

namespace ControleFinanceiroApp.ModelBinders
{
    public class EmptyToNullDecimalModelBinder : IModelBinder
    {
        private readonly IModelBinder _defaultBinder;

        public EmptyToNullDecimalModelBinder(IModelBinder defaultBinder)
        {
            _defaultBinder = defaultBinder;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                return _defaultBinder.BindModelAsync(bindingContext);
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            string value = valueProviderResult.FirstValue;

            // Se o valor for uma string vazia, defina o Model como null
            if (string.IsNullOrWhiteSpace(value))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            // Se não for vazia, deixe o binder padrão cuidar da conversão para decimal?
            return _defaultBinder.BindModelAsync(bindingContext);
        }
    }
}