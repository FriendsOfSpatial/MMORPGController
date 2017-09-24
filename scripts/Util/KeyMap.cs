using System.Collections.Generic;
using UnityEngine;

namespace Util
{
  public static class KeyMap
  {
      private static IDictionary<string, KeyCode[]> Keymap = new Dictionary<string, KeyCode[]>();
      private static IDictionary<KeyCode[], string> ReverseMap = new Dictionary<KeyCode[], string>();

      public static KeyCode[] GetCodes(string symbol)
      {
          if (Keymap.ContainsKey(symbol))
          {
              return Keymap[symbol];
          }
          return null;
      }

      public static bool IsMapped(string symbol)
      {
          return Keymap.ContainsKey(symbol);
      }

      public static string ReverseMapping(List<KeyCode[]> binding)
      {
          List<string> keyList = new List<string>();
          foreach (KeyCode[] key in binding)
          {
              keyList.Add(ReverseMap[key]);
          }
          return string.Join("+", keyList.ToArray());
      }

      static KeyMap()
      {
          Keymap.Add("A", new KeyCode[] { KeyCode.A });
          Keymap.Add("B", new KeyCode[] { KeyCode.B });
          Keymap.Add("C", new KeyCode[] { KeyCode.C });
          Keymap.Add("D", new KeyCode[] { KeyCode.D });
          Keymap.Add("E", new KeyCode[] { KeyCode.E });
          Keymap.Add("F", new KeyCode[] { KeyCode.F });
          Keymap.Add("G", new KeyCode[] { KeyCode.G });
          Keymap.Add("H", new KeyCode[] { KeyCode.H });
          Keymap.Add("I", new KeyCode[] { KeyCode.I });
          Keymap.Add("J", new KeyCode[] { KeyCode.J });
          Keymap.Add("K", new KeyCode[] { KeyCode.K });
          Keymap.Add("L", new KeyCode[] { KeyCode.L });
          Keymap.Add("M", new KeyCode[] { KeyCode.M });
          Keymap.Add("N", new KeyCode[] { KeyCode.N });
          Keymap.Add("O", new KeyCode[] { KeyCode.O });
          Keymap.Add("P", new KeyCode[] { KeyCode.P });
          Keymap.Add("Q", new KeyCode[] { KeyCode.Q });
          Keymap.Add("R", new KeyCode[] { KeyCode.R });
          Keymap.Add("S", new KeyCode[] { KeyCode.S });
          Keymap.Add("T", new KeyCode[] { KeyCode.T });
          Keymap.Add("U", new KeyCode[] { KeyCode.U });
          Keymap.Add("V", new KeyCode[] { KeyCode.V });
          Keymap.Add("W", new KeyCode[] { KeyCode.W });
          Keymap.Add("X", new KeyCode[] { KeyCode.X });
          Keymap.Add("Y", new KeyCode[] { KeyCode.Y });
          Keymap.Add("Z", new KeyCode[] { KeyCode.Z });
          Keymap.Add("1", new KeyCode[] { KeyCode.Alpha1, KeyCode.Keypad1 });
          Keymap.Add("2", new KeyCode[] { KeyCode.Alpha2, KeyCode.Keypad2 });
          Keymap.Add("3", new KeyCode[] { KeyCode.Alpha3, KeyCode.Keypad3 });
          Keymap.Add("4", new KeyCode[] { KeyCode.Alpha4, KeyCode.Keypad4 });
          Keymap.Add("5", new KeyCode[] { KeyCode.Alpha5, KeyCode.Keypad5 });
          Keymap.Add("6", new KeyCode[] { KeyCode.Alpha6, KeyCode.Keypad6 });
          Keymap.Add("7", new KeyCode[] { KeyCode.Alpha7, KeyCode.Keypad7 });
          Keymap.Add("8", new KeyCode[] { KeyCode.Alpha8, KeyCode.Keypad8 });
          Keymap.Add("9", new KeyCode[] { KeyCode.Alpha9, KeyCode.Keypad9 });
          Keymap.Add("0", new KeyCode[] { KeyCode.Alpha0, KeyCode.Keypad0 });
          Keymap.Add("-", new KeyCode[] { KeyCode.Minus, KeyCode.Underscore });
          Keymap.Add("+", new KeyCode[] { KeyCode.Equals, KeyCode.Plus });
          Keymap.Add("`", new KeyCode[] { KeyCode.BackQuote });
          Keymap.Add("F1", new KeyCode[] { KeyCode.F1 });
          Keymap.Add("F2", new KeyCode[] { KeyCode.F2 });
          Keymap.Add("F3", new KeyCode[] { KeyCode.F3 });
          Keymap.Add("F4", new KeyCode[] { KeyCode.F4 });
          Keymap.Add("F5", new KeyCode[] { KeyCode.F5 });
          Keymap.Add("F6", new KeyCode[] { KeyCode.F6 });
          Keymap.Add("F7", new KeyCode[] { KeyCode.F7 });
          Keymap.Add("F8", new KeyCode[] { KeyCode.F8 });
          Keymap.Add("F9", new KeyCode[] { KeyCode.F9 });
          Keymap.Add("F10", new KeyCode[] { KeyCode.F10 });
          Keymap.Add("F11", new KeyCode[] { KeyCode.F11 });
          Keymap.Add("F12", new KeyCode[] { KeyCode.F12 });
          Keymap.Add("Ctrl", new KeyCode[] { KeyCode.LeftControl, KeyCode.RightControl });
          Keymap.Add("Shift", new KeyCode[] { KeyCode.LeftShift, KeyCode.RightShift });
          Keymap.Add("Alt", new KeyCode[] { KeyCode.LeftAlt, KeyCode.RightAlt });
          Keymap.Add("Caps", new KeyCode[] { KeyCode.CapsLock });
          Keymap.Add("Bksp", new KeyCode[] { KeyCode.Backspace });
          Keymap.Add("Pgup", new KeyCode[] { KeyCode.PageUp });
          Keymap.Add("Pgdn", new KeyCode[] { KeyCode.PageDown });
          Keymap.Add("Home", new KeyCode[] { KeyCode.Home });
          Keymap.Add("End", new KeyCode[] { KeyCode.End });
          Keymap.Add("Ins", new KeyCode[] { KeyCode.Insert });
          Keymap.Add("Del", new KeyCode[] { KeyCode.Delete });
          Keymap.Add("Up", new KeyCode[] { KeyCode.UpArrow });
          Keymap.Add("Down", new KeyCode[] { KeyCode.DownArrow });
          Keymap.Add("Left", new KeyCode[] { KeyCode.LeftArrow });
          Keymap.Add("Right", new KeyCode[] { KeyCode.RightArrow });
          Keymap.Add("Space", new KeyCode[] { KeyCode.Space });

          foreach (var mapping in Keymap)
          {
              if (!ReverseMap.ContainsKey(mapping.Value))
              {
                  ReverseMap.Add(mapping.Value, mapping.Key);
              }
          }
      }
  }
}
