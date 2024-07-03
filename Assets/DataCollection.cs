using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using Helios.Relay;

public class DataCollection : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nameInput;
    [SerializeField] private TMP_InputField _numberInput;
    [SerializeField] private TMP_Dropdown _monthDropdown;
    [SerializeField] private Button _takeDataBtn;

    private string filePath;
    private UserDataList userDataList = new UserDataList();
    Relay relay = new Relay("https://localhost:9696/", "9I7kkFXNQlgFZvGN");
    RExperience rExperience = new RExperience("Example", "922c09f6-bdd2-4825-95c0-280c0328eba4");
    RGuest rGuest = new RGuest();
 


    // This class is used to store user data
    [System.Serializable]
    private class UserData
    {
        public string Name;
        public int Number;
        public string Month;
    }

    // This class is used to store a list of user data

    private class UserDataList
    {
        public List<UserData> Users = new List<UserData>();
    }

    // Start is called before the first frame update
    void Start()
    {
        filePath = Path.Combine(Application.streamingAssetsPath, "UserDataList.json");
        //    RFile rFile = new RFile { name = "Example.png", path = "StreamingAssets/Examples/ExampleFileName.png" };
        Debug.Log("File Path: " + filePath);
        _takeDataBtn.onClick.AddListener(SaveData);
        LoadUserData();
    }

    private void SaveData()
    {
        // Create a new UserData instance
        UserData userData = new UserData
        {
            Name = _nameInput.text
        };

        // Parse the number input
        if (int.TryParse(_numberInput.text, out int number))
        {
            userData.Number = number;
        }
        else
        {
            Debug.LogError("Invalid Number Input");
            return;
        }

        // Get the selected month from the dropdown
        int selectedIndex = _monthDropdown.value;
        userData.Month = _monthDropdown.options[selectedIndex].text;

        // Add the new user data to the list
        userDataList.Users.Add(userData);

        // Save the updated list to the JSON file
        SaveUserData();

        // Log the data
        Debug.Log($"Name: {userData.Name}, Number: {userData.Number}, Month: {userData.Month}");
    }

    private void SaveUserData()
    {
        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath);
        }
        string json = JsonUtility.ToJson(userDataList, true);
        Debug.Log("Saving Data: " + json);
        File.WriteAllText(filePath, json);
        Debug.Log(filePath);
        Debug.Log("Data saved to file");
    }

    private void LoadUserData()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            userDataList = JsonUtility.FromJson<UserDataList>(json);
            Debug.Log("Data loaded from file: " + json);
        }
        else
        {
            Debug.Log("File does not exist, creating new file.");
        }
    }
}

    // later functions should interact with button to show data currently logged in JSON
    // Search button near the top should allow for user to search by name for number and month data

