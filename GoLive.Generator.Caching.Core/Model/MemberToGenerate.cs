using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace GoLive.Generator.Caching.Core.Model;

public class MemberToGenerate
{
    public bool Async { get; set; }
    public string Name { get; set; }
    public int CacheDuration { get; set; }
    public TimeFrame CacheDurationTimeFrame { get; set; }
    public int StaleDuration { get; set; }
    public TimeFrame StaleDurationTimeFrame { get; set; }
    public INamedTypeSymbol returnType { get; set; }
    public bool IsGenericMethod { get; set; }/*
    public string[] GenericConstraints { get; set; }*/
    public List<ParameterToGenerate> Parameters { get; set; }
    public List<GenericParameter> GenericParameters { get; set; }
    
    /*public List<ParameterToGenerate> GenericTypeParameters { get; set; }*/
    public bool returnTypeUnwrappedTask { get; set; }
}


public class GenericParameter
{
    public string Name { get; set; }
    public List<string> Constraints { get; set; }
}