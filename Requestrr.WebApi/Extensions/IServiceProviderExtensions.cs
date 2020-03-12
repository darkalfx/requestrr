using System;

namespace Requestrr.WebApi
{
    public static class IServiceProviderExtensions
    {
        public static T Get<T>(this IServiceProvider serviceProvider)
        {
            return (T)serviceProvider.GetService(typeof(T));
        }
    }
}