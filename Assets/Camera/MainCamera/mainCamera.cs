using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Cameras
{
    [ExecuteInEditMode]
    public class mainCamera : PivotBasedCameraRig
    {
        // This script is designed to be placed on the root object of a camera rig,
        // comprising 3 gameobjects, each parented to the next:

        // 	Camera Rig
        // 		Pivot
        // 			Camera

        [SerializeField] private float m_MoveSpeed = 1f;                      // How fast the rig will move to keep up with the target's position.
        [Range(0f, 10f)] [SerializeField] private float m_TurnSpeed = 1.5f;   // How fast the rig will rotate from user input.
        [SerializeField] private float m_TiltMax = 75f;                       // The maximum value of the x axis rotation of the pivot.
        [SerializeField] private float m_TiltMin = 45f;                       // The minimum value of the x axis rotation of the pivot.
        [SerializeField] private float m_RollSpeed = 0.2f;// How fast the rig will roll (around Z axis) to match target's roll.
        [SerializeField] private bool m_FollowVelocity = false;// Whether the rig will rotate in the direction of the target's velocity.
        [SerializeField] private bool m_FollowTilt = true; // Whether the rig will tilt (around X axis) with the target.
        [SerializeField] private float m_SpinTurnLimit = 90;// The threshold beyond which the camera stops following the target's rotation. (used in situations where a car spins out, for example)
        [SerializeField] private float m_TargetVelocityLowerLimit = 4f;// the minimum velocity above which the camera turns towards the object's velocity. Below this we use the object's forward direction.
        [SerializeField] private float m_SmoothTurnTime = 0.2f; // the smoothing for the camera's rotation

        private float m_LookAngle;                    // The rig's y axis rotation.
        private float m_TiltAngle;                    // The pivot's x axis rotation.
        private const float k_LookDistance = 100f;    // How far in front of the pivot the character's look target is.
		private Vector3 m_PivotEulers;
		private Quaternion m_PivotTargetRot;
		private Quaternion m_TransformTargetRot;
        private float m_LastFlatAngle; // The relative angle of the target and the rig from the previous frame.
        private float m_CurrentTurnAmount; // How much to turn the camera
        private float m_TurnSpeedVelocityChange; // The change in the turn speed velocity
        private Vector3 m_RollUp = Vector3.up;// The roll of the camera around the z axis ( generally this will always just be up )

        protected override void Awake()
        {
            base.Awake();
            // Lock or unlock the cursor.
            Cursor.lockState = true ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = false;

            m_PivotEulers = m_Pivot.rotation.eulerAngles;

	        m_PivotTargetRot = m_Pivot.transform.localRotation;
			m_TransformTargetRot = transform.localRotation;

        }


        protected void Update()
        {
            HandleRotationMovement();

            if (Input.GetButton("Ctrl"))
            {
                Cursor.lockState = false ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = true ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = false;
            }
        }

        protected override void FollowTarget(float deltaTime)
        {
            // if no target, or no time passed then we quit early, as there is nothing to do
            if (!(deltaTime > 0) || m_Target == null)
            {
                return;
            }

            // initialise some vars, we'll be modifying these in a moment
            var targetForward = m_Target.forward;
            var targetUp = m_Target.up;

            if (m_FollowVelocity && Application.isPlaying)
            {
                // in follow velocity mode, the camera's rotation is aligned towards the object's velocity direction
                // but only if the object is traveling faster than a given threshold.

                if (targetRigidbody.velocity.magnitude > m_TargetVelocityLowerLimit)
                {
                    // velocity is high enough, so we'll use the target's velocty
                    targetForward = targetRigidbody.velocity.normalized;
                    targetUp = Vector3.up;
                }
                else
                {
                    targetUp = Vector3.up;
                }
                m_CurrentTurnAmount = Mathf.SmoothDamp(m_CurrentTurnAmount, 1, ref m_TurnSpeedVelocityChange, m_SmoothTurnTime);
            }
            else
            {
                // we're in 'follow rotation' mode, where the camera rig's rotation follows the object's rotation.

                // This section allows the camera to stop following the target's rotation when the target is spinning too fast.
                // eg when a car has been knocked into a spin. The camera will resume following the rotation
                // of the target when the target's angular velocity slows below the threshold.
                var currentFlatAngle = Mathf.Atan2(targetForward.x, targetForward.z) * Mathf.Rad2Deg;
                if (m_SpinTurnLimit > 0)
                {
                    var targetSpinSpeed = Mathf.Abs(Mathf.DeltaAngle(m_LastFlatAngle, currentFlatAngle)) / deltaTime;
                    var desiredTurnAmount = Mathf.InverseLerp(m_SpinTurnLimit, m_SpinTurnLimit * 0.75f, targetSpinSpeed);
                    var turnReactSpeed = (m_CurrentTurnAmount > desiredTurnAmount ? .1f : 1f);
                    if (Application.isPlaying)
                    {
                        m_CurrentTurnAmount = Mathf.SmoothDamp(m_CurrentTurnAmount, desiredTurnAmount,
                                                             ref m_TurnSpeedVelocityChange, turnReactSpeed);
                    }
                    else
                    {
                        // for editor mode, smoothdamp won't work because it uses deltaTime internally
                        m_CurrentTurnAmount = desiredTurnAmount;
                    }
                }
                else
                {
                    m_CurrentTurnAmount = 1;
                }
                m_LastFlatAngle = currentFlatAngle;
            }

            // camera position moves towards target position:
            transform.position = Vector3.Lerp(transform.position, m_Target.position, deltaTime * m_MoveSpeed);

            // camera's rotation is split into two parts, which can have independend speed settings:
            // rotating towards the target's forward direction (which encompasses its 'yaw' and 'pitch')
            if (!m_FollowTilt)
            {
                targetForward.y = 0;
                if (targetForward.sqrMagnitude < float.Epsilon)
                {
                    targetForward = transform.forward;
                }
            }
            var rollRotation = Quaternion.LookRotation(targetForward, m_RollUp);

            // and aligning with the target object's up direction (i.e. its 'roll')
            m_RollUp = m_RollSpeed > 0 ? Vector3.Slerp(m_RollUp, targetUp, m_RollSpeed * deltaTime) : Vector3.up;
            transform.rotation = Quaternion.Lerp(transform.rotation, rollRotation, 0);
        }

        private void HandleRotationMovement()
        {
            if (Time.timeScale < float.Epsilon)
			return;
            
            // Read the user input
            var x = CrossPlatformInputManager.GetAxis("Mouse X");
            var y = CrossPlatformInputManager.GetAxis("Mouse Y");
        

            if (!Input.GetButton("Ctrl"))
            {
                // Adjust the look angle by an amount proportional to the turn speed and horizontal input.
                m_LookAngle += x * m_TurnSpeed;
            
                m_TiltAngle -= y * m_TurnSpeed;
            }

            m_TiltAngle = Mathf.Clamp(m_TiltAngle, -m_TiltMin, m_TiltMax);

            // Rotate the rig (the root object) around Y axis only:
            m_TransformTargetRot = Quaternion.Euler(0f, m_LookAngle, 0f);

            // Tilt input around X is applied to the pivot (the child of this object)
			m_PivotTargetRot = Quaternion.Euler(m_TiltAngle, m_PivotEulers.y , m_PivotEulers.z);

            m_Pivot.localRotation = m_PivotTargetRot;
            transform.localRotation = m_TransformTargetRot;
        }
    }
}
