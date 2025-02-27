﻿using System.Diagnostics;

namespace CatalogSupport.SharedKernel.ActivityScope;

public sealed record StartActivityOptions
{
    public ActivityKind Kind = ActivityKind.Internal;
    public Dictionary<string, object?> Tags { get; set; } = [];

    public string? ParentId { get; set; }

    public ActivityContext? Parent { get; set; }
}
