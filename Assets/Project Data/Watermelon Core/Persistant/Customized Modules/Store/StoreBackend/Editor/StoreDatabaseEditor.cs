using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using Type = System.Type;
using System.Collections.Generic;

namespace Watermelon
{
    //Store Module v0.9.2
    [CustomEditor(typeof(StoreDatabase))]
    public class StoreDatabaseEditor : WatermelonEditor
    {
        private readonly string PRODUCTS_PROP_NAME = "products";
        private readonly string GUN_SKIN_PRICE_PROP_NAME = "gunSkinPrice";
        private readonly string HAT_SKIN_PRICE_PROP_NAME = "hatSkinPrice";
        private readonly string COINS_FOR_ADS_AMOUNT_PROP_NAME = "coinsForAdsAmount";

        private SerializedProperty productsProperty;
        private SerializedProperty gunSkinPriceProperty;
        private SerializedProperty hatSkinPriceProperty;
        private SerializedProperty coinsForAdsAmountProperty;        

        private Type[] allowedTypes;
        private string[] typeNames;

        private int selectedType;

        private string selectedObjectName;

        private SerializedProperty selectedObject;
        private Editor selectedProductEditor;
        private static int selectedObjectInstanceID = -1;

        private GUIContent filterGUIContent;

        private StoreDatabase storeDatabase;

        private FilterMenuItem[] filterMenuItems;

        private GenericMenu filterGenericMenu;

        protected override void OnEnable()
        {
            // Get properties
            productsProperty = serializedObject.FindProperty(PRODUCTS_PROP_NAME);
            gunSkinPriceProperty = serializedObject.FindProperty(GUN_SKIN_PRICE_PROP_NAME);
            hatSkinPriceProperty = serializedObject.FindProperty(HAT_SKIN_PRICE_PROP_NAME);
            coinsForAdsAmountProperty = serializedObject.FindProperty(COINS_FOR_ADS_AMOUNT_PROP_NAME);

            // Get store product types
            allowedTypes = Assembly.GetAssembly(typeof(StoreProduct)).GetTypes().Where(type => type.IsClass && !type.IsAbstract && (type.IsSubclassOf(typeof(StoreProduct)) || type.Equals(typeof(StoreProduct)))).ToArray();
            typeNames = new string[allowedTypes.Length];

            for (int i = 0; i < allowedTypes.Length; i++)
            {
                typeNames[i] = Regex.Replace(allowedTypes[i].ToString(), "([a-z]) ?([A-Z])", "$1 $2");
            }

            // Cache store database
            storeDatabase = (StoreDatabase)target;

            // Create filter menu items
            List<FilterMenuItem> tempFilterMenuItems = new List<FilterMenuItem>();
            System.Array storeProductTypes = System.Enum.GetValues(typeof(StoreProductType));
            foreach(var productType in storeProductTypes)
            {
                StoreProductType tempProductType = (StoreProductType)productType;

                tempFilterMenuItems.Add(new FilterMenuItem(tempProductType.ToString(), new ProductTypeFilter(tempProductType)));
            }
            tempFilterMenuItems.Add(new FilterMenuItem("Unknown", new ProductUnknownTypeFilter()));
            filterMenuItems = tempFilterMenuItems.ToArray();
        }

        protected override void Styles()
        {
            filterGUIContent = new GUIContent(EditorStylesExtended.GetTexture("icon_filter", EditorStylesExtended.IconColor));
        }

