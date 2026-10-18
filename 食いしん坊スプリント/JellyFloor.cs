using UnityEngine;
using UnityEngine.InputSystem;

public class JellyFloor : MonoBehaviour
{
    // ジャンプパワー　*の値を変更
    [SerializeField] private float boostedJumpPower = 2f;
    private bool _jumpPower = false;
    private PlayerView _playerView;
    private PlayerInput _playerInput;

    // PlayerTag のリスト
    private readonly string[] _playerTag = { "Player", "Player1", "Player2", "Player3", "Player4" };

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsPlayerTag(collision.collider.tag))
        {
            _playerView = collision.collider.GetComponent<PlayerView>();
            _playerInput = collision.collider.GetComponent<PlayerInput>();

            if (_playerView != null && _playerInput != null)
            {
                _jumpPower = true;

                InputAction jumpAction = _playerInput.actions["Jump"];
                jumpAction.performed += OnJumpPressed;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (IsPlayerTag(collision.collider.tag))
        {
            if (_playerInput != null)
            {
                InputAction jumpAction = _playerInput.actions["Jump"];
                jumpAction.performed -= OnJumpPressed;
            }

            _playerView = null;
            _playerInput = null;
            _jumpPower = false;
        }
    }
  
   private void OnJumpPressed(InputAction.CallbackContext context)
    {
        if (_jumpPower && _playerView != null)
        {
            Rigidbody2D rb = _playerView.GetComponent<Rigidbody2D>();
            
            if (rb != null)
            {
                // PlayerViewのジャンプパワー * で跳ぶ
                float boostedJump = _playerView.JumpPower * boostedJumpPower;
                rb.velocity = new Vector2(rb.velocity.x, 0f);
                rb.AddForce(Vector2.up * boostedJump, ForceMode2D.Impulse);
            }
        }
    }

    private bool IsPlayerTag(string tag)
    {
        foreach (var playerTag in _playerTag)
        {
            if (tag == playerTag)
                return true;
        }
        return false;
    }
}