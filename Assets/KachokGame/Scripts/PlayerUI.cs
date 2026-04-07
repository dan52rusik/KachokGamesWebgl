using TMPro;
using UnityEngine;

namespace Tutorial
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField] private GameObject[] hearts;
        [SerializeField] private TextMeshProUGUI treeCountText;


        public void SetHealth(int health)
        {
            if(health > hearts.Length) return;

            for (int i = 0; i < hearts.Length; i++)
            {
                hearts[i].SetActive(health > i);
            }
        }

        public int TreeCount
        {
            get => _treeCount;
            set
            {
                _treeCount = value;
                
                treeCountText.SetText(_treeCount.ToString());
            }
        }
        private int _treeCount;
    }
}