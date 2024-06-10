using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class DataCollection : MonoBehaviour
   
{
    // These are currently broken, need to connect
    [SerializeField] private InputField NameInput;
    [SerializeField] private InputField NumberInput;
    [SerializeField] private Dropdown MonthDropdown;
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
           // Name = NameInput.text;
          //  Number = int.Parse(NumberInput.text);
         //   Month = MonthDropdown.options[MonthDropdown.value].text;
        }

        string json = JsonUtility.ToJson(userData);

        File.WriteAllText(Application.persistentDataPath + "/UserData.json", json);

        Debug.Log("Data saved to " + Application.persistentDataPath + "/UserData.json");
    }


    // later functions should interact with button to show data currently logged in JSON
    // Search button near the top should allow for user to search by name for number and month data
}
