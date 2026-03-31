using System.Collections.Generic;

namespace GameRandom.ViewModels.FilterGameSystem;

public class FilterLoadData(List<string> genresList, List<string> categoriesList)
{
    public List<string> GenresList { get; set; } = genresList;
    public List<string> CategoriesList { get; set; } = categoriesList;
}