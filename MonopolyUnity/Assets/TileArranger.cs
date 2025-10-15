using UnityEngine;

public class TileArranger : MonoBehaviour
{
    public Transform[] tiles;   // Kéo 40 GameObject Tile vào đây trong Inspector
    public float boardSize = 9f; // kích thước tổng (tùy theo map bạn scale)
    public float offset = 1f;    // khoảng cách giữa các ô

    void Start()
    {
        if (tiles == null || tiles.Length != 40)
        {
            Debug.LogError("⚠️ Cần đúng 40 Tile để sắp xếp bàn cờ!");
            return;
        }

        ArrangeTiles();
    }

    void ArrangeTiles()
    {
        float halfSize = boardSize / 2f;
        int index = 0;

        // Bottom row (0 → 10)
        for (int i = 0; i < 10; i++, index++)
            tiles[index].position = new Vector3(halfSize - i * offset, -halfSize, 0);

        // Left column (10 → 20)
        for (int i = 0; i < 10; i++, index++)
            tiles[index].position = new Vector3(-halfSize, -halfSize + i * offset, 0);

        // Top row (20 → 30)
        for (int i = 0; i < 10; i++, index++)
            tiles[index].position = new Vector3(-halfSize + i * offset, halfSize, 0);

        // Right column (30 → 39)
        for (int i = 0; i < 10; i++, index++)
            tiles[index].position = new Vector3(halfSize, halfSize - i * offset, 0);
    }
}
