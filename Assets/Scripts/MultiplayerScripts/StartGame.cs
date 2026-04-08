using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;


namespace Unity.Multiplayer.Widgets
{
    [RequireComponent(typeof(Button))]
   public class StartGame : MonoBehaviour
    {
        Button m_Button;

        void Start()
        {
            m_Button = GetComponent<Button>();
            m_Button.onClick.AddListener(GameStart);
            SetButtonActive();
        }

        public void OnSessionLeft()
        {
            SetButtonActive();
        }

        public void OnSessionJoined()
        {
            SetButtonActive();
        }

        
        void SetButtonActive()
        {
            // idk
        }

        async void GameStart()
        {
            // TODO: Add Scene transition here... 
        }
    }

}
// TODO: Implement a script that loads the main GameScene when pressed. 
