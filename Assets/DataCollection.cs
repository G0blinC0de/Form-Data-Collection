using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class DataCollection : MonoBehaviour
   
{
    [SerializeField] private Text NameInput;
    [SerializeField] private Button TakeDataBtn;
    // Start is called before the first frame update
    void Start()
    {
        UserData userData = new UserData();

    }

    private class UserData
    {
        public string NameData;
        public int Number;
        public Selectable Month;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("TakeDataBtn"))
        {
           // Record data in fields to UserData when TakeDataBtn is clicked to JSON file

          // UserData.NameData = NameInput.text;
        }
    }
}
