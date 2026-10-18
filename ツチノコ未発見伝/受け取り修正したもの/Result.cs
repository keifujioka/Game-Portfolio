using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Result : MonoBehaviour
{
    private PlayerInput _playerInput;
    [SerializeField] private Image _fadeImage;
    [SerializeField] private GameManager.Result_TYPE _resultType;
    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private List<Image> _buttonImages = new List<Image>();
    [SerializeField] private List<Transform> _buttnPoss = new List<Transform>();
    [SerializeField] private Image _selectImage;
    private int _selectIndex;
    private bool _isSelect = false;

    private bool _isDpadPressed = false;

    [SerializeField] private List<Sprite> _button = new List<Sprite>();
    [SerializeField] private List<Sprite> _pressed = new List<Sprite>();
    [SerializeField] private float _pressTime = 0.1f;

    [SerializeField] private float _high = 0.5f; // これ以上なら入力
    [SerializeField] private float _low = 0.2f; // これ以下なら入力なし

    //BGM
    [SerializeField] private AudioSource _audio;
    [SerializeField] private float _fade = 1.5f;
    //SE
    [SerializeField] private AudioClip _buttonSE;
    [SerializeField] private AudioClip _selectSE;
    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _isSelect = false;
    }
    /*
    private void OnEnable()
    {
        if (_playerInput == null) return;
        _playerInput.onActionTriggered += OnSelect;
        _playerInput.onActionTriggered += OnDecision;

    }

    private void OnDisable()
    {
        if (_playerInput == null) return;
        _playerInput.onActionTriggered -= OnSelect;
        _playerInput.onActionTriggered-= OnDecision;
    }
    */
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _resultType = GameManager.Instance._result;
        switch (_resultType)
        {
            case GameManager.Result_TYPE.Clear:
                _resultText.text = _resultType.ToString();
                break;
            case GameManager.Result_TYPE.GameOver:
                _resultText.text = _resultType.ToString();
                break;

        }
        _selectIndex = 0;

        if (_selectImage != null && _buttnPoss.Count > 0)
        {
            _selectImage.transform.position = _buttnPoss[_selectIndex].transform.position;
        }

        LMotion.Create(new Color(0, 0, 0, 1), new Color(0, 0, 0, 0), 1f).BindToColor(_fadeImage);
    }

    // Update is called once per frame
    void Update()
    {
        // 既に決定・遷移演出中なら入力を受け付けない
        if (_isSelect) return;

        HandleControllerInput();
    }
    /*
    private void OnSelect(InputAction.CallbackContext context)
    {
        if (context.action.name != "Move") return;
        Vector2 axs = context.ReadValue<Vector2>();
        //リトライ選択
        if(axs.x >= 0.1)
        {
            _selectIndex = 1;
            
        }
        //タイトル選択
        if(axs.x <= -0.1)
        {
            _selectIndex = 0;
        }
        _selectImage.transform.position = _buttnPoss[_selectIndex].transform.position;
    }
    private void OnDecision(InputAction.CallbackContext context)
    {
        if (context.action.name != "Hide") return;
        if(_selectIndex == 0)
        {
            GameManager.Instance._isGameOver = false;
            TitleButton();
            return;
        }
        if(_selectIndex == 1)
        {
            GameManager.Instance._isGameOver = false;
            RetryButton();
            return;
        }
    }
    */

    private void HandleControllerInput()
    {
        var gamepad = Gamepad.current;
        if (gamepad == null) return;

        // 十字キーまたは左スティックの「左右」の入力を取得
        float xInput = gamepad.leftStick.x.ReadValue();

        // 左右どちらかに一定以上スティックが倒されたら
        if (!_isDpadPressed)
        {
            if (Mathf.Abs(xInput) > _high)
            {
                _isDpadPressed = true;

                // 方向で決める
                _selectIndex = (xInput > 0) ? 1 : 0;
                PlaySE(_selectSE);
                UpdateCursorPosition();
            }
        }
        else
        {
            // _low 以下になるまでは入力を受け付けない
            if (Mathf.Abs(xInput) < _low)
            {
                _isDpadPressed = false;
            }
        }
        // 十字キー
        if (gamepad.dpad.left.wasPressedThisFrame)
        {
            _selectIndex = 0;
            PlaySE(_selectSE);
            UpdateCursorPosition();
        }
        else if (gamepad.dpad.right.wasPressedThisFrame)
        {
            _selectIndex = 1;
            PlaySE(_selectSE);
            UpdateCursorPosition();
        }

        if (gamepad.buttonSouth.wasPressedThisFrame)
        {
            PlaySE(_buttonSE);
            ExecuteSelection();
        }
    }

    private void UpdateCursorPosition()
    {
        if (_selectImage == null || _buttnPoss.Count <= _selectIndex) return;

        LMotion.Create(_selectImage.transform.position, _buttnPoss[_selectIndex].transform.position, 0.15f)
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
        Debug.Log("いるよ");
       //await LMotion.Create(new Vector3(1f, 1f, 1f), new Vector3(0.8f, 0.8f, 0.8f), 0.1f).BindToLocalScale(_buttonImages[_selectIndex].transform);
       //await LMotion.Create(new Vector3(0.8f, 0.8f, 0.8f), new Vector3(1f, 1f, 1f), 0.1f).BindToLocalScale(_buttonImages[_selectIndex].transform);
        ButtonSprite(_selectIndex, pressed: true);

        await UniTask.Delay(System.TimeSpan.FromSeconds(_pressTime));

        ButtonSprite(_selectIndex, pressed: false);

        await UniTask.WhenAll(
          // AnimateButton(),
           BGM(),
           Fade()
       );

        GameManager.Instance.SceneLoad("TitleScene");
    }
    private async Task RetryButton()
    {
        if (_isSelect == true) return;
        _isSelect = true;
        Debug.Log("いるよ");
        //await LMotion.Create(new Vector3(1f, 1f, 1f), new Vector3(0.8f, 0.8f, 0.8f), 0.1f).BindToLocalScale(_buttonImages[_selectIndex].transform);
        //await LMotion.Create(new Vector3(0.8f, 0.8f, 0.8f), new Vector3(1f, 1f, 1f), 0.1f).BindToLocalScale(_buttonImages[_selectIndex].transform);

        ButtonSprite(_selectIndex, pressed: true);

        await UniTask.Delay(System.TimeSpan.FromSeconds(_pressTime));

        ButtonSprite(_selectIndex, pressed: false);

        await UniTask.WhenAll(
          //AnimateButton(),
          BGM(),
          Fade()
      );
        GameManager.Instance.SceneLoad(GameManager.Instance._sceneName);
    }

    private async UniTask BGM()
    {
        if(_audio == null || !_audio.isPlaying) return;
        float startVolume = _audio.volume;

        await LMotion.Create(startVolume, 0f, _fade)
            .WithScheduler(MotionScheduler.UpdateRealtime)
            .Bind(v => _audio.volume = v);

        _audio.Stop();
    }

    private async UniTask Fade()
    {
        if (_fadeImage == null) return;

        // 透明から黒へ変化
        await LMotion.Create(new Color(0, 0, 0, 0), new Color(0, 0, 0, 1), _fade)
             .WithScheduler(MotionScheduler.UpdateRealtime)
             .BindToColor(_fadeImage);
    }

    private void PlaySE(AudioClip clip)
    {
        if (_audio != null && clip != null)
        {
            _audio.PlayOneShot(clip);
        }
    }

    private void ButtonSprite(int index, bool pressed)
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
