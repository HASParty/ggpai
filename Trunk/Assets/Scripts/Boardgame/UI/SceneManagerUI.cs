using System;
using UnityEngine;

namespace Boardgame.UI {
    /// <summary>
    /// Loads the selected scene
    /// TODO: make it not hard coded which scene is to be loaded someday
    /// </summary>
    public class SceneManagerUI : Singleton<SceneManagerUI> {
        public void PlayButton() {
            //UnityEngine.SceneManagement.SceneManager.LoadScene("SpaceBoardgame");
        }
    }
}
