using Godot;
using System;

using System.Threading.Tasks;

public partial class TestScene : Control
{
    [Signal]
    public delegate void PanelColorChangeEventHandler(ColorRect box, Color color);

    private static readonly Color[] COLOR_CYCLE = { Colors.RoyalBlue, Colors.Chartreuse, Colors.YellowGreen };

    private Color _initial_color = Colors.Red;

    private RefCounted? _nullable;

#pragma warning disable CS8618
    public ColorRect _box1;
    private ColorRect _box2;
    private ColorRect _box3;

    public override void _Ready()
    {
        _box1 = GetNode<ColorRect>("VBoxContainer/PanelContainer/HBoxContainer/Panel1");
        _box2 = GetNode<ColorRect>("VBoxContainer/PanelContainer/HBoxContainer/Panel2");
        _box3 = GetNode<ColorRect>("VBoxContainer/PanelContainer/HBoxContainer/Panel3");
        Connect(SignalName.PanelColorChange, Godot.Callable.From<ColorRect, Color>((cr, color) => _OnPanelColorChanged(cr, color)));
        OnlyOneTimeCall();
    }

    private void OnlyOneTimeCall()
    {
    }

    public async void _OnTestPressed(long buttonId)
    {
        Console.WriteLine($"pressed {buttonId}");
        ColorRect box;
        switch (buttonId)
        {
            case 1:
                box = _box1;
                break;
            case 2:
                box = _box2;
                break;
            case 3:
                box = _box3;
                break;
            default:
                return;
        }

        EmitSignal(SignalName.PanelColorChange, box, Colors.Red);
        if (buttonId == 3)
        {
            var timer = GetTree().CreateTimer(1);
            await ToSignal(timer, Timer.SignalName.Timeout);
        }
        EmitSignal(SignalName.PanelColorChange, box, Colors.Gray);
    }

    private void _OnGrayTimeout(ColorRect box)
    {
        EmitSignal(SignalName.PanelColorChange, box, Colors.Gray);
    }

    private Timer CreateTimer(float timeout)
    {
        Timer timer = new Timer();
        AddChild(timer);
        timer.Connect(Timer.SignalName.Timeout, Callable.From(() => _OnTimeout(timer)));
        timer.OneShot = true;
        timer.WaitTime = timeout;
        timer.Start();
        return timer;
    }

    private void _OnTimeout(Timer timer)
    {
        RemoveChild(timer);
        timer.QueueFree();
    }

    public async Task<string> ColorCycle()
    {
        GD.Print("color_cycle initial");
        await ToSignal(CreateTimer(0.5f), Timer.SignalName.Timeout);
        EmitSignal(SignalName.PanelColorChange, _box1, Colors.Red);
        GD.Print("changed to RED");
        await ToSignal(CreateTimer(0.5f), Timer.SignalName.Timeout);
        EmitSignal(SignalName.PanelColorChange, _box1, Colors.Blue);
        GD.Print("changed to BLUE");
        await ToSignal(CreateTimer(0.5f), Timer.SignalName.Timeout);
        EmitSignal(SignalName.PanelColorChange, _box1, Colors.Green);
        GD.Print("changed to GREEN");
        return "black";
    }

    public async void StartColorCycle()
    {
        await ColorCycle();
    }

    private Spell _CreateSpell()
    {
        return new Spell();
    }

    public Spell CreateSpell()
    {
        Spell spell = _CreateSpell();
        spell.Connect(Spell.SignalName.SpellExplode, Callable.From<Spell>((s) => _DestroySpell(s)));
        return spell;
    }

    private void _DestroySpell(Spell spell)
    {
        RemoveChild(spell);
        spell.QueueFree();
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent.IsActionReleased("ui_accept"))
        {
            AddChild(CreateSpell());
        }
    }

    public int Add(int a, int b)
    {
        return a + b;
    }

    private void _OnPanelColorChanged(ColorRect box, Color color)
    {
        box.Color = color;
    }
}
