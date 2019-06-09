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
