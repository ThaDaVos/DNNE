using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using DNNE.Assembly.Entities.Interfaces;

namespace DNNE.Assembly.Entities;

internal abstract class ExportedEntity<TEntity> : IExportedEntity where TEntity : struct
{
    protected readonly MetadataReader metadataReader;
    protected readonly TEntity entity;
    protected string? name;
    public string Name => name ??= GetName();

    public ExportedEntity(MetadataReader metadataReader, TEntity entity)
    {
        this.entity = entity;
        this.metadataReader = metadataReader;
    }
    protected abstract string GetName();
}
