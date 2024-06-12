using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class DataCollection : MonoBehaviour
   
{
    // These are currently broken, need to connect
    [SerializeField] private TMP_InputField NameInput;
    [SerializeField] private TMP_InputField NumberInput;
    [SerializeField] private TMP_Dropdown MonthDropdown;
    [SerializeField] private Button TakeDataBtn;
    // Start is called before the first frame update
    void Start()
    {
        TakeDataBtn.onClick.AddListener(SaveData);
    }
    private class UserData
    {
        public string Name;
        public int Number;
        public string Month;
    }
    private void SaveData()
    {
        // should log entry from fields into JSON file
        UserData userData = new UserData();
        {

            userData.Name = NameInput.text;

            if (int.TryParse(NumberInput.text, out int number))
            {
                userData.Number = number;
            }
            else
            {
                Debug.LogError("Invalid Number Input");
            }


            int selectedIndex = MonthDropdown.value;
            userData.Month = MonthDropdown.options[selectedIndex].text;
            Debug.Log($"Name: {userData.Name}, Number: {userData.Number}, Month: {userData.Month}");
        }

        //string json = JsonUtility.ToJson(userData);

        //File.WriteAllText(Application.persistentDataPath + "/UserData.json", json);

        //Debug.Log("Data saved to " + Application.persistentDataPath + "/UserData.json");
    }


    // later functions should interact with button to show data currently logged in JSON
    // Search button near the top should allow for user to search by name for number and month data
}
