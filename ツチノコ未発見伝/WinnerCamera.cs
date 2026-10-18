using Cysharp.Threading.Tasks;
using System.Threading;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class WinnerCamera : MonoBehaviour
{

    [SerializeField] private Camera _playerCamera1;
    [SerializeField] private Camera _playerCamera2;
    [SerializeField] private float _cameraTime = 1f; // カメラを全画面表示にするまでの時間

    [SerializeField] private CinemachineCamera _cineCamera1;
    [SerializeField] private CinemachineCamera _cineCamera2;

    [SerializeField] private float _zoomSize = 3f;
    [SerializeField] private float _zoomTime = 1f;

    // プレイヤー1が勝利した場合
    public void Player1Win(Transform goal)
    {
        Fullscreen(_playerCamera1, _playerCamera2, _cineCamera1, _cineCamera2, new Rect(0f, 0f, 0.5f, 1f),
             goal, this.GetCancellationTokenOnDestroy()).Forget();
    }
    // プレイヤー2が勝利した場合
    public void Player2Win(Transform goal)
    {
        Fullscreen(_playerCamera2, _playerCamera1, _cineCamera2, _cineCamera1, new Rect(0.5f, 0, 0.5f, 1),
            goal, this.GetCancellationTokenOnDestroy()).Forget();
    }

    /// <summary>
    /// カメラを全画面表示にする非同期処理
    /// </summary>
    /// <param name="win">勝利したプレイヤーのカメラ</param>
    /// <param name="lose">敗北したプレイヤーのカメラ</param>
    /// <param name="startRect">カメラの初期表示範囲 </param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// /// <param name="cancellationToken">ゴール座標</param>
    /// <returns></returns>
    private async UniTask Fullscreen(Camera win, Camera lose, CinemachineCamera winCine, CinemachineCamera loseCine,Rect startRect, Transform goal,CancellationToken cancellationToken)
    {
        if(win == null || lose == null|| winCine == null) return;

        float elapsedTime = 0f;
        // 全画面表示
        Rect fullscreen = new Rect(0, 0, 1, 1);

        // 勝ったプレイヤーカメラの初期 depth を取得
        // depth はカメラの描画順序を決定するための値、値が大きい前面に表示される
        float originalDepth = win.depth;
        // 勝ったプレイヤーカメラの depth を大きくして、前面に表示されるようにする
        win.depth = Mathf.Max(_playerCamera1.depth, _playerCamera2.depth) + 1;
        // 負けたプレイヤーカメラの初期表示範囲を取得
        Rect losestartRect = lose.rect;

        // Cinemachine 追従をストップ
        winCine.enabled = false;
        if (loseCine != null)
        {
            loseCine.enabled = false;
        }
        // OrthographicSize はカメラの表示範囲の大きさを決定する値、値が小さいほどズームインされる
        float _zoom = win.orthographicSize;

        try
        {
            // カメラの表示範囲を徐々に変更するループ
            while (elapsedTime < _cameraTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsedTime / _cameraTime);
                float eased = Mathf.SmoothStep(0f, 1f, t);

                win.rect = new Rect(
                    Mathf.Lerp(startRect.x, fullscreen.x, eased),
                    Mathf.Lerp(startRect.y, fullscreen.y, eased),
                    Mathf.Lerp(startRect.width, fullscreen.width, eased),
                    Mathf.Lerp(startRect.height, fullscreen.height, eased)
                );
                lose.rect = new Rect(
                    Mathf.Lerp(losestartRect.x, fullscreen.x, eased),
                    Mathf.Lerp(losestartRect.y, fullscreen.y, eased),
                    Mathf.Lerp(losestartRect.width, fullscreen.width, eased),
                    Mathf.Lerp(losestartRect.height, fullscreen.height, eased)
                );
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }
            win.rect = fullscreen;
            lose.gameObject.SetActive(false);

            if(goal != null)
            {
                Vector3 _cameraPos = win.transform.position;
                _cameraPos.x = goal.position.x;
                _cameraPos.y = goal.position.y;
                win.transform.position = _cameraPos;
            }
            elapsedTime = 0f;

            while (elapsedTime < _zoomTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsedTime / _zoomTime);
                float eased = Mathf.SmoothStep(0f, 1f, t);
                // カメラをズームする
                win.orthographicSize = Mathf.Lerp(_zoom, _zoomSize, eased);
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }
            // 最終値を設定
            win.orthographicSize = _zoomSize;
        }
        catch (System.OperationCanceledException)
        {
           // return;
        }
        finally
        {
            // 勝ったプレイヤーカメラのdepthを元に戻す
            if (win != null)
            {
                win.depth = originalDepth;
            }
        }
    }
}