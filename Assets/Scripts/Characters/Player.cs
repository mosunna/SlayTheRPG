using UnityEngine;

public class Player : Character
{
    //Player specific fields (skill list, Defend-state tracking, etc.)
    //Write out the core combat logic

    public UnityEngine.UI.Image hpBarFill;
    public UnityEngine.UI.Image hpBarGhostFill;
    public TMPro.TMP_Text hpNumberText;

    public UnityEngine.UI.Image fpBarFill;
    public UnityEngine.UI.Image fpBarGhostFill;
    public TMPro.TMP_Text fpNumberText;

    private void Update()
    {
        UpdateHPDisplay(hpBarFill, hpBarGhostFill, hpNumberText);
        UpdateFPDisplay(fpBarFill, fpBarGhostFill, fpNumberText);
    }
}
