using UnityEngine;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public GameObject titlePanel;
    public GameObject namePanel;
    public TMP_InputField heroNameInput;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
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
    }
}