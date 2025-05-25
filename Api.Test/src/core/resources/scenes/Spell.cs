namespace GdUnit4.Tests;

using Godot;

public partial class Spell : Node
{
    [Signal]
    public delegate void SpellExplodeEventHandler(ulong spellId);

    private const float SPELL_LIVE_TIME = 1000f;
    private bool spellExploded;

    private bool spellFired;
    private double spellLiveTime;
    private Vector3 spellPos = Vector3.Zero;

    public override void _Ready()
        => Name = "Spell";

    public override void _Process(double delta)
    {
        spellLiveTime += delta * 1000;

        if (spellLiveTime < SPELL_LIVE_TIME)
            Move((float)delta);
        else
            Explode();
    }

    private void Move(float delta) => spellPos.X += delta;

    private void Explode()
    {
        if (spellExploded)
            return;

        EmitSignal(SignalName.SpellExplode, GetInstanceId());
        QueueFree();
        spellExploded = true;
    }
}
