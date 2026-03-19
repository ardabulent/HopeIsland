using UnityEngine;
using UnityEngine.SceneManagement; // Sahne geçişleri için şart!

public class SceneNavigator : MonoBehaviour
{
    // 🏠 1. Ana Menüye Dön (En Sık Kullanacağın)
    public void GoToStartScene()
    {
        SceneManager.LoadScene("StartScene");
    }

    // 🔙 2. İstediğin Sahneye Git (Esnek Kullanım)
    // Butonun içinden sahne ismini (örn: "ParentScene") elle yazabilirsin.
    public void GoToSpecificScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // 🔄 3. Oyunu Yeniden Başlat (Aynı Sahneyi Yükle)
    public void RestartCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}