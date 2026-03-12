using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAdditionalAbilitiesUI : MonoBehaviour
{
    public Button TimeStopButton;
    public Button BetOnRedButton;
    public Button BetOnBlackButton;
    public GameObject BettingParent;

    private TextMeshProUGUI timestopButtonLabel;

    private void Start()
    {
        timestopButtonLabel = TimeStopButton.GetComponentInChildren<TextMeshProUGUI>();
        OnNewRound();
        Player.Instance.additionalPlayerUI = this;
    }

    private const string timestopString = "Time stop ({0} spins)";
    public void OnNewRound()
    {
        TimeStopButton.gameObject.SetActive(Player.Instance.unlocksTimeStop);
        BettingParent.SetActive(Player.Instance.unlocksBlackRedBetting);
        timestopButtonLabel.SetText(string.Format(timestopString, Player.Instance.GetTimeStopSpinCost));
        TimeStopButton.GetComponent<TimestopButton>().UpdateVisuals();
    }
}
