using UnityEngine;
using Improbable.Unity.Visualizer;
using Mmocontroller.Physics;
using Mmocontroller.Client;


namespace Mmocontroller.Client {
  /**
   * Attach this behavior to your player prefab (don't forget to export prefabs)
   * to allow control of their avatar.
   */
  [WorkerType(Improbable.Unity.WorkerPlatform.UnityClient)]
  public class CharacterMovementController : MonoBehaviour
  {
    // /schema/mmocontroller/client/MovementControllerState.schema
    [Require] private MovementControllerState.Writer controller;

    public ActionBinding Forward = new ActionBinding("W", ActionBinding.KeyMode.HOLD);
    public ActionBinding Backward = new ActionBinding("S", ActionBinding.KeyMode.HOLD);
    public ActionBinding StrafeLeft = new ActionBinding("Q", ActionBinding.KeyMode.HOLD);
    public ActionBinding StrafeRight = new ActionBinding("E", ActionBinding.KeyMode.HOLD);
    public ActionBinding TurnLeft = new ActionBinding("A", ActionBinding.KeyMode.HOLD);
    public ActionBinding TurnRight = new ActionBinding("D", ActionBinding.KeyMode.HOLD);
    public ActionBinding Jump = new ActionBinding("Space", ActionBinding.KeyMode.PUSH);
    public ActionBinding RunModifier = new ActionBinding("Shift", ActionBinding.KeyMode.HOLD);
    public ActionBinding MouseTurn = new ActionBinding("RMB", ActionBinding.KeyMode.HOLD);
    public ActionBinding MouseMove = new ActionBinding("LMB+RMB", ActionBinding.KeyMode.HOLD);

    // You could create other moement types here, like crawling or flying and assign
    // the bindings.

    public float MouseSensitivity = 2f;
    public bool InvertMouseY = false;

    private void OnEnable()
    {
      // You could initialize the controller bindings from a configuration here.
    }

    private void Update()
    {
      var update = new MovementControllerState.Update();

      // Clear the state on every frame
      ResetState(update);

      if (MouseMove.Check())
      {
        update.SetMoveYAxis(1.0f);
        update.SetTurnYAxis(Input.GetAxis("Mouse X"));
      }
      else if (MouseTurn.Check())
      {
        update.SetTurnYAxis(Input.GetAxis("Mouse X"));
      }

      if (Forward.Check())
      {
        update.SetMoveYAxis(1.0f);
      }
      else if (Backward.Check())
      {
        update.SetMoveYAxis(-1.0f);
      }

      if (StrafeLeft.Check())
      {
        update.SetMoveXAxis(-1.0f);
      }
      else if (StrafeRight.Check())
      {
        update.SetMoveXAxis(1.0f);
      }

      if (TurnLeft.Check())
      {
        update.SetTurnYAxis(-1.0f);
      }
      else if (TurnRight.Check())
      {
        update.SetTurnYAxis(1.0f);
      }

      RunModifier.CheckAsync(() => update.SetRun(true));
      Jump.CheckAsync(() => update.SetJump(true));

      controller.Send(update);
    }

    private void ResetState(MovementControllerState.Update update)
    {
      update.SetJump(false);
      update.SetMoveXAxis(0);
      update.SetMoveYAxis(0);
      update.SetRun(false);
      update.SetTurnYAxis(0);
    }
  }

}
