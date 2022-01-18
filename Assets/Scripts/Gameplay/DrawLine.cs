using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour
{
    [SerializeField] private float pointDistance;
    [SerializeField] private float drawPitchFactor;
    [SerializeField] private float drawPitchOffset;

    [SerializeField] private GameObject player;
    [SerializeField] private GameObject pencilNormal;
    [SerializeField] private GameObject pencilBroken;

	[SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private EdgeCollider2D edgeCollider;

    [SerializeField] private AudioSource drawSource;
    [SerializeField] private Texture2D crackedCursorTexture;
    [SerializeField] private Texture2D cursorTexture;

    private List<Vector3> linePoints = new List<Vector3>();
    private Vector3[] lRendererPoints;

    private Vector2 lastMousePos;

    private bool stopDrawing;
    private bool drawing;

    private string noDrawTagName = "NoDraw";
    private string playerTagName = "Player";

    private float timePassed;
    private float lastMousePosTimeStamp;

    private void Start()
    {
        lineRenderer.useWorldSpace = true;
        linePoints = new List<Vector3>();

        Cursor.SetCursor(cursorTexture, new Vector2(1f, 27f), CursorMode.ForceSoftware);
        UpdateLastMousePos();

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag(playerTagName);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            drawing = true;
            timePassed = Time.time;

            if (linePoints.Count != 0)
            {
                stopDrawing = true;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (Time.time - timePassed <= 0.2)
            {
                return;
            }
            drawing = false;
        }

        else
        {
            if (!drawing || stopDrawing)
            {
                return;
            }

            Vector3 mousePositionInWorldSpace = this.GetMousePosWorldSpace();
            int num = CheckForInvalidPoint(mousePositionInWorldSpace) ? 1 : 0;
            mousePositionInWorldSpace.z = 1f;

            if (num != 0 || player.GetComponent<CircleCollider2D>().bounds.Contains(mousePositionInWorldSpace) || linePoints.Count > 1 
                && Vector3.Distance(mousePositionInWorldSpace, linePoints[linePoints.Count - 1]) < pointDistance)
            {
                return;
            }

            if (linePoints.Count > 0)
            {
                RaycastHit2D raycastHit2D = Physics2D.Raycast(linePoints[linePoints.Count - 1], (mousePositionInWorldSpace - linePoints[linePoints.Count - 1]), Vector2.Distance(linePoints[linePoints.Count - 1], mousePositionInWorldSpace), 4);
                
                if (raycastHit2D && raycastHit2D.collider.tag == playerTagName)
                {
                    mousePositionInWorldSpace = raycastHit2D.point;
                }
            }

            this.AddPointToLine(mousePositionInWorldSpace);
        }
    }

    private void FixedUpdate()
    {
        if ((stopDrawing || !drawing ? 0 : (Vector2.Distance(lastMousePos, GetMousePosWorldSpace()) > 0 ? 1 : 0)) != 0)
        {

            if (drawSource.isPlaying)
            {
                return;
            }
            drawSource.Play();
        }
        else
        {
            if (!drawSource.isPlaying)
            {
                return;
            }
            drawSource.Pause();
        }
    }

    private Vector3 GetMousePosWorldSpace() => Camera.main.ScreenToWorldPoint(Input.mousePosition);

    private void UpdateLastMousePos()
    {
        lastMousePos = GetMousePosWorldSpace();
        lastMousePosTimeStamp = Time.time;
    }

    private void AddPointToLine(Vector3 point)
    {
        linePoints.Add(point);
        lineRenderer.positionCount = linePoints.Count;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, point);
        lRendererPoints = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(lRendererPoints);
        edgeCollider.points = Vector3ToVector2(lRendererPoints);
    }

    private bool CheckForInvalidPoint(Vector3 mPos)
    {
        bool flag = false;
        mPos.z = Camera.main.transform.position.z;

        RaycastHit2D raycastHit2D = Physics2D.Raycast(mPos, Vector3.forward, float.PositiveInfinity, 4);

        if (raycastHit2D && raycastHit2D.collider.tag == noDrawTagName)
        {
            if (linePoints.Count > 1)
            {
                CrackPen();
            }
            else
            {
                linePoints = new List<Vector3>();
                flag = true;
                drawing = false;
            }
        }
        return flag;
    }

    private Vector2[] Vector3ToVector2(Vector3[] vector3)
    {
        Vector2[] vector2Array = new Vector2[vector3.Length];
        for (int index = 0; index < vector3.Length; index++)
        {
            vector2Array[index] = vector3[index];
        }
        return vector2Array;
    }

    private void CrackPen()
    {
        stopDrawing = true;
        Cursor.SetCursor(crackedCursorTexture, new Vector2(5f, 60f), CursorMode.ForceSoftware);
        SFXHandler.Instance.Play(SFXHandler.Sounds.Crack);

        pencilNormal.SetActive(false);
        pencilBroken.SetActive(true);
    }
}
