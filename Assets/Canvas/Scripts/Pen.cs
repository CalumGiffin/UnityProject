using System.Linq;
using UnityEngine;
using UnityEngine.XR; // HAPTICS

public class Pen : MonoBehaviour
{
    [SerializeField] private Transform tip;
    [SerializeField] private int penSize = 5;
    [SerializeField] private XRNode hand = XRNode.RightHand;

    private InputDevice device;
    private bool hapticsInitialized;

    private Renderer tipRenderer;
    private Color[] penColors;
    private float tipHeight;

    private RaycastHit hit;
    private Canvas canvas;

    private Vector2 lastTouchPos;
    private Quaternion lastTouchRotation;
    private bool touchedLastFrame;

    private void Start()
    {
        InitializePen();
    }

    private void Update()
    {
        Draw();
    }

    private void InitializePen()
    {
        tipRenderer = tip.GetComponent<Renderer>();
        tipHeight = tip.localScale.y;

        penColors = Enumerable
            .Repeat(tipRenderer.material.color, penSize * penSize)
            .ToArray();
    }

    private void Draw()
    {
        if (!TryGetCanvasHit(out Vector2 pixelPos))
        {
            ResetTouchState();
            return;
        }

        if (!touchedLastFrame)
        {
            PulseHaptics(0.3f, 0.05f);
        }

        if (touchedLastFrame)
        {
            DrawAt(pixelPos);
            DrawLineFromLastTouch(pixelPos);
            RestoreRotation();
            canvas.texture.Apply();

            PulseHaptics(0.15f, 0.10f);
        }

        StoreTouchState(pixelPos);
    }

    private bool TryGetCanvasHit(out Vector2 pixelPos)
    {
        pixelPos = Vector2.zero;

        if (!Physics.Raycast(tip.position, transform.up, out hit, tipHeight))
            return false;

        if (!hit.transform.CompareTag("Canvas"))
            return false;

        if (canvas == null)
            canvas = hit.transform.GetComponent<Canvas>();

        Vector2 uv = hit.textureCoord;

        int x = (int)(uv.x * canvas.textureSize.x - penSize / 2);
        int y = (int)(uv.y * canvas.textureSize.y - penSize / 2);

        if (IsOutOfBounds(x, y))
            return false;

        pixelPos = new Vector2(x, y);
        return true;
    }

    private bool IsOutOfBounds(int x, int y)
    {
        return x < 0 || x > canvas.textureSize.x ||
               y < 0 || y > canvas.textureSize.y;
    }

    private void DrawAt(Vector2 position)
    {
        canvas.texture.SetPixels(
            (int)position.x,
            (int)position.y,
            penSize,
            penSize,
            penColors
        );
    }

    private void DrawLineFromLastTouch(Vector2 currentPos)
    {
        for (float t = 0.01f; t < 1f; t += 0.01f)
        {
            int lerpX = (int)Mathf.Lerp(lastTouchPos.x, currentPos.x, t);
            int lerpY = (int)Mathf.Lerp(lastTouchPos.y, currentPos.y, t);

            canvas.texture.SetPixels(lerpX, lerpY, penSize, penSize, penColors);
        }
    }

    private void RestoreRotation()
    {
        transform.rotation = lastTouchRotation;
    }

    private void StoreTouchState(Vector2 pixelPos)
    {
        lastTouchPos = pixelPos;
        lastTouchRotation = transform.rotation;
        touchedLastFrame = true;
    }

    private void ResetTouchState()
    {
        canvas = null;
        touchedLastFrame = false;
    }

    private void TryInitializeHaptics()
    {
        if (hapticsInitialized && device.isValid) return;

        device = InputDevices.GetDeviceAtXRNode(hand);
        hapticsInitialized = device.isValid;
    }

    private void PulseHaptics(float amplitude, float duration)
    {
        TryInitializeHaptics();

        if (!device.isValid) return;

        device.SendHapticImpulse(0, amplitude, duration);
    }
}
