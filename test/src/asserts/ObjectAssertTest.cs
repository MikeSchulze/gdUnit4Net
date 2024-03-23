namespace GdUnit4.Tests.Asserts;

using System.Collections.Generic;

using Executions;
using Exceptions;
using static Assertions;
using GdUnit4.Asserts;

internal partial class CustomClass : Godot.RefCounted
{
    public partial class InnerClassA : Godot.Node { }

    public sealed partial class InnerClassB : InnerClassA { }

    public sealed partial class InnerClassC : Godot.Node { }
}

internal sealed partial class CustomClassB : CustomClass
{
}

[TestSuite]
public class ObjectAssertTest
{
    [TestCase]
    public void IsEqual()
    {
        var obj = new object();
        var boxMesh = new Godot.BoxMesh();
        var skin = new Godot.Skin();
        AssertObject(boxMesh).IsEqual(boxMesh);
        AssertObject(boxMesh).IsEqual(new Godot.BoxMesh());
        AssertObject(obj).IsEqual(obj);

        // should fail because the current is an CubeMesh and we expect equal to a Skin
        AssertThrown(() => AssertObject(boxMesh).IsEqual(skin))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(37)
            .HasMessage("""
                Expecting be equal:
                    $skin but is $boxMesh
                """
                    .Replace("$boxMesh", AssertFailures.AsObjectId(boxMesh))
                    .Replace("$skin", AssertFailures.AsObjectId(skin)));
        AssertThrown(() => AssertObject(obj).IsEqual(new List<int>()))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(46)
            .HasMessage("""
                Expecting be equal:
                    <Empty>
                 but is
                    $obj
                """
                    .Replace("$obj", AssertFailures.AsObjectId(obj)));
    }

    [TestCase]
    public void IsNotEqual()
    {
        var obj = new object();
        var boxMesh = new Godot.BoxMesh();
        var skin = new Godot.Skin();
        AssertObject(boxMesh).IsNotEqual(skin);
        AssertObject(obj).IsNotEqual(new List<object>());

        // should fail because the current is an CubeMesh and we expect not equal to a CubeMesh
        AssertThrown(() => AssertObject(boxMesh).IsNotEqual(boxMesh))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(68)
            .HasMessage("""
                Expecting be NOT equal:
                    $boxMesh but is $boxMesh
                """
                    .Replace("$boxMesh", AssertFailures.AsObjectId(boxMesh)));
        AssertThrown(() => AssertObject(obj).IsNotEqual(obj))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(76)
            .HasMessage("""
                Expecting be NOT equal:
                    $obj but is $obj
                """
                    .Replace("$obj", AssertFailures.AsObjectId(obj)));
    }

    [TestCase]
    public void IsInstanceOf()
    {
        // engine class test
        AssertObject(AutoFree(new Godot.Path2D())).IsInstanceOf<Godot.Node>();
        AssertObject(AutoFree(new Godot.Camera2D())).IsInstanceOf<Godot.Camera2D>();
        // script class test
        // inner class test
        AssertObject(AutoFree(new CustomClass.InnerClassA())).IsInstanceOf<Godot.Node>();
        AssertObject(AutoFree(new CustomClass.InnerClassB())).IsInstanceOf<CustomClass.InnerClassA>();
        // c# class
        AssertObject(new object()).IsInstanceOf<object>();
        AssertObject("").IsInstanceOf<object>();
        AssertObject(new CustomClass()).IsInstanceOf<object>();
        AssertObject(new CustomClassB()).IsInstanceOf<object>();
        AssertObject(new CustomClassB()).IsInstanceOf<CustomClass>();

        // should fail because the current is not a instance of `Tree`
        AssertThrown(() => AssertObject(AutoFree(new Godot.Path2D())).IsInstanceOf<Godot.Tree>())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(104)
            .HasMessage("Expected be instance of:\n"
                + "    <Godot.Tree> but is <Godot.Path2D>");
        AssertThrown(() => AssertObject(null).IsInstanceOf<Godot.Tree>())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(109)
            .HasMessage("Expected be instance of:\n"
                + "    <Godot.Tree> but is <Null>");
        AssertThrown(() => AssertObject(new CustomClass()).IsInstanceOf<CustomClassB>())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(114)
            .HasMessage("Expected be instance of:\n"
                + "    <GdUnit4.Tests.Asserts.CustomClassB> but is <GdUnit4.Tests.Asserts.CustomClass>");
    }

    [TestCase]
    public void IsNotInstanceOf()
    {
        AssertObject(null).IsNotInstanceOf<Godot.Node>();
        // engine class test
        AssertObject(AutoFree(new Godot.Path2D())).IsNotInstanceOf<Godot.Tree>();
        // inner class test
        AssertObject(AutoFree(new CustomClass.InnerClassA())).IsNotInstanceOf<Godot.Tree>();
        AssertObject(AutoFree(new CustomClass.InnerClassB())).IsNotInstanceOf<CustomClass.InnerClassC>();
        // c# class
        AssertObject(new CustomClass()).IsNotInstanceOf<CustomClassB>();

        // should fail because the current is not a instance of `Tree`
        AssertThrown(() => AssertObject(AutoFree(new Godot.Path2D())).IsNotInstanceOf<Godot.Node>())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(134)
            .HasMessage("Expecting be NOT a instance of:\n"
                + "    <Godot.Node>");
        AssertThrown(() => AssertObject(AutoFree(new CustomClassB())).IsNotInstanceOf<CustomClass>())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(139)
            .HasMessage("Expecting be NOT a instance of:\n"
                + "    <GdUnit4.Tests.Asserts.CustomClass>");
    }

