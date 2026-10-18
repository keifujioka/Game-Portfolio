using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.iOS;

public class OpenDoor : MonoBehaviour
{
    // ドア画像 4 つをアタッチ 0:閉じている 1～2:開く途中 3:開いている
    [SerializeField, Header("ドアのSpriteをアタッチ")] private Sprite[] _doorSprites;

    [SerializeField, Header("Mapに配置したドアをアタッチ")] private SpriteRenderer _doorRenderer;

    [SerializeField, Header("フレーム間隔")] private float _animationTime = 0.1f;

    // ドアの開閉状態 プロパティ
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
        for (int i = 1; i < _doorSprites.Length; i++)
        {
            _doorRenderer.sprite = _doorSprites[i];
            yield return new WaitForSeconds(_animationTime);
        }
    }

    // ドアが閉まるアニメーション
    private IEnumerator CloseAnimation()
    {
        for (int i = _doorSprites.Length - 2; i >= 0; i--)
        {
            _doorRenderer.sprite = _doorSprites[i];
            yield return new WaitForSeconds(_animationTime);
        }
    }
}
