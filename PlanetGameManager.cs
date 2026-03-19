using UnityEngine;
using TMPro;
using System.Collections;

public class PlanetGameManager : MonoBehaviour
{
    [Header("--- PANEL VE HARÝTA GEÇÝÞLERÝ ---")]
    public GameObject mapPanel;
    public GameObject planetGamePanel;

    [Header("--- YILDIZ SÝSTEMÝ ---")]
    public TextMeshProUGUI starText;
    public RectTransform starCounterIcon;
    public GameObject flyingStarPrefab;
    public Transform gameContainer;      // Yýldýzlarýn uçacaðý ana panel

    [Header("--- GEZEGENLER ---")]
    public DraggablePlanet[] allPlanets; // Sahnedeki 4 renkli gezegen

    private int totalStars = 0;
    private int placedPlanets = 0;

    void Start()
    {
        // --- VELÝ PANELÝ TEMELÝ: ORTAK HAVUZDAN YILDIZLARI ÇEK ---
        totalStars = PlayerPrefs.GetInt("Global_TotalStars", 0);
        UpdateStarUI();
    }

    public void OpenGame()
    {
        mapPanel.SetActive(false);
        planetGamePanel.SetActive(true);

        // --- MERKEZ BANKASINDAN GÜNCEL YILDIZLARI ÇEK ---
        // Baþka oyunda yýldýz kazanýp buraya geldiyse, direkt o güncel rakamý görsün!
        totalStars = PlayerPrefs.GetInt("Global_TotalStars", 0);
        UpdateStarUI();

        ResetGame(); // Girdiðimizde her þey yerli yerinde olsun
    }

    public void CloseGame()
    {
        StopAllCoroutines();
        planetGamePanel.SetActive(false);
        mapPanel.SetActive(true);
    }

    // Gezegen kodu doðru yere oturduðunda buraya haber verecek
    public void PlanetPlacedCorrectly(Vector3 startPos)
    {
        placedPlanets++;
        StartCoroutine(FlyStarEffect(startPos, starCounterIcon.position));

        // 4 gezegen de oturdu mu?
        if (placedPlanets >= 4)
        {
            StartCoroutine(RestartRoundDelay());
        }
    }

    IEnumerator RestartRoundDelay()
    {
        yield return new WaitForSeconds(2f); // Çocuk baþardýðýný görsün diye 2 sn bekle
        ResetGame(); // Yeni tura baþla
    }

    public void ResetGame()
    {
        placedPlanets = 0;
        foreach (var planet in allPlanets)
        {
            planet.ResetPlanet();
        }
    }

    IEnumerator FlyStarEffect(Vector3 startPos, Vector3 endPos)
    {
        GameObject flyingStar = Instantiate(flyingStarPrefab, gameContainer);
        flyingStar.transform.position = startPos;
        flyingStar.transform.SetAsLastSibling();

        float elapsed = 0;
        float duration = 0.6f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);
            flyingStar.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        Destroy(flyingStar);

        // --- VELÝ PANELÝ ÝÇÝN ORTAK HAVUZA KAYIT SÝSTEMÝ ---
        totalStars++;
        PlayerPrefs.SetInt("Global_TotalStars", totalStars); // ORTAK HAVUZA KAYDET
        PlayerPrefs.Save(); // Kaydý kesinleþtir
        // --------------------------------------

        UpdateStarUI();
    }

    void UpdateStarUI()
    {
        if (starText != null) starText.text = totalStars.ToString();
    }
}