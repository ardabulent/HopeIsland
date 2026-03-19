using System;
using UnityEngine;
using HuaweiMobileServices.CloudDB;
using HuaweiMobileServices.Common;
using HmsPlugin; // Eðer bu satýr kýrmýzý kalýrsa, Unity Konsoluna bakacaðýz

public class EmotionCloudManager : MonoBehaviour
{
    private CloudDBZone _cloudDBZone;
    private readonly string _zoneName = "UmutAdasiZone";

    void Start()
    {
        // Ön eki sildik, direkt sýnýf adýyla çaðýrýyoruz
        // Baþýna 'CloudDBZoneConfig.' ekleyerek tam yolu gösteriyoruz:
        HMSCloudDBManager.Instance.OpenCloudDBZone(
            _zoneName,
            CloudDBZoneConfig.CloudDBZoneSyncProperty.CLOUDDBZONE_CLOUD_CACHE,
            CloudDBZoneConfig.CloudDBZoneAccessProperty.CLOUDDBZONE_PUBLIC
        );

        Debug.Log("TheraTech: Bulut baðlantý komutu gönderildi.");
    }

    public void SendEmotionToCloud(string detectedEmotion)
    {
        _cloudDBZone = HMSCloudDBManager.Instance.GetCloudDBZone();

        if (_cloudDBZone == null)
        {
            Debug.LogError("TheraTech: Zone henüz hazýr deðil!");
            return;
        }

        try
        {
            EmotionRecord record = new EmotionRecord();

            record.id = Guid.NewGuid().ToString();
            record.childID = "Cocuk_001";
            record.emotionState = detectedEmotion;
            record.timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            _cloudDBZone.ExecuteUpsert(record);
            Debug.Log("TheraTech: Buluta uçtu: " + detectedEmotion);
        }
        catch (Exception e)
        {
            Debug.LogError("TheraTech Hata: " + e.Message);
        }
    }
}