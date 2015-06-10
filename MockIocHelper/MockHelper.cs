using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Moq;

namespace MockIocHelper
{
    public class MockHelper
    {
        private readonly IDictionary<Type, Mock> _mocks;

        public MockHelper()
        {
            _mocks = new Dictionary<Type, Mock>();
        }

        public T Object<T>()
            where T : class
        {
            return GetMock<T>().Object;
        }

        public Mock<T> GetMock<T>()
            where T : class
        {
            var key = typeof(T);
            return (Mock<T>)(_mocks.ContainsKey(key) ? _mocks[key] : _mocks[key] = new Mock<T>());
        }

        public T Create<T>(params object[] overrides)
            where T : class
        {
            var largestCtor = typeof(T).GetConstructors().Select(c => new
            {
                ParameterCount = c.GetParameters().Count(),
                ConstructorInfo = c
            }).OrderByDescending(c => c.ParameterCount).First().ConstructorInfo;
            var parms = largestCtor.GetParameters();
            var objects = new List<object>();
            foreach (var p in parms)
            {
                var matchedInOverrides = GetMatchFromOverrides(overrides, p.ParameterType);
                if (matchedInOverrides != null)
                {
                    objects.Add(matchedInOverrides);
                    continue;
                }
                GetMethod("GetMock", p.ParameterType).Invoke(this, new object[] { });
                objects.Add(GetMethod("Object", p.ParameterType).Invoke(this, new object[] { }));
            }
            return (T)largestCtor.Invoke(objects.ToArray());
        }

        private static object GetMatchFromOverrides(object[] overrides, Type t)
        {
            return overrides.FirstOrDefault(o => o.GetType().IsSubclassOf(t) || o.GetType() == t || o.GetType().GetInterfaces().Any(i => i == t));
        }

        private MethodInfo GetMethod(string methodName, Type t)
        {
            return GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public, null, new Type[] { }, null).MakeGenericMethod(t);
        }
    }
}
