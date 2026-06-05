
namespace _3rdBy.ByFramework.UI.Helper.List
{
    using Cysharp.Threading.Tasks;
    using Extension.ExtendComponent;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    public class RefreshButtonHelper : MonoBehaviour
    {
        public Button btnRefresh;
        public Button btnTop;
        public TextMeshProUGUI txtRefreshTime;

        [Header("刷新间隔")]
        public float refreshTime = 5f;

        [Header("列表")]
        public ListHelper listHelper;
    
        public UnityAction OnRefresh { get; set; }
        public UnityAction OnTop { get; set; }

        private bool _isRefreshClicked;
        private float _timer;

        private void Awake()
        {
            btnRefresh.OnClick(OnRefreshHandler);
            btnTop.OnClick(OnTopHandler);
            listHelper.OnScrollOverScreenHandler = OnScrollOverScreenHandler;
        
            _isRefreshClicked = false;
            _timer = 0;
            txtRefreshTime.text = "";
        }
    
        private void Update()
        {
            if (_isRefreshClicked)
            {
                //刷新按钮点击后，刷新按钮不可用，显示倒计时
                btnRefresh.interactable = false;
                txtRefreshTime.text = ((int)(refreshTime - _timer)).ToString();
                _timer += Time.deltaTime;
                if (_timer >= refreshTime)
                {
                    _timer = 0;
                    _isRefreshClicked = false;
                    btnRefresh.interactable = true;
                    txtRefreshTime.text = "";
                }
            }
        }
    
        private void OnScrollOverScreenHandler(bool arg0)
        {
            btnTop.gameObject.SetActive(arg0);
        }

        private void OnRefreshHandler()
        {
            _isRefreshClicked = true;
            OnRefresh?.Invoke();
        }

        private void OnTopHandler()
        {
            listHelper.NavigateTo(0).Forget();
            OnTop?.Invoke();
        }
    }
}