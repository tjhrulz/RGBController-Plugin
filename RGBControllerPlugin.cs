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

namespace PluginRGBController
{
    internal class Measure
    {
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

        String currentColor = "";
        String lastUpdate = "";

        String device = "";
        String percentMax = "100";
        String percentMin = "0";


        Boolean dynamicMouseStrip = false;
        String mouseTarget = "ALL";
        static Color[] currMouseColor = new Color[30];

        String keyboardTarget = "ALL";
        enum keyboardGroups
        {
            MAIN, //The normal keys
            MAINNOWASD, //Normal keys not including WASD
            WASD, //W A S and D keys
            NUMPAD, //Numpad
            ARROWS, //Arrow keys
            FUNCTION, //The 12 function keys and the escape button
            SYSTEM, //The 9 system keys (insert, home, pg up, delete, end, pg down, print screen, scroll lock and break) 
            EXTRAS, //Any extra keys your keyboard has, if applicable
            CUSTOM //A set of custom keys to change defined in the rainmeter measure using the option keylist (comma or space seperated)
        }

        static List<Key> wasdKeys = new List<Key> { Key.W, Key.A, Key.S, Key.D };
        static List<Key> numberKeys = new List<Key> { Key.OemTilde, Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9, Key.D0, Key.OemMinus, Key.OemEquals, Key.Backspace };
        static List<Key> qwertyKeys = new List<Key> { Key.Tab, Key.Q, Key.W, Key.E, Key.R, Key.T, Key.Y, Key.U, Key.I, Key.O, Key.P, Key.OemLeftBracket, Key.OemRightBracket, Key.OemBackslash };
        static List<Key> qwertyNoWASDKeys = new List<Key> { Key.Tab, Key.Q, Key.W, Key.E, Key.R, Key.T, Key.Y, Key.U, Key.I, Key.O, Key.P, Key.OemLeftBracket, Key.OemRightBracket, Key.OemBackslash };
        static List<Key> asdfKeys = new List<Key> { Key.CapsLock, Key.A, Key.S, Key.D, Key.F, Key.G, Key.H, Key.J, Key.K, Key.L, Key.OemSemicolon, Key.OemApostrophe, Key.Enter };
        static List<Key> asdfNoWASDKeys = new List<Key> { Key.CapsLock, Key.F, Key.G, Key.H, Key.J, Key.K, Key.L, Key.OemSemicolon, Key.OemApostrophe, Key.Enter };
        static List<Key> zxcvKeys = new List<Key> { Key.LeftShift, Key.Z, Key.X, Key.C, Key.V, Key.B, Key.N, Key.M, Key.OemComma, Key.OemPeriod, Key.OemSlash, Key.RightShift};
        static List<Key> ctrlKeys = new List<Key> { Key.LeftControl, Key.LeftWindows, Key.LeftAlt, Key.Space, Key.RightAlt, Key.Function, Key.RightMenu, Key.RightControl };
        static List<Key> fnKeys = new List<Key> { Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6, Key.F7, Key.F8, Key.F9, Key.F10, Key.F11, Key.F12 };
        static List<Key> escapeKey = new List<Key> { Key.Escape };
        static List<Key> sysKeys = new List<Key> { Key.PrintScreen, Key.Scroll, Key.Pause };
        static List<Key> macroKeys = new List<Key> { Key.Macro1, Key.Macro2, Key.Macro3, Key.Macro4, Key.Macro5 };
        static List<Key> manipKeys = new List<Key> { Key.Insert, Key.Home, Key.PageUp, Key.Delete, Key.End, Key.PageDown };
        static List<Key> arrowKeys = new List<Key> { Key.Up, Key.Left, Key.Down, Key.Right };
        static List<Key> numpadKeys = new List<Key> { Key.NumLock, Key.NumDivide, Key.NumMultiply, Key.NumSubtract, Key.Num7, Key.Num8, Key.Num9, Key.NumAdd, Key.Num4, Key.Num5, Key.Num6, Key.Num1, Key.Num2, Key.Num3, Key.NumEnter, Key.Num0, Key.NumDecimal };
        static List<Key> logoKey = new List<Key> { Key.Logo };


        static List<Key> fullKeylist = new List<Key>();
        static List<Key> mainKeylist = new List<Key>();
        static List<Key> WASDKeylist = new List<Key>();
        static List<Key> mainNoWASDKeylist = new List<Key>();
        static List<Key> functionKeylist = new List<Key>();
        static List<Key> homeKeylist = new List<Key>();
        static List<Key> arrowKeylist = new List<Key>();
        static List<Key> numpadKeylist = new List<Key>();
        static List<Key> macroKeylist = new List<Key>();



