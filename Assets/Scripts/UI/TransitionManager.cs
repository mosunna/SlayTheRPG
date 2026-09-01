using UnityEngine;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{
    public delegate void OnFadeComplete();

    public Image fadeImage; //Full screen Image (Filled, Radial 360) used as the wipe

    private const float LerpSpeed = 5f; //How quickly fillAmount moves toward its target each frame

    private float targetFillAmount = 1f;
    private bool isAnimating = false;
    private OnFadeComplete pendingCallback;

    //Wipes the screen to fully covered, then invokes the callback (typically a scene load)
    public void FadeOut(OnFadeComplete callback)
    {
        if(fadeImage != null)
        {
            fadeImage.enabled = true;
        }

        targetFillAmount = 1f;
        pendingCallback = callback;
        isAnimating = true;
    }

    //Wipes the screen from fully covered back to clear, then invokes the callback
    public void FadeIn(OnFadeComplete callback)
    {
        if(fadeImage != null)
        {
            fadeImage.enabled = true;
        }

        targetFillAmount = 0f;
        pendingCallback = callback;
        isAnimating = true;
    }

    public bool IsAnimating()
    {
        return isAnimating;
    }

    private void Update()
    {
        if(isAnimating == false || fadeImage == null)
        {
            return;
        }

        fadeImage.fillAmount = Mathf.Lerp(fadeImage.fillAmount, targetFillAmount, Time.deltaTime * LerpSpeed);

        if(Mathf.Abs(fadeImage.fillAmount - targetFillAmount) < 0.01f)
        {
            fadeImage.fillAmount = targetFillAmount;
            isAnimating = false;

            if(targetFillAmount == 0f)
            {
                fadeImage.enabled = false; //Fully revealed, stop blocking raycasts on the UI underneath
            }

            if(pendingCallback != null)
            {
                OnFadeComplete callbackToInvoke = pendingCallback;
                pendingCallback = null;
                callbackToInvoke();
            }
        }
    }
}
