using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalAnimation : MonoBehaviour
{

    [SerializeField,Header("ゴール画像")] private Sprite[] _sprites;   
    [SerializeField,Header("画像切り替え")] private float _speed = 4f;

    [SerializeField,Header("揺れ幅")] private float _amplitude = 0.1f;  
    [SerializeField,Header("揺れる速度")] private float _frequency = 2f;

    // 変数
    private SpriteRenderer sr; // ゴール画像
    private int index = 0;     // 画像インデックス
    private float timer = 0f; // 画像を切り替えるタイマー
    private Vector3 startPos; // 初期位置

    void Start()
    {
        // SpriteRenderer 取得
        sr = GetComponent<SpriteRenderer>();
        // 初期位置保存
        startPos = transform.position;

        // 画像設定
        if (_sprites != null && _sprites.Length > 0)
            sr.sprite = _sprites[0];
    }

    void Update()
    {
        // 画像切り替え
        timer += Time.deltaTime;
        // 一定時間経過したら画像を切り替え
        if (timer >= 1f / _speed)
        {
            timer = 0f; // タイマーリセット
            // 画像を切り替え
            index = (index + 1) % _sprites.Length;
            // スプライト変更
            sr.sprite = _sprites[index];
        }

        // 上下揺れ
        float offsetY = Mathf.Sin(Time.time * _frequency) * _amplitude;
        // 位置更新
        transform.position = startPos + new Vector3(0, offsetY, 0);
    }
}
