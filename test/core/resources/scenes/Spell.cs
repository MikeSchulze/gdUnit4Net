using Godot;

public partial class Spell : Node
{
    [Signal]
    public delegate void SpellExplodeEventHandler();

    private const float SPELL_LIVE_TIME = 1000f;

    private bool _spellFired = false;
    private double _spellLiveTime = 0f;
    private Vector3 _spellPos = Vector3.Zero;

    public override void _Ready()
    {
        Name = "Spell";
    }

    public override void _Process(double delta)
    {
        _spellLiveTime += delta * 1000;

        if (_spellLiveTime < SPELL_LIVE_TIME)
            Move((float)delta);
        else
            Explode();
    }

    private void Move(float delta)
    {
        _spellPos.X += delta;
    }

    private void Explode()
    {
        EmitSignal(Spell.SignalName.SpellExplode, this);
    }
}
