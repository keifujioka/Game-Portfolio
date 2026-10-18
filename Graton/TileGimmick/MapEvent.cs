using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class MapEvent : MonoBehaviour
{
    // Tilemap をアタッチ
    [SerializeField,Header("イベントタイル")] private Tilemap _tile;
    // キャラの Transform
    [SerializeField, Header("キャラクターTransform")] private Transform _characterTransform;
    // キャラの Renderer
    [SerializeField, Header("キャラクターRenderer")] private SpriteRenderer _characterRenderer;
    [SerializeField, Header("消える秒数")] private float fadeTime = 1f;　// 消える秒数
    [SerializeField, Header("復元の秒数")] private float respawnTime = 1.5f; // 復元する秒数
    // 落とし穴に落ちる演出
    [SerializeField, Header("落ちる時下に下がる")] private float _down = -0.3f;
    [SerializeField, Header("落ちる時の大きさ")] private float _scale = 1f;
    // 判定を遅らせて完全に乗ってから消えるように
    [SerializeField, Header("落とし穴の判定を遅らせる")] private float _delay = 1f;
         
    // ゴールテキストをアタッチ
    [SerializeField, Header("ゴール時のテキスト")] private TextMeshProUGUI _goalText;
    // ゴール演出の表示時間
    [SerializeField, Header("ゴールの演出時間")] private int _stopScene = 3;
    // 遷移するシーン名
    [SerializeField, Header("シーン遷移の時間")] private string _sceneName;

    // SE関係
    [SerializeField, Header("水を飲むSE")] private AudioClip _growSE;
    [SerializeField, Header("水を出す")] private AudioClip _saallSE;
    [SerializeField, Header("落ちるSE")] private AudioClip _holeSE;
    [SerializeField, Header("物を落とす")] private AudioClip _objectFall;
    [SerializeField, Header("床ゲートSE")] private AudioClip _floorGateSE;
    [SerializeField, Header("SEの再生時間")] private float _seTime = 1.0f;
    // AudioSource を取得
    AudioSource _audioSource;

    // 落とし穴に落ちた時の画像変更
    // EventTile で設定した画像に変更
    [SerializeField,Header("最初に落ちた画像岩")] private TileBase _firstRock;
    [SerializeField, Header("最初に落ちた画像木箱")] private TileBase _firstWood;
    [SerializeField, Header("2回目に落ちた画像岩")] private TileBase _secondRock;
    [SerializeField, Header("2回目に落ちた画像木箱")] private TileBase _secondWood;

    // FloorGateAnimation をアタッチ
    [SerializeField, Header("Mapに配置したFloorGate")] private FloorGateAnimation _floorGateAnimation;

    // 体重変化の関数呼び出し用 仮の実装になります
    [SerializeField] private PlayerView _playerView;

    // 大谷君が制作した StageManager を参照
    [SerializeField] private StageManager _stageManager;

    private bool fading = false; // フェード
    private Vector3 firstPosition; // 初期位置

    // Mapにある落とし穴が埋まった位置を記録(落とし穴を増やしても大丈夫なように)
    private Dictionary<Vector3Int, int> holeSpaces = new Dictionary<Vector3Int, int>();

    // 二重登録しないよう、落下しているかを管理
    private HashSet<Transform> _fallObject = new HashSet<Transform>();

    private bool _floorGateTriggered = false; // floorGateが発動したか

    private bool _floorGateSEPlaying = false; // FloorGateのSEが再生中か

    private bool _playerFall = false; // プレイヤーの落下フラグ

    private void Start()
    {
        if (_characterTransform == null)
        {
            enabled = false;
            return;
        }
        if (_characterRenderer == null)
        {
            _characterRenderer = _characterTransform.GetComponent<SpriteRenderer>();

            if (_characterRenderer == null)
            {
                enabled = false;
                return;
            }
        }
        firstPosition = _characterTransform.position;

        // ゴール Text を非表示にする
        if (_goalText != null) _goalText.gameObject.SetActive(false);

        // StageManager を取得
        if (_stageManager == null)
            _stageManager = FindObjectOfType<StageManager>();

        // AudioSource を取得
        _audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (fading) return; // フェード中他の処理無効
        if (Keyboard.current == null) return;

        if(_playerFall) return;

        // キャラの現在地から Tile 座標を取得
        Vector3Int cellPos = _tile.WorldToCell(_characterTransform.position);
       // Debug.Log(cellPos);

        TileBase tile = _tile.GetTile(cellPos);

        if (tile is EventTile eventTile)
        {
            Debug.Log(eventTile);
            switch (eventTile.eventType)
            {
                // リーダーの要望で Space キー 又は Enter キーで発動するように変更 2026_01_21 大谷
                case EventTile.EventType.Grow:
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                    {
                        Debug.Log("Spaceキーを押した");
                        GrowSmall(eventTile.eventType);
                        _audioSource.PlayOneShot(_growSE);
                    }
                    break;
                case EventTile.EventType.Small:
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                    {
                        Debug.Log("Spaceキーを押した");
                        GrowSmall(eventTile.eventType);
                        _audioSource.PlayOneShot(_saallSE);
                    }
                    break;

                case EventTile.EventType.FloorGate:
                    FloorGate(_characterTransform, _playerView, true);
                    break;

                case EventTile.EventType.Hole:
                    if(!holeSpaces.ContainsKey(cellPos)) // holeSpaces に cellPos がないなら
                    {
                        holeSpaces[cellPos] = 2; // 初期値
                        Debug.Log(holeSpaces);
                    }
                    if (holeSpaces[cellPos] > 0) // holeSpaces が 0 より大きいなら
                    {
                        _playerFall = true;
                        StartCoroutine(Delay(true,null,cellPos));
                    }
                    break;

                case EventTile.EventType.Goal:
                    Debug.Log("クリア");
                    Goal();
                    break;

                default:
                    break;
            }
        }
        // ステージにある木箱、岩のどれを落としてもいいように
        // GimmickList を参照して落とす
        // SEを追加したら動かない時があったので変更
        foreach (var gimmick in _stageManager.GimmickList)
        {
            // gimmick が null または 非アクティブならスキップ
            if (gimmick == null || gimmick.gameObject.activeSelf == false) continue;

            Transform obj = gimmick.transform;

            // Object の現在地から Tile 座標を取得
            Vector3Int objCellPos = _tile.WorldToCell(obj.position);
            TileBase objTile = _tile.GetTile(objCellPos);

            if (objTile is EventTile objEventTile)
            {
                if (objEventTile.eventType == EventTile.EventType.FloorGate)
                {
                    // FloorGate に落とす
                    FloorGate(obj, _playerView, false);
                }

                // 現在の Tile が EventTile で Hole なら
                else if (objEventTile.eventType == EventTile.EventType.Hole)
                {
                    // holeSpaces に objCellPos が存在しなければ追加
                    if (!holeSpaces.TryGetValue(objCellPos, out int objSpace))
                    {
                        objSpace = 2; // 2つまでオブジェクトを落とせる
                        holeSpaces[objCellPos] = objSpace;
                    }
                    // 落下中ならスキップする
                    if(_fallObject.Contains(obj))
                    {
                        continue;
                    }
                    // holeSpace が 0 より大きいなら
                    if (holeSpaces[objCellPos] > 0)
                    {
                        StartCoroutine(Delay(false, obj, objCellPos));
                    }
                }
            }
        }
    }

    // 大きく、小さくする処理
    private void GrowSmall(EventTile.EventType eventType)
    {
        switch (eventType)
        {
            case EventTile.EventType.Grow:
                _playerView.SetFatStatus(true);
                break;
                case EventTile.EventType.Small:
                _playerView.SetFatStatus(false);
                break;
        }
    }

    /// <summary>
    /// 穴に落ちた時の処理
    /// </summary>
    /// <param name="isPlayer">プレイヤーかの判別</param>
    /// <param name="objTransorm">落下オブジェクトのTransorm</param>
    /// <param name="holePos">落下処理が発生した場所(座標)</param>
    private IEnumerator Pitfall(bool isPlayer, Transform objTransorm = null ,Vector3Int? holePos = null)
    {
        if (isPlayer)
        {
            _audioSource.PlayOneShot(_holeSE);
            fading = true;

            Color startColor = _characterRenderer.color;
            Color transparent = new Color(startColor.r, startColor.g, startColor.b, 0f);

            float elapsed = 0f;

            // 穴に落ちる演出
            Vector3 startPos = _characterTransform.position;
            Vector3 endPos = startPos + new Vector3(0, _down, 0); // 下に下げる

            Vector3 startScale = _characterTransform.localScale;
            Vector3 endScale = startScale * _scale; // 縮める

            // 下に下げ、縮小して消える
            while (elapsed < fadeTime)
            {
                
                float t = elapsed / fadeTime;
                // 透明に
                _characterRenderer.color = Color.Lerp(startColor, transparent, t);
                // 下に下げる
                _characterTransform.position = Vector3.Lerp(startPos, endPos, t);
                // 縮小
                _characterTransform.localScale = Vector3.Lerp(startScale, endScale, t);

                elapsed += Time.deltaTime;
                yield return null;
            }
            // 最後の状態
            _characterRenderer.color = transparent;

            _characterTransform.position = endPos;
            _characterTransform.localScale = endScale;

            // 数秒待機 (シーン再読み込みに変更したので不要なら削除)
            yield return new WaitForSeconds(respawnTime);

            // シーンを再読み込み
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            _audioSource.PlayOneShot(_objectFall);
            // 落とし穴が埋まっていないかの確認
            if (holePos.HasValue)
            {
                if(!holeSpaces.ContainsKey(holePos.Value))
                {
                    objTransorm.gameObject.SetActive(false);
                    // 落下が終わったら登録を解除する
                    _fallObject.Remove(objTransorm);
                    yield break;
                }

                // 落とし穴での処理
                holeSpaces[holePos.Value]--; //  holeSpaces から 1 引く
                var gimmick = objTransorm.GetComponent<GimmickController>();
                if (gimmick != null)
                {
                    // 落とした回数で画像を変える
                    if (holeSpaces[holePos.Value] == 1) // 1回目
                    {
                        // SetTile を使用してタイルの画像を変更
                        // SetTile は Tilemap クラスの画像を変更する関数
                        // 場所は holePos で指定(落とし穴の位置) 画像は _firstRock または _firstWood
                        if (gimmick._gimmickData.MyGimmickType == GimmickData.GimmickType.Rock)
                            _tile.SetTile(holePos.Value, _firstRock); // タイルの画像を岩に
                        else
                            _tile.SetTile(holePos.Value, _firstWood); // タイルの画像を木箱に
                    }
                    else if (holeSpaces[holePos.Value] == 0) // 2回目
                    {
                        // SetTile を使用してタイルの画像を変更
                        // SetTile は Tilemap クラスの画像を変更する関数
                        // 場所は holePos で指定(落とし穴の位置) 画像は _firstRock または _firstWood
                        if (gimmick._gimmickData.MyGimmickType == GimmickData.GimmickType.Rock)
                            _tile.SetTile(holePos.Value, _secondRock); // タイルの画像を岩に
                        else
                            _tile.SetTile(holePos.Value, _secondWood);  // タイルの画像を木箱に
                    }
                }
            }
            // プレイヤーと同じ落ち方 岩木箱用
            // 落とし穴、フロアゲート共通の落ち方
            SpriteRenderer sr = objTransorm.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color startColor = sr.color;
                Color transparent = new Color(startColor.r, startColor.g, startColor.b, 0f);

                Vector3 startPos = objTransorm.position;
                Vector3 endPos = startPos + new Vector3(0, _down, 0);

                Vector3 startScale = objTransorm.localScale;
                Vector3 endScale = startScale * _scale;

                float elapsed = 0f;
                while (elapsed < fadeTime)
                {
                    float t = elapsed / fadeTime;
                    sr.color = Color.Lerp(startColor, transparent, t);
                    objTransorm.position = Vector3.Lerp(startPos, endPos, t);
                    objTransorm.localScale = Vector3.Lerp(startScale, endScale, t);
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                sr.color = transparent;
                objTransorm.position = endPos;
                objTransorm.localScale = endScale;
            }
            // オブジェクトを消す
            //Destroy(objTransorm.gameObject); // 20251217 オブジェクトを消すと不都合なので仕様変更
            objTransorm.gameObject.SetActive(false);
            _fallObject.Remove(objTransorm);
        }
    }

    // Pitfall の判定を遅らせる
   private IEnumerator Delay(bool isPlayer,Transform obj, Vector3Int cellPos)
   {
        if(!isPlayer)
        {
            // 岩、木箱が二重に落下しないよう
            if(_fallObject.Contains(obj))
                yield break;
            _fallObject.Add(obj); // 落下中フラグ
        }
        // 一定時間待つ
        yield return new WaitForSeconds(_delay);
        // プレイヤーかどうか、　木箱 岩の中心を落とし穴の中心と一致するよう補正
        Vector3 pos = (isPlayer ? _characterTransform.position : obj.position) + new Vector3(0, 0.6f, 0);
        // タイルマップの座標に
        Vector3Int now = _tile.WorldToCell(pos);

        // タイルの上に乗ったら
        if (now == cellPos)
        {
            // 落下
            StartCoroutine(Pitfall(isPlayer, obj, cellPos));
        }
    }

    // Player ならリスタートする
    // Player 以外なら消す
    /// <summary>
    /// プレイヤーが Fat 又は岩なら落ちる
    /// </summary>
    /// <param name="target">位置や名前を参照</param>
    /// <param name="playerView">Playerの状態を参照</param>
    /// <param name="isPlayer">乗ったオブジェクトが Player かどうかの判定</param>
    private void FloorGate(Transform target,PlayerView playerView, bool isPlayer)
    {
        // floorg = Floor Gate の位置
        Vector3Int floorg = _tile.WorldToCell(target.position);
        if (isPlayer)
        {
            if (playerView.IsFat)
            {
                Debug.Log("落ちた");
                _floorGateTriggered = true; // FloorGateが発動した
                StartCoroutine(FloorGateSE(target, true, floorg));
            }
            return;
        }
        //　大谷君制作の GimmickController を取得
        var gimmick = target.GetComponent<GimmickController>();
        if (gimmick == null) return;
        // 岩の処理
        if (gimmick._gimmickData.MyGimmickType == GimmickData.GimmickType.Rock)
        { 
            Debug.Log("岩が落ちた");
            _floorGateTriggered = true; // FloorGateが発動した
            StartCoroutine(FloorGateSE(target, false, floorg));
        }

        _floorGateTriggered = false; // 何度も落ちるように false に戻す
    }

    /// <summary>
    /// SE再生と落下処理
    /// </summary>
    /// <param name="target">落とす</param>
    /// <param name="isPlayer">乗ったオブジェクトが Player かどうかの判定</param>
    /// <param name="floorg">落下処理が発生した場所(座標)</param>
    private IEnumerator FloorGateSE(Transform target, bool isPlayer, Vector3Int floorg)
    {
        // すでにSEが再生中なら処理を抜ける
        if (_floorGateSEPlaying) yield break;
        _floorGateSEPlaying = true; // SEが再生中
        _floorGateAnimation.Open();
        _audioSource.PlayOneShot(_floorGateSE); // SE再生
        // SE再生時間待機(これがないと触れた瞬間岩やプレイヤーが落ちる)
        yield return new WaitForSeconds(_seTime);
            yield return StartCoroutine(Pitfall(isPlayer,target,null));
        _floorGateAnimation.Close();
        _floorGateSEPlaying = false; // SEが再生終了
    }

    // ゴール処理
    // ゴール演出を今のシーンでする (演出時間を2～5秒くらい)
    // ゴールしたら 非表示の Text を表示
    // ゴール演出後シーンを遷移させる 
    private void Goal()
    {
        _playerView.DisablePlayerInput();
        // ゴールアニメーション再生
        _playerView.SetGoalPlayerAnime();

        StartCoroutine(GoalProcess());
    }

    private IEnumerator GoalProcess()
    {
        fading = true;

        // ゴールTextを表示
        if (_goalText != null) _goalText.gameObject.SetActive(true);

        // ゴール文字表示後指定時間待機
        yield return new WaitForSeconds(_stopScene);

        // シーンを移動する
        SceneManager.LoadScene(_sceneName);
        Debug.Log(_sceneName);
    }
}