using BepInEx;
using UnityEngine;
using Expedition;
using System;
using Menu;
using Menu.Remix.MixedUI;
using System.Linq;
using BepInEx.Logging;
using JollyCoop.JollyMenu;
using MSCSceneID = MoreSlugcats.MoreSlugcatsEnums.MenuSceneID;
using System.Runtime.CompilerServices;
using System.Runtime;
using HUD;
using RWCustom;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using static MonoMod.InlineRT.MonoModRule;
using JetBrains.Annotations;
using SlugBase;
using IL.JollyCoop.JollyManual;
using HarmonyLib;
using On.MoreSlugcats;
using RainMeadow;
using Unity.Mathematics;
using Logger = UnityEngine.Logger;

// ReSharper disable SimplifyLinqExpressionUseAll

// ReSharper disable UseMethodAny.0

// ReSharper disable once CheckNamespace
namespace TextReplacementTool
{
    [BepInPlugin(MOD_ID, "Text Replacement Tool", "1.0.0")]
    internal class Plugin : BaseUnityPlugin
    {
        public const string MOD_ID = "nassoc.textreplacementtool";
        
        // thank you alphappy for logging help too
        internal static BepInEx.Logging.ManualLogSource logger;
        internal static void Log(LogLevel loglevel, object msg) => logger.Log(loglevel, msg);

        internal static Plugin instance;
        public Plugin()
        {
            logger = Logger;
            instance = this;
        }
        
