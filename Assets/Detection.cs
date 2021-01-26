using System;
using System.Runtime.InteropServices;

namespace AprilTag {

[StructLayoutAttribute(LayoutKind.Sequential)]
public struct Detection
{
    #region Internal data structure

    IntPtr family;
    int id;
    int hamming;
    float decision_margin;
    IntPtr H;
    double c0, c1;
    double p00, p01;
    double p10, p11;
    double p20, p21;
    double p30, p31;

    #endregion

    #region Public accessors

    public int ID => id;
    public int Hamming => hamming;
    public float DecisionMargin => decision_margin;

    #endregion
}

} // namespace AprilTag
