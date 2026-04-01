using System;
using System.Threading.Tasks;
using GameRandom.Src;
using GameRandom.Src.Enums;

namespace GameRandom.Scr.Service;

public static class Logger //To:Do Connect Error window to logger
{
    
    public static void Info(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(msg);
    }

    public static void Error(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(msg);
    }

    public static void Warning(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(msg);
    }

    public static void Debug(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(msg);
    }
}