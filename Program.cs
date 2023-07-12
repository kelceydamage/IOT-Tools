// See https://aka.ms/new-console-template for more information
using McMaster.Extensions.CommandLineUtils;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;

// Gpiod uses line numbers to access gpio. The line numbers can be found with `gpioinfo` and cross referenced with the nvidia 
// pinmux spreadsheet.
// The pinmux spreadsheet can be found here: https://developer.nvidia.com/jetson-nano-pinmux-datasheet
// Notes for personal use, these are free GPIO pins.
// gpiochip1 127 -- BCM 17 -- J41 Pin 11
// gpiochip1 112 -- BCM 18 -- J41 Pin 12

[Command(Name = "test-gpio", Description = "test a range of logical gpio pins")]
public class Program
{
    public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);
    private GpioController? Gpio;

    [Option(ShortName = "s", LongName = "start-pin", Description = "Start pin number")]
    public int StartPin { get; } = 0;

    [Option(ShortName = "r", LongName = "number-of-pins", Description = "The number of pins to test starting at start-pin")]
    public int PinRange { get; } = 0;

    [Option(ShortName = "d", LongName = "gpio-device-id", Description = "The ID of the gpio controller to test")]
    public int GpioDeviceId { get; } = 0;

    private void OnExecute()
    {
        Console.WriteLine("Starting Test");
        try
        {
            Console.WriteLine($"Using GpioDeviceId: {GpioDeviceId}");
            Gpio = new GpioController(PinNumberingScheme.Logical, new LibGpiodDriver(GpioDeviceId));
            Console.WriteLine($"Pincout: {Gpio.PinCount}");
            Console.WriteLine($"Numbering Scheme: {Gpio.NumberingScheme}");
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to initialize GPIO Controller: {e.Message}");
        }   

        for (var i = 0; i < PinRange; i++)
        {
            TestPin(Gpio, i + StartPin);
        }
        Gpio.Dispose();
    }

    private void TestPin(GpioController Gpio, int PinNumber)
    {
        Gpio.OpenPin(PinNumber);
        PinMode currentPinMod = Gpio.GetPinMode(PinNumber);
        Console.WriteLine($"Testing Pin: {PinNumber}, PinMode: {currentPinMod}");
        if (currentPinMod != PinMode.Output)
        {
            Console.WriteLine($"Skipping pin {PinNumber} because it is not an output pin");
        }
        else
        {
            if (Gpio.IsPinModeSupported(PinNumber, PinMode.Output))
            {
                Gpio.SetPinMode(PinNumber, PinMode.Output);
                for (var i = 0; i < 6; i++)
                {
                    Console.WriteLine($"Writing High/Low to Pin: {PinNumber}");
                    Gpio.Write(PinNumber, PinValue.High);
                    Thread.Sleep(500);
                    Gpio.Write(PinNumber, PinValue.Low);
                    Thread.Sleep(500);
                }
            }
            else
            {
                Console.WriteLine($"Pin {PinNumber} does not support Output");
            }
        }
        Gpio.ClosePin(PinNumber);
    }
}
