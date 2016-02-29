using UnityEngine;
using System.Collections;

namespace Boardgame.UI {
    public class SetConfigItem : MonoBehaviour {

        public string Which;
        public string Value;

        public void Set(bool ok) {
            if(ok) Configuration.Config.SetValue(Which, Value);
        }

        public void Set(float value) {
            Configuration.Config.SetValue(Which, ((int)value).ToString());
        }

        public void Set(string value) {
            Configuration.Config.SetValue(Which, value);
        }
    }
}