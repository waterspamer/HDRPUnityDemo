using UnityEngine;
using System;
using System.IO;

namespace Nettle {
public class Encoder_Generic_Uint : Encoder {

	public override Color[] Encode(byte[] In){
		throw new Exception ("Obsolete method Encode! Do not use it!");
	}
	
	public override int DataSize(int width, int height){
		return -1;
	}

	public int GetMaxBitPosition(int value){
		for (int i = (8*4-1); i <= 0; --i) {
			if((value & (1 << i)) != 0){
				return i;
			}
		}
		return 0;
	}

	public int GetPixelShift(int mask){
		var maxBit = GetMaxBitPosition (mask);
		if (maxBit <= 7) {
			return 0;
		}
		return maxBit - 7;
	}

	public override Color[] ReadPixels(BinaryReader source, DDSImageInfo info, int pixelWidth, int pixelHeight){
		var result = new Color[pixelWidth * pixelHeight];
		var fmt = info.Header.ImageFormat;
		byte[] pixelBuffer = new byte[4];

		int[] rgbaMasks = new [] { fmt.DwRBitMask, fmt.DwGBitMask, fmt.DwBBitMask, fmt.DwABitMask };
		int[] rgbaShift = new [] { 
			GetPixelShift (fmt.DwRBitMask),  
			GetPixelShift (fmt.DwGBitMask),
			GetPixelShift (fmt.DwBBitMask),
			GetPixelShift (fmt.DwABitMask)};

		int pixelBytesCount = (fmt.DwRGBBitCount + 7) / 8;

		for (int i = 0; i < result.Length; ++i) {
			//read pixel bytes
			source.Read(pixelBuffer, 0, pixelBytesCount);
			var pixel = BitConverter.ToUInt32(pixelBuffer, 0);

			int x = i % pixelWidth;
			int y = i / pixelWidth;

			int flipY = pixelHeight - 1 - y;

			//assign channels
			for(int j = 0; j < 4; ++j){
				result[flipY * pixelWidth + x][j] = (float)((pixel & rgbaMasks[j]) >> rgbaShift[j]) / 255.0f;
			}
		}
		return result;
	}
	
	public static DX9SurfaceFormat GetSurfaceFormatDX9(DDSImageInfo info){
		switch (info.Header.ImageFormat.DwRGBBitCount) {
		case 8:
			return DX9SurfaceFormat.D3DFMT_A8;
		case 24:
			return DX9SurfaceFormat.D3DFMT_R8G8B8;
		case 32:
			return DX9SurfaceFormat.D3DFMT_A8R8G8B8;
		case 16:
		default:
			return DX9SurfaceFormat.D3DFMT_UNKNOWN;
		}
	}

}







}
