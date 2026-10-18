using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SwitchManager : MonoBehaviour
{
    public static SwitchManager Instance; // インスタンスを保持

    [SerializeField, Header("EventTileをアタッチ")] private Tilemap _tilemap;
    [SerializeField, Header("Mapに配置したDoorをアタッチ")] private OpenDoor _door;

    [SerializeField, Header("スイッチのSE")] private AudioClip _switchSE;

    AudioSource _audioSource;

    private PlayerView _player;
    private StageManager _stageManager;

    // スイッチの座標リスト
    [SerializeField, Header("スイッチの座標を格納")] public List<Vector3Int> switchCellPositions = new List<Vector3Int>();

    // スイッチが押されているかの状態を保持
    private Dictionary<Vector3Int, bool> _switchPressed = new Dictionary<Vector3Int, bool>();

    private void Awake()
    {
        Instance = this;

        // AudioSource を取得
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        switchCellPositions.Clear();
        _switchPressed.Clear();

        // Switch の position を Tilemap の座標へ変換して登録
        foreach (var obj in GameObject.FindObjectsOfType<Transform>())
        {
            // (obj.name.Contains("Switch")) 名前で判別してしまうと "Switch" 全てを拾ってしまったので Tag に
            if (obj.CompareTag("Switch") && obj.gameObject != this.gameObject) // 自分自身は除外
            {
                // Tilemap の座標に変換
                Vector3Int cellPos = _tilemap.WorldToCell(obj.position);

                // 中央に配置
                obj.position = _tilemap.GetCellCenterWorld(cellPos);

                // 座標として保持
                switchCellPositions.Add(cellPos);

                // スイッチの初期状態
                _switchPressed[cellPos] = false;

                Debug.Log($"Switch登録: Cell={cellPos} World={obj.position}");
            }
        }
    }

    private void Start()
    {
        _player = FindObjectOfType<PlayerView>();
        _stageManager = FindObjectOfType<StageManager>();
    }

    public void CheckSwitches()
    {
        // プレイヤーの Tilemap 座標
        Vector3Int playerCell = _tilemap.WorldToCell(_player.transform.position);

        int pressedCount = 0; // 押されているスイッチの数

        foreach (var cell in switchCellPositions)
        {
            bool pressed = false;

            Debug.Log(_player.IsFat);
            //  Fat プレイヤーが乗っている
            if (_player.IsFat && playerCell == cell)
            {
                pressed = true;
                Debug.Log($"プレイヤーがスイッチ({cell.x}, {cell.y})に乗った");
            }

            // Rock が乗っている
            foreach (var gimmick in _stageManager.GimmickList)
            {
                if (gimmick._gimmickData.MyGimmickType == GimmickData.GimmickType.Rock)
                {
                    Vector3Int rockCell = _tilemap.WorldToCell(gimmick.transform.position);

                    if (rockCell == cell)
                    {
                        pressed = true;
                        Debug.Log($"岩がスイッチ({cell.x}, {cell.y})に乗った");
                    }
                }
            }
            // スイッチが押されたら音を鳴らす
            if (pressed && !_switchPressed[cell])
            {
                _audioSource.PlayOneShot(_switchSE);
            }
            // スイッチの状態を更新
            _switchPressed[cell] = pressed;

            // 押されているスイッチの数をカウント
            if (pressed)
                pressedCount++;
        }

        // 全部押されているか
        if (pressedCount == switchCellPositions.Count)
            _door.Open();
        else
            _door.Close();
    }
}
