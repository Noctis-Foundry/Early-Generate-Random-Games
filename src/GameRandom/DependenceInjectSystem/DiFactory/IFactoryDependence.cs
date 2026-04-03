namespace DIContainer.DiFactory;

public interface IFactoryDependence
{
    public TInstanceType CreateInstance<TInstanceType>() where TInstanceType : class, new();
}