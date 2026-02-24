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

    public List<Image> CreateImagesInGrid(int countImage, Grid grid)
    {
        grid.Children.Clear();

        List<Image> images = new List<Image>();

        for (int i = 0; i < countImage; i++)
        {
            images.Add(CreateImageInGrid(grid, i));
        }

        return images;
    }

    public Image CreateImageInGrid(Grid grid, int imageColum)
    {
        Image image = new Image
        {
            Source = AvaloniaService.CreateBitmapFromPath("Assets/avalonia-logo.ico"),
        };

        Grid.SetColumn(image, imageColum);
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