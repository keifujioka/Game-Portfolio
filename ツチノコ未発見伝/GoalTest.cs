using Cysharp.Threading.Tasks;
using DG.Tweening;
using LitMotion;
using LitMotion.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoalTest : MonoBehaviour
{
    [SerializeField] private Image _winImage;
    [SerializeField] private string _nextSceneName = "";

    [Header("勝利Image関係")]
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private float _posX = 0f;
    [SerializeField] private float _fastPosY = 0f;
    [SerializeField] private float _posY = 50f;
    [SerializeField] private float _winTime = 1f;

    [SerializeField] private WinnerCamera _winnerCamera;

    [SerializeField] private float _fadeTime = 0.5f;

    [Header("ボタン選択")]
    [SerializeField] private List<Image> _buttonImages = new List<Image>(); // 0: タイトルイメージ, 1: リトライイメージ
    [SerializeField] private List<Transform> _buttnPoss = new List<Transform>(); // 0: タイトル位置, 1: リトライ位置
    [SerializeField] private Image _selectImage; // 動かすカーソル画像
    [SerializeField] private List<Sprite> _button = new List<Sprite>(); // 押す前のボタン
    [SerializeField] private List<Sprite> _pressed = new List<Sprite>(); // 押した後のボタン
    [SerializeField] private float _pressTime = 1f;
    private int _selectIndex;
    private bool _isSelect = false;
    private bool _isDpadPressed = false; // スティック・十字キーの連続入力を防ぐフラグ

    private bool _goal = false; // ゴール範囲にいるか
    private GameObject _playerObj;
    private bool _selectUI;

    [Header("BGM")]
    [SerializeField] private AudioClip _win;
    [SerializeField] private bool _useBGM = true;
    [SerializeField] private float _fadeBGM = 0.5f;

    [Header("SE")]
    [SerializeField] private AudioClip _buttonSE;
    [SerializeField] private AudioClip _selectSE;

    [SerializeField] private Image _fadeImage;
    [SerializeField] private AudioSource _audio;
    [SerializeField] private float _fadeImageTime = 1.5f;
    // リザルトにタイムを表示するための参照
    [SerializeField] private TimeControll _timeControll;
    [SerializeField] private TextMeshProUGUI _timeText;

    [Header("ゴール後非表示")]
    [SerializeField] private GameObject _playerBar;
    [SerializeField] private GameObject _gameInages;
    [SerializeField] private GameObject _enemy1;
    [SerializeField] private GameObject _enemy2;

    // WinImage の初期位置を設定
    void Awake()
    {
        _rectTransform = _winImage.GetComponent<RectTransform>();

        _rectTransform.anchoredPosition = new Vector2(_posX, _fastPosY);

        // 画像を初期状態で透明にする
        SetAlpha(_selectImage, 0f);
        foreach (var btnImg in _buttonImages)
        {
            SetAlpha(btnImg, 0f);
        }

        _playerBar.SetActive(true);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _selectIndex = 0; // 初期選択はタイトル
        if (_selectImage != null && _buttnPoss.Count > 0)
        {
            _selectImage.transform.position = _buttnPoss[_selectIndex].transform.position;
        }
    }

    void Update()
    {
        // ゴール前、またはUIフェードイン前なら何もしない
        if (!_selectUI) return;

        if (_isSelect) return;

        HandleControllerInput();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_goal) return;
        var player = other.gameObject.GetComponent<PlayerInput>();
        if (player == null) return;

        _goal = true;
        _playerObj = other.gameObject;

        _playerBar.SetActive(false);

        _gameInages.SetActive(false);

        _enemy1.SetActive(false);
        _enemy2.SetActive(false);

        _timeControll.StopTimer();
        other.gameObject.GetComponent<PlayerController>().WinAnim();

        if (_timeText != null && _timeControll != null)
        {
            float goalTime = _timeControll.GoalTime();
            _timeText.text = $"クリアタイム {goalTime:F1}秒";
        }

        // ゲームを止める
       // Time.timeScale = 0f;

        StartCoroutine(Goal(player));
    }

    // DoTween で WinImage を上から落とす
    private void Win()
    {
        _rectTransform.DOAnchorPos(new Vector2(_posX, _posY), _winTime)
            .SetEase(DG.Tweening.Ease.OutBounce)
            .SetUpdate(true) // timeScale を無視
            .OnComplete(() =>
            {
                // Imageをフェードインさせる
                foreach (var btnImg in _buttonImages)
                {
                    if (btnImg != null) btnImg.DOFade(1f, _fadeTime).SetUpdate(true);
                }
                if (_selectImage != null)
                {
                    // カーソル画像も一緒にフェードインし、完了したら操作を許可
                    _selectImage.DOFade(1f, _fadeTime).SetUpdate(true).OnComplete(() =>
                    {
                        _selectUI = true;
                        UpdateCursorPosition();
                    });
                }
                else
                {
                    // カーソル画像がない場合のセーフティ
                    _selectUI = true;
                    UpdateCursorPosition();
                }
            });
    }

    private IEnumerator Goal(PlayerInput player)
    {
        if (BGM.Instance != null && _win != null)
        {
            if (_useBGM)
            {
                _ = BGM.Instance.PlayBGMWithFade(_win, _fadeBGM);
            }
        }

        if (_winnerCamera != null)
        {
            yield return new WaitForSecondsRealtime(1f);

            if (player.playerIndex == 0)
            {
                _winnerCamera.Player1Win(player.transform);
            }
            else if (player.playerIndex == 1)
            {
                _winnerCamera.Player2Win(player.transform);
            }
        }
        yield return new WaitForSecondsRealtime(1f);
        Win();
    }

    private void HandleControllerInput()
    {
        bool anyStickMove = false;

        foreach (var gamepad in Gamepad.all)
        {
            if (gamepad == null) return;
            // 十字キーまたは左スティックの「左右」の入力を取得
            float xInput = gamepad.dpad.x.ReadValue() + gamepad.leftStick.x.ReadValue();

            // 左右どちらかに一定以上スティックが倒されたら
            if (Mathf.Abs(xInput) > 0.5f)
            {
                anyStickMove = true;
                if (!_isDpadPressed)
                {
                    _isDpadPressed = true;

                    // 左か右に入力されたら選択を反転（0と1を入れ替える）
                    _selectIndex = (_selectIndex == 0) ? 1 : 0;
                    PlaySE(_selectSE);
                    UpdateCursorPosition();
                    return;
                }
            }

            // X ボタンで決定
            if (gamepad.buttonSouth.wasPressedThisFrame)
            {
                PlaySE(_buttonSE);
                ExecuteSelection();
                return;
            }
        }

        if (!anyStickMove)
        {
            _isDpadPressed = false;
        }
    }

    private void UpdateCursorPosition()
    {
        if (_selectImage == null || _buttnPoss.Count <= _selectIndex) return;

        // LitMotion移動
        LMotion.Create(_selectImage.transform.position, _buttnPoss[_selectIndex].transform.position, 0.15f)
            .WithScheduler(MotionScheduler.UpdateRealtime) // timeScale を無職
            .Bind(pos => _selectImage.transform.position = pos);
    }

    private void ExecuteSelection()
    {
        if (_selectIndex == 0)
        {
            GameManager.Instance._isGameOver = false;
            _ = TitleButton(); // Taskの警告除け（破棄フラグ）
        }
        else if (_selectIndex == 1)
        {
            GameManager.Instance._isGameOver = false;
            _ = RetryButton(); // Taskの警告除け（破棄フラグ）
        }
    }
    private async Task TitleButton()
    {
        if (_isSelect == true) return;
        _isSelect = true;

        var token = this.destroyCancellationToken;

        // 押した画像に切り替え
        ButtonSprite(_selectIndex, pressed: true);

      //  await UniTask.Delay(System.TimeSpan.FromSeconds(_pressTime), ignoreTimeScale: true, cancellationToken: token);

     //   ButtonSprite(_selectIndex, pressed: false);

        await UniTask.WhenAll(
        BGMFadeOut(token),
        Fade(token)
        );

       // Time.timeScale = 1f;
        GameManager.Instance.SceneLoad("TitleScene");
    }
    private async Task RetryButton()
    {
        if (_isSelect == true) return;
        _isSelect = true;

        var token = this.destroyCancellationToken;

        // 押した画像に切り替え
        ButtonSprite(_selectIndex, pressed: true);

      //  await UniTask.Delay(System.TimeSpan.FromSeconds(_pressTime), ignoreTimeScale: true, cancellationToken: token);

       // ButtonSprite(_selectIndex, pressed: false);

        await UniTask.WhenAll(
        BGMFadeOut(token),
        Fade(token)
        );

       // Time.timeScale = 1f;
        string currentSceneName = SceneManager.GetActiveScene().name;
        GameManager.Instance.SceneLoad(currentSceneName);
    }

    private async UniTask BGMFadeOut(System.Threading.CancellationToken token)
    {
        if (_audio == null || !_audio.isPlaying) return;

        float startVolume = _audio.volume;

        await LMotion.Create(startVolume, 0f, _fadeImageTime)
            .WithScheduler(MotionScheduler.UpdateRealtime)
            .Bind(v => _audio.volume = v)
            .ToUniTask(cancellationToken: token);

        _audio.Stop();
    }

    private async UniTask Fade(System.Threading.CancellationToken token)
    {
        if (_fadeImage == null) return;

        await LMotion.Create(new Color(0, 0, 0, 0), new Color(0, 0, 0, 1), _fadeImageTime)
            .WithScheduler(MotionScheduler.UpdateRealtime)
            .BindToColor(_fadeImage)
            .ToUniTask(cancellationToken: token);
    }

    private void SetAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    private void PlaySE(AudioClip clip)
    {
        if (_audio != null && clip != null)
        {
            _audio.PlayOneShot(clip);
        }
    }
    // ボタン Image 切り替え
    private void ButtonSprite(int index,bool pressed)
    {
        if (index < 0 || index >= _buttonImages.Count) return;
        if (_buttonImages[index] == null) return;

        if (pressed)
        {
            if (index < _pressed.Count && _pressed[index] != null)
                _buttonImages[index].sprite = _pressed[index];
        }
        else
        {
            if (index < _button.Count && _button[index] != null)
                _buttonImages[index].sprite = _button[index];
        }
    }
}
