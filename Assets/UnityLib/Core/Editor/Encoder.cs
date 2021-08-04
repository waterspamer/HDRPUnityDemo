using UnityEngine;
using System.IO;

namespace Nettle {

public abstract class Encoder {

	public abstract Color[] Encode(byte[] In);
	public abstract Color[] ReadPixels(BinaryReader source, DDSImageInfo info, int pixelWidth, int pixelHeight);
	public abstract int DataSize(int width, int height);
}
}
