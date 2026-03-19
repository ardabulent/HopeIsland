using UnityEngine;
using UnityEngine.UI;

public class LevelLocker : MonoBehaviour
{
    [Header("Ayarlar")]
    public int unlockPrice = 100; // Kaç yıldız lazım?
    public GameObject lockedImage; // Gri Ok + Kilit Resmi
    public GameObject unlockedImage; // Renkli Ok Resmi

    // Kamerayı kaydıracak olan diğer scriptimize ulaşacağız
    public WorldSwitcher cameraMover;

    private bool isUnlocked = false;

    void Start()
    {
        // Daha önce satın almış mı? Hafızayı kontrol et.
        if (PlayerPrefs.GetInt("World2_Unlocked", 0) == 1)
        {
            UnlockImmediate();
        }
        else
        {
            ShowLockedState();
        }
    }

    public void OnClickButton()
    {
        if (isUnlocked)
        {
            // Zaten açık, direkt git
            cameraMover.GoToNextWorld();
        }
        else
        {
            // Kilitli, satın almaya çalış
            AttemptToUnlock();
        }
    }

    void AttemptToUnlock()
    {
        // Kasada para var mı?
        if (EconomyManager.Instance.SpendStars(unlockPrice))
        {
            // Yetti ve satın aldı!
            UnlockImmediate();

            // Hafızaya kaydet (Bir daha sormasın)
            PlayerPrefs.SetInt("World2_Unlocked", 1);
            PlayerPrefs.Save();

            Debug.Log("Kilit Açıldı! 🎉");
        }
        else
        {
            // Para yetmedi
            Debug.Log("Yetersiz Bakiye! Daha fazla havuç topla.");
            // Buraya ilerde "Dıt dıt" diye hata sesi ekleriz.
        }
    }

    void UnlockImmediate()
    {
        isUnlocked = true;
        lockedImage.SetActive(false);
        unlockedImage.SetActive(true);
    }

    void ShowLockedState()
    {
        isUnlocked = false;
        lockedImage.SetActive(true);
        unlockedImage.SetActive(false);
    }
}