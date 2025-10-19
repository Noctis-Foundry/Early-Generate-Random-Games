using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameRandom.DataBaseContexts;
using Microsoft.EntityFrameworkCore;

namespace GameRandom.UserSystem;

public class AddNewUserToGroup
{
    public async Task AddNewUser(Users user)
    {
        try
        {
            await using var context = new AppDbContext();

            await context.Users.AddAsync(new Users { Username = $"{user.Username}", ClientID = user.ClientID });
            await context.SaveChangesAsync();

            var allUsers = await context.Users.ToListAsync();

            var userData = allUsers.First(a => a.Username == $"{user.Username}");
            Console.WriteLine($"Added user {userData.Username}");
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
            return;
        }
    }

    public async Task DeleteUser(Users user)
    {
        await using var context = new AppDbContext();
        
        context.Remove(user);
        await context.SaveChangesAsync();
    }
    
    public async Task<List<Users>?> GetAllUsers()
    {
        try
        {
            await using var context = new AppDbContext();
            var list = await context.Users.ToListAsync();
            return list;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
            return null;
        }
    }
}