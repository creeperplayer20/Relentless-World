using UnityEngine;

public class ZombieIdleState : IZombieState
{
    private readonly ZombieAI ai;

    public ZombieIdleState(ZombieAI ai) { this.ai = ai; }

    public void Enter()
    {
        ai.Agent.isStopped = true;
    }

    public void Tick(float dt)
    {
        if (ai.Target == null) return;

        if (ai.Perception != null && ai.Perception.IsTargetInRange(ai.Target, ai.Config.attackRange))
            ai.SetState(new ZombieAttackState(ai));
        else
            ai.SetState(new ZombieChaseState(ai));
    }

    public void Exit() { }
}
