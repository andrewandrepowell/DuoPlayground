using Pow.Utilities;
using Pow.Utilities.GO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arch.Core;
using Arch.Core.Extensions;
using Duo.Data;
using Pow.Components;
using System.Diagnostics;
using DuoGlobals = Duo.Globals;
using PowGlobals = Pow.Globals;
using System.Drawing;

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
        private void EnvironmentUpdate()
        {
            Debug.Assert(_initialized);
            while (_polygonEntities.TryDequeue(out var entity))
            {
                var node = _polygonNodes.Dequeue();
                var entityType = Enum.Parse<EntityTypes>(node.Parameters["EntityType"]);
                Debug.Assert(_entityTypeGetEnvironments.ContainsKey(entityType));
                _entityTypeGetEnvironments[entityType].GetEnvironment(entity).Initialize(node);
            }
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
            DuoGlobals.Initialize(this);
        }
        public override void Update()
        {

            Debug.Assert(_initialized);
            EnvironmentUpdate();
            base.Update();
        }
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
