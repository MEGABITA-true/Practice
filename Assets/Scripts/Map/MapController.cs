using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;

public class MapController : MonoBehaviour
{

    public Camera referenceCamera;
    public float checkInterval = 0.5f;

    [Header("Chunk Settings")]
    public PropsRandomizer[] terrainChunks;
    public Vector2 chunkSize = new Vector2(20f, 20f);
    public LayerMask terrainMask = 1;
    public bool deleteCulledChunks = false;
    Vector3 lastCameraPosition;
    Rect lastCameraRect;
    float cullDistanceSqr;

    void Start()
    {
        if (!referenceCamera)
            Debug.LogError("Ошибка");

        if (terrainChunks.Length < 1)
            Debug.LogError("Ошибка");
        StartCoroutine(HandleMapCheck());
        HandleChunkSpawning(Vector2.zero, true);
    }

    void Reset()
    {
        referenceCamera = Camera.main;
    }
    IEnumerator HandleMapCheck()
    {
        for (; ; )
        {
            yield return new WaitForSeconds(checkInterval);
            Vector3 moveDelta = referenceCamera.transform.position - lastCameraPosition;
            bool hasCamWidthChanged = !Mathf.Approximately(referenceCamera.pixelWidth - lastCameraRect.width, 0),
                 hasCamHeightChanged = !Mathf.Approximately(referenceCamera.pixelHeight - lastCameraRect.height, 0);

            if (hasCamWidthChanged || hasCamHeightChanged || moveDelta.magnitude > 0.1f)
            {
                HandleChunkCulling();
                HandleChunkSpawning(moveDelta, true);
            }

            lastCameraPosition = referenceCamera.transform.position;
            lastCameraRect = referenceCamera.pixelRect;
        }
    }
    public Rect GetWorldRectFromViewport()
    {
        if (!referenceCamera)
        {
            referenceCamera = Camera.main;
        }

        Vector2 minPoint = referenceCamera.ViewportToWorldPoint(referenceCamera.rect.min),
                maxPoint = referenceCamera.ViewportToWorldPoint(referenceCamera.rect.max);
        Vector2 size = new Vector2(maxPoint.x - minPoint.x, maxPoint.y - minPoint.y);
        cullDistanceSqr = Mathf.Max(size.sqrMagnitude, chunkSize.sqrMagnitude) * 3;

        return new Rect(minPoint, size);
    }
    public Vector2[] GetCheckedPoints()
    {
        Rect viewArea = GetWorldRectFromViewport();
        Vector2Int tileCount = new Vector2Int(
            (int)Mathf.Ceil(viewArea.width / chunkSize.x) + 1,
            (int)Mathf.Ceil(viewArea.height / chunkSize.y) + 1
        );

        HashSet<Vector2> result = new HashSet<Vector2>();
        for (int y = -1; y < tileCount.y; y++)
        {
            for (int x = -1; x < tileCount.x; x++)
            {
                result.Add(new Vector2(
                    viewArea.min.x + chunkSize.x * x,
                    viewArea.min.y + chunkSize.y * y
                ));
            }
        }

        return result.ToArray();
    }

    void HandleChunkSpawning(Vector2 moveDelta, bool checkWithoutDelta = false)
    {

        HashSet<Vector2> spawnedPositions = new HashSet<Vector2>();
        Vector2 currentPosition = referenceCamera.transform.position;

        foreach (Vector3 vp in GetCheckedPoints())
        {
            if (!checkWithoutDelta)
            {
                if (moveDelta.x > 0 && vp.x < 0.5f) continue;
                else if (moveDelta.x < 0 && vp.x > 0.5f) continue;

                if (moveDelta.y > 0 && vp.y < 0.5f) continue;
                else if (moveDelta.y < 0 && vp.y > 0.5f) continue;
            }

            Vector3 checkedPosition = SnapPosition(vp);

            if (!spawnedPositions.Contains(checkedPosition) && !Physics2D.OverlapPoint(checkedPosition, terrainMask))
                SpawnChunk(checkedPosition);

            spawnedPositions.Add(checkedPosition);
        }
    }
    Vector3 SnapPosition(Vector3 position)
    {
        return new Vector3(
            Mathf.Round(position.x / chunkSize.x) * chunkSize.x,
            Mathf.Round(position.y / chunkSize.y) * chunkSize.y,
            transform.position.z
        );
    }
    PropsRandomizer SpawnChunk(Vector3 spawnPosition, int variant = -1)
    {
        if (terrainChunks.Length < 1) return null;
        int rand = variant < 0 ? Random.Range(0, terrainChunks.Length) : variant;
        PropsRandomizer chunk = Instantiate(terrainChunks[rand], transform);
        chunk.transform.position = spawnPosition;
        return chunk;
    }
    void HandleChunkCulling()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform chunk = transform.GetChild(i);
            Vector2 dist = referenceCamera.transform.position - chunk.position;
            bool cull = dist.sqrMagnitude > cullDistanceSqr;
            chunk.gameObject.SetActive(!cull);
            if (deleteCulledChunks && cull) Destroy(chunk.gameObject);
        }
    }
}
