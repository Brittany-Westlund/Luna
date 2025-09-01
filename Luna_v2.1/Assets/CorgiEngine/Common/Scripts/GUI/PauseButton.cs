using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
    /// <summary>
    /// A simple component meant to be added to the pause button
    /// </summary>
    public class PauseButton : CorgiMonoBehaviour
    {
        void Update()
        {
            // Check if the pause input has been pressed
            if (Input.GetButtonDown("Pause"))
            {
                PauseButtonAction();
            }
        }

        /// Puts the game on pause
        public virtual void PauseButtonAction()
        {
            StartCoroutine(PauseButtonCo());
        }

        /// <summary>
        /// A coroutine used to trigger the pause event
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator PauseButtonCo()
        {
            yield return null;
            // we trigger a Pause event for the GameManager and other classes that could be listening to it too
            CorgiEngineEvent.Trigger(CorgiEngineEventTypes.TogglePause);
        }
    }
}
