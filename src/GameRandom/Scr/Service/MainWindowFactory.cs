using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Labs.Gif;
using Avalonia.Media.Imaging;

namespace GameRandom.Service;

public class MainWindowFactory
{
    public void ChangeGrid(int countImage, Grid grid)
    {
        grid.ColumnDefinitions.Clear();
        grid.Children.Clear();

        for (int i = 1; i <= countImage; i++) //TODO Change to i = 0 and i < countImage
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
    }

    public GridElements CreateButtonInGrid(Grid grid, int countGame)
    {
        Image image = new Image
        {
            Source = AvaloniaService.CreateBitmapFromPath("Assets/avalonia-logo.ico"),
            Name = $"AppImage{countGame}"
        };

        image.Classes.Add("GameImages");

        var button = new Button
        {
            Content = image,
            Name = $"AppButton{countGame}"
        };

        button.Classes.Add("RandomButton");

        Grid.SetColumn(button, countGame);
        grid.Children.Add(button);
        
        return new GridElements(button, image);
    }

    public List<Image> CreateImageInGrid(Grid grid, int count)
    {
        grid.ColumnDefinitions.Clear();
    
        for (int i = 0; i < count; i++)
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        var images = new List<Image>();
    
        for (int i = 0; i < count; i++)
        {
            var image = new Image
            {
                Source = AvaloniaService.CreateBitmapFromPath("Assets/avalonia-logo.ico")
            };

            var border = new Border
            {
                Height = 30,
                MinHeight = 30,
                MaxHeight = 40,
                ClipToBounds = true,
                CornerRadius = new CornerRadius(10),
                Child = image
            };

            Grid.SetColumn(border, i);
            grid.Children.Add(border);
            images.Add(image);
        }

        return images;
    }

    public GifImage CreateAnimatedImage(Grid grid)
    {
        var image = AvaloniaService.CreateGifImageFromPath("Assets/load.gif");
        Grid.SetColumn(image, grid.ColumnDefinitions.Count - 1);
        grid.Children.Add(image);

        return image;
    }
}

public class ButtonContext(Button button, Image buttonImage, byte[] imageBytes)
{
    public Button Button { get; private set; } = button;
    public Image ButtonImage { get; private set; } = buttonImage;
    public byte[] ImageBytes { get; private set; } = imageBytes;
}

public class GridElements(Button button, Image image)
{
    public Button Button { get; set; } = button;
    public Image Image { get; set; } = image;
}