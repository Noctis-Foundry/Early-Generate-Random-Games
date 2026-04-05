using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using GameRandom.Scr.Service;
using GameRandom.ViewModels.AdminConfirmSystem;
using GameRandom.ViewModels.FilterGameSystem.Interface;

namespace GameRandom.ViewModels.FilterGameSystem;

public sealed class LoadFilterData : BaseModelService, IFilterGame
{
    public FilterLoadData LoadDataFromJson()
    {
        try
        {
            string categoriesPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Jsons", "categories.json");
            string genresPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Jsons", "genres.json");

            List<string>? categoriesList = new();
            List<string>? genresList = new();
            
            Logger.Info($"Path to json {categoriesPath}");
            
            if (File.Exists(categoriesPath))
            {
                categoriesList = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(categoriesPath));
                
                if (categoriesList is null || categoriesList.Count == 0)
                    Logger.Debug("Failed to load categories list");
            }

            if (File.Exists(genresPath))
            {
                genresList = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(genresPath));
                
                if (genresList is null || genresList.Count == 0)
                    Logger.Debug("Failed to load categories list");
                
            }

            return new FilterLoadData(genresList, categoriesList); //Can load how null. This not throwing app but method have logs if list not loading
        }
        catch (Exception ex)
        {
            Logger.Error($"Error loading filter data: {ex.Message}");
            throw;
        }
    }
}
