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
using Menu.Remix.MixedUI.ValueTypes;
using On.MoreSlugcats;
using RainMeadow;
using Unity.Mathematics;
using Logger = UnityEngine.Logger;
using Random = UnityEngine.Random;

// ReSharper disable SimplifyLinqExpressionUseAll

// ReSharper disable UseMethodAny.0

// ReSharper disable once CheckNamespace
namespace TextReplacementTool
{
    public static class GeneralCWT
    {
        static ConditionalWeakTable<SimpleButton, Data> table = new ConditionalWeakTable<SimpleButton, Data>();
        public static Data GetCustomData(this SimpleButton self) => table.GetOrCreateValue(self);

        public class Data
        {
            // stored simplebutton stuff
            public string origMessage = "";
        }
    }
    
    [BepInPlugin(MOD_ID, "Text Replacement Tool", "1.0.2")]
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
        public static bool configInitializedYet = false;
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
                    On.Menu.SimpleButton.GrafUpdate += TranslateDaButton;
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

        public string StringInator(string myString)
        {
            
            if (configInitializedYet && TextReplacementToolConfig.Instance != null && TextReplacementToolConfig.replacementCount != null)
            {
                //Logger.Log(LogLevel.Info, "dude");
                for (int i = 0; i < Math.Min(TextReplacementToolConfig.replacementCount.Value,TextReplacementToolConfig.replacementStringToBeReplaced.Length);i++)
                {
                    //Logger.Log(LogLevel.Info, "jude "+TextReplacementToolConfig.replacementCount.Value);
                    //Logger.Log(LogLevel.Info, "mude "+TextReplacementToolConfig.replacementStringToBeReplaced.Length);
                    string noobert = Regex.Replace(myString, TextReplacementToolConfig.replacementStringToBeReplaced[i].Value, TextReplacementToolConfig.replacementStringToReplaceWith[i].Value, ((!TextReplacementToolConfig.caseSensitiveOverride.Value && TextReplacementToolConfig.replacementBoolCaseSensitive[i].Value) ? RegexOptions.None : RegexOptions.IgnoreCase));
                    //Logger.Log(LogLevel.Info, "flude");
                    if (stroin(noobert) != stroin(myString) && TextReplacementToolConfig.replacementStringToBeReplaced[i].Value.ToCharArray().Length > 1)
                    {
                        myString = noobert;
                        if (TextReplacementToolConfig.oneReplacementPerText.Value || TextReplacementToolConfig.replacementBoolStopReplaceAfter[i].Value)
                        {
                            break;
                        }
                    }
                }
                //Logger.Log(LogLevel.Info, "deede");
            }

            /*if (stroin(Regex.Replace(myString, "SURVIVOR", "THE VIVOR", RegexOptions.IgnoreCase)) != stroin(myString))
            {
                myString = Regex.Replace(myString,"SURVIVOR","THE VIVOR",RegexOptions.IgnoreCase);
            }*/
            return myString;
        }

        private string TranslateDeez(On.InGameTranslator.orig_Translate orig, InGameTranslator self, string s)
        {
            string myString = orig(self, s);
            
            return StringInator(myString);
        }

        private void TranslateDaButton(On.Menu.SimpleButton.orig_GrafUpdate orig, SimpleButton self, float timeStacker)
        {
            if (replaceableSignals.Contains(stroin(self.signalText)) && self.GetCustomData().origMessage == "")
            {
                self.GetCustomData().origMessage = self.menuLabel.text+"";
                self.menuLabel.text = StringInator(self.menuLabel.text);
            }
            orig(self,timeStacker);
        }
        
        // MARKER: Lists n Stuff
        public string[] replaceableSignals = ["GETFANCY"];

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
        public static Configurable<string>[] replacementStringFunnyIDNumber;// = Instance.config.Bind("replacementStringFunnyIDNumber", "0");
        public static Configurable<int> replacementCount = Instance.config.Bind("replacementCount", 1);

