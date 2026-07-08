using GameRandom.Scripts.Enums;

namespace GameRandom.Scripts.WindowServices.ErrorServiceSystem;

public interface IErrorService
{
    public void ShowWindow(object? data = null);
    public void ShowWindow(string message, ErrorEnum errorEnum = ErrorEnum.Error);
}