namespace GameFoundation.Scripts.UnityAI.BehaviourTree.Nodes
{
    using System.Collections.Generic;
    using GameFoundation.Scripts.Extenstions;
    using ZLinq;

    public class RandomSelector : PrioritySelector {
        protected override List<Node> SortChildren() => this.children.Shuffle().AsValueEnumerable().ToList();
        
        public RandomSelector(string name, int priority = 0) : base(name, priority) { }
    }
}