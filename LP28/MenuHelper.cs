using System;
using System.Collections.Generic;
using Modding;
using Modding.Menu;
using Modding.Menu.Config;
using UnityEngine;
using UnityEngine.UI;

namespace LP28;

public static class MenuHelper
{
    public static readonly Color DISABLED_BUTTON_COLOR = Color.gray;
    public static readonly Color ENABLED_BUTTON_COLOR = Color.white;

    public static MenuScreen rootOptsScreen { get; private set; }

    public static MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
    {
        MenuBuilder builder = MenuUtils.CreateMenuBuilderWithBackButton("LP28", modListMenu, out _);
        builder.CreateTitle("LP28 Options", MenuTitleStyle.vanillaStyle);

        builder.AddContent(RegularGridLayout.CreateVerticalLayout(105f), c =>
        {
            c.AddScrollPaneContent(new ScrollbarConfig
                {
                    CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu),
                    Position = new AnchoredPosition
                    {
                        ParentAnchor = Vector2.one,
                        ChildAnchor = new Vector2(0f, 1f),
                        Offset = new Vector2(-310f, 0f)
                    }
                }, new RelLength(105f * GlitchModule.InitializedModules.Count),
                RegularGridLayout.CreateVerticalLayout(105f),
                ca =>
                {
                    foreach (var (type, module) in GlitchModule.InitializedModules)
                    {
                        List<IMenuMod.MenuEntry> opts = new(module.RegisterMoreOptions());
                        MenuScreen moreOpts = null;

                        if (opts.Count > 0)
                        {
                            MenuBuilder mb = MenuUtils.CreateMenuBuilder(module.GetName() + " Options");
                            mb.AddControls(
                                new SingleContentLayout(new AnchoredPosition(new Vector2(0.5f, 0.5f),
                                    new Vector2(0.5f, 0.5f), new Vector2(0.0f, -64f))), cnt =>
                                    cnt.AddMenuButton("BackButton", new MenuButtonConfig
                                    {
                                        Label = Language.Language.Get("NAV_BACK", "MainMenu"),
                                        CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(rootOptsScreen),
                                        SubmitAction = _ => UIManager.instance.UIGoToDynamicMenu(rootOptsScreen),
                                        Proceed = true,
                                        Style = MenuButtonStyle.VanillaStyle
                                    }, out _));
                            
                            mb.AddContent(RegularGridLayout.CreateVerticalLayout(105f), cnt =>
                            {
                                opts.Insert(0, new IMenuMod.MenuEntry
                                {
                                    Name = module.GetName(),
                                    Values = new []{"Disabled", "Enabled"},
                                    Saver = i => module.SetEnabled(Convert.ToBoolean(i)),
                                    Loader = () => Convert.ToInt32(module.IsEnabled),
                                });
                                
                                MenuUtils.AddModMenuContent(opts, cnt, rootOptsScreen);
                            });

                            moreOpts = mb.Build();
                        }

                        ca.AddMenuButton(module.GetName(), new MenuButtonConfig
                        {
                            Label = module.GetName(),
                            Description = GetModuleDescription(module),
                            Proceed = opts.Count > 0,
                            SubmitAction = b =>
                            {
                                if (opts.Count > 0)
                                {
                                    UIManager.instance.UIGoToDynamicMenu(moreOpts);
                                }
                                else
                                {
                                    module.SetEnabled(!module.IsEnabled);
                                    SetButtonColor(b, module.IsEnabled ? ENABLED_BUTTON_COLOR : DISABLED_BUTTON_COLOR);
                                }
                            },
                            CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu)
                        }, out MenuButton button);
                        if (!module.IsEnabled)
                        {
                            SetButtonColor(button, DISABLED_BUTTON_COLOR);
                        }
                    }
                });
        });

        rootOptsScreen = builder.Build();

        return rootOptsScreen;
    }

    private static DescriptionInfo GetModuleDescription(GlitchModule module) => new()
    {
        Text = module.GetDescription(),
        Style = DescriptionStyle.MenuButtonSingleLineVanillaStyle
    };

    private static DescriptionInfo GetBasicDescriptionInfo(string s) => new()
    {
        Text = s,
        Style = DescriptionStyle.HorizOptionSingleLineVanillaStyle
    };

    private static void SetButtonColor(MenuButton b, Color c)
    {
        b.transform.GetComponentInChildren<Text>().color = c;
    }
}