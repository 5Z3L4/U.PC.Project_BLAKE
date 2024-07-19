using _Project.Scripts.PointsSystem;
using _Project.Scripts.GlobalHandlers;
using UnityEngine;

public class BuyPerkButton : MonoBehaviour
{
    public PerkScriptableObject perk;
    public float perkCost = 0;

    private bool activated = false;
    [SerializeField]
    TMPro.TMP_Text buttonText;

    private void Start()
    {
        if (buttonText != null)
        {
            buttonText.text = string.Format("BUY: {0}", perkCost);
        }

        if(ReferenceManager.PlayerInputController != null)
        {
            var perks = ReferenceManager.PlayerInputController.GetComponent<PlayerPerkManager>();
            if(perks.GetPerkList().Contains(perk))
            {
                if (buttonText != null)
                {
                    buttonText.text = "BOUGHT";
                }
                activated = false;
            }
        }
    }

    public void GiveOrRemove()
    {
        if (activated) return;

        var player = ReferenceManager.BlakeHeroCharacter?.GetComponent<PlayerPerkManager>();
        if (player == null) return;
        if (EnemyDeathMediator.Instance.PlayerCurrencyController.Points < perkCost) return;

        player.AddPerk(perk);
        EnemyDeathMediator.Instance.PlayerCurrencyController.RemovePoints(perkCost);
        EnemyDeathMediator.Instance.Refresh();
        if (buttonText != null)
        {
            buttonText.text = "BOUGHT";
        }
        activated = !activated;
    }
}
