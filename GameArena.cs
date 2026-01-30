using Nebula;
using Nebula.Utility.Tools;

public partial class GameArena : NetNode3D
{
    public override void _WorldReady()
    {
        base._WorldReady();
        Debugger.Instance.Log(Debugger.DebugLevel.INFO, $"GameArena _WorldReady!");
    }
}
