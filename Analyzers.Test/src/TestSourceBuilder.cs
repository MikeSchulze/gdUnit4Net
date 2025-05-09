namespace GdUnit4.Analyzers.Test;

public static class TestSourceBuilder
{
    public static string Instrument(string sourceCode) =>
        $$"""
          using System;
          using System.Collections;
          using System.Collections.Generic;
          using System.Collections.Immutable;
          using System.Collections.Specialized;
          using GdUnit4.Asserts;
          using GdUnit4.Core.Execution.Exceptions;
          using GdUnit4.Core.Extensions;
          using Godot;
          using Godot.Collections;
          using static GdUnit4.Assertions;

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
