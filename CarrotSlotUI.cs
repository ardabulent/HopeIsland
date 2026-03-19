using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(AudioSource))] // Otomatik hoparlör ekler
public class CarrotSlotUI : MonoBehaviour
{
    [Header("Görseller (Child Objeler)")]
    public GameObject imgEmpty;   // Boş toprak
    public GameObject imgSprout;  // Filiz
    public GameObject imgCarrot;  // Havuç

    [Header("Ses ve Efektler")]
    public AudioClip plantSound;   // Tohum ekme sesi (Kum/Toprak)
    public AudioClip harvestSound; // Toplama sesi (Pop/Lop)
    public GameObject sparkleEffect; // Toplayınca çıkacak yıldız efekti (FX_Sparkle)

    [Header("Ayarlar")]
    public string slotID = "UI_Slot_1"; // Tarlanın ID'si
    public int growTimeSeconds = 5; // Varsayılan süre

    private AudioSource audioSource; // Ses çalacak bileşen

    void Start()
    {
        // Hoparlörü hazırla
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        CheckStatus(); // Başlarken kontrol et
    }

    // --- ZORLUK AYARI (MOOD) ---
    void OnEnable()
    {
        // GameManager'dan Duygu Durumunu Al
        if (GameManager.Instance != null)
        {
            int mood = GameManager.Instance.currentMoodLevel;

            if (mood == 1) // Yorgun/Sakin
            {
                growTimeSeconds = 2; // Hızlı büyüsün
            }
            else if (mood == 2) // Enerjik
            {
                growTimeSeconds = 10; // Yavaş büyüsün
            }
            else // Normal
            {
                growTimeSeconds = 5;
            }
        }
        CheckStatus();
    }
    // ---------------------------------------------------

    public void OnClickSlot()
    {
        // 1. Havuç Hazırsa -> Topla
        if (imgCarrot.activeSelf)
        {
            Collect();
        }
        // 2. Boşsa -> Ek
        else if (imgEmpty.activeSelf)
        {
            Plant();
        }
        // 3. Büyüyorsa -> Uyarı
        else
        {
            Debug.Log("Henüz büyümedi! Bekle... 🌱");
        }
    }

    void Plant()
    {
        // Tohumu ektiğimiz saati kaydet
        PlayerPrefs.SetString(slotID + "_StartTime", DateTime.Now.ToBinary().ToString());
        PlayerPrefs.Save();

        CheckStatus();

        // --- EKME SESİ ÇAL ---
        if (plantSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(plantSound);
        }
    }

    void Collect()
    {
        // Ödülü Ver
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.AddStars(50);

        // Tarlayı Sıfırla
        PlayerPrefs.DeleteKey(slotID + "_StartTime");

        CheckStatus();

        // --- TOPLAMA SESİ ÇAL ---
        if (harvestSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(harvestSound);
        }

        // --- EFEKT PATLAT (Yıldızlar) ---
        if (sparkleEffect != null)
        {
            // Efekti tarlanın üzerinde oluştur
            Instantiate(sparkleEffect, transform.position, Quaternion.identity);
        }
    }

    void CheckStatus()
    {
        if (PlayerPrefs.HasKey(slotID + "_StartTime"))
        {
            long temp = Convert.ToInt64(PlayerPrefs.GetString(slotID + "_StartTime"));
            DateTime plantTime = DateTime.FromBinary(temp);
            TimeSpan diff = DateTime.Now.Subtract(plantTime);

            if (diff.TotalSeconds >= growTimeSeconds) ShowImage(2); // Havuç olmuş
            else ShowImage(1); // Hala filiz
        }
        else
        {
            ShowImage(0); // Boş
        }
    }

    void ShowImage(int state)
    {
        if (imgEmpty) imgEmpty.SetActive(state == 0);
        if (imgSprout) imgSprout.SetActive(state == 1);
        if (imgCarrot) imgCarrot.SetActive(state == 2);
    }

    void Update()
    {
        // Büyürken sürekli kontrol et
        if (imgSprout != null && imgSprout.activeSelf) CheckStatus();
    }
}