using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class WorldCard : MonoBehaviour
{
    [Header("Bu Kart Hangi Sahneyi Açacak?")]
    public string targetSceneName;

    private Button btn;
    private RectTransform rect;

    void Start()
    {
        btn = GetComponent<Button>();
        rect = GetComponent<RectTransform>();

        // Butona týklanma olayýný (Event) kodla dinliyoruz
        btn.onClick.AddListener(OnCardClicked);
    }

    void OnCardClicked()
    {
        // Ýki kere üst üste týklanmasýný engelle
        btn.interactable = false;

        // Animasyonu baþlatmasý için Manager'a haber ver
        if (WorldSelectionManager.Instance != null)
        {
            WorldSelectionManager.Instance.SelectWorld(rect, targetSceneName);
        }
        else
        {
            Debug.LogError("Sahnede WorldSelectionManager yok!");
        }
    }
}