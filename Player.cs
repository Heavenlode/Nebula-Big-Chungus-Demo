using Godot;
using Nebula;

public partial class Player : NetNode
{
    [NetProperty(NotifyOnChange = true)]
    public ulong ColorSeed { get; set; } = 0;
    protected virtual void OnNetChangeColorSeed(int tick, ulong oldValue, ulong newValue)
    {
        var random = new RandomNumberGenerator();
        random.Seed = ColorSeed;
        _model.GetActiveMaterial(0).Set("albedo_color", new Color(
            random.Randf(),
            random.Randf(),
            random.Randf()
            ));
    }

    private const float BaseSizeScale = 1f;
    private const float ScoreSizeDivisor = 50f;
    private const float ScaleLerpSpeed = 8f;
    public Label ScoreLabel;
    private Vector3 _targetScale = Vector3.One;

    [NetProperty(NotifyOnChange = true)]
    public int Score { get; set; } = 0;
    protected virtual void OnNetChangeScore(int tick, int oldValue, int newValue)
    {
        if (Network.IsCurrentOwner)
        {
            ScoreLabel?.Text = $"Score: {newValue}";
        }
        UpdateModelScale();
    }

    [Export]
    public Node3D PositionNode;

    [Export]
    private MeshInstance3D _model;

    public override void _WorldReady()
    {
        base._WorldReady();

        if (Network.IsClient && Network.IsCurrentOwner)
        {
            ScoreLabel = Network.CurrentWorld.RootScene.RawNode.GetNode<Label>("%ScoreLabel");
            ScoreLabel?.Text = $"Score: {Score}";

            var camera = new Camera3D();
            camera.Position = new Vector3(0, 10, 0);
            GetNode("Model").AddChild(camera);
            camera.LookAt(new Vector3(0, 0, 0));
        }

        if (Network.IsServer)
        {
            ColorSeed = (ulong)GD.RandRange(0, 1000000);
            var scoreManager = Network.CurrentWorld.RootScene.RawNode.GetNode<GameScoreManager>("GameScoreManager");
            scoreManager?.Players.Add(this);
        }

        UpdateModelScale();
        if (_model != null)
        {
            _model.Scale = _targetScale;
        }
    }

    public override void _Despawn()
    {
        base._Despawn();
        if (Network.IsClient && Network.IsCurrentOwner)
        {
            var manager = Network.CurrentWorld?.RootScene?.RawNode?.GetNode<PlayerSpawner>("PlayerSpawner");
            manager?.OnPlayerDespawn();
            var FinalScoreLabel = Network.CurrentWorld.RootScene.RawNode.GetNode<Label>("%FinalScoreLabel");
            FinalScoreLabel?.Text = $"Final Score: {Score}";

        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_model == null)
        {
            return;
        }

        float t = 1f - Mathf.Exp(-ScaleLerpSpeed * (float)delta);
        _model.Scale = _model.Scale.Lerp(_targetScale, t);
    }

    public Vector3 GetWorldPosition()
    {
        return PositionNode?.GlobalPosition ?? Vector3.Zero;
    }

    public float GetSizeScale()
    {
        return BaseSizeScale + Score / ScoreSizeDivisor;
    }

    public float GetCollisionRadius()
    {
        return GetSizeScale();
    }

    private void UpdateModelScale()
    {
        float scale = GetSizeScale();
        _targetScale = new Vector3(scale, scale, scale);
    }
}
