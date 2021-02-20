using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SEP
{
    public class InputFlag
    {
        public bool Start;
        public bool Update;
        public bool End;

        private KeyCode[] _keyCodes;
        private int keyCodeLength;

        public InputFlag(params KeyCode[] keyCodes)
        {
            _keyCodes = keyCodes;
            keyCodeLength = keyCodes.Length;
        }

        public void SetFlags()
        {
            for (int i = 0; i < keyCodeLength; i++)
            {
                if (Input.GetKeyDown(_keyCodes[i]))
                {
                    Start = true;
                    break;
                }
            }


            for (int i = 0; i < keyCodeLength; i++)
            {
                if (Input.GetKey(_keyCodes[i]))
                {
                    Update = true;
                    break;
                }
            }


            for (int i = 0; i < keyCodeLength; i++)
            {
                if (Input.GetKeyUp(_keyCodes[i]))
                {
                    End = true;
                    break;
                }
            }
        }

        public void ResetStartEndFlags()
        {
            Start = false;
            End = false;
        }
    }
}