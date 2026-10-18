using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;


public class FloorChoclat : MonoBehaviour
{
    [SerializeField] private BoxCollider2D _collider2D;
    [SerializeField] private Image _image;
    [SerializeField, Header("消える時間")] private float _disappearTime = 5f;
    [SerializeField, Header("待機")] private float _waitTime = 2f;
    [SerializeField, Header("復元サイズの時間")] private float _restorationTime = 5f;
    [SerializeField, Header("復元フェードインの時間")] private float _fadeTime = 1f; // 追加
    [SerializeField, Header("OffsetMax")] private Vector2 _collderOffsetMax = new Vector2(0f, 0f);
    [SerializeField, Header("OffsetMin")] private Vector2 _collderOffsetMin = new Vector2(0f, -0.5f);
    [SerializeField, Header("SizeMax")] private Vector2 _collderSizeMax = new Vector2(1f, 1f);
    [SerializeField, Header("SizeMin")] private Vector2 _collderSizeMin = new Vector2(1f, 0f);

    // 手動確認用
    //[SerializeField, Range(0, 1)] private float _sizeParam = 1.0f;
    // private float _sizeParamOld = 1.0f;

    // PlayerTag
    private readonly string[] _playerTag = { "Player", "Player1", "Player2", "Player3", "Player4" };

    private bool _isAnimating = false;

    private void Awake()
    {
        _collider2D.enabled = true;
        _collider2D.offset = _collderOffsetMax;
        _collider2D.size = _collderSizeMax;
        _image.fillAmount = 1f;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(WaitForPhysicsStabilization());
    }

    private IEnumerator WaitForPhysicsStabilization()
    {
        yield return null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_isAnimating && IsTargetTag(collision.collider.tag))
        {
            StartCoroutine(AnimateDisappearAndReappear());
        }
    }
   
    private bool IsTargetTag(string tag)
    {
        foreach (string t in _playerTag)
        {
            if (t == tag) return true;
        }
        return false;
    }

    private IEnumerator AnimateDisappearAndReappear()
    {
        _isAnimating = true;
        //消える
        yield return StartCoroutine(AnimateValuse(1f, 0f, _disappearTime));
        // コライダーを無効
        _collider2D.enabled = false;
        //待機
        yield return new WaitForSeconds(_waitTime);
        //復元
        _collider2D.enabled = true;
        _image.fillAmount = 1f; //追加
        _collider2D.offset = _collderOffsetMax;
        _collider2D.size = _collderSizeMax;
        yield return StartCoroutine(FadeIn(_fadeTime)); //ここまで
      //  yield return StartCoroutine(AnimateValuse(0f, 1f, _restorationTime));
        _isAnimating = false;
    }

    private IEnumerator AnimateValuse(float from, float to, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            float t = time / duration;
            float value = Mathf.Lerp(from, to, t);

            _image.fillAmount = value;
            _collider2D.offset =  Vector2.Lerp(_collderOffsetMin, _collderOffsetMax, value);
            _collider2D.size = Vector2.Lerp(_collderSizeMin, _collderSizeMax, value);

            time += Time.deltaTime;
            yield return null;
        }
    }

    // 追加フェードイン
    private IEnumerator FadeIn(float duradion)
    {
        float time = 0f;
        Color color = _image.color;
        color.a = 0f;
        _image.color = color;


        while (time < duradion)
        {
            float t = time / duradion;
            color.a = Mathf.Lerp(0f, 1f, t);
            _image.color = color;

            time += Time.deltaTime;
            yield return null;
        }

        color.a = 1f;
        _image.color = color;
    }

    // Update is called once per frame
    /* void Update()
     {
    　　　手動確認用
         if (_sizeParamOld.Equals(_sizeParam) is false)
         {
             _sizeParamOld = _sizeParam;
             _collider2D.offset = new Vector2(0, Mathf.Lerp(-0.5f, 0f, _sizeParam));
             _collider2D.size = new Vector2(1, Mathf.Lerp(0f, 1.0f, _sizeParam));
             _image.fillAmount = Mathf.Lerp(0f, 1.0f, _sizeParam);
         }
     }*/
}