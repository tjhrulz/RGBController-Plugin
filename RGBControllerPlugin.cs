using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Rainmeter;

using Corale.Colore.Core;

using Corale.Colore.Razer.Mouse;
using Corale.Colore.Razer.Mouse.Effects;

using Corale.Colore.Razer.Headset;
using Corale.Colore.Razer.Headset.Effects;

using Corale.Colore.Razer.Keyboard;
using Corale.Colore.Razer.Keyboard.Effects;

using CUE.NET;
using CUE.NET.Devices.Generic.Enums;

using System.Threading;
using System.Diagnostics;
using CUE.NET.Devices.Mouse.Enums;

namespace PluginRGBController
{
    internal class Measure
    {
        //Keep track of how many instances of the plugin are floating around to ensure that deinitialization happens correctly
        public static int numOfInstances = 0;
        //In the event that a scenario occurs where the effect may have been overridden this will become true
        public static bool mayNeedToRedoEffect = false;
        public static int instancesRedone = 0;

        //Lock so that errors and their info get outputed together and in order 
        static object errorOutputLock = new object();

        enum effectTypes
        {
            STATIC,
            BREATHING,
            //BLINKING,
            WAVE,
            SPECTRUM,
            REACTIVE,
            GRADIENT
        }
        enum deviceTypes
        {
            MOUSE,
            HEADSET,
            KEYBOARD
        }
        enum manufactureTypes
        {
            RAZER,
            CORSAIR,
            LOGITECH
        }

        //Color to report as string for user
        //Note: Sometimes is not an RGB value
        String currentColor = "";
        //A string of what the parameters were for the last update so we dont do it again unless we need to.
        //Also may be useful for if we detect that we need to redo the color but is not used for that (Use cases would be if skin is unloaded load the other skins last known and maybe to fix the screen off bug and the refresh bug)
        String lastUpdate = "";

        String RGB = "";
        String RGB2 = "";
        String effect = "";
        String device = "";
        String manufacture = "";
        double percent = 0.0;
        String percentMax = "100";
        String percentMin = "0";


        Boolean colorAllLEDs = true;
        String mouseTarget = "ALL";
        static Color[] mouseLEDColorArr = new Color[30];

        String keyboardTarget = "ALL";
        enum keyboardGroups
        {
            MAIN, //The normal keys
            MAINNOWASD, //Normal keys not including WASD
            WASD, //W A S and D keys
            NUMBERS, //Top row of the main section of the keyboard
            QWER, //The row on keyboard containing qwer
            ASDF, //The row on keyboard containing asdf
            ZXCV, //The row on keyboard containing zxcv
            CTRL, //The bottom row on the keyboard
            NUMPAD, //Numpad
            ARROWS, //Arrow keys
            FUNCTION, //The 12 function keys and the escape button
            FUNCTIONNOESCAPE, //The 12 function keys and the escape button
            SYSTEM, //The 9 system keys (insert, home, pg up, delete, end, pg down, print screen, scroll lock and break) 
            EXTRAS, //Any extra keys your keyboard has, if applicable
            LOGO, //Logo on the keyboard
            CUSTOM //A set of custom keys to change defined in the rainmeter measure using the option keylist (comma or space seperated)
        }

        #region Definitions of custom Razer Key lists
        static List<Key> mainKeylistRazer = new List<Key> { Key.OemTilde, Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9, Key.D0, Key.OemMinus, Key.OemEquals, Key.Backspace, Key.Tab, Key.Q, Key.W, Key.E, Key.R, Key.T, Key.Y, Key.U, Key.I, Key.O, Key.P, Key.OemLeftBracket, Key.OemRightBracket, Key.OemBackslash, Key.CapsLock, Key.A, Key.S, Key.D, Key.F, Key.G, Key.H, Key.J, Key.K, Key.L, Key.OemSemicolon, Key.OemApostrophe, Key.Enter, Key.LeftShift, Key.Z, Key.X, Key.C, Key.V, Key.B, Key.N, Key.M, Key.OemComma, Key.OemPeriod, Key.OemSlash, Key.RightShift, Key.LeftControl, Key.LeftWindows, Key.LeftAlt, Key.Space, Key.RightAlt, Key.Function, Key.RightMenu, Key.RightControl };
        static List<Key> mainNoWASDKeylistRazer = new List<Key> { Key.OemTilde, Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9, Key.D0, Key.OemMinus, Key.OemEquals, Key.Backspace, Key.Tab, Key.Q, Key.E, Key.R, Key.T, Key.Y, Key.U, Key.I, Key.O, Key.P, Key.OemLeftBracket, Key.OemRightBracket, Key.OemBackslash, Key.CapsLock, Key.F, Key.G, Key.H, Key.J, Key.K, Key.L, Key.OemSemicolon, Key.OemApostrophe, Key.Enter, Key.LeftShift, Key.Z, Key.X, Key.C, Key.V, Key.B, Key.N, Key.M, Key.OemComma, Key.OemPeriod, Key.OemSlash, Key.RightShift, Key.LeftControl, Key.LeftWindows, Key.LeftAlt, Key.Space, Key.RightAlt, Key.Function, Key.RightMenu, Key.RightControl };
        static List<Key> wasdKeylistRazer = new List<Key> { Key.W, Key.A, Key.S, Key.D };

        static List<Key> numberKeylistRazer = new List<Key> { Key.OemTilde, Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9, Key.D0, Key.OemMinus, Key.OemEquals, Key.Backspace };
        static List<Key> qwerKeylistRazer = new List<Key> { Key.Tab, Key.Q, Key.W, Key.E, Key.R, Key.T, Key.Y, Key.U, Key.I, Key.O, Key.P, Key.OemLeftBracket, Key.OemRightBracket, Key.OemBackslash };
        static List<Key> asdfKeylistRazer = new List<Key> { Key.CapsLock, Key.A, Key.S, Key.D, Key.F, Key.G, Key.H, Key.J, Key.K, Key.L, Key.OemSemicolon, Key.OemApostrophe, Key.Enter };
        static List<Key> zxcvKeylistRazer = new List<Key> { Key.LeftShift, Key.Z, Key.X, Key.C, Key.V, Key.B, Key.N, Key.M, Key.OemComma, Key.OemPeriod, Key.OemSlash, Key.RightShift};
        static List<Key> ctrlKeylistRazer = new List<Key> { Key.LeftControl, Key.LeftWindows, Key.LeftAlt, Key.Space, Key.RightAlt, Key.Function, Key.RightMenu, Key.RightControl };


        static List<Key> numpadKeylistRazer = new List<Key> { Key.NumLock, Key.NumDivide, Key.NumMultiply, Key.NumSubtract, Key.Num7, Key.Num8, Key.Num9, Key.NumAdd, Key.Num4, Key.Num5, Key.Num6, Key.Num1, Key.Num2, Key.Num3, Key.NumEnter, Key.Num0, Key.NumDecimal };

