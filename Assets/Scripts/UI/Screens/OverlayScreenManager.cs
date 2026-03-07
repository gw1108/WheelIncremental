using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class OverlayScreenManager : SingletonMonoBehaviour<OverlayScreenManager>
{
    [Serializable]
    public enum ScreenType
    {
        Shop,
        Settings,
        MainMenu,
        SteamDemoUpsell,
        GameOver,
    }

    [Serializable]
    public class ScreenTypeToScreenDictionary : UnitySerializedDictionary<ScreenType, BaseScreen> { }

    public ScreenTypeToScreenDictionary Screens;

    public GameObject inputBlocker;

    public Action<ScreenType> OnScrenShown;

    private Stack<BaseScreen> ScreenStack;

    protected override void Awake()
    {
        base.Awake();

        ScreenStack = new Stack<BaseScreen>(4);

        ServiceLocator.Instance.Register(this);
    }

    private void Start()
    {
        foreach (var screen in Screens.Values)
        {
            screen.QuickHide();
        }
    }

    private void Update()
    {
        if (Keyboard.current[Key.Escape].wasPressedThisFrame && ScreenStack.Count >= 1)
        {
            ScreenStack.Peek().EscapeOut();
        }
    }

    private void ExpandSpecificScreen(BaseScreen screen, bool expand, bool setContentActive)
    {
        SceneHierarchyUtility.SetExpanded(screen.gameObject, true);
        screen.ShowContainer(setContentActive);
    }

    private void CollapseSpecificScreen(BaseScreen screen, bool expand)
    {
        SceneHierarchyUtility.SetExpanded(screen.gameObject, false);
        screen.ShowContainer(false);
    }

    [HorizontalGroup("ExpandShop", 0.33f)]
    [Button(ButtonSizes.Small)]
    private void ExpandShop()
    {
        if (Screens.TryGetValue(ScreenType.Shop, out var screen))
        {
            ExpandSpecificScreen(screen, true, true);
        }
    }

    [HorizontalGroup("ExpandShop", 0.33f)]
    [Button(ButtonSizes.Small)]
    private void CollapseShop()
    {
        if (Screens.TryGetValue(ScreenType.Shop, out var screen))
        {
            CollapseSpecificScreen(screen, true);
        }
    }

    [HorizontalGroup("ExpandAll", 0.33f)]
    [Button(ButtonSizes.Medium)]
    private void ExpandAllScreens()
    {
        foreach (BaseScreen screen in Screens.Values)
        {
            if (screen == null) continue;

            SceneHierarchyUtility.SetExpanded(screen.gameObject, true);
        }
    }

    [HorizontalGroup("ExpandAll", 0.33f)]
    [Button(ButtonSizes.Medium)]
    private void ExpandAllScreensRecursive()
    {
        foreach (BaseScreen screen in Screens.Values)
        {
            if (screen == null) continue;

            SceneHierarchyUtility.SetExpandedRecursive(screen.gameObject, true);
        }
    }

    [HorizontalGroup("ExpandAll", 0.33f)]
    [Button(ButtonSizes.Medium)]
    private void CollapseAllScreens()
    {
        foreach (BaseScreen screen in Screens.Values)
        {
            if (screen == null) continue;

            SceneHierarchyUtility.SetExpanded(screen.gameObject, false);
        }
    }

    public void RequestShowScreen(int screenTypeAsInt)
    {
        RequestShowScreen((ScreenType)screenTypeAsInt);
    }

    // TODO: Make a stack/queue system. Right now its only 1 allowed at a time. Im lazy rn
    public BaseScreen RequestShowScreen(ScreenType screenType)
    {
        if (!Screens.TryGetValue(screenType, out BaseScreen Screen))
        {
            return null;
        }

        // TODO: We currently will only support 1 instance of the screen at a time
        if (ScreenStack.Contains(Screen))
        {
            return null;
        }

        // TODO: Check if current screen allows for things to appear on top

        ScreenStack.Push(Screen);
        Screen.Show();
        Screen.transform.SetAsLastSibling();
        OnScrenShown?.Invoke(screenType);
        return Screen;
    }

    public void RequestToggleScreen(ScreenType screenType)
    {
        if (!Screens.TryGetValue(screenType, out BaseScreen Screen))
        {
            return;
        }

        if (ScreenStack.TryPeek(out BaseScreen topScreen) && topScreen == Screen)
        {
            HideActiveScreen();
        }
        else
        {
            RequestShowScreen(screenType);
        }
    }

    public void HideActiveScreen()
    {
        if (ScreenStack.TryPop(out BaseScreen Screen))
        {
            Screen.Hide();
        }
    }

    public void HideAllScreens()
    {
        while (ScreenStack.TryPop(out BaseScreen Screen))
        {
            Screen.Hide();
        }
    }

    public void HideAllScreensThenShowScreen(int screenTypeAsInt)
    {
        HideAllScreens();
        RequestShowScreen((ScreenType)screenTypeAsInt);
    }

    /*public void RequestConfirmationScreen(Action callback, string title, string message)
    {
        RequestShowScreen(ScreenType.Confirmation);
        ConfirmationScreen screen = Screens[ScreenType.Confirmation] as ConfirmationScreen;
        screen.SetData(callback, title, message);
    }*/
}
