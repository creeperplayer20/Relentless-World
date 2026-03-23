using UnityEngine;

public class ZombieAttackState : IZombieState
{
    private readonly ZombieAI ai;
    private float nextAttackTime;

    public ZombieAttackState(ZombieAI ai) { this.ai = ai; }

    public void Enter()
    {
        ai.Agent.isStopped = true;
        nextAttackTime = 0f;
    }

    public void Tick(float dt)
    {
        if (ai.Target == null)
        {
            ai.SetState(new ZombieIdleState(ai));
            return;
        }

        if (ai.Perception != null && !ai.Perception.IsTargetInRange(ai.Target, ai.Config.attackRange))
        {
            ai.SetState(new ZombieChaseState(ai));
            return;
        }

        Vector3 toTarget = ai.Target.position - ai.transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude > 0.0001f)
            ai.transform.rotation = Quaternion.Slerp(ai.transform.rotation, Quaternion.LookRotation(toTarget), dt * 10f);

        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + ai.Config.attackCooldown;

        var stats = ai.Target.GetComponent<PlayerStats>();
        if (stats != null) stats.TakeDamage(ai.Config.attackDamage);
    }

    public void Exit() { }
}
