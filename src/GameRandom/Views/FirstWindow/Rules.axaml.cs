using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace GameRandom.Views;

public partial class Rules : Window
{
    private bool _isEnglish = false;
    public Rules()
    {
        InitializeComponent();
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    private Action<string> _onShowContent;

    public void AddListener(Action<string> _onChangeContent) => _onShowContent = _onChangeContent;
    
    private void Close(Object? sender, RoutedEventArgs e)
    {
        _onShowContent?.Invoke("Main");
    }

    private void ToggleLanguage(object? sender, RoutedEventArgs e)
    {
        _isEnglish = !_isEnglish;
        var languageButton = sender as Button;
            
        if (_isEnglish)
        {
            // Переключаем на английский
            languageButton.Content = "РУССКИЙ";
            UpdateTextToEnglish();
        }
        else
        {
            // Переключаем на русский
            languageButton.Content = "ENGLISH";
            UpdateTextToRussian();
        }
    }
     private void UpdateTextToEnglish()
        {
            this.FindControl<TextBlock>("TitleText").Text = "CHALLENGE RULES";
            this.FindControl<TextBlock>("Rule1Text").Text = "1. The initial number of game rolls is 3, but this number may vary according to other rules";
            this.FindControl<TextBlock>("Rule2Text").Text = "2. If a player completes a game faster than the allotted time (time can be checked on Cubiq.ru), one additional roll is added.";
            this.FindControl<TextBlock>("Rule3Text").Text = "3. All games are played on maximum difficulty, hardcore mode with one life adds one more roll when completed.";
            this.FindControl<TextBlock>("Rule4Text").Text = "4. If a player cannot complete the game on maximum difficulty, there is an option to lower the difficulty with the condition that one roll will be removed";
            this.FindControl<TextBlock>("Rule5Text").Text = "5. If a multiplayer game drops, the player needs to play it for 5 hours, in this case you can take someone to play with you, considering that the money for completing the game will be divided among people";
            this.FindControl<TextBlock>("Rule6Text").Text = "6. How payment will be processed: each game has a cost, payment occurs after completing the game";
            this.FindControl<TextBlock>("PricesTitleText").Text = "Prices for completing games:";
            this.FindControl<TextBlock>("Price1Text").Text = "• Game from 1-5 hours: 150 rubles";
            this.FindControl<TextBlock>("Price2Text").Text = "• Game from 5-10 hours: 250 rubles";
            this.FindControl<TextBlock>("Price3Text").Text = "• Game from 10-25 hours: 500 rubles";
            this.FindControl<TextBlock>("Price4Text").Text = "• Game from 25 hours and above: 1000 rubles";
            this.FindControl<TextBlock>("Price5Text").Text = "• Game completed 100%: the full amount of the game is paid to the user";
            this.FindControl<TextBlock>("Rule7Text").Text = "7. When dropping a game, this money is paid not to you but to the common bank, the bank will be 50,000 rubles, from this bank you can take money when completing games, and also replenish it when violating rules (for example, you dropped a game for 150, you put this money in the bank, you can immediately roll another game and try to take this money)";
            this.FindControl<TextBlock>("Rule8Text").Text = "8. When dropping a game, you get minus one roll and randomly select a year";
            this.FindControl<TextBlock>("Rule9Text").Text = "9. Watching a speedrun for faster completion minus one roll and will not add a roll from rule 2";
            this.FindControl<TextBlock>("Rule10Text").Text = "10. If a game was completed earlier in this challenge, it can be rerolled, previous completions from the first part do not count";
            
            // Обновляем текст кнопки закрытия
            var closeButton = this.FindControl<Button>(null); // Нужно добавить x:Name для кнопки закрытия
            // Для кнопки закрытия лучше добавить x:Name="CloseButton" в XAML
        }

        private void UpdateTextToRussian()
        {
            this.FindControl<TextBlock>("TitleText").Text = "ПРАВИЛА ЧЕЛЛЕНДЖА";
            this.FindControl<TextBlock>("Rule1Text").Text = "1. Изначальное количество роллов игр является 3, но это число может варьироваться по другим правилам";
            this.FindControl<TextBlock>("Rule2Text").Text = "2. Если игрок пробегает игру быстрее положенного времени (время можно посмотреть на Cubiq.ru) добавляется ещё один ролл.";
            this.FindControl<TextBlock>("Rule3Text").Text = "3. Все игры играются на максимальной сложности, хардкорный режим с одной жизнью при прохождении добавляет ещё один ролл.";
            this.FindControl<TextBlock>("Rule4Text").Text = "4. Если игрок не может пройти на максимальной сложности, есть возможность понизить сложность с учётом что один ролл будет убран";
            this.FindControl<TextBlock>("Rule5Text").Text = "5. Если падает мультиплеер игра, игроку необходимо проиграть в неё 5 часов, в этом случае можно взять кого нибудь поиграть с собой, с учётом что деньги за прохождение игры будут делится на людей";
            this.FindControl<TextBlock>("Rule6Text").Text = "6. Как будет проходить выплата, каждая игра стоит денег, оплата проходит после прохождения игры";
            this.FindControl<TextBlock>("PricesTitleText").Text = "Цены за прохождения игры:";
            this.FindControl<TextBlock>("Price1Text").Text = "• Игра от 1-5 часов: 150 рублей";
            this.FindControl<TextBlock>("Price2Text").Text = "• Игра от 5-10 часов: 250 рублей";
            this.FindControl<TextBlock>("Price3Text").Text = "• Игра от 10-25 часов: 500 рублей";
            this.FindControl<TextBlock>("Price4Text").Text = "• Игра от 25 и выше: 1000 рублей";
            this.FindControl<TextBlock>("Price5Text").Text = "• Игра пройдена на 100%: выплачивается вся сумма игры пользователю";
            this.FindControl<TextBlock>("Rule7Text").Text = "7. При дропе игры эти деньги выплачиваются не вам а в общий банк, банк будет 50000 рублей, с этого банка вы можете забирать деньги при прохождении, а так же при нарушении пополнять его (к примеру вы дропнули игру на 150, скидываете эти деньги в банк, можете сразу выбить другую игру и попытаться забрать эти деньги)";
            this.FindControl<TextBlock>("Rule8Text").Text = "8. При дропе игры у вас минус один ролл и вы рандомно выбираете год";
            this.FindControl<TextBlock>("Rule9Text").Text = "9. Просмотр спидрана для быстрого прохождения минус один ролл и не будет добавлять ролл с правила 2";
            this.FindControl<TextBlock>("Rule10Text").Text = "10. Если игра была пройдена ранее в этом челлендже то её можно переролить, прошлые прохождения с первой части не засчитывается";
        }
    }
    