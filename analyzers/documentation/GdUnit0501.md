# GdUnit0501

## Godot Runtime Required for Test Method

| Id         | Category        | Severity | Enabled |
|------------|-----------------|----------|---------|
| GdUnit0501 | Attribute Usage | Error    | True    |

# Problem Description

Test methods that interact with Godot types (such as Node, Scene, or other Godot-derived classes) must use `[GodotTestCase]` instead of `[TestCase]`.
The `[GodotTestCase]` attribute ensures the test runs in a proper Godot engine environment, which is required for any Godot native functionality.

### Error example:

```csharp
    [TestCase] // ❌ GdUnit0501: Test method 'VerifyDictionaryTypes' uses Godot native types or calls but is marked with [TestCase]. Use [GodotTestCase] instead when testing Godot functionality.
    public void TestWithGodot() 
    {
        var instance = new Node2D()
        AssertThat(instance).IsNotNull();
    }
```

### How to fix:

Replace `[TestCase]` with `[GodotTestCase]` when testing Godot functionality:

```csharp
    [GodotTestCase] // ✅ Correct: Uses [GodotTestCase] for Godot runtime tests
    public void TestDataPointProperty(int a, int b, int expected) 
    {
        var instance = new Node2D()
        AssertThat(instance).IsNotNull();
    }
```
