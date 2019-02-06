using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace Gamekit2D
{
    public class PressurePad : MonoBehaviour
    {
       // bool rightPad = true;               //used to chaged mass of box in puzzle 1 (1 pads 1 cube)

        public GameObject box;              //gets the game object of the push for puzzle 1 (2 pads 1 cube

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
                        Debug.Log("is this happening?");
                        OnPressed.Invoke();
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
                        OnRelease.Invoke();
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
                        Debug.Log("is this happening?");
                        
                        //Debug.Log("Pressed " + rightPad + " " + gameObject.name);  //used for debuging objects

                        //when pad0 is pressed by the Puzzle1Box it will changed the mass of the box to 29
                        if (gameObject.name == "Puzzle1Pad0")
                        {
                            //rightPad = false;
                            box = GameObject.Find("Puzzle1Box");
                            box.GetComponent<Rigidbody2D>().mass = 29f;

                            /*puzzles 1 door is opned by 30 mass, this will changed puzzles 1 mass to 29, 
                             your mass which is 1 + box mass(29) will open the door
                            */
                        }

                        OnPressed.Invoke();
                        m_EventFired = true;
                    }
                }
                else
                {

                    if (m_PreviousWasPressed)
                    {
                        //Debug.Log("Released " + rightPad + " 1 " + gameObject.name);  //used for debuging objects
                        m_PreviousWasPressed = false;
                        m_ActivationFrameCount = 1;
                    }
                    else
                        m_ActivationFrameCount += 1;

                    if (m_ActivationFrameCount > DELAYEDFRAME_COUNT && m_EventFired)
                    {
                        //Debug.Log("Released " + rightPad + " 2 " + gameObject.name);  //used for debuging objects
                        OnRelease.Invoke();
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