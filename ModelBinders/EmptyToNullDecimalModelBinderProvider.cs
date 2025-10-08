using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;
using Microsoft.Extensions.Logging;

namespace ControleFinanceiroApp.ModelBinders
{
    public class EmptyToNullDecimalModelBinderProvider : IModelBinderProvider
    {
        // Removido o ILoggerFactory do construtor
        // O ASP.NET Core injetará ILoggerFactory no SimpleTypeModelBinder internamente.

        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (context.Metadata.ModelType == typeof(decimal?))
            {
                // Para a sua versão do .NET, a injeção do ILoggerFactory 
                // precisa ser feita através do contexto do serviço, sem o construtor explícito.

                // O ILoggerFactory é buscado do contexto de serviços
                var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();

                IModelBinder fallbackBinder = new SimpleTypeModelBinder(
                    typeof(decimal?),
                    loggerFactory
                );

                return new EmptyToNullDecimalModelBinder(fallbackBinder);
            }

            return null;
        }
    }
}