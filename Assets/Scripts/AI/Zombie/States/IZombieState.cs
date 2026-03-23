public interface IZombieState
{
    void Enter();
    void Tick(float dt);
    void Exit();
}
