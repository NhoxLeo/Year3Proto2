using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
struct OptionData
{
    // Display
    bool enemyIndicators;
    bool environmentTooltips;
    bool fullscreenMode;
    bool vSync;
    Vector2 resolutionData;

    // Graphics
    int quality;
    int textureQuality;
    int antiAliasing;
    int shadowQuality;
    bool ambientOcclusion;

    // Audio
    float masterVolume;
    float musicVolume;
    float soundEffectsVolume;
    float ambientVolume;
    bool waveStartHorn;

    // Controls
    bool mouseEdgeCameraControl;
    float cameraZoomSensitivity;
    float cameraMovementSensitivity;
};

public class Options : MonoBehaviour
{
    [Header("Categories")]
    [SerializeField] private Transform categoryPrefab;
    [SerializeField] private Transform categoryParent;
    [SerializeField] private OptionCategory[] categories;
    private List<OptionCategoryObject> categoryObjects;

    [Header("Buttons")]
    [SerializeField] private Transform buttonParent;
    [SerializeField] private Transform buttonPrefab;

    private void Start()
    {
        categoryObjects = new List<OptionCategoryObject>();

        for (int i = 0; i < categories.Length; i++)
        {
            // Get element based on index
            OptionCategory optionCategory = categories[i];

            // Instansiate Button
            Transform categoryButton = Instantiate(buttonParent, buttonParent);
            Button button = categoryButton.GetComponent<Button>();
            button.image.sprite = optionCategory.icon;

            // Instantiate Category
            Transform categoryObject = Instantiate(categoryPrefab, categoryParent);
            OptionCategoryObject optionCategoryObject = categoryObject.GetComponent<OptionCategoryObject>();

            categoryObjects.Add(optionCategoryObject);
        }
    }

    public void Select(int index) 
    {
        // If current index is not active
        if (!categoryObjects[index].gameObject.activeSelf)
        {
            for (int i = 0; i < categoryObjects.Count; i++)
            {
                categoryObjects[i].gameObject.SetActive((index == i) ? true : false);
            }
        }
    }

    public void Deserialize()    
    {

    }

    public void Serialize()
    {
        // Save data to disk and in game.
    }
}
