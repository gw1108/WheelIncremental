using TMPro;

public class GameOverScreen : BaseScreen
{
    public TextMeshProUGUI MainLabel;

    private const string MainLabelTextFormat = "You earned ${0}!";

    protected override void OnShow()
    {
        base.OnShow();
        MainLabel.SetText(string.Format(MainLabelTextFormat, Player.Instance.CurrentRoundMoney));
        Player.Instance.Money += Player.Instance.CurrentRoundMoney;
    }
}
