using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls; // ButtonControl（ボタン入力の判定）を使用するため
using UnityEngine.SceneManagement; // シーン切り替え (画面遷移)
using System.Collections;
using UnityEngine.UI;
using UnityEngine.InputSystem.LowLevel;

public class TitleController : MonoBehaviour
{
    // タイトルからのシーン遷移
    [SerializeField] private string _nextSceneName = "";
    // 遷移中かどうかを管理するフラグ
    private bool _isNextScene = false;
    // 起動直後の誤入力を防ぐためのタイマー（秒）
    private float _inputDelayTimer = 0.5f;

    [SerializeField] private AudioSource _audio;
    [SerializeField] private float _fade = 1.5f;

    [SerializeField] private Image _image;

    private void Start()
    {
        PlayerManager.Instance.ReEntry();

        if(_image != null)
        {
            Color c = _image.color;
            c.a = 0f;
            _image.color = c;
        }
    }
    // Update is called once per frame
    void Update()
    {
        // 起動直後の数フレームはタイマーを減算し、入力を受け付けない
        if (_inputDelayTimer > 0)
        {
            _inputDelayTimer -= Time.deltaTime;
            return;
        }

        // ゲームパッドが接続されている
        foreach (var gamepad in Gamepad.all)
        {
            // ボタンが押されたか
            if (AnyGamepadPressed(gamepad))
            {
                TriggerSceneTransition();
                break;
            }
        }
    }

    /// <summary>
    /// ゲームパッドのボタンが押されたかの判定
    /// </summary>
    /// <returns>押されたら true 押されてないなら false</returns>
    private bool AnyGamepadPressed(Gamepad gamepad)
    {
        return
        gamepad.buttonSouth.wasPressedThisFrame ||
        gamepad.buttonEast.wasPressedThisFrame ||
        gamepad.buttonWest.wasPressedThisFrame ||
        gamepad.buttonNorth.wasPressedThisFrame ||
        gamepad.startButton.wasPressedThisFrame ||
        gamepad.selectButton.wasPressedThisFrame ||
        gamepad.leftShoulder.wasPressedThisFrame ||
        gamepad.rightShoulder.wasPressedThisFrame ||
        gamepad.leftTrigger.wasPressedThisFrame ||
        gamepad.rightTrigger.wasPressedThisFrame ||
        gamepad.dpad.up.wasPressedThisFrame ||
        gamepad.dpad.down.wasPressedThisFrame ||
        gamepad.dpad.left.wasPressedThisFrame ||
        gamepad.dpad.right.wasPressedThisFrame;
    }

    private void TriggerSceneTransition()
    {
        // 遷移処理をしているなら処理を中断する
        if (_isNextScene) return;
        // フラグを立てる
        _isNextScene = true;

        StartCoroutine(Title());
    }

    private IEnumerator Title()
    {
        float startVolume = _audio != null ? _audio.volume : 0f;

        // BGMのフェードアウトと画像のフェードインを同時に行う
        for (float t = 0; t < _fade; t += Time.deltaTime)
        {
            float rate = t / _fade;

            if (_audio != null)
            {
                _audio.volume = Mathf.Lerp(startVolume, 0f, rate);
            }

            if (_image != null)
            {
                Color c = _image.color;
                c.a = Mathf.Lerp(0f, 1f, rate);
                _image.color = c;
            }

            yield return null;
        }

        if (_audio != null)
        {
            _audio.volume = 0f;
            _audio.Stop();
        }

        if (_image != null)
        {
            Color c = _image.color;
            c.a = 1f;
            _image.color = c;
        }

        // 指定したシーンに切り替える
        SceneManager.LoadScene(_nextSceneName);
    }
}
