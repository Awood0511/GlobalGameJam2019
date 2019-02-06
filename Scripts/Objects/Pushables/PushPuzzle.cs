using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Sprites;

namespace Gamekit2D
{
    public class PushPuzzle : MonoBehaviour
    {

        public enum ActivationType
        {
            ItemCount, ItemMass
        }

        public PlatformCatcher platformCatcher;
        public ActivationType activationType;
        public int requiredCount;
        public float requiredMass;
        public Sprite deactivatedBoxSprite;
        public Sprite activatedBoxSprite;
        public SpriteRenderer[] boxes;
        public UnityEvent OnPressed;
        public UnityEvent OnRelease;

        protected bool m_EventFired;


        //bug in 17.3 make rigidbody loose all contacts when sprites of different size/pivot are swapped in spriterenderer
        //so we delay (de)activation to "ignore" any outlier single frame problem 
        static int DELAYEDFRAME_COUNT = 2;
        protected int m_ActivationFrameCount = 0;
        protected bool m_PreviousWasPressed = false;

#if UNITY_EDITOR
        protected GUIStyle errorStyle = new GUIStyle();
        protected GUIStyle errorBackgroundStyle = new GUIStyle();
#endif
        // Code to control push puzzle
        [SerializeField] GameObject light1;
        [SerializeField] GameObject light2;
        [SerializeField] GameObject light3;
        [SerializeField] GameObject elev;
        public Sprite lightOn;
        public Sprite lightOff;
        [SerializeField] int type;
        public static bool state1 = false;
        public static bool state2 = false;
        public static bool state3 = false;

        //change the bools depending on the pad number
        void puzzleChange()
        {
            Debug.Log("Changing Puzzle from pad " + type);

            //pad 1 logic
            if(type == 1)
            {
                //state 0
                if (state1 == false && state2 == false && state3 == false)
                {
                    //do nothing
                }
                //state 1/3/5/7
                else if (state1 == false && state2 == false && state3 == true ||
                    state1 == false && state2 == true && state3 == true ||
                    state1 == true && state2 == false && state3 == true ||
                    state1 == true && state2 == true && state3 == true)
                {
                    //toggle pad 1 and 3
                    if (state1 == false)
                        state1 = true;
                    else
                        state1 = false;

                    if (state3 == false)
                        state3 = true;
                    else
                        state3 = false;
                }
                //state 2/4/6
                else
                {
                    //toggle pad 2
                    if (state2 == false)
                        state2 = true;
                    else
                        state2 = false;
                }
            }
            //pad 2 logic
            else if(type == 2)
            {
                if (state1 == false)
                    state1 = true;
                else
                    state1 = false;

                if (state3 == false)
                    state3 = true;
                else
                    state3 = false;
            }
            //pad 3 logic
            else
            {
                bool temp = state3;
                state3 = state2;
                state2 = state1;
                state1 = temp;
            }
            //call sprite change and door check
            spriteChange();
            elevCheck();
        }

        //change torch sprites to match the bool values
        void spriteChange()
        {
            //check light 1
            if(state1 == false)
            {
                light1.GetComponent<SpriteRenderer>().sprite = lightOff;
            }
            else
            {
                light1.GetComponent<SpriteRenderer>().sprite = lightOn;
            }
            //check light 2
            if (state2 == false)
            {
                light2.GetComponent<SpriteRenderer>().sprite = lightOff;
            }
            else
            {
                light2.GetComponent<SpriteRenderer>().sprite = lightOn;
            }
            //check light 1
            if (state3 == false)
            {
                light3.GetComponent<SpriteRenderer>().sprite = lightOff;
            }
            else
            {
                light3.GetComponent<SpriteRenderer>().sprite = lightOn;
            }
        }

        //change the door if all are true
        void elevCheck()
        {
            if (state1 == true && state2 == true && state3 == true)
                elev.GetComponent<MovingPlatform>().speed = 5;
        }
    

        void FixedUpdate()
        {
            if (activationType == ActivationType.ItemCount)
            {
                if (platformCatcher.CaughtObjectCount >= requiredCount)
                {
                    if (!m_PreviousWasPressed)
                    {
                        m_PreviousWasPressed = true;
                        m_ActivationFrameCount = 1;
                    }
                    else
                        m_ActivationFrameCount += 1;

                    if (m_ActivationFrameCount > DELAYEDFRAME_COUNT && !m_EventFired)
                    {
                        Invoke("puzzleChange", 0);
                        m_EventFired = true;
                    }
                }
                else
                {
                    if (m_PreviousWasPressed)
                    {
                        m_PreviousWasPressed = false;
                        m_ActivationFrameCount = 1;
                    }
                    else
                        m_ActivationFrameCount += 1;

                    if (m_ActivationFrameCount > DELAYEDFRAME_COUNT && m_EventFired)
                    {

                        m_EventFired = false;
                    }
                }
            }
            else
            {
                if (platformCatcher.CaughtObjectsMass >= requiredMass)
                {
                    if (!m_PreviousWasPressed)
                    {
                        m_PreviousWasPressed = true;
                        m_ActivationFrameCount = 1;
                    }
                    else
                        m_ActivationFrameCount += 1;


                    if (m_ActivationFrameCount > DELAYEDFRAME_COUNT && !m_EventFired)
                    {
                        Invoke("puzzleChange", 0);
                        m_EventFired = true;
                    }
                }
                else
                {

                    if (m_PreviousWasPressed)
                    {
                        m_PreviousWasPressed = false;
                        m_ActivationFrameCount = 1;
                    }
                    else
                        m_ActivationFrameCount += 1;

                    if (m_ActivationFrameCount > DELAYEDFRAME_COUNT && m_EventFired)
                    {
                        m_EventFired = false;
                    }
                }
            }

            for (int i = 0; i < boxes.Length; i++)
            {
                boxes[i].sprite = platformCatcher.HasCaughtObject(boxes[i].gameObject) ? activatedBoxSprite : deactivatedBoxSprite;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Rigidbody2D rb = GetComponentInChildren<Rigidbody2D>();
            if (rb == null)
                return;

            if (rb.bodyType == RigidbodyType2D.Static && GetComponentInParent<MovingPlatform>() != null)
            {
                errorStyle.alignment = TextAnchor.MiddleLeft;
                errorStyle.fontSize = Mathf.FloorToInt(18 * (1.0f / HandleUtility.GetHandleSize(transform.position)));
                errorStyle.normal.textColor = Color.white;

                Handles.Label(transform.position + Vector3.up * 1.5f + Vector3.right, "ERROR : Rigidbody body type on that pressure plate is set to Static!\n It won't move with the moving platform. Change it to Kinematic.", errorStyle);

                Handles.color = Color.red;
                Handles.DrawWireDisc(transform.position, Vector3.back, 0.5f);
                Handles.color = Color.white;
                Handles.DrawLine(transform.position + Vector3.up * 1.0f + Vector3.right, transform.position);
            }
        }
#endif
    }
}