using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI
{
    public class InGameUnitStatusUI : MonoBehaviour
    {
        [SerializeField] private Image hpBar;

        public void SetHpBar(float currentHp, float maxHp)
        {
            hpBar.fillAmount = currentHp / maxHp;
        }
    }
}
