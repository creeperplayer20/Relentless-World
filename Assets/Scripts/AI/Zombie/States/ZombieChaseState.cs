using UnityEngine;

public class ZombieChaseState : IZombieState
{
    private readonly ZombieAI ai;

    public ZombieChaseState(ZombieAI ai) { this.ai = ai; }

    public void Enter()
    {
        ai.Agent.isStopped = false;
    }

    public void Tick(float dt)
    {
        if (ai.Target == null)
        {
            ai.SetState(new ZombieIdleState(ai));
            return;
        }

        ai.Agent.SetDestination(ai.Target.position);

        if (ai.Perception != null && ai.Perception.IsTargetInRange(ai.Target, ai.Config.attackRange))
            ai.SetState(new ZombieAttackState(ai));
    }

    public void Exit() { }
}
