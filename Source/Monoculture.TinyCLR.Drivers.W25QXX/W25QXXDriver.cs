/*
 * Author: Monoculture 2019
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

using GHIElectronics.TinyCLR.Devices.Spi;

namespace Monoculture.TinyCLR.Drivers.W25QXX
{
    public class W25QXXDriver
    {
        private readonly SpiDevice _device;

        private const byte CMD_W25_WREN       = 0x06; /* Write Enable */
        private const byte CMD_W25_RDSR       = 0x05; /* Read Status Register */
        private const byte CMD_W25_READ       = 0x03; /* Read Data Bytes */
        private const byte CMD_W25_FAST_READ  = 0x0b; /* Read Data Bytes at Higher Speed */
        private const byte CMD_W25_PP         = 0x02; /* Page Program */
        private const byte CMD_W25_SE         = 0x20; /* Sector (4K) Erase */
        private const byte CMD_W25_RDID       = 0x9f; /* Read ID */
        private const byte CMD_W25_BE         = 0xd8; /* Block (64K) Erase */
        private const byte CMD_W25_CE         = 0xc7; /* Chip Erase */
        private const byte CMD_W25_DP         = 0xb9; /* Deep Power-down */
        private const byte CMD_W25_RES        = 0xab; /* Release from DP and Read Signature */

        public static SpiConnectionSettings GetSpiConnectionSettings(int chipSelectLine) =>
            new SpiConnectionSettings
            {
                Mode = SpiMode.Mode3,
                ClockFrequency = 50000000,
                ChipSelectLine = chipSelectLine,
                ChipSelectType = SpiChipSelectType.Gpio
            };

        public W25QXXDriver(SpiDevice device, W25QXXType chipType)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));

            ChipInfo = new W25QXXInfo(chipType);
        }

        public W25QXXInfo ChipInfo { get; }

        public void WriteEnable()
        {
            SpiSend(CMD_W25_WREN);
        }

        public byte[] GetIdentification()
        {
            return SpiSendReceive(CMD_W25_RDID, 3);
        }

        public bool IsWriteInProgress()
        {
            return (GetStatus() & 1) == 0;
        }

        private byte GetStatus()
        {
            return SpiSendReceive(CMD_W25_RDSR, 1)[0];
        }

        /// <summary>
        /// Erase entire chip
        /// </summary>
        public void EraseChip()
        {
            if (IsWriteInProgress())
            {
                Wait();
            }

            WriteEnable();

            SpiSend(CMD_W25_CE);

            Wait();
        }

        /// <summary>
        /// Erase 64K blocks sequentially
        /// </summary>
        /// <param name="block"></param>
        /// <param name="count"></param>
        public void EraseBlock(int block, int count)
        {
            if (block < 0)
                throw new ArgumentOutOfRangeException(nameof(block), "block must not be negative.");

            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "count must be positive.");

            if (block + count > ChipInfo.NumberOfBlocks)
                throw new ArgumentOutOfRangeException(nameof(block), "block + count must be less than the total number of blocks.");

            if (IsWriteInProgress())
            {
                Wait();
            }

            WriteEnable();

            var address = block * ChipInfo.BlockSize; 

            var writeData = new byte[4];

            for (var i = 0; i < count; i++)
            {
                writeData[0] = CMD_W25_BE;

                writeData[1] = (byte)(address >> 16);
                writeData[2] = (byte)(address >> 8);
                writeData[3] = (byte)(address >> 0);

                SpiSend(writeData);

                address += ChipInfo.BlockSize;

                Wait();
            }
        }

        /// <summary>
        /// Erase 4k sectors sequentially starting at address
        /// </summary>
        /// <param name="sector"></param>
        /// <param name="count"></param>
        public void EraseSector(int sector, int count)
        {
            if (sector < 0)
                throw new ArgumentOutOfRangeException(nameof(sector), "sector must not be negative.");

            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "count must be positive.");

            if ((sector + count) * ChipInfo.SectorSize > ChipInfo.Capacity)
                throw new ArgumentOutOfRangeException(nameof(sector), "sector + count must be less than the total number of blocks.");

            if (IsWriteInProgress())
            {
                Wait();
            }

            WriteEnable();

            var address = sector * ChipInfo.SectorSize;

            var writeData = new byte[4];

            for (var i = 0; i < count; i++)
            {
                writeData[0] = CMD_W25_SE;

                writeData[1] = (byte)(address >> 16);
                writeData[2] = (byte)(address >> 8);
                writeData[3] = (byte)(address >> 0);

                SpiSend(writeData);

                address += ChipInfo.BlockSize;

                Wait();
            }
        }

        public void Write(int address, byte[] writeBuffer)
        {
            if (writeBuffer == null)
                throw new ArgumentNullException("buffer");

            Write(address, writeBuffer, 0, writeBuffer.Length);
        }

        public void Write(int address, byte[] writeBuffer, int writeOffset, int writeLength)
        {
            if (address < 0)
                throw new ArgumentOutOfRangeException(nameof(address), "address must not be negative.");

            if (writeOffset < 0)
                throw new ArgumentOutOfRangeException(nameof(writeOffset), "offset must not be negative.");

            if (writeLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(writeLength), "count must be positive.");

            if (writeBuffer == null)
                throw new ArgumentNullException("buffer");

            if (writeLength + address > ChipInfo.Capacity)
                throw new ArgumentOutOfRangeException(nameof(address),
                    "address + buffer.Length must be less than the total number of blocks.");

            WriteEnable();

            if (IsWriteInProgress())
            {
                Wait();
            }

            var length = writeLength;

            var pages = writeLength / ChipInfo.PageSize;

            if (pages > 0)
            {
                var writeData = new byte[ChipInfo.PageSize + 4];

                for (var i = 0; i < pages; i++)
                {
                    writeData[0] = CMD_W25_PP;
                    writeData[1] = (byte)(address >> 16);
                    writeData[2] = (byte)(address >> 8);
                    writeData[3] = (byte)(address >> 0);

                    Array.Copy(
                        writeBuffer, 
                        i * ChipInfo.PageSize + writeOffset, 
                        writeData,
                        4, 
                        ChipInfo.PageSize);

                    SpiSend(writeData);

                    Wait();

                    address += ChipInfo.PageSize;
                    length -= ChipInfo.PageSize;
                }
            }

            if (length > 0)
            {
                var writeData = new byte[length];

                writeData[0] = CMD_W25_PP;
                writeData[1] = (byte)(address >> 16);
                writeData[2] = (byte)(address >> 8);
                writeData[3] = (byte)(address >> 0);

                Array.Copy(writeBuffer, 0, writeData, 4, length);

                SpiSend(writeData);

                Wait();
            }
        }

        public byte[] Read(int address, int length)
        {
            if (address < 0)
                throw new ArgumentOutOfRangeException(nameof(address), "address must not be negative.");

            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), "length must be positive.");

            if (length + address > ChipInfo.Capacity)
                throw new ArgumentOutOfRangeException(nameof(address), "address + length must be less than the total number of blocks.");

            var writeData = new byte[4];

            writeData[0] = CMD_W25_READ;
            writeData[1] = (byte)(address >> 16);
            writeData[2] = (byte)(address >> 8);
            writeData[3] = (byte)(address >> 0);

            return SpiSendReceive(writeData, length);
        }

        public byte[] ReadFast(int address, int length)
        {
            if (address < 0)
                throw new ArgumentOutOfRangeException(nameof(address), "address must not be negative.");

            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), "length must be positive.");

            if (length + address > ChipInfo.Capacity)
                throw new ArgumentOutOfRangeException(nameof(address), "address + length must be less than the total number of blocks.");

            var writeData = new byte[6];

            writeData[0] = CMD_W25_FAST_READ;
            writeData[1] = (byte)(address >> 16);
            writeData[2] = (byte)(address >> 8);
            writeData[3] = (byte)(address >> 0);

            return SpiSendReceive(writeData, length);
        }

        public void Suspend()
        {
            SpiSend(CMD_W25_DP);
        }

        public void Resume()
        {
            SpiSend(CMD_W25_RES);
        }

        private void Wait()
        {
            while(true)
            {
                if (IsWriteInProgress() == false)
                    break;
            }
        }

        private void SpiSend(byte data)
        {
            SpiSend(new[] { data });
        }

        private void SpiSend(byte[] data)
        {
            _device.Write(data);
        }

        private byte[] SpiSendReceive(byte data, int readLength)
        {
            return SpiSendReceive(new[] { data }, readLength);
        }

        private byte[] SpiSendReceive(byte[] data, int readLength)
        {
            var bufferSize = data.Length + readLength;

            var txBuffer = new byte[bufferSize];

            var rxBuffer = new byte[bufferSize];

            Array.Copy(data, txBuffer, data.Length);

            _device.TransferFullDuplex(txBuffer, rxBuffer);

            var result = new byte[readLength];

            Array.Copy(rxBuffer, data.Length, result, 0, readLength);

            return result;
        }
    }
}
