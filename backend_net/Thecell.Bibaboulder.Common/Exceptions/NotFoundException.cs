using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Thecell.Bibaboulder.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException()
    {
    }

    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public static void ThrowIfNull([NotNull] object? obj, string? entityName = null, Guid? id = null)
    {
        if (obj is null)
        {
            throw CreateNotFoundException(entityName, id);
        }
    }

    public static void ThrowIfNullOrDefault<T>([NotNull] T? obj, string? entityName = null, Guid? id = null)
    {
        if (obj is null || EqualityComparer<T>.Default.Equals(obj, default!))
        {
            throw CreateNotFoundException(entityName, id);
        }
    }

    private static NotFoundException CreateNotFoundException(string? entityName, Guid? id)
    {
        var idString = id is null ? string.Empty : $" (Id: {id})";
        var entityNameString = entityName is null ? "object" : $"{entityName}";
        return new NotFoundException($"{entityNameString} not found.{idString}");
    }
}
