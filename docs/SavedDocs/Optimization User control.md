
# Change initialization pipeline for user controls. 

- Add lazy load for user controls
- Clear user controls data after closing


Delegate lazy load example
---
```C# 
private Register<String, Delegate> _lazyRegister = new();

private void InitializeUserControls(){
    _lazyRegister.RegisterNewObject("Roll", DelegateSwitchFactory<RollGame>(_selectorAction));  
    _lazyRegister.RegisterNewObject("Table", DelegateSwitchFactory<GameTable>(_selectorAction));  
    _lazyRegister.RegisterNewObject("Profile", DelegateSwitchFactory<ProfileContent>(_selectorAction));
}

private Navigate(){
if (_lazyRegister.GetObjectFromRegister(nameControl, out var @delegate))  
{  
    @delegate?.Invoke();  
}
}

private RefControlDelegate DelegateSwitchFactory<TUserControl>(Action<string> switchAction) TODO upgrade lifetime for users control   
    where TUserControl : UserControl, IAddListener, new()  
{  
    RefControlDelegate del = delegate() //Sending ControlMain.Control  
    {  
        var newClass = new TUserControl();  
        newClass.AddListener(switchAction);  
        _oldControl = newClass;  
        Console.WriteLine($"Delegate is work. Created class {typeof(TUserControl).Name}." +  
                          $" new class name: {newClass.Name}");  
        ControlMain.Content = newClass;  
    };    return del;  
}
```