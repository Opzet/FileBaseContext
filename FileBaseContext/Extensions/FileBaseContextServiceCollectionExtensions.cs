﻿using FileBaseContext.Infrastructure;
using FileBaseContext.Infrastructure.Query;
using FileBaseContext.Storage;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;

namespace FileBaseContext.Extensions;

public static class FileBaseContextServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkFileBaseContextDatabase(this IServiceCollection serviceCollection)
    {
        var builder = new EntityFrameworkServicesBuilder(serviceCollection)
            .TryAdd<LoggingDefinitions, FileBaseContextLoggingDefinitions>()
            .TryAdd<IDatabaseProvider, DatabaseProvider<FileBaseContextOptionsExtension>>()
            .TryAdd<IValueGeneratorSelector, FileContextBaseValueGeneratorSelector>()
            .TryAdd<IDatabase>(p => p.GetService<IFileBaseContextDatabase>())
            .TryAdd<IDbContextTransactionManager, FileBaseContextTransactionManager>()
            .TryAdd<IDatabaseCreator, FileBaseContextDatabaseCreator>()
            .TryAdd<IQueryContextFactory, FileBaseContextQueryContextFactory>()
            .TryAdd<IProviderConventionSetBuilder, FileBaseContextConventionSetBuilder>()
            .TryAdd<ITypeMappingSource, FileBaseContextTypeMappingSource>()

            //// New Query pipeline
            .TryAdd<IShapedQueryCompilingExpressionVisitorFactory, FileBaseContextShapedQueryCompilingExpressionVisitorFactory>()
            .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, FileBaseContextQueryableMethodTranslatingExpressionVisitorFactory>()
            .TryAdd<IQueryTranslationPostprocessorFactory, FileBaseContextQueryTranslationPostprocessorFactory>()

            .TryAddProviderSpecificServices(
                b => b
                    .TryAddSingleton<IFileBaseContextFileManager, FileBaseContextFileManager>()
                    .TryAddSingleton<IFileBaseContextSingletonOptions, FileBaseContextSingletonOptions>()
                    .TryAddSingleton<IFileBaseContextStoreCache, FileBaseContextStoreCache>()
                    .TryAddScoped<IFileBaseContextDatabase, FileBaseContextDatabase>()
            );

        builder.TryAddCoreServices();

        return serviceCollection;
    }
}