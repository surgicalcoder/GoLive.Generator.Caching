﻿using System.Collections.Generic;

namespace GoLive.Generator.Caching.Core.Model;

public class ClassToGenerate
{
    public override string ToString()
    {
        return $"{nameof(Name)}: {Name}, {nameof(Members)}: {Members?.Count}, {nameof(Filename)}: {Filename}, {nameof(Namespace)}: {Namespace}";
    }

    public string Name { get; set; }
    public List<MemberToGenerate> Members { get; set; } = new();
    public string Filename { get; set; }
    public string Namespace { get; set; }
    public bool HasJsonOptions { get; set; }
}