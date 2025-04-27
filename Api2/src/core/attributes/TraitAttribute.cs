namespace GdUnit4.core.attributes;

using System;

/// <summary>
///     Specifies a trait for a test method.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class TraitAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TraitAttribute" /> class.
    /// </summary>
    /// <param name="name">The name of the trait.</param>
    /// <param name="value">The value of the trait.</param>
    public TraitAttribute(string name, string value)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    ///     Gets the name of the trait.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets the value of the trait.
    /// </summary>
    public string Value { get; }
}
