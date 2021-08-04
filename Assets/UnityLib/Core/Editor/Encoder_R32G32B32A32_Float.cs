using UnityEngine;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Nettle {

public class Encoder_R32G32B32A32_Float : Encoder {
	
	public override Color[] Encode(byte[] In){
		
		Color[] Result = new Color[In.Length/16];
		GCHandle Handle = GCHandle.Alloc(In,GCHandleType.Pinned);
		
		IntPtr InPixel = Handle.AddrOfPinnedObject();
		float[] Pixel = new float[4];
		for (int i=0; i<Result.Length; i++){
			
			Marshal.Copy(InPixel,Pixel,0,4);
			Result[i] = new Color(Pixel[0],Pixel[1],Pixel[2],Pixel[3]);
			InPixel = new IntPtr(InPixel.ToInt64() + 16);
		}
		
		Handle.Free();
		return Result;
	}
	
	public override int DataSize(int width, int height){
		return 16*width*height;
	}
	public override Color[] ReadPixels(BinaryReader source, DDSImageInfo info, int pixelWidth, int pixelHeight){
		throw new Exception("Not implemented");
	}

}
}
