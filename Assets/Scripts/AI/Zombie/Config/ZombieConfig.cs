using UnityEngine;

[CreateAssetMenu(menuName = "AI/Zombie Config")]
public class ZombieConfig : ScriptableObject
{
    public float walkSpeed = 1.5f;
    public float chaseSpeed = 3.0f;
    public float attackRange = 1.6f;
    public float attackCooldown = 1.0f;
    public int attackDamage = 10;
    public float detectRadius = 15f;
}
