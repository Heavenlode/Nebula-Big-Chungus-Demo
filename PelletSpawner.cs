using Godot;
using Nebula;
using Nebula.Serialization;

public partial class PelletSpawner : NetNode
{

    [Export]
    public MultiMeshInstance3D PelletMeshInstance;

    [NetProperty(NotifyOnChange = true, ChunkBudget = 512)]
    public NetArray<Vector3> PelletPositions { get; set; } = new(2000, 2000);
    protected virtual void OnNetChangePelletPositions(int tick, Vector3[] deletedValues, int[] changedIndices, Vector3[] addedValues)
    {
        var multimesh = PelletMeshInstance?.Multimesh;
        if (multimesh == null) return;

        if (!multimesh.UseColors)
        {
            multimesh.UseColors = true;
        }

        if (multimesh.InstanceCount < PelletPositions.Length)
        {
            multimesh.InstanceCount = PelletPositions.Length;
        }

        for (int i = 0; i < changedIndices.Length; i++)
        {
            int idx = changedIndices[i];
            var position = PelletPositions[idx];
            multimesh.SetInstanceTransform(idx, new Transform3D(Basis.Identity, position));
            multimesh.SetInstanceColor(idx, CalculatePelletColor(position));
        }
    }

    private static Color CalculatePelletColor(Vector3 position)
    {
        const float period = 20f;
        float tX = position.X / period;
        float tZ = position.Z / period;
        tX -= Mathf.Floor(tX);
        tZ -= Mathf.Floor(tZ);
        return new Color(tX, 0.35f, tZ, 1f);
    }

    public override void _WorldReady()
    {
        base._WorldReady();

        if (Network.IsClient)
        {
            return;
        }

        for (int i = 0; i < PelletPositions.Capacity; i++)
        {
            PelletPositions[i] = new Vector3(GD.RandRange(-100, 100), 0.1f, GD.RandRange(-100, 100));
        }
    }

    public void RespawnPellet(int index)
    {
        if (index >= 0 && index < PelletPositions.Length)
        {
            PelletPositions[index] = new Vector3(GD.RandRange(-100, 100), 0.1f, GD.RandRange(-100, 100));
        }
    }
}