        public static OpTextBox[] replacementStringToBeReplacedInputtables = new OpTextBox[32];
        public static OpTextBox[] replacementStringToReplaceWithInputtables = new OpTextBox[32];
        public static OpCheckBox[] replacementBoolCaseSensitiveInputtables = new OpCheckBox[32];
        public static OpCheckBox[] replacementBoolStopReplaceAfterInputtables = new OpCheckBox[32];

        public static Configurable<int> evilDebugNumber1 = Instance.config.Bind("evilDebugNumber1", 36, new ConfigAcceptableRange<int>(10, 50));
        public static Configurable<int> evilDebugNumber2 = Instance.config.Bind("evilDebugNumber2", 120, new ConfigAcceptableRange<int>(40, 240));
        public OpSlider evilOpSlider1;
        public OpSlider evilOpSlider2;
        
        //public OpTab tabWithinMyOpScrollBox;
        public OpScrollBox myOpScrollBox;
        //public UIelement[] myOpRects = new OpRect[10];

        public bool shouldUpdate = true;
        public int whichButtonTypePressed = -1;
        public int whereButtonPressed = -1;
        public bool wasButtonPressed = true;

        // Menus and stuff
        public override void Initialize()
        {
            base.Initialize();
            Tabs = [
                new OpTab(this, "Main Page")
            ];
            /*
            if (myOpScrollBox.items != null && myOpScrollBox.items.Count > 0)
            {
                foreach (UIelement element in myOpScrollBox.items)
                {
                    element.Deactivate();
                    myOpScrollBox.tab._RemoveItem(element);
                    element.Unload();
                }
                myOpScrollBox.items.Clear();
            }*/

            //tabWithinMyOpScrollBox = new OpTab(this, "Replacement Interface");
            myOpScrollBox = new OpScrollBox(new Vector2(20f, 20f), new Vector2(550f, 400f), 1f);
            
            /*for (int i = 0; i < myOpRects.Length; i++)
            {
                myOpRects[i] = new OpRect(new Vector2(0f,0f),new Vector2(0f,0f));
            }*/
            //Plugin.Log(LogLevel.Info, "bbb");
            //myOpScrollBox.tab.AddItems(myOpRects);
            //Plugin.Log(LogLevel.Info, "ccc");
            //evilOpSlider1 = new OpSlider(evilDebugNumber1, new Vector2(30f,10f),50);// {mousewheelTick = 1, description = "debug one"};
            //evilOpSlider2 = new OpSlider(evilDebugNumber2, new Vector2(90f,10f),50) {mousewheelTick = 20, description = "debug two"};

            //Plugin.Log(LogLevel.Info, "aaa");
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
                
                // this is debug stuff not really needed smh
                //evilOpSlider1,
                //evilOpSlider2,
            ]);

            if (!Plugin.configInitializedYet)
            {
                for (int i = 0; i < 32; i++)
                {
                    //Plugin.Log(LogLevel.Info, "bbb "+i);
                    //replacementStringToBeReplaced=replacementStringToBeReplaced.AddToArray(Instance.config.Bind("replacementStringToBeReplaced_0","11"));
                    //replacementStringToBeReplaced=replacementStringToBeReplaced.AddToArray(Instance.config.Bind("replacementStringToBeReplaced_1","11"));
                    //Plugin.Log(LogLevel.Info, "ccc "+i);
                    replacementStringToBeReplaced=replacementStringToBeReplaced.AddToArray(Instance.config.Bind("replacementStringToBeReplaced_"+i.ToString(),"toReplace "));
                    //Plugin.Log(LogLevel.Info, "ddd1 "+i);
                    replacementStringToReplaceWith=replacementStringToReplaceWith.AddToArray(Instance.config.Bind("replacementStringToReplaceWith_"+i.ToString(),"replacedText "));
                    //Plugin.Log(LogLevel.Info, "ddd2 "+i);
                    replacementBoolCaseSensitive=replacementBoolCaseSensitive.AddToArray(Instance.config.Bind("replacementBoolCaseSensitive_"+i.ToString(),false));
                    //Plugin.Log(LogLevel.Info, "ddd3 "+i);
                    replacementBoolStopReplaceAfter=replacementBoolStopReplaceAfter.AddToArray(Instance.config.Bind("replacementBoolStopReplaceAfter_"+i.ToString(),false));
                    //Plugin.Log(LogLevel.Info, "ddd4 "+i);
                    replacementStringFunnyIDNumber=replacementStringFunnyIDNumber.AddToArray(Instance.config.Bind("replacementStringFunnyIDNumber_"+i.ToString(),"0"));
                    //Plugin.Log(LogLevel.Info, "ddd4 "+i);
                    //Plugin.Log(LogLevel.Info, "aag "+replacementStringToBeReplaced[replacementStringToBeReplaced.Length-1].key);
                    //Plugin.Log(LogLevel.Info, "awg "+replacementStringFunnyIDNumber[replacementStringFunnyIDNumber.Length-1].key);
                }
                replacementStringFunnyIDNumber=replacementStringFunnyIDNumber.AddToArray(Instance.config.Bind("replacementStringFunnyIDNumber_32","0"));
                Plugin.configInitializedYet = true;
            }
            Instance.config.Save();

            shouldUpdate = true;
        }

        public override void Update()
        {
            for (int i = 0; i < 32; i++)
            {
                if (replacementStringToBeReplaced[i].BoundUIconfig != null)
                {
                    replacementStringToBeReplaced[i].Value = replacementStringToBeReplacedInputtables[i].value;
                    replacementStringToReplaceWith[i].Value = replacementStringToReplaceWithInputtables[i].value;
                    replacementBoolCaseSensitive[i].Value = bool.TryParse(replacementBoolCaseSensitiveInputtables[i].value, out var result1) && result1;
                    replacementBoolStopReplaceAfter[i].Value = bool.TryParse(replacementBoolStopReplaceAfterInputtables[i].value, out var result2) && result2;
                }
            }

            if (whichButtonTypePressed != -1 && whereButtonPressed != -1)
            {
                Plugin.Log(LogLevel.Info, "[TextReplacementTool] Replacement "+whereButtonPressed+" button type "+whichButtonTypePressed+" pressed");
                switch (whichButtonTypePressed)
                {
                    case 0:
                    {
                        if (whereButtonPressed > 0)
                        {
                            string preStringToBeReplaced = replacementStringToBeReplaced[whereButtonPressed-1].Value+"";
                            string preStringToReplaceWith = replacementStringToReplaceWith[whereButtonPressed-1].Value+"";
                            bool preBoolCaseSensitive = (replacementBoolCaseSensitive[whereButtonPressed-1].Value ? true : false);
                            bool preBoolStopReplaceAfter = (replacementBoolStopReplaceAfter[whereButtonPressed-1].Value ? true : false);
                            string preStringFunnyIDNumber = replacementStringFunnyIDNumber[whereButtonPressed-1].Value+"";
                            replacementStringToBeReplaced[whereButtonPressed-1].Value = replacementStringToBeReplaced[whereButtonPressed].Value+"";
                            replacementStringToReplaceWith[whereButtonPressed-1].Value = replacementStringToReplaceWith[whereButtonPressed].Value+"";
                            replacementBoolCaseSensitive[whereButtonPressed-1].Value = (replacementBoolCaseSensitive[whereButtonPressed].Value ? true : false);
                            replacementBoolStopReplaceAfter[whereButtonPressed-1].Value = (replacementBoolStopReplaceAfter[whereButtonPressed].Value ? true : false);
                            replacementStringFunnyIDNumber[whereButtonPressed-1].Value = replacementStringFunnyIDNumber[whereButtonPressed].Value+"";
                            replacementStringToBeReplaced[whereButtonPressed].Value = preStringToBeReplaced;
                            replacementStringToReplaceWith[whereButtonPressed].Value = preStringToReplaceWith;
                            replacementBoolCaseSensitive[whereButtonPressed].Value = preBoolCaseSensitive;
                            replacementBoolStopReplaceAfter[whereButtonPressed].Value = preBoolStopReplaceAfter;
                            replacementStringFunnyIDNumber[whereButtonPressed].Value = preStringFunnyIDNumber;
                        }
                        break;
                    }
                    case 1:
                    {
                        if (whereButtonPressed + 1 < replacementCount.Value)
                        {
                            string preStringToBeReplaced = replacementStringToBeReplaced[whereButtonPressed+1].Value+"";
                            string preStringToReplaceWith = replacementStringToReplaceWith[whereButtonPressed+1].Value+"";
                            bool preBoolCaseSensitive = (replacementBoolCaseSensitive[whereButtonPressed+1].Value ? true : false);
                            bool preBoolStopReplaceAfter = (replacementBoolStopReplaceAfter[whereButtonPressed+1].Value ? true : false);
                            string preStringFunnyIDNumber = replacementStringFunnyIDNumber[whereButtonPressed+1].Value+"";
                            replacementStringToBeReplaced[whereButtonPressed+1].Value = replacementStringToBeReplaced[whereButtonPressed].Value+"";
                            replacementStringToReplaceWith[whereButtonPressed+1].Value = replacementStringToReplaceWith[whereButtonPressed].Value+"";
                            replacementBoolCaseSensitive[whereButtonPressed+1].Value = (replacementBoolCaseSensitive[whereButtonPressed].Value ? true : false);
                            replacementBoolStopReplaceAfter[whereButtonPressed+1].Value = (replacementBoolStopReplaceAfter[whereButtonPressed].Value ? true : false);
                            replacementStringFunnyIDNumber[whereButtonPressed+1].Value = replacementStringFunnyIDNumber[whereButtonPressed].Value+"";
                            replacementStringToBeReplaced[whereButtonPressed].Value = preStringToBeReplaced;
                            replacementStringToReplaceWith[whereButtonPressed].Value = preStringToReplaceWith;
                            replacementBoolCaseSensitive[whereButtonPressed].Value = preBoolCaseSensitive;
                            replacementBoolStopReplaceAfter[whereButtonPressed].Value = preBoolStopReplaceAfter;
                            replacementStringFunnyIDNumber[whereButtonPressed].Value = preStringFunnyIDNumber;
                        }
                        break;
                    }
                    case 2:
                    {
                        if (replacementCount.Value > 1)
                        {
                            //Plugin.Log(LogLevel.Info, "Replacement Count: "+replacementCount.Value+"\nStarting position: "+(whereButtonPressed+1));
                            for (int i = whereButtonPressed + 1; i < replacementCount.Value; i++)
                            {
                                //Plugin.Log(LogLevel.Info, "Moving Replacement " + i + " down by one!");
                                replacementStringToBeReplaced[i - 1].Value = replacementStringToBeReplaced[i].Value;
                                replacementStringToReplaceWith[i - 1].Value = replacementStringToReplaceWith[i].Value;
                                replacementBoolCaseSensitive[i - 1].Value = replacementBoolCaseSensitive[i].Value;
                                replacementBoolStopReplaceAfter[i - 1].Value = replacementBoolStopReplaceAfter[i].Value;
                                replacementStringFunnyIDNumber[i - 1].Value = replacementStringFunnyIDNumber[i].Value;
                            }
                            replacementCount.Value -= 1;
                            //Plugin.Log(LogLevel.Info, "Now the "+replacementCount.Value+" is length.");
                        }
                        break;
                    }
                }
                whichButtonTypePressed = -1;
                whereButtonPressed = -1;
                shouldUpdate = true;
                wasButtonPressed = true;
            }
            if (shouldUpdate)
            {
                //Plugin.Log(LogLevel.Info, "fff "+myOpScrollBox.items.Count);
                //Plugin.Log(LogLevel.Info, "ddd "+myOpRects.Length);
                //Plugin.Log(LogLevel.Info, "there's a "+replacementCount.Value);
                for (int i = 0; i < replacementCount.Value; i++)
                {
                    //Plugin.Log(LogLevel.Info, "janue a " + replacementStringFunnyIDNumber[i].Value);
                }
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

                //int yOffset = (int)Math.Max(0, (replacementCount.Value - (evilOpSlider1.GetValueInt()/10)) * evilOpSlider2.GetValueInt());
                int yOffset = (int)Math.Max(0, (replacementCount.Value < 32 ? 80f : 20f)+120f*(replacementCount.Value)-400);
                //Plugin.Log(LogLevel.Info, "that value there is " + (yOffset)+" from being "+replacementCount.Value);
                //yOffset = 0;
                for (int i = 0; i < replacementCount.Value; i++)
                {
                    //Plugin.Log(LogLevel.Info, "that value juino la " + (280f-i*120f+yOffset)+" de la "+i);
                    var i1 = i+0;
                    OpSimpleButton butoneUp = new OpSimpleButton(new Vector2(420f, 335f-i*120f+yOffset), new Vector2(30f, 45f)) { description = "Moves this Replacement up in order." };
                    OpSimpleButton butoneDown = new OpSimpleButton(new Vector2(420f, 280f-i*120f+yOffset), new Vector2(30f, 45f)) { description = "Moves this Replacement down in order." };
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
                        Plugin.Log(LogLevel.Info, "[TextReplacementTool] Button "+(i1+0)+" Pressed!");
                        whichButtonTypePressed = 2;
                        whereButtonPressed = i1+0;
                    };

                    if (replacementStringToBeReplaced[i].BoundUIconfig != null)
                    {
                        replacementStringToBeReplacedInputtables[i].Unload();
                        replacementStringToReplaceWithInputtables[i].Unload();
                        replacementBoolCaseSensitiveInputtables[i].Unload();
                        replacementBoolStopReplaceAfterInputtables[i].Unload();
                    }
                    replacementStringToBeReplacedInputtables[i] = new OpTextBox(replacementStringToBeReplaced[i], new Vector2(165,320f-i*120f+yOffset), 240f) {cosmetic = false, maxLength = 290, value=replacementStringToBeReplaced[i].Value, description = ""};
                    replacementStringToReplaceWithInputtables[i] = new OpTextBox(replacementStringToReplaceWith[i], new Vector2(165,290f-i*120f+yOffset), 240f) {cosmetic = false, maxLength = 290, value=replacementStringToReplaceWith[i].Value, description = ""};
                    replacementBoolCaseSensitiveInputtables[i] = new OpCheckBox(replacementBoolCaseSensitive[i], new Vector2(160,350f-i*120f+yOffset)) {cosmetic = false, description = ""};
                    replacementBoolCaseSensitiveInputtables[i].SetValueBool(replacementBoolCaseSensitive[i].Value);
                    replacementBoolStopReplaceAfterInputtables[i] = new OpCheckBox(replacementBoolStopReplaceAfter[i], new Vector2(270,350f-i*120f+yOffset)) {cosmetic = false, description = ""};
                    replacementBoolStopReplaceAfterInputtables[i].SetValueBool(replacementBoolStopReplaceAfter[i].Value);
                    myOpScrollBox.AddItems([
                        new OpRect(new Vector2(40f, 280f-i*120f+yOffset), new Vector2(370f, 100f)),
                        new OpLabel(60f, 280f-i*120f+yOffset, "Entry "+i),
                        /*new OpLabel(50f, 320f-i*120f+yOffset, 
                        "To Replace: "+replacementStringToBeReplaced[i].Value+ 
                        "\nReplace With: "+replacementStringToReplaceWith[i].Value+
                        "\nCase Sensitive: "+replacementBoolCaseSensitive[i].Value+
                        "\nStop Replace After: "+replacementBoolStopReplaceAfter[i].Value),*/
                        new OpLabel(60f, 350f-i*120f+yOffset, "(GUID: "+replacementStringFunnyIDNumber[i].Value+")"),
                        new OpLabel(50,325f-i*120f+yOffset, "Text to be replaced"),
                        new OpLabel(50,295f-i*120f+yOffset, "Text to replace with"),
                        new OpLabel(190,350f-i*120f+yOffset, "Case Sensitive"),
                        new OpLabel(300,350f-i*120f+yOffset, "Stop replacement\nafter this"),
                        replacementStringToBeReplacedInputtables[i],
                        replacementStringToReplaceWithInputtables[i],
                        replacementBoolCaseSensitiveInputtables[i],
                        replacementBoolStopReplaceAfterInputtables[i],
                        //new OpRect(new Vector2(420f, 280f-i*120f+yOffset), new Vector2(30f, 45f)),
                        //new OpRect(new Vector2(420f, 335f-i*120f+yOffset), new Vector2(30f, 45f)),
                        //new OpRect(new Vector2(460f, 280f-i*120f+yOffset), new Vector2(30f, 100f)),
                        butoneUp,
                        butoneDown,
                        butoneDelete,
                    ]);
                }

                // ReSharper disable once EqualExpressionComparison
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
#pragma warning disable CS0162 // Unreachable code detected
                if (0 == 1)
                {
                    myOpScrollBox.AddItems([
                        new OpLabel(0f, 0f+yOffset, "hmm"),
                        new OpLabel(30f, 0f, "hmm2"),
                    ]);
                }
#pragma warning restore CS0162 // Unreachable code detected

                if (replacementCount.Value < 32)
                {
                    OpSimpleButton myAwesomeButton = new OpSimpleButton(new Vector2(40f, 340f-replacementCount.Value*120f+yOffset), new Vector2(450f, 40f)) { description = "Adds another Replacement." };
                    myAwesomeButton.OnClick += delegate(UIfocusable _)
                    {
                        shouldUpdate = true;
                        replacementCount.Value += 1;
                        replacementStringToBeReplaced[replacementCount.Value - 1].Value = "toReplace";//"toReplace #"+(replacementCount.Value - 1);
                        replacementStringToReplaceWith[replacementCount.Value - 1].Value = "replacedText";
                        replacementBoolCaseSensitive[replacementCount.Value - 1].Value = false;
                        replacementBoolStopReplaceAfter[replacementCount.Value - 1].Value = false;
                        replacementStringFunnyIDNumber[replacementCount.Value - 1].Value = Random.Range(1000000,9999999).ToString();
                        if (replacementStringToBeReplaced[replacementCount.Value - 1].BoundUIconfig != null)
                        {
                            replacementStringToBeReplacedInputtables[replacementCount.Value - 1].value = "toReplace";
                            replacementStringToReplaceWithInputtables[replacementCount.Value - 1].value = "replacedText";
                            replacementBoolCaseSensitiveInputtables[replacementCount.Value - 1].value = "false";
                            replacementBoolStopReplaceAfterInputtables[replacementCount.Value - 1].value = "false";
                        }
                    };
                    myOpScrollBox.AddItems([
                        myAwesomeButton,
                        //new OpRect(new Vector2(40f, 340f-replacementCount.Value*120f+yOffset), new Vector2(320f, 40f)),
                        new OpLabel(250f, 350f-replacementCount.Value*120f+yOffset, "+",true),
                    ]);
                }

                if ((replacementCount.Value < 32 ? 80 : 20) + 120 * (replacementCount.Value) !=
                    (int)Math.Round(myOpScrollBox.contentSize))
                {
                    myOpScrollBox.SetContentSize((replacementCount.Value < 32 ? 80f : 20f)+120f*(replacementCount.Value),false);
                }
                wasButtonPressed = (whichButtonTypePressed != -1);
                _SaveConfigFile();
                
                
                // ok this should stop the constant updating every frame
                shouldUpdate = false;
            }
            Instance.config.Save();
            base.Update();
        }
    }
}