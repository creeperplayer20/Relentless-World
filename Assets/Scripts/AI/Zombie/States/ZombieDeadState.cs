public class ZombieDeadState : IZombieState
{
    private readonly ZombieAI ai;

    public ZombieDeadState(ZombieAI ai) { this.ai = ai; }

    public void Enter()
    {
        ai.Agent.isStopped = true;
        ai.Agent.enabled = false;
    }

    public void Tick(float dt) { }
    public void Exit() { }
}
