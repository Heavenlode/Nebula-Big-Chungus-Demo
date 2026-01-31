using Godot;
using Nebula;
using Nebula.Utility.Tools;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PlayerInput
{
    public bool Up;
    public bool Down;
    public bool Right;
    public bool Left;
}

public partial class PlayerController : NetNode3D
{
    [Export]
    public float BaseSpeed { get; set; } = 0.50f;

    [Export]
    public float ScoreSlowdownFactor { get; set; } = 0.05f;

    [Export]
    private Player _player;

    [NetProperty]
    public Vector3 Direction { get; set; } = new Vector3(1, 0, 0);

    public override void _WorldReady()
    {
        base._WorldReady();
        Network.InitializeInput<PlayerInput>();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (!Network.IsWorldReady) return;

        Network.SetInput(new PlayerInput {
            Up = Input.IsActionPressed("ui_up"),
            Down = Input.IsActionPressed("ui_down"),
            Right = Input.IsActionPressed("ui_right"),
            Left = Input.IsActionPressed("ui_left")
        });
    }

    public override void _NetworkProcess(int tick)
    {
        base._NetworkProcess(tick);
        ref readonly var input = ref Network.GetInput<PlayerInput>();
        if (input.Right || input.Left || input.Up || input.Down)
        {
            Direction = new Vector3(input.Up ? -1 : input.Down ? 1 : 0, 0, input.Left ? 1 : input.Right ? -1 : 0).Normalized();
        }

        var score = _player?.Score ?? 0;
        var speed = BaseSpeed / (1f + Mathf.Max(0, score) * ScoreSlowdownFactor);
        Position += Direction * speed;
        Position = new Vector3(Mathf.Clamp(Position.X, -100, 100), Position.Y, Mathf.Clamp(Position.Z, -100, 100));
    }
}
