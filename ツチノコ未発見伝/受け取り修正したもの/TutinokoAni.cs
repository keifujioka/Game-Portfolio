using NaughtyAttributes;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class TutinokoAni : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private GameObject _enemy;
    //[SerializeField, AnimatorParam("_animator")] private string _lookBackTrigger;

    [Dropdown("_animation")]
    [SerializeField] private string _animationName = "Rotate";

    private string[] _animation = new string[] { "Rotate", "LRAnimation" };

    [SerializeField] private Transform _point;
    [SerializeField] private Transform _top;
    [SerializeField] private float _speed = 2f;
    [SerializeField] private Transform _enemyPoint;
    [SerializeField] private float _wait = 2f;

   // public static bool _isEntry = false;
    // private bool _down = false;
    private int _start = 0;
    private float _timer = 0f;

    private GameObject _spawnEnemy;

    void Start()
    {
        _animator = GetComponent<Animator>();
        _start = 0;

        _animator.Play("TitleMove");
    }

    void Update()
    {
        // ターゲットが設定されていない場合は処理しない
        if (_point == null || _top == null) return;

        // 上昇
        if (_start == 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, _top.position, _speed * Time.deltaTime);

            if (transform.position.y >= _top.position.y)
            {
                _start = 1;
                _animator.Play(_animationName);
            }
        }

        else if (_start == 1)
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            bool isTransitioning = _animator.IsInTransition(0);

            if (stateInfo.IsName(_animationName) && stateInfo.normalizedTime >= 0.99f && !isTransitioning)
            {
                _start = 2;
                _timer = 0f;
            }
        }
        // 待機
        else if (_start == 2)
        {
            _timer += Time.deltaTime;

            if (_timer >= _wait)
            {
                _start = 3;

                _animator.Play("Back");
            }
        }
        // 下降
        else if (_start == 3)
        {
            // 初期位置に戻す
            transform.position = Vector3.MoveTowards(transform.position, _point.position, _speed * Time.deltaTime);

            // 初期位置に到着したら
            if (Vector3.Distance(transform.position, _point.position) < 0.01f)
            {
                transform.position = _point.position;

                // エネミーの生成
                if (_enemy != null && _enemyPoint != null)
                {
                    _spawnEnemy = Instantiate(_enemy, _enemyPoint.position, _enemyPoint.rotation);
                }
                _start = 4;
            }
        }
        else if (_start == 4)
        {
            // エネミーが消滅して null になったら
            if (_spawnEnemy == null)
            {
                _start = 0;
                _animator.Play("TitleMove");
            }
        }
    }
}
