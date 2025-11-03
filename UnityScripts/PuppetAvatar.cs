using Microsoft.Azure.Kinect.BodyTracking;
using Microsoft.Azure.Kinect.Sensor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public class PuppetAvatar : MonoBehaviour
{
    [Header("==== CORE REFERENCES ====")]
    public TrackerHandler KinectDevice;
    Dictionary<JointId, Quaternion> absoluteOffsetMap;
    Animator PuppetAnimator;
    public GameObject RootPosition;
    public Transform CharacterRootTransform;
    private LevelUI levelUI;

    [Header("==== POSITION OFFSETS ====")]
    public float OffsetY;
    public float OffsetZ;

    [Header("==== COLLIDER/RIGIDBODY REFERENCES ====")]
    BoxCollider bodyCollider;
    Rigidbody rb;
    BoxCollider leftThighCollider, leftShinCollider, leftFootCollider;
    BoxCollider rightThighCollider, rightShinCollider, rightFootCollider;

    private float currentStepHeight = 0f;
    public float stepSmoothSpeed = 40f;

    [Header("==== LATERAL MOVEMENT ====")]
    public float moveSpeed = 3f; // Lateral movement speed multiplier
    float lateralThreshold = 0.01f;
    private float previousPelvisX;

    [Header("==== JUMP DETECTION ====")]
    private bool isJumping = false;
    private float jumpStartTime;           // Track when jump started
    private float lastPelvisY = 0f;
    private float jumpStartY = 0f;
    private float maxJumpY = 0f;
    private bool trackingJump = false;
    private float upwardVelocityThreshold = 4.0f;
    private float pelvisFallThreshold = 0.90f;
    private float minJumpHeight = 0.05f;

    public Dictionary<int, string> jumpInfo = new Dictionary<int, string>();

    [Header("==== CROUCH DETECTION ====")]
    public float crouchKneeAngleThreshold = 100f;
    private float crouchStartTime = 0f;
    private bool isCrouching = false;

    List<float> leftLegAngles = new List<float>();
    List<float> rightLegAngles = new List<float>();

    public Dictionary<int, string> crouchInfo = new Dictionary<int, string>();

    [Header("==== LEG HOLD DETECTION ====")]
    public bool isLegHoldingLeft = false;
    public bool isLegHoldingRight = false;
    public float legHoldStartTime = 0f;

    public Dictionary<int, string> holdInfo = new Dictionary<int, string>();

    [Header("==== OTHER ====")]
    private ObstacleMovement obstacleMovement;
    public GameObject currentObstacle;


    [Header("== PAUSE MENU RELATED ==")]
    private bool pauseGestureActive = false;
    private float pauseCooldown = 1.0f; // cooldown
    private float lastPauseTime = -10f;
    public static event Action OnPauseTriggered;

    // Kinect to Character //
    private static HumanBodyBones MapKinectJoint(JointId joint)
    {
        // https://docs.microsoft.com/en-us/azure/Kinect-dk/body-joints
        switch (joint)
        {
            case JointId.Pelvis: return HumanBodyBones.Hips;
            case JointId.SpineNavel: return HumanBodyBones.Spine;
            case JointId.SpineChest: return HumanBodyBones.Chest;
            case JointId.Neck: return HumanBodyBones.Neck;
            case JointId.Head: return HumanBodyBones.Head;
            case JointId.HipLeft: return HumanBodyBones.LeftUpperLeg;
            case JointId.KneeLeft: return HumanBodyBones.LeftLowerLeg;
            case JointId.AnkleLeft: return HumanBodyBones.LeftFoot;
            case JointId.FootLeft: return HumanBodyBones.LeftToes;
            case JointId.HipRight: return HumanBodyBones.RightUpperLeg;
            case JointId.KneeRight: return HumanBodyBones.RightLowerLeg;
            case JointId.AnkleRight: return HumanBodyBones.RightFoot;
            case JointId.FootRight: return HumanBodyBones.RightToes;
            case JointId.ClavicleLeft: return HumanBodyBones.LeftShoulder;
            case JointId.ShoulderLeft: return HumanBodyBones.LeftUpperArm;
            case JointId.ElbowLeft: return HumanBodyBones.LeftLowerArm;
            case JointId.WristLeft: return HumanBodyBones.LeftHand;
            case JointId.ClavicleRight: return HumanBodyBones.RightShoulder;
            case JointId.ShoulderRight: return HumanBodyBones.RightUpperArm;
            case JointId.ElbowRight: return HumanBodyBones.RightLowerArm;
            case JointId.WristRight: return HumanBodyBones.RightHand;
            default: return HumanBodyBones.LastBone;
        }
    }

    private static SkeletonBone GetSkeletonBone(Animator animator, string boneName)
    {
        int count = 0;
        StringBuilder cloneName = new StringBuilder(boneName);
        cloneName.Append("(Clone)");
        foreach (SkeletonBone sb in animator.avatar.humanDescription.skeleton)
        {
            if (sb.name == boneName || sb.name == cloneName.ToString())
            {
                return animator.avatar.humanDescription.skeleton[count];
            }
            count++;
        }
        return new SkeletonBone();
    }

    private void UpdateSkeletonPose()
    {
        for (int j = 0; j < (int)JointId.Count; j++)
        {
            HumanBodyBones mappedBone = MapKinectJoint((JointId)j);
            if (mappedBone == HumanBodyBones.LastBone || !absoluteOffsetMap.ContainsKey((JointId)j))
                continue;

            Quaternion absOffset = absoluteOffsetMap[(JointId)j];
            Transform finalJoint = PuppetAnimator.GetBoneTransform(mappedBone);
            if (finalJoint == null)
                continue;

            finalJoint.rotation = KinectDevice.absoluteJointRotations[j] * absOffset;

            if (j == 0) // pelvis joint
            {
                // Set pelvis position absolutely — no addition with root transform pos
                finalJoint.position = CharacterRootTransform.position + new Vector3(
                    RootPosition.transform.localPosition.x,
                    RootPosition.transform.localPosition.y + OffsetY,
                    (RootPosition.transform.localPosition.z / 10f) - OffsetZ
                );
            }
        }
    }

    // Main Functions //
    private void Start()
    {
        PuppetAnimator = GetComponent<Animator>();
        Transform _rootJointTransform = CharacterRootTransform;
        bodyCollider = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();

        //previousPelvisY = PuppetAnimator.GetBoneTransform(HumanBodyBones.Hips).position.y;

        obstacleMovement = GameObject.FindGameObjectWithTag("ObstacleMovement")?.GetComponent<ObstacleMovement>();
        if (obstacleMovement != null)
        {
            obstacleMovement.OnObstacleChanged += OnCurrentObstacleChanged;
            currentObstacle = obstacleMovement.currentObstacle;
        }

        levelUI = GameObject.FindGameObjectWithTag("LevelUI")?.GetComponent<LevelUI>();

        leftThighCollider = PuppetAnimator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).GetComponent<BoxCollider>();
        leftShinCollider = PuppetAnimator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).GetComponent<BoxCollider>();
        leftFootCollider = PuppetAnimator.GetBoneTransform(HumanBodyBones.LeftToes).GetComponent<BoxCollider>();

        rightThighCollider = PuppetAnimator.GetBoneTransform(HumanBodyBones.RightUpperLeg).GetComponent<BoxCollider>();
        rightShinCollider = PuppetAnimator.GetBoneTransform(HumanBodyBones.RightLowerLeg).GetComponent<BoxCollider>();
        rightFootCollider = PuppetAnimator.GetBoneTransform(HumanBodyBones.RightToes).GetComponent<BoxCollider>();

        absoluteOffsetMap = new Dictionary<JointId, Quaternion>();
        for (int i = 0; i < (int)JointId.Count; i++)
        {
            HumanBodyBones hbb = MapKinectJoint((JointId)i);
            if (hbb != HumanBodyBones.LastBone)
            {
                Transform transform = PuppetAnimator.GetBoneTransform(hbb);
                Quaternion absOffset = GetSkeletonBone(PuppetAnimator, transform.name).rotation;
                // find the absolute offset for the tpose
                while (!ReferenceEquals(transform, _rootJointTransform))
                {
                    transform = transform.parent;
                    absOffset = GetSkeletonBone(PuppetAnimator, transform.name).rotation * absOffset;
                }
                absoluteOffsetMap[(JointId)i] = absOffset;
            }
        }
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        // Always allow skeleton pose to update (even if paused)
        if (!obstacleMovement.inicialized || currentObstacle == null)
            return;

        UpdateSkeletonPose(); // <-- this reads from TrackerHandler and moves the puppet

        //UpdateColliders();

        // Always allow pause gesture detection from Kinect
        CheckForPauseMenu(); // <-- moved BEFORE the pause check!

        // If the game is paused, skip movement inputs and gameplay logic
        if (levelUI.hasPaused)
            return;

        // Skip gameplay input if countdown is still running
        if (!levelUI.isTimerRunning || CountDown.IsRunning)
            return;

        // Only run game input & logic if we're running
        HandleLateralMovement();
        HandleJump();
        HandleCrouching();
        HandleLegHold();
        UpdateColliders();
    }

    private void CheckForPauseMenu()
    {
        // Prevent pause gesture during countdown (unless already paused)
        if (!levelUI.hasPaused && CountDown.IsRunning)
            return;

        if (Time.unscaledTime - lastPauseTime < pauseCooldown)
            return; // still cooling down, don't allow retrigger

        Transform leftShoulder = PuppetAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        Transform leftElbow = PuppetAnimator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        Transform leftWrist = PuppetAnimator.GetBoneTransform(HumanBodyBones.LeftHand);

        Transform rightShoulder = PuppetAnimator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        Transform rightElbow = PuppetAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        Transform rightWrist = PuppetAnimator.GetBoneTransform(HumanBodyBones.RightHand);

        Vector3 leftUpperArm = (leftShoulder.position - leftElbow.position).normalized;
        Vector3 leftLowerArm = (leftElbow.position - leftWrist.position).normalized;

        Vector3 rightUpperArm = (rightShoulder.position - rightElbow.position).normalized;
        Vector3 rightLowerArm = (rightElbow.position - rightWrist.position).normalized;

        float leftElbowAngle = Mathf.Clamp(Vector3.Angle(leftUpperArm, leftLowerArm), 0f, 180f);
        float rightElbowAngle = Mathf.Clamp(Vector3.Angle(rightUpperArm, rightLowerArm), 0f, 180f);

        bool pauseLeft = leftElbowAngle > 50f && leftElbowAngle < 95f;
        bool pauseRight = rightElbowAngle > 50f && rightElbowAngle < 95f;

        bool pauseMovement = pauseLeft && pauseRight;
        bool armsCrossed = leftWrist.position.x < rightWrist.position.x;

        if (pauseMovement && armsCrossed)
        {
            if (!pauseGestureActive) // trigger only once on new gesture
            {
                pauseGestureActive = true;
                lastPauseTime = Time.unscaledTime;
                OnPauseTriggered?.Invoke();
            }
        }
        else
        {
            pauseGestureActive = false; // reset flag when pose is broken
        }
    }

    // Movement Functions //
    private void HandleLateralMovement()
    {
        Transform pelvis = PuppetAnimator.GetBoneTransform(HumanBodyBones.Hips);
        float currentPelvisX = pelvis.position.x;

        float delta = currentPelvisX - previousPelvisX;

        if (Mathf.Abs(delta) > lateralThreshold)
        {
            string direction = delta > 0 ? "left" : "right";
            //Debug.Log("Lateral Movement Detected = " + direction);
        }

            previousPelvisX = currentPelvisX;
    }

    private void HandleJump()
    {
        Transform pelvis = PuppetAnimator.GetBoneTransform(HumanBodyBones.Hips);
        float currentPelvisY = pelvis.position.y;

        if (Time.time < 0.1f)
        {
            lastPelvisY = currentPelvisY;
            return;
        }

        float verticalVelocity = (currentPelvisY - lastPelvisY) / Time.deltaTime;

        // --- Start tracking a potential jump when pelvis goes above threshold ---
        if (!trackingJump && verticalVelocity > upwardVelocityThreshold)
        {
            trackingJump = true;
            jumpStartY = currentPelvisY;
            maxJumpY = currentPelvisY;
            jumpStartTime = Time.time;
        }

        // --- While tracking, update the max height reached ---
        if (trackingJump)
        {
            if (currentPelvisY > maxJumpY)
                maxJumpY = currentPelvisY;

            // Jump finished when pelvis drops below threshold
            if (currentPelvisY < pelvisFallThreshold)
            {
                trackingJump = false;

                float jumpHeight = maxJumpY - jumpStartY;

                float jumpDuration = Time.time - jumpStartTime;

                if (currentObstacle != null && currentObstacle.GetComponent<ObstacleBase>().ObstacleCode.StartsWith("J"))
                {
                    if (jumpHeight > minJumpHeight)
                    {
                        Debug.Log("Jump detected: " + (jumpHeight * 100) + " cm");

                        string attemptInfo = $"Duration: {jumpDuration:F2}s | Jump Height: {(jumpHeight*100):F2} cm";
                        AddAttempt(jumpInfo, attemptInfo);
                    }
                }
            }
        }

        lastPelvisY = currentPelvisY;
    }

    private void HandleCrouching()
    {
        Transform hipLeft = PuppetAnimator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        Transform kneeLeft = PuppetAnimator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        Transform ankleLeft = PuppetAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);

        Transform hipRight = PuppetAnimator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        Transform kneeRight = PuppetAnimator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        Transform ankleRight = PuppetAnimator.GetBoneTransform(HumanBodyBones.RightFoot);

        Vector3 leftThigh = (kneeLeft.position - hipLeft.position).normalized;
        Vector3 leftShin = (ankleLeft.position - kneeLeft.position).normalized;
        Vector3 rightThigh = (kneeRight.position - hipRight.position).normalized;
        Vector3 rightShin = (ankleRight.position - kneeRight.position).normalized;

        float leftKneeAngle = Mathf.Clamp(180 - Vector3.Angle(leftThigh, leftShin), 0f, 180f);
        float rightKneeAngle = Mathf.Clamp(180 - Vector3.Angle(rightThigh, rightShin), 0f, 180f);

        bool currentlyCrouching = leftKneeAngle < crouchKneeAngleThreshold && rightKneeAngle < crouchKneeAngleThreshold;

        if (currentlyCrouching && !isCrouching && !isJumping)
        {
            // Start crouch
            crouchStartTime = Time.time;
            isCrouching = true;
            Debug.Log("Crouch Detected!");
        }

        if (currentlyCrouching && isCrouching)
        {
            // Store angles every frame while crouching
            leftLegAngles.Add(leftKneeAngle);
            rightLegAngles.Add(rightKneeAngle);
        }

        if (!currentlyCrouching && isCrouching)
        {
            // End crouch
            float crouchDuration = Time.time - crouchStartTime;

            if (currentObstacle != null && currentObstacle.GetComponent<ObstacleBase>().ObstacleCode.StartsWith("C"))
            {
                // Calculate stats
                float leftAvg = leftLegAngles.Average();
                float leftMax = leftLegAngles.Max();
                float leftMin = leftLegAngles.Min();

                float rightAvg = rightLegAngles.Average();
                float rightMax = rightLegAngles.Max();
                float rightMin = rightLegAngles.Min();

                string attemptInfo = $"Duration: {crouchDuration:F2}s | " +
                $"Left Avg: {leftAvg:F1}, Max: {leftMax:F1}, Min: {leftMin:F1} | " +
                $"Right Avg: {rightAvg:F1}, Max: {rightMax:F1}, Min: {rightMin:F1}";

                AddAttempt(crouchInfo, attemptInfo);
            }

            // Reset angle lists
            leftLegAngles.Clear();
            rightLegAngles.Clear();

            isCrouching = false;
        }        
    }

    private void HandleLegHold()
    {
        int feetStatus = FeetInGround();

        string legHold = "";

        if (!isCrouching && !isJumping)
        {
            if (feetStatus == -1 && !isLegHoldingLeft && !isLegHoldingRight)
            {
                legHoldStartTime = Time.time;
                isLegHoldingLeft = true;

                Debug.Log("Left Leg Hold Detected!");
            }
            else if (feetStatus == 1 && !isLegHoldingRight && !isLegHoldingLeft)
            {
                legHoldStartTime = Time.time;
                isLegHoldingRight = true;

                Debug.Log("Right Leg Hold Detected!");
            }
            else if ((isLegHoldingLeft || isLegHoldingRight) && (feetStatus == 2 || feetStatus == 0))
            {
                float legHoldDuration = Time.time - legHoldStartTime;

                if (legHoldDuration >= 0.5f) // Only consider leg holds of more then 0.5 seconds
                {
                    Debug.Log($"Leg Hold ended.");

                    string requiredCode = obstacleMovement.currentObstacle?.GetComponent<ObstacleBase>()?.ObstacleCode;

                    bool correctHold =
                        (requiredCode.StartsWith("LHL") && isLegHoldingLeft) ||
                        (requiredCode.StartsWith("LHR") && isLegHoldingRight);

                    if (correctHold)
                    {
                        if (requiredCode.StartsWith("LHL")) legHold = "Left";
                        else if (requiredCode.StartsWith("LHR")) legHold = "Right";

                        string attemptInfo = $"Holded: {legHold} | " + $"Duration: {legHoldDuration:F2}";

                        AddAttempt(holdInfo, attemptInfo);

                        obstacleMovement.AddToHoldTimer(legHoldDuration);
                    }
                }

                isLegHoldingLeft = false;
                isLegHoldingRight = false;
            }
        }
    }

    private bool IsGrounded()
    {
        return FeetInGround() != 0;  // either foot grounded
    }

    private int FeetInGround()
    {
        Transform leftFoot = PuppetAnimator.GetBoneTransform(HumanBodyBones.LeftToes);
        Transform rightFoot = PuppetAnimator.GetBoneTransform(HumanBodyBones.RightToes);

        bool leftGrounded = IsFootGrounded(leftFoot);
        bool rightGrounded = IsFootGrounded(rightFoot);

        if (leftGrounded && rightGrounded) return 2;
        if (leftGrounded && !rightGrounded) return -1;
        if (!leftGrounded && rightGrounded) return 1;
        return 0;
    }

    private bool IsFootGrounded(Transform foot)
    {
        Collider collider = foot.GetComponent<Collider>();
        if (collider == null)
        {
            Debug.LogWarning("Foot has no collider!");
            return false;
        }

        Vector3 rayStart = foot.position + Vector3.up * 0.05f;

        float rayLength = 0.25f;

        int layerMask = ~LayerMask.GetMask("Character");
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayLength, layerMask))
        {
            //Debug.DrawRay(rayStart, Vector3.down * rayLength, Color.red, 0.1f);

            if (!IsPartOfSelf(hit.collider.transform))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsPartOfSelf(Transform hitTransform)
    {
        return hitTransform.root == transform.root;
    }


    // Collider Functions //
    private void UpdateColliders()
    {
        UpdateBodyCollider();
        UpdateLegColliders();
        UpdateFeetColliders();
    }

    private void UpdateBodyCollider()
    {
        Transform head = PuppetAnimator.GetBoneTransform(HumanBodyBones.Head);
        Transform chest = PuppetAnimator.GetBoneTransform(HumanBodyBones.Chest);
        Transform spine = PuppetAnimator.GetBoneTransform(HumanBodyBones.Spine);
        Transform pelvis = PuppetAnimator.GetBoneTransform(HumanBodyBones.Hips);

        if (head == null || chest == null || spine == null || pelvis == null)
        {
            Debug.LogWarning("Missing critical body bones for collider update.");
            return;
        }

        float padding1 = 0.05f;
        float padding2 = 0.2f;

        // Vertical bounds
        float pelvisY = pelvis.position.y - padding1;
        float headY = head.position.y + padding2;
        float height = Mathf.Max(0.1f, headY - pelvisY);

        // Horizontal bounds (width and depth) 
        List<Transform> widthBones = new List<Transform> { pelvis, spine, chest, head };
        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;

        foreach (var t in widthBones)
        {
            Vector3 p = t.position;
            minX = Mathf.Min(minX, p.x);
            maxX = Mathf.Max(maxX, p.x);

            minZ = Mathf.Min(minZ, p.z);
            maxZ = Mathf.Max(maxZ, p.z);
        }

        float width = Mathf.Max(0.1f, maxX - minX);
        float depth = Mathf.Max(0.1f, maxZ - minZ);

        Vector3 bodyColliderCenter = new Vector3(
            (minX + maxX) / 2f,
            pelvisY + (height / 2f),
            (minZ + maxZ) / 2f
        );

        bodyCollider.center = CharacterRootTransform.InverseTransformPoint(bodyColliderCenter);
        bodyCollider.size = new Vector3(width + padding2, height + padding1, depth + padding2);
    }

    private void UpdateLegColliders()
    {
        Transform hipLeft = PuppetAnimator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        Transform kneeLeft = PuppetAnimator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        Transform ankleLeft = PuppetAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);

        Transform hipRight = PuppetAnimator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        Transform kneeRight = PuppetAnimator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        Transform ankleRigth = PuppetAnimator.GetBoneTransform(HumanBodyBones.RightFoot);

        if (hipLeft == null || kneeLeft == null || ankleLeft == null
            || hipRight == null || kneeRight == null || ankleRigth == null)
        {
            Debug.LogWarning("Missing critical leg bones for collider update.");
            return;
        }

        // Left leg
        UpdateLimbSegmentCollider(leftThighCollider, hipLeft, kneeLeft);
        UpdateLimbSegmentCollider(leftShinCollider, kneeLeft, ankleLeft);

        // Right leg
        UpdateLimbSegmentCollider(rightThighCollider, hipRight, kneeRight);
        UpdateLimbSegmentCollider(rightShinCollider, kneeRight, ankleRigth);
    }

    private void UpdateLimbSegmentCollider(BoxCollider collider, Transform start, Transform end)
    {
        float upperY = start.position.y;
        float lowerY = end.position.y;

        float height = Mathf.Max(0.1f, upperY - lowerY);

        List<Transform> widthBones = new List<Transform> { start, end };
        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;

        foreach (var t in widthBones)
        {
            Vector3 p = t.position;
            minX = Mathf.Min(minX, p.x);
            maxX = Mathf.Max(maxX, p.x);

            minZ = Mathf.Min(minZ, p.z);
            maxZ = Mathf.Max(maxZ, p.z);
        }

        float width = Mathf.Max(0.1f, maxX - minX);
        float depth = Mathf.Max(0.1f, maxZ - minZ);

        Vector3 center = new Vector3(
            (minX + maxX) / 2f,
            lowerY + (height / 2f),
            (minZ + maxZ) / 2f
        );

        collider.center = collider.transform.InverseTransformPoint(center);
        collider.size = new Vector3(width, height, depth);
    }

    private void UpdateFeetColliders()
    {
        Transform ankleLeft = PuppetAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
        Transform footLeft = PuppetAnimator.GetBoneTransform(HumanBodyBones.LeftToes);

        Transform ankleRigth = PuppetAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
        Transform footRight = PuppetAnimator.GetBoneTransform(HumanBodyBones.RightToes);

        if (ankleLeft == null || footLeft == null
            || ankleRigth == null || footRight == null)
        {
            Debug.LogWarning("Missing critical feet for collider update.");
            return;
        }

        UpdateSingleFootCollider(leftFootCollider, ankleLeft, footLeft);
        UpdateSingleFootCollider(rightFootCollider, ankleRigth, footRight);
    }

    private void UpdateSingleFootCollider(BoxCollider collider, Transform ankle, Transform foot)
    {
        float ankleY = ankle.position.y;
        float footY = foot.position.y;

        float height = Mathf.Max(0.1f, ankleY - footY);

        // Horizontal bounds (width and depth) 
        List<Transform> widthBones = new List<Transform> { ankle, foot };
        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;

        foreach (var t in widthBones)
        {
            Vector3 p = t.position;
            minX = Mathf.Min(minX, p.x);
            maxX = Mathf.Max(maxX, p.x);

            minZ = Mathf.Min(minZ, p.z);
            maxZ = Mathf.Max(maxZ, p.z);
        }

        float width = Mathf.Max(0.1f, maxX - minX);
        float depth = Mathf.Max(0.1f, maxZ - minZ);

        Vector3 center = new Vector3(
            (minX + maxX) / 2f,
            footY + (height / 2f),
            (minZ + maxZ) / 2f + 0.02f
        );

        collider.center = collider.transform.InverseTransformPoint(center);
        collider.size = new Vector3(width, height + 0.02f, depth + 0.13f);
    }


    // Other Functions //
    private void OnCurrentObstacleChanged(GameObject newObstacle)
    {
        currentObstacle = newObstacle;
        Debug.Log("Updated currentObstacle in PuppetAvatar: " + (newObstacle != null ? newObstacle : null));
    }

    void OnDestroy()
    {
        if (obstacleMovement != null)
            obstacleMovement.OnObstacleChanged -= OnCurrentObstacleChanged;
    }

    public Dictionary<int, string> GetAndClearActionInfo(Dictionary<int, string> actionInfo)
    {
        var clone = new Dictionary<int, string>(actionInfo);
        actionInfo.Clear();
        return clone;
    }

    public void AddAttempt(Dictionary<int, string> actionInfo , string attemptInfo)
    {
        int key = 0;
        while (actionInfo.ContainsKey(key))
        {
            key++;
        }
        actionInfo[key] = attemptInfo;
    }
}
