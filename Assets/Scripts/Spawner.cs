using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    #region Enum

    public enum GizmoType
    {
        Never,
        SelectedOnly,
        Always
    }

    #endregion

    #region Reference

    public Boid prefab;

    #endregion

    #region Variables

    public float spawnRadius = 10;
    public int spawnCount = 10;
    public Color colour;
    public GizmoType showSpawnRegion;

    #endregion

    #region Awake

    void Awake()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
            Boid boid = Instantiate(prefab);
            boid.transform.position = pos;
            boid.transform.forward = Random.insideUnitSphere;

            boid.SetColour(colour);
        }
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmos()
    {
        if (showSpawnRegion == GizmoType.Always)
        {
            DrawGizmos();
        }
    }

    #endregion

    #region Functions

    void OnDrawGizmosSelected()
    {
        if (showSpawnRegion == GizmoType.SelectedOnly)
        {
            DrawGizmos();
        }
    }

    void DrawGizmos()
    {
        Gizmos.color = new Color(colour.r, colour.g, colour.b, 0.3f);
        Gizmos.DrawSphere(transform.position, spawnRadius);
    }

    #endregion
}