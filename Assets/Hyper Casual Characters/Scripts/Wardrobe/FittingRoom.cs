using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class FittingRoom : MonoBehaviour
{
    [SerializeField]
    private GameObject mainBody;
    public List<GameObject> Boots;
    public List<GameObject> Pants;
    public List<GameObject> Chests;
    public List<GameObject> hairs;
    public List<GameObject> faces;

    private Wardrobe wardrobe;
    private int chestIndex = 0;
    private int pantIndex = 0;
    private int bootsIndex = 0;
    private int faceIndex = 0;
    private int hairIndex = 0;

    private GameObject currentChest;
    private GameObject currentpants;
    private GameObject currentboots;
    private GameObject currentFace;
    private GameObject currentHair;

    public InputField filePath;
    public InputField PrefabName;

    private int prefabIndex;

    private void OnEnable()
    {
        wardrobe = mainBody.GetComponent<Wardrobe>();
        StartCoroutine(WaitAndWear());
    }

    private void Update()
    {
        if (Input.GetKeyDown("q"))
            ChangeChestButton(1);
        if (Input.GetKeyDown("a"))
            ChangePantsButton(1);
        if (Input.GetKeyDown("z"))
            ChangeBootsButton(1);
        if (Input.GetKeyDown("e"))
            ChangeFaceButton(1);
        if (Input.GetKeyDown("d"))
            ChangeHairButton(1);
        ////////////////////
         if (Input.GetKeyDown("w"))
        ChangeChestButton(-1);
        if (Input.GetKeyDown("s"))
            ChangePantsButton(-1);
        if (Input.GetKeyDown("x"))
            ChangeBootsButton(-1);
        if (Input.GetKeyDown("r"))
            ChangeFaceButton(-1);
        if (Input.GetKeyDown("f"))
            ChangeHairButton(-1);
    }

    private IEnumerator WaitAndWear()
    {
        ChangeChestButton(1);
        yield return new WaitForSeconds(0.05f);
        ChangePantsButton(1);
        yield return new WaitForSeconds(0.05f);
        ChangeBootsButton(1);
        yield return new WaitForSeconds(0.05f);
        ChangeFaceButton(1);
        yield return new WaitForSeconds(0.05f);
        ChangeHairButton(1);
    }

    public void ChangeChestButton(int vector)
    {
        GetNextIndex(vector, ref chestIndex);

        var chest = Instantiate(Chests[chestIndex]);

        DestroyImmediate(currentChest);

        currentChest = chest;
        wardrobe.Wear(chest, mainBody);
    }

    public void ChangePantsButton(int vector)
    {
        GetNextIndex(vector, ref pantIndex);

        var pants = Instantiate(Pants[pantIndex]);

        DestroyImmediate(currentpants);

        currentpants = pants;
        wardrobe.Wear(pants, mainBody);
    }

    public void ChangeBootsButton(int vector)
    {
        GetNextIndex(vector, ref bootsIndex);

        var boots = Instantiate(Boots[bootsIndex]);

        DestroyImmediate(currentboots);

        currentboots = boots;
        wardrobe.Wear(boots, mainBody);
    }

    public void ChangeFaceButton(int vector)
    {
        GetNextIndex(vector, ref faceIndex);

        var face = Instantiate(faces[faceIndex]);

        DestroyImmediate(currentFace);

        currentFace = face;
        wardrobe.Wear(face, mainBody);
    }

    public void ChangeHairButton(int vector)
    {
        GetNextIndex(vector, ref hairIndex);

        var hair = Instantiate(hairs[hairIndex]);

        DestroyImmediate(currentHair);

        currentHair = hair;
        wardrobe.Wear(hair, mainBody);
    }

    [MenuItem("GameObject/Create Folder")]
    public void SavePrefab()
    {
        if (!AssetDatabase.IsValidFolder(@"Assets\Prefabs"))
        {
            string guid = AssetDatabase.CreateFolder("Assets", "Prefabs");
            string newFolderPath = AssetDatabase.GUIDToAssetPath(guid);
        }
        var name = PrefabName.text == string.Empty ? PrefabName.placeholder.GetComponent<Text>().text : PrefabName.text;
        var path = filePath.text == string.Empty ? filePath.placeholder.GetComponent<Text>().text : filePath.text;

        PrefabUtility.SaveAsPrefabAsset(mainBody, path + "/" + name + ".prefab");
    }
    private void GetNextIndex(int vector, ref int currentIndex)
    {
        var index = currentIndex + vector;

        currentIndex = index >= Pants.Count ? 0 : index;
        if (currentIndex < 0)
            currentIndex = index < 0 ? Pants.Count - 1 : index;
    }
}
