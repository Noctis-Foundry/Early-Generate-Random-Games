namespace GameRandom.Scr.DI;

public abstract class Register
{
    public virtual void Init<T1>(T1 arg1)
    {
        
    }

    public virtual void Init<T1, T2>(T1 arg1, T2 arg2)
    {
        
    }
    
    public virtual void Init<TClass>(TClass instance, params object[] args)
    {
        
    }
}