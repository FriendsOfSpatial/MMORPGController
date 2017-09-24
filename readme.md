*Warning: This is still a WiP and not complete yet*

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
namespace Physics.Util
{
  public static class VectorUtils
  {
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

Don't forget to add the `PhysicsMonitorReceiver` behaviour to your Player Prefab and export your prefabs within Unity.

### Key Bindings framework

One of the things that makes games like World of Warcraft such a joy to players is
the ability to completely customize the gameplay experience through setting up action
bindings that accomodate your own style and the various gaming input devices that
exist. For instance, when I play WoW I use my Razer MMO mouse that has 12 user-assignable
thumb buttons on the side of the mouse as well as a gaming keyboard with programmable
keys.

Within the game client, I can customize the `Fireball` action to be
bound to the key combination `Ctrl+Shift+F`, and then bind the same key combination to
one of the programmable keys on my gaming device. In robust game designs that allow
players to perform potentially hundreds of different actions depending on what's
happening at that precise moment, this level of customization is imperative to the
success of the game.

#### KeyMap

First you'll need to add the [KeyMap](../blob/master/scripts/Util/KeyMap.cs) file to
your scripts directory. Rather than putting that whole file here, I'll briefly discuss
what it is and how it works.

The primary purpose of the `KeyMap` is to provide a lookup table from `string` to an array
of `KeyCode` and visa-versa. The aim of such a dictionary is to allow you to create a
simple customization (and storage) interface for custom key-bindings and handle some things
like equality between Left Shift and Right Shift. You can customize the static section
in this map however you'd like. Say for instance you wanted to differentiate between
the shift keys described above.

Presently in the KapMap, the mapping

```C#
Keymap.Add("Shift", new KeyCode[] { KeyCode.LeftShift, KeyCode.RightShift });
```

Specifies that a binding for "Shift" would match on either `KeyCode.LeftShift || KeyCode.RightShift`. You could simply replace this line with

```C#
Keymap.Add("Left Shift", new KeyCode[] { KeyCode.LeftShift });
Keymap.Add("Right Shift", new KeyCode[] { KeyCode.RightShift });
```

Then you could have bindings mapped to `Ctrl+Left Shift+B` and `Ctrl+Right Shift+B`
that would do different things in your controller.

#### Action Bindings

Now that we've got our "Mappable Keys" sorted out and supported, we'll need a way
to actually build a binding between a combination of keys and an associated action.
This is the purpose of the [ActionBinding](../blob/master/scripts/Client/ActionBinding.cs)
class. Like the KeyMap, there's no need to post the entire block of code here, but
you'll need to understand how it works.

Let's say, for example you'd like to create a binding for moving forward (which we'll
be doing later in this recipe). You'd like to simply bind this to the "W" key on the
keyboard. You'd also like to create a complex binding for casting a fireball.

```C#
   var MoveForward = new ActionBinding("W");
   var Fireball = new ActionBinding("Ctrl+Alt+Shift+4");
```
Behind the scenes, this will create a mapping that looks like

```
  MoveForward => ( KeyCode.W )
  Fireball => (
    ( KeyCode.LeftCtrl || KeyCode.RightCtrl ) &&
    ( KeyCode.LeftAlt) || KeyCode.RightAlt) &&
    ( KeyCode.LeftShift || KeyCode.RightShift ) &&
    ( KeyCode.Numpad4 || KeyCode.Alpha4 )
  )
```

As you can see, it greatly reduces the complexity of building complex keyboard
bindings.

In your controller, you'd simply execute the `Check()` method on any actions to
determine whether the binding is active on the controller.

 ```C#
    if (MoveForward.Check())
    {
      // Move the character forward
    }

    // ...

    if (Fireball.Check())
    {
      // Cast a fireball
    }
  ```
