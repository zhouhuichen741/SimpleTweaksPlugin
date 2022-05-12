﻿using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SimpleTweaksPlugin.GameStructs
{
    //*(*(a2 + 0x20)+0x20)=Data
    //*(*(*(a3+0x18) + 0x20))+0x30+0x68*index)   =NameText
    //*(*(*(a3+0x18) + 0x20))+0x38+0x68*index)   =Casting Skill

    [StructLayout(LayoutKind.Explicit, Size = 0x68)]
    public unsafe struct MemberStrings
    {
        [FieldOffset(0x00)] public byte* Name;

        [FieldOffset(0x08)] public byte* Casting;
        //[FieldOffset(0x10)] public fixed long Buff[10];
        //[FieldOffset(0x60)] public byte* Unknown;

        public unsafe string GetLvlName()
        {
            var ptr = Name;
            var offset = 0;
            while (true) {
                var b = *(ptr + offset);
                if (b == 0) {
                    break;
                }
                offset += 1;
            }
            return Encoding.UTF8.GetString(ptr, offset);

        }

        public unsafe string GetName()
        {
            byte* ptr = (Name + 0x68);
            var offset = 0;
            while (true) {
                var b = *(ptr + offset);
                if (b == 0) {
                    break;
                }
                offset += 1;
            }
            return Encoding.UTF8.GetString(ptr, offset);
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x68 * 13)]
    public struct PartyStrings
    {
        [FieldOffset(0x00)] public MemberStrings Member0;
        [FieldOffset(0x68)] public MemberStrings Member1;
        [FieldOffset(0xD0)] public MemberStrings Member2;
        [FieldOffset(0x138)] public MemberStrings Member3;
        [FieldOffset(0x1A0)] public MemberStrings Member4;
        [FieldOffset(0x208)] public MemberStrings Member5;
        [FieldOffset(0x270)] public MemberStrings Member6;
        [FieldOffset(0x2D8)] public MemberStrings Member7;

        [FieldOffset(0x340)] public MemberStrings Member8;
        [FieldOffset(0x3A8)] public MemberStrings Member9;
        [FieldOffset(0x410)] public MemberStrings Member10;
        [FieldOffset(0x478)] public MemberStrings Member11;
        [FieldOffset(0x4E0)] public MemberStrings Member12;

        public MemberStrings MemberStrings(int index)
        {
            return index switch
            {
                0 => Member0,
                1 => Member1,
                2 => Member2,
                3 => Member3,
                4 => Member4,
                5 => Member5,
                6 => Member6,
                7 => Member7,
                8 => Member8,
                9 => Member9,
                10 => Member10,
                11 => Member11,
                12 => Member12,
                _ => new MemberStrings(),
            };
        }
    }



    [StructLayout(LayoutKind.Explicit, Size = 0x9C)]
    public unsafe struct MemberData
    {
        [FieldOffset(0x00)] public int Changed;
        [FieldOffset(0x04)] public int HasMP;
        [FieldOffset(0x08)] public int Level;
        [FieldOffset(0x0C)] public int JobId; //+固定值
        [FieldOffset(0x10)] public int UnknownINT;
        [FieldOffset(0x14)] public int CurrentHP; //范围外FFFFFFFF  跨服00000001
        [FieldOffset(0x18)] public int MaxHp; //范围外00000001  跨服00000001
        [FieldOffset(0x1C)] public int ShieldPercent;
        [FieldOffset(0x20)] public int CurrentMP; //范围外FFFFFFFF  跨服00000000
        [FieldOffset(0x24)] public int MaxMp; //范围外00000000  跨服00000000

        [FieldOffset(0x2C)] public int EmnityPercent;
        [FieldOffset(0x30)] public int EmnityNumber;
        [FieldOffset(0x34)] public uint Unknown1; //本地FFFEFFE8    跨服FFFFC8C5
        [FieldOffset(0x38)] public uint Unknown2; //本地FF985008    跨服FFE22A00
        [FieldOffset(0x3C)] public int BuffCount;

        [FieldOffset(0x40)] public fixed uint BuffIcon[20];

        //[FieldOffset(0x8C)] FFFFFFF
        [FieldOffset(0x98)] public uint Unknown3;
        [FieldOffset(0x9C)] public uint ActorId;
        [FieldOffset(0xA0)] public uint Unknown4;
    }


    [StructLayout(LayoutKind.Explicit)]
    public struct DataArray
    {
        [FieldOffset(0x04)] private uint Unknown;
        [FieldOffset(0x0C)] public int HideWhenSolo;
        [FieldOffset(0x14)] public int PlayerCount;

        [FieldOffset(0x18)] public int LeaderNumber;

        [FieldOffset(0x1C)] private MemberData MemberData0; //数量未知
        [FieldOffset(0xC0)] private MemberData MemberData1;
        [FieldOffset(0x164)] private MemberData MemberData2;
        [FieldOffset(0x208)] private MemberData MemberData3;
        [FieldOffset(0x2AC)] private MemberData MemberData4;
        [FieldOffset(0x350)] private MemberData MemberData5;
        [FieldOffset(0x3F4)] private MemberData MemberData6;
        [FieldOffset(0x498)] private MemberData MemberData7;

        [FieldOffset(0x53C)] public int QinXinCount;
        [FieldOffset(0x540)] private MemberData MemberData8;
        [FieldOffset(0x5E4)] private MemberData MemberData9;
        [FieldOffset(0x688)] private MemberData MemberData10;

        [FieldOffset(0x72C)] private MemberData MemberData11;
        [FieldOffset(0x7D0)] private MemberData MemberData12;

        [FieldOffset(0xB04)] public int CPCount;
        [FieldOffset(0xB08)] public int PetCount;

        public MemberData MemberData(int index)
        {
            return index switch
            {
                0 => MemberData0,
                1 => MemberData1,
                2 => MemberData2,
                3 => MemberData3,
                4 => MemberData4,
                5 => MemberData5,
                6 => MemberData6,
                7 => MemberData7,
                8 => MemberData8,
                9 => MemberData9,
                10 => MemberData10,
                11 => MemberData11,
                12 => MemberData12,
                _ => new MemberData(),
            };
        }
    }
}