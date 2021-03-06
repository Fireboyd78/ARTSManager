﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARTSManager
{
    public struct Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public void CopyTo(asStream stream)
        {
            stream.Put(X);
            stream.Put(Y);
        }

        public Vector2(asStream stream)
        {
            X = stream.GetFloat();
            Y = stream.GetFloat();
        }
    }

    public struct Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public void CopyTo(asStream stream)
        {
            stream.Put(X);
            stream.Put(Y);
            stream.Put(Z);
        }

        public Vector3(asStream stream)
        {
            X = stream.GetFloat();
            Y = stream.GetFloat();
            Z = stream.GetFloat();
        }
    }

    public struct ColorRGB
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }

        public int iR
        {
            get { return (int)(R * 255.999f); }
            set { R = (value / 255.0f); }
        }

        public int iG
        {
            get { return (int)(G * 255.999f); }
            set { G = (value / 255.0f); }
        }

        public int iB
        {
            get { return (int)(B * 255.999f); }
            set { B = (value / 255.0f); }
        }

        public void CopyTo(asStream stream)
        {
            stream.Put(R);
            stream.Put(G);
            stream.Put(B);
        }

        public ColorRGB(asStream stream)
        {
            R = stream.GetFloat();
            G = stream.GetFloat();
            B = stream.GetFloat();
        }
    }

    public struct ColorRGBA
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }

        public int iR
        {
            get { return (int)(R * 255.999f); }
            set { R = (value / 255.0f); }
        }

        public int iG
        {
            get { return (int)(G * 255.999f); }
            set { G = (value / 255.0f); }
        }

        public int iB
        {
            get { return (int)(B * 255.999f); }
            set { B = (value / 255.0f); }
        }

        public int iA
        {
            get { return (int)(A * 255.999f); }
            set { A = (value / 255.0f); }
        }

        public void CopyTo(asStream stream)
        {
            stream.Put(R);
            stream.Put(G);
            stream.Put(B);
            stream.Put(A);
        }

        public ColorRGBA(asStream stream)
        {
            R = stream.GetFloat();
            G = stream.GetFloat();
            B = stream.GetFloat();
            A = stream.GetFloat();
        }
    }
}
