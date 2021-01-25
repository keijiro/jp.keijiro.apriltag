using System;
using System.Runtime.InteropServices;

namespace AprilTag {

[StructLayoutAttribute(LayoutKind.Sequential)]
struct Detection
{
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
}

} // namespace AprilTag
