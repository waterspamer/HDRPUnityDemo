using UnityEngine;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Nettle {

public class Encoder_A32B32G32R32_Float : Encoder {

	public override Color[] Encode(byte[] In){
		
		Color[] Result = new Color[In.Length/16];
		GCHandle Handle = GCHandle.Alloc(In,GCHandleType.Pinned);
		
		IntPtr InPixel = Handle.AddrOfPinnedObject();
		float[] Pixel = new float[4];
		for (int i=0; i<Result.Length; i++){
			
			Marshal.Copy(InPixel,Pixel,0,4);
			
			float Max = Mathf.Max(Mathf.Max(Pixel[0],Pixel[1]),Pixel[2]);
			float Divider = 1f;
			if (Max>1f){
				Divider = 1f/Max;
			}
			
			Result[i] = new Color(Pixel[0]*Divider,Pixel[1]*Divider,Pixel[2]*Divider,Divider);
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
