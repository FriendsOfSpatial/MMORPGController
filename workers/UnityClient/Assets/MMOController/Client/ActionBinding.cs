using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mmocontroller.Client
{
  [Serializable]
  public class ActionBinding
  {
    public enum KeyMode { PUSH, HOLD }

    private const uint MAX_MAPPINGS = 2;
    private const uint MAX_KEYS_PER_MAPPING = 4;

    private List<List<KeyCode[]>> bindings = new List<List<KeyCode[]>>();

    public delegate void ActionHandler();
    public event ActionHandler Handler;

    public KeyMode Mode;
    public String Bindings
    {
        get
        {
            string _bindings = "";
            for (int i = 0; i < bindings.Count; i++)
            {
                _bindings += KeyMap.ReverseMapping(bindings[i]);
                if (i < (bindings.Count - 1))
                {
                    _bindings += ",";
                }
            }
            return _bindings;
        }
        set
        {
            ParseCombinations(value.Split(','));
        }
    }

    private static void NoopHandler() { }

    public ActionBinding(string combinations) : this(combinations, NoopHandler, KeyMode.PUSH) { }
    public ActionBinding(string combinations, KeyMode mode) : this(combinations, NoopHandler, mode) { }
    public ActionBinding(string[] combinations) : this(combinations, NoopHandler, KeyMode.PUSH) { }
    public ActionBinding(string combination, ActionHandler handler) : this(combination.Split(','), handler, KeyMode.PUSH) { }
    public ActionBinding(string combination, ActionHandler handler, KeyMode mode) : this(combination.Split(','), handler, mode) { }

    public ActionBinding(string[] combination, ActionHandler handler, KeyMode mode)
    {
      ParseCombinations(combination);
      Handler += handler;
      this.mode = mode;
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

    public void CheckAsync(Action cb)
    {
      if (Check())
      {
        cb();
      }
    }

    public bool Check()
    {
      bool active = false;

      foreach (List<KeyCode[]> binding in bindings)
      {
        bool check = true;
        foreach (KeyCode[] key in binding)
        {
          bool keyTrigger = false;
          foreach (KeyCode code in key)
          {
            switch (Mode)
            {
              case KeyMode.HOLD:
              keyTrigger = keyTrigger || Input.GetKey(code);
              break;
              case KeyMode.PUSH:
              keyTrigger = keyTrigger || Input.GetKeyDown(code);
              break;
            }
          }

          check = check && keyTrigger;

          // Fail-Fast for non-matches
          if (!check) break;
        }

        if (check)
        {
          active = true;
          break;
        }
      }

      if (active) Handler();
      return active;
    }
  }
}
