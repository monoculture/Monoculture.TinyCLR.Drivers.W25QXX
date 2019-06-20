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

using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.Devices.Spi;

using Monoculture.TinyCLR.Drivers.W25QXX;

namespace Monoculture.TinyCLR.W25QXX.Demo
{
    class Program
    {
        static void Main()
        {
            var driver = GetDriver();

            var chipId = driver.GetIdentification();
        }

        private static W25QXXDriver GetDriver()
        {
            var settings = W25QXXDriver.GetSpiConnectionSettings(G120E.GpioPin.P2_27);

            var controller = SpiController.FromName(G120E.SpiBus.Spi0);

            var device = controller.GetDevice(settings);

            var driver = new W25QXXDriver(device, W25QXXType.W25Q128);

            return driver;
        }
    }
}
