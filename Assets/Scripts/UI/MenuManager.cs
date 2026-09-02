using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public GameObject titlePanel;
    public GameObject namePanel;
    public GameObject chooseLevelPanel;
    public TMP_InputField heroNameInput;

    public GameObject endscreenPanel;
    public TMP_Text endingText;

    public TransitionManager transitionManager;

    public AudioSource audioSource;
    public AudioClip titleMusic;
    public AudioClip levelSelectMusic;
    public AudioClip buttonClickSfx;

    private GameManager gameManager;

    //Plays a music clip on the shared AudioSource, replacing whatever is currently playing
    private void PlayMusic(AudioClip clip)
    {
        if(audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    //Plays a one-shot SFX without disturbing whatever music is currently looping on the same AudioSource
    private void PlayClickSfx()
    {
        if(audioSource != null && buttonClickSfx != null)
        {
            audioSource.PlayOneShot(buttonClickSfx);
        }
    }

    private void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();

        if(gameManager != null && gameManager.showEndingScreen == true)
        {
            gameManager.showEndingScreen = false;

            if(titlePanel != null)
            {
                titlePanel.SetActive(false);
            }

            if(endingText != null)
            {
                endingText.text = gameManager.endingMessage;
            }

            if(endscreenPanel != null)
            {
                endscreenPanel.SetActive(true);
            }

            return;
        }

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

            PlayMusic(levelSelectMusic);
            return;
        }

        PlayMusic(titleMusic);
    }

    //Called by the Play button's OnClick() on the Title panel
    public void OnPlayButtonPressed()
    {
        PlayClickSfx();

        if(gameManager != null)
        {
            gameManager.ResetRun();
        }

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
        PlayClickSfx();

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

        PlayMusic(levelSelectMusic);
    }

    //Called by each level/boss button's OnClick(). Each button passes its own
    //EncounterData as the fixed argument, set in the Inspector
    public void OnLevelSelected(EncounterData encounter)
    {
        PlayClickSfx();

        if(gameManager != null && encounter != null)
        {
            gameManager.selectedEncounter = encounter;
        }

        if(transitionManager != null)
        {
            transitionManager.FadeOut(() => SceneManager.LoadScene("Battle Scene"));
        }
        else
        {
            SceneManager.LoadScene("Battle Scene");
        }
    }

    //Called by the Return to Title button's OnClick() on the ending screen. Reloads Main Menu fresh, landing on Title
    public void OnReturnToTitleFromEndingPressed()
    {
        PlayClickSfx();

        if(gameManager != null)
        {
            gameManager.StopPersistentMusic(); //Stops the still-playing boss victory theme so it doesn't overlap with Title music
        }

        SceneManager.LoadScene("Main Menu");
    }
}