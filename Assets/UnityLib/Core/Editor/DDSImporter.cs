using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace Nettle {


public class DDSImporter {
	private static string currentDirectory = "";

	[MenuItem ("DDS/Import")]
	public static void Execute(){
		string FileName = EditorUtility.OpenFilePanel("Select *.dds file",currentDirectory,"dds");
		
		if (FileName=="") return;
		currentDirectory = Path.GetDirectoryName(FileName);

		Texture TheTexture = new DDSImporter().Load(FileName);
		TheTexture.filterMode = FilterMode.Trilinear;
		TheTexture.anisoLevel = 1;
		TheTexture.hideFlags = HideFlags.NotEditable;

		string OutputFileName = EditorUtility.SaveFilePanelInProject("Save texture",Path.GetFileNameWithoutExtension(FileName),"asset","msg");
		AssetDatabase.CreateAsset(TheTexture,OutputFileName);
		Debug.Log(OutputFileName);
	}
	
	public Encoder encoder;
    public Texture Load(string FileName) {

        Stream stream = new FileStream(FileName,FileMode.Open);
        BinaryReader input = new BinaryReader(stream);
        DDSImageInfo image = ReadHeaderInfo(input);
		string Name = Path.GetFileNameWithoutExtension(FileName);

        encoder = SelectEncoder(image);
		if (encoder==null){
			Debug.LogError("Unsupported texture format.");
			return null;
		}
        TextureDimensions dimensions = DetermineTextureDimensions(image);

        switch(dimensions) {
            case TextureDimensions.Two:
                Texture2D tex2d = LoadTexture2D(image, input);
                tex2d.name = Name;
                input.Close();
                return tex2d;
            case TextureDimensions.Three:
                Texture3D tex3d = LoadTexture3D(image, input);
                tex3d.name = Name;
                input.Close();
                return tex3d;
            case TextureDimensions.Cube:
                Cubemap texcube = LoadTextureCube(image, input);
                texcube.name = Name;
                input.Close();
                return texcube;
            default:
                input.Close();
                Debug.LogError("Error loading DDS file");
				return null;
        }
    }

    /*private static int CalculateMipMapSize(bool compressed, int width, int height, int bpp, int level) {
        int size = 0;
        if(compressed) {
            size = ((width + 3) / 4) * ((height + 3) / 4) * bpp;
        } else {
            size = width * height;
        }
        size = ((size + 3) / 4) * 4;
        return size;
    }*/

	TextureFormat GetClosestUnityFormat(DDSImageInfo image){
		if (image.Header10.Exists){
			switch (image.Header10.Format){
			default: 
				return TextureFormat.BGRA32;
			}
		}else{
			var format = (DX9SurfaceFormat)image.Header.ImageFormat.DwFourCC;
			if(format == DX9SurfaceFormat.D3DFMT_UNKNOWN){
				format = Encoder_Generic_Uint.GetSurfaceFormatDX9(image);
			}
			switch (format){
			case DX9SurfaceFormat.D3DFMT_A8:
				return TextureFormat.Alpha8;
			case DX9SurfaceFormat.D3DFMT_R8G8B8:
				return TextureFormat.RGB24;
			case DX9SurfaceFormat.D3DFMT_A8R8G8B8:
				return TextureFormat.RGBA32;
			case DX9SurfaceFormat.D3DFMT_UNKNOWN: 
			default:
				return TextureFormat.BGRA32;
			}
		}
	}

    private Texture2D LoadTexture2D(DDSImageInfo image, BinaryReader input) {
        DDSHeader header = image.Header;

        int width = header.DwWidth;
        int height = header.DwHeight;
        //int flags = header.ImageFormat.DwFlags;
        //bool compressedFormat = IsSet(flags, (int) DDSPixelFlags.FourCC);
        //int[] mipMapSizes = new int[header.DwMipMapCount];
		var fmt = GetClosestUnityFormat (image);
		Texture2D tex = new Texture2D(width, height, fmt, header.DwMipMapCount > 1);


        for(int m = 0; m < header.DwMipMapCount; m++) {

			//byte[] data = input.ReadBytes(encoder.DataSize(width, height));

			tex.SetPixels(encoder.ReadPixels(input, image, width, height), m);
            width = System.Math.Max(width >> 1, 1);
            height = System.Math.Max(height >> 1, 1);
        }

        return tex;
		
    }

    private Texture3D LoadTexture3D(DDSImageInfo image, BinaryReader input) {
        return null;
    }




    private Cubemap LoadTextureCube(DDSImageInfo image, BinaryReader input) {
        DDSHeader header = image.Header;

        int width = header.DwWidth;
        int height = header.DwHeight;
        //int flags = header.ImageFormat.DwFlags;

        //bool compressedFormat = IsSet(flags, (int) DDSPixelFlags.FourCC);
        //int[] mipMapSizes = new int[header.DwMipMapCount];
        Cubemap tex = new Cubemap(width, TextureFormat.ARGB32, header.DwMipMapCount > 1);

        for(int face = 0; face < 6; face++) {
			for(int m = 0; m < header.DwMipMapCount; m++) {
                byte[] data = input.ReadBytes(encoder.DataSize(width, height));
				
				tex.SetPixels(encoder.Encode(data),(CubemapFace)face,m);

                width = System.Math.Max(width / 2, 1);
                height = System.Math.Max(height / 2, 1);
            }

            width = header.DwWidth;
            height = header.DwHeight;
        }

        return tex;
    }

    private static TextureDimensions DetermineTextureDimensions(DDSImageInfo image) {
        DDSHeader header = image.Header;
        //DDSHeader10 header10 = image.Header10;

        if(IsSet(header.DwCaps2, (int) DDSCaps2.CubeMap)) {
            return TextureDimensions.Cube;
        } else if(IsSet(header.DwCaps2, (int) DDSCaps2.Volume)) {
            return TextureDimensions.Three;
        } else {
            return TextureDimensions.Two;
        }
    }


	




    private static Encoder SelectEncoder(DDSImageInfo image) {
        DDSHeader header = image.Header;
        DDSHeader10 header10 = image.Header10;

        //int flags = header.ImageFormat.DwFlags;
        //bool compressedFormat = IsSet(flags, (int) DDSPixelFlags.FourCC);
        //bool rgb = IsSet(flags, (int) DDSPixelFlags.RGB);
        //bool alphaPixels = IsSet(flags, (int) DDSPixelFlags.AlphaPixels);
        //bool alpha = IsSet(flags, (int) DDSPixelFlags.Alpha);
        //bool luminance = IsSet(flags, (int) DDSPixelFlags.Luminance);
	
		if (header10.Exists){
			switch (header10.Format){
				case DXGIFormat.R32G32B32A32_Float: return new Encoder_R32G32B32A32_Float();
			}
		}else{
			var DX9Format = (DX9SurfaceFormat)header.ImageFormat.DwFourCC;
			switch (DX9Format){
				case DX9SurfaceFormat.D3DFMT_A32B32G32R32F: return new Encoder_A32B32G32R32_Float();

				case DX9SurfaceFormat.D3DFMT_X8B8G8R8:
				case DX9SurfaceFormat.D3DFMT_X8R8G8B8:
				case DX9SurfaceFormat.D3DFMT_A8R8G8B8:
				case DX9SurfaceFormat.D3DFMT_A8B8G8R8:
				case DX9SurfaceFormat.D3DFMT_R8G8B8:
				case DX9SurfaceFormat.D3DFMT_UNKNOWN: return new Encoder_Generic_Uint();
			}
		}
	
	
	
        /*if(compressedFormat) {
            int fourCC = header.ImageFormat.DwFourCC;
            if(fourCC == GetInt("DXT1")) {
                return TextureFormat.DXT1;
            /*} else if(fourCC == GetInt("DXT3")) {
                return TextureFormat.DXT3;
            } else if(fourCC == GetInt("DXT5")) {
                return TextureFormat.DXT5;
            } 
        } else {
            FoufCC DX9Format = (FoufCC)
		
		
			if(rgb) {
                int bpp = header.ImageFormat.DwRGBBitCount;
                switch(bpp) {
                    case 32:
                        return TextureFormat.ARGB32;
                }
            } else if(alphaPixels && alpha && (header.ImageFormat.DwRGBBitCount == 8)) {
                return TextureFormat.Alpha8;
            } 
        }*/

		return null;
        //throw new InvalidDataException("Unsupported DDS format.");
    }

    private static bool IsSet(int value, int mask) {
        if((value & mask) == mask) {
            return true;
        } else {
            return false;
        }
    }

    private static int GetInt(string s) {
        char[] chars = s.ToCharArray();
        byte[] bytes = new byte[chars.Length];
        for(int i = 0; i < chars.Length; i++) {
            bytes[i] = Convert.ToByte(chars[i]);
        }
        return GetInt(bytes);
    }

    private static int GetInt(byte[] bytes) {
        int value = 0;
        value |= (bytes[0] & 0xff) << 0;
        if(bytes.Length > 1) {
            value |= (bytes[1] & 0xff) << 8;
        }
        if(bytes.Length > 2) {
            value |= (bytes[2] & 0xff) << 16;
        }
        if(bytes.Length > 3) {
            value |= (bytes[3] & 0xff) << 24;
        }
        return value;
    }

    private static DDSImageInfo ReadHeaderInfo(BinaryReader input) {
        int dwMagic = input.ReadInt32();
        if(dwMagic != GetInt("DDS ")) {
            throw new InvalidOperationException("Not a DDS file");
        }

        DDSImageInfo image = new DDSImageInfo();

        image.Header = ReadHeader(input);
        if(image.Header.ImageFormat.DwFourCC == GetInt("DX10")) {
            image.Header10 = ReadHeader10(input);
        } else {
            image.Header10.Exists = false;
        }
        return image;
    }

    private static DDSHeader ReadHeader(BinaryReader input) {
        DDSHeader header = new DDSHeader();
        header.DwSize = input.ReadInt32();
        if(header.DwSize != 124) {
            throw new InvalidOperationException("Invalid dds header size.");
        }
        header.DwFlags = input.ReadInt32();
        header.DwHeight = input.ReadInt32();
        header.DwWidth = input.ReadInt32();
        header.DwLinearSize = input.ReadInt32();
        header.DwDepth = input.ReadInt32();
        header.DwMipMapCount = input.ReadInt32();
        header.DwAlphaBitDepth = input.ReadInt32();
        header.DwReserved1 = new int[10];
        for(int i = 0; i < 10; i++) {
            header.DwReserved1[i] = input.ReadInt32();
        }
        header.ImageFormat = ReadImageFormat(input);
        header.DwCaps= input.ReadInt32();
        header.DwCaps2 = input.ReadInt32();
        header.DwCaps3 = input.ReadInt32();
        header.DwCaps4 = input.ReadInt32();
        header.DwTextureStage = input.ReadInt32();

        int mipMaps = 1 + (int) System.Math.Ceiling(System.Math.Log(System.Math.Max(header.DwHeight, header.DwWidth)) / System.Math.Log(2));
        DDSCaps cap = (DDSCaps) header.DwCaps;
        if((cap & DDSCaps.MipMap) == DDSCaps.MipMap) {
            DDSFlags flag = (DDSFlags) header.DwFlags;
            if((flag & DDSFlags.MipCount) != DDSFlags.MipCount) {
                header.DwMipMapCount = mipMaps;
            }
        } else {
            header.DwMipMapCount = 1;
        }
        
        return header;
    }

    private static DDSHeader10 ReadHeader10(BinaryReader input) {
        DDSHeader10 header10 = new DDSHeader10();
        header10.Exists = true;
        header10.Format = ReadDXGIFormat(input);
        header10.Dimension = ReadResourceDimension(input);
        header10.MiscFlag = input.ReadInt32();
        header10.ArraySize = input.ReadInt32();
        header10.Reserved = input.ReadInt32();
        return header10;
    }

    private static DXGIFormat ReadDXGIFormat(BinaryReader input) {
        int dxgi = input.ReadInt32();
		return (DXGIFormat)dxgi;
        /*foreach(DXGIFormat format in MemUtils.GetEnumValues(new DXGIFormat())) {
            if(dxgi == (int) format) {
                return format;
            }
        }
        return DXGIFormat.Unknown;*/
    }

    private static D3D10ResourceDimension ReadResourceDimension(BinaryReader input) {
        int dxgi = input.ReadInt32();
		return (D3D10ResourceDimension)dxgi;
        /*foreach(D3D10ResourceDimension format in MemUtils.GetEnumValues(new D3D10ResourceDimension())) {
            if(dxgi == (int) format) {
                return format;
            }
        }
        return D3D10ResourceDimension.Unknown;*/
    }

    private static DDSImageFormat ReadImageFormat(BinaryReader input) {
        DDSImageFormat iformat = new DDSImageFormat();
        iformat.DwSize = input.ReadInt32();
        if(iformat.DwSize != 32) {
            throw new InvalidOperationException("Invalid image format size.");
        }
        iformat.DwFlags = input.ReadInt32();
        iformat.DwFourCC = input.ReadInt32();
        iformat.DwRGBBitCount = input.ReadInt32();
        iformat.DwRBitMask = input.ReadInt32();
        iformat.DwGBitMask = input.ReadInt32();
        iformat.DwBBitMask = input.ReadInt32();
        iformat.DwABitMask = input.ReadInt32();
        return iformat;
    }

}

    public struct DDSHeader {
        public int DwSize;
        public int DwFlags;
        public int DwHeight;
        public int DwWidth;
        public int DwLinearSize;
        public int DwDepth;
        public int DwMipMapCount;
        public int DwAlphaBitDepth;
        public int[] DwReserved1;
        public DDSImageFormat ImageFormat;
        public int DwCaps;
        public int DwCaps2;
        public int DwCaps3;
        public int DwCaps4;
        public int DwTextureStage;
    }

public struct DDSHeader10 {
        public bool Exists;
        public DXGIFormat Format;
        public D3D10ResourceDimension Dimension;
        public int MiscFlag;
        public int ArraySize;
        public int Reserved;
    }

    public struct DDSImageInfo {
        public DDSHeader Header;
        public DDSHeader10 Header10;
    }

    public struct DDSImageFormat {
        public int DwSize;
        public int DwFlags;
        public int DwFourCC;
        public int DwRGBBitCount;
        public int DwRBitMask;
        public int DwGBitMask;
        public int DwBBitMask;
        public int DwABitMask;
    }

public enum D3D10ResourceDimension {
        Unknown = 0,
        Buffer = 1,
        Texture1D = 2,
        Texture2D = 3,
        Texture3D = 4
    }


public enum DDSFlags {
        Caps = 0x1,
        Height = 0x2,
        Width = 0x4,
        Pitch = 0x8,
        Format = 0x1000,
        MipCount = 0x20000,
        LinearSize = 0x80000,
        Depth = 0x800000
    }


public enum DDSPixelFlags {
        AlphaPixels = 0x1,
        Alpha = 0x2,
        FourCC = 0x4,
        RGB = 0x40,
        YUV = 0x200,
        Luminance = 0x20000
    }


public enum DDSCaps {
        Complex = 0x8,
        MipMap = 0x400000,
        Texture = 0x1000
    }

public enum DDSCaps2 {
        CubeMap = 0x200,
        CubeMapPositiveX = 0x400,
        CubeMapNegativeX = 0x800,
        CubeMapPositiveY = 0x1000,
        CubeMapNegativeY = 0x2000,
        CubeMapPositiveZ = 0x4000,
        CubeMapNegativeZ = 0x8000,
        Volume = 0x200000
    }

    [Flags]
public enum D3D10ResourceMisc {
        GenerateMips = 0x1,
        Shared = 0x2,
        TextureCube = 0x4,
        SharedKeyedMutex = 0x10,
        GDICompatible = 0x20
    }


public enum TextureDimensions {
		Two,
		Three,
		Cube
	}

public enum DXGIFormat {
        Unknown = 0,
        R32G32B32A32_Typeless = 1,
        R32G32B32A32_Float = 2,
        R32G32B32A32_Uint = 3,
        R32G32B32A32_Sint = 4,
        R32G32B32_Typeless = 5,
        R32G32B32_Float = 6,
        R32G32B32_Uint = 7,
        R32G32B32_Sint = 8,
        R16G16B16A16_Typeless = 9,
        R16G16B16A16_Float = 10,
        R16G16B16A16_Unorm = 11,
        R16G16B16A16_Uint = 12,
        R16G16B16A16_Snorm = 13,
        R16G16B16A16_Sint = 14,
        R32G32_Typeless = 15,
        R32G32_Float = 16,
        R32G32_Uint = 17,
        R32G32_Sint = 18,
        R32G8X24_Typeless = 19,
        D32_Float_S8X24_Uint = 20,
        D32_Float_X8X24_Typeless = 21,
        X32_Typeless_G8X24_Uint = 22,
        R10G10B10A2_Typeless = 23,
        R10G10B10A2_Unorm = 24,
        R10G10B10A2_Uint = 25,
        R11G11B10_Float = 26,
        R8G8B8A8_Typeless = 27,
        R8G8B8A8_Unorm = 28,
        R8G8B8A8_Unorm_SRGB = 29,
        R8G8B8A8_Uint = 30,
        R8G8B8A8_Snorm = 31,
        R8G8B8A8_Sint = 32,
        R16G16_Typeless = 33,
        R16G16_Float = 34,
        R16G16_Unorm = 35,
        R16G16_Uint = 36,
        R16G16_Snorm = 37,
        R16G16_Sint = 38,
        R32_Typeless = 39,
        D32_Float = 40,
        R32_Float = 41,
        R32_Uint = 32,
        R32_Sint = 43,
        R24G8_Typeless = 44,
        D24_Unorm_S8_Uint = 45,
        R24_Unorm_X8_Typeless = 46,
        X24_Typeless_G8_Uint = 47,
        R8G8_Typeless = 48,
        R8G8_Unorm = 49,
        R8G8_Uint = 50,
        R8G8_Snorm = 51,
        R8G8_Sint = 52,
        R16_Typeless = 53,
        R16_Float = 54,
        D16_Unorm = 55,
        R16_Unorm = 56,
        R16_Uint = 57,
        R16_Snorm = 58,
        R16_Sint = 59,
        R8_Typeless = 60,
        R8_Unorm = 61,
        R8_Uint = 62,
        R8_Snorm = 63,
        R8_Sint = 64,
        A8_Unorm = 65,
        R1_Unorm = 66, 
        R9G9B9E5_SharedExp = 67,
        R8G8_B8G8_Unorm = 68,
        G8R8_G8B8_Unorm = 69,
        BC1_Typeless = 70,
        BC1_Unorm = 71,
        BC1_Unorm_SRGB = 72,
        BC2_Typeless = 73,
        BC2_Unorm = 74,
        BC2_Unorm_SRGB = 75,
        BC3_Typeless = 76,
        BC3_Unorm = 77,
        BC3_Unorm_SRGB = 78,
        BC4_Typeless = 79,
        BC4_Unorm = 80,
        BC4_Snorm = 81,
        BC5_Typeless = 82,
        BC5_Unorm = 83,
        BC5_Snorm = 84,
        B5G6R5_Unorm = 85,
        B5G5R5A1_Unorm = 86,
        B8G8R8A8_Unorm = 87,
        B8G8R8X8_Unorm = 88,
        R10G10B10_XR_Bias_A2_Unorm = 89,
        B8G8R8A8_Typeless = 90,
        B8G8R8A8_Unorm_SRGB = 91,
        B8G8R8X8_Typeless = 92,
        B8G8R8X8_Unorm_SRGB = 93,
        BC6H_Typeless = 94,
        BC6H_UF16 = 95,
        BC6H_SF16 = 96,
        BC7_Typeless = 97,
        BC7_Unorm = 98,
        BC7_Unorm_SRGB = 99
    }


public enum D3D10_RESOURCE_DIMENSION {
		D3D10_RESOURCE_DIMENSION_UNKNOWN = 0,
		D3D10_RESOURCE_DIMENSION_BUFFER = 1,
		D3D10_RESOURCE_DIMENSION_TEXTURE1D = 2,
		D3D10_RESOURCE_DIMENSION_TEXTURE2D = 3,
		D3D10_RESOURCE_DIMENSION_TEXTURE3D = 4
	}

public enum DX9SurfaceFormat {
    D3DFMT_UNKNOWN              =  0,

    D3DFMT_R8G8B8               = 20,
    D3DFMT_A8R8G8B8             = 21,
    D3DFMT_X8R8G8B8             = 22,
    D3DFMT_R5G6B5               = 23,
    D3DFMT_X1R5G5B5             = 24,
    D3DFMT_A1R5G5B5             = 25,
    D3DFMT_A4R4G4B4             = 26,
    D3DFMT_R3G3B2               = 27,
    D3DFMT_A8                   = 28,
    D3DFMT_A8R3G3B2             = 29,
    D3DFMT_X4R4G4B4             = 30,
    D3DFMT_A2B10G10R10          = 31,
    D3DFMT_A8B8G8R8             = 32,
    D3DFMT_X8B8G8R8             = 33,
    D3DFMT_G16R16               = 34,
    D3DFMT_A2R10G10B10          = 35,
    D3DFMT_A16B16G16R16         = 36,

    D3DFMT_A8P8                 = 40,
    D3DFMT_P8                   = 41,

    D3DFMT_L8                   = 50,
    D3DFMT_A8L8                 = 51,
    D3DFMT_A4L4                 = 52,

    D3DFMT_V8U8                 = 60,
    D3DFMT_L6V5U5               = 61,
    D3DFMT_X8L8V8U8             = 62,
    D3DFMT_Q8W8V8U8             = 63,
    D3DFMT_V16U16               = 64,
    D3DFMT_A2W10V10U10          = 67,

    /*D3DFMT_UYVY                 = MAKEFOURCC('U', 'Y', 'V', 'Y'),
    D3DFMT_R8G8_B8G8            = MAKEFOURCC('R', 'G', 'B', 'G'),
    D3DFMT_YUY2                 = MAKEFOURCC('Y', 'U', 'Y', '2'),
    D3DFMT_G8R8_G8B8            = MAKEFOURCC('G', 'R', 'G', 'B'),
    D3DFMT_DXT1                 = MAKEFOURCC('D', 'X', 'T', '1'),
    D3DFMT_DXT2                 = MAKEFOURCC('D', 'X', 'T', '2'),
    D3DFMT_DXT3                 = MAKEFOURCC('D', 'X', 'T', '3'),
    D3DFMT_DXT4                 = MAKEFOURCC('D', 'X', 'T', '4'),
    D3DFMT_DXT5                 = MAKEFOURCC('D', 'X', 'T', '5'),*/

	
	
	
    D3DFMT_D16_LOCKABLE         = 70,
    D3DFMT_D32                  = 71,
    D3DFMT_D15S1                = 73,
    D3DFMT_D24S8                = 75,
    D3DFMT_D24X8                = 77,
    D3DFMT_D24X4S4              = 79,
    D3DFMT_D16                  = 80,

    D3DFMT_D32F_LOCKABLE        = 82,
    D3DFMT_D24FS8               = 83,

    D3DFMT_D32_LOCKABLE         = 84,
    D3DFMT_S8_LOCKABLE          = 85,

    D3DFMT_L16                  = 81,

    D3DFMT_VERTEXDATA           =100,
    D3DFMT_INDEX16              =101,
    D3DFMT_INDEX32              =102,

    D3DFMT_Q16W16V16U16         =110,

    //D3DFMT_MULTI2_ARGB8         = MAKEFOURCC('M','E','T','1'),

    D3DFMT_R16F                 = 111,
    D3DFMT_G16R16F              = 112,
    D3DFMT_A16B16G16R16F        = 113,

    D3DFMT_R32F                 = 114,
    D3DFMT_G32R32F              = 115,
    D3DFMT_A32B32G32R32F        = 116,

    D3DFMT_CxV8U8               = 117,


    D3DFMT_A1                   = 118,
    D3DFMT_A2B10G10R10_XR_BIAS  = 119,
    D3DFMT_BINARYBUFFER         = 199,

    D3DFMT_FORCE_DWORD          =0x7fffffff
}
}
