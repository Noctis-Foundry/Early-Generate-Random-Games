using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace GameRandom.Service;

public class MainWindowFactory
{
    public void ChangeGrid(int countImage, Grid grid)
    {
        grid.ColumnDefinitions.Clear();
        
        for (int i = 1; i <= countImage; i++)
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
    }

    public (List<Button>, List<Image>) CreateButtonInGrid(int countImage, Grid grid)
    {
        grid.Children.Clear();
        List<Button> buttons = new List<Button>();
        List<Image> images = new List<Image>();
        
        for (int i = 0; i < countImage; i++)
        {
            Image image = new Image
            {
                Source = new Bitmap("Assets\\avalonia-logo.ico"),
                Name = $"AppImage{i}"
            };
            
            image.Classes.Add("GameImages");
            images.Add(image);
            
            var button = new Button
            {
                Content = image,
                Name = $"AppButton{i}"
            };
            
            Grid.SetColumn(button, i);
            grid.Children.Add(button);
            
            buttons.Add(button);
        }
        
        return (buttons, images);
    }
}