        static List<Key> arrowsKeylistRazer = new List<Key> { Key.Up, Key.Left, Key.Down, Key.Right };

        static List<Key> functionKeylistRazer = new List<Key> { Key.Escape, Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6, Key.F7, Key.F8, Key.F9, Key.F10, Key.F11, Key.F12 };
        static List<Key> functionNoEscapeKeylistRazer = new List<Key> { Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6, Key.F7, Key.F8, Key.F9, Key.F10, Key.F11, Key.F12 };

        static List<Key> systemKeylistRazer = new List<Key> { Key.PrintScreen, Key.Scroll, Key.Pause, Key.Insert, Key.Home, Key.PageUp, Key.Delete, Key.End, Key.PageDown };

        static List<Key> extrasKeylistRazer = new List<Key> { Key.Macro1, Key.Macro2, Key.Macro3, Key.Macro4, Key.Macro5};

        static List<Key> logoKeylistRazer = new List<Key> { Key.Logo };

        //List of keys the measure is to use
        List<Key> customKeylistRazer = new List<Key> { };

        //Note keylistArr does not include custom keys so it can be static and take up less memory
        //I may wish to merge the keylistArrRazer with the corsair one in the future but seeing as the are two different functions I dont think it is worth it
        static List<List<Key>> keylistArrRazer = new List<List<Key>> { mainKeylistRazer, mainNoWASDKeylistRazer, wasdKeylistRazer, numberKeylistRazer, qwerKeylistRazer, asdfKeylistRazer, zxcvKeylistRazer, ctrlKeylistRazer, numpadKeylistRazer, arrowsKeylistRazer, functionKeylistRazer, functionNoEscapeKeylistRazer, systemKeylistRazer, extrasKeylistRazer, logoKeylistRazer };
        #endregion
        #region Definitions of custom Corsair Key Lists
        //TODO make corsair keylists
        #endregion


