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

        String currentColor = "";
        String lastUpdate = "";

        String device = "";
        String percentMax = "100";
        String percentMin = "0";


        Boolean dynamicMouseStrip = false;
        String mouseTarget = "all";

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
                Color RGBColor, RGBColor2 = new Color(0,0,0);

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
                    //TODO add option to also scale leds that are targeted based on percent
                    //SetStaic seems to not work on an idividual LED basis and just updates the whole mouse so I am using SetLED
                    if (!dynamicMouseStrip)
                    {
                        Mouse.Instance.SetStatic(new Corale.Colore.Razer.Mouse.Effects.Static(Led.All, blendedColor));
                    }
                    else
                    {
                        if (mouseTarget == null || mouseTarget.ToUpper().CompareTo("ALL") == 0 || mouseTarget == "")
                        {
                            //+1 is so that we use the count of LEDs
                            int midPoint = ((int)Led.Strip14 - (int)Led.Strip1 + 1) / 2;


                            Mouse.Instance.SetLed(Led.Backlight, blendedColor);
                            Mouse.Instance.SetLed(Led.Logo, blendedColor);
                            Mouse.Instance.SetLed(Led.ScrollWheel, blendedColor);

                            for (int i = (int)Led.Strip1; i <= (int)Led.Strip14; i++)
                            {
                                //LED Order from bottom to top is 8 through 1 on the left and 9 through 14 on the right

                                //Go through left side
                                if (i < midPoint + (int)Led.Strip1)
                                {
                                    if ((double)(i - (int)Led.Strip1) / midPoint < percent)
                                    {
                                        Mouse.Instance.SetLed((Led)i, blendedColor, false);
                                    }
                                    else
                                    {
                                        Mouse.Instance.SetLed((Led)i, new Color(0, 0, 0), false);
                                    }
                                }
                                //Go through  right side
                                else
                                {
                                    if ((double)(i - (int)Led.Strip1 - midPoint) / midPoint < percent)
                                    {
                                        Mouse.Instance.SetLed((Led)i, blendedColor, false);
                                    }
                                    else
                                    {
                                        Mouse.Instance.SetLed((Led)i, new Color(0, 0, 0), false);
                                    }
                                }
                            }
                        }
                    }

                }
                else if (device.CompareTo(deviceTypes.HEADSET.ToString()) == 0)
                {
                    Headset.Instance.SetStatic(new Corale.Colore.Razer.Headset.Effects.Static(blendedColor));
                }
                else if (device.CompareTo(deviceTypes.KEYBOARD.ToString()) == 0)
                {
                    Keyboard.Instance.SetStatic(new Corale.Colore.Razer.Keyboard.Effects.Static(blendedColor));
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

            double percent = 0.0;

            if(percentString != null && percentString !="")
            {
                percent = (Convert.ToDouble(percentString) - Convert.ToDouble(percentMin)) / (Convert.ToDouble(percentMax) - Convert.ToDouble(percentMin));
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
            Mouse.Instance.Clear();
            Headset.Instance.Clear();
            Keyboard.Instance.Clear();

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
