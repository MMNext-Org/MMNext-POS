// Quick resource checker
using System;
using System.Reflection;

class Program
{
    static void Main()
    {
        var assembly = Assembly.LoadFrom(@"J:\Project 1\MMNext POS\src\MMNextPOS.Infrastructure\bin\Release\net8.0\MMNextPOS.Infrastructure.dll");
        var resources = assembly.GetManifestResourceNames();

        Console.WriteLine("Found {0} resources:", resources.Length);
        foreach (var r in resources)
        {
            if (r.Contains("Migration", StringComparison.OrdinalIgnoreCase))
                Console.WriteLine("  " + r);
        }
    }
}