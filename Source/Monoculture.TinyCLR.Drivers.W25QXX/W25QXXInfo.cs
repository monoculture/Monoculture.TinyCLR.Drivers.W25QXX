﻿using System;

namespace Monoculture.TinyCLR.Drivers.W25QXX
{
    public class W25QXXInfo
    {
        internal W25QXXInfo(W25QXXType type)
        {
            switch (type)
            {
                case W25QXXType.W25X16:
                    L2PageSize = 8;
                    PagesPerSector = 16;
                    SectorsPerBlock = 16;
                    NumberOfBlocks = 32;
                    ChipType = W25QXXType.W25X16;
                    break;
                case W25QXXType.W25X32:
                    Id = 0x3016;
                    L2PageSize = 8;
                    PagesPerSector = 16;
                    SectorsPerBlock = 16;
                    NumberOfBlocks = 64;
                    ChipType = W25QXXType.W25X32;
                    break;
                case W25QXXType.W25X64:
                    Id = 0x3017;
                    L2PageSize = 8;
                    PagesPerSector = 16;
                    SectorsPerBlock = 16;
                    NumberOfBlocks = 128;
                    ChipType = W25QXXType.W25X64;
                    break;
                case W25QXXType.W25Q16:
                    Id = 0x4015;
                    L2PageSize = 8;
                    PagesPerSector = 16;
                    SectorsPerBlock = 16;
                    NumberOfBlocks = 32;
                    ChipType = W25QXXType.W25Q16;
                    break;
                case W25QXXType.W25Q32:
                    Id = 0x4016;
                    L2PageSize = 8;
                    PagesPerSector = 16;
                    SectorsPerBlock = 16;
                    NumberOfBlocks = 64;
                    ChipType = W25QXXType.W25Q32;
                    break;
                case W25QXXType.W25Q64:
                    Id = 0x4017;
                    L2PageSize = 8;
                    PagesPerSector = 16;
                    SectorsPerBlock = 16;
                    NumberOfBlocks = 128;
                    ChipType = W25QXXType.W25Q64;
                    break;
                case W25QXXType.W25Q128:
                    Id = 0x4018;
                    L2PageSize = 8;
                    PagesPerSector = 16;
                    SectorsPerBlock = 16;
                    NumberOfBlocks = 256;
                    ChipType = W25QXXType.W25Q128;
                    break;
                default:
                    throw new InvalidOperationException("Unknown chip");
            }

            PageSize = 1 << L2PageSize;

            SectorSize = (1 << L2PageSize) * PagesPerSector;

            Capacity = PageSize
                       * PagesPerSector
                       * SectorsPerBlock
                       * NumberOfBlocks;
        }

        public ushort Id { get;}
        public W25QXXType ChipType { get; }
        public byte L2PageSize { get;  }
        public ushort PagesPerSector { get;  }
        public ushort SectorsPerBlock { get;  }
        public ushort NumberOfBlocks { get;  }
        public int SectorSize { get;  }
        public int Capacity { get;  }
        public int PageSize { get;  }

        public int BlockSize { get; }
    }
}
