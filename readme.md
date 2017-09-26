*Warning: This is still a WiP and not complete yet*

## About the MMOController

MMO Controller is a Server-Authoratative Keyboard & Mouse controller that provides
similar capabilities to a MMORPG like World of Warcraft. Some features of the
approach are

* Server Authoratative controller to reduce cheating
* Adjustable 3rd-person camera
* Customizable Key Bindings and multiple bindings per action
* Text-to-binding and binding-to-text lookups
* Extensible framework for adding new actions
* Built-in Physics Monitor component for debugging in Unity

## Server-Authoratative Controller

1. Add the following behaviours to your Player prefab
   * `MMOThirdPersonCamera`
   * `MovementControlDispatcher`
   * `PhysicsMonitorReceiver`
   * `RigidbodyCharacterController`
1. Add the following components to your player entity template
   * `.AddComponent(new MovementControllerState.Data(), CommonRequirementSets.specificClient(clientId))`
   * `.AddComponent(new MovementState.Data(), CommonRequirementSets.PhysicsOnly)`
   * `.AddComponent(new PhysicsMonitor.Data(), CommonRequirementSets.PhysicsOnly)`

## Third Person Camera

In the [MMOThirdPersonCamera](scripts/client/MMOThirdPersonCamera.cs) script, you'll
notice there are several public variables that you can set. You'll want to play
with these to match your own gameplay mechanics. For instance, you may with to
set `MinDistance` to `0.0f` which will allow you to zoom in to a "First Person"
mode with your camera.

### Using the Third Person Camera

Simply attach the script to your player prefab. The script uses the UnityClient's
main camera. You'll also need to ensure that the `MovementState` component is added
to your `PlayerEntityTemplate`

```C#
  .AddComponent(new MovementState.Data(new MovementStateData()), CommonRequirementSets.PhysicsOnly);
```

### Attributes

`DefaultFollowYaw` - The angle on the Y-Axis to slerp back to when free-look mode is
disabled.

`InitialFollowPitch` - The angle on the X-Axis to initialize the camera to.

`InitialCameraDistance` - The initial distance for the camera

`SnapBackToFollowSpeed` - How quickly to snap back to `DefaultFollowYaw` when free-look
mode is disabled.

`ScrollWheelSensitivity` - How quickly the scroll wheel should zoom the camera

`MouseSensitivity` - How sensitive the mouse axis' are

`MinPitch` and `MaxPitch` - Constraints on camera look for pitch

`MinDistance` and `MaxDistance` - Constraints on camera distance

## Key Bindings framework

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

### KeyMap

The primary purpose of the `KeyMap` is to provide a lookup table from `string` to an array
of `KeyCode` and visa-versa. The aim of such a dictionary is to allow you to create a
simple customization (and storage) interface for custom key-bindings and handle some things
like equality between Left Shift and Right Shift. You can customize the static section
in this map however you'd like. Say for instance you wanted to differentiate between
the shift keys described above.

Presently in the KeyMap, the mapping

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

### Action Bindings

Now that we've got our "Mappable Keys" sorted out and supported, we'll need a way
to actually build a binding between a combination of keys and an associated action.
This is the purpose of the [ActionBinding](scripts/Client/ActionBinding.cs)
class.

Let's say, for example you'd like to create a binding for moving forward (which we'll
be doing later in this recipe). You'd like to simply bind this to the "W" key on the
keyboard. You'd also like to create a complex binding for casting a fireball. Finally,
you'd like to bind two difference key combinations to the SelfHeal action. Furthermore
the MoveForward action should bind as a key that needs to be held down, while you
only want to detect the initial keypress for something like casting a spell.

```C#
   var MoveForward = new ActionBinding("W", ActionBinding.KeyMode.HOLD);
   var Fireball = new ActionBinding("Ctrl+Shift+4", ActionBinding.KeyMode.PUSH);
   var SelfHeal = new ActionBinding("Ctrl+Shift+5,Ctrl+H", ActionBinding.KeyMode.PUSH);
```

Behind the scenes, this will create a mapping that looks like

```
  MoveForward => Input.GetKey
  (
    KeyCode.W
  )

  Fireball => Input.GetKeyDown
  (
    ( KeyCode.LeftCtrl || KeyCode.RightCtrl ) &&
    ( KeyCode.LeftAlt) || KeyCode.RightAlt) &&
    ( KeyCode.LeftShift || KeyCode.RightShift ) &&
    ( KeyCode.Numpad4 || KeyCode.Alpha4 )
  )

  SelfHeal => Input.GetKeyDown
  (
    ( KeyCode.LeftControl || KeyCode.RightControl ) &&
    ( KeyCode.LeftShift || KeyCode.RightShift ) &&
    ( KeyCode.Numpad5 || KeyCode.Alpha5 )
  )
  ||
  (
    ( KeyCode.LeftControl || KeyCode.RightControl ) &&
    ( KeyCode.H )
  )
```

Multiple keypresses in a single binding are joined with a `+` while multiple bindings
for the same action are joined by `,`. Additionally, the difference between a key
that must be held down vs. a single keypress is expressed the the modes `KeyMode.HOLD`
or `KeyMode.PUSH`.

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

In some situations, you may want to make things more dynamic; so with this framework, you can set a delegate method to be invoked when matched.

```C#
var action1 = new ActionBinding("Ctrl+1", Action_1, ActionBinding.KeyMode.PUSH);
action1.Check();

// ...

private void Action_1() {
  // do whatever is assigned to action 1
}
```

Or you can simply invoke the `CheckAsync` method and provide a callback

```C#
action1.CheckAsync(Action_1);
```
