using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boardgame.Agent {
    public class Switch {
        bool value = false;
        public void Enable() {
            value = true;
        }

        public bool Check() {
            if(value) {
                value = false;
                return true;
            }
            return false;
        }

        public bool Peek() {
            return value;
        }
    }
}
