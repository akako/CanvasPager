using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

namespace CreatorGene.UI
{
    /// <summary>
    /// Canvasを１枚のページとして扱うためのクラスです
    /// CanvasPagerをアタッチしたCanvasを１つだけ配置したシーンを作成し、別シーンから
    /// CanvasPager.Show("シーン名");とすることで
    /// 対象のCanvasにページ遷移します（複数のページを開くと自動的にスタックします）
    /// Hide()すると、対象のCanvasがシーンごと破棄されます
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasPager : UIBehaviour
    {
        private enum Direction
        {
            Horizontal,
            Vertical
        }

        private CanvasPager _rootPage;
        static readonly List<CanvasPager> pages = new List<CanvasPager>();

        [SerializeField] private bool _isRootPage;
        [SerializeField] private Direction _direction = Direction.Vertical;
        [SerializeField] private float _duration = 0.3f;
        [SerializeField] private Transform _content;
        [SerializeField] private Button _closeButton;

        private Scene _scene;
        private CanvasGroup _canvasGroup;

        public static bool HasStackedPage
        {
            get
            {
                return pages.Any(x => !x._isRootPage);
            }
        }

        public static void Show(string pageSceneName)
        {
            SceneManager.LoadScene(pageSceneName, LoadSceneMode.Additive);
        }

        public static void HideLast()
        {
            if (pages.Count > 0)
            {
                pages.Last().Hide();
            }
        }

        public static void HideAll()
        {
            while (pages.Count > 0)
            {
                pages.Last().Hide();
            }
        }

        public static void DestroyAll()
        {
            foreach (var page in pages)
            {
                page.OnHideComplete();
                GameObject.Destroy(page.gameObject);
            }
        }

        static void Add(CanvasPager page)
        {
            pages.Add(page);
            page.Show();
        }

        protected override void Awake()
        {
            base.Awake();
            _scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            _canvasGroup = GetComponent<CanvasGroup>();
            if (null == _content)
            {
                if (transform.childCount == 0)
                {
                    throw new UnityException("CanvasPager content not found.");
                }
                _content = transform.GetChild(0);
            }
            if (SceneManager.sceneCount == 1)
            {
                _isRootPage = true;
                if (null == FindObjectOfType<EventSystem>())
                {
                    gameObject.AddComponent<EventSystem>();
                }
                if (null == FindObjectOfType<StandaloneInputModule>())
                {
                    gameObject.AddComponent<StandaloneInputModule>();
                }
                if (null == FindObjectOfType<Camera>())
                {
                    gameObject.AddComponent<Camera>();
                }
            }

            if (_closeButton != null) _closeButton.onClick.AddListener(Hide);
        }

        protected override void Start()
        {
            base.Start();
            if (_isRootPage && _rootPage != this)
            {
                DestroyAllPages();
                _rootPage = this;
            }
            else
            {
                pages.Add(this);
                Show();
            }

            var canvas = GetComponent<Canvas>();
            if (canvas.worldCamera == null)
            {
                canvas.worldCamera = Camera.main;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SceneManager.UnloadSceneAsync(_scene);
        }

        protected virtual void OnShowComplete()
        {
        }

        protected virtual void OnHideComplete()
        {
        }

        public void ShowOtherPage(string pageSceneName)
        {
            Show(pageSceneName);
        }

        public void HideAllPages()
        {
            HideAll();
        }

        public void DestroyAllPages()
        {
            DestroyAll();
        }

        void Show()
        {
            if (_isRootPage)
            {
                return;
            }

            var pos = _content.localPosition;
            var defaultValue = _direction == Direction.Horizontal ? pos.x : pos.y;
            Tweener tweener;
            if (_direction == Direction.Horizontal)
            {
                pos.x = Screen.width;
                tweener = _content.DOLocalMoveX(defaultValue, _duration);
            }
            else
            {
                pos.y = -Screen.height;
                tweener = _content.DOLocalMoveY(defaultValue, _duration);
            }
            _content.localPosition = pos;
            tweener.SetUpdate(true);
            _canvasGroup.alpha = 0f;
            DOTween.To(() => _canvasGroup.alpha,
                x => _canvasGroup.alpha = x,
                1f, _duration)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    foreach (var otherPage in pages.Where(x => x != this))
                    {
                        otherPage.gameObject.SetActive(false);
                    }
                    OnShowComplete();
                });
        }

        public void Hide()
        {
            if (_isRootPage)
            {
                return;
            }

            pages.Remove(this);
            var lastPage = pages.LastOrDefault();
            if (null != lastPage)
            {
                lastPage.gameObject.SetActive(true);
                lastPage._canvasGroup.alpha = 1f;
            }
            // 順序が同じ場合はSetActive(true)されたものが前面にくるので、並べ直し
            gameObject.SetActive(false);
            gameObject.SetActive(true);

            var tweener = _direction == Direction.Horizontal ? _content.DOLocalMoveX(Screen.width, _duration) : _content.DOLocalMoveY(-Screen.height, _duration);
            tweener.SetUpdate(true);
            _canvasGroup.alpha = 1f;
            DOTween.To(() => _canvasGroup.alpha,
                x => _canvasGroup.alpha = x,
                0f, _duration)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    OnHideComplete();
                    Destroy(gameObject);
                });
        }
    }
}
