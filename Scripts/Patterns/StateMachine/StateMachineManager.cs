namespace GameFoundation.Scripts.Patterns.StateMachine
{
    using System.Collections.Generic;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using Cysharp.Threading.Tasks.Linq;

    public class StateMachineManager : IStateMachineManager
    {
        private readonly Dictionary<string, StateMachine> stateMachines = new();

        public StateMachine GetStateMachine(string stateMachineName)
        {
            return this.stateMachines[stateMachineName];
        }

        public void StartStateMachine(CancellationToken token)
        {
            this.StartUpdate(token).Forget();
            this.StartFixedUpdate(token).Forget();
            foreach (var stateMachine in this.stateMachines.Values)
            {
                stateMachine.IsRunning = true;
            }
        }

        public void StopStateMachine(string stateMachineName)
        {
            this.stateMachines[stateMachineName].IsRunning = false;
        }

        public void ContinueStateMachine(string stateMachineName)
        {
            this.stateMachines[stateMachineName].IsRunning = true;
        }

        public void StopAll()
        {
            foreach (var stateMachine in this.stateMachines.Values) stateMachine.IsRunning = false;
        }

        public void AddStateMachine(string stateMachineName, StateMachine stateMachine)
        {
            if (this.stateMachines.ContainsKey(stateMachineName)) throw new($"State machine with name {stateMachineName} already exists.");
            this.stateMachines.Add(stateMachineName, stateMachine);
        }

        private async UniTask StartUpdate(CancellationToken token)
        {
            await foreach (var _ in UniTaskAsyncEnumerable.EveryUpdate().WithCancellation(token))
            foreach (var stateMachine in this.stateMachines.Values)
                stateMachine.Tick();
        }

        private async UniTask StartFixedUpdate(CancellationToken token)
        {
            while (true)
            {
                await UniTask.WaitForFixedUpdate(token);
                foreach (var stateMachine in this.stateMachines.Values) stateMachine.FixedTick();
            }
        }
    }
}