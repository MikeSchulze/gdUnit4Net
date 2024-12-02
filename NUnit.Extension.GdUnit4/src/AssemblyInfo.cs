using System.Runtime.CompilerServices;

using NUnit.Engine;
using NUnit.Engine.Extensibility;

[assembly: ExtensionPoint("/NUnit/Engine/DriverService", typeof(IDriverService))]
[assembly: InternalsVisibleTo("nunit.extension.gdunit4.test")]
