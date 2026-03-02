using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ProductionPlanning.Models.Logging
{
    public static class FileLoggerExtensions
    {
        public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>());
            builder.Services.Configure<FileLoggerOptions>(options => { });
            return builder;
        }

        public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, Action<FileLoggerOptions> configure)
        {
            builder.AddFileLogger();
            builder.Services.Configure(configure);
            return builder;
        }

        //public static ILoggingBuilder AddDatabaseLogger(this ILoggingBuilder builder)
        //{
        //    builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, DatabaseLoggerProvider>());
        //    builder.Services.Configure<DatabaseLoggerOptions>(options => { });
        //    return builder;
        //}

        //public static ILoggingBuilder AddDatabaseLogger(this ILoggingBuilder builder, Action<DatabaseLoggerOptions> configure)
        //{
        //    builder.AddDatabaseLogger();
        //    builder.Services.Configure(configure);
        //    return builder;
        //}
    }
}
