using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.WireDiscPlayerExample
{
    public class CapsulePlayer : SaintsMonoBehaviour
    {
        [GetComponent]
        [DrawWireDisc(norY: 1, norZ: 0, posYOffset: -1f, color: nameof(curColor), radisCallback: nameof(curRadius))]
        [DrawLabel(EColor.Brown, "$" + nameof(curStatus))]
        public Transform player;

        [Range(1f, 1.5f)] public float initRadius;
        [Range(1f, 1.5f)] public float alertRadius;

        [AdvancedDropdown] public Color initColor;
        [AdvancedDropdown] public Color alertColor;

        public Transform enemy;

        [InputAxis] public string horizontalAxis;
        [InputAxis] public string verticalAxis;

        [ShowInInspector]
        private Color curColor;

        [ShowInInspector] private float curRadius = 0.5f;
        [ShowInInspector] private string curStatus = "Idle";

        private void Awake()
        {
            curColor = initColor;
            curRadius = initRadius;
        }

        public void Update()
        {
            Vector3 playerPos = player.position;
            Vector3 enemyPos = enemy.position;

            float distance = Vector3.Distance(playerPos, enemyPos);

            float nowRadius = distance < alertRadius ? alertRadius : initRadius;
            Color nowColor = distance < alertRadius ? alertColor : initColor;
            curStatus = distance < alertRadius ? "Alert" : "Idle";

            curRadius = Mathf.Lerp(curRadius, nowRadius, Time.deltaTime * 10);
            curColor = Color.Lerp(curColor, nowColor, Time.deltaTime * 10);

            float horizontal = Input.GetAxis(horizontalAxis);
            float vertical = Input.GetAxis(verticalAxis);

            Vector3 move = new Vector3(horizontal, 0, vertical);
            player.Translate(move * Time.deltaTime * 3);
        }
    }
}
