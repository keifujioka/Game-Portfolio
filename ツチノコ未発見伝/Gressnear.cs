using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class Gressnear : MonoBehaviour
{
    // 草に隠れているオブジェクトリスト
    private HashSet<GameObject> _hiding = new HashSet<GameObject>();

    // 現在隠れているプレイヤーの静的リスト
    private static HashSet<GameObject> _allHidingPlayers = new HashSet<GameObject>();

    // プレイヤーが隠れているかを判定するための static メソッド
    public static bool IsObjectHiding(GameObject target)
    {
        return _allHidingPlayers.Contains(target);
    }

    // 草の揺れ方
    [SerializeField] private float _shakeTime = 0.3f; // 時間
    [SerializeField] private float _shakePower = 0.1f; //強さ
    private Vector3 _startPosition; // 位置
    private Coroutine _shakeCoroutine; //揺れるコルーチン

    [SerializeField] private AudioSource _audio;
    [SerializeField] private AudioClip _se;

    private void Awake()
    {
        // 初期位置を記録
        _startPosition = transform.position;
    }

    // プレイヤーが入ったか
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }
        HidePlayer(other.gameObject);
    }
    // プレイヤーを隠す処理
    private void HidePlayer(GameObject player)
    {
        // すでに隠れている場合は何もしない
        if (_hiding.Contains(player)) return;
        _hiding.Add(player);
        _allHidingPlayers.Add(player); // 全体リストに追加
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0.5f;
            sr.color = c;
        }

        PlaySound(_se);
        Shake();
    }
    // プレイヤーが出たか
    private void OnTriggerExit2D(Collider2D other)
    {
        if(!other.CompareTag("Player")) return;
        GameObject player = other.gameObject;
        if(!_hiding.Contains(player)) return;
        _hiding.Remove(player); 
        if (!IsPlayerInAnyGrass(player))
        {
            _allHidingPlayers.Remove(player); // どの草にもいないなら全体リストから完全削除

            SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1.0f;
                sr.color = c;
            }
            PlaySound(_se);
            Shake();
        }
    }
    private bool IsPlayerInAnyGrass(GameObject player)
    {
        // ステージ上のすべての草コンポーネントを検索
        Gressnear[] allGrasses = FindObjectsByType<Gressnear>(FindObjectsSortMode.None);
        foreach (Gressnear gress in allGrasses)
        {
            // 自分以外の草で、まだプレイヤーをリストに持っている草があれば true
            if (gress != this && gress._hiding.Contains(player))
            {
                return true;
            }
        }
        return false;
    }
    private void Shake()
    {
        // すでに揺れている最中なら
        if(_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
        }

        // 揺らす
        _shakeCoroutine = StartCoroutine(GressShake());
    }

    private IEnumerator GressShake()
    {
        // 経過時間
        float elapsedTime = 0f;

        // 指定時間までループ
        while (elapsedTime < _shakeTime)
        {
            elapsedTime += Time.deltaTime;
            //徐々に揺れを小さく
            float slowly = Mathf.Lerp(_shakePower, 0f, elapsedTime / _shakeTime);
            // X 軸に揺らす幅の計算
            float randomX = Random.Range(-1f,1f) * slowly;
            // 揺らす
            transform.position = _startPosition + new Vector3(randomX, 0f, 0f);

            yield return null; // 1 フレーム待機
        }
        // 元の位置に戻す
        transform.position = _startPosition;
        _shakeCoroutine = null;
    }

    private void PlaySound(AudioClip clip)
    {
        if (_audio != null && clip != null)
        {
            _audio.PlayOneShot(clip);
        }
    }
}
