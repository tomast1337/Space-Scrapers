using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider))]
public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private GameObject head;
    [SerializeField] private GameObject body;
    [SerializeField] private GameObject leftArm;
    [SerializeField] private GameObject rightArm;

    private Collider _collider;

    private Tween leftArmTween;
    private Tween rightArmTween;

    private float swingSpeed = 1f;
    private float swingAngle = 30f;

    private Vector3 lastPosition;
    private Vector3 currentVelocity;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        lastPosition = transform.position;
    }

    void Start()
    {
        StartSwingingArms();
    }

    void Update()
    {
        // Calculate movement velocity
        currentVelocity = (transform.position - lastPosition) / Time.deltaTime;
        Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);

        float speed = horizontalVelocity.magnitude;

        // Rotate body toward movement direction
        if (speed > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity);
            body.transform.rotation = Quaternion.Slerp(body.transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        float targetSpeed = Mathf.Clamp(speed * 0.5f, 0.5f, 3f); // adjust animation speed
        swingSpeed = targetSpeed;

        UpdateTweenSpeeds();

        lastPosition = transform.position; // store for next frame
    }

    private void StartSwingingArms()
    {
        leftArmTween = leftArm.transform
            .DOLocalRotate(new Vector3(swingAngle, 0, 0), swingSpeed)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        rightArmTween = rightArm.transform
            .DOLocalRotate(new Vector3(-swingAngle, 0, 0), swingSpeed)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void UpdateTweenSpeeds()
    {
        if (leftArmTween != null) leftArmTween.timeScale = swingSpeed;
        if (rightArmTween != null) rightArmTween.timeScale = swingSpeed;
    }
}
