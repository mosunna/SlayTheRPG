using UnityEngine;

public class Player : Character
{
    //Player specific fields (skill list, Defend-state tracking, etc.)
    //Write out the core combat logic

    public UnityEngine.UI.Image hpBarFill;
    public UnityEngine.UI.Image hpBarGhostFill;
    public TMPro.TMP_Text hpNumberText;

    private void Update()
    {
        UpdateHPDisplay(hpBarFill, hpBarGhostFill, hpNumberText);
    }
}
