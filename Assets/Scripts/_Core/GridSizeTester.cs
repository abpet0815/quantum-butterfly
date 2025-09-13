using UnityEngine;

public class GridSizeTester : MonoBehaviour
{
    private void Update()
    {
        if (GameManager.Instance == null) return;
        
        // Test different grid sizes with number keys
        if (Input.GetKeyDown(KeyCode.Alpha1))
            GameManager.Instance.SetGridSize(new Vector2Int(2, 2));
        if (Input.GetKeyDown(KeyCode.Alpha2))
            GameManager.Instance.SetGridSize(new Vector2Int(3, 4));
        if (Input.GetKeyDown(KeyCode.Alpha3))
            GameManager.Instance.SetGridSize(new Vector2Int(4, 4));
        if (Input.GetKeyDown(KeyCode.Alpha4))
            GameManager.Instance.SetGridSize(new Vector2Int(4, 6));
        if (Input.GetKeyDown(KeyCode.Alpha5))
            GameManager.Instance.SetGridSize(new Vector2Int(5, 6));
    }
}
