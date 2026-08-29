using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public GameObject titlePanel;
    public GameObject namePanel;
    public GameObject chooseLevelPanel;
    public TMP_InputField heroNameInput;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();

        if(gameManager != null && gameManager.skipToLevelSelect == true)
        {
            gameManager.skipToLevelSelect = false;

            if(titlePanel != null)
            {
                titlePanel.SetActive(false);
            }

            if(chooseLevelPanel != null)
            {
                chooseLevelPanel.SetActive(true);
            }
        }
    }

    //Called by the Play button's OnClick() on the Title panel
    public void OnPlayButtonPressed()
    {
        if(titlePanel != null)
        {
            titlePanel.SetActive(false);
        }

        if(namePanel != null)
        {
            namePanel.SetActive(true);
        }
    }

    //Called by the Confirm button's OnClick() on the Name panel
    public void OnNameConfirmed()
    {
        string enteredName = "Hero";

        if(heroNameInput != null && heroNameInput.text != "")
        {
            enteredName = heroNameInput.text;
        }

        if(gameManager != null)
        {
            gameManager.heroName = enteredName;
        }

        Debug.Log($"[MenuManager] Hero name set to: {enteredName}");

        if(namePanel != null)
        {
            namePanel.SetActive(false);
        }

        if(chooseLevelPanel != null)
        {
            chooseLevelPanel.SetActive(true);
        }
     
    }

    //Called by each level/boss button's OnClick(). Each button passes its own
    //EncounterData as the fixed argument, set in the Inspector
    public void OnLevelSelected(EncounterData encounter)
    {
        if(gameManager != null && encounter != null)
        {
            gameManager.selectedEncounter = encounter;
        }

        SceneManager.LoadScene("SampleScene");
    }
}