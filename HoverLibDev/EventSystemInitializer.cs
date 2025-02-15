using MelonLoader;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HoverMenu
{
    public class EventSystemInitializer : MonoBehaviour
    {
        void Awake()
        {   

            //Looks for the EventSystem, if it doesnt find one it creates one so UI will work.
            EventSystem existingEventSystem = FindObjectOfType<EventSystem>();

            if (existingEventSystem == null)
            {
                GameObject eventSystemObject = new GameObject("EventSystem");
                EventSystem eventSystem = eventSystemObject.AddComponent<EventSystem>();
                StandaloneInputModule inputModule = eventSystemObject.AddComponent<StandaloneInputModule>();
                DontDestroyOnLoad(eventSystemObject);

                MelonLogger.Msg("EventSystem created and added to the scene.");
            }
            else
            {
                MelonLogger.Msg("EventSystem already exists in the scene.");
            }
        }
    }
}
