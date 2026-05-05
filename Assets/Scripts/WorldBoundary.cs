using UnityEngine;

public class WorldBoundary : MonoBehaviour
{

    [Header("Boundary")]
    [Tooltip("World-space center of the playable area.")]
    public Vector3 boundaryCenter = new Vector3(250f, 0f, 250f);

    [Tooltip("Half-width and half-depth of the playable square in world units")]
    public float halfExtent = 230f;

    [Tooltip("Height of the invisible walls.")]
    public float wallHeight = 60f;

    [Tooltip("Thickness of each wall collider.")]
    public float wallThickness = 2f;

    [Header("Debug")]
    [Tooltip("Draws the boundary as a yellow box in the Scene view.")]
    public bool showGizmos = true;

    void Awake()
    {
        BuildWalls();
    }

    void BuildWalls()
    {

        float cx = boundaryCenter.x;
        float cy = boundaryCenter.y + wallHeight * 0.5f;
        float cz = boundaryCenter.z;
        float e  = halfExtent;
        float t  = wallThickness;
        float h  = wallHeight;
        float w  = e * 2f + t * 2f;

        CreateWall("Boundary_North", new Vector3(cx,      cy, cz + e), new Vector3(w, h, t));
        CreateWall("Boundary_South", new Vector3(cx,      cy, cz - e), new Vector3(w, h, t));
        CreateWall("Boundary_East",  new Vector3(cx + e,  cy, cz),     new Vector3(t, h, w));
        CreateWall("Boundary_West",  new Vector3(cx - e,  cy, cz),     new Vector3(t, h, w));
    }

    void CreateWall(string wallName, Vector3 position, Vector3 size)
    {
        GameObject wall = new GameObject(wallName);
        wall.transform.SetParent(transform);
        wall.transform.position = position;

        BoxCollider col = wall.AddComponent<BoxCollider>();
        col.size   = size;
        col.center = Vector3.zero;
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = new Color(1f, 0.8f, 0f, 0.3f);
        Gizmos.DrawCube(
            boundaryCenter + Vector3.up * wallHeight * 0.5f,
            new Vector3(halfExtent * 2f, wallHeight, halfExtent * 2f));

        Gizmos.color = new Color(1f, 0.8f, 0f, 0.9f);
        Gizmos.DrawWireCube(
            boundaryCenter + Vector3.up * wallHeight * 0.5f,
            new Vector3(halfExtent * 2f, wallHeight, halfExtent * 2f));
    }
}
