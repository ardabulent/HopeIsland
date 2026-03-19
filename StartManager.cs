using UnityEngine;
using UnityEngine.SceneManagement;

public class StartManager : MonoBehaviour
{
    // OYNA Butonuna Baðla
    public void GoToIntroScene()
    {
        // Çocuðu kameraya yolla
        SceneManager.LoadScene("IntroScene");
    }

    // EBEVEYN Butonuna Baðla
    public void GoToParentScene()
    {
        // Veliyi panele yolla
        SceneManager.LoadScene("ParentScene");
    }
}