    [TestCase]
    public void IsNull()
    {
        AssertObject(null).IsNull();
        // should fail because the current is not null
        AssertThrown(() => AssertObject(AutoFree(new Godot.Node())).IsNull())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(151)
            .StartsWithMessage("Expecting be <Null>:\n"
                + " but is\n"
                + "    <Godot.Node>");
        AssertThrown(() => AssertObject(new object()).IsNull())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(157)
            .StartsWithMessage("Expecting be <Null>:\n"
                + " but is\n"
                + "    <System.Object>");
    }

    [TestCase]
    public void IsNotNull()
    {
        AssertObject(AutoFree(new Godot.Node())).IsNotNull();
        AssertObject(new object()).IsNotNull();
        // should fail because the current is null
        AssertThrown(() => AssertObject(null).IsNotNull())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(171)
            .HasMessage("Expecting be NOT <Null>:");
    }

    [TestCase]
    public void IsSame()
    {
        var obj1 = AutoFree(new Godot.Node())!;
        var obj2 = obj1;
        var obj3 = AutoFree(obj1.Duplicate())!;
        AssertObject(obj1).IsSame(obj1);
        AssertObject(obj1).IsSame(obj2);
        AssertObject(obj2).IsSame(obj1);
        var o1 = new object();
        var o2 = o1;
        AssertObject(o1).IsSame(o1);
        AssertObject(o1).IsSame(o2);
        AssertObject(o2).IsSame(o1);

        AssertThrown(() => AssertObject(null).IsSame(obj1))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(192)
            .HasMessage("""
                Expecting be same:
                    $obj
                 to refer to the same object
                    <Null>
                """
                    .Replace("$obj", AssertFailures.AsObjectId(obj1)));
        AssertThrown(() => AssertObject(obj1).IsSame(obj3))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(202)
            .HasMessage("""
                Expecting be same:
                    $obj3
                 to refer to the same object
                    $obj1
                """
                    .Replace("$obj1", AssertFailures.AsObjectId(obj1))
                    .Replace("$obj3", AssertFailures.AsObjectId(obj3)));
        AssertThrown(() => AssertObject(obj3).IsSame(obj1))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(213)
            .HasMessage("""
                Expecting be same:
                    $obj1
                 to refer to the same object
                    $obj3
                """
                    .Replace("$obj1", AssertFailures.AsObjectId(obj1))
                    .Replace("$obj3", AssertFailures.AsObjectId(obj3)));
        AssertThrown(() => AssertObject(obj3).IsSame(obj2))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(224)
            .HasMessage("""
                Expecting be same:
                    $obj2
                 to refer to the same object
                    $obj3
                """
                    .Replace("$obj2", AssertFailures.AsObjectId(obj2))
                    .Replace("$obj3", AssertFailures.AsObjectId(obj3)));
    }

    [TestCase]
    public void IsNotSame()
    {
        var obj1 = AutoFree(new Godot.Node())!;
        var obj2 = obj1;
        var obj3 = AutoFree(obj1.Duplicate())!;
        AssertObject(null).IsNotSame(obj1);
        AssertObject(obj1).IsNotSame(obj3);
        AssertObject(obj3).IsNotSame(obj1);
        AssertObject(obj3).IsNotSame(obj2);

        var o1 = new object();
        var o2 = new object();
        AssertObject(null).IsNotSame(o1);
        AssertObject(o1).IsNotSame(o2);
        AssertObject(o2).IsNotSame(o1);

        AssertThrown(() => AssertObject(obj1).IsNotSame(obj1))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(254)
            .HasMessage("Expecting be NOT same: $obj".Replace("$obj", AssertFailures.AsObjectId(obj1)));
        AssertThrown(() => AssertObject(obj1).IsNotSame(obj2))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(258)
            .HasMessage("Expecting be NOT same: $obj".Replace("$obj", AssertFailures.AsObjectId(obj2)));
        AssertThrown(() => AssertObject(obj2).IsNotSame(obj1))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(262)
            .HasMessage("Expecting be NOT same: $obj".Replace("$obj", AssertFailures.AsObjectId(obj1)));
    }

    [TestCase]
    public void MustFailIsPrimitive()
    {
        AssertThrown(() => AssertObject(1))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(271)
            .HasMessage("ObjectAssert initial error: current is primitive <System.Int32>");
        AssertThrown(() => AssertObject(1.3))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(275)
            .HasMessage("ObjectAssert initial error: current is primitive <System.Double>");
        AssertThrown(() => AssertObject(true))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(279)
            .HasMessage("ObjectAssert initial error: current is primitive <System.Boolean>");
    }

    [TestCase]
    public void OverrideFailureMessage()
        => AssertThrown(() => AssertObject(AutoFree(new Godot.Node()))
                .OverrideFailureMessage("Custom failure message")
                .IsNull())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(287)
            .HasMessage("Custom failure message");

    [TestCase]
    public void InterruptIsFailure()
    {
        // we disable failure reporting until we simulate an failure
        if (ExecutionContext.Current != null)
            ExecutionContext.Current.FailureReporting = false;
        // try to fail
        AssertObject(null).IsNotNull();

        // expect this line will never called because of the test is interrupted by a failing assert
        AssertBool(true).OverrideFailureMessage("This line should never be called").IsFalse();
    }
}
