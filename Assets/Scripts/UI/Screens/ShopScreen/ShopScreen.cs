public class ShopScreen : BaseScreen
{
    protected override void OnCloseButtonClicked()
    {
        base.OnCloseButtonClicked();
        // start new game.
        Player.Instance.StartNewRound();
    }
}
