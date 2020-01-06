﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reinforced.Tecture.Commands;

namespace Reinforced.Tecture.Services
{
    
    class ServiceManager
    {
        private readonly ActionsQueue _postSaveActions;
        private readonly ActionsQueue _finallyActions;
        private readonly Pipeline _pipeline;
        class ServiceContextEntry
        {
            public Type[] ContextTypes { get; set; }
            public object[] Context { get; set; }
            public TectureService ServiceInstance { get; set; }
        }
        private readonly List<TectureService> _allServices = new List<TectureService>();

        public void OnSave()
        {
            foreach (var srv in _allServices)
            {
                srv.CallOnSave();
            }
        }

        public void OnFinally()
        {
            foreach (var srv in _allServices)
            {
                srv.CallOnFinally();
            }
        }

        private readonly Dictionary<Type, TectureService> _noContextServicesCache = new Dictionary<Type, TectureService>();
        private readonly Dictionary<Type, List<ServiceContextEntry>> _contextServices = new Dictionary<Type, List<ServiceContextEntry>>();
        private readonly Dictionary<Type, LetBuilder> _letCache = new Dictionary<Type, LetBuilder>();

        private TectureService LocateExistingContextService(Type serviceType, Type[] contextTypes, object[] contextValues)
        {
            if (!_contextServices.ContainsKey(serviceType)) return null;
            var entries = _contextServices[serviceType];
            var serviceEntry = entries.FirstOrDefault(d =>
                d.ContextTypes.SequenceEqual(contextTypes) && d.Context.SequenceEqual(contextValues));
            if (serviceEntry == null) return null;
            return serviceEntry.ServiceInstance;
        }

        private void SaveExistingContextService(Type serviceType, Type[] contextTypes, object[] contextValues, TectureService instance)
        {
            var entry = new ServiceContextEntry()
            {
                ContextTypes = contextTypes,
                Context = contextTypes,
                ServiceInstance = instance
            };
            if (!_contextServices.ContainsKey(serviceType))
            {
                _contextServices[serviceType] = new List<ServiceContextEntry>();
            }

            var sd = _contextServices[serviceType];
            sd.Add(entry);
        }

        public ServiceManager(
            ActionsQueue finallyActions, 
            ActionsQueue postSaveActions, 
            Pipeline pipeline)
        {
            _finallyActions = finallyActions;
            _postSaveActions = postSaveActions;
            _pipeline = pipeline;
        }

        private TService CreateService<TService>() where TService : TectureService
        {
            var service = (TService)typeof(TService).InstanceNonpublic();
            service.FinallyActions = _finallyActions;
            service.PostSaveActions = _postSaveActions;
            service.ServiceManager = this;
            
            service.Pipeline = _pipeline;
            return service;
        }

        internal TService CreateWithContext<TService>(Type[] paramTypes, object[] context) where TService : TectureService, IWithContext
        {
            var service = LocateExistingContextService(typeof(TService), paramTypes, context);
            if (service != null) return (TService)service;

            service = CreateService<TService>();
            
            var contextMethod = typeof(TService).GetRuntimeMethod("Context", paramTypes);
            if (contextMethod == null)
                throw new Exception($"Cannot find context method of {typeof(TService).FullName} having arguments of types {string.Join(", ", paramTypes.Select(d => d.Name))} ");
            try
            {
                contextMethod.Invoke(service, context);
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot set context for {typeof(TService)}. See inner exception for details.", ex);
            }

            SaveExistingContextService(typeof(TService), paramTypes, context, service);
            service.CallInit(_pipeline);
            _allServices.Add(service);
            return (TService) service;
        }

        public void DestroyService(TectureService service)
        {
            service.FinallyActions = null;
            service.PostSaveActions = null;
            service.ServiceManager = null;
            service.Pipeline = null;
            
            var st = service.GetType();
            if (service is INoContext)
            {
                if (_noContextServicesCache.ContainsKey(st)) _noContextServicesCache.Remove(st);
            }

            if (service is IWithContext)
            {
                if (_contextServices.ContainsKey(st))
                {
                    var lst = _contextServices[st];
                    lst.RemoveAll(d => d.ServiceInstance == service);
                }
            }

            _allServices.Remove(service);
        }

        public T Do<T>() where T : TectureService, INoContext
        {
            if (_noContextServicesCache.ContainsKey(typeof(T))) return (T)_noContextServicesCache[typeof(T)];
            var service = CreateService<T>();
            _noContextServicesCache[typeof(T)] = service;
            service.CallInit(_pipeline);
            _allServices.Add(service);
            return service;
        }


        public LetBuilder<T> Let<T>() where T : TectureService, IWithContext
        {
            if (_letCache.ContainsKey(typeof(T))) return (LetBuilder<T>)_letCache[typeof(T)];
            var lb = new LetBuilder<T>(this);
            _letCache[typeof(T)] = lb;
            return lb;
        }
    }
}