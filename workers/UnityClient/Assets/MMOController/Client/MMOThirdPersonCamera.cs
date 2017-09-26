using UnityEngine;
using Improbable.Unity.Visualizer;
using Improbable;

using Mmocontroller.Physics;

namespace MMOController.Client {
  [WorkerType(Improbable.Unity.WorkerPlatform.UnityClient)]
  public class MMOThirdPersonCamera : MonoBehaviour
  {
    [Require] private MovementState.Reader movementState;

    // You can adjust this to be whatever your default follow is.
    private float DefaultFollowYaw = 0.0f;
    private float InitialFollowPitch = 40.0f;

    // Specifies how quick the camera should snap back to follow mode when free look is released
    // and the player begins moving
    public float SnapBackToFollowSpeed = 2.0f;

    // Sensitivity Settings (These should be adjustable by the player)
    public float ScrollWheelSensitivity = 3.0f;
    public float MouseSensitivity = 2.0f;

    // Initial camera starting distance (should persist across sessions)
    public float InitialCameraDistance = 15.0f;

    // Camera pitch and distance limits
    public float MinPitch = 5.0f;
    public float MaxPitch = 70.0f;
    public float MinDistance = 4.0f;
    public float MaxDistance = 20.0f;

    private UnityEngine.Quaternion rotation;
    private float distance;
    private ActionBinding Look = new ActionBinding("LMB", ActionBinding.KeyMode.HOLD);
    private Transform playerCamera;

    public UnityEngine.Quaternion CurrentRotation { get { return rotation; } }
    public float CurrentDistance { get { return distance; } }
    public bool FreeLookMode { get { return Look.Check(); } }

    private void OnEnable()
    {
      playerCamera = UnityEngine.Camera.main.transform;
      playerCamera.transform.parent = transform;
      rotation = UnityEngine.Quaternion.Euler(new Vector3(DefaultFollowYaw, InitialFollowPitch, 0.0f));
      distance = InitialCameraDistance;
    }

    private void Update()
    {
      SelectNextCameraDistance();
      SelectNextCameraRotation();
    }

    private void LateUpdate()
    {
      SetCameraTransform();
    }

    private void SetCameraTransform()
    {
      playerCamera.localPosition = rotation * Vector3.back * distance;
      playerCamera.LookAt(transform.position);
    }

    private void SelectNextCameraDistance()
    {
      var mouseScroll = Input.GetAxis("Mouse ScrollWheel");
      if (!mouseScroll.Equals(0f))
      {
        var distanceChange = distance - mouseScroll * ScrollWheelSensitivity;
        distance = Mathf.Clamp(distanceChange, MinDistance, MaxDistance);
      }
    }

    private void SelectNextCameraRotation()
    {
      if (Look.Check())
      {
        var yaw = (rotation.eulerAngles.y + Input.GetAxis("Mouse X") * MouseSensitivity % 360f);
        var pitch = Mathf.Clamp(rotation.eulerAngles.x - Input.GetAxis("Mouse Y") * MouseSensitivity, MinPitch, MaxPitch);
        rotation = UnityEngine.Quaternion.Euler(new Vector3(pitch, yaw, 0));
      }
      else
      {
        if (movementState.Data.movementState != State.STANDING)
        {
          var pitch = rotation.eulerAngles.x;
          rotation = UnityEngine.Quaternion.Slerp(rotation, UnityEngine.Quaternion.Euler(new Vector3(pitch, DefaultFollowYaw, 0)), Time.deltaTime * SnapBackToFollowSpeed);
        }
      }
    }
  }
}
