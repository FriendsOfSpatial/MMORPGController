using System;
using UnityEngine;
using Improbable.Unity.Visualizer;

using Mmocontroller.Client;
using Mmocontroller.Physics;

namespace Mmocontroller.Physics.controller
{
  /**
   * This is a port of the Unity RigidbodyCharacterController from the Standard
   * Assets package to be a Server-Authoratative Rigidbody Character Controller
   * that works on the SpatialOS platform. To use this controller, simply add
   * this behaviour to your player prefab, add the required components to the player
   * entity and fire up your game. 
   */
  [WorkerType(Improbable.Unity.WorkerPlatform.UnityWorker)]
  [RequireComponent(typeof(Rigidbody))]
  [RequireComponent(typeof(CapsuleCollider))]
  public class RigidbodyCharacterController : MonoBehaviour
  {
    [Require] MovementControllerState.Reader controller;
    [Require] PhysicsMonitor.Writer monitor;
    [Require] MovementState.Writer state;

    public MovementSettings movementSettings = new MovementSettings();
    public AdvancedSettings advancedSettings = new AdvancedSettings();

    private Rigidbody m_Rigidbody { get { return GetComponent<Rigidbody>(); } }
    private CapsuleCollider m_Capsule { get { return GetComponent<CapsuleCollider>(); } }
    private float m_YRotation;
    private Vector3 m_GroundContactNormal;
    private bool m_Jump, m_PreviouslyGrounded, m_Jumping, m_IsGrounded;

    public Vector3 Velocity { get { return m_Rigidbody.velocity; } }
    public bool Grounded { get { return m_IsGrounded; } }
    public bool Jumping { get { return m_Jumping; } }
    public bool Running { get { return movementSettings.Running; } }

    private void Update()
    {
      RotateView();
      if (controller.Data.jump && !m_Jump)
      {
        m_Jump = true;
      }
    }

    private void FixedUpdate()
    {
      MovementState.Update stateUpdate = new MovementState.Update();

      GroundCheck();
      Vector2 input = new Vector2(controller.Data.moveXAxis, controller.Data.moveYAxis);
      movementSettings.UpdateDesiredTargetSpeed(input, controller.Data.run);

      Vector3 force = new Vector3();

      if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && (advancedSettings.airControl || m_IsGrounded))
      {
        Vector3 desiredMove = m_Rigidbody.transform.forward * input.y + m_Rigidbody.transform.right * input.x;
        desiredMove = Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal).normalized;

        desiredMove.x = desiredMove.x * movementSettings.CurrentTargetSpeed;
        desiredMove.y = desiredMove.y * movementSettings.CurrentTargetSpeed;
        desiredMove.z = desiredMove.z * movementSettings.CurrentTargetSpeed;

        if (m_Rigidbody.velocity.sqrMagnitude < (movementSettings.CurrentTargetSpeed * movementSettings.CurrentTargetSpeed))
        {
          m_Rigidbody.AddForce(desiredMove * SlopeMultiplier(), ForceMode.Impulse);
          force += desiredMove * SlopeMultiplier();
        }

