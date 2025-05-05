using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Watermelon
{
    public class CustomList
    {
        private SerializedObject levelsDatabaseSerializedObject;
        private SerializedProperty levelsSerializedProperty;

        #region Reordable list(original) default values
        private GUIContent iconToolbarPlus = EditorGUIUtility.TrIconContent("Toolbar Plus");
        private GUIContent iconToolbarMinus = EditorGUIUtility.TrIconContent("Toolbar Minus");
        private GUIStyle draggingHandle;
        private GUIStyle headerBackground;
        private GUIStyle emptyHeaderBackground;
        private GUIStyle footerBackground;
        private GUIStyle boxBackground;
        private GUIStyle preButton;
        private GUIStyle elementBackground;
        private const string DRAGGING_HANDLE_STYLE_NAME = "RL DragHandle";
        private const string HEADER_STYLE_NAME = "RL Header";
        private const string EMPTY_HEADER_STYLE_NAME = "RL Empty Header";
        private const string FOOTER_STYLE_NAME = "RL Footer";
        private const string BACKGROUND_STYLE_NAME = "RL Background";
        private const string FOOTER_BUTTON_STYLE_NAME = "RL FooterButton";
        private const string ELEMENT_STYLE_NAME = "RL Element";
        private const int padding = 6;
        private const int dragHandleWidth = 20;
        internal const int propertyDrawerPadding = 8;
        private const float listElementTopPadding = 0;
        private const float listElementBottomPadding = 4;
        private const string EMPTY_LIST_LABEL = "List is empty";
        #endregion

        private const int BUTTON_WIDTH = 25;
        private const int BUTTON_HEIGHT = 16;
        private const string FIRST_PAGE_LABEL = "<<";
        private const string PREVIOUS_PAGE_LABEL = "<";
        private const string SEPARATOR = " / ";
        private const string NEXT_PAGE_LABEL = ">";
        private const string LAST_PAGE_LABEL = ">>";
        private const float LIST_MIN_HEIGHT = 200;
        private const int LIST_MIN_WIDTH = 150;
        private float HEADER_HEIGHT = 20;
        private float FOOTER_PAGINATION_HEIGHT = 20;
        private float FOOTER_BUTTONS_HEIGHT = 20;
        private float LIST_ELEMENT_HEIGHT = 20;

        //global
        private bool stylesInited;
        private int selectedIndex = -1;
        private int prevIndent;
        private float tempX;
        private float tempY;
        private Event currentEvent;
        private GUIStyle controlStyle;
        private Rect globalRect;
        private Rect footerPaginationRect;
        private Rect footerButtonsRect;
        private Rect listRect;
        private Rect filledElementsRect;


        //header
        private Rect headerRect;
        private Rect headerContentRect;

        //footer
        private float rightEdge;
        private float leftEdge;
        private Rect buttonsRect;
        private Rect addRect;
        private Rect removeRect;

        //list
        private bool dragging;
        private bool isSelected;
        private int fullIndex;
        private int startDragIndex;
        private int currentDragIndex;
        private int dragIndexAdjustment;
        private int elementCount;
        private int maxElementCount;
        private float dragOffset;
        private float tempFloat;
        private float listHeight;
        private Rect listContentRect;
        private Rect elementRect;
        private Rect elementContentRect;
        private Rect draggingHandleRect;

        //pagination
        private bool enablePagination;
        private int currentPage;
        private int pagesCount;
        private int pageBeginIndex = 0;
        private Rect paginationContentRect;
        private Rect firstPageButtonRect;
        private Rect previousPageButtonRect;
        private Rect lastPageButtonRect;
        private Rect paginationLabelRect;
        private Rect nextPageButtonRect;
        private GUIStyle centeredLabelStyle;


        #region delegates
        public delegate string GetHeaderLabelCallbackDelegate();
        public delegate string GetElementLabelCallbackDelegate(int index);
        public delegate void SelectionChangedCallbackDelegate();
        public delegate void ListChangedCallbackDelegate();
        public delegate void ListReorderedCallbackDelegate();
        public delegate void AddElementCallbackDelegate();
        public delegate void RemoveElementCallbackDelegate();
        public delegate void DisplayContextMenuCallbackDelegate();

        public GetHeaderLabelCallbackDelegate getHeaderLabelCallback;
        public GetElementLabelCallbackDelegate getElementLabelCallback;
        public SelectionChangedCallbackDelegate selectionChangedCallback;
        public ListReorderedCallbackDelegate listReorderedCallback;
        public ListChangedCallbackDelegate listChangedCallback;
        public AddElementCallbackDelegate addElementCallback;
        public RemoveElementCallbackDelegate removeElementCallback;
        public DisplayContextMenuCallbackDelegate displayContextMenuCallback;
        #endregion

        public int SelectedIndex { get => selectedIndex; set => selectedIndex = value; }
        

        public CustomList(SerializedObject levelsDatabaseSerializedObject, SerializedProperty levelsSerializedProperty)
        {
            this.levelsDatabaseSerializedObject = levelsDatabaseSerializedObject;
            this.levelsSerializedProperty = levelsSerializedProperty;
            this.stylesInited = false;
        }

        public void Display()
        {
            if (!stylesInited)
            {
                InitStyles();
            }

            globalRect = GUILayoutUtility.GetRect(GUIContent.none, controlStyle, GUILayout.MinHeight(LIST_MIN_HEIGHT), GUILayout.MinWidth(LIST_MIN_WIDTH));
            currentEvent = Event.current;
            prevIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            DoCalculations();
            DrawHeader();
            DrawList();

            if (enablePagination)
            {
                DrawPagination();
                DrawFooterButtons();
            }
            else
            {
                DrawFooterButtons();
            }

            EditorGUI.indentLevel = prevIndent;
        }

        

        private void InitStyles()
        {
            controlStyle = new GUIStyle();
            controlStyle.stretchHeight = true;
            controlStyle.stretchWidth = true;
            stylesInited = true;

            centeredLabelStyle = new GUIStyle(GUI.skin.label);
            centeredLabelStyle.alignment = TextAnchor.MiddleCenter;

            // init default styles
            draggingHandle = DRAGGING_HANDLE_STYLE_NAME;
            headerBackground = HEADER_STYLE_NAME;
            emptyHeaderBackground = EMPTY_HEADER_STYLE_NAME;
            footerBackground = FOOTER_STYLE_NAME;
            boxBackground = BACKGROUND_STYLE_NAME;
            preButton = FOOTER_BUTTON_STYLE_NAME;
            elementBackground = ELEMENT_STYLE_NAME;
        }

        private void DoCalculations()
        {
            maxElementCount = Mathf.FloorToInt((globalRect.height - HEADER_HEIGHT - FOOTER_BUTTONS_HEIGHT) / LIST_ELEMENT_HEIGHT);

            if(maxElementCount >= levelsSerializedProperty.arraySize)
            {
                enablePagination = false;
                elementCount = levelsSerializedProperty.arraySize;
                currentPage = 0;
                pageBeginIndex = 0;
            }
            else
            {
                enablePagination = true;
                maxElementCount = Mathf.FloorToInt((globalRect.height - HEADER_HEIGHT - FOOTER_PAGINATION_HEIGHT - FOOTER_BUTTONS_HEIGHT) / LIST_ELEMENT_HEIGHT);
                pagesCount = Mathf.CeilToInt((levelsSerializedProperty.arraySize + 0f) / maxElementCount);

                if(pagesCount > 1) // fix to layout event bug
                {
                    currentPage = Mathf.Clamp(currentPage, 0, pagesCount - 1);

                    if(selectedIndex != -1) // keep selected element in view even if number of pages changed because of resize
                    {
                        currentPage = Mathf.FloorToInt((selectedIndex + 0f) / maxElementCount);
                    }
                }

                pageBeginIndex = currentPage * maxElementCount;

                if(pageBeginIndex + maxElementCount > levelsSerializedProperty.arraySize)
                {
                    elementCount = levelsSerializedProperty.arraySize - pageBeginIndex;
                }
                else
                {
                    elementCount = maxElementCount;
                }
            }

            listHeight = maxElementCount * LIST_ELEMENT_HEIGHT;

            //rects
            headerRect = new Rect(globalRect.position.x, globalRect.position.y, globalRect.width, HEADER_HEIGHT);
            listRect = new Rect(globalRect.position.x, globalRect.position.y + HEADER_HEIGHT, globalRect.width, listHeight);
            filledElementsRect = new Rect(globalRect.position.x, globalRect.position.y + HEADER_HEIGHT, globalRect.width, (elementCount ) * LIST_ELEMENT_HEIGHT);

            if (enablePagination)
            {
                footerPaginationRect = new Rect(globalRect.position.x, listRect.position.y + listHeight, globalRect.width, FOOTER_BUTTONS_HEIGHT);
                footerButtonsRect = new Rect(globalRect.position.x, footerPaginationRect.position.y + FOOTER_PAGINATION_HEIGHT, globalRect.width, FOOTER_BUTTONS_HEIGHT);
            }
            else
            {
                footerButtonsRect = new Rect(globalRect.position.x, listRect.position.y + listHeight, globalRect.width, FOOTER_BUTTONS_HEIGHT);
            }

            CopyRectsFromReordableList();
        }

        private void CopyRectsFromReordableList()
        {
            headerContentRect = new Rect(headerRect);
            headerContentRect.xMin += padding;
            headerContentRect.xMax -= padding;
            headerContentRect.height -= 2;
            headerContentRect.y += 1;

            listContentRect = new Rect(listRect);
            listContentRect.yMin += listElementTopPadding;
            listContentRect.yMax -= listElementBottomPadding;
            listContentRect.xMin += 1;
            listContentRect.xMax -= 1;
        }

        private void DrawHeader()
        {
            if (currentEvent.type == EventType.Repaint)
            {
                headerBackground.Draw(headerRect, false, false, false, false);
            }

            EditorGUI.LabelField(headerContentRect, GetHeaderLabel());
        }

        private void DrawList()
        {
            HandleDraggingDetection();

            if (currentEvent.type == EventType.Repaint)
            {
                boxBackground.Draw(listRect, false, false, false, false);
            }

            tempX = listRect.position.x;
            tempY = listRect.position.y;

            if(levelsSerializedProperty.arraySize == 0)
            {
                elementRect = new Rect(tempX, tempY, listContentRect.width, LIST_ELEMENT_HEIGHT);
                elementContentRect = new Rect(elementRect);
                elementContentRect.xMin += padding;
                elementContentRect.xMax -= padding;
                GUI.Label(elementContentRect, EMPTY_LIST_LABEL);
                return;

            }

            dragIndexAdjustment = 0;

            for (int i = 0; i < elementCount; i++)
            {
                if (dragging && (i == startDragIndex) && (startDragIndex < currentDragIndex))
                {
                    dragIndexAdjustment = 1;
                }

                if (dragging)
                {
                    if (i == currentDragIndex)
                    {
                        if (startDragIndex < currentDragIndex)
                        {
                            dragIndexAdjustment = 0;
                        }
                        else if (currentDragIndex < startDragIndex)
                        {
                            dragIndexAdjustment = -1;
                        }

                        elementRect = new Rect(tempX, Mathf.Clamp(currentEvent.mousePosition.y - dragOffset,filledElementsRect.y,filledElementsRect.yMax - LIST_ELEMENT_HEIGHT), listContentRect.width, LIST_ELEMENT_HEIGHT);
                        fullIndex = startDragIndex + pageBeginIndex;
                    }
                    else
                    {
                        elementRect = new Rect(tempX, tempY, listContentRect.width, LIST_ELEMENT_HEIGHT);
                        fullIndex = i + pageBeginIndex + dragIndexAdjustment;
                    }
                }
                else
                {
                    elementRect = new Rect(tempX, tempY, listContentRect.width, LIST_ELEMENT_HEIGHT);
                    fullIndex = i + pageBeginIndex;
                }

                elementContentRect = new Rect(elementRect);
                isSelected = (fullIndex == selectedIndex);

                if (currentEvent.type == EventType.Repaint)
                {
                    if (dragging && (i == currentDragIndex))
                    {
                        elementBackground.Draw(elementRect, false, true, true, true);
                    }
                    else
                    {
                        elementBackground.Draw(elementRect, false, isSelected, isSelected, isSelected);
                    }

                    draggingHandleRect = new Rect(elementRect.x + 5, elementRect.y + 8, 10, 6);
                    draggingHandle.Draw(draggingHandleRect, false, false, false, false);

                }

                elementContentRect.xMin += dragHandleWidth;
                elementContentRect.xMax -= padding;
                GUI.Label(elementContentRect, GetElementLabel(fullIndex));
                tempY += LIST_ELEMENT_HEIGHT;

                if (GUI.Button(elementRect, GUIContent.none, GUIStyle.none))
                {
                    if (!isSelected)
                    {
                        selectedIndex = fullIndex;
                        OnSelectionChanged();
                    }

                    if(currentEvent.button == 1)
                    {
                        DisplayContextMenu();
                    }
                }

                if (dragging && (i == startDragIndex) && (currentDragIndex < startDragIndex))
                {
                    dragIndexAdjustment = 0;
                }
            }
        }

        private void HandleDraggingDetection()
        {
            if (!dragging)
            {
                if ((currentEvent.type == EventType.MouseDrag) && filledElementsRect.Contains(currentEvent.mousePosition))
                {
                    DraggingStarted();
                }
            }
            else
            {
                UpdateDrag();

                if (currentEvent.type == EventType.MouseUp)
                {
                    DraggingFinished();
                }
            }
        }

        private void DraggingStarted()
        {
            dragging = true;
            tempFloat = currentEvent.mousePosition.y - listRect.position.y;
            startDragIndex = Mathf.FloorToInt(tempFloat / LIST_ELEMENT_HEIGHT);
            currentDragIndex = startDragIndex;
            dragOffset = tempFloat - LIST_ELEMENT_HEIGHT * startDragIndex;

            if(selectedIndex != startDragIndex + pageBeginIndex)
            {
                selectedIndex = startDragIndex + pageBeginIndex;
                OnSelectionChanged();
            }
        }

        private void UpdateDrag()
        {
            tempFloat = currentEvent.mousePosition.y - listRect.position.y;
            currentDragIndex = Mathf.Clamp(Mathf.FloorToInt(tempFloat / LIST_ELEMENT_HEIGHT), 0, Mathf.Min(maxElementCount - 1,levelsSerializedProperty.arraySize - pageBeginIndex - 1));
        }

        private void DraggingFinished()
        {
            dragging = false;
            selectedIndex = currentDragIndex + pageBeginIndex;
            levelsSerializedProperty.MoveArrayElement(startDragIndex + pageBeginIndex, currentDragIndex + pageBeginIndex);
            levelsDatabaseSerializedObject.ApplyModifiedProperties();
            ListReorderedCallback();
            ListChangedCallback();
        }

        private void DrawPagination()
        {
            if (currentEvent.type == EventType.Repaint)
            {
                emptyHeaderBackground.Draw(footerPaginationRect, false, false, false, false);
            }

            paginationContentRect = new Rect(footerPaginationRect);
            paginationContentRect.xMin += padding;
            paginationContentRect.xMax -= padding;
            paginationContentRect.yMin += 2;


            firstPageButtonRect = new Rect(paginationContentRect.xMin, paginationContentRect.y, BUTTON_WIDTH, BUTTON_HEIGHT);
            previousPageButtonRect = new Rect(firstPageButtonRect.xMax, paginationContentRect.y, BUTTON_WIDTH, BUTTON_HEIGHT);

            nextPageButtonRect = new Rect(paginationContentRect.xMax - (2 * BUTTON_WIDTH), paginationContentRect.y, BUTTON_WIDTH, BUTTON_HEIGHT);
            lastPageButtonRect = new Rect(paginationContentRect.xMax - BUTTON_WIDTH, paginationContentRect.y, BUTTON_WIDTH, BUTTON_HEIGHT);
            paginationLabelRect = new Rect(previousPageButtonRect.xMax, paginationContentRect.y, nextPageButtonRect.xMin - previousPageButtonRect.xMax, FOOTER_BUTTONS_HEIGHT);


            using (new EditorGUI.DisabledScope(currentPage <= 1))
            {
                if (GUI.Button(firstPageButtonRect, FIRST_PAGE_LABEL, preButton))
                {
                    selectedIndex = -1;
                    currentPage = 0;
                }
            }

            using (new EditorGUI.DisabledScope(currentPage == 0))
            {
                if (GUI.Button(previousPageButtonRect, PREVIOUS_PAGE_LABEL, preButton))
                {
                    selectedIndex = -1;
                    currentPage--;
                }
            }

            GUI.Label(paginationLabelRect, (currentPage + 1) + SEPARATOR + pagesCount, centeredLabelStyle);

            using (new EditorGUI.DisabledScope(currentPage == pagesCount - 1))
            {
                if (GUI.Button(nextPageButtonRect, NEXT_PAGE_LABEL, preButton))
                {
                    selectedIndex = -1;
                    currentPage++;
                }
            }

            using (new EditorGUI.DisabledScope(currentPage >= pagesCount - 2))
            {
                if (GUI.Button(lastPageButtonRect, LAST_PAGE_LABEL, preButton))
                {
                    selectedIndex = -1;
                    currentPage = pagesCount - 1;
                }
            }
        }

        private void DrawFooterButtons()
        {
            rightEdge = footerButtonsRect.xMax - 10f;
            leftEdge = rightEdge - listElementBottomPadding - listElementBottomPadding;
            leftEdge -= BUTTON_WIDTH; //space for add button
            leftEdge -= BUTTON_WIDTH; // space for remove button

            buttonsRect = new Rect(leftEdge, footerButtonsRect.y, rightEdge - leftEdge, footerButtonsRect.height);
            addRect = new Rect(leftEdge + listElementBottomPadding, buttonsRect.y, BUTTON_WIDTH, BUTTON_HEIGHT);
            removeRect = new Rect(rightEdge - BUTTON_WIDTH - listElementBottomPadding, buttonsRect.y, BUTTON_WIDTH, BUTTON_HEIGHT);

            if (currentEvent.type == EventType.Repaint)
            {
                footerBackground.Draw(buttonsRect, false, false, false, false);
            }

            //add button
            if (GUI.Button(addRect, iconToolbarPlus, preButton))
            {
                AddElement();
                ListChangedCallback();
            }

            //remove button
            using (new EditorGUI.DisabledScope((selectedIndex < 0) || (selectedIndex >= levelsSerializedProperty.arraySize)))
            {
                if (GUI.Button(removeRect, iconToolbarMinus, preButton))
                {
                    RemoveElement();
                    ListChangedCallback();
                }
            }
        }

        #region handle callbacks
        private string GetHeaderLabel()
        {
            return getHeaderLabelCallback?.Invoke();
        }

        private string GetElementLabel(int fullIndex)
        {
            return getElementLabelCallback?.Invoke(fullIndex);
        }

        private void OnSelectionChanged()
        {
            selectionChangedCallback?.Invoke();
        }

        private void ListChangedCallback()
        {
            listChangedCallback?.Invoke();
        }

        private void ListReorderedCallback()
        {
            listReorderedCallback?.Invoke();
        }

        private void AddElement()
        {
            addElementCallback?.Invoke();
        }

        private void RemoveElement()
        {
            removeElementCallback?.Invoke();
        }

        private void DisplayContextMenu()
        {
            displayContextMenuCallback?.Invoke();
        }

        #endregion
    }
}
