using Reflex.Core;
using Reflex.Enums;

namespace GridFinder.Installation
{
    public static class ReflexExtensions
    {
        public static void RegisterSingleton<T>(this ContainerBuilder builder, Resolution resolution)
        {
            builder.RegisterType(typeof(T), Lifetime.Singleton, resolution);
        }
    }
}