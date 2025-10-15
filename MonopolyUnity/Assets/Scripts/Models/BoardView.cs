using UnityEngine;

namespace Monopoly.UnityAdapter
{
    public class BoardView : MonoBehaviour
    {
        [Header("Assign 40 tile transforms in clockwise order")]
        public Transform[] Tiles;

        public Vector3 GetTilePosition(int index)
        {
            if (Tiles == null || Tiles.Length == 0)
            {
                Debug.LogError("❌ Tiles not assigned in BoardView!");
                return Vector3.zero;
            }

            if (index < 0 || index >= Tiles.Length)
                index = index % Tiles.Length;

            return Tiles[index].position;
        }
    }
}
