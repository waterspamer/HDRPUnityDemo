using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace Nettle {
    public class Alpha8TGALoader : iRuntimeTextureLoader {
        int NextPixel(int bitDepth) {
            if (bitDepth == 32) {
                return 4;
            }
            if (bitDepth == 24) {
                return 3;
            }
            return 2;
        }

        public Texture2D Load(string path) {
            try {
                byte[] data = File.ReadAllBytes(path);
                bool isCompressed = data[2] >= 9;
                // Skip some header info we don't care about.
                int pointer = 12;
                short width = BitConverter.ToInt16(data, pointer);
                pointer += sizeof(Int16);
                short height = BitConverter.ToInt16(data, pointer);
                pointer += sizeof(Int16);
                int bitDepth = data[pointer];
                pointer += 2;
                Texture2D tex = new Texture2D(width, height, TextureFormat.Alpha8, false);
                byte[] pulledBytes = new byte[width * height];
                if (!isCompressed) {
                    for (int i = 0; i < width * height; i++) {
                        pulledBytes[i] = data[pointer];
                        pointer += NextPixel(bitDepth);
                    }
                }
                else {
                    int outputPointer = 0;
                    while (outputPointer < pulledBytes.Length) {
                        int chunkHeader = data[pointer];
                        pointer++;
                        if (chunkHeader < 128) {
                            //Read RAW sequence
                            chunkHeader++;
                            for (int i = 0; i < chunkHeader; i++) {
                                pulledBytes[outputPointer] = data[pointer];
                                pointer += NextPixel(bitDepth);
                                outputPointer++;
                            }
                        }
                        else {
                            //Header is RLE, read the next pixel and fill chunk with its value
                            chunkHeader -= 127;
                            for (int i = 0; i < chunkHeader; i++) {
                                pulledBytes[outputPointer] = data[pointer];
                                outputPointer++;
                            }
                            pointer += NextPixel(bitDepth);
                        }
                    }
                }
                tex.LoadRawTextureData(pulledBytes);
                tex.Apply();
                return tex;
            }            
            catch (Exception ex){
                Debug.LogError("Error loading image " + path + "\n"+ ex.Message);
                return null;
            }        
        }        
    }
}