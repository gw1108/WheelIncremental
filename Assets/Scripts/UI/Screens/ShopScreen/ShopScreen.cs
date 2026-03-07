using TMPro;

public class ShopScreen : BaseScreen
{
    public TextMeshProUGUI CurrentMoneyLabel;

    protected override void OnShow()
    {
        base.OnShow();
        CurrentMoneyLabel.SetText("$" + Player.Instance.Money);
    }

    protected override void OnCloseButtonClicked()
    {
        base.OnCloseButtonClicked();
        // start new game.
        Player.Instance.StartNewRound();
    }
}
