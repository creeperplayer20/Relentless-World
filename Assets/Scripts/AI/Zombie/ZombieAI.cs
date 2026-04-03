using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieAI : MonoBehaviour
{
    [SerializeField] private ZombieConfig config;
    [SerializeField] private ZombiePerception perception;
    [SerializeField] private Transform target;

    private NavMeshAgent agent;
    private IZombieState currentState;

    public ZombieConfig Config => config;
    public ZombiePerception Perception => perception;
    public Transform Target { get => target; set => target = value; }
    public NavMeshAgent Agent => agent;

    public bool IsDead { get; private set; }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (perception == null) perception = GetComponent<ZombiePerception>();
        if (perception != null && config != null) perception.SetDetectRadius(config.detectRadius);
    }

    private void OnEnable()
    {
        SetState(new ZombieIdleState(this));
    }

    private void Update()
    {
        if (IsDead) return;

        if (target == null && perception != null)
            target = perception.AcquireTarget();

        currentState?.Tick(Time.deltaTime);
    }

    public void SetState(IZombieState next)
    {
        if (next == null) return;
        currentState?.Exit();
        currentState = next;
        currentState.Enter();
    }

    public void Die()
    {
        if (IsDead) return;
        IsDead = true;
        SetState(new ZombieDeadState(this));
    }
}
