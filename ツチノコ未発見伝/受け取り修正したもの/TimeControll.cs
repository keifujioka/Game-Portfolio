using UnityEngine;
using TMPro;
using System.Threading;

public class TimeControll : MonoBehaviour
{

    //カウントダウン
    [SerializeField]private float countdown = 0.0f;

    //時間を表示するText型の変数
    public TextMeshProUGUI timeText;

    public bool isEnableTimer = true;

    private float _startTime; // 開始時の秒数

    void Start()
    {
        timeText.text = countdown.ToString("f0");

        _startTime = countdown;
    }
    void Update()
    {
        if (isEnableTimer == false) return;
        //時間をカウントする
        countdown -= Time.deltaTime;

        //時間を表示する
        timeText.text = countdown.ToString("f0");

        if (countdown <= 0) GameManager.Instance.GameOver();
    }

    public void StartTimer()
    {
        // countdown = 0.0f;
        countdown = _startTime;
        isEnableTimer = true;
    }

    public void StopTimer()
    {
        isEnableTimer = false;
    }

    // ゴールにかかった時間
    public float GoalTime()
    {
        return _startTime - Mathf.Max(countdown, 0f);
    }
}
