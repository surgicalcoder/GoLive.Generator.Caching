﻿using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace GoLive.Generator.Caching.FusionCache;

public class MemberToGenerate
{
    public bool Async { get; set; }
    public string Name { get; set; }
    public int CacheDuration { get; set; }
    public TimeFrame CacheDurationTimeFrame { get; set; }
    public INamedTypeSymbol returnType { get; set; }
    public bool IsGenericMethod { get; set; }
    public string[] GenericConstraints { get; set; }
    public List<ParameterToGenerate> Parameters { get; set; }
}