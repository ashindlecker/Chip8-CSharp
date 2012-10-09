using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Chip8
{
    class Emulator
    {

        byte[] memory;
        Stack<ushort> stack;

        byte[] registers;
        byte[] timers;
        byte[] inputs;
        byte[,] screen;
        ushort currentOpcode;
        ushort addressRegister;
        Random randomizer;
        bool inputSent;
        Queue<int> MemoryQ = new Queue<int>();

        
        const uint FONTMEMORYOFFSET = 0;

        enum TimerTypes: byte
        {
            DelayTimer = 0,
            SoundTimer = 1,
        };

        public byte[,] Screen
        {
            get
            {
                return screen;
            }
        }

        public byte[] Input
        {
            get
            {
                return inputs;
            }
        }

        public Emulator()
        {
            randomizer = new Random();

            memory = new byte[4096];
            registers = new byte[16];
            stack = new Stack<ushort>();
            timers = new byte[2];
            inputs = new byte[16];
            screen = new byte[64 , 32];
            currentOpcode = 0;

            for (int i = 0; i < 100; i++)
            {
                MemoryQ.Enqueue(0);
            }

            Reset();
        }

        public void Reset()
        {
            for (int i = 0; i < memory.Length; i++)
            {
                memory[i] = 0;
            }
            for (int i = 0; i < registers.Length; i++)
            {
                registers[i] = 0;
            }
            for (int i = 0; i < timers.Length; i++)
            {
                timers[i] = 0;
            }
            for (int i = 0; i < inputs.Length; i++)
            {
                inputs[i] = 0;
            }
            addressRegister = 0;
            stack.Clear();
            inputSent = false;
        }

        public bool Step()
        {

            byte[] parseRaw = new byte[2] { memory[currentOpcode + 1], memory[currentOpcode] };
            ushort parseConvert = BitConverter.ToUInt16(parseRaw, 0);
            //Console.WriteLine(currentOpcode + "\t" + String.Format("{0,10:X}", parseConvert));
            bool screenchanged = parseOpcode(parseConvert);
            MemoryMod(false, memory[currentOpcode]);
            for (int i = 0; i < 2; i++)
            {
                if(timers[i] > 0)
                timers[i]--;
            }
            return screenchanged;
        }
        void MemoryMod(bool written, int address)
        {
         /*   int row = address / 40;
            int col = address % 40 * 2;
            MemoryQ.Enqueue(address);
            Console.SetCursorPosition(col, row);
            if (written)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.Write("W");
            }
            else
            {

                Console.BackgroundColor = ConsoleColor.Green;
                Console.Write("R");
            }
            int CullAddress = MemoryQ.Dequeue();

            row = CullAddress / 40;
            col = CullAddress % 40 * 2;
            Console.SetCursorPosition(col, row);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(" ");*/
        }
        void applyFontToMemory()
        {
            //Characters 0-F (in hexadecimal) are represented by a 4x5 font.

            byte[] font =
            { 
              0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
              0x20, 0x60, 0x20, 0x20, 0x70, // 1
              0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
              0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
              0x90, 0x90, 0xF0, 0x10, 0x10, // 4
              0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
              0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
              0xF0, 0x10, 0x20, 0x40, 0x40, // 7
              0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
              0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
              0xF0, 0x90, 0xF0, 0x90, 0x90, // A
              0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
              0xF0, 0x80, 0x80, 0x80, 0xF0, // C
              0xE0, 0x90, 0x90, 0x90, 0xE0, // D
              0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
              0xF0, 0x80, 0xF0, 0x80, 0x80  // F
            };

            for (int i = 0; i < font.Length; i++)
            {
                memory[i + FONTMEMORYOFFSET] = font[i];
            }
        }

        public void LoadROM(string file)
        {
            if (File.Exists(file))
            {
                Reset();
                FileStream stream = File.Open(file, FileMode.Open);

                byte[] data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);

                const int PROGRAMMEMORYSTART = 512;
                for (int i = 0; i < data.Length; i++)
                {
                    memory[i + PROGRAMMEMORYSTART] = data[i];
                    MemoryMod(true, i + PROGRAMMEMORYSTART);
                }
                applyFontToMemory();
                currentOpcode = PROGRAMMEMORYSTART;
            }
        }

        void stepInstrucion()
        {
            currentOpcode += 2; //each opcode is 2 bytes
        }

        public void SendInput(int id, bool val)
        {
            inputs[id] = BitConverter.GetBytes(val)[0];
            if(val)
            inputSent = true;
        }

        bool parseOpcode(ushort opcode)
        {
            bool ScreenChanged =  false;
            switch (opcode & (0xF000))
            {
                case(0x0000):
                    if ((opcode & (0x00FF)) == (0x00EE))//00EE
                    {
                        //Returns from a subroutine.
                        currentOpcode = stack.Pop();
                        stepInstrucion();
                    }
                    else if ((opcode & (0x00FF)) == (0x00E0))//00E0
                    {
                        //Clears the screen.

                        for (int x = 0; x < screen.GetLength(0); x++)
                        {
                            for (int y = 0; y < screen.GetLength(1); y++)
                            {
                                screen[x, y] = 0;
                            }
                        }
                        ScreenChanged = true;
                        stepInstrucion();
                    }
                    else
                    {
                        Console.WriteLine("Unknown");
                    }
                    break;

                case(0x1000)://1NNN
                    //Jumps to address NNN.

                    currentOpcode = (ushort)(opcode & (0x0FFF));
                    break;

                case (0x2000)://2NNN
                    //Calls subroutine at NNN

                    stack.Push(currentOpcode);
                    currentOpcode = (ushort)(opcode & (0x0FFF));
                    break;

                case (0x3000)://3XNN
                    {
                        //Skips the next instruction if VX equals NN.

                        ushort register = (ushort)((opcode & 0x0F00) >> 8);
                        ushort checkValue = (ushort)(opcode & 0x00FF);

                        if (registers[register] == checkValue)
                        {
                            stepInstrucion();
                        }
                        stepInstrucion();
                    }
                    break;

                case (0x4000)://4XNN
                    {
                        //Skips the next instruction if VX doesn't equals NN.

                        ushort register = (ushort)((opcode & 0x0F00) >> 8);
                        ushort checkValue = (ushort)(opcode & 0x00FF);

                        if (registers[register] != checkValue)
                        {
                            stepInstrucion();
                        }
                        stepInstrucion();
                    }
                    break;

                case (0x5000)://5XY0
                    {
                        //Skips the next instruction if VX equals VY

                        ushort x = (ushort)((opcode & 0x0F00) >> 8);
                        ushort y = (ushort)((opcode & 0x00F0) >> 4);

                        if (registers[x] == registers[y])
                        {
                            stepInstrucion();
                        }
                        stepInstrucion();
                    }
                    break;

                case (0x6000)://6XNN
                    //Sets VX to NN
                    {
                        ushort x = (ushort)((opcode & 0x0F00) >> 8);
                        byte nn = (byte)(opcode & 0x00FF);
                        registers[x] = nn;
                        stepInstrucion();
                    }
                    break;

                case (0x7000)://7XNN
                    //Adds NN to VX
                    {
                        ushort x = (ushort)((opcode & 0x0F00) >> 8);
                        byte nn = (byte)(opcode & 0x00FF);
                        registers[x] += nn;
                        stepInstrucion();
                    }
                    break;
                case (0x8000)://8XYO
                    {
                        ushort x = (ushort)((opcode & 0x0F00) >> 8);
                        ushort y = (ushort)((opcode & 0x00F0) >> 4);

                        switch (opcode & 0x000F)
                        {
                            case (0x0000)://8XY0
                                //Sets VX to the value of VY.
                                registers[x] = registers[y];

                                break;

                            case (0x0001)://8XY1
                                //Sets VX to VX or VY.
                                registers[x] = (byte)(registers[x] | registers[y]);
                                break;

                            case (0x0002)://8XY2
                                //Sets VX to VX and VY.
                                registers[x] = (byte)(registers[x] & registers[y]);
                                break;

                            case (0x0003)://8XY3
                                //Sets VX to VX xor VY.
                                registers[x] = (byte)(registers[x] ^ registers[y]);
                                break;

                            case (0x0004)://8XY4
                                //Adds VY to VX. VF is set to 1 when there's a carry, and to 0 when there isn't.

                                if ((ushort)registers[x] + (ushort)registers[y] > 255)
                                {
                                    registers[15] = 1;
                                }
                                else
                                {
                                    registers[15] = 0;
                                }
                                registers[x] += registers[y];
                                break;

                            case (0x0005)://8XY5
                                //VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there isn't.

                                if ((ushort)registers[x] - (ushort)registers[y] < 0)
                                {
                                    registers[15] = 0;
                                }
                                else
                                {
                                    registers[15] = 1;
                                }
                                registers[x] -= registers[y];
                                break;

                            case (0x0006)://8XY6
                                //Shifts VX right by one. VF is set to the value of the least significant bit of VX before the shift.[2]

                                registers[15] = (byte)(registers[x] & 0x01);
                                registers[x] >>= 1;
                                break;

                            case (0x0007)://8XY7
                                //Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there isn't.

                                if ((ushort)registers[y] - (ushort)registers[x] < 0)
                                {
                                    registers[15] = 0;
                                }
                                else
                                {
                                    registers[15] = 1;
                                }
                                registers[x] = (byte)(registers[y] - registers[x]);
                                break;

                            case (0x000E)://8XYE
                                //Shifts VX left by one. VF is set to the value of the most significant bit of VX before the shift.[2]

                                registers[15] = (byte)((registers[x] & 0x80) >> 7);
                                registers[x] <<= 1;
                                break;

                            default:

                                Console.WriteLine("Unknown");
                                break;
                        }
                    }
                    stepInstrucion();
                    break;

                case (0x9000)://9XY0
                    {
                        //Skips the next instruction if VX doesn't equal VY.

                        ushort x = (ushort)((opcode & 0x0F00) >> 8);
                        ushort y = (ushort)((opcode & 0x00F0) >> 4);
                        if (registers[x] != registers[y])
                        {
                            stepInstrucion();
                        }
                        stepInstrucion();
                    }
                    break;

                case (0xA000)://ANNN
                    {
                        //Sets I to the address NNN.

                        ushort val = (ushort)((opcode & 0x0FFF));
                        addressRegister = val;
                        stepInstrucion();
                    }
                    break;

                case (0xB000)://BNNN
                    {
                        //Jumps to the address NNN plus V0.
                        ushort val = (ushort)((opcode & 0x0FFF));
                        currentOpcode = (ushort)(val + registers[0]);
                    }
                    break;

                case (0xC000): //CXNN
                    {
                        //Sets VX to a random number and NN.

                        byte x = (byte)((opcode & 0x0F00) >> 8);
                        byte nn = (byte)(opcode & 0x00FF);

                        registers[x] = (byte)randomizer.Next(0, 255);
                        registers[x] &= nn;
                        stepInstrucion();
                    }
                    break;

                case (0xD000): //DXYN
                    {
                        //Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels. Each row of 8 pixels is read as bit-coded (with the most significant bit of each byte displayed on the left) starting from memory location I; I value doesn't change after the execution of this instruction. As described above, VF is set to 1 if any screen pixels are flipped from set to unset when the sprite is drawn, and to 0 if that doesn't happen.
                        ScreenChanged = true;
                        byte x = (byte)((opcode & 0x0F00) >> 8);
                        byte y = (byte)((opcode & 0x00F0) >> 4);
                        byte n = (byte)(opcode & 0x000F);
                        
                        x = registers[x];
                        y = registers[y];

                        registers[15] = 0;

                        for (int height = 0; height < n; height++)
                        {
                            byte memoryLook = memory[addressRegister + height];
                            MemoryMod(false, addressRegister + height);
                            for (int width = 0; width < 8; width++)
                            {

                                int coordX = x + width;
                                int coordY = y + height;

                                while (coordX >= screen.GetLength(0)) coordX -= screen.GetLength(0);
                                while (coordY >= screen.GetLength(1)) coordY -= screen.GetLength(1);

                                if ((memoryLook & (0x80 >> width)) != 0)
                                {
                                    //Console.WriteLine(coordY + ", " + coordY);
                                    if (screen[coordX, coordY] == 1)  //flip from set to unset
                                    {
                                        registers[15] = 1;
                                    }
                                    screen[coordX, coordY] ^= 1;
                                }
                            }
                        }
                        stepInstrucion();
                    }
                    break;

                case (0xE000)://EXOO
                    {

                        byte x = (byte)((opcode & 0xF00) >> 8);

                        if ((opcode & 0x00FF) == (0x009E)) //EX9E
                        {
                            //Skips the next instruction if the key stored in VX is pressed.

                            if (inputs[registers[x]] != 0)
                            {
                                stepInstrucion();
                            }
                        }
                        else if ((opcode & 0x00FF) == (0x00A1)) //EXA1
                        {
                            //Skips the next instruction if the key stored in VX isn't pressed.

                            if (inputs[registers[x]] == 0)
                            {
                                stepInstrucion();
                            }
                        }
                        else
                        {

                            Console.WriteLine("Unknown");
                        }
                        stepInstrucion();
                    }
                    break;

                case (0xF000)://FXOO
                    {
                        byte x = (byte)((opcode & 0xF00) >> 8);
                        switch (opcode & 0x00FF)
                        {
                            case (0x0007):
                                //Sets VX to the value of the delay timer.

                                registers[x] = timers[(int)TimerTypes.DelayTimer];
                                stepInstrucion();
                                break;

                            case(0x000A):
                                //A key press is awaited, and then stored in VX.
                                if (inputSent)
                                {
                                    stepInstrucion();
                                    inputSent = false;
                                }
                                break;

                            case (0x0015):
                                //Sets the delay timer to VX.

                                timers[(int)TimerTypes.DelayTimer] = registers[x];
                                stepInstrucion();
                                break;

                            case (0x0018):
                                //Sets the sound timer to VX.

                                timers[(int)TimerTypes.SoundTimer] = registers[x];
                                stepInstrucion();
                                break;

                            case(0x001E):
                                //Adds VX to I
                                if (addressRegister + registers[x] > memory.Length)
                                {
                                    registers[15] = 1;
                                }
                                else
                                {
                                    registers[15] = 0;
                                }
                                addressRegister += registers[x];
                                stepInstrucion();
                                break;

                            case (0x0029):
                                //Sets I to the location of the sprite for the character in VX. Characters 0-F (in hexadecimal) are represented by a 4x5 font.
                                addressRegister = (ushort)((registers[x] * 5) + FONTMEMORYOFFSET);
                                stepInstrucion();
                                break;

                            case (0x0033):
                                //Stores the Binary-coded decimal representation of VX, with the most significant of three digits at the address in I, the middle digit at I plus 1, and the least significant digit at I plus 2.  
                                //credit goes to Laurence Muller for the explanation of this opcode (http://www.multigesture.net/articles/how-to-write-an-emulator-chip-8-interpreter/)
                                
                                memory[addressRegister]     = (byte)(registers[x] / 100);
                                memory[addressRegister + 1] = (byte)((registers[x] / 10) % 10);
                                memory[addressRegister + 2] = (byte)((registers[x] % 100) % 10);

                                MemoryMod(true, addressRegister);
                                MemoryMod(true, addressRegister+1);
                                MemoryMod(true, addressRegister+2);
                                stepInstrucion();
                                break;

                            case(0x0055):
                                //Stores V0 to VX in memory starting at address I.
                                
                                for (int r = 0; r <= x; r++)
                                {
                                    memory[addressRegister + r] = registers[r];
                                    MemoryMod(true, addressRegister + r);
                                }
                                addressRegister = (ushort)(addressRegister + x + 1);
                                stepInstrucion();
                                break;
                                
                            case(0x0065):
                                //Fills V0 to VX with values from memory starting at address I.
                                
                                for (int r = 0; r <= x; r++)
                                {
                                    registers[r] = memory[addressRegister + r];
                                    MemoryMod(false, addressRegister + r);
                                }
                                addressRegister = (ushort)(addressRegister + x + 1);
                                stepInstrucion();

                                break;
                            default:

                                Console.WriteLine("Unknown");
                                break;
                        }
                    }
                    break;
                default:
                    Console.WriteLine("Unknown");
                    break;
               }
            return ScreenChanged;
            
        }
    }
}
