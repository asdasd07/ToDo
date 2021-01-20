using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ToDo {
    static class Tools {
        public static Brush getTaskBrush(int groupId, int taskId = 0) {
            int hue = groupId * 100 % 360 + taskId * 29 % 40;
            double value = ((double)(taskId * 17 % 40))/100.0+0.5;
            return BrushFromHSV(hue, value, value, 0.5);
        }
        /// <param name="hue">0-360</param>
        /// <param name="saturation">0-1</param>
        /// <param name="value">0-1</param>
        /// <returns></returns>
        static Brush BrushFromHSV(double hue, double saturation, double value, double alfa) {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value *= 255;
            Byte v = Convert.ToByte(value);
            Byte p = Convert.ToByte(value * (1 - saturation));
            Byte q = Convert.ToByte(value * (1 - f * saturation));
            Byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));
            alfa *= 255;
            Byte a = Convert.ToByte(alfa);

            Color color;
            if (hi == 0)
                color = Color.FromArgb(a, v, t, p);
            else if (hi == 1)
                color = Color.FromArgb(a, q, v, p);
            else if (hi == 2)
                color = Color.FromArgb(a, p, v, t);
            else if (hi == 3)
                color = Color.FromArgb(a, p, q, v);
            else if (hi == 4)
                color = Color.FromArgb(a, t, p, v);
            else
                color = Color.FromArgb(a, v, p, q);

            return new SolidColorBrush(color);
        }
    }
}
