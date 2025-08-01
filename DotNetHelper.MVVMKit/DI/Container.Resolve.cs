using System.Reflection;

namespace DotNetHelper.MVVMKit.DI
{
    public sealed partial class Container
    {
        /// <summary>
        /// 생성자 분석을 통해 의존성을 재귀적으로 Resolve하고 인스턴스를 생성.
        /// </summary>
        private object CreateWithConstructor(Type type, HashSet<Type>? chain = null)
        {
            chain ??= [];
            if (!chain.Add(type)) // 순환 참조 감지
                throw new InvalidOperationException($"Circular dependency detected: {string.Join(" -> ", chain.Select(t => t.Name))} -> {type.Name}");

            // public 생성자만 가져옴
            ConstructorInfo[] constructors = type.GetConstructors();
            if (constructors.Length == 0) // 생성자가 없으면 예외
                throw new InvalidOperationException("Public constructor not found for type: " + type.FullName);

            // 파라미터 수가 가장 많은 생성자 선택 (보통 DI 용도로 설계된 생성자)
            ConstructorInfo constructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();

            ParameterInfo[] parameters = constructor.GetParameters();
            object[] args = new object[parameters.Length];

            // 생성자 파라미터들을 모두 Resolve
            for (int i = 0; i < parameters.Length; i++)
            {
                var paramType = parameters[i].ParameterType;
                args[i] = ResolveInternal(paramType, chain);
            }

            var instance = constructor.Invoke(args);
            chain.Remove(type);
            return instance;
        }

        /// <summary>
        /// 타입 기반으로 인스턴스를 Resolve (등록 안되어 있으면 생성자 기반 생성)
        /// </summary>
        public object Resolve(Type type) => ResolveInternal(type, null);
        /// <summary>
        /// 타입 기반으로 인스턴스를 Resolve (등록 안되어 있으면 생성자 기반 생성)
        /// </summary>
        public T Resolve<T>() => (T)Resolve(typeof(T));
        /// <summary>
        /// 타입 + 키 기반으로 인스턴스를 Resolve (등록 안되어 있으면 예외)
        /// </summary>
        public object Resolve(Type type, string key) => ResolveNamedInternal(type, key);
        /// <summary>
        /// 타입 + 키 기반으로 인스턴스를 Resolve (등록 안되어 있으면 예외)
        /// </summary>
        public T Resolve<T>(string key) => (T)Resolve(typeof(T), key);

        private object ResolveInternal(Type service, HashSet<Type>? chain)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            if (_singletons.TryGetValue(service, out var lazy)) return lazy.Value;
            if (_transients.TryGetValue(service, out var factory)) return factory();
            // 미등록 타입 : 직접 생성
            return CreateWithConstructor(service, chain);
        }
        private object ResolveNamedInternal(Type type, string key)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            if (_namedSingletons.ContainsKey((type, key))) return _namedSingletons[(type, key)].Value;
            if (_namedTransients.ContainsKey((type, key))) return _namedTransients[(type, key)]();
            throw new InvalidOperationException($"Type {type.FullName} with key '{key}' is not registered.");
        }
    }
}
