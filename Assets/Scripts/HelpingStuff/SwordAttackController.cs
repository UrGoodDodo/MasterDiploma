using System.Collections;
using UnityEngine;

public class SwordAttackController : MonoBehaviour
{
    [Header("Input")]
    public KeyCode attackKey = KeyCode.Mouse0;

    [Header("References")]
    public Transform swordVisual;
    public ObjectSlicer slicer;

    [Header("Attack Angles")]
    public Vector3 attackAxis = Vector3.up;

    [Tooltip("Угол отвода назад перед ударом")]
    public float windUpAngle = 35f;

    [Tooltip("Основной угол режущего удара")]
    public float swingAngle = 160f;

    [Header("Attack Timing")]
    public float windUpDuration = 0.18f;
    public float swingDuration = 0.32f;
    public float recoverDuration = 0.20f;

    [Header("Motion")]
    [Tooltip("Дополнительный наклон для более живого удара")]
    public Vector3 secondaryAxis = Vector3.forward;
    public float secondaryTiltAngle = 18f;

    [Header("Curves")]
    public AnimationCurve windUpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve swingCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 2.5f),
        new Keyframe(0.35f, 0.25f, 1.2f, 1.2f),
        new Keyframe(1f, 1f, 2.8f, 0f)
    );
    public AnimationCurve recoverCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool isAttacking;
    private Quaternion initialLocalRotation;

    public bool IsAttacking => isAttacking;

    private void Start()
    {
        if (swordVisual == null)
            swordVisual = transform;

        if (slicer == null)
            slicer = GetComponent<ObjectSlicer>();

        initialLocalRotation = swordVisual.localRotation;
    }

    private void Update()
    {
        if (Input.GetKeyDown(attackKey) && !isAttacking)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        Vector3 mainAxis = attackAxis.normalized;
        Vector3 tiltAxis = secondaryAxis.normalized;

        Quaternion idle = initialLocalRotation;

        Quaternion windUpRot =
            idle *
            Quaternion.AngleAxis(-windUpAngle, mainAxis) *
            Quaternion.AngleAxis(-secondaryTiltAngle, tiltAxis);

        Quaternion swingRot =
            idle *
            Quaternion.AngleAxis(swingAngle, mainAxis) *
            Quaternion.AngleAxis(secondaryTiltAngle * 0.35f, tiltAxis);

        if (slicer != null)
            slicer.SetSliceEnabled(false);

        yield return RotateOverTime(idle, windUpRot, windUpDuration, windUpCurve);

        if (slicer != null)
            slicer.SetSliceEnabled(true);

        yield return RotateOverTime(windUpRot, swingRot, swingDuration, swingCurve);

        if (slicer != null)
            slicer.SetSliceEnabled(false);

        yield return RotateOverTime(swingRot, idle, recoverDuration, recoverCurve);

        swordVisual.localRotation = idle;
        isAttacking = false;
    }

    private IEnumerator RotateOverTime(Quaternion from, Quaternion to, float duration, AnimationCurve curve)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float curvedT = curve.Evaluate(t);

            swordVisual.localRotation = Quaternion.Slerp(from, to, curvedT);
            yield return null;
        }

        swordVisual.localRotation = to;
    }
}