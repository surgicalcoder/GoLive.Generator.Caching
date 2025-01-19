﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GoLive.Generator.Caching.Core.EmbeddedResources;

public static class EmbeddedResource
{
#if DEBUG
    static readonly string baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
#endif

    /// <summary>
    /// Gets the content of the embedded resource at the specified relative path.
    /// </summary>
    public static string GetContent(string relativePath)
    {
        using var stream = GetStream(relativePath);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Gets the bytes of the embedded resource at the specified relative path.
    /// </summary>
    public static byte[] GetBytes(string relativePath)
    {
        using var stream = GetStream(relativePath);
        var bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);
        return bytes;
    }

    /// <summary>
    /// Gets the stream of the embedded resource at the specified relative path.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static Stream GetStream(string relativePath)
    {
        var baseName = Assembly.GetExecutingAssembly().GetName().Name;
        var resourceName = relativePath
            .TrimStart('.')
            .Replace('/', '.')
            .Replace('\\', '.');

        var manifestResourceName = Assembly.GetExecutingAssembly()
            .GetManifestResourceNames().FirstOrDefault(x => x.EndsWith(resourceName, StringComparison.Ordinal));

        if (string.IsNullOrEmpty(manifestResourceName))
            throw new InvalidOperationException($"Did not find required resource ending in '{resourceName}' in assembly '{baseName}'.");

        return
            Assembly.GetExecutingAssembly().GetManifestResourceStream(manifestResourceName) ??
            throw new InvalidOperationException($"Did not find required resource '{manifestResourceName}' in assembly '{baseName}'.");
    }
}