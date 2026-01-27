using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class PenDrawer : MonoBehaviour
{
    [Header("References")]
    public Transform Tip;           // The tip of the pen
    public Material lineMaterial;      // Material used for drawing
    public LayerMask canvasMask;       // Layer of the canvas

    [Header("Settings")]
    public float lineWidth = 0.005f;
    public float maxDrawDistance = 0.02f; // Max distance from pen tip to canvas

    private LineRenderer currentLine;
    private List<Vector3> points = new List<Vector3>();

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor interactor;

    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    void Update(){
        // Only draw if the pen is currently grabbed
        if (!grabInteractable.isSelected)
        {
            currentLine = null;
            points.Clear();
            return;
        }

        // Get the interactor holding the pen
        if (interactor == null && grabInteractable.interactorsSelecting.Count > 0)
        {
            interactor = grabInteractable.interactorsSelecting[0] as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor;
        }

        if (interactor == null) return;

        // Get XRController from interactor
        XRController xrController = interactor.GetComponent<XRController>();
        if (xrController == null || !xrController.inputDevice.isValid) return;

        // Read trigger value
        xrController.inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed);

        if (!triggerPressed)
        {
            currentLine = null;
            points.Clear();
            return;
        }

        // Cast a ray from pen tip
        if (Physics.Raycast(Tip.position, Tip.forward, out RaycastHit hit, maxDrawDistance, canvasMask))
        {
            DrawAt(hit.point);
        }
        else
        {
            currentLine = null;
            points.Clear();
        }
    }

    void DrawAt(Vector3 point)
    {
        Debug.Log("Touching");
        if (currentLine == null)
        {
            GameObject lineObj = new GameObject("Line");
            currentLine = lineObj.AddComponent<LineRenderer>();
            currentLine.material = lineMaterial;
            currentLine.startWidth = lineWidth;
            currentLine.endWidth = lineWidth;
            currentLine.positionCount = 0;
        }

        points.Add(point);
        currentLine.positionCount = points.Count;
        currentLine.SetPositions(points.ToArray());
    }
}