        static Color[] keyboardColors = new Color[1000];

        void UpdateColor(String RGB, String RGB2, String effect, String device, double percent)
        {
            currentColor = RGB;
            if (device.CompareTo("ALL") == 0)
            {
                foreach (deviceTypes currDevice in Enum.GetValues(typeof(deviceTypes)))
                {
                    UpdateColor(RGB, RGB2, effect, currDevice.ToString(), percent);
                }
            }
            else if (effect.CompareTo(effectTypes.SPECTRUM.ToString()) == 0)
            {
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
                if (device.CompareTo(deviceTypes.MOUSE.ToString()) == 0)
                {
                    //TODO Add user defined direction
                    Mouse.Instance.SetWave(new Corale.Colore.Razer.Mouse.Effects.Wave(Corale.Colore.Razer.Mouse.Effects.Direction.FrontToBack));
                }
                else if (device.CompareTo(deviceTypes.KEYBOARD.ToString()) == 0)
                {
                    //TODO Add user defined direction
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
                Color RGBColor2 = new Color(0,0,0);

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
                currentColor += ":" + RGB2;

                if (device.CompareTo(deviceTypes.MOUSE.ToString()) == 0)
                {
                    //Uses a global array of LED colors. Since each side can be targeted independently it just updates its part of the array.
                    //Supports targeting the all LEDs, Left side, Right side, or just the logo backlight and scrollwheel

                    if (!dynamicMouseStrip)
                    {
                        Mouse.Instance.SetStatic(new Corale.Colore.Razer.Mouse.Effects.Static(Led.All, blendedColor));
                    }
                    else
                    {

                        if (mouseTarget == null || mouseTarget.CompareTo("ALL") == 0 || mouseTarget == "")
                        {
                            //+1 is so that we use the count of LEDs
                            int midPoint = ((int)Led.Strip14 - (int)Led.Strip1 + 1) / 2;

                            currMouseColor[(int)Led.Backlight] = blendedColor;
                            currMouseColor[(int)Led.Logo] = blendedColor;
                            currMouseColor[(int)Led.ScrollWheel] = blendedColor;


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
                                        currMouseColor[i] = blendedColor;
                                    }
                                    else
                                    {
                                        currMouseColor[i] = new Color(0, 0, 0);
                                    }
                                }
                                //Go through  right side
                                else
                                {
                                    //Plus one so the top light is not missed
                                    //Minus one so that it goes from bottom to top and not top to bottom
                                    if (1 - (double)(i - (int)Led.Strip1 - midPoint + 1) / midPoint < percent)
                                    {
                                        currMouseColor[i] = blendedColor;
                                    }
                                    else
                                    {
                                        currMouseColor[i] = new Color(0, 0, 0);
                                    }
                                }
                            }
                        }
                        else if (mouseTarget.CompareTo("LEFT") == 0)
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
                                        currMouseColor[i] = blendedColor;
                                    }
                                    else
                                    {
                                        currMouseColor[i] = new Color(0, 0, 0);
                                    }
                                }
                            }
                        }
                        else if (mouseTarget.CompareTo("RIGHT") == 0)
                        {
                            //+1 is so that we use the count of LEDs
                            int midPoint = ((int)Led.Strip14 - (int)Led.Strip1 + 1) / 2;


                            for (int i = (int)Led.Strip1; i <= (int)Led.Strip14; i++)
                            {
                                //LED Order from bottom to top is 8 through 1 on the left and 9 through 14 on the right

                                //Go through right side
                                if (i >= midPoint + (int)Led.Strip1)
                                { 
                                    //Plus one so the top light is not missed
                                    //Minus one so that it goes from bottom to top and not top to bottom
                                    if (1 - (double)(i - (int)Led.Strip1 - midPoint + 1) / midPoint < percent)
                                    {
                                        currMouseColor[i] = blendedColor;
                                    }
                                    else
                                    {
                                        currMouseColor[i] = new Color(0, 0, 0);
                                    }
                                }
                            }
                        }
                    else if(mouseTarget.CompareTo("EXTRA") == 0)
                    {
                        currMouseColor[(int)Led.Backlight] = blendedColor;
                        currMouseColor[(int)Led.Logo] = blendedColor;
                        currMouseColor[(int)Led.ScrollWheel] = blendedColor;
                    }
                        Corale.Colore.Razer.Mouse.Effects.Custom gradient = new Corale.Colore.Razer.Mouse.Effects.Custom(currMouseColor);
                        Mouse.Instance.SetCustom(gradient);
                    }

                }
                else if (device.CompareTo(deviceTypes.HEADSET.ToString()) == 0)
                {
                    Headset.Instance.SetStatic(new Corale.Colore.Razer.Headset.Effects.Static(blendedColor));
                }
                else if (device.CompareTo(deviceTypes.KEYBOARD.ToString()) == 0)
                {
                    if (keyboardTarget == null || keyboardTarget == "" || keyboardTarget.CompareTo("ALL") == 0)
                    {
                        Keyboard.Instance.SetStatic(new Corale.Colore.Razer.Keyboard.Effects.Static(blendedColor));
                    }
                    else if (keyboardTarget.CompareTo(keyboardGroups.WASD.ToString()) == 0)
                    {
                        Keyboard.Instance.SetKeys(wasdKeys, blendedColor);
                    }
                }
            }
            else if (RGB != null && RGB != "")
            {
                Color RGBColor, RGBColor2 = new Color();

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
                    if (device.CompareTo(deviceTypes.MOUSE.ToString()) == 0)
                    {
                        Mouse.Instance.SetStatic(new Corale.Colore.Razer.Mouse.Effects.Static(Led.All, RGBColor));
                    }
                    else if (device.CompareTo(deviceTypes.HEADSET.ToString()) == 0)
                    {
                        Headset.Instance.SetStatic(new Corale.Colore.Razer.Headset.Effects.Static(RGBColor));
                    }
                    else if (device.CompareTo(deviceTypes.KEYBOARD.ToString()) == 0)
                    {
                        Keyboard.Instance.SetStatic(new Corale.Colore.Razer.Keyboard.Effects.Static(RGBColor));
                    }
                }
                else if (effect.CompareTo(effectTypes.BREATHING.ToString()) == 0)
                {
                    if (device.CompareTo(deviceTypes.MOUSE.ToString()) == 0)
                    {
                        if (RGB2 == null || RGB2 == "")
                        {
                            Mouse.Instance.SetBreathing(new Corale.Colore.Razer.Mouse.Effects.Breathing(Led.All, RGBColor));
                        }
                        else
                        {
                            currentColor += ":" + RGB2;
                            if (device.CompareTo(deviceTypes.MOUSE.ToString()) == 0)
                            {
                                Mouse.Instance.SetBreathing(new Corale.Colore.Razer.Mouse.Effects.Breathing(Led.All, RGBColor, RGBColor2));
                            }
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
                            currentColor += ":" + RGB2;
                            if (device.CompareTo(deviceTypes.MOUSE.ToString()) == 0)
                            {
                                Keyboard.Instance.SetBreathing(new Corale.Colore.Razer.Keyboard.Effects.Breathing(RGBColor, RGBColor2));
                            }
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
                    if (device.CompareTo(deviceTypes.MOUSE.ToString()) == 0)
                    {
                        //TODO Add user defined duration
                        Mouse.Instance.SetReactive(new Corale.Colore.Razer.Mouse.Effects.Reactive(Led.All, Corale.Colore.Razer.Mouse.Effects.Duration.Long, RGBColor));
                    }
                    else if (device.CompareTo(deviceTypes.KEYBOARD.ToString()) == 0)
                    {
                        //TODO Add user defined duration
                        Keyboard.Instance.SetReactive(new Corale.Colore.Razer.Keyboard.Effects.Reactive(RGBColor, Corale.Colore.Razer.Keyboard.Effects.Duration.Long));
                    }
                }
            }
        }

        internal Measure()
        {
            Chroma.Instance.Initialize();


            fullKeylist.AddRange(numberKeys);
            fullKeylist.AddRange(qwertyKeys);
            fullKeylist.AddRange(asdfKeys);
            fullKeylist.AddRange(zxcvKeys);
            fullKeylist.AddRange(ctrlKeys);
            fullKeylist.AddRange(fnKeys);
            fullKeylist.AddRange(escapeKey);
            fullKeylist.AddRange(sysKeys);
            fullKeylist.AddRange(macroKeys);
            fullKeylist.AddRange(manipKeys);
            fullKeylist.AddRange(arrowKeys);
            fullKeylist.AddRange(numpadKeys);
            fullKeylist.AddRange(logoKey);

            mainKeylist.AddRange(numberKeys);
            mainKeylist.AddRange(qwertyKeys);
            mainKeylist.AddRange(asdfKeys);
            mainKeylist.AddRange(zxcvKeys);
            mainKeylist.AddRange(ctrlKeys);

            mainNoWASDKeylist.AddRange(numberKeys);
            mainNoWASDKeylist.AddRange(qwertyNoWASDKeys);
            mainNoWASDKeylist.AddRange(asdfNoWASDKeys);
            mainNoWASDKeylist.AddRange(zxcvKeys);
            mainNoWASDKeylist.AddRange(ctrlKeys);

            WASDKeylist.AddRange(wasdKeys);

            functionKeylist.AddRange(fnKeys);
            functionKeylist.AddRange(escapeKey);
            functionKeylist.AddRange(sysKeys);

            homeKeylist.AddRange(manipKeys);

            arrowKeylist.AddRange(arrowKeys);

            numpadKeylist.AddRange(numpadKeys);

            macroKeylist.AddRange(macroKeys);

            Chroma.Instance.ApplicationState += (args, args2) => {
                API.Log(API.LogType.Notice, "args:" + args.ToString());
                API.Log(API.LogType.Notice, "args2:" + args2.ToString());
            };
        }

        internal void Reload(Rainmeter.API api, ref double maxValue)
        {
            String RGB = api.ReadString("Color", null);
            String RGB2 = api.ReadString("Color2", null);

            String effect = api.ReadString("Effect", "static").ToUpper();
            device = api.ReadString("Device", "all").ToUpper();

            String percentString = api.ReadString("Percent", null);
            percentMax = api.ReadString("PercentMax", "100");
            percentMin = api.ReadString("PercentMin", "0");

            dynamicMouseStrip = Convert.ToBoolean(Convert.ToInt16(api.ReadString("MouseStripsUsePercent", "0")));
            mouseTarget = api.ReadString("MouseTarget", "all").ToUpper();

            keyboardTarget = api.ReadString("KeyboardTarget", "all").ToUpper();

            double percent = 0.0;

            if(percentString != null && percentString !="")
            {
                percent = (Convert.ToDouble(percentString) - Convert.ToDouble(percentMin)) / (Convert.ToDouble(percentMax) - Convert.ToDouble(percentMin));

                if (double.IsInfinity(percent))
                {
                    percent = 100;
                }
            }
            
            //Check if anything has changed since last update
            if (lastUpdate != RGB + RGB2 + effect + device + percent)
            {
                UpdateColor(RGB, RGB2, effect, device, percent);
                lastUpdate = RGB + RGB2 + effect + device + percent;
            }
        }

        internal double Update()
        {
            //API.Log(API.LogType.Notice, Corale.Colore.Core.Chroma.Instance.ApplicationState());
            //API.Log(API.LogType.Notice, Corale.Colore.Events.ApplicationStateEventArgs);
            //API.Log(API.LogType.Notice, "State:" + Corale.Colore.Core.Chroma.Instance.Initialized.ToString());

            return 0.0;
        }

        internal string GetString()
        {
            return currentColor;
        }

        internal void ExecuteBang(string args)
        {
            String[] argArr = args.Split( new char[] { ' ', ':' } );

            String effect = argArr[0].ToUpper();

            //TODO add support for percent min and max on gradient bangs
            if (effect.CompareTo(effectTypes.GRADIENT.ToString()) == 0)
            {
                double percent = Convert.ToDouble(argArr[1]);
                percent = (percent - Convert.ToDouble(percentMin)) / (Convert.ToDouble(percentMax) - Convert.ToDouble(percentMin));

                if(double.IsInfinity(percent))
                {
                    percent = 100;
                }

                String RGB = argArr[2].ToUpper();
                String RGB2 = null;

                if (argArr.Length >= 4)
                {
                    RGB2 = argArr[3].ToUpper();
                }

                if (lastUpdate != RGB + RGB2 + effect + device + percent)
                {
                    UpdateColor(RGB, RGB2, effect, device, percent);
                    lastUpdate = RGB + RGB2 + effect + device + percent;
                }
            }
            else
            {
                String RGB = argArr[1].ToUpper();
                String RGB2 = null;

                if (argArr.Length >= 3)
                {
                    RGB2 = argArr[2].ToUpper();
                }
                double percent = 0.0;

                if (lastUpdate != RGB + RGB2 + effect + device + percent)
                {
                    UpdateColor(RGB, RGB2, effect, device, percent);
                    lastUpdate = RGB + RGB2 + effect + device + percent;
                }
            }
        }
    }

    public static class Plugin
    {
        static IntPtr StringBuffer = IntPtr.Zero;

        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure()));
        }

        [DllExport]
        public static void Finalize(IntPtr data)
        {
            Chroma.Instance.Uninitialize();

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
