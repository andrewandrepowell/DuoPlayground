using Pow.Utilities;
using Pow.Utilities.GO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arch.Core;
using Arch.Core.Extensions;
using Duo.Data;
using System.Diagnostics;
using Pow.Components;

namespace Duo.Managers
{
    public interface IDuoRunnerParent
    {
        public void Initialize(DuoRunner duoRunner);
    }
    public class DuoRunner : GOCustomManager
    {
        private static IDuoRunnerParent _parent;
        private static bool _parentInitialized = false;
        private bool _initialized = false;
        private readonly Queue<Map.PolygonNode> _polygonNodes = [];
        private readonly Queue<Entity> _polygonEntities = [];
        private readonly Dictionary<EntityTypes, IGetEnvironment<Environment>> _entityTypeGetEnvironments = [];
        private interface IGetEnvironment<out T> where T : Environment
        {
            public T GetEnvironment(in Entity entity);
        }
        private class EnvironmentGetter<T> : IGetEnvironment<T> where T : Environment
        {
            public T GetEnvironment(in Entity entity) => entity.Get<GOCustomComponent<T>>().Manager;
        }
        public static void Initialize(IDuoRunnerParent parent)
        {
            Debug.Assert(!_parentInitialized);
            _parent = parent;
            _parentInitialized = true;
        }
        public override void Initialize() 
        {
            base.Initialize();
            Debug.Assert(_parentInitialized);
            Debug.Assert(!_initialized);
            _parent.Initialize(this);
            _initialized = true;
            Globals.Initialize(this);
        }
        public override void Update()
        {

            Debug.Assert(_initialized);
            while (_polygonEntities.TryDequeue(out var entity))
            { 
                var node = _polygonNodes.Dequeue();
                var entityType = Enum.Parse<EntityTypes>(node.Parameters["EntityType"]);
                Debug.Assert(_entityTypeGetEnvironments.ContainsKey(entityType));
                _entityTypeGetEnvironments[entityType].GetEnvironment(entity).Initialize(node);
            }
            base.Update();
        }
        public void Add(Map.PolygonNode node)
        {
            Debug.Assert(_initialized);
            Pow.Globals.Runner.CreateEntity((int)Enum.Parse<EntityTypes>(node.Parameters["EntityType"]), _polygonEntities);
            _polygonNodes.Enqueue(node);
        }
        public void Add<T>(EntityTypes entityType) where T : Environment
        {
            Debug.Assert(!_initialized);
            Debug.Assert(!_entityTypeGetEnvironments.ContainsKey(entityType));
            _entityTypeGetEnvironments.Add(entityType, new EnvironmentGetter<T>());
        }
    }
}
