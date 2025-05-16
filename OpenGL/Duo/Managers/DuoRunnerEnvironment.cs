using Duo.Data;
using Pow.Components;
using Pow.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Arch.Core;
using Arch.Core.Extensions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowGlobals = Pow.Globals;
using System.Collections.ObjectModel;

namespace Duo.Managers
{
    public partial class DuoRunner
    {
        private readonly Queue<Map.PolygonNode> _polygonNodes = [];
        private readonly Queue<Entity> _polygonEntities = [];
        private readonly Dictionary<EntityTypes, IGetEnvironment<Environment>> _entityTypeGetEnvironments = [];
        private readonly List<Environment> _environments = [];
        private readonly Action<Environment> _returnEnvironment;
        private interface IGetEnvironment<out T> where T : Environment
        {
            public T GetEnvironment(in Entity entity);
        }
        private class EnvironmentGetter<T> : IGetEnvironment<T> where T : Environment
        {
            public T GetEnvironment(in Entity entity) => entity.Get<GOCustomComponent<T>>().Manager;
        }
        private void EnvironmentUpdate()
        {
            Debug.Assert(_initialized);
            while (_polygonEntities.TryDequeue(out var entity))
            {
                var node = _polygonNodes.Dequeue();
                var entityType = Enum.Parse<EntityTypes>(node.Parameters["EntityType"]);
                var environment = _entityTypeGetEnvironments[entityType].GetEnvironment(entity);
                _environments.Add(environment);
                environment.Initialize(node, _returnEnvironment);
            }
        }
        public ReadOnlyCollection<Environment> Environments => _environments.AsReadOnly();
        public void AddEnvironment(Map.PolygonNode node)
        {
            Debug.Assert(_initialized);
            PowGlobals.Runner.CreateEntity((int)Enum.Parse<EntityTypes>(node.Parameters["EntityType"]), _polygonEntities);
            _polygonNodes.Enqueue(node);
        }
        public void AddEnvironment<T>(EntityTypes entityType) where T : Environment
        {
            Debug.Assert(!_initialized);
            Debug.Assert(!_entityTypeGetEnvironments.ContainsKey(entityType));
            _entityTypeGetEnvironments.Add(entityType, new EnvironmentGetter<T>());
        }
    }
}
