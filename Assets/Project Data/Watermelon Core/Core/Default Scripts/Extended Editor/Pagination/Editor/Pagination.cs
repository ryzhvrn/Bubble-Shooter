using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Watermelon
{
    /// <summary>
    /// Editor ui pagination extension
    /// </summary>
    public class Pagination
    {
        private int m_ArrayLenth;
        public int arrayLenth
        {
            get { return m_ArrayLenth; }
        }

        private int m_ElementsPerPage;
        private int m_PaginationMaxElements;

        private int m_PagesCount;

        private int m_CurrentPage;

        public PaginationCallback onPageChanged;

        public Pagination(int pageElements, int paginationMaxElements)
        {
            m_ElementsPerPage = pageElements;
            m_PaginationMaxElements = paginationMaxElements;
        }

        public void Init(SerializedProperty array)
        {
            m_ArrayLenth = array.arraySize;

            m_PagesCount = (m_ArrayLenth / m_ElementsPerPage) + (m_ArrayLenth % m_ElementsPerPage > 0 ? 1 : 0);

            InitCurrentPage();
        }

        public void Init(int arraySize)
        {
            m_ArrayLenth = arraySize;

            m_PagesCount = (m_ArrayLenth / m_ElementsPerPage) + (m_ArrayLenth % m_ElementsPerPage > 0 ? 1 : 0);

            InitCurrentPage();
        }

        private void InitCurrentPage()
        {
            if (m_CurrentPage >= m_PagesCount)
                SetPage(0);
        }

        public void SetPage(int id)
        {
            if (m_CurrentPage == id)
                return;

            if (onPageChanged != null)
                onPageChanged.Invoke();

            m_CurrentPage = id;
        }

        public int GetPage(int index)
        {
            return index / m_ElementsPerPage;
        }

        public int GetMinElementNumber()
        {
            return m_CurrentPage * m_ElementsPerPage;
        }

        public int GetMaxElementNumber()
        {
            int maxElementNumber = (m_CurrentPage + 1) * m_ElementsPerPage;

            if (maxElementNumber > m_ArrayLenth)
                maxElementNumber = maxElementNumber - (maxElementNumber - m_ArrayLenth);

            return maxElementNumber;
        }

        public void DrawPagination()
        {
            if (m_ArrayLenth <= m_ElementsPerPage)
                return;

            bool firstPage = m_CurrentPage == 0;
            bool lastPage = m_CurrentPage == m_PagesCount - 1;
            bool cutPositions = m_PaginationMaxElements < m_PagesCount;

            int minPos = 0;
            int maxPos = m_PagesCount;

            if (cutPositions)
            {
                int plusPos = (m_PaginationMaxElements / 2);
                if (m_PaginationMaxElements % 2 == 0)
                    plusPos = 0;

                minPos = m_CurrentPage - plusPos >= 0 ? m_CurrentPage - plusPos : 0;
                maxPos = minPos + m_PaginationMaxElements > m_PagesCount ? m_PagesCount : minPos + m_PaginationMaxElements;
                minPos = maxPos - m_PaginationMaxElements >= 0 ? maxPos - m_PaginationMaxElements : 0;
            }

            EditorGUILayout.BeginHorizontal();
            if (firstPage)
                GUI.enabled = false;

            if (GUILayout.Button("<<", EditorStyles.miniButtonLeft))
            {
                SetPage(0);
            }
            if (GUILayout.Button("<", EditorStyles.miniButtonMid))
            {
                SetPage(m_CurrentPage - 1);
            }
            if (firstPage)
                GUI.enabled = true;

            for (int i = minPos; i < maxPos; i++)
            {
                bool isCurrent = m_CurrentPage == i;
                if (isCurrent)
                    GUI.enabled = false;

                if (GUILayout.Button((i + 1).ToString(), EditorStyles.miniButtonMid))
                {
                    SetPage(i);
                }

                if (isCurrent)
                    GUI.enabled = true;
            }

            if (lastPage)
                GUI.enabled = false;
            if (GUILayout.Button(">", EditorStyles.miniButtonMid))
            {
                SetPage(m_CurrentPage + 1);
            }
            if (GUILayout.Button(">>", EditorStyles.miniButtonRight))
            {
                SetPage(m_PagesCount - 1);
            }

            if (lastPage)
                GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        public delegate void PaginationCallback();
    }
}