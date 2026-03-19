using UnityEngine;
using UnityEngine.EventSystems;

public class MissedClickDetector : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        // Balon dışına tıklandığında hata sinyali gönder
        BalloonSpawner spawner = Object.FindFirstObjectByType<BalloonSpawner>();
        if (spawner != null)
        {
            spawner.OnBalloonClicked(false); // Seriyi bozdur ve yavaşlat
        }
    }
}