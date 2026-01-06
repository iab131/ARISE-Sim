using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Floating brick label that:
/// • Lifts once until clear of other parts
/// • Stays upright in world space
/// • Bobs + spins
/// • Scales smoothly on hover
/// </summary>
[RequireComponent(typeof(Collider))]
public class MotorLabel : MonoBehaviour
{
    // ───────── Inspector ─────────
    [Header("Scene References")]
    [SerializeField] private GameObject labelBrick;
    [SerializeField] private TextMeshPro[] labelTexts;

    [Header("Animation")]
    [SerializeField] private Vector3 baseOffset = new(0, 1.1f, 0);  // world-up offset
    [SerializeField] private float extraFloat = 150f;
    [SerializeField] private float floatAmp = 25f;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float spinSpeed = 20f;
    [SerializeField] private float hoverScale = 1.3f;
    [SerializeField] private float scaleLerpSpeed = 10f;

    [Header("Collision")]
    [SerializeField] private LayerMask collisionMask = 1 << 9;
    [SerializeField] private float stepUp = 20f;
    [SerializeField] private BoxCollider brickCol;
    [SerializeField] private float clearance = .4f;   // 2 cm; tweak in Inspector


    // ───────── Runtime ─────────
    private bool isHovered;
    private float baseY;                   // world-space offset after clearance
    private float spinAngle;
    private Vector3 startScale, targetScale;
    public static int _nextIndex = 1;   // shared by all instances
    public int MotorIndex { get; private set; }
    public char MotorLetter { get; private set; }

    private static List<MotorLabel> allLabels = new();

    // ───────── Unity Life-cycle ─────────
    private void Awake()
    {
        if (!brickCol) brickCol = labelBrick.GetComponent<BoxCollider>();

        startScale = labelBrick.transform.localScale;
        targetScale = startScale;
        labelBrick.SetActive(false);

        MotorIndex = _nextIndex++;
        allLabels.Add(this);
    }
    private void OnDestroy()
    {
        allLabels.Remove(this); 
        MotorLabelManager.Instance.ReleaseLetter(this);
    }
    public static IEnumerable<MotorLabel> All => allLabels;

    private void Update()
    {
        if (!labelBrick.activeSelf) return;

        /* Smooth scale */
        labelBrick.transform.localScale =
            Vector3.Lerp(labelBrick.transform.localScale, targetScale,
                         Time.deltaTime * scaleLerpSpeed);

        /* Bob + spin (world-space) */
        if (!isHovered)
            spinAngle = Mathf.Repeat(spinAngle + spinSpeed * Time.deltaTime, 360f);

        float yWave = Mathf.Sin(Time.time * floatSpeed) * floatAmp;
        Vector3 posWS = transform.position + Vector3.up * (baseY + yWave);

        labelBrick.transform.SetPositionAndRotation(
            posWS,
            Quaternion.Euler(0f, spinAngle, 0f));   // upright (no parent tilt)
    }

    // ───────── One-time collision lift ─────────
    public void AdjustLabelToAvoidCollision()
    {
        /* Disable our own collider so the probe won’t hit itself */
        bool colInitiallyEnabled = brickCol.enabled;
        brickCol.enabled = false;

        float probeY = transform.position.y + baseOffset.y;
        float travelled = 0f;

        
        while (true)
        {
            Vector3 probePos = new(transform.position.x, probeY, transform.position.z);
            labelBrick.transform.SetPositionAndRotation(probePos, Quaternion.identity);

            Bounds b = brickCol.bounds;
            Vector3 halfExt = b.extents + Vector3.one * clearance;
            bool hit = Physics.CheckBox(b.center, halfExt, Quaternion.identity,
                                        collisionMask, QueryTriggerInteraction.Ignore);
            if (!hit) break;

            probeY += stepUp;
            travelled += stepUp;
        }

        brickCol.enabled = colInitiallyEnabled;   // restore

        probeY += extraFloat;
        baseY = probeY - transform.position.y;  // store offset for Update()

        labelBrick.transform.SetPositionAndRotation(
            new Vector3(transform.position.x, probeY, transform.position.z),
            Quaternion.identity);
    }

    // ───────── UI helpers ─────────
    public void SetLabelVisible(bool on)
    {
        labelBrick.SetActive(on);
        if (on) AdjustLabelToAvoidCollision();
    }

    public void SetLetter(char c)
    {
        foreach (var t in labelTexts) if (t) t.text = c.ToString();
        GetComponentInChildren<SimMotor>().SetMotorLabel(c);
        MotorLetter = c;
    }

    public void SetHovered(bool state)
    {
        if (isHovered == state) return;
        isHovered = state;
        targetScale = state ? startScale * hoverScale : startScale;
    }

    // ───────── Mouse events with guards ─────────
    // Add these handlers inside MotorLabel
    public void HandleMouseEnter()
    {
        if (MotorLabelManager.Instance.IsModalOpen ||
            EventSystem.current.IsPointerOverGameObject()) return;

        if (MotorLabelManager.Instance.assignModeActive) SetHovered(true);
    }

    public void HandleMouseExit()
    {
        if (MotorLabelManager.Instance.IsModalOpen) return;
        SetHovered(false);
    }

    public void HandleMouseUp()
    {
        if (MotorLabelManager.Instance.IsModalOpen ||
            EventSystem.current.IsPointerOverGameObject()) return;

        MotorLabelManager.Instance.OnMotorClicked(this);
        TutorialManager.Instance?.SignalStepDone();
    }

    // Keep Unity’s original OnMouse* but just forward to the handlers
    private void OnMouseEnter() => HandleMouseEnter();
    private void OnMouseExit() => HandleMouseExit();
    private void OnMouseUp() => HandleMouseUp();



}
