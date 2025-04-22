namespace GameFoundation.Scripts.Patterns.StateMachine
{
    using System.Threading;

    public interface IStateMachineManager
    {
        public StateMachine GetStateMachine(string              stateMachineName);
        public void         StartStateMachine(CancellationToken token);
        public void         StopStateMachine(string             stateMachineName);
        public void         ContinueStateMachine(string         stateMachineName);
        public void         StopAll();
        public void         AddStateMachine(string stateMachineName, StateMachine stateMachine);
    }
}