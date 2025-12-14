namespace GameFoundation.Scripts.EntityManager.Core
{
    using System;
    using System.Collections.Generic;

    public interface IComponentArray
    {
        void Remove(int entityId);
        bool Has(int entityId);
    }

    public class ComponentArray<T> : IComponentArray where T : struct, IComponent
    {
        private T[]   components;
        private int[] entityToIndex;
        private int[] indexToEntity;
        private int   count;

        private const int DefaultCapacity = 64;

        public ComponentArray(int capacity = DefaultCapacity)
        {
            this.components    = new T[capacity];
            this.entityToIndex = new int[capacity];
            this.indexToEntity = new int[capacity];
            this.count         = 0;

            for (var i = 0; i < capacity; i++)
            {
                this.entityToIndex[i] = -1;
            }
        }

        public void Add(int entityId, T component)
        {
            this.EnsureCapacity(entityId);

            if (this.entityToIndex[entityId] != -1)
            {
                this.components[this.entityToIndex[entityId]] = component;
                return;
            }

            var index = this.count;
            this.entityToIndex[entityId] = index;
            this.indexToEntity[index]    = entityId;
            this.components[index]       = component;
            this.count++;
        }

        public ref T Get(int entityId)
        {
            if (entityId >= this.entityToIndex.Length || this.entityToIndex[entityId] == -1)
            {
                throw new KeyNotFoundException($"Entity {entityId} does not have component {typeof(T).Name}");
            }
            return ref this.components[this.entityToIndex[entityId]];
        }

        public bool TryGet(int entityId, out T component)
        {
            if (this.Has(entityId))
            {
                component = this.components[this.entityToIndex[entityId]];
                return true;
            }
            component = default;
            return false;
        }

        public bool Has(int entityId)
        {
            return entityId < this.entityToIndex.Length && this.entityToIndex[entityId] != -1;
        }

        public void Remove(int entityId)
        {
            if (!this.Has(entityId)) return;

            var indexToRemove = this.entityToIndex[entityId];
            var lastIndex     = this.count - 1;
            var lastEntityId  = this.indexToEntity[lastIndex];

            // Swap-remove: move last element to removed position
            this.components[indexToRemove]       = this.components[lastIndex];
            this.indexToEntity[indexToRemove]    = lastEntityId;
            this.entityToIndex[lastEntityId]     = indexToRemove;

            this.entityToIndex[entityId] = -1;
            this.count--;
        }

        public int Count => this.count;

        public ReadOnlySpan<T> AsSpan() => new ReadOnlySpan<T>(this.components, 0, this.count);

        public Enumerator GetEnumerator() => new Enumerator(this);

        private void EnsureCapacity(int entityId)
        {
            if (entityId < this.entityToIndex.Length && this.count < this.components.Length) return;

            var newCapacity = Math.Max(this.entityToIndex.Length, this.components.Length);
            while (newCapacity <= entityId || newCapacity <= this.count)
            {
                newCapacity *= 2;
            }

            Array.Resize(ref this.components, newCapacity);
            Array.Resize(ref this.indexToEntity, newCapacity);

            var oldLength = this.entityToIndex.Length;
            Array.Resize(ref this.entityToIndex, newCapacity);
            for (var i = oldLength; i < newCapacity; i++)
            {
                this.entityToIndex[i] = -1;
            }
        }

        public ref struct Enumerator
        {
            private readonly ComponentArray<T> array;
            private          int               index;

            public Enumerator(ComponentArray<T> array)
            {
                this.array = array;
                this.index = -1;
            }

            public bool MoveNext() => ++this.index < this.array.count;

            public ref T Current => ref this.array.components[this.index];
        }
    }
}
