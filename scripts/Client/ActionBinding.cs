using UnityEngine;

namespace Client
{
  public class ActionBinding
  {
      private const uint MAX_MAPPINGS = 2; // Change this to allow more than 2 bindings per action
      private const uint MAX_KEYS_PER_MAPPING = 4; // Change this to allow more than 4 keys per binding

      public List<List<KeyCode[]>> bindings = new List<List<KeyCode[]>>();

      // Action callacks for delegating
      public delegate void ActionHandler();
      public event ActionHandler Handler;

      private static void NoopHandler() { }

      public ActionBinding(string combinations) : this (combinations, NoopHandler) {}
      public ActionBinding(string[] combinations) : this(combinations, NoopHandler) {}
      public ActionBinding(string combination, ActionHandler handler) : this(combination.Split(','), handler) {}

      public ActionBinding(string[] combination, ActionHandler handler)
      {
          ParseCombinations(combination);
          Handler += handler;
      }

      private void ParseCombinations(string[] combinations)
      {
          if (combinations.Length > MAX_MAPPINGS)
          {
              Debug.LogErrorFormat("Too many mappings, only {0} are allowed per action.", MAX_MAPPINGS);
              return;
          }

          for (int combination = 0; combination < combinations.Length; combination++)
          {
              string keys = combinations[combination];
              List<KeyCode[]> resolved = ResolveBinding(keys);
              bindings.Add(resolved);
          }
      }

      private List<KeyCode[]> ResolveBinding(string binding)
      {
          List<KeyCode[]> resolved = new List<KeyCode[]>();

          string[] keys = binding.Split('+');
          if (keys.Length > MAX_KEYS_PER_MAPPING)
          {
              Debug.LogErrorFormat("Too many keys in binding {0} - Ignoring Binding");
              return null;
          }

          for (int i = 0; i < keys.Length; i++)
          {
              string key = keys[i].Trim();
              if (KeyMap.IsMapped(key))
              {
                  KeyCode[] resolvedCodes = KeyMap.GetCodes(key);
                  resolved.Add(resolvedCodes);
              }
              else
              {
                  Debug.LogErrorFormat("Invalid Key {0} - Ignoring Mapping {1}", key, binding);
                  return null;
              }
          }

          return resolved;
      }

      public bool Check()
      {
          foreach (List<KeyCode[]> binding in bindings)
          {
              bool check = true;
              foreach (KeyCode[] key in binding)
              {
                  bool keyTrigger = false;
                  foreach (KeyCode code in key)
                  {
                      keyTrigger = keyTrigger || Input.GetKey(code);
                  }

                  check = check && keyTrigger;

                  // Fail-Fast for non-matches
                  if (!check) break;
              }

              if (check) return true;
          }

          return false;
      }
  }
}