        void UpdateColorRazer(String RGB, String RGB2, String effect, String device, double percent)
        {
            //TODO raise flag when inside here so that I dont deinit when setting a new effect (Likely the cause of random errors)
            try
            {
                if (!Chroma.SdkAvailable)
                {
                    API.Log(API.LogType.Warning, "Cannot find chroma SDK, please make sure it is installed");
                }
                {
                    if (!Chroma.Instance.Initialized)
                    {
                        Chroma.Instance.Initialize();
                    }

                    if (device.CompareTo("ALL") == 0)
                    {
                        foreach (deviceTypes currDevice in Enum.GetValues(typeof(deviceTypes)))
                        {
                            UpdateColorRazer(RGB, RGB2, effect, currDevice.ToString(), percent);
                        }
                    }
                    else if (effect.CompareTo(effectTypes.SPECTRUM.ToString()) == 0)
                    {
                        currentColor = "Spectrum";
                        if (device.CompareTo(deviceTypes.MOUSE.ToString()) == 0)
                        {
                            Mouse.Instance.SetSpectrumCycling(new SpectrumCycling(Led.All));
                        }
                        else if (device.CompareTo(deviceTypes.HEADSET.ToString()) == 0)
                        {
                            Headset.Instance.SetEffect(Corale.Colore.Razer.Headset.Effects.Effect.SpectrumCycling);
                        }
                        else if (device.CompareTo(deviceTypes.KEYBOARD.ToString()) == 0)
                        {
                            Keyboard.Instance.SetEffect(Corale.Colore.Razer.Keyboard.Effects.Effect.SpectrumCycling);
                        }
                    }
                    else if (effect.CompareTo(effectTypes.WAVE.ToString()) == 0)
                    {
                        currentColor = "Wave";
                        //TODO Add user defined direction
                        if (device.CompareTo(deviceTypes.MOUSE.ToString()) == 0)
                        {
                            Mouse.Instance.SetWave(new Corale.Colore.Razer.Mouse.Effects.Wave(Corale.Colore.Razer.Mouse.Effects.Direction.FrontToBack));
                        }
                        else if (device.CompareTo(deviceTypes.KEYBOARD.ToString()) == 0)
                        {
                            Keyboard.Instance.SetWave(new Corale.Colore.Razer.Keyboard.Effects.Wave(Corale.Colore.Razer.Keyboard.Effects.Direction.LeftToRight));
                        }
                    }
                    else if (effect.CompareTo(effectTypes.GRADIENT.ToString()) == 0)
                    {
                        //I am debating two different ways to do this

                        //The first trys to maintain a bright color however it would make the transition between to dim colors get birght in between
                        //0 percent is 100% color1
                        //100 percent is 100% color2
                        //50 percent is color1 + color2 and if any value is higher than 255 then all colors are rescaled to relative that value
                        //So if color1 is 255,255,0 and color2 is 0,255,255 the midpoint would be 122,255,122
                        //The 25% point would be 255,319,64 which would scale to 204,255,51

                        //The second blends colors better but would get dimmer in between
                        //0 percent is 100% color1
                        //100 percent is 100% color2
                        //50 percent is 50% color1 + 50% color2
                        //25 percent is 75% color1 + 25% color2

                        //Currently the second one is the one used and it seems to work well enough
                        Color RGBColor = new Color(0, 0, 0);
                        Color RGBColor2 = new Color(0, 0, 0);

                        try
                        {
                            if (RGB != null && RGB != "")
                            {
                                String[] RGBarr = RGB.Split(',');
                                byte R = Convert.ToByte(RGBarr[0]);
                                byte G = Convert.ToByte(RGBarr[1]);
                                byte B = Convert.ToByte(RGBarr[2]);
                                RGBColor = new Color(R, G, B);
                            }

                            if (RGB2 != null && RGB2 != "")
                            {
                                String[] RGBarr = RGB2.Split(',');
                                byte R = Convert.ToByte(RGBarr[0]);
                                byte G = Convert.ToByte(RGBarr[1]);
                                byte B = Convert.ToByte(RGBarr[2]);
                                RGBColor2 = new Color(R, G, B);
                            }
                            else
                            {
                                RGB2 = "0,0,0";
                            }
                        }
                        catch
                        {
                            API.Log(API.LogType.Error, "RGB Value(s) are malformed, correct form is R,G,B");
                            return;
                        }

                        Color blendedColor = new Color((byte)(RGBColor.R * (1.0 - percent) + RGBColor2.R * percent), (byte)(RGBColor.G * (1.0 - percent) + RGBColor2.G * percent), (byte)(RGBColor.B * (1.0 - percent) + RGBColor2.B * percent));
                        currentColor = blendedColor.R.ToString() + "," + blendedColor.G.ToString() + "," + blendedColor.B.ToString();

                        if (device.CompareTo(deviceTypes.MOUSE.ToString()) == 0)
                        {
                            //Uses a global array of LED colors. Since each side can be targeted independently it just updates its part of the array.
                            //Supports targeting the all LEDs, Left side, Right side, or just the logo backlight and scrollwheel

                            //Update just the extra stuff
                            //Note we do this here for all to prevent duplicate code
                            if (mouseTarget == null || mouseTarget.CompareTo("EXTRA") == 0 || mouseTarget.CompareTo("ALL") == 0 || mouseTarget == "")
                            {
                                mouseLEDColorArr[(int)Led.Backlight] = blendedColor;
                                mouseLEDColorArr[(int)Led.Logo] = blendedColor;
                                mouseLEDColorArr[(int)Led.ScrollWheel] = blendedColor;
                            }
                            //Color mouse region all the same color
                            else if (colorAllLEDs)
                            {
                                //Color whole mouse same color
                                if (mouseTarget == null || mouseTarget.CompareTo("ALL") == 0 || mouseTarget == "")
                                {
                                    for (int i = (int)Led.Strip1; i <= (int)Led.Strip14; i++)
                                    {
                                        mouseLEDColorArr[i] = blendedColor;
                                    }
                                }
                                //Color left mouse region same color
                                else if (mouseTarget.CompareTo("LEFT") == 0)
                                {
                                    for (int i = (int)Led.Strip1; i <= (int)Led.Strip7; i++)
                                    {
                                        mouseLEDColorArr[i] = blendedColor;
                                    }
                                }
                                //Color right mouse region same color
                                else if (mouseTarget.CompareTo("RIGHT") == 0)
                                {
                                    for (int i = (int)Led.Strip8; i <= (int)Led.Strip14; i++)
                                    {
                                        mouseLEDColorArr[i] = blendedColor;
                                    }
                                }
                            }
                            //Intelegent coloring based on percent
                            //TODO make values inbetween light up next LED partially based on how close to value it is
                            else
                            {
                                //Update both side based on this 
                                if (mouseTarget == null || mouseTarget.CompareTo("ALL") == 0 || mouseTarget == "")
                                {
                                    //+1 is so that we use the count of LEDs
                                    int midPoint = ((int)Led.Strip14 - (int)Led.Strip1 + 1) / 2;

                                    for (int i = (int)Led.Strip1; i <= (int)Led.Strip14; i++)
                                    {
                                        //LED Order from bottom to top is 8 through 1 on the left and 9 through 14 on the right

                                        //Go through left side
                                        if (i < midPoint + (int)Led.Strip1)
                                        {
                                            //Plus one so the top light is not missed
                                            //Minus one so that it goes from bottom to top and not top to bottom
                                            if (1 - (double)(i - (int)Led.Strip1 + 1) / midPoint < percent)
                                            {
                                                mouseLEDColorArr[i] = blendedColor;
                                            }
                                            else
                                            {
                                                mouseLEDColorArr[i] = new Color(0, 0, 0);
                                            }
                                        }
                                        //Go through  right side
                                        else
                                        {
                                            //Plus one so the top light is not missed
                                            //Minus one so that it goes from bottom to top and not top to bottom
                                            if (1 - (double)(i - (int)Led.Strip1 - midPoint + 1) / midPoint < percent)
                                            {
                                                mouseLEDColorArr[i] = blendedColor;
                                            }
                                            else
                                            {
                                                mouseLEDColorArr[i] = new Color(0, 0, 0);
                                            }
                                        }
                                    }
                                }
                                else if (mouseTarget.CompareTo("LEFT") == 0)
                                {
                                    //+1 is so that we use the count of LEDs
                                    int midPoint = ((int)Led.Strip14 - (int)Led.Strip1 + 1) / 2;


                                    for (int i = (int)Led.Strip1; i <= (int)Led.Strip7; i++)
                                    {
                                        //LED Order from bottom to top is 8 through 1 on the left and 9 through 14 on the right

                                        //Go through left side
                                        if (i < midPoint + (int)Led.Strip1)
                                        {
                                            //Plus one so the top light is not missed
                                            //Minus one so that it goes from bottom to top and not top to bottom
                                            if (1 - (double)(i - (int)Led.Strip1 + 1) / midPoint < percent)
                                            {
                                                mouseLEDColorArr[i] = blendedColor;
                                            }
                                            else
                                            {
                                                mouseLEDColorArr[i] = new Color(0, 0, 0);
                                            }
                                        }
                                    }
                                }
                                else if (mouseTarget.CompareTo("RIGHT") == 0)
                                {
                                    //+1 is so that we use the count of LEDs
                                    int midPoint = ((int)Led.Strip14 - (int)Led.Strip1 + 1) / 2;


                                    for (int i = (int)Led.Strip8; i <= (int)Led.Strip14; i++)
                                    {
                                        //LED Order from bottom to top is 8 through 1 on the left and 9 through 14 on the right

                                        //Go through right side
                                        if (i >= midPoint + (int)Led.Strip1)
                                        {
                                            //Plus one so the top light is not missed
                                            //Minus one so that it goes from bottom to top and not top to bottom
                                            if (1 - (double)(i - (int)Led.Strip1 - midPoint + 1) / midPoint < percent)
                                            {
                                                mouseLEDColorArr[i] = blendedColor;
                                            }
                                            else
                                            {
                                                mouseLEDColorArr[i] = new Color(0, 0, 0);
                                            }
                                        }
                                    }
                                }
                            }

                            //Update mouse to current colors set
                            Corale.Colore.Razer.Mouse.Effects.Custom myGradientEffect = new Corale.Colore.Razer.Mouse.Effects.Custom(mouseLEDColorArr);
                            Mouse.Instance.SetCustom(myGradientEffect);
                        }
                        else if (device.CompareTo(deviceTypes.HEADSET.ToString()) == 0)
                        {
                            Headset.Instance.SetStatic(new Corale.Colore.Razer.Headset.Effects.Static(blendedColor));
                        }
                        else if (device.CompareTo(deviceTypes.KEYBOARD.ToString()) == 0)
                        {
                            if (keyboardTarget == null || keyboardTarget == "" || keyboardTarget.CompareTo("ALL") == 0)
                            {
                                Keyboard.Instance.SetAll(blendedColor);
                                //Keyboard.Instance.SetKeys(mainKeylist, blendedColor);
                            }
                            else if (keyboardTarget.CompareTo(keyboardGroups.CUSTOM.ToString()) == 0)
                            {
                                API.Log(API.LogType.Notice, "Custom keygroups not yet supported");
                            }
                            else
                            {
                                int index = -1;

                                try
                                {
                                    keyboardGroups groupLocation = (keyboardGroups)Enum.Parse(typeof(keyboardGroups), keyboardTarget);
                                    index = (int)groupLocation;
                                }
                                catch (ArgumentException)
                                {
                                    API.Log(API.LogType.Notice, "Keygroup " + keyboardTarget + "unrecognized, assuming coloring all keys");
                                }

                                if (index != -1)
                                {
                                    Keyboard.Instance.SetKeys(keylistArrRazer[index], blendedColor);
                                }
                                else
                                {
                                    Keyboard.Instance.SetAll(blendedColor);
                                }
                            }
                        }
                    }
                    else if (RGB != null && RGB != "")
                    {
                        Color RGBColor = new Color(0, 0, 0);
                        Color RGBColor2 = new Color(0, 0, 0);

                        try
                        {
                            {
                                String[] RGBarr = RGB.Split(',');
                                byte R = Convert.ToByte(RGBarr[0]);
                                byte G = Convert.ToByte(RGBarr[1]);
                                byte B = Convert.ToByte(RGBarr[2]);
                                RGBColor = new Color(R, G, B);
                            }

                            if (RGB2 != null && RGB2 != "")
                            {
                                String[] RGBarr = RGB2.Split(',');
                                byte R = Convert.ToByte(RGBarr[0]);
                                byte G = Convert.ToByte(RGBarr[1]);
                                byte B = Convert.ToByte(RGBarr[2]);
                                RGBColor2 = new Color(R, G, B);
                            }
                        }
                        catch
                        {
                            API.Log(API.LogType.Error, "RGB Value(s) are malformed, correct form is R,G,B");
                            return;
                        }

                        if (effect.CompareTo(effectTypes.STATIC.ToString()) == 0)
                        {
                            currentColor = RGBColor.R.ToString() + "," + RGBColor.G.ToString() + "," + RGBColor.B.ToString();
                            if (device.CompareTo(deviceTypes.MOUSE.ToString()) == 0)
                            {
                                for (int i = 0; i < mouseLEDColorArr.Length; i++)
                                {
                                    mouseLEDColorArr[i] = RGBColor;
                                }
                                Corale.Colore.Razer.Mouse.Effects.Custom myStaticEffect = new Corale.Colore.Razer.Mouse.Effects.Custom(mouseLEDColorArr);
                                Mouse.Instance.SetCustom(myStaticEffect);
                            }
                            else if (device.CompareTo(deviceTypes.HEADSET.ToString()) == 0)
                            {
                                Headset.Instance.SetStatic(new Corale.Colore.Razer.Headset.Effects.Static(RGBColor));
                            }
                            else if (device.CompareTo(deviceTypes.KEYBOARD.ToString()) == 0)
                            {
                                if (keyboardTarget == null || keyboardTarget == "" || keyboardTarget.CompareTo("ALL") == 0)
                                {
                                    Keyboard.Instance.SetAll(RGBColor);
                                    //Keyboard.Instance.SetKeys(mainKeylist, blendedColor);
                                }
                                else if (keyboardTarget.CompareTo(keyboardGroups.CUSTOM.ToString()) == 0)
                                {
                                    API.Log(API.LogType.Notice, "Custom keygroups not yet supported");
                                }
                                else
                                {
                                    int index = -1;

                                    try
                                    {
                                        keyboardGroups groupLocation = (keyboardGroups)Enum.Parse(typeof(keyboardGroups), keyboardTarget);
                                        index = (int)groupLocation;
                                    }
                                    catch (ArgumentException)
                                    {
                                        API.Log(API.LogType.Notice, "Keygroup " + keyboardTarget + "unrecognized, assuming coloring all keys");
                                    }

                                    //if (index == 0)
                                    //{
                                    //    //For some odd reason Visual studio keeps telling me this index is always 0 when it is not, temp fix for debuging till I feel like rebooting. Edit: Still doing it wtf VS2015
                                    //    Console.WriteLine("Index is actually 0, Hey VS2015 ° ͜ʖ͡° ╭∩╮");
                                    //}

                                    if (index != -1)
                                    {
                                        Keyboard.Instance.SetKeys(keylistArrRazer[index], RGBColor);
                                    }
                                    else
                                    {
                                        Keyboard.Instance.SetAll(RGBColor);
                                    }
                                }
                            }
                        }
                        else if (effect.CompareTo(effectTypes.BREATHING.ToString()) == 0)
                        {
                            currentColor = RGBColor.R.ToString() + "," + RGBColor.G.ToString() + "," + RGBColor.B.ToString();
                            if (device.CompareTo(deviceTypes.MOUSE.ToString()) == 0)
                            {
                                if (RGB2 == null || RGB2 == "")
                                {
                                    Mouse.Instance.SetBreathing(new Corale.Colore.Razer.Mouse.Effects.Breathing(Led.All, RGBColor));
                                }
                                else
                                {
                                    currentColor += ":" + RGBColor2.R.ToString() + "," + RGBColor2.G.ToString() + "," + RGBColor2.B.ToString();
                                    Mouse.Instance.SetBreathing(new Corale.Colore.Razer.Mouse.Effects.Breathing(Led.All, RGBColor, RGBColor2));
                                }
                            }
                            else if (device.CompareTo(deviceTypes.HEADSET.ToString()) == 0)
                            {
                                Headset.Instance.SetBreathing(new Corale.Colore.Razer.Headset.Effects.Breathing(RGBColor));
                            }
                            else if (device.CompareTo(deviceTypes.KEYBOARD.ToString()) == 0)
                            {
                                if (RGB2 == null || RGB2 == "")
                                {
                                    //For some odd reason keyboard breathing effect only takes two colors
                                    Keyboard.Instance.SetBreathing(new Corale.Colore.Razer.Keyboard.Effects.Breathing(RGBColor, RGBColor));
                                }
                                else
                                {
                                    currentColor += ":" + RGBColor2.R.ToString() + "," + RGBColor2.G.ToString() + "," + RGBColor2.B.ToString();
                                    Keyboard.Instance.SetBreathing(new Corale.Colore.Razer.Keyboard.Effects.Breathing(RGBColor, RGBColor2));
                                }
                            }
                        }
                        //else if (effect.CompareTo(effectTypes.BLINKING.ToString()) == 0)
                        //{
                        //    if (device.CompareTo(deviceTypes.MOUSE.ToString()) == 0)
                        //    {
                        //        Mouse.Instance.SetBlinking(new Blinking(Led.All, RGBColor));
                        //    }
                        //}
                        else if (effect.CompareTo(effectTypes.REACTIVE.ToString()) == 0)
                        {
                            currentColor = RGBColor.R.ToString() + "," + RGBColor.G.ToString() + "," + RGBColor.B.ToString();
                            //TODO Add user defined duration
                            if (device.CompareTo(deviceTypes.MOUSE.ToString()) == 0)
                            {
                                Mouse.Instance.SetReactive(new Corale.Colore.Razer.Mouse.Effects.Reactive(Led.All, Corale.Colore.Razer.Mouse.Effects.Duration.Long, RGBColor));
                            }
                            else if (device.CompareTo(deviceTypes.KEYBOARD.ToString()) == 0)
                            {
                                Keyboard.Instance.SetReactive(new Corale.Colore.Razer.Keyboard.Effects.Reactive(RGBColor, Corale.Colore.Razer.Keyboard.Effects.Duration.Long));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                lock(errorOutputLock)
                {
                    API.Log(API.LogType.Error, "An error occured when setting a new effect, this can happen sometimes when refreshing repeatedly. Turn on debug mode for more info");

                    API.Log(API.LogType.Debug, "RGBController.dll Error:" + e.Message + " doing:" + RGB + "," + RGB2 + "," + effect + "," + device + "," + percent + "," + mouseTarget + "," + keyboardTarget);
                    API.Log(API.LogType.Debug, "RGBController.dll Stacktrace:" + e.StackTrace);
                    API.Log(API.LogType.Debug, "RGBController.dll Data:" + e.Data);
                    API.Log(API.LogType.Debug, "RGBController.dll Error:" + e.InnerException.Message);
                    API.Log(API.LogType.Debug, "Feel free to contact me at tjhrulz#5476 on discord with this info");
                }
            }
        }
        void UpdateColorCorsair(String RGB, String RGB2, String effect, String device, double percent)
        {
            try
            {
                if (!CueSDK.IsSDKAvailable())
                {
                    API.Log(API.LogType.Warning, "Cannot find CUE SDK, please make sure it is installed");
                }
                else
                {
                    if (!CueSDK.IsInitialized)
                    {
                        CueSDK.Initialize();

                        //Having the update mode note be continuous means I can better manage update timings
                        //CueSDK.UpdateMode = UpdateMode.Continuous;
                    }

                    if (device.CompareTo("ALL") == 0)
                    {
                        foreach (deviceTypes currDevice in Enum.GetValues(typeof(deviceTypes)))
                        {
                            UpdateColorCorsair(RGB, RGB2, effect, currDevice.ToString(), percent);
                        }
                    }
                    else if (effect.CompareTo(effectTypes.SPECTRUM.ToString()) == 0)
                    {
                        currentColor = "Spectrum";
                        if (device.CompareTo(deviceTypes.MOUSE.ToString()) == 0)
                        {
                            //TODO Decide if I want to make setting every LED on the device an LED list instead of manual setting

                            API.Log(API.LogType.Warning, "Effect:" + effect + " is not yet implemented on corsair products");
                            foreach (CUE.NET.Devices.Generic.CorsairLed LED in CueSDK.MouseSDK.GetLeds())
                            {
                                CueSDK.MouseSDK[LED] = CUE.NET.Effects.
                            }
                        }
                        else if (device.CompareTo(deviceTypes.HEADSET.ToString()) == 0)
                        {
                            API.Log(API.LogType.Warning, "Effect:" + effect + " is not yet implemented on corsair products");
                        }
                        else if (device.CompareTo(deviceTypes.KEYBOARD.ToString()) == 0)
                        {
                            API.Log(API.LogType.Warning, "Effect:" + effect + " is not yet implemented on corsair products");
                        }
                    }
                    else if (effect.CompareTo(effectTypes.WAVE.ToString()) == 0)
                    {
                        currentColor = "Wave";
                        //TODO Add user defined direction
                        if (device.CompareTo(deviceTypes.MOUSE.ToString()) == 0)
                        {
                            API.Log(API.LogType.Warning, "Effect:" + effect + " is not yet implemented on corsair products");
                        }
                        else if (device.CompareTo(deviceTypes.KEYBOARD.ToString()) == 0)
                        {
                            API.Log(API.LogType.Warning, "Effect:" + effect + " is not yet implemented on corsair products");
                        }
                    }
                    else if (effect.CompareTo(effectTypes.GRADIENT.ToString()) == 0)
                    {
                        //I am debating two different ways to do this

                        //The first trys to maintain a bright color however it would make the transition between to dim colors get birght in between
                        //0 percent is 100% color1
                        //100 percent is 100% color2
                        //50 percent is color1 + color2 and if any value is higher than 255 then all colors are rescaled to relative that value
                        //So if color1 is 255,255,0 and color2 is 0,255,255 the midpoint would be 122,255,122
                        //The 25% point would be 255,319,64 which would scale to 204,255,51

                        //The second blends colors better but would get dimmer in between
                        //0 percent is 100% color1
                        //100 percent is 100% color2
                        //50 percent is 50% color1 + 50% color2
                        //25 percent is 75% color1 + 25% color2

                        //Currently the second one is the one used and it seems to work well enough
                        Color RGBColor = new Color(0, 0, 0);
                        Color RGBColor2 = new Color(0, 0, 0);

                        try
                        {
                            if (RGB != null && RGB != "")
                            {
                                String[] RGBarr = RGB.Split(',');
                                byte R = Convert.ToByte(RGBarr[0]);
                                byte G = Convert.ToByte(RGBarr[1]);
                                byte B = Convert.ToByte(RGBarr[2]);
                                RGBColor = new Color(R, G, B);
                            }

                            if (RGB2 != null && RGB2 != "")
                            {
                                String[] RGBarr = RGB2.Split(',');
                                byte R = Convert.ToByte(RGBarr[0]);
                                byte G = Convert.ToByte(RGBarr[1]);
                                byte B = Convert.ToByte(RGBarr[2]);
                                RGBColor2 = new Color(R, G, B);
                            }
                            else
                            {
                                RGB2 = "0,0,0";
                            }
                        }
                        catch
                        {
                            API.Log(API.LogType.Error, "RGB Value(s) are malformed, correct form is R,G,B");
                            return;
                        }

                        Color blendedColor = new Color((byte)(RGBColor.R * (1.0 - percent) + RGBColor2.R * percent), (byte)(RGBColor.G * (1.0 - percent) + RGBColor2.G * percent), (byte)(RGBColor.B * (1.0 - percent) + RGBColor2.B * percent));
                        currentColor = blendedColor.R.ToString() + "," + blendedColor.G.ToString() + "," + blendedColor.B.ToString();

                        if (device.CompareTo(deviceTypes.MOUSE.ToString()) == 0)
                        {
                            //Uses a global array of LED colors. Since each side can be targeted independently it just updates its part of the array.
                            //Supports targeting the all LEDs, Left side, Right side, or just the logo backlight and scrollwheel

                            //Update just the extra stuff
                            //Note we do this here for all to prevent duplicate code
                            if (mouseTarget == null || mouseTarget.CompareTo("EXTRA") == 0 || mouseTarget.CompareTo("ALL") == 0 || mouseTarget == "")
                            {
                                API.Log(API.LogType.Warning, "Effect:" + effect + " is not yet implemented on corsair products");
                            }
                            //Color mouse region all the same color
                            else if (colorAllLEDs)
                            {
                                //Color whole mouse same color
                                if (mouseTarget == null || mouseTarget.CompareTo("ALL") == 0 || mouseTarget == "")
                                {
                                    for (int i = (int)Led.Strip1; i <= (int)Led.Strip14; i++)
                                    {
                                        mouseLEDColorArr[i] = blendedColor;
                                    }
                                }
                                //Color left mouse region same color
                                else if (mouseTarget.CompareTo("LEFT") == 0)
                                {
                                    for (int i = (int)Led.Strip1; i <= (int)Led.Strip7; i++)
                                    {
                                        mouseLEDColorArr[i] = blendedColor;
                                    }
                                }
                                //Color right mouse region same color
                                else if (mouseTarget.CompareTo("RIGHT") == 0)
                                {
                                    for (int i = (int)Led.Strip8; i <= (int)Led.Strip14; i++)
                                    {
                                        mouseLEDColorArr[i] = blendedColor;
                                    }
                                }
                            }
                            //Intelegent coloring based on percent
                            //TODO make values inbetween light up next LED partially based on how close to value it is
                            else
                            {
                                //Update both side based on this 
                                if (mouseTarget == null || mouseTarget.CompareTo("ALL") == 0 || mouseTarget == "")
                                {
                                    //+1 is so that we use the count of LEDs
                                    int midPoint = ((int)Led.Strip14 - (int)Led.Strip1 + 1) / 2;

                                    for (int i = (int)Led.Strip1; i <= (int)Led.Strip14; i++)
                                    {
                                        //LED Order from bottom to top is 8 through 1 on the left and 9 through 14 on the right

                                        //Go through left side
                                        if (i < midPoint + (int)Led.Strip1)
                                        {
                                            //Plus one so the top light is not missed
                                            //Minus one so that it goes from bottom to top and not top to bottom
                                            if (1 - (double)(i - (int)Led.Strip1 + 1) / midPoint < percent)
                                            {
                                                mouseLEDColorArr[i] = blendedColor;
                                            }
                                            else
                                            {
                                                mouseLEDColorArr[i] = new Color(0, 0, 0);
                                            }
                                        }
                                        //Go through  right side
                                        else
                                        {
                                            //Plus one so the top light is not missed
                                            //Minus one so that it goes from bottom to top and not top to bottom
                                            if (1 - (double)(i - (int)Led.Strip1 - midPoint + 1) / midPoint < percent)
                                            {
                                                mouseLEDColorArr[i] = blendedColor;
                                            }
                                            else
                                            {
                                                mouseLEDColorArr[i] = new Color(0, 0, 0);
                                            }
                                        }
                                    }
                                }
                                else if (mouseTarget.CompareTo("LEFT") == 0)
                                {
                                    //+1 is so that we use the count of LEDs
                                    int midPoint = ((int)Led.Strip14 - (int)Led.Strip1 + 1) / 2;


                                    for (int i = (int)Led.Strip1; i <= (int)Led.Strip7; i++)
                                    {
                                        //LED Order from bottom to top is 8 through 1 on the left and 9 through 14 on the right

                                        //Go through left side
                                        if (i < midPoint + (int)Led.Strip1)
                                        {
                                            //Plus one so the top light is not missed
                                            //Minus one so that it goes from bottom to top and not top to bottom
                                            if (1 - (double)(i - (int)Led.Strip1 + 1) / midPoint < percent)
                                            {
                                                mouseLEDColorArr[i] = blendedColor;
                                            }
                                            else
                                            {
                                                mouseLEDColorArr[i] = new Color(0, 0, 0);
                                            }
                                        }
                                    }
                                }
                                else if (mouseTarget.CompareTo("RIGHT") == 0)
                                {
                                    //+1 is so that we use the count of LEDs
                                    int midPoint = ((int)Led.Strip14 - (int)Led.Strip1 + 1) / 2;


                                    for (int i = (int)Led.Strip8; i <= (int)Led.Strip14; i++)
                                    {
                                        //LED Order from bottom to top is 8 through 1 on the left and 9 through 14 on the right

                                        //Go through right side
                                        if (i >= midPoint + (int)Led.Strip1)
                                        {
                                            //Plus one so the top light is not missed
                                            //Minus one so that it goes from bottom to top and not top to bottom
                                            if (1 - (double)(i - (int)Led.Strip1 - midPoint + 1) / midPoint < percent)
                                            {
                                                mouseLEDColorArr[i] = blendedColor;
                                            }
                                            else
                                            {
                                                mouseLEDColorArr[i] = new Color(0, 0, 0);
                                            }
                                        }
                                    }
                                }
                            }

                            //Update mouse to current colors set
                            API.Log(API.LogType.Warning, "Effect:" + effect + " is not yet implemented on corsair products");
                        }
                        else if (device.CompareTo(deviceTypes.HEADSET.ToString()) == 0)
                        {
                            API.Log(API.LogType.Warning, "Effect:" + effect + " is not yet implemented on corsair products");
                        }
                        else if (device.CompareTo(deviceTypes.KEYBOARD.ToString()) == 0)
                        {
                            if (keyboardTarget == null || keyboardTarget == "" || keyboardTarget.CompareTo("ALL") == 0)
                            {
                                API.Log(API.LogType.Warning, "Effect:" + effect + " is not yet implemented on corsair products");
                            }
                            else if (keyboardTarget.CompareTo(keyboardGroups.CUSTOM.ToString()) == 0)
                            {
                                API.Log(API.LogType.Notice, "Custom keygroups not yet supported");
                            }
                            else
                            {
                                int index = -1;

                                try
                                {
                                    keyboardGroups groupLocation = (keyboardGroups)Enum.Parse(typeof(keyboardGroups), keyboardTarget);
                                    index = (int)groupLocation;
                                }
                                catch (ArgumentException)
                                {
                                    API.Log(API.LogType.Notice, "Keygroup " + keyboardTarget + "unrecognized, assuming coloring all keys");
                                }

                                if (index != -1)
                                {
                                    API.Log(API.LogType.Warning, "Effect:" + effect + " is not yet implemented on corsair products");
                                }
                                else
                                {
                                    API.Log(API.LogType.Warning, "Effect:" + effect + " is not yet implemented on corsair products");
                                }
                            }
                        }
                    }
                    else if (RGB != null && RGB != "")
                    {
                        Color RGBColor = new Color(0, 0, 0);
                        Color RGBColor2 = new Color(0, 0, 0);

                        try
                        {
                            {
                                String[] RGBarr = RGB.Split(',');
                                byte R = Convert.ToByte(RGBarr[0]);
                                byte G = Convert.ToByte(RGBarr[1]);
                                byte B = Convert.ToByte(RGBarr[2]);
                                RGBColor = new Color(R, G, B);
                            }

                            if (RGB2 != null && RGB2 != "")
                            {
                                String[] RGBarr = RGB2.Split(',');
                                byte R = Convert.ToByte(RGBarr[0]);
                                byte G = Convert.ToByte(RGBarr[1]);
                                byte B = Convert.ToByte(RGBarr[2]);
                                RGBColor2 = new Color(R, G, B);
                            }
                        }
                        catch
                        {
                            API.Log(API.LogType.Error, "RGB Value(s) are malformed, correct form is R,G,B");
                            return;
                        }

                        if (effect.CompareTo(effectTypes.STATIC.ToString()) == 0)
                        {
                            currentColor = RGBColor.R.ToString() + "," + RGBColor.G.ToString() + "," + RGBColor.B.ToString();
                            if (device.CompareTo(deviceTypes.MOUSE.ToString()) == 0)
                            {
                                for (int i = 0; i < mouseLEDColorArr.Length; i++)
                                {
                                    mouseLEDColorArr[i] = RGBColor;
                                }
                                API.Log(API.LogType.Warning, "Effect:" + effect + " is not yet implemented on corsair products");
                            }
                            else if (device.CompareTo(deviceTypes.HEADSET.ToString()) == 0)
                            {
                                API.Log(API.LogType.Warning, "Effect:" + effect + " is not yet implemented on corsair products");
                            }
                            else if (device.CompareTo(deviceTypes.KEYBOARD.ToString()) == 0)
                            {
                                if (keyboardTarget == null || keyboardTarget == "" || keyboardTarget.CompareTo("ALL") == 0)
                                {
                                    API.Log(API.LogType.Warning, "Effect:" + effect + " is not yet implemented on corsair products");
                                }
                                else if (keyboardTarget.CompareTo(keyboardGroups.CUSTOM.ToString()) == 0)
                                {
                                    API.Log(API.LogType.Notice, "Custom keygroups not yet supported");
                                }
                                else
                                {
                                    int index = -1;

                                    try
                                    {
                                        keyboardGroups groupLocation = (keyboardGroups)Enum.Parse(typeof(keyboardGroups), keyboardTarget);
                                        index = (int)groupLocation;
                                    }
                                    catch (ArgumentException)
                                    {
                                        API.Log(API.LogType.Notice, "Keygroup " + keyboardTarget + "unrecognized, assuming coloring all keys");
                                    }

                                    //if (index == 0)
                                    //{
                                    //    //For some odd reason Visual studio keeps telling me this index is always 0 when it is not, temp fix for debuging till I feel like rebooting. Edit: Still doing it wtf VS2015
                                    //    Console.WriteLine("Index is actually 0, Hey VS2015 ° ͜ʖ͡° ╭∩╮");
                                    //}

                                    if (index != -1)
                                    {
                                        API.Log(API.LogType.Warning, "Effect:" + effect + " is not yet implemented on corsair products");
                                    }
                                    else
                                    {
                                        API.Log(API.LogType.Warning, "Effect:" + effect + " is not yet implemented on corsair products");
                                    }
                                }
                            }
                        }
                        else if (effect.CompareTo(effectTypes.BREATHING.ToString()) == 0)
                        {
                            currentColor = RGBColor.R.ToString() + "," + RGBColor.G.ToString() + "," + RGBColor.B.ToString();
                            if (device.CompareTo(deviceTypes.MOUSE.ToString()) == 0)
                            {
                                if (RGB2 == null || RGB2 == "")
                                {
                                    API.Log(API.LogType.Warning, "Effect:" + effect + " is not yet implemented on corsair products");
                                }
                                else
                                {
                                    currentColor += ":" + RGBColor2.R.ToString() + "," + RGBColor2.G.ToString() + "," + RGBColor2.B.ToString();
                                    API.Log(API.LogType.Warning, "Effect:" + effect + " is not yet implemented on corsair products");
                                }
                            }
                            else if (device.CompareTo(deviceTypes.HEADSET.ToString()) == 0)
                            {
                                API.Log(API.LogType.Warning, "Effect:" + effect + " is not yet implemented on corsair products");
                            }
                            else if (device.CompareTo(deviceTypes.KEYBOARD.ToString()) == 0)
                            {
                                if (RGB2 == null || RGB2 == "")
                                {
                                    API.Log(API.LogType.Warning, "Effect:" + effect + " is not yet implemented on corsair products");
                                }
                                else
                                {
                                    currentColor += ":" + RGBColor2.R.ToString() + "," + RGBColor2.G.ToString() + "," + RGBColor2.B.ToString();
                                    API.Log(API.LogType.Warning, "Effect:" + effect + " is not yet implemented on corsair products");
                                }
                            }
                        }
                        //else if (effect.CompareTo(effectTypes.BLINKING.ToString()) == 0)
                        //{
                        //    if (device.CompareTo(deviceTypes.MOUSE.ToString()) == 0)
                        //    {
                        //        Mouse.Instance.SetBlinking(new Blinking(Led.All, RGBColor));
                        //    }
                        //}
                        else if (effect.CompareTo(effectTypes.REACTIVE.ToString()) == 0)
                        {
                            currentColor = RGBColor.R.ToString() + "," + RGBColor.G.ToString() + "," + RGBColor.B.ToString();
                            //TODO Add user defined duration
                            if (device.CompareTo(deviceTypes.MOUSE.ToString()) == 0)
                            {
                                API.Log(API.LogType.Warning, "Effect:" + effect + " is not yet implemented on corsair products");
                            }
                            else if (device.CompareTo(deviceTypes.KEYBOARD.ToString()) == 0)
                            {
                                API.Log(API.LogType.Warning, "Effect:" + effect + " is not yet implemented on corsair products");
                            }
                        }
                    }
                }
            }
            catch (CUE.NET.Exceptions.CUEException ex)
            {
                Debug.WriteLine("CUE Exception! ErrorCode: " + Enum.GetName(typeof(CUE.NET.Devices.Generic.Enums.CorsairError), ex.Error));
            }
            catch (CUE.NET.Exceptions.WrapperException ex)
            {
                Debug.WriteLine("Wrapper Exception! Message:" + ex.Message);
            }
        }

        internal Measure()
        {
            //Microsoft.Win32.SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }

        //private void SystemEvents_PowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
        //{
        //    API.Log(API.LogType.Debug, "Detected resume from sleep, reiniting");
        //    Chroma.Instance.Initialize();
        //    UpdateColor(RGB, RGB2, effect, device, percent);
        //}

        internal void Reload(Rainmeter.API api, ref double maxValue)
        {
            //TODO clean up to minimize reads on effect types that dont use them
            //Actually I think I am gonna keep them so that user can define stuff for if they change effects and its not a big impact on perf

            RGB = api.ReadString("Color", null);
            RGB2 = api.ReadString("Color2", null);

            effect = api.ReadString("Effect", "static").ToUpper();
            device = api.ReadString("Device", "all").ToUpper();
            manufacture = api.ReadString("Manufacture", "razer").ToUpper();

            String percentString = api.ReadString("Percent", null);
            percentMax = api.ReadString("PercentMax", "100");
            percentMin = api.ReadString("PercentMin", "0");

            colorAllLEDs = Convert.ToBoolean(Convert.ToInt16(api.ReadString("ColorAllLEDs", "1")));
            mouseTarget = api.ReadString("MouseTarget", "all").ToUpper();

            keyboardTarget = api.ReadString("KeyboardTarget", "all").ToUpper();

            percent = 0.0;

            if(percentString != null && percentString !="")
            {
                percent = (Convert.ToDouble(percentString) - Convert.ToDouble(percentMin)) / (Convert.ToDouble(percentMax) - Convert.ToDouble(percentMin));

                if (double.IsInfinity(percent))
                {
                    percent = 100;
                }
            }

            if (lastUpdate != RGB + ":" + RGB2 + ":" + effect + ":" + device + ":" + percent)
            {
                if (manufacture.CompareTo(manufactureTypes.RAZER.ToString()) == 0)
                {
                    UpdateColorRazer(RGB, RGB2, effect, device, percent);
                }
                else if (manufacture.CompareTo(manufactureTypes.CORSAIR.ToString()) == 0)
                {
                    API.Log(API.LogType.Error, "Corsair device support is a WIP");
                    UpdateColorCorsair(RGB, RGB2, effect, device, percent);
                }
                else if (manufacture.CompareTo(manufactureTypes.LOGITECH.ToString()) == 0)
                {
                    API.Log(API.LogType.Error, "Logitech devices not yet supported");
                }
                lastUpdate = RGB + ":" + RGB2 + ":" + effect + ":" + device + ":" + percent;
            }
        }

        internal double Update()
        {
            //Corale.Colore.Razer.DeviceInfo mouse = new ;
            //API.Log(API.LogType.Debug, "Mouse:" + Corale.Colore.Core.Chroma.Instance.Query(Corale.Colore.Razer.Devices.Mamba).Connected);

            return 0.0;
        }

        internal string GetString()
        {
            return currentColor;
            //return Chroma.Instance.Mouse[Led.Strip7].R.ToString() + "," + Chroma.Instance.Mouse[Led.Strip7].G.ToString() + "," + Chroma.Instance.Mouse[Led.Strip7].B.ToString();
        }

        internal void ExecuteBang(string args)
        {
            String[] argArr = args.Split(new char[] { ' ', ':' });


            int colorReadLocation = 0;
            //Check if the first location is an effect 
            if(Enum.IsDefined(typeof(effectTypes), argArr[0].ToUpper()))
            {
                effect = argArr[0].ToUpper();
                colorReadLocation = 1;

                if (effect.CompareTo(effectTypes.GRADIENT.ToString()) == 0)
                {
                    double newPercent = Convert.ToDouble(argArr[1]);
                    newPercent = (newPercent - Convert.ToDouble(percentMin)) / (Convert.ToDouble(percentMax) - Convert.ToDouble(percentMin));

                    if (double.IsInfinity(newPercent))
                    {
                        newPercent = 100;
                    }

                    percent = newPercent;
                    colorReadLocation = 2;
                }
            }
            else
            {
                if (effect.CompareTo(effectTypes.GRADIENT.ToString()) == 0)
                {
                    double newPercent = Convert.ToDouble(argArr[0]);
                    newPercent = (newPercent - Convert.ToDouble(percentMin)) / (Convert.ToDouble(percentMax) - Convert.ToDouble(percentMin));

                    if (double.IsInfinity(newPercent))
                    {
                        newPercent = 100;
                    }

                    percent = newPercent;
                    colorReadLocation = 1;
                }
            }

            //If there is a color to read after
            if(argArr.Length >= colorReadLocation + 1)
            {
                //Check if color if not inform user
                if (argArr[colorReadLocation].Split(',').Length >= 3)
                {

                    RGB = argArr[colorReadLocation].ToUpper();

                    //TODO add support for skipping to next argument if current one is not a color

                    //If there is a second color to read after
                    if (argArr.Length >= colorReadLocation + 2)
                    {
                        //Check if second location is a color if not inform user
                        if (argArr[colorReadLocation + 1].Split(',').Length >= 3)
                        {
                            RGB2 = argArr[colorReadLocation + 1].ToUpper();
                        }
                    }
                    else
                    {
                        API.Log(API.LogType.Warning, "Second color expected, got " + argArr[colorReadLocation + 1]);
                    }
                }
                else
                {
                    API.Log(API.LogType.Warning, "First color expected, got " + argArr[colorReadLocation]);
                }
            }

            if (lastUpdate != RGB + ":" + RGB2 + ":" + effect + ":" + device + ":" + percent)
            {
                if (manufacture.CompareTo(manufactureTypes.RAZER.ToString()) == 0)
                {
                    UpdateColorRazer(RGB, RGB2, effect, device, percent);
                }
                else if (manufacture.CompareTo(manufactureTypes.CORSAIR.ToString()) == 0)
                {
                    API.Log(API.LogType.Error, "Corsair device support is a WIP");
                    UpdateColorCorsair(RGB, RGB2, effect, device, percent);
                }
                else if (manufacture.CompareTo(manufactureTypes.LOGITECH.ToString()) == 0)
                {
                    API.Log(API.LogType.Error, "Logitech devices not yet supported");
                }
                lastUpdate = RGB + ":" + RGB2 + ":" + effect + ":" + device + ":" + percent;
            }
        }
    }

    public static class Plugin
    {
        static IntPtr StringBuffer = IntPtr.Zero;

        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            //*******************************************************************
            //Beter init and uninit code that causes rainmeter to crash on unload
            //*******************************************************************
            //if (deinitThread != null)
            //{
            //    if (deinitThread.ThreadState == System.Threading.ThreadState.WaitSleepJoin)
            //    {
            //        deinitThread.Abort();
            //    }
            //    else if(deinitThread.ThreadState == System.Threading.ThreadState.Running)
            //    {
            //        Thread.Sleep(1000);
            //        Chroma.Instance.Initialize();
            //    }
            //    else if (deinitThread.ThreadState != System.Threading.ThreadState.Aborted && deinitThread.ThreadState != System.Threading.ThreadState.AbortRequested)
            //    {
            //        Debug.WriteLine("Unsure what to do with thread" + deinitThread.ThreadState);
            //    }
            //}

            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure()));
        }

        [DllExport]
        public static void Finalize(IntPtr data)
        {
            //*******************************************************************
            //Beter init and uninit code that causes rainmeter to crash on unload
            //*******************************************************************
            //deinitThread = new Thread(() =>
            //{
            //    Thread.Sleep(500);
            //    
            //    //For some reason this does not want to delete effects sometimes
            //    Chroma.Instance.Uninitialize();
            //
            //});
            //deinitThread.Name = "ChromaDeinitializer";
            //deinitThread.Start();

            GCHandle.FromIntPtr(data).Free();

            if (StringBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(StringBuffer);
                StringBuffer = IntPtr.Zero;
            }
        }

        [DllExport]
        public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            measure.Reload(new Rainmeter.API(rm), ref maxValue);
        }

        [DllExport]
        public static double Update(IntPtr data)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            return measure.Update();
        }

        [DllExport]
        public static IntPtr GetString(IntPtr data)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            if (StringBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(StringBuffer);
                StringBuffer = IntPtr.Zero;
            }

            string stringValue = measure.GetString();
            if (stringValue != null)
            {
                StringBuffer = Marshal.StringToHGlobalUni(stringValue);
            }

            return StringBuffer;
        }

        [DllExport]
        public static void ExecuteBang(IntPtr data, IntPtr args)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            measure.ExecuteBang(Marshal.PtrToStringUni(args));
        }
    }
}
