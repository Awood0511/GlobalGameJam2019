﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeScene : MonoBehaviour
{
    public void change(string name)
    {
        Application.LoadLevel(name);   
    }
}
