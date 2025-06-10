using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private GameObject head;
    [SerializeField] private GameObject body;
    [SerializeField] private GameObject leftArm;
    [SerializeField] private GameObject rightArm;

    private Collider _collider;
    private Rigidbody _rigidbody;

    private Tween leftArmTween;
    private Tween rightArmTween;

    private float swingSpeed = 1f;
    private float swingAngle = 30f;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartSwingingArms();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 horizontalVelocity = _rigidbody.linearVelocity;
        horizontalVelocity.y = 0f;

        float speed = horizontalVelocity.magnitude;

        // Only face movement direction if moving
        if (speed > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity);
            body.transform.rotation = Quaternion.Slerp(body.transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        float targetSpeed = Mathf.Clamp(speed * 0.5f, 0.5f, 3f); // map speed to animation speed range
        swingSpeed = targetSpeed;

        UpdateTweenSpeeds();
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
