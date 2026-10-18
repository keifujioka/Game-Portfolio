using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "CustomTile/EventTile")]
public class EventTile : Tile
{
    // タイルイベントの作成
   public enum EventType
    {
        None, // 何もない
        Grow,  // 大きく
        Small,  // 小さく
        Hole, // 落とし穴
        FloorGate, // 床のゲート
        Goal // ゴール
    }
    public EventType eventType = EventType.None;

    // アニメーション用のスプライト
    [SerializeField,Header("アニメーション画像")] public Sprite[] _animationSprites;
    [SerializeField,Header("アニメーション速度")] public float _animationSpeed = 1.0f; // アニメーション速度
    [SerializeField,Header("アニメーション開始")] public float _animationStartTime = 0; // アニメーション開始時間

    // アニメーションがあるか確認
    // override で 継承元のメソッドを上書き
    public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
    {
        if (_animationSprites != null && _animationSprites.Length > 0)
        {
            // アニメーションを設定
            tileAnimationData.animatedSprites = _animationSprites;
            tileAnimationData.animationSpeed = _animationSpeed;
            tileAnimationData.animationStartTime = _animationStartTime;
            return true; // アニメーションがある
        }
        return false;
    }
}