        if (controller.Data.run) stateUpdate.SetMovementState(State.RUNNING);
        else stateUpdate.SetMovementState(State.WALKING)
      }
      else
      {
        stateUpdate().SetMovementState(State.STANDING));
      }

      if (m_IsGrounded)
      {
        m_Rigidbody.drag = 5f;

        if (m_Jump)
        {
          m_Rigidbody.drag = 0f;
          m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, 0f, m_Rigidbody.velocity.z);
          m_Rigidbody.AddForce(new Vector3(0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
          m_Jumping = true;
          stateUpdate.SetMovementState(State.JUMPING);
        }

        if (!m_Jumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && m_Rigidbody.velocity.magnitude < 1f)
        {
          m_Rigidbody.Sleep();
        }
      }
      else
      {
        m_Rigidbody.drag = 0f;
        if (m_PreviouslyGrounded && !m_Jumping)
        {
          StickToGroundHelper();
        }
      }
      m_Jump = false;

      PhysicsMonitor.Update update = new PhysicsMonitor.Update();
      update.SetMovement(new MovementPhysics(m_Rigidbody.velocity.ToNativeVector3(), force.ToNativeVector3(), movementSettings.CurrentTargetSpeed, m_Jumping, m_IsGrounded));
      monitor.Send(update);

      if (state.Data.MovementState != stateUpdate.MovementState)
      {
        state.Send(stateUpdate);
      }
    }

    private float SlopeMultiplier()
    {
      float angle = Vector3.Angle(m_GroundContactNormal, Vector3.up);
      return movementSettings.SlopeCurveModifier.Evaluate(angle);
    }

    private void StickToGroundHelper()
    {
      RaycastHit hitInfo;
      if (UnityEngine.Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
      ((m_Capsule.height / 2f) - m_Capsule.radius) +
      advancedSettings.stickToGroundHelperDistance, UnityEngine.Physics.AllLayers, QueryTriggerInteraction.Ignore))
      {
        if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
        {
          m_Rigidbody.velocity = Vector3.ProjectOnPlane(m_Rigidbody.velocity, hitInfo.normal);
        }
      }
    }

    private void GroundCheck()
    {
      m_PreviouslyGrounded = m_IsGrounded;
      RaycastHit hitInfo;
      if (UnityEngine.Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
      ((m_Capsule.height / 2f) - m_Capsule.radius) + advancedSettings.groundCheckDistance, UnityEngine.Physics.AllLayers, QueryTriggerInteraction.Ignore))
      {
        m_IsGrounded = true;
        m_GroundContactNormal = hitInfo.normal;
      }
      else
      {
        m_IsGrounded = false;
        m_GroundContactNormal = Vector3.up;
      }
      if (!m_PreviouslyGrounded && m_IsGrounded && m_Jumping)
      {
        m_Jumping = false;
      }
    }

    private void RotateView()
    {
      float oldYRotation = transform.eulerAngles.y;

      // TODO: Tune for mouse v. keyboard
      movementSettings.CurrentTurnSpeed = movementSettings.KeyboardTurnSpeed;

      var m_CharacterTargetRot = m_Rigidbody.transform.localRotation * UnityEngine.Quaternion.Euler(0f, controller.Data.turnYAxis * movementSettings.CurrentTurnSpeed, 0f);

      if (movementSettings.TurnSmoothing)
      {
        m_Rigidbody.transform.localRotation = UnityEngine.Quaternion.Slerp(m_Rigidbody.transform.localRotation, m_CharacterTargetRot, movementSettings.TurnSmoothingTime * Time.deltaTime);
      }
      else
      {
        m_Rigidbody.transform.localRotation = m_CharacterTargetRot;
      }

      if (m_IsGrounded || advancedSettings.airControl)
      {
        UnityEngine.Quaternion velRotation = UnityEngine.Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
        m_Rigidbody.velocity = velRotation * m_Rigidbody.velocity;
      }
    }

    [Serializable]
    public class MovementSettings
    {
      public float ForwardSpeed = 20.0f;
      public float BackwardSpeed = 10.0f;
      public float StrafeSpeed = 16.0f;
      public float RunMultiplier = 2.0f;
      public float JumpForce = 30f;
      public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
      public float CurrentTargetSpeed = 0.0f;
      public float CurrentTurnSpeed = 0.0f;
      public float KeyboardTurnSpeed = 2f;
      public float MouseTurnSensitivity = 2f;
      public float TurnSmoothingTime = 5f;
      public bool TurnSmoothing = false;

      private bool m_Running;

      public void UpdateDesiredTargetSpeed(Vector2 input, bool run)
      {
        if (input.x > 0 || input.y > 0)
        {
          // Strafe
          CurrentTargetSpeed = StrafeSpeed;
        }
        if (input.y < 0)
        {
          // Backwards
          CurrentTargetSpeed = BackwardSpeed;
        }
        if (input.y > 0)
        {
          // Forward
          CurrentTargetSpeed = ForwardSpeed;
        }

        if (run)
        {
          CurrentTargetSpeed += RunMultiplier;
          m_Running = true;
        }
        else
        {
          m_Running = false;
        }
      }

      public bool Running { get { return m_Running; } }
    }

    [Serializable]
    public class AdvancedSettings
    {
      public float groundCheckDistance = 0.01f;
      public float stickToGroundHelperDistance = 0.5f;
      public float slowDownRate = 20f;
      public bool airControl;
      public float shellOffset = 0.1f;
    }
  }
}
