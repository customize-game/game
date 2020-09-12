using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public List<GameObject> characterList;
    public int selectedCharacterIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        characterList = new List<GameObject>();
        characterList.AddRange(GameObject.FindGameObjectsWithTag("CharacterModel"));

        bool firstFlg = true;
        foreach (GameObject character in characterList) {
            if (firstFlg)
            {
                firstFlg = false;
            }
            else {
                character.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickSelectLeft() {
        Debug.Log("left");
        if (selectedCharacterIndex != 0)
        {
            characterList[selectedCharacterIndex].SetActive(false);
            characterList[--selectedCharacterIndex].SetActive(true);
        }
    }

    public void OnClickSelectRight()
    {
        Debug.Log("right");
        if (selectedCharacterIndex < characterList.Count - 1)
        {
            characterList[selectedCharacterIndex].SetActive(false);
            characterList[++selectedCharacterIndex].SetActive(true);
        }
    }

}
