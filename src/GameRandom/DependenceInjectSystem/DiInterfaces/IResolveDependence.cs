namespace GameRandom.DependenceInjectSystem.DiInterfaces;

public interface IResolveDependence
{
    public void ResolveInstanceFromClass(object classInstance);
    public void ResolveFiled<TField>(out TField instance) where TField : class;
    public TType TryGetInstance<TType>() where TType : class;
}