namespace GdUnit4.Analyzers.Test;

public static class TestSourceBuilder
{
    public static string Instrument(string sourceCode) =>
        $$"""
          using System;
          using System.Collections.Generic;
          using GdUnit4;
          using Godot;

          namespace GdUnit4.Analyzers.Test.Example
          {
              [TestSuite]
              public class TestClass
              {
                  {{sourceCode}}
              }
          }
          """;
}
