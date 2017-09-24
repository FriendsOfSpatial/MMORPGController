## About this Recipe

MMO Controller is a Server-Authoratative Keyboard & Mouse controller that provides
similar capabilities to a MMORPG like World of Warcraft. Some features of the
approach are

* Server Authoratative controller to reduce cheating
* Adjustable 3rd-person camera
* Customizable Key Bindings and multiple bindings per action
* Text-to-binding and binding-to-text lookups
* Extensible framework for adding new actions
* Built-in Physics Monitor component for debugging in Unity

## Create Components

### Vector3D

While this is an optional type, it certainly makes things easier going forward
and I highly recommend creating the Vector3D type as well as the extension methods
for converting back and forth between Unity Vectors and Native Vectors.

Create a file named `Vector3D.schema` in your `schema/physics`
folder with the following content

```schemalang
package physics;

type Vector3D {
  float x;
  float y;
  float z;
}
```

Next you'll want to add the convenience methods for converting back and forth
between Native and Unity Vector3 types. In your scripts folder create a new script named `VectorExt.cs`

```c#
namespace Physics.Util {
  public static class VectorUtils {
    public static UnityEngine.Vector3 ToUnityVector3(this Vector3D native)
    {
      return new UnityEngine.Vector3(native.x, native.y, native.z);
    }

    public static Vector3D ToNativeVector3D(this UnityEngine.Vector3 vector)
    {
      return new Vector3D(vector.x, vector.y, vector.z);
    }
  }
}
```

### Physics Monitor

Also optional, but something that will probably help you a lot in tuning things
later on (and also provides some visual evidence that your stuff is working) is
the PhysicsMonitor component and associated MonoBehaviour

Create a file named `PhysicsMonitor.schema` under `schema/physics`

```schemalang
package physics;

import "phyics/Vector3D.schema";

type MovementPhysics {
  physics.Vector3D velocity = 1;
  physics.Vector3D force = 2;
  float speed = 3;
  bool jumping = 4;
  bool grounded = 5;
  bool running = 6;
}

component PhysicsMonitor {
  id = 5000;

  MovementPhysics movement = 1;
}
```

In my case, I separated out the movement physics as a sub-type of the component
to facilitate other physics things that I may want to monitor in the future.

Next, we'll create our associated MonoBehaviour and add it to our PlayerEntityTemplate.

Create a new MonoBehaviour named `PhysicsMonitorReceiver.cs`

```C#
using UnityEngine;
using Improbably.Unity.Visualizer;
using Physics;
using Physics.Util;

namespace Physics.Behaviour
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
```

Finally we'll need to add this component to our `PlayerEntityTemplate`

```C#
      public static BuildPlayerEntity()
      {
        // ...
          .AddComponent(new PhysicsMonitor.Data(new PhysicsMonitorData()), CommonRequirementSets.PhysicsOnly)
        // ...
      }
```
