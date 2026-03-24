// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
using System;
using System.Collections.Generic;

using GdUnit4.Core.TestExtensions;

using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.TestExtensions;

[TestSuite]
public class ParameterStoreTest
{
    [TestCase]
    public void TestAddAndRetrieveString()
    {
        var store = new ParameterStore();
        store.Add("key", "hello");
        AssertString(store.Value<string>("key")).IsEqual("hello");
    }

    [TestCase]
    public void TestAddAndRetrieveInt()
    {
        var store = new ParameterStore();
        store.Add("n", 42);
        AssertInt(store.Value<int>("n")).IsEqual(42);
    }

    [TestCase]
    public void TestAddAndRetrieveComplexType()
    {
        var store = new ParameterStore();
        var list = new List<int> { 1, 2, 3 };
        store.Add("list", list);
        AssertThat(store.Value<List<int>>("list")).IsEqual(list);
    }

    [TestCase]
    public void TestAddOverwritesExistingKey()
    {
        var store = new ParameterStore();
        store.Add("key", "first");
        store.Add("key", "second");
        AssertString(store.Value<string>("key")).IsEqual("second");
    }

    [TestCase]
    [ThrowsException(typeof(InvalidOperationException))]
    public void TestValueThrowsForMissingKey()
    {
        var store = new ParameterStore();
        store.Value<string>("missing");
    }

    [TestCase]
    [ThrowsException(typeof(InvalidOperationException))]
    public void TestValueThrowsNullWhenTypeMismatch()
    {
        var store = new ParameterStore();
        store.Add("key", "a string");
        store.Value<int?>("key");
    }

    [TestCase]
    public void TestCountIsZeroInitially()
    {
        var store = new ParameterStore();
        AssertInt(store.Count()).IsEqual(0);
    }

    [TestCase]
    public void TestCountIncreasesAfterAdd()
    {
        var store = new ParameterStore();
        store.Add("a", 1);
        store.Add("b", 2);
        AssertInt(store.Count()).IsEqual(2);
    }

    [TestCase]
    public void TestCountDoesNotIncreaseWhenOverwritingKey()
    {
        var store = new ParameterStore();
        store.Add("key", "first");
        store.Add("key", "second");
        AssertInt(store.Count()).IsEqual(1);
    }

    [TestCase]
    public void TestRemoveReturnsAndDeletesValue()
    {
        var store = new ParameterStore();
        store.Add("key", "value");

        var removed = store.Remove<string>("key");

        AssertString(removed).IsEqual("value");
        AssertInt(store.Count()).IsEqual(0);
    }

    [TestCase]
    public void TestRemoveReturnsNullForMissingKey()
    {
        var store = new ParameterStore();
        AssertObject(store.Remove<string>("missing")).IsNull();
    }

    [TestCase]
    public void TestRemoveReturnsNullOnTypeMismatch()
    {
        var store = new ParameterStore();
        store.Add("key", "a string");

        var result = store.Remove<int?>("key");

        // Key should not be removed when the type does not match
        AssertObject(result).IsNull();
        AssertInt(store.Count()).IsEqual(1);
    }

    [TestCase]
    public void TestRemoveDecreasesCount()
    {
        var store = new ParameterStore();
        store.Add("a", 1);
        store.Add("b", 2);
        store.Remove<int>("a");

        AssertInt(store.Count()).IsEqual(1);
    }

    [TestCase]
    public void TestValueFallsBackToParentStore()
    {
        var parent = new ParameterStore();
        parent.Add("key", "parent-value");

        var child = new ParameterStore(parent);

        AssertString(child.Value<string>("key")).IsEqual("parent-value");
    }

    [TestCase]
    public void TestChildValueTakesPrecedenceOverParent()
    {
        var parent = new ParameterStore();
        parent.Add("key", "parent-value");

        var child = new ParameterStore(parent);
        child.Add("key", "child-value");

        AssertString(child.Value<string>("key")).IsEqual("child-value");
        AssertString(parent.Value<string>("key")).IsEqual("parent-value");
    }

    [TestCase]
    [ThrowsException(typeof(InvalidOperationException))]
    public void TestRemoveCascadesToParent()
    {
        var parent = new ParameterStore();
        parent.Add("key", "parent-value");

        var child = new ParameterStore(parent);

        // Remove on child should remove from parent
        child.Remove<string>("key");

        // After removal from child store, parent should throw if retrieving value
        parent.Value<string>("key");
    }

    [TestCase]
    public void TestChildCountIncludesParentEntries()
    {
        var parent = new ParameterStore();
        parent.Add("parentKey", "x");

        var child = new ParameterStore(parent);
        child.Add("childKey", "y");

        // Count reflects only the child's own store
        AssertInt(child.Count()).IsEqual(2);
    }

    [TestCase]
    [ThrowsException(typeof(InvalidOperationException))]
    public void TestValueThrowsWhenKeyAbsentFromBothStores()
    {
        var parent = new ParameterStore();
        var child = new ParameterStore(parent);

        child.Value<string>("missing");
    }

    [TestCase]
    public void TestMultiLevelParentFallback()
    {
        var grandparent = new ParameterStore();
        grandparent.Add("key", "grandparent-value");

        var parent = new ParameterStore(grandparent);
        var child = new ParameterStore(parent);

        AssertString(child.Value<string>("key")).IsEqual("grandparent-value");
    }
}
