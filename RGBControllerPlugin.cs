using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Rainmeter;

using Corale.Colore.Core;

using Corale.Colore.Razer.Mouse;
using Corale.Colore.Razer.Mouse.Effects;

using Corale.Colore.Razer.Headset;
using Corale.Colore.Razer.Headset.Effects;

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
            REACTIVE
        }
        enum deviceTypes
        {
            MOUSE,
            HEADSET
        }

        String currentColor = "";

        internal Measure()
        {
        }

        internal void Reload(Rainmeter.API api, ref double maxValue)
        {
            String RGB = api.ReadString("Color", null);
            String RGB2 = api.ReadString("Color2", null);

            String effect = api.ReadString("Effect", "static").ToUpper();
            String device = api.ReadString("Device", "all").ToUpper();

            currentColor = RGB;

            if (RGB != null && RGB != "")
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
                    else if(device.CompareTo(deviceTypes.HEADSET.ToString()) == 0)
                    {
                        Headset.Instance.SetStatic(new Corale.Colore.Razer.Headset.Effects.Static(RGBColor));
                    }
                }
                else if (effect.CompareTo(effectTypes.BREATHING.ToString()) == 0)
                {
                    if (device.CompareTo(deviceTypes.MOUSE.ToString()) == 0)
                    {
                        if (RGB2 == null)
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
                        Mouse.Instance.SetReactive(new Reactive(Led.All, Duration.Long, RGBColor));
                    }
                }
                else if (effect.CompareTo(effectTypes.WAVE.ToString()) == 0)
                {
                    if (device.CompareTo(deviceTypes.MOUSE.ToString()) == 0)
                    {
                        //TODO Add user defined direction
                        Mouse.Instance.SetWave(new Wave(Direction.FrontToBack));
                    }
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
            }
        }

        internal double Update()
        {
            return 0.0;
        }

        internal string GetString()
        {
            return Color.Blue.ToString();
        }

        internal void ExecuteBang(string args)
        {
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
