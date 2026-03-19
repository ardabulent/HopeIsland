using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

// Bu satır, scripti attığın objeye otomatik AudioSource ekler
[RequireComponent(typeof(AudioSource))]
public class ObjectJuice : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Hareket Ayarları")]
    public float shrinkFactor = 0.9f; // Basınca %90 küçülsün
    public float animationSpeed = 0.1f; // Hareket hızı

    [Header("Ses Ayarları")]
    public AudioClip clickSound; // Sesi buraya sürükleyeceğiz

    private Vector3 originalScale;
    private AudioSource audioSource;

    void Start()
    {
        originalScale = transform.localScale;
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false; // Oyun başlayınca kendiliğinden ötmesin
    }

    // Dokununca (Basılınca)
    public void OnPointerDown(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateScale(originalScale * shrinkFactor)); // Küçül

        // Ses varsa çal
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    // Bırakınca (Çekince)
    public void OnPointerUp(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateScale(originalScale)); // Eski haline dön
    }

    // Yumuşak hareket motoru
    IEnumerator AnimateScale(Vector3 targetScale)
    {
        float timer = 0;
        Vector3 startScale = transform.localScale;

        while (timer < animationSpeed)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, targetScale, timer / animationSpeed);
            yield return null;
        }
        transform.localScale = targetScale;
    }
}