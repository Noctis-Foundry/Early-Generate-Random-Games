using System;
using System.Threading.Tasks;

namespace GameRandom.Scripts.Service;

public class AppLoading
{
    private const float MaxProgress = 100f;
    
    public float Progress;
    public string Message;
    public Action OnChangeValue;

    public async Task UpdateProgress(float target, float duration, string message = "Loading...")
    {
        Message = message;
        
        var start = Progress;
        var elapsed = 0;

        while (elapsed < duration)
        {
            if (Progress >= target)
            {
                Progress = MaxProgress;
                break;
            }
            
            elapsed += 16;

            var t = elapsed / (float)duration;
            Progress = (
                float.Lerp(start, target, t)
            );

            OnChangeValue?.Invoke();
            
            await Task.Delay(16);
        }

        Progress = target;
    }
} 