        private bool weInitializedYet = false;
        public void OnEnable()
        {
            try
            {
                Logger.LogDebug("Text Replacement Tool Plugin loading...");
                //On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);

                if (!weInitializedYet)
                {
                    On.RainWorld.OnModsInit += RainWorldOnModsInitHook;
                    
                    On.InGameTranslator.Translate += TranslateDeez;
                }

                weInitializedYet = true;
                Logger.LogDebug("Text Replacement Tool Plugin successfully loaded!");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void RainWorldOnModsInitHook(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            try
            {
                MachineConnector.SetRegisteredOI(Plugin.MOD_ID, TextReplacementToolConfig.Instance);
            }
            catch (Exception ex)
            {
                Log("[TextReplacementTool] EXCEPTION! "+ex.ToString());
            }
        }

        private string TranslateDeez(On.InGameTranslator.orig_Translate orig, InGameTranslator self, string s)
        {
            string myString = orig(self, s);
            if (stroin(Regex.Replace(myString, "SURVIVOR", "THE VIVOR", RegexOptions.IgnoreCase)) != stroin(myString))
            {
                myString = Regex.Replace(myString,"SURVIVOR","THE VIVOR",RegexOptions.IgnoreCase);
            }
            return myString;
        }

        // MARKER: Utils
        private void Log(object text)
        {
            Logger.LogDebug("[TextReplacementTool] " + text);
        }
        
        public static string stroin(string input)
        {
            return string.Join("", input.ToUpper().ToCharArray());
        }
    }

    public class TextReplacementToolConfig : OptionInterface
    {
        public static TextReplacementToolConfig Instance { get; } = new TextReplacementToolConfig();

        public static void RegisterOI()
        {
            if (MachineConnector.GetRegisteredOI(Plugin.MOD_ID) != Instance)
                MachineConnector.SetRegisteredOI(Plugin.MOD_ID, Instance);
        }
        
        public static Configurable<bool> oneReplacementPerText = Instance.config.Bind("oneReplacementPerText", false,
            new ConfigurableInfo("When enabled, the first replacement that happens prevents any other replacements from applying afterwards. Default false.")
        );
        
        public static Configurable<bool> caseSensitiveOverride = Instance.config.Bind("caseSensitiveOverride", false,
            new ConfigurableInfo("When enabled, the \"Case Sensitive\" option in each replacement is ignored, always making replacements non-case-sensitive. Default false.")
        );

        public static Configurable<string>[] replacementStringToBeReplaced;// = Instance.config.Bind("replacementStringToBeReplaced", "");
        public static Configurable<string>[] replacementStringToReplaceWith;// = Instance.config.Bind("replacementStringToReplaceWith", "");
        public static Configurable<bool>[] replacementBoolCaseSensitive;// = Instance.config.Bind("replacementBoolCaseSensitive", 0);
        public static Configurable<bool>[] replacementBoolStopReplaceAfter;// = Instance.config.Bind("replacementBoolStopReplaceAfter", 0);
        public static Configurable<int> replacementCount = Instance.config.Bind("replacementBoolStopReplaceAfter", 1);
        
        //public OpTab tabWithinMyOpScrollBox;
        public OpScrollBox myOpScrollBox;
        //public UIelement[] myOpRects = new OpRect[10];

        public bool shouldUpdate = true;
        public int whichButtonTypePressed = -1;
        public int whereButtonPressed = -1;

        // Menus and stuff
        public override void Initialize()
        {
            base.Initialize();
            Tabs = [
                new OpTab(this, "Main Page")
            ];

            //tabWithinMyOpScrollBox = new OpTab(this, "Replacement Interface");
            myOpScrollBox = new OpScrollBox(new Vector2(20f, 20f), new Vector2(550f, 400f), 1f);
            
            /*for (int i = 0; i < myOpRects.Length; i++)
            {
                myOpRects[i] = new OpRect(new Vector2(0f,0f),new Vector2(0f,0f));
            }*/
            //Plugin.Log(LogLevel.Info, "bbb");
            //myOpScrollBox.tab.AddItems(myOpRects);
            //Plugin.Log(LogLevel.Info, "ccc");

            Plugin.Log(LogLevel.Info, "aaa");
            Tabs[0].AddItems([
                new OpLabel(30f, 560f, "Text Replacement Tool Config - Main Page", true),
                new OpLabel(30f, 520f, "This mod allows you to use the below Remix interface\nto replace any \"translatable\" text in the game."),
                new OpCheckBox(oneReplacementPerText, new Vector2(30f, 460f)) { description = oneReplacementPerText.info.description },
                new OpLabel(60f, 460f, "Only make one\nreplacement per\ntranslatable text"),
                new OpCheckBox(caseSensitiveOverride, new Vector2(180f, 460f)) { description = caseSensitiveOverride.info.description },
                new OpLabel(210f, 460f, "Override case-\nsensitive to OFF\nfor all replacements"),
                //new OpLabel(30f, 450f, "...there's nothing else to configure here, sorry"),
                new OpRect(new Vector2(20f, 20f), new Vector2(550f, 400f)),
                myOpScrollBox,
            ]);

            for (int i = 0; i < 32; i++)
            {
                //Plugin.Log(LogLevel.Info, "bbb "+i);
                //replacementStringToBeReplaced=replacementStringToBeReplaced.AddToArray(Instance.config.Bind("replacementStringToBeReplaced_0","11"));
                //replacementStringToBeReplaced=replacementStringToBeReplaced.AddToArray(Instance.config.Bind("replacementStringToBeReplaced_1","11"));
                //Plugin.Log(LogLevel.Info, "ccc "+i);
                replacementStringToBeReplaced=replacementStringToBeReplaced.AddToArray(Instance.config.Bind("replacementStringToBeReplaced_"+i.ToString(),"toReplace"));
                //Plugin.Log(LogLevel.Info, "ddd1 "+i);
                replacementStringToReplaceWith=replacementStringToReplaceWith.AddToArray(Instance.config.Bind("replacementStringToReplaceWith_"+i.ToString(),"replacedText"));
                //Plugin.Log(LogLevel.Info, "ddd2 "+i);
                replacementBoolCaseSensitive=replacementBoolCaseSensitive.AddToArray(Instance.config.Bind("replacementBoolCaseSensitive_"+i.ToString(),false));
                //Plugin.Log(LogLevel.Info, "ddd3 "+i);
                replacementBoolStopReplaceAfter=replacementBoolStopReplaceAfter.AddToArray(Instance.config.Bind("replacementBoolStopReplaceAfter_"+i.ToString(),false));
                //Plugin.Log(LogLevel.Info, "ddd4 "+i);
            }

            shouldUpdate = true;
        }

        public override void Update()
        {
            if (whichButtonTypePressed != -1 && whereButtonPressed != -1)
            {
                Plugin.Log(LogLevel.Info, "Replacement "+whereButtonPressed+" button type "+whichButtonTypePressed+" pressed");
                switch (whichButtonTypePressed)
                {
                    case 0:
                    {
                        
                        break;
                    }
                    case 1:
                    {
                        
                        break;
                    }
                    case 2:
                    {
                        if (replacementCount.Value > 1)
                        {
                            Plugin.Log(LogLevel.Info, "Replacement Count: "+replacementCount.Value+"\nStarting position: "+(whereButtonPressed+1));
                            for (int i = whereButtonPressed + 1; i < replacementCount.Value; i++)
                            {
                                Plugin.Log(LogLevel.Info, "Moving Replacement " + i + " down by one!");
                                replacementStringToBeReplaced[i - 1].Value = replacementStringToBeReplaced[i].Value;
                                replacementStringToReplaceWith[i - 1].Value = replacementStringToReplaceWith[i].Value;
                                replacementBoolCaseSensitive[i - 1].Value = replacementBoolCaseSensitive[i].Value;
                                replacementBoolStopReplaceAfter[i - 1].Value = replacementBoolStopReplaceAfter[i].Value;
                            }
                            replacementCount.Value -= 1;
                            Plugin.Log(LogLevel.Info, "Now the "+replacementCount.Value+" still exists.");
                        }
                        break;
                    }
                }
                whichButtonTypePressed = -1;
                whereButtonPressed = -1;
                shouldUpdate = true;
            }
            if (shouldUpdate)
            {
                //Plugin.Log(LogLevel.Info, "fff "+myOpScrollBox.items.Count);
                //Plugin.Log(LogLevel.Info, "ddd "+myOpRects.Length);
                /*
                for (int i = 0; i < myOpRects.Length; i++)
                {
                    if (myOpRects[i] != null)
                    {
                        Plugin.Log(LogLevel.Info, "d1 "+((int)myOpRects[i].PosY==550-i*40));
                        myOpRects[i].pos = new Vector2(40f, 550f-i*40f);
                        myOpRects[i].size = new Vector2(20f, 360f);
                        Plugin.Log(LogLevel.Info, "d2 "+myOpRects[i].PosY);
                        //Plugin.Log(LogLevel.Info, "d3 "+(550f-i*40f));
                    }
                    else
                    {
                        Plugin.Log(LogLevel.Info, "d? "+(550f-i*40f));
                        myOpRects[i] = new OpRect(new Vector2(40f, 550f-i*40f), new Vector2(360f, 20f));
                    }
                }
                Plugin.Log(LogLevel.Info, "eee");
                /*
                for (int i = 0; i < myOpRects.Length; i++)
                {
                    myOpRects[i] = new OpRect(new Vector2(0f,0f),new Vector2(0f,0f));
                }*/

                replacementCount.Value = Math.Min(Math.Max(replacementCount.Value, 1), 32);
                foreach (UIelement element in myOpScrollBox.items)
                {
                    element.Deactivate();
                    myOpScrollBox.tab._RemoveItem(element);
                }
                myOpScrollBox.items.Clear();

                int yOffset = (int)Math.Max(0, (replacementCount.Value - 3.6) * 120);
                for (int i = 0; i < replacementCount.Value; i++)
                {
                    var i1 = i+0;
                    OpSimpleButton butoneUp = new OpSimpleButton(new Vector2(420f, 280f-i*120f+yOffset), new Vector2(30f, 45f)) { description = "Moves this Replacement up in order." };
                    OpSimpleButton butoneDown = new OpSimpleButton(new Vector2(420f, 335f-i*120f+yOffset), new Vector2(30f, 45f)) { description = "Moves this Replacement down in order." };
                    OpSimpleButton butoneDelete = new OpSimpleButton(new Vector2(460f, 280f-i*120f+yOffset), new Vector2(30f, 100f)) { description = "Deletes this Replacement." };
                    butoneUp.OnClick += delegate(UIfocusable _)
                    {
                        whichButtonTypePressed = 0;
                        whereButtonPressed = i1+0;
                    };
                    butoneDown.OnClick += delegate(UIfocusable _)
                    {
                        whichButtonTypePressed = 1;
                        whereButtonPressed = i1+0;
                    };
                    butoneDelete.OnClick += delegate(UIfocusable _)
                    {
                        Plugin.Log(LogLevel.Info, "Button "+(i1+0)+" Pressed!");
                        whichButtonTypePressed = 2;
                        whereButtonPressed = i1+0;
                    };
                    
                    myOpScrollBox.AddItems([
                        new OpRect(new Vector2(40f, 280f-i*120f+yOffset), new Vector2(370f, 100f)),
                        new OpLabel(50f, 280f-i*120f+yOffset, "Entry "+i),
                        new OpLabel(50f, 320f-i*120f+yOffset, 
                        "To Replace: "+replacementStringToBeReplaced[i].Value+ 
                        "\nReplace With: "+replacementStringToReplaceWith[i].Value+
                        "\nCase Sensitive: "+replacementBoolCaseSensitive[i].Value+
                        "\nStop Replace After: "+replacementBoolStopReplaceAfter[i].Value),
                        //new OpRect(new Vector2(420f, 280f-i*120f+yOffset), new Vector2(30f, 45f)),
                        //new OpRect(new Vector2(420f, 335f-i*120f+yOffset), new Vector2(30f, 45f)),
                        //new OpRect(new Vector2(460f, 280f-i*120f+yOffset), new Vector2(30f, 100f)),
                        butoneUp,
                        butoneDown,
                        butoneDelete,
                    ]);
                }
                myOpScrollBox.AddItems([
                    new OpLabel(0f, 0f+yOffset, "hmm"),
                ]);
                if (replacementCount.Value < 32)
                {
                    OpSimpleButton myAwesomeButton = new OpSimpleButton(new Vector2(40f, 340f-replacementCount.Value*120f+yOffset), new Vector2(450f, 40f)) { description = "Adds another Replacement." };
                    myAwesomeButton.OnClick += delegate(UIfocusable _)
                    {
                        shouldUpdate = true;
                        replacementCount.Value += 1;
                        replacementStringToBeReplaced[replacementCount.Value - 1].Value = "toReplace #"+(replacementCount.Value - 1);
                        replacementStringToReplaceWith[replacementCount.Value - 1].Value = "replacedText";
                        replacementBoolCaseSensitive[replacementCount.Value - 1].Value = false;
                        replacementBoolStopReplaceAfter[replacementCount.Value - 1].Value = false;
                    };
                    myOpScrollBox.AddItems([
                        myAwesomeButton,
                        //new OpRect(new Vector2(40f, 340f-replacementCount.Value*120f+yOffset), new Vector2(320f, 40f)),
                        new OpLabel(250f, 350f-replacementCount.Value*120f+yOffset, "+",true),
                    ]);
                }
                myOpScrollBox.SetContentSize((replacementCount.Value < 32 ? 80f : 20f)+120f*(replacementCount.Value));
                
                // ok this should stop the constant updating every frame
                shouldUpdate = false;
            }
            base.Update();
        }
    }
}