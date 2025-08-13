using System;
using System.Collections.Generic;
using System.Linq;
using IceCold.Interface;
using IceCold.MonoBehaviourHost.Interface;
using IceCold.UI.Interface;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace IceCold.UI
{
    [ServicePriority(2)]
    public class UIService : IUIService
    {
        public static event Action OnInitialized;
        public bool IsInitialized { get; private set; }
        
        private readonly Dictionary<Type, List<IWindow>> windows = new();
        private readonly List<IInternalWindow> windowsHistory = new();
        private readonly Dictionary<Scene, List<IWindow>> windowsPerScene = new();
        
        private UIConfig config;
        private GameObject uiRoot;
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            config = ConfigLoader.Get<UIConfig>(nameof(UIConfig));
            if (config == null) return;
            
            CreateWindowStructure();

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    ScanAndRegisterWindowsInScene(scene);
                }
            }

            UI.Init(this);
            
            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        public void Deinitialize()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            
            foreach (var windowList in windows.Values)
            {
                foreach (var window in windowList.ToList())
                {
                    (window as IInternalWindow)?.DestroySelf();
                }
            }
            
            windows.Clear();
            windowsPerScene.Clear();
            windowsHistory.Clear();
            
            config = null;
            
            if (uiRoot != null)
            {
                UnityEngine.Object.Destroy(uiRoot.gameObject);
            }

            IsInitialized = false;
        }
        
        public void OnWillQuit() { }
        
        private void RegisterWindow(IWindow window, Scene scene)
        {
            if (window == null)
            {
#if ICECOLD_DEBUG
                CoreLogger.LogWarning("Attempted to register a null window");
#else
                Debug.LogWarning("Attempted to register a null window");
#endif
                return;
            }
            
            try
            {
                var type = window.GetType();
                
                var internalWnd = window as IInternalWindow;
                
                if (windows.TryGetValue(type, out var existingWindows) && 
                    !string.IsNullOrEmpty(window.Id) && 
                    existingWindows.Any(w => w.Id == window.Id))
                {
#if ICECOLD_DEBUG
                    CoreLogger.LogWarning($"[UIService] A window of type '{type.Name}' with ID '{window.Id}' " +
                                          $"is already registered. Skipping duplicate from scene '{scene.name}'.");
#else
                    Debug.LogWarning($"[UIService] A window of type '{type.Name}' with ID '{window.Id}' " +
                                          $"is already registered. Skipping duplicate from scene '{scene.name}'.");
#endif
                    
                    internalWnd?.DestroySelf();
                    return;
                }
                
                if (!windows.ContainsKey(type))
                {
                    windows[type] = new List<IWindow>();
                }

                windows[type].Add(window);
                
                if (!windowsPerScene.ContainsKey(scene))
                {
                    windowsPerScene[scene] = new List<IWindow>();
                }
                windowsPerScene[scene].Add(window);

                if (internalWnd != null && !window.IsInitialized)
                {
                    internalWnd.ReadyWindow();
                }
            }
            catch (Exception e)
            {
#if ICECOLD_DEBUG
                CoreLogger.LogError($"Error registering window of type {window.GetType().Name}: {e.Message}\n{e.StackTrace}");
#else
                Debug.LogError($"Error registering window of type {window.GetType().Name}: {e.Message}\n{e.StackTrace}");
#endif
                if (windows.ContainsKey(window.GetType()))
                {
                    windows[window.GetType()].Remove(window);
                }
            }
        }
        
        private void UnregisterWindow(IWindow window)
        {
            var type = window.GetType();
            if (!windows.TryGetValue(type, out var list)) return;

            list.Remove(window);
            if (list.Count == 0)
                windows.Remove(type);

            if (window is IInternalWindow internalWindow)
                windowsHistory.Remove(internalWindow);
        }
        
        public T GetWindow<T>(string id = null) where T : class, IWindow
        {
            if (!windows.TryGetValue(typeof(T), out var windowList)) return null;

            if (string.IsNullOrEmpty(id)) return windowList.FirstOrDefault() as T;

            return windowList.OfType<T>().FirstOrDefault(w => w.Id == id);
        }

        public IWindow GetWindow(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            
            return windows.Values
                .SelectMany(list => list)
                .FirstOrDefault(w => w.Id == id);
        }
        
        public void ShowWindow(string id, bool instant = false) => ShowWindow(GetWindow(id), instant);
        
        public void ShowWindow(IWindow wnd, bool instant = false)
        {
            if (wnd is not IInternalWindow internalWindow) return;

            windowsHistory.Remove(internalWindow);

            if (windowsHistory.Count > 0)
                windowsHistory[^1].HideInstantInternal();

            windowsHistory.Add(internalWindow);

            if (instant) internalWindow.ShowInstantInternal();
            else internalWindow.ShowInternal();
        }
        
        public void HideWindow(string id, bool instant = false) => HideWindow(GetWindow(id), instant);
        
        public void HideWindow(IWindow wnd, bool instant = false)
        {
            if (wnd is not IInternalWindow internalWindow) return;

            windowsHistory.Remove(internalWindow);

            if (instant) internalWindow.HideInstantInternal();
            else internalWindow.HideInternal();

            if (windowsHistory.Count > 0)
                windowsHistory[^1].ShowInstantInternal();
        }

        public void RequestBack(bool instant = false)
        {
            if (windowsHistory.Count == 0) return;

            var last = windowsHistory[^1];
            windowsHistory.RemoveAt(windowsHistory.Count - 1);

            if (instant) last.HideInstantInternal();
            else last.HideInternal();

            if (windowsHistory.Count > 0)
                windowsHistory[^1].ShowInstantInternal();
        }
        
        private void CreateWindowStructure()
        {
            var host = Core.GetService<IMonoBehaviourHostService>();
            if (host == null)
            {
#if ICECOLD_DEBUG
                CoreLogger.LogError("[UIService] MonoBehaviourHostService is not available. UI System cannot initialize.");
#else
                Debug.LogError("[UIService] MonoBehaviourHostService is not available. UI System cannot initialize.");
#endif
                return;
            }
            
            var obj = host.AddEmptyGameObjectToRoot("[UI]");
            if (obj == null)
            {
#if ICECOLD_DEBUG
                CoreLogger.LogError("[UIService] Failed to create UI Root via host service. UI System cannot initialize.");
#else
                Debug.LogError("[UIService] Failed to create UI Root via host service. UI System cannot initialize.");
#endif
                return;
            }

            uiRoot = obj;

            if(!obj.TryGetComponent<Canvas>(out var canvas))
            {
                canvas = obj.AddComponent<Canvas>();
            }
            canvas.renderMode = config.canvasRenderMode;
            canvas.sortingOrder = config.canvasSortingOrder;
                
            if(obj.GetComponent<GraphicRaycaster>() == null) obj.AddComponent<GraphicRaycaster>();
            if(obj.GetComponent<CanvasScaler>() == null) obj.AddComponent<CanvasScaler>();
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            if (sceneMode == LoadSceneMode.Single)
            {
                windowsHistory.Clear();
            }
            
            ScanAndRegisterWindowsInScene(scene);
        }
        
        private void OnSceneUnloaded(Scene scene)
        {
            if (!windowsPerScene.TryGetValue(scene, out var windowsToUnload)) return;
            
#if ICECOLD_DEBUG
            CoreLogger.Log($"[UIService] Unloading {windowsToUnload.Count} windows from scene '{scene.name}'.");
#else
            Debug.Log($"[UIService] Unloading {windowsToUnload.Count} windows from scene '{scene.name}'.");
#endif

            foreach (var window in windowsToUnload.ToList())
            {
                UnregisterWindow(window);
                var intWnd = window as IInternalWindow;
                intWnd?.DestroySelf();
            }
            
            windowsPerScene.Remove(scene);
        }
        
        private void ScanAndRegisterWindowsInScene(Scene scene)
        {
            var rootGameObjects = scene.GetRootGameObjects();
            var windowsInScene = new List<IWindow>();

            foreach (var root in rootGameObjects)
            {
                windowsInScene.AddRange(root.GetComponentsInChildren<IWindow>(true));
            }

#if ICECOLD_DEBUG
            CoreLogger.Log($"[UIService] Found {windowsInScene.Count} windows in scene '{scene.name}'.");
#else
            Debug.Log($"[UIService] Found {windowsInScene.Count} windows in scene '{scene.name}'.");
#endif

            foreach (var window in windowsInScene)
            {
                if (uiRoot && uiRoot.transform)
                {
                    var intWnd = window as IInternalWindow;
                    intWnd?.Reparent(uiRoot.transform, false);
                }
                
                RegisterWindow(window, scene);
            }
        }
    }
}