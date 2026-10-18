using TMPro;
using UnityEngine;

public class TimeScript : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] float limitTime = 99; // 制限時間

    public bool IsTimeUp => limitTime <= 0f; // 外部から参照可能に

    void Update()
    {
        limitTime -= Time.deltaTime;

        if (limitTime < 0)
        {
            limitTime = 0;
        }

        timeText.text = limitTime.ToString("F0"); // 残り時間を整数で表示
    }
}