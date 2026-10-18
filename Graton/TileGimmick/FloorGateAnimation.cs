using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorGateAnimation : MonoBehaviour
{
    [SerializeField,Header("FloorGate画像")] private Sprite[] _floorGateSprites;

    [SerializeField,Header("Mapに配置したFloorGateをアタッチ")] private SpriteRenderer _floorGateRenderer;

    [SerializeField,Header("フレーム間隔")] private float _animationTime = 0.1f;

    public bool IsOpen { get; private set; } = false;

    // ドアを開ける
    public void Open()
    {
        if (IsOpen) return;
        IsOpen = true;

        StopAllCoroutines();
        StartCoroutine(OpenAnimation());
    }

    // ドアを閉める
    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;

        StopAllCoroutines();
        StartCoroutine(CloseAnimation());
    }

    // ドアが開くアニメーション
    private IEnumerator OpenAnimation()
    {
        for (int i = 1; i < _floorGateSprites.Length; i++)
        {
            _floorGateRenderer.sprite = _floorGateSprites[i];
            yield return new WaitForSeconds(_animationTime);
        }
    }

    // ドアが閉まるアニメーション
    private IEnumerator CloseAnimation()
    {
        for (int i = _floorGateSprites.Length - 2; i >= 0; i--)
        {
            _floorGateRenderer.sprite = _floorGateSprites[i];
            yield return new WaitForSeconds(_animationTime);
        }
    }
}
