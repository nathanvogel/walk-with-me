using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Firebase;
using Firebase.Unity.Editor;
using Firebase.Database;


public class PersonData {
    public Vector3 pos;
    public Vector3 rot;
    public string id;

    PersonData(Vector3 pos, Vector3 rot){
        this.pos = pos;
        this.rot = rot;
        id = SystemInfo.deviceUniqueIdentifier;
    }

    // convenience
    public static PersonData CreatePersonData(Transform t){
        return new PersonData(t.position, t.rotation.eulerAngles);
    }
}

public class DatabaseConnection : MonoBehaviour {

    void Start()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://newp-f426c.firebaseio.com/");
        StartCoroutine(_SendData());
    }

    // Coroutine
    IEnumerator _SendData() {

        var root = FirebaseDatabase.DefaultInstance.RootReference;
        var people = root.Child("people");
        while(true) {            
            string id = SystemInfo.deviceUniqueIdentifier;

            PersonData cd = PersonData.CreatePersonData(Camera.main.transform);
            string scd = JsonUtility.ToJson(cd);

            people.Child(cd.id).SetValueAsync(scd);

            //people[cd.id].SetValueAsync(scd);

            // Wait one second
            yield return new WaitForSeconds(1f);
        }
    }

}