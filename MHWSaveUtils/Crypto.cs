﻿using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MHWSaveUtils
{
    // Copied from this repository: git@github.com:Nexusphobiker/MHWSaveDecrypter.git
    // Then optimzed
    // Interestingly, the unsafe way is as fast as the managed way (was expecting much faster)

    public static class Crypto
    {
        private static readonly byte[] key_4096_bytes = { 0x79, 0xE5, 0x70, 0x83, 0xE1, 0xB9, 0xA4, 0x44, 0xC4, 0xB0, 0x6C, 0xF8, 0x82, 0x1D, 0x3A, 0xAD, 0xD9, 0xF4, 0xC9, 0xBF, 0x84, 0xD0, 0x0F, 0x83, 0x60, 0x51, 0xD5, 0xDD, 0x58, 0x9F, 0xE9, 0xD4, 0xFD, 0xA9, 0x8B, 0x31, 0x52, 0xA7, 0x5E, 0x5B, 0x39, 0xF1, 0x39, 0x00, 0xFF, 0xD3, 0xCF, 0x9F, 0xC7, 0xD4, 0xD8, 0xA2, 0xD3, 0x27, 0x50, 0x35, 0xF9, 0xC8, 0xC9, 0xB5, 0x86, 0x0C, 0x2F, 0x7E, 0xB0, 0x11, 0x59, 0x83, 0x75, 0x96, 0xCE, 0x16, 0x52, 0xA5, 0x18, 0xB5, 0xFF, 0x35, 0x5E, 0xCE, 0xFF, 0xD0, 0xAF, 0x09, 0x59, 0xFE, 0x94, 0xBC, 0xD7, 0x3E, 0xF3, 0xF3, 0x5C, 0x2D, 0x85, 0xCF, 0x52, 0xF5, 0xBF, 0xEF, 0x23, 0xCB, 0x4A, 0xB3, 0x5E, 0xEE, 0x60, 0x72, 0x9C, 0xA9, 0xD8, 0x75, 0x57, 0x0C, 0xC7, 0x99, 0x58, 0xD9, 0x2C, 0xCB, 0x06, 0x9A, 0x16, 0xA0, 0x61, 0xA5, 0x05, 0xD1, 0xD4, 0x0D, 0xAF, 0xF7, 0xFD, 0x52, 0xFB, 0xB8, 0xDA, 0x84, 0x2C, 0xAE, 0x0A, 0x10, 0x0D, 0x0D, 0x2C, 0x02, 0x0C, 0xFA, 0xC7, 0x03, 0xB9, 0x86, 0xFE, 0xB4, 0xC5, 0xC4, 0x67, 0x62, 0x5F, 0x61, 0xD4, 0x9A, 0x6E, 0x33, 0x22, 0x37, 0x18, 0x0B, 0x0C, 0x63, 0x10, 0x86, 0x66, 0x31, 0x4D, 0xB3, 0x10, 0x76, 0xAF, 0x92, 0x94, 0x52, 0x56, 0x85, 0xAF, 0x1A, 0x55, 0x77, 0x3B, 0x29, 0x28, 0x4E, 0x3D, 0x22, 0xBB, 0x67, 0x70, 0xC8, 0xA9, 0xEF, 0x4B, 0x52, 0x3B, 0x80, 0x69, 0xEF, 0xEF, 0x9E, 0xD8, 0x0A, 0x7E, 0x2F, 0xAA, 0x98, 0x97, 0x46, 0x24, 0x83, 0x5F, 0xEA, 0x07, 0xDD, 0x3E, 0x34, 0x87, 0x53, 0xD7, 0x5E, 0x72, 0xED, 0x6C, 0x59, 0x9D, 0xF8, 0x4A, 0x7D, 0x53, 0x96, 0xE8, 0xFB, 0x24, 0x43, 0x83, 0xA0, 0xF1, 0xF0, 0x55, 0x9A, 0x88, 0x34, 0x15, 0x6D, 0x55, 0x3B, 0x14, 0xE9, 0xEA, 0x74, 0xE0, 0x79, 0x79, 0x75, 0x8B, 0x29, 0xED, 0x9D, 0xE5, 0xEC, 0x16, 0x09, 0xD3, 0x5F, 0xB5, 0xB2, 0x12, 0x6A, 0x4F, 0x55, 0x50, 0xF2, 0xB3, 0x5B, 0x6F, 0x8D, 0x88, 0xAE, 0x01, 0xA9, 0xDF, 0x08, 0x30, 0x10, 0xFB, 0x18, 0x9F, 0x2B, 0xBD, 0x40, 0x81, 0x72, 0xB0, 0x0A, 0x54, 0xE9, 0xA8, 0x9B, 0xD7, 0xD8, 0x71, 0xA7, 0xFF, 0xFA, 0xE9, 0x8E, 0x86, 0xA5, 0x91, 0x9B, 0x3E, 0x80, 0x2A, 0x89, 0x31, 0xEF, 0x34, 0x06, 0xCE, 0x7E, 0x23, 0x70, 0x0F, 0xEA, 0xD8, 0x2A, 0x66, 0xB5, 0x22, 0x7A, 0xA6, 0x60, 0x57, 0xF2, 0xF0, 0xEB, 0xA6, 0x60, 0x50, 0xC7, 0x70, 0xAB, 0x2E, 0xA9, 0xFA, 0xE7, 0x05, 0x98, 0xCC, 0x14, 0xA8, 0x94, 0xA6, 0x29, 0xBF, 0xFB, 0x96, 0xCE, 0xD2, 0x28, 0x58, 0x30, 0xAF, 0xF3, 0xA1, 0xCE, 0x5F, 0xD5, 0x85, 0x6F, 0x96, 0x2A, 0x2D, 0xA2, 0x9B, 0xA5, 0xA8, 0xB4, 0x92, 0xAB, 0xB7, 0x0F, 0x07, 0x73, 0x5B, 0x77, 0x9B, 0x75, 0xF0, 0x31, 0x9C, 0x8F, 0xC0, 0x49, 0x0A, 0x55, 0x9B, 0x42, 0x5A, 0x46, 0x34, 0xE1, 0x2C, 0x5D, 0x88, 0xE9, 0xFC, 0x24, 0xC3, 0x20, 0x82, 0x7B, 0x33, 0xFA, 0x32, 0x13, 0x0E, 0x0F, 0x11, 0xCA, 0xB8, 0x92, 0x88, 0x5C, 0x2B, 0x0C, 0x95, 0x29, 0xEF, 0x81, 0x76, 0xC9, 0xE2, 0x4E, 0xC9, 0x68, 0x25, 0xA2, 0x40, 0xD4, 0x91, 0x15, 0xD4, 0xA1, 0x27, 0xFD, 0xC8, 0xA7, 0xF5, 0xF1, 0x56, 0xF8, 0x0B, 0xF8, 0xFB, 0xE9, 0x5D, 0x09, 0xE4, 0x9E, 0xB8, 0xAB, 0xD7, 0x7C, 0x86, 0x48, 0x72, 0x1F, 0x8D, 0x20, 0xD2, 0x5F, 0x45, 0x16, 0x1D, 0xA1, 0x06, 0xED, 0x2E, 0xC5, 0x49, 0x5C, 0x38, 0x3A, 0x9D, 0x04, 0x3B, 0x33, 0x35, 0x6D, 0xD5, 0xA9, 0x59, 0x13, 0x0A, 0x7A, 0xE5, 0xCD, 0x4A, 0xD5, 0xD8, 0x80, 0x2F, 0x67, 0x5C, 0x14, 0x0E, 0x65, 0xB3, 0x23, 0x56, 0x1D, 0xE1, 0xA6, 0x10, 0xC1, 0xC6, 0x68, 0x83, 0x93, 0xE4, 0x04, 0xB2, 0x7F, 0xC6, 0x2F, 0x53, 0x09, 0xBE, 0xB3, 0x08, 0x57, 0x40, 0x53, 0x77, 0x53, 0xF2, 0xCB, 0x45, 0x79, 0xD8, 0x3E, 0x4B, 0xAE, 0xE2, 0x2D, 0x03, 0x4A, 0xBA, 0x0B, 0x2D, 0x73, 0xF7, 0x4C, 0x12, 0x30, 0x9F, 0x05, 0x34, 0xCE, 0x0B, 0x5A, 0x72, 0x3B, 0x58, 0xD8, 0x11, 0x0A, 0x66, 0xC2, 0xD0, 0x04, 0xFD, 0x8C, 0x5A, 0xCA, 0x89, 0x6B, 0x59, 0x11, 0x47, 0xBD, 0xBE, 0xDE, 0xC3, 0x27, 0x47, 0x90, 0x0B, 0xE8, 0x48, 0x83, 0xF4, 0x1C, 0x82, 0x96, 0x43, 0x80, 0xE3, 0xC6, 0x81, 0x1C, 0xD9, 0x8D, 0x16, 0x61, 0x48, 0xF7, 0x73, 0xB1, 0x9A, 0xAD, 0x56, 0x44, 0xF9, 0x55, 0xFD, 0xAD, 0xB8, 0x39, 0xDB, 0x44, 0x08, 0x9D, 0xDB, 0x5D, 0x0E, 0x5B, 0x34, 0xB7, 0x3B, 0x53, 0xAB, 0x68, 0x1F, 0x27, 0xA7, 0x71, 0xDE, 0x41, 0x4E, 0xE9, 0x1B, 0x11, 0x7A, 0xE7, 0x46, 0x41, 0xD0, 0xA8, 0xA4, 0x9F, 0x87, 0x23, 0xCC, 0xC5, 0xD0, 0xEC, 0x52, 0xAA, 0x9A, 0x8D, 0x78, 0xD5, 0x23, 0x2D, 0xE2, 0xA0, 0xAC, 0xF1, 0x15, 0xF9, 0x98, 0x52, 0xA8, 0x87, 0x1C, 0x7D, 0x35, 0xD3, 0x16, 0xB7, 0x73, 0x8F, 0x00, 0x9C, 0x99, 0x2F, 0xEA, 0x8A, 0xAD, 0xAB, 0xE3, 0xA8, 0x27, 0x6C, 0xC1, 0x24, 0x4E, 0x4C, 0xB8, 0x6E, 0xCE, 0x6D, 0xB7, 0x84, 0xD5, 0xBD, 0xC2, 0x72, 0x3A, 0x3A, 0x33, 0x75, 0x34, 0x62, 0xC9, 0x82, 0x78, 0xF1, 0xB0, 0x9E, 0x6F, 0x4B, 0xA5, 0xBA, 0xBE, 0x53, 0xBF, 0x36, 0x2E, 0xB3, 0x7E, 0xBC, 0xC3, 0x7B, 0xE6, 0xB6, 0x66, 0x62, 0xE7, 0xE8, 0xB3, 0xAD, 0xB2, 0xF6, 0x83, 0xBD, 0xAC, 0x5F, 0x10, 0x75, 0x7A, 0xA2, 0x42, 0xDA, 0x5B, 0x89, 0x1A, 0xAB, 0x7D, 0x71, 0xD7, 0xBD, 0x9C, 0xB1, 0xEB, 0x00, 0xDF, 0x28, 0xA1, 0x18, 0x7A, 0x70, 0x41, 0x01, 0x0C, 0x05, 0x8A, 0xEA, 0x72, 0x1B, 0x6D, 0x15, 0x38, 0x7B, 0xC7, 0x48, 0x57, 0x15, 0x5D, 0x1D, 0xC9, 0xC0, 0x3C, 0xF5, 0x31, 0xCC, 0x82, 0x3E, 0xC6, 0x6B, 0xC9, 0x77, 0x58, 0xAC, 0xEF, 0xE7, 0x78, 0x18, 0x94, 0xF7, 0xAA, 0xB1, 0x99, 0x78, 0x1C, 0xE2, 0xA0, 0x92, 0xE3, 0x87, 0xBA, 0x6C, 0x83, 0x8E, 0x30, 0xA0, 0xB3, 0x27, 0x5D, 0xF4, 0xFF, 0xDC, 0x58, 0x4D, 0xB4, 0x05, 0x3C, 0x4A, 0x46, 0x04, 0xAB, 0x5A, 0xCA, 0x3D, 0xEA, 0x7F, 0xAE, 0xCD, 0x06, 0x34, 0xDD, 0xDA, 0x71, 0x68, 0xFD, 0x54, 0x4E, 0x2E, 0x6F, 0x06, 0x3D, 0x89, 0x6E, 0x15, 0x59, 0x2D, 0x0D, 0xC3, 0x0E, 0xBC, 0x5D, 0xF8, 0x51, 0x5B, 0x07, 0xFC, 0x9F, 0xA2, 0x0A, 0x3C, 0x10, 0x27, 0x0B, 0x81, 0xE5, 0x85, 0x82, 0x9B, 0xB5, 0xC4, 0x74, 0xE3, 0x92, 0x81, 0xCA, 0x84, 0xE7, 0x3E, 0xC6, 0xB9, 0x05, 0x82, 0xEA, 0x1B, 0x0A, 0x71, 0x30, 0xCC, 0x74, 0x20, 0x2A, 0x01, 0x7A, 0x15, 0x0E, 0x6E, 0xAC, 0x1B, 0xD6, 0x7F, 0x91, 0x69, 0x77, 0x1C, 0xB7, 0xB7, 0xD1, 0x68, 0x2A, 0xF7, 0x37, 0xB5, 0xCB, 0xE3, 0x34, 0x34, 0xF4, 0x18, 0x18, 0xF8, 0x03, 0xB5, 0x6C, 0xCF, 0xE7, 0x4C, 0x5A, 0x82, 0xF0, 0x13, 0x68, 0x18, 0x32, 0xC0, 0x21, 0xBC, 0xAD, 0x17, 0x51, 0xD3, 0xAC, 0xAE, 0x4F, 0x63, 0xC9, 0x71, 0x55, 0xED, 0x10, 0x12, 0xF2, 0x73, 0x18, 0xFB, 0xC1, 0xE2, 0x42, 0x11, 0x3C, 0x98, 0xFD, 0xC7, 0x8E, 0x70, 0x01, 0x35, 0xAF, 0x44, 0x82, 0xB7, 0x9B, 0xAE, 0x12, 0x6D, 0x35, 0xC5, 0xA5, 0x33, 0xDE, 0x07, 0x83, 0x84, 0x52, 0x6D, 0xD2, 0x9F, 0x8F, 0x12, 0x28, 0x0B, 0x9D, 0x7D, 0x46, 0x66, 0x9C, 0x88, 0x14, 0x77, 0xD0, 0x95, 0x9F, 0xF8, 0x4E, 0x34, 0xCC, 0x8E, 0xA8, 0x40, 0x75, 0xFC, 0xC4, 0x4B, 0x4A, 0x55, 0xC2, 0x43, 0x20, 0x8B, 0xB8, 0xB4, 0x35, 0x74, 0x55, 0x74, 0xEC, 0xCB, 0xE7, 0x8A, 0x33, 0x7A, 0x39, 0xD0, 0x22, 0xA7, 0x9F, 0x2F, 0x74, 0x1C, 0x62, 0x40, 0x46, 0xB1, 0x6E, 0xDF, 0xA9, 0xF0, 0xD2, 0xB1, 0x5C, 0xD6, 0x52, 0x99, 0x91, 0x0F, 0x67, 0x84, 0xC9, 0xFB, 0x39, 0x74, 0x96, 0x0B, 0xF3, 0x03, 0x95, 0xE2, 0x67, 0x7E, 0x16, 0xFE, 0x70, 0xDD, 0x77, 0xF5, 0xDC, 0x2C, 0xA5, 0x0E, 0xCD, 0xA9, 0xED, 0x68, 0x64, 0xDC, 0xC1, 0xE2, 0x36, 0xD2, 0x97, 0x6D, 0x5E, 0x0C, 0x13, 0xBF, 0x14, 0xFA, 0xEC, 0x66, 0x3D, 0x30, 0x75, 0xE9, 0x92, 0x92, 0x17, 0x8F, 0xD7, 0x5D, 0x8E, 0x77, 0x4C, 0x30, 0xEE, 0x54, 0x71, 0xED, 0xE0, 0xD1, 0x1B, 0x65, 0x56, 0x47, 0x14, 0x07, 0xA5, 0x4B, 0x2C, 0xF4, 0x4B, 0x75, 0x3B, 0x07, 0xEB, 0x10, 0x25, 0x2C, 0xC2, 0x11, 0x68, 0xB4, 0x4D, 0xBC, 0xC2, 0x48, 0xA2, 0xD9, 0x88, 0xBA, 0x19, 0x25, 0x81, 0x87, 0x5C, 0x17, 0x43, 0xCB, 0x0F, 0xE6, 0x85, 0x6D, 0x35, 0xB4, 0x77, 0x41, 0x7A, 0x20, 0x99, 0x93, 0x15, 0x0D, 0x62, 0x30, 0x3B, 0x15, 0x37, 0x6A, 0x14, 0x0D, 0x71, 0x70, 0x6E, 0x3F, 0xE9, 0x4A, 0x09, 0xE6, 0xBE, 0xC9, 0x25, 0x3C, 0xE2, 0x21, 0xB5, 0x7F, 0xB2, 0x60, 0xB9, 0x12, 0xD8, 0x75, 0xA1, 0x29, 0xB7, 0x16, 0x40, 0x6C, 0xF9, 0xE5, 0x4A, 0xB9, 0x53, 0x8E, 0xD3, 0x0F, 0x06, 0x05, 0x51, 0xE2, 0x00, 0x27, 0xD4, 0x61, 0x5B, 0x55, 0x41, 0x31, 0x0E, 0xB8, 0xB2, 0x05, 0x2B, 0x79, 0x72, 0x0D, 0x76, 0x0C, 0x58, 0xA7, 0xF7, 0x2A, 0x5E, 0x8E, 0x3D, 0x88, 0x19, 0xD2, 0xD4, 0x66, 0x5D, 0x45, 0x38, 0xB2, 0x32, 0xF5, 0x23, 0x68, 0x62, 0x91, 0x90, 0xC4, 0xBC, 0x54, 0x18, 0x17, 0x6D, 0xA1, 0xE6, 0xA5, 0x9D, 0xE9, 0x82, 0xC9, 0x5C, 0xC2, 0xB8, 0x83, 0x91, 0x74, 0xC6, 0x9C, 0x88, 0x18, 0x7E, 0xBC, 0x68, 0xCF, 0x0C, 0x03, 0x90, 0x71, 0x38, 0xA7, 0x97, 0x9C, 0x55, 0x7A, 0x36, 0x47, 0x98, 0xA1, 0x34, 0x2E, 0x50, 0xD2, 0x97, 0xF9, 0xFC, 0x33, 0x16, 0xAE, 0x44, 0x4D, 0xC2, 0xFB, 0xE9, 0x04, 0xC6, 0x4C, 0x03, 0xB9, 0x6D, 0x4A, 0x39, 0xE5, 0xF3, 0x0B, 0xE1, 0x66, 0x98, 0x73, 0x6B, 0xE5, 0xEB, 0x54, 0x1D, 0x76, 0xDC, 0x3A, 0xF1, 0xB9, 0x80, 0x5A, 0xB4, 0x08, 0x83, 0x82, 0x7E, 0x44, 0x4A, 0x23, 0x7A, 0xDE, 0x57, 0x4A, 0x52, 0x8A, 0xAD, 0xEE, 0x32, 0x66, 0x70, 0x5E, 0xAA, 0x39, 0x74, 0xEF, 0xFF, 0xF4, 0x40, 0x4B, 0x50, 0xB6, 0x5B, 0x2A, 0x1D, 0x2E, 0x49, 0x78, 0x10, 0xAE, 0x2D, 0xE6, 0x34, 0xCA, 0x08, 0x5F, 0x24, 0x27, 0x9E, 0x97, 0x37, 0xB0, 0x1A, 0x54, 0xF7, 0x54, 0xE8, 0x57, 0x90, 0x32, 0x08, 0x18, 0x6B, 0xEA, 0xF3, 0xD2, 0xE7, 0x53, 0x5E, 0x83, 0xA3, 0xE8, 0x3E, 0xB4, 0x03, 0xD4, 0x29, 0xB7, 0xB0, 0x95, 0x89, 0xB6, 0x3D, 0x59, 0x19, 0x3A, 0x12, 0x5B, 0x70, 0x5C, 0x17, 0xA7, 0x35, 0x66, 0x04, 0xC3, 0x02, 0xDF, 0xC4, 0x0E, 0xB6, 0xDB, 0x59, 0x69, 0xD5, 0xB7, 0xBD, 0xC1, 0x89, 0x59, 0x25, 0x31, 0xFA, 0x53, 0xBE, 0x03, 0x0D, 0x18, 0x07, 0x96, 0x09, 0xAF, 0x31, 0x9C, 0x19, 0x23, 0x8C, 0x48, 0xCA, 0x50, 0x5D, 0x3B, 0xAB, 0x9B, 0x0A, 0xED, 0x14, 0x88, 0xBF, 0x9B, 0xC4, 0x98, 0xAB, 0x53, 0x1B, 0xA2, 0x9A, 0xFD, 0x23, 0x76, 0x07, 0x11, 0x92, 0x9F, 0xCB, 0x4E, 0x3F, 0xC2, 0x80, 0x5C, 0x59, 0xA5, 0x89, 0x9A, 0x6B, 0xC9, 0x58, 0x73, 0x81, 0x7A, 0xBD, 0x23, 0x31, 0x5B, 0x12, 0x1D, 0x4B, 0x0C, 0x0C, 0xBE, 0x8A, 0x67, 0x41, 0xA1, 0xC2, 0x54, 0x89, 0xD6, 0x85, 0xCB, 0x93, 0xBA, 0x5E, 0x77, 0xED, 0x93, 0x52, 0xC3, 0x9C, 0xF0, 0x89, 0xD3, 0xC7, 0x19, 0x8F, 0x40, 0xAE, 0xB7, 0x63, 0x4F, 0xC1, 0x93, 0xDE, 0xB8, 0x85, 0x5E, 0x8D, 0x5C, 0x6C, 0x65, 0xDC, 0x36, 0x36, 0xB2, 0x97, 0x20, 0xE5, 0xE5, 0x30, 0x34, 0x59, 0xB0, 0xEB, 0xBB, 0x48, 0xCB, 0x12, 0x4F, 0xE5, 0xB7, 0x39, 0x16, 0x73, 0x71, 0xEF, 0x5E, 0x9D, 0xE3, 0x86, 0xDD, 0x31, 0x08, 0xAA, 0x46, 0x68, 0x0E, 0x27, 0x9A, 0x92, 0x08, 0x18, 0x97, 0x76, 0xEB, 0x8C, 0x3B, 0x9F, 0x62, 0xD6, 0x03, 0x33, 0x76, 0x38, 0x28, 0xA5, 0xC2, 0x83, 0x8B, 0xCE, 0xD2, 0xCD, 0x4E, 0x56, 0x74, 0x51, 0xC2, 0x29, 0x0C, 0x2A, 0x5B, 0x4F, 0xA4, 0x2F, 0x09, 0xE5, 0xEC, 0x6B, 0xD5, 0xD7, 0x79, 0x8C, 0xC4, 0xF2, 0xDE, 0x37, 0x15, 0x73, 0xB5, 0x8A, 0x08, 0x69, 0x0B, 0x12, 0xEA, 0x0F, 0x02, 0x29, 0x2E, 0x2D, 0xD0, 0xF1, 0x92, 0x42, 0x3B, 0xF6, 0x42, 0x63, 0x9F, 0x10, 0x88, 0xFE, 0x98, 0xB8, 0x14, 0x7D, 0x3D, 0x05, 0x92, 0xDE, 0xAE, 0x17, 0xB4, 0xD0, 0xEA, 0xDD, 0xF0, 0xE7, 0x92, 0xF3, 0x0B, 0x49, 0xDE, 0x57, 0xFF, 0x34, 0xC3, 0x22, 0xC4, 0x45, 0xF9, 0xEA, 0xD5, 0x1C, 0x7D, 0x11, 0x76, 0xF9, 0xE5, 0x93, 0x74, 0xCD, 0x08, 0xCE, 0x89, 0xB1, 0x55, 0xE9, 0xF3, 0xFE, 0x8A, 0x4E, 0x68, 0x54, 0x96, 0x12, 0x48, 0x7F, 0x5D, 0x31, 0xCB, 0x33, 0x7A, 0x57, 0xE2, 0x20, 0x0B, 0xBA, 0x26, 0xE3, 0xE8, 0xE7, 0xAD, 0x85, 0x97, 0xCB, 0xFF, 0x01, 0x12, 0x50, 0x16, 0x4D, 0xEA, 0xDF, 0x37, 0x18, 0xEA, 0xF5, 0xC6, 0xBF, 0x4D, 0x06, 0x15, 0xCB, 0x3C, 0x97, 0x7C, 0xF8, 0x82, 0x05, 0x9D, 0x92, 0xDF, 0x1C, 0x8B, 0x38, 0x47, 0xA0, 0x62, 0x3E, 0xE8, 0x04, 0x79, 0x10, 0xA9, 0xA1, 0xDD, 0xEA, 0xAE, 0x93, 0xAD, 0x4F, 0xD1, 0x5D, 0x22, 0x0A, 0x77, 0x1F, 0xED, 0x55, 0x54, 0x03, 0x87, 0xAD, 0xA8, 0x3C, 0x94, 0xF0, 0xBB, 0xE4, 0x90, 0x59, 0x41, 0x1A, 0x8E, 0x88, 0x94, 0xD6, 0xAE, 0x65, 0xE8, 0xB2, 0xA1, 0xB8, 0xD5, 0xAA, 0xBB, 0x8B, 0xF4, 0x58, 0x06, 0xD8, 0x64, 0xAB, 0x1F, 0x0E, 0x35, 0x56, 0x0E, 0x93, 0xEE, 0x1B, 0xA1, 0x2E, 0xE3, 0xD5, 0x74, 0xAE, 0x9C, 0x91, 0x68, 0x33, 0x8D, 0xCF, 0x89, 0x6A, 0x27, 0xC1, 0x13, 0x2B, 0x39, 0x45, 0xB3, 0x9F, 0x29, 0xA4, 0x19, 0x00, 0xAC, 0xEB, 0x2B, 0x25, 0x5E, 0x03, 0xDD, 0x19, 0x3B, 0xF6, 0x05, 0xFC, 0xC6, 0xF7, 0x03, 0xBC, 0x04, 0x1D, 0xF3, 0x7C, 0x3E, 0x39, 0xFE, 0x8A, 0x01, 0x76, 0x08, 0xB3, 0x17, 0x96, 0x22, 0x1B, 0xAA, 0xDF, 0xAC, 0x42, 0xCA, 0x61, 0xAC, 0xCD, 0xCB, 0xBF, 0x60, 0x5C, 0x88, 0x61, 0x12, 0x1B, 0x4A, 0xD2, 0x30, 0x3E, 0xD1, 0x3B, 0x62, 0xF3, 0xEC, 0xFB, 0xCB, 0x61, 0xEC, 0xD0, 0xD2, 0x30, 0xC4, 0x90, 0xDD, 0x9B, 0xD6, 0x36, 0xE7, 0xA8, 0x5C, 0xD0, 0xD8, 0x99, 0xB2, 0xEF, 0x19, 0x92, 0xC1, 0x34, 0xCD, 0xC6, 0x45, 0xDC, 0xD7, 0x58, 0x44, 0x27, 0xF7, 0xCB, 0x26, 0xE2, 0x74, 0x79, 0x57, 0x66, 0x04, 0x71, 0x7B, 0x25, 0xCE, 0xE3, 0x0B, 0xBD, 0xD9, 0x67, 0x9B, 0xEB, 0x38, 0x69, 0x91, 0x6E, 0x37, 0xEE, 0x4A, 0x2C, 0x1B, 0xE9, 0xF1, 0xA7, 0x38, 0xCB, 0xE7, 0x87, 0xBB, 0xFE, 0x9F, 0x0B, 0x95, 0x5F, 0x34, 0x5F, 0x70, 0x9B, 0xB9, 0x4C, 0x04, 0x6F, 0x1C, 0xA7, 0x92, 0x0D, 0x8A, 0x73, 0x60, 0xFC, 0x7E, 0x19, 0x3B, 0xDC, 0xF1, 0x45, 0x56, 0xA5, 0x09, 0xAD, 0x5C, 0xA6, 0x2D, 0xE6, 0xCF, 0xED, 0xF4, 0xFA, 0x16, 0x73, 0xF4, 0xD8, 0x3E, 0xA6, 0x7F, 0xF1, 0x53, 0x77, 0xD7, 0xA0, 0x71, 0x2B, 0xD8, 0x12, 0x0F, 0x19, 0xA4, 0x70, 0x33, 0x61, 0x36, 0xBA, 0xA6, 0xF5, 0x73, 0x25, 0xED, 0x0B, 0x4B, 0xAD, 0x59, 0x59, 0xDA, 0xB9, 0xAB, 0xBF, 0x87, 0x80, 0x54, 0xA1, 0x46, 0xB4, 0xD6, 0xE8, 0xE9, 0xB4, 0x3C, 0xF5, 0x13, 0x5B, 0x5B, 0x2C, 0xF3, 0xDE, 0xEE, 0xA8, 0xF7, 0x5E, 0x82, 0xA0, 0xBC, 0x01, 0x43, 0x4A, 0x3D, 0xC5, 0x1E, 0xC1, 0xB0, 0x8C, 0x44, 0x0A, 0x2D, 0x6C, 0xC1, 0xD7, 0x3B, 0xDE, 0x8D, 0x86, 0x35, 0x2E, 0x10, 0x03, 0xAE, 0x6C, 0x1D, 0x82, 0x77, 0x22, 0x68, 0x10, 0x58, 0x1A, 0x50, 0x6B, 0x03, 0x16, 0x3F, 0xEE, 0x4C, 0x6B, 0xEF, 0x1B, 0x9C, 0x0F, 0xE0, 0x65, 0x32, 0x17, 0xFC, 0x7E, 0x7E, 0xC9, 0x7C, 0x5D, 0x40, 0xE8, 0x3E, 0xCA, 0xB0, 0xAC, 0x67, 0x85, 0xF3, 0x8F, 0x24, 0x6E, 0x27, 0xCF, 0x93, 0x9E, 0xD3, 0x4A, 0x13, 0xB5, 0xE4, 0xEE, 0x5E, 0x32, 0x0B, 0xBB, 0xBC, 0x3B, 0xD5, 0xFB, 0xFD, 0xA5, 0x11, 0x41, 0xA4, 0xD7, 0xEF, 0xF4, 0x0A, 0x49, 0xFD, 0x5D, 0x3D, 0x67, 0x46, 0xC9, 0xC4, 0xDF, 0x64, 0x15, 0xC9, 0x5D, 0x08, 0x07, 0x9C, 0x2D, 0x45, 0x57, 0x66, 0x2A, 0x61, 0x0E, 0xC9, 0x75, 0x67, 0x53, 0xBD, 0x93, 0xC2, 0xA4, 0x34, 0x2E, 0x86, 0x4D, 0x1C, 0x3D, 0x35, 0x74, 0xDA, 0xAA, 0x3B, 0xB3, 0xC8, 0x38, 0xE5, 0x45, 0xCE, 0xA2, 0xCF, 0xD3, 0xCE, 0x8F, 0x3D, 0x0A, 0x8D, 0xA2, 0xDA, 0x33, 0x3B, 0x35, 0x1D, 0x77, 0x16, 0xB4, 0x44, 0x7C, 0x59, 0x49, 0xFD, 0xA0, 0x1C, 0x13, 0x02, 0xBF, 0x8F, 0x5A, 0xDF, 0xAA, 0x46, 0xD1, 0xCB, 0x3D, 0x54, 0x68, 0xC3, 0xB4, 0xF7, 0x96, 0x02, 0xDF, 0xF8, 0xB8, 0x3F, 0x01, 0xB0, 0x54, 0x2B, 0xD3, 0xCB, 0xC7, 0x9D, 0xB0, 0x9C, 0x66, 0x51, 0xA0, 0xCC, 0x3C, 0xBD, 0xCE, 0x3A, 0x62, 0x09, 0x3F, 0xA2, 0x7F, 0x8C, 0x8D, 0x37, 0x76, 0x4A, 0x2F, 0xEF, 0x5B, 0x0D, 0x1E, 0x83, 0x35, 0x9F, 0xF4, 0xC4, 0x07, 0xF3, 0xA0, 0x34, 0x15, 0x07, 0xA9, 0x7B, 0x30, 0xAF, 0x90, 0xC6, 0xC1, 0xFD, 0x09, 0xBB, 0xD7, 0xF6, 0xDD, 0x44, 0x94, 0x57, 0xD6, 0xEE, 0xDB, 0x95, 0x53, 0xB0, 0xB3, 0xFD, 0x46, 0x8C, 0xBE, 0xF4, 0x87, 0xE7, 0xD0, 0x76, 0xF9, 0xC5, 0x17, 0xF5, 0xD2, 0x48, 0xDD, 0xB2, 0xE0, 0x07, 0x04, 0x0A, 0x1B, 0x52, 0x04, 0x35, 0x8E, 0x2B, 0x52, 0x2E, 0x57, 0x46, 0x70, 0x59, 0xCB, 0x4C, 0x8E, 0x18, 0x70, 0xE2, 0x47, 0xB3, 0xE9, 0xA9, 0xCD, 0x0C, 0x8B, 0x1D, 0xF4, 0xD8, 0x9C, 0xCD, 0xD0, 0x35, 0xD0, 0x41, 0x84, 0x61, 0x99, 0x1F, 0x7E, 0x3D, 0x09, 0xF9, 0x56, 0xEB, 0x48, 0xE2, 0x86, 0x9A, 0x6F, 0xF7, 0x0C, 0x1D, 0x2A, 0xE4, 0xB1, 0x61, 0x96, 0xD2, 0xD3, 0x38, 0xB2, 0x29, 0xA6, 0x83, 0x4B, 0x47, 0x17, 0x8B, 0x15, 0xC2, 0x89, 0x98, 0xCB, 0x99, 0x61, 0x6E, 0x59, 0xF9, 0x7B, 0x13, 0xC2, 0x2A, 0xBA, 0x21, 0x13, 0xE6, 0xCD, 0x8F, 0x5F, 0x13, 0xF9, 0x6F, 0x57, 0xBE, 0xAA, 0xF1, 0x82, 0xA0, 0xF0, 0xD8, 0xF5, 0x48, 0x77, 0x41, 0xFE, 0xA9, 0xDA, 0x32, 0x91, 0x4F, 0x0D, 0x55, 0xFD, 0x06, 0xE6, 0x20, 0x0E, 0xBF, 0x88, 0xF6, 0x2E, 0x09, 0xDD, 0xF8, 0x96, 0xB4, 0xD0, 0x7F, 0xFC, 0xDD, 0x7D, 0xFE, 0xF1, 0x50, 0x89, 0xFE, 0xAB, 0xC7, 0x22, 0x58, 0x57, 0xB7, 0x56, 0xC8, 0x2A, 0x12, 0x9F, 0x33, 0x34, 0xAA, 0x3D, 0xED, 0xAA, 0x86, 0x3E, 0xA3, 0x00, 0x1B, 0x38, 0xE1, 0xC9, 0xCA, 0xA4, 0x9D, 0x57, 0x3B, 0xC2, 0x22, 0x27, 0x8B, 0x42, 0x3A, 0xA4, 0x73, 0xB5, 0x0A, 0x19, 0x68, 0xD4, 0x1D, 0x4A, 0x93, 0x97, 0xA9, 0x1D, 0x18, 0x09, 0x13, 0x7B, 0xF0, 0x70, 0xD9, 0x42, 0xD0, 0xDE, 0xF7, 0x2B, 0x36, 0xC9, 0xA5, 0xE2, 0x3D, 0xCE, 0x69, 0xB7, 0x3C, 0x96, 0xBB, 0x7D, 0x0F, 0x0E, 0xD1, 0x9C, 0x16, 0xEE, 0x2F, 0x70, 0x38, 0x56, 0x7E, 0x41, 0x45, 0x64, 0x8F, 0x9D, 0xEF, 0x51, 0xA7, 0x6E, 0x3E, 0x93, 0x35, 0x7C, 0x75, 0x8A, 0x96, 0xDF, 0xA7, 0x43, 0xD4, 0x93, 0x9F, 0xDF, 0xF8, 0xA7, 0x72, 0x26, 0xD0, 0x66, 0x6F, 0xAD, 0x2E, 0x54, 0x0C, 0x4F, 0xF3, 0x02, 0x36, 0xD6, 0xFE, 0x58, 0xAC, 0xFA, 0x9F, 0x0D, 0xD4, 0x78, 0xC0, 0x76, 0xC7, 0x07, 0x75, 0x47, 0x5A, 0x0A, 0x46, 0xCF, 0xB6, 0xEF, 0x99, 0x6C, 0x41, 0x60, 0xEA, 0x10, 0x49, 0x6F, 0x8E, 0x63, 0x8A, 0x3D, 0xCE, 0x91, 0x4C, 0x28, 0x95, 0x1D, 0x40, 0xFF, 0xDE, 0x77, 0xA2, 0x50, 0xC2, 0x47, 0xAD, 0x86, 0xEF, 0x9A, 0x5A, 0x52, 0xCE, 0x9F, 0x59, 0x34, 0xAB, 0xFB, 0xD4, 0xB2, 0xA4, 0xFC, 0x9F, 0x31, 0xD7, 0xD4, 0xDA, 0x15, 0x5D, 0x8E, 0x66, 0xD5, 0x85, 0xE5, 0x4F, 0x02, 0xCB, 0x32, 0x78, 0x6C, 0x21, 0x27, 0xF7, 0x23, 0x74, 0xA2, 0xDA, 0xBD, 0x17, 0x0F, 0xA6, 0xB4, 0x3F, 0xA8, 0xC9, 0x2A, 0xC8, 0x06, 0x02, 0x93, 0x15, 0x8F, 0xA3, 0x5D, 0xE7, 0xA8, 0xF6, 0xBB, 0x81, 0x65, 0x09, 0x3D, 0xE1, 0x93, 0xEE, 0x80, 0x3F, 0x76, 0xFB, 0x18, 0xD8, 0xF4, 0x5F, 0x06, 0xF7, 0x1B, 0x85, 0x7E, 0x18, 0xE6, 0xD2, 0x4D, 0x4E, 0xEA, 0x23, 0x06, 0xD3, 0xEF, 0x41, 0x1B, 0x96, 0x5B, 0x1C, 0x14, 0xD1, 0x80, 0x6F, 0x88, 0x38, 0xF7, 0x4A, 0x9D, 0x74, 0xA7, 0x34, 0xDB, 0x20, 0x4A, 0xE5, 0x4A, 0xE6, 0x96, 0x01, 0x37, 0xDD, 0x35, 0x1B, 0xFE, 0x23, 0xD3, 0x53, 0x20, 0x47, 0x77, 0xD7, 0xF2, 0xA9, 0xDE, 0x77, 0xE7, 0x4F, 0xAF, 0x02, 0x8C, 0xE3, 0x08, 0xF7, 0xC8, 0x39, 0xD8, 0x43, 0x39, 0x01, 0xB5, 0xF3, 0x3C, 0xEA, 0x17, 0x03, 0x85, 0x96, 0x59, 0x94, 0x7D, 0x98, 0x6C, 0x3E, 0xB5, 0x96, 0xCE, 0x29, 0xCA, 0xB6, 0x55, 0x72, 0x7A, 0x01, 0x11, 0x93, 0x55, 0x65, 0x00, 0x90, 0xCE, 0xDB, 0xE0, 0x8E, 0x69, 0x02, 0x34, 0x2B, 0xB4, 0x87, 0x9D, 0x9B, 0x33, 0xE8, 0x6A, 0xB6, 0xDE, 0x4C, 0x26, 0x23, 0xF6, 0x82, 0xA3, 0x65, 0x05, 0x43, 0xD0, 0x55, 0x61, 0x03, 0x9B, 0x79, 0xCF, 0xA8, 0xDF, 0xD0, 0xB7, 0x1C, 0x32, 0xB8, 0x9F, 0xE8, 0x96, 0x0F, 0xE6, 0x43, 0x52, 0xE7, 0xAA, 0x4D, 0xE3, 0x56, 0xF0, 0x59, 0xDC, 0xF7, 0xC1, 0x54, 0xA2, 0xC6, 0x42, 0x31, 0x76, 0x20, 0xC5, 0x08, 0xDE, 0xAA, 0xF6, 0xE9, 0xFB, 0xB0, 0x14, 0xF8, 0x14, 0x98, 0x91, 0xA9, 0x43, 0xC2, 0x41, 0x0C, 0x24, 0x63, 0x91, 0x5B, 0xB9, 0x2D, 0xCE, 0x3E, 0x2E, 0xE3, 0x23, 0x83, 0xD5, 0x71, 0x63, 0x9A, 0x09, 0xC4, 0x9F, 0x1B, 0x12, 0xF4, 0xDE, 0x92, 0x6A, 0xFF, 0x9F, 0x00, 0x64, 0xA4, 0x73, 0xD4, 0xE7, 0xBC, 0x25, 0xFE, 0xC7, 0x59, 0x3B, 0xCD, 0x6B, 0x96, 0x4B, 0x37, 0x45, 0x67, 0x39, 0xBE, 0x99, 0xA9, 0x3F, 0xA0, 0xA1, 0xA7, 0xC7, 0x35, 0x4F, 0xFA, 0xF5, 0x34, 0x94, 0x4E, 0x89, 0x26, 0xDA, 0x63, 0x05, 0x55, 0xDD, 0x61, 0x35, 0xDF, 0xD7, 0x82, 0x6B, 0x29, 0xCD, 0x47, 0x84, 0x3C, 0xE4, 0x54, 0xE3, 0x01, 0xCC, 0x3A, 0x6C, 0x43, 0x28, 0x27, 0x51, 0xE7, 0xE4, 0xB6, 0x2D, 0x3E, 0xE7, 0xFD, 0x03, 0x1F, 0xFD, 0x61, 0x6C, 0x89, 0xB1, 0x80, 0x93, 0xC2, 0x18, 0xF0, 0x4E, 0x25, 0x20, 0x55, 0xAE, 0x78, 0xF0, 0x6C, 0xC4, 0x25, 0xD1, 0xC2, 0x98, 0xCD, 0x1A, 0xCF, 0xC0, 0xBC, 0x55, 0xE4, 0xD9, 0x78, 0x0C, 0xBC, 0xB0, 0x64, 0xEA, 0xFB, 0xC7, 0xBA, 0x36, 0xD9, 0xD6, 0xED, 0x25, 0x9D, 0x2C, 0x8A, 0x43, 0x88, 0x97, 0x98, 0x8A, 0x38, 0x19, 0xD8, 0xDC, 0x3F, 0xB5, 0xD3, 0xA9, 0x15, 0x92, 0xF8, 0xDC, 0x88, 0xA1, 0x69, 0x18, 0x9E, 0xED, 0x1D, 0xE3, 0x05, 0x7B, 0x1C, 0x55, 0x23, 0x18, 0x72, 0x69, 0x09, 0x83, 0x22, 0x03, 0x3C, 0x70, 0x95, 0x95, 0x50, 0x23, 0x33, 0x21, 0x46, 0x0D, 0xCA, 0x02, 0xAA, 0xF3, 0xF2, 0x30, 0xB8, 0x0A, 0x9E, 0xBE, 0x23, 0x57, 0xF4, 0xA5, 0xE6, 0xAE, 0x1E, 0x17, 0x8B, 0xF8, 0x5D, 0xFE, 0x21, 0x79, 0x45, 0xC5, 0x66, 0x1F, 0x74, 0x38, 0xAE, 0xC1, 0x10, 0x60, 0x11, 0x9D, 0x59, 0x22, 0x60, 0x69, 0x42, 0x7F, 0xB8, 0xE7, 0x13, 0x30, 0x19, 0x2D, 0x2A, 0x21, 0xE1, 0x93, 0xC4, 0xBC, 0x2A, 0x23, 0x44, 0x23, 0x01, 0x9C, 0xD9, 0x09, 0x17, 0x5A, 0xBD, 0x8B, 0x1A, 0x68, 0x5A, 0x32, 0xF9, 0x4C, 0x0C, 0x2C, 0xF6, 0xD0, 0xAF, 0xC3, 0x33, 0x8D, 0x4E, 0x68, 0x0C, 0xCC, 0x67, 0x7F, 0xDB, 0xAB, 0x07, 0xD4, 0x10, 0x09, 0x81, 0x55, 0x7D, 0xEF, 0x90, 0x82, 0x57, 0xE7, 0x49, 0x32, 0xA0, 0x0B, 0xC9, 0x2F, 0xD4, 0xAA, 0xB3, 0x67, 0x78, 0xD8, 0x6D, 0xDB, 0x0B, 0xB7, 0x8E, 0xD4, 0x7C, 0xCA, 0x20, 0xD9, 0x4D, 0xB9, 0xE5, 0x64, 0xC3, 0x8C, 0x5D, 0x58, 0xBB, 0x0B, 0x44, 0x50, 0x94, 0xFC, 0x48, 0xEA, 0x41, 0x99, 0x77, 0xAB, 0x98, 0x4B, 0xEA, 0x31, 0xBE, 0xD2, 0xE8, 0x3C, 0x61, 0xF2, 0xD9, 0x3E, 0xEB, 0x22, 0xAD, 0x83, 0x00, 0x14, 0xB9, 0xB1, 0xBA, 0xC9, 0xFB, 0xC2, 0xD1, 0xCC, 0xD8, 0xA1, 0x89, 0xAF, 0x67, 0x04, 0xC7, 0xFC, 0x92, 0x33, 0x0C, 0x8F, 0x36, 0xD8, 0x7B, 0xC0, 0x9F, 0x62, 0xBB, 0xC6, 0xB8, 0xF9, 0xED, 0x56, 0x6A, 0x5C, 0x57, 0xF9, 0xB1, 0xF1, 0xC1, 0x22, 0x94, 0x37, 0xC2, 0xF4, 0x48, 0x82, 0xC4, 0x38, 0x52, 0x4B, 0x55, 0x0B, 0x82, 0xCD, 0x26, 0x60, 0x72, 0xF0, 0xAE, 0x09, 0x71, 0x4F, 0xB3, 0x27, 0xC8, 0xA3, 0xF0, 0x59, 0x36, 0xB6, 0x6E, 0x00, 0xFE, 0xEE, 0x35, 0x6A, 0x6B, 0xB7, 0x07, 0xA6, 0xED, 0x85, 0x1D, 0x77, 0x98, 0xE0, 0xB4, 0xF1, 0x38, 0x1B, 0x9A, 0x43, 0x1E, 0x2E, 0x02, 0x31, 0xC4, 0xA5, 0xEF, 0xD8, 0x69, 0x58, 0x36, 0xE8, 0x40, 0x32, 0x53, 0x72, 0x64, 0x64, 0x47, 0x30, 0xF5, 0x3F, 0x4E, 0x96, 0xA7, 0xE7, 0x92, 0x2C, 0x38, 0x3B, 0xC3, 0xB8, 0x01, 0xE4, 0x66, 0xD8, 0x68, 0x82, 0x06, 0xF5, 0x56, 0x13, 0x35, 0xB8, 0x6B, 0x85, 0x2B, 0x86, 0x8B, 0x72, 0x41, 0x33, 0x8F, 0x11, 0x29, 0x47, 0x27, 0x01, 0xB1, 0x9A, 0xA3, 0x0D, 0x36, 0x54, 0xB5, 0x99, 0x57, 0x14, 0x7B, 0x2F, 0xAE, 0x58, 0x24, 0x14, 0x06, 0xB4, 0x23, 0x60, 0x17, 0x99, 0x22, 0x23, 0x0B, 0xDE, 0x24, 0x94, 0x7F, 0x77, 0x69, 0xDF, 0xE6, 0x6D, 0x03, 0x94, 0x15, 0x2C, 0xDA, 0xC1, 0xD8, 0x77, 0xC7, 0xAB, 0x39, 0x84, 0x55, 0x76, 0xBA, 0x33, 0xD2, 0x6A, 0x80, 0xF0, 0xD1, 0x6D, 0x4B, 0xE2, 0xBC, 0x76, 0x53, 0xF0, 0x2C, 0x4C, 0xCF, 0xEB, 0x3A, 0x2D, 0x4D, 0xA3, 0xDE, 0x16, 0x18, 0x6F, 0xDE, 0x56, 0x54, 0x1F, 0x2B, 0xCD, 0x4D, 0x29, 0x43, 0x5E, 0x8A, 0xB0, 0x63, 0x00, 0x85, 0x3D, 0x3D, 0x6D, 0x3A, 0xB5, 0xC3, 0x54, 0x64, 0xBB, 0x04, 0xC3, 0x99, 0x6F, 0xA5, 0x57, 0x3F, 0xCD, 0x37, 0xCB, 0x4E, 0x48, 0xAF, 0xEF, 0x02, 0x97, 0x1A, 0xDB, 0x55, 0x39, 0x8B, 0x77, 0xB4, 0x81, 0x47, 0x08, 0xE2, 0x9B, 0x7C, 0x2D, 0xE4, 0x98, 0x5B, 0x24, 0x2F, 0x1C, 0x93, 0xC7, 0xD9, 0xA4, 0x06, 0x9A, 0x87, 0x05, 0xC3, 0xE4, 0x01, 0x1B, 0xF5, 0xAF, 0x44, 0xCB, 0xC5, 0x73, 0xD5, 0x59, 0x0F, 0xEB, 0x35, 0xBA, 0xAE, 0x36, 0x58, 0xA4, 0xF1, 0x25, 0xDB, 0xAC, 0xDF, 0xE3, 0xB7, 0x8A, 0x11, 0x5C, 0xEB, 0x0A, 0x71, 0x06, 0xF9, 0x3C, 0x84, 0x6F, 0x4D, 0x6A, 0x01, 0xAD, 0x9C, 0x52, 0x96, 0x86, 0x8A, 0x0E, 0x9A, 0xAA, 0xEA, 0xA4, 0x07, 0xAB, 0xF7, 0xB8, 0xED, 0x18, 0x09, 0x2A, 0x57, 0x89, 0x5A, 0x0E, 0x9C, 0x96, 0xEF, 0xF1, 0xB0, 0x89, 0x31, 0x86, 0x8E, 0xF2, 0x10, 0xA0, 0x3B, 0xFC, 0xEE, 0xD2, 0x1D, 0xAE, 0x56, 0x5D, 0x21, 0x4E, 0xCF, 0x35, 0x46, 0x6F, 0xD2, 0x8B, 0x1E, 0xE9, 0x7B, 0x82, 0x76, 0x4C, 0x48, 0x16, 0xBF, 0x32, 0x58, 0x7A, 0x61, 0x1B, 0x43, 0x01, 0x0A, 0xA7, 0x72, 0x97, 0x34, 0x64, 0xE0, 0x57, 0x45, 0x3A, 0x29, 0xDA, 0xC8, 0x6A, 0xD0, 0x38, 0x7C, 0x1A, 0xB9, 0x5A, 0x97, 0x8C, 0x9A, 0x71, 0x4A, 0x28, 0xBB, 0xF5, 0x3C, 0xFC, 0x9A, 0x29, 0x3E, 0x8E, 0x0F, 0x2E, 0x6E, 0xF1, 0x6E, 0x2F, 0xD0, 0x5E, 0x35, 0xB4, 0xE3, 0xAD, 0x04, 0xDD, 0x97, 0x83, 0x61, 0xBA, 0xE3, 0x96, 0x48, 0xAF, 0xA5, 0x09, 0x4E, 0x19, 0x56, 0x70, 0x6A, 0xB0, 0xB1, 0x2F, 0x36, 0xC0, 0x67, 0x12, 0x7C, 0xE0, 0x59, 0x8E, 0x24, 0x5B, 0xEE, 0x50, 0x17, 0xF8, 0x5B, 0xEF, 0x0E, 0x39, 0x2C, 0x4F, 0x61, 0x11, 0x18, 0x9D, 0x2D, 0x73, 0x47, 0x3A, 0x77, 0xC4, 0x27, 0xA1, 0xB7, 0x89, 0x8C, 0x53, 0x5E, 0xFF, 0x7D, 0xCC, 0x2F, 0x2C, 0x60, 0x9F, 0xB1, 0xA7, 0xCB, 0x00, 0x7D, 0x63, 0xE5, 0xCB, 0x83, 0xF3, 0xA7, 0xC1, 0xCB, 0xD4, 0x02, 0x03, 0xBA, 0xAD, 0xA9, 0xA0, 0xD0, 0xF6, 0x4D, 0x5E, 0x62, 0x79, 0x41, 0x5A, 0x37, 0xB5, 0x8C, 0xC6, 0x9D, 0xA3, 0x64, 0x25, 0x01, 0x3A, 0x74, 0x1B, 0x94, 0xEA, 0x16, 0x76, 0x13, 0xF2, 0x52, 0xDD, 0x5C, 0x6D, 0xB4, 0x32, 0xED, 0xF7, 0x7B, 0x97, 0x52, 0xE3, 0x92, 0x48, 0xB0, 0xB9, 0x90, 0x45, 0xD7, 0xA9, 0xA4, 0x94, 0x72, 0xE8, 0x6C, 0x8A, 0x5A, 0x72, 0x68, 0x5C, 0x52, 0x33, 0x5A, 0xD7, 0xF2, 0xAF, 0xC8, 0xD8, 0x73, 0x09, 0x48, 0x67, 0xDD, 0x68, 0xFB, 0xAD, 0x4B, 0x00, 0x8C, 0x18, 0x83, 0x45, 0x96, 0xA6, 0xF5, 0x51, 0xC4, 0x11, 0xEA, 0xD7, 0x3D, 0x0C, 0x7A, 0x3D, 0x0B, 0x23, 0xE5, 0x9B, 0xBA, 0x86, 0xA0, 0x2B, 0x02, 0x31, 0x76, 0x7A, 0x75, 0x2D, 0x8E, 0xA3, 0x5D, 0x28, 0xE8, 0xFA, 0x99, 0x1F, 0xD2, 0x25, 0x1D, 0x06, 0x6E, 0xAD, 0xC2, 0x34, 0x94, 0xD3, 0xD8, 0x4F, 0xD3, 0x6F, 0xC5, 0xFF, 0xA9, 0xFA, 0x3B, 0xE4, 0xA8, 0x33, 0xDD, 0x16, 0x16, 0x70, 0x15, 0xD0, 0x60, 0x7A, 0x2B, 0xE7, 0x35, 0x29, 0x58, 0x5F, 0xF3, 0x2A, 0xCC, 0x04, 0x1A, 0xA1, 0x18, 0xF9, 0xB2, 0x5E, 0xE8, 0x1F, 0x05, 0xEC, 0xC1, 0x40, 0x31, 0x3A, 0xBF, 0x43, 0x0D, 0x81, 0xA8, 0xAD, 0x2C, 0xA0, 0x7C, 0x36, 0x74, 0x53, 0x27, 0x5E, 0x9C, 0xA0, 0xC4, 0x77, 0x7A, 0xB0, 0xB9, 0xC9, 0x20, 0x96, 0xF7, 0xA8, 0xE5, 0xFF, 0x88, 0x2B, 0x5A, 0x08, 0x48, 0x09, 0x42, 0xFB, 0x39, 0x93, 0xD7, 0x37, 0xCC, 0x69, 0x44, 0xB6, 0xA5, 0x46, 0xEC, 0x6D, 0xE4, 0xBD, 0x96, 0x85, 0xD4, 0x9A, 0x5B, 0x0D, 0x24, 0xFB, 0xAE, 0x70, 0x0F, 0x75, 0xFE, 0xEC, 0x29, 0x7C, 0xFF, 0xCE, 0xD6, 0xFC, 0x39, 0x22, 0xDB, 0x63, 0xEB, 0xEB, 0x84, 0xA7, 0x77 };
        private static readonly byte[] key_72_bytes = { 0x5F, 0x92, 0xBF, 0xF3, 0xB9, 0x66, 0x00, 0xB7, 0x4C, 0x03, 0xFE, 0xEE, 0x49, 0xDD, 0x44, 0xE2, 0x92, 0xB1, 0xDA, 0x13, 0x0B, 0xCA, 0x24, 0x82, 0xBC, 0xA0, 0xB4, 0x00, 0xC9, 0xD9, 0xDD, 0xE8, 0x65, 0x7A, 0x2F, 0x15, 0xA2, 0x1F, 0x8E, 0xBE, 0xB9, 0x80, 0x3E, 0xEB, 0xAF, 0xBF, 0x79, 0x7B, 0xB1, 0xD2, 0xB8, 0x5F, 0x2A, 0xB9, 0xD1, 0xA2, 0x56, 0xBC, 0x8A, 0xAB, 0x65, 0xC4, 0x2B, 0x73, 0xCB, 0xE6, 0x78, 0xAF, 0xDA, 0xFA, 0xA9, 0x88 };

        private static readonly uint[] key_4096;
        private static readonly uint[] key_72;

        static Crypto()
        {
            key_4096 = new uint[key_4096_bytes.Length / 4];
            for (int i = 0; i < key_4096_bytes.Length; i += 4)
                key_4096[i / 4] = BitConverter.ToUInt32(key_4096_bytes, i);

            key_72 = new uint[key_72_bytes.Length];
            for (int i = 0; i < key_72_bytes.Length; i += 4)
                key_72[i / 4] = BitConverter.ToUInt32(key_72_bytes, i);
        }

        public static Task DecryptAsync(Stream inputStream, Stream outputStream, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() => Decrypt(inputStream, outputStream, cancellationToken));
        }

        public static Task DecryptUnsafeAsync(Stream inputStream, Stream outputStream, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() => DecryptUnsafe(inputStream, outputStream, cancellationToken));
        }

        private static void Decrypt(Stream inputStream, Stream outputStream, CancellationToken cancellationToken)
        {
            if (inputStream == null)
                throw new ArgumentNullException(nameof(inputStream));
            if (outputStream == null)
                throw new ArgumentNullException(nameof(outputStream));

            if (inputStream.CanRead == false)
                throw new ArgumentException($"Argument '{nameof(inputStream)}' must be readable");
            if (outputStream.CanWrite == false)
                throw new ArgumentException($"Argument '{nameof(outputStream)}' must be writable");

            using (var reader = new BinaryReader(inputStream))
            {
                using (var writer = new BinaryWriter(outputStream, Encoding.UTF8, true))
                {
                    long length = reader.BaseStream.Length;

                    uint[] input = new uint[length / 4];
                    for (int i = 0; i < input.Length; i++)
                        input[i] = reader.ReadUInt32();

                    ulong[] output = new ulong[length / 8];
                    var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
                    Parallel.For(0, output.Length, options, i => output[i] = DecryptBlock(input[i * 2], input[i * 2 + 1]));

                    for (int i = 0; i < output.Length; i++)
                        writer.Write(output[i]);
                }
            }
        }

        private static byte[] DecryptUnsafe(Stream inputStream, Stream outputStream, CancellationToken cancellationToken)
        {
            if (inputStream == null)
                throw new ArgumentNullException(nameof(inputStream));
            if (outputStream == null)
                throw new ArgumentNullException(nameof(outputStream));

            if (inputStream.CanRead == false)
                throw new ArgumentException($"Argument '{nameof(inputStream)}' must be readable");
            if (outputStream.CanWrite == false)
                throw new ArgumentException($"Argument '{nameof(outputStream)}' must be writable");

            byte[] buffer = new byte[inputStream.Length];
            inputStream.Read(buffer, 0, buffer.Length);

            unsafe
            {
                fixed (void* pointer = buffer)
                {
                    uint* inputPointer = (uint*)pointer;
                    ulong* outputPointer = (ulong*)pointer;

                    // both parallel and non-parallel run at the same speed

                    // -=-=-=- bellow is the non-parallel way -=-=-=-
                    int length = buffer.Length / 8;
                    for (int i = 0; i < length; i++)
                        outputPointer[i] = DecryptBlock(inputPointer[i * 2], inputPointer[i * 2 + 1]);

                    // -=-=-=- bellow is the parallel way -=-=-=-
                    //Parallel.For(0, buffer.Length / 8, i => outputPointer[i] = DecryptBlock(inputPointer[i * 2], inputPointer[i * 2 + 1]));
                }
            }

            outputStream.Position = 0;
            outputStream.Write(buffer, 0, buffer.Length);

            return buffer;
        }

        private static ulong DecryptBlock(uint data, uint nextData)
        {
            uint key72Offset = 17;

            uint A;
            uint B;

            A = data ^ key_72[key72Offset];
            key72Offset--;
            B = BasicAlg(key72Offset, A, nextData);
            key72Offset--;

            for (int i = 0; i < 7; i++)
            {
                A = BasicAlg(key72Offset, B, A);
                key72Offset--;
                B = BasicAlg(key72Offset, A, B);
                key72Offset--;
            }

            A = BasicAlg(key72Offset, B, A);
            key72Offset--;

            B = B ^ key_72[key72Offset];

            return (ulong)A << 32 | B;
        }

        private static uint BasicAlg(uint arr72Offset, uint prevStep1, uint prevStep2)
        {
            uint A = BYTE2(prevStep1) + 0x100;
            uint result = key_4096[A];
            uint B = BYTE3(prevStep1);
            result = result + key_4096[B];
            B = BYTE1(prevStep1) + 0x200;
            result = result ^ key_4096[B];
            A = BYTE(prevStep1) + 0x300;
            result = result + key_4096[A];
            result = result ^ key_72[arr72Offset];
            result = result ^ prevStep2;
            return result;
        }

        private static uint BYTE(uint data)
        {
            return data & 0xFF;
        }

        private static uint BYTE1(uint data)
        {
            return (data >> 8) & 0xFF;
        }

        private static uint BYTE2(uint data)
        {
            return (data >> 16) & 0xFF;
        }

        private static uint BYTE3(uint data)
        {
            return (data >> 24) & 0xFF;
        }
    }
}