        public override void OnInspectorGUI()
        {
            InitStyles();

            serializedObject.Update();

            EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

            EditorGUILayoutCustom.Header("SETTINGS");
            EditorGUILayout.PropertyField(gunSkinPriceProperty);
            EditorGUILayout.PropertyField(hatSkinPriceProperty);

            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(coinsForAdsAmountProperty);

            EditorGUILayout.EndVertical();
            GUILayout.Space(10f);


            EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);
            EditorGUILayout.BeginHorizontal(EditorStylesExtended.padding05, GUILayout.Height(21), GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("PRODUCTS", EditorStylesExtended.boxHeader, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();

            if(GUILayout.Button(filterGUIContent, GUILayout.Width(20), GUILayout.Height(20)))
            {
                CreateFilterMenu();
                filterGenericMenu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();

            selectedType = EditorGUILayout.Popup("Product Type", selectedType, typeNames);
            EditorGUILayout.BeginHorizontal();
            selectedObjectName = EditorGUILayout.TextField("Object Name", selectedObjectName);
            if (GUILayout.Button("Add", GUILayout.Height(18f)))
            {
                if (!string.IsNullOrEmpty(selectedObjectName))
                {
                    GUI.FocusControl(null);

                    CreateProduct(allowedTypes[selectedType], selectedObjectName);
                }
                else
                {
                    Debug.LogWarning("[Store Database]: Object name can't be empty!");
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            //Display objects array box with fixed size
            EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box, GUILayout.ExpandWidth(true));
            int productsCount = productsProperty.arraySize;
            int unfilteredCount = 0;
            for (int i = 0; i < productsCount; i++)
            {
                int index = i;
                SerializedProperty objectProperty = productsProperty.GetArrayElementAtIndex(i);

                if (objectProperty.objectReferenceValue != null)
                {
                    SerializedObject referenceObject = new SerializedObject(objectProperty.objectReferenceValue);

                    // Filter
                    bool drawElement = false;
                    if(filterMenuItems.Length > 0)
                    {
                        for (int f = 0; f < filterMenuItems.Length; f++)
                        {
                            if (filterMenuItems[f].isActive)
                            {
                                if (filterMenuItems[f].serializedPropertyFilter.Validate(referenceObject))
                                {
                                    drawElement = true;

                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        drawElement = true;
                    }

                    if(drawElement)
                    {
                        bool isLevelSelected = IsObjectSelected(objectProperty);

                        EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

                        Rect clickRect = EditorGUILayout.BeginHorizontal();

                        string title = referenceObject.FindProperty("productName").stringValue;
                        EditorGUILayout.LabelField(string.IsNullOrEmpty(title) ? objectProperty.objectReferenceValue.name.Replace(".asset", "") : title);

                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button("=", EditorStyles.miniButton, GUILayout.Width(16), GUILayout.Height(16)))
                        {
                            GenericMenu menu = new GenericMenu();

                            int productId = referenceObject.FindProperty("id").intValue;

                            menu.AddItem(new GUIContent("Remove"), false, delegate
                            {
                                if (EditorUtility.DisplayDialog("This product will be removed!", "Are you sure?", "Remove", "Cancel"))
                                {
                                    UnselectObject();

                                    Object removedObject = objectProperty.objectReferenceValue;

                                    productsProperty.RemoveFromObjectArrayAt(index);

                                    AssetDatabase.RemoveObjectFromAsset(removedObject);

                                    DestroyImmediate(removedObject, true);

                                    AssetDatabase.SaveAssets();

                                    EditorUtility.SetDirty(target);

                                    return;
                                }
                            });

                            menu.AddSeparator("");

                            menu.AddItem(new GUIContent("Unlock"), false, delegate
                            {
                                UnlockProduct(productId);
                            });

                            menu.AddItem(new GUIContent("Lock"), false, delegate
                            {
                                LockProduct(productId);
                            });

                            menu.AddSeparator("");

                            if (i > 0)
                            {
                                menu.AddItem(new GUIContent("Move up"), false, delegate
                                {
                                    productsProperty.MoveArrayElement(index, index - 1);
                                    serializedObject.ApplyModifiedProperties();

                                    if (selectedObject != null)
                                        UnselectObject();
                                });
                            }
                            else
                            {
                                menu.AddDisabledItem(new GUIContent("Move up"));
                            }


                            if (i + 1 < productsCount)
                            {
                                menu.AddItem(new GUIContent("Move down"), false, delegate
                                {
                                    productsProperty.MoveArrayElement(index, index + 1);
                                    serializedObject.ApplyModifiedProperties();

                                    if (selectedObject != null)
                                        UnselectObject();
                                });
                            }
                            else
                            {
                                menu.AddDisabledItem(new GUIContent("Move down"));
                            }

                            menu.ShowAsContext();
                        }

                        GUILayout.Space(5);

                        if (GUI.Button(clickRect, GUIContent.none, GUIStyle.none))
                        {
                            SelectedObject(objectProperty, i);

                            return;
                        }

                        EditorGUILayout.EndHorizontal();

                        if (selectedObject != null && selectedObjectInstanceID != -1 && selectedObjectInstanceID == objectProperty.objectReferenceInstanceIDValue)
                        {
                            GUILayout.Space(3);
                            EditorGUILayout.LabelField(GUIContent.none, EditorStylesExtended.editorSkin.horizontalSlider);
                            GUILayout.Space(-10);

                            EditorGUILayout.BeginVertical();

                            if (selectedProductEditor == null)
                                Editor.CreateCachedEditor(selectedObject.objectReferenceValue, null, ref selectedProductEditor);

                            selectedProductEditor.OnInspectorGUI();

                            EditorGUILayout.EndVertical();
                        }

                        EditorGUILayout.EndVertical();

                        unfilteredCount++;
                    }
                }
            }

            if (productsCount == 0)
            {
                EditorGUILayout.HelpBox("Tab is empty, add products first!", MessageType.Warning);
            }
            else if(unfilteredCount == 0)
            {
                EditorGUILayout.HelpBox("Products with selected filters dont exist!", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        private void UnselectObject()
        {
            GUI.FocusControl(null);

            if (selectedProductEditor != null)
                DestroyImmediate(selectedProductEditor, true);
            selectedProductEditor = null;

            selectedObject = null;
            selectedObjectInstanceID = -1;
        }

        private void SelectedObject(SerializedProperty serializedProperty, int index)
        {
            GUI.FocusControl(null);

            if (selectedProductEditor != null)
                DestroyImmediate(selectedProductEditor, true);
            selectedProductEditor = null;

            //Check if current selected object is equals to new and unselect it
            if (selectedObject != null && selectedObject.objectReferenceInstanceIDValue == serializedProperty.objectReferenceInstanceIDValue)
            {
                selectedObject = null;
                selectedObjectInstanceID = -1;

                return;
            }

            if (serializedProperty != null)
            {
                selectedObjectInstanceID = serializedProperty.objectReferenceInstanceIDValue;
                selectedObject = serializedProperty;
            }
        }

        private void ClearProducts()
        {
            StoreSaveData storeData = LoadData();

            storeData.ClearUnlockedProductsData();

            SaveData(storeData);
        }

        private void UnlockProduct(int id)
        {
            StoreSaveData storeData = LoadData();

            storeData.UnlockProduct(id);

            SaveData(storeData);
        }

        private void LockProduct(int id)
        {
            StoreSaveData storeData = LoadData();

            storeData.RemoveProductFormUnlocked(id);

            SaveData(storeData);
        }

        private void SaveData(StoreSaveData storeData)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/" + StoreController.SAVE_FILE_NAME, FileMode.Create);

            bf.Serialize(file, storeData);

            file.Close();
        }

        private StoreSaveData LoadData()
        {
            if (File.Exists(Application.persistentDataPath + "/" + StoreController.SAVE_FILE_NAME))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/" + StoreController.SAVE_FILE_NAME, FileMode.Open);

                StoreSaveData saveData = (StoreSaveData)bf.Deserialize(file);

                file.Close();

                return saveData;
            }

            return new StoreSaveData();
        }

        private bool IsObjectSelected(SerializedProperty serializedProperty)
        {
            return selectedObject != null && selectedObjectInstanceID == serializedProperty.objectReferenceInstanceIDValue;
        }

        private int GetUniqueProductId()
        {
            if(storeDatabase.Products != null && storeDatabase.Products.Length > 0)
            {
                return storeDatabase.Products.Max(x => x.ID) + 1;
            }
            else
            {
                return 1;
            }
        }

        private void CreateProduct(Type type, string name)
        {
            if (!type.IsSubclassOf(typeof(StoreProduct)))
            {
                Debug.LogError("[Store Database]: Product type should be subclass of Store Product class!");

                return;
            }

            serializedObject.Update();

            productsProperty.arraySize++;

            int productUniqueID = GetUniqueProductId();

            StoreProduct tempProduct = (StoreProduct)ScriptableObject.CreateInstance(type);
            tempProduct.name = name.Replace(" ", "") + "Product" + productUniqueID.ToString("000");
            //tempProduct.hideFlags = HideFlags.HideInHierarchy;

            AssetDatabase.AddObjectToAsset(tempProduct, target);
            AssetDatabase.SaveAssets();

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(tempProduct));
            AssetDatabase.Refresh();

            productsProperty.GetArrayElementAtIndex(productsProperty.arraySize - 1).objectReferenceValue = tempProduct;

            serializedObject.ApplyModifiedProperties();

            SerializedObject tempProductSerializedObject = new SerializedObject(tempProduct);
            tempProductSerializedObject.Update();
            tempProductSerializedObject.FindProperty("productName").stringValue = name;
            tempProductSerializedObject.FindProperty("id").intValue = productUniqueID;
            tempProductSerializedObject.ApplyModifiedProperties();

            selectedObjectName = "";
        }

        private void CreateFilterMenu()
        {
            filterGenericMenu = new GenericMenu();
            filterGenericMenu.AddItem(new GUIContent("All"), false, delegate
            {
                for (int i = 0; i < filterMenuItems.Length; i++)
                {
                    filterMenuItems[i].isActive = true;
                }
            });

            filterGenericMenu.AddItem(new GUIContent("None"), false, delegate
            {
                for (int i = 0; i < filterMenuItems.Length; i++)
                {
                    filterMenuItems[i].isActive = false;
                }
            });

            filterGenericMenu.AddSeparator("");

            for (int i = 0; i < filterMenuItems.Length; i++)
            {
                int index = i;

                filterGenericMenu.AddItem(new GUIContent(filterMenuItems[index].title), filterMenuItems[index].isActive, delegate
                {
                    filterMenuItems[index].isActive = !filterMenuItems[index].isActive;
                });
            }
        }

        private class ProductTypeFilter : SerializedPropertyFilter
        {
            private StoreProductType storeProductType;

            public ProductTypeFilter(StoreProductType storeProductType)
            {
                this.storeProductType = storeProductType;
            }

            public override bool Validate(SerializedObject serializedObject)
            {
                SerializedProperty typeProperty = serializedObject.FindProperty("type");

                return typeProperty.enumValueIndex == (int)storeProductType;
            }
        }

        private class ProductUnknownTypeFilter : SerializedPropertyFilter
        {
            public ProductUnknownTypeFilter()
            {

            }

            public override bool Validate(SerializedObject serializedObject)
            {
                SerializedProperty typeProperty = serializedObject.FindProperty("type");

                return typeProperty.enumValueIndex == -1;
            }
        }
    }
}