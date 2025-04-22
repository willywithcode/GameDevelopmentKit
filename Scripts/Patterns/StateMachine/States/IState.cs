namespace GameFoundation.Scripts.Patterns.StateMachine.States
{
    using Cysharp.Threading.Tasks;

    public interface IState
    {
        UniTask OnEnter();
        UniTask OnExit();
        void    Tick();
        void    FixedTick();
    }
}