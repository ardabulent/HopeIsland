using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    // Bu static deðiþken, sahnedeki müzik kutusunun tek patron olduðunu garanti eder.
    private static BackgroundMusic instance;

    void Awake()
    {
        // EÐER ZATEN BÝR MÜZÝK KUTUSU VARSA...
        if (instance != null && instance != this)
        {
            // ...BENÝ YOK ET (Çünkü ikincisine gerek yok, yoksa müzikler üst üste biner)
            Destroy(gameObject);
            return;
        }

        // EÐER YOKSA...
        instance = this;

        // Sahne deðiþirken beni yok etme! (Kritik Komut)
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        AudioSource audio = GetComponent<AudioSource>();

        // Müzik çalmýyorsa baþlat
        if (!audio.isPlaying)
        {
            audio.Play();
        }
    }
}