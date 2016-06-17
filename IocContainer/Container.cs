using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace IocContainer
{
    public class Container : IContainer
    {


        private readonly IDictionary<MappingKey, Func<object>> _mappings;


        public Container()
        {
            _mappings = new Dictionary<MappingKey, Func<object>>();
        }

        public void Register<TFrom, TTo>(string instanceName = null) where TTo : TFrom
        {
            Register(typeof(TFrom), typeof(TTo), instanceName);
        }
        public void Register(Type @from, Type to, string instanceName = null)
        {
            if (to == null)
                throw new ArgumentNullException(nameof(to));

            if (!from.IsAssignableFrom(to))
            {
                var errorMessage =
                    $"Error trying to register the instance: '{@from.FullName}' is not assignable from '{to.FullName}'";

                throw new InvalidOperationException(errorMessage);
            }
            //Func<object> createInstanceDelegate = () => Activator.CreateInstance(to);
            var creator = CreateCtor(to);
            Func<object> createInstanceDelegate = () => creator();
            Register(from, createInstanceDelegate, instanceName);
        }

        private static ObjectActivator CreateCtor(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            var emptyConstructor = type.GetConstructor(Type.EmptyTypes);
            if (emptyConstructor == null) throw new ArgumentNullException(nameof(emptyConstructor));
            var newExp = Expression.New(emptyConstructor);
            var lambda = Expression.Lambda(typeof(ObjectActivator), newExp);
            // Compile it
            return (ObjectActivator)lambda.Compile();
             
        }

        public void Register(Type type, Func<object> createInstanceDelegate, string instanceName = null)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (createInstanceDelegate == null)
                throw new ArgumentNullException(nameof(createInstanceDelegate));

            var key = new MappingKey(type, instanceName);

            if (_mappings.ContainsKey(key))
            {
                const string errorMessageFormat = "The requested mapping already exists - {0}";
                throw new InvalidOperationException(string.Format(errorMessageFormat, key.ToTraceString()));
            }

            _mappings.Add(key, createInstanceDelegate);
        }

        public void Register<T>(Func<T> createInstanceDelegate, string instanceName = null)
        {
            if (createInstanceDelegate == null)
                throw new ArgumentNullException(nameof(createInstanceDelegate));

            var createInstance = createInstanceDelegate as Func<object>;
            Register(typeof(T), createInstance, instanceName);
        }

        public bool IsRegistered(Type type, string instanceName = null)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            var key = new MappingKey(type, instanceName);
            return _mappings.ContainsKey(key);


        }

        public bool IsRegistered<T>(string instanceName = null)
        {
            return IsRegistered(typeof(T), instanceName);
        }

        public object Resolve(Type type, string instanceName = null)
        {
            var key = new MappingKey(type, instanceName);
            Func<object> createInstance;
            if (_mappings.TryGetValue(key, out createInstance))
            {
                var instance = createInstance();
                return instance;
            }
            const string errorMessageFormat = "Could not find mapping for type '{0}'";
            throw new InvalidOperationException(string.Format(errorMessageFormat, type.FullName));

        }

        public T Resolve<T>(string instanceName = null)
        {
            var instance = Resolve(typeof(T), instanceName);
            return (T)instance;
        }
    }
    public delegate object ObjectActivator();
}