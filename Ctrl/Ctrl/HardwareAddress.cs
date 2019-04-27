using System;
using System.Linq;

public class HardwareAddress
{
    public byte[] bytes;

    public HardwareAddress(string macAddress)
    {
        if (macAddress == null)
            throw new NullReferenceException("macAddress");

        this.bytes = macAddress.Split(new char[] { ':', '-' }).Select(x => (byte)int.Parse((string) x, System.Globalization.NumberStyles.AllowHexSpecifier)).ToArray();
        if (this.bytes.Length != 6)
            throw new FormatException("macAddress");
    }

    public override string ToString()
    {
        return string.Join(':', this.bytes.Select(x => x.ToString("X2")));
    }
}