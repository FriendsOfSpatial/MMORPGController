using UnityEngine;
using Improbably.Unity.Visualizer;
using MMOControler.Physics;
using MMOController.Physics.Util;

namespace Mmocontroller.Client
{
  [WorkerType(Improbably.Unity.WorkerPlatform.UnityClient)]
  public class PhysicsMonitorReceiver : MonoBehaviour
  {
    [Require] PhysicsMonitor.Reader monitor;

    // Create some fields to show up in our inspector (you could also create a serializable MovementPhysics wrapper for this)
    public Vector3 Velocity;
    public Vector3 Force;
    public float Speed;
    public bool Jumping;
    public bool Grounded;

    void Update()
    {
      Velocity = monitor.Data.movement.velocity.ToUnityVector3();
      Force = monitor.Data.movement.force.ToUnityVector3();
      Speed = monitor.Data.movement.speed;
      Jumping = monitor.Data.movement.jumping;
      Grounded = monitor.data.movement.grounded;
    }
  }
}
