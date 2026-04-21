namespace GameFoundation.Scripts.UnityAI.BehaviourTree
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using Cysharp.Threading.Tasks.Linq;
    using IGameLogger = GameFoundation.Scripts.Features.Logger.Services.ILogger;
    using LoggerService = GameFoundation.Scripts.Features.Logger.Services.LoggerService;
    using GameFoundation.Scripts.UnityAI.BehaviourTree.Nodes;
    using VContainer.Unity;

    public class BehaviourTreeManager : IBehaviourTreeManager, ITickable
    {
        private readonly IGameLogger logger;
        private Dictionary<string, (bool isRunning, BehaviourTree behaviourTree)> behaviourTrees = new();

        public BehaviourTreeManager(IGameLogger logger = null)
        {
            this.logger = logger ?? new LoggerService();
        }

        public void AddBehaviourTree(string name, BehaviourTree behaviourTree)
        {
            if (this.behaviourTrees.ContainsKey(name))
            {
                throw new ArgumentException($"Behaviour tree with name {name} already exists.");
            }

            this.behaviourTrees[name] = (true, behaviourTree);
        }

        public void RemoveBehaviourTree(string name)
        {
            if (!this.behaviourTrees.ContainsKey(name))
            {
                throw new ArgumentException($"Behaviour tree with name {name} does not exist.");
            }

            this.behaviourTrees.Remove(name);
        }

        public void Start(CancellationToken token)
        {
            this.ProcessBT(token).Forget();
        }

        public void Stop(string name)
        {
            if (!this.behaviourTrees.TryGetValue(name, value: out var tree))
            {
                throw new ArgumentException($"Behaviour tree with name {name} does not exist.");
            }

            var behaviourTree = tree.behaviourTree;
            this.behaviourTrees[name] = (false, behaviourTree);
        }

        public void StopAll()
        {
            foreach (var key in this.behaviourTrees.Keys)
            {
                var behaviourTree = this.behaviourTrees[key].behaviourTree;
                this.behaviourTrees[key] = (false, behaviourTree);
            }
        }

        private async UniTask ProcessBT(CancellationToken token)
        {
            await foreach (var _ in UniTaskAsyncEnumerable.EveryUpdate().WithCancellation(token))
            {
                if (this.behaviourTrees.Count == 0) continue;
                foreach (var tree in this.behaviourTrees)
                {
                    if (!tree.Value.isRunning) continue;
                    tree.Value.behaviourTree.Process();
                }
            }
        }

        public void Tick()
        {
            if (this.behaviourTrees.Count == 0) return;
            foreach (var tree in this.behaviourTrees)
            {
                if (!tree.Value.isRunning) continue;
                try
                {
                    tree.Value.behaviourTree.Process();
                }
                catch (Exception e)
                {
                    this.logger.Error($"Error processing behaviour tree {tree.Key}: {e.Message}");
                    throw;
                }
            }
        }
    }
}
