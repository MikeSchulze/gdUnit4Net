namespace GdUnit4.Tests.Asserts.GodotTypes;

using Godot;

using static Assertions;

[TestSuite]
public class AssertObjectTest
{
    [TestCase]
    [RequireGodotRuntime]
    public void TestObjectEqualityAndComparison()
    {
        // Create objects with same values
        var player1 = AutoFree(new Player("Mage", 15, 75.0f, true));
        var player2 = AutoFree(new Player("Mage", 15, 75.0f, true));
        var player3 = AutoFree(new Player("Mage", 15, 75.0f, false));
        var player4 = player1; // Same reference


        AssertObject(player1).IsNotEqual(player3);
        // Test reference equality
        AssertObject(player1)
            .IsNotSame(player2) // Different instances
            .IsSame(player4) // Same reference
            .IsEqual(player2) // equal by properties
            .IsNotEqual(player3); // Different properties
    }
}

public partial class Player : Node
{
    public Player(string name, int level, float health, bool isAlive)
    {
        Name = name;
        Level = level;
        Health = health;
        IsAlive = isAlive;
    }

    public int Level { get; set; }

    public float Health { get; set; }

    public bool IsAlive { get; set; }
}
