using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public class Color
    {
        public float A = 1;
        public float R = 1;
        public float G = 1;
        public float B = 1;

        public bool Use = true;

        public event EventHandler OnChangeEvent;
        public Color() { }

        public Color(float r, float g, float b, float a) 
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public Color(Vector4 color) 
        {
            A = color.W;
            R = color.X;
            G = color.Y;
            B = color.Z;
        }

        public Color(Color color)
        {
            A = color.A;
            R = color.R;
            G = color.G;
            B = color.B;
        }

        public static Color operator -(Color color1, Color color2) 
        {
            Color newCol = new Color();
            newCol.A = color1.A - color2.A;
            newCol.R = color1.R - color2.R;
            newCol.G = color1.G - color2.G;
            newCol.B = color1.B - color2.B;

            return newCol;
        }

        public static Color operator +(Color color1, Color color2)
        {
            Color newCol = new Color();
            newCol.A = color1.A + color2.A;
            newCol.R = color1.R + color2.R;
            newCol.G = color1.G + color2.G;
            newCol.B = color1.B + color2.B;

            return newCol;
        }

        public static Color operator /(Color color, float num) 
        {
            Color newCol = new Color();
            newCol.A = color.A / num;
            newCol.R = color.R / num;
            newCol.G = color.G / num;
            newCol.B = color.B / num;

            return newCol;
        }

        public static Color operator *(Color color, float num)
        {
            Color newCol = new Color();
            newCol.A = color.A * num;
            newCol.R = color.R * num;
            newCol.G = color.G * num;
            newCol.B = color.B * num;

            return newCol;
        }

        public Color Add(Color color) 
        {
            A += color.A;
            R += color.R;
            G += color.G;
            B += color.B;

            _onChange();



            return this;
        }

        public Color Sub(Color color)
        {
            A -= color.A;
            R -= color.R;
            G -= color.G;
            B -= color.B;

            _onChange();
            return this;
        }

        public void _onChange() 
        {
            OnChangeEvent?.Invoke(this, EventArgs.Empty);
        }

        public Vector4 ToVector() 
        {
            return new Vector4(R, G, B, A);
        }
    